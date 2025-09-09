using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net;
using UnisonRestAdapter.Models.Responses;

namespace UnisonRestAdapter.Middleware
{
    /// <summary>
    /// Enhanced error handling middleware that provides structured JSON error responses,
    /// SOAP fault to REST error mapping, proper HTTP status codes, and correlation tracking
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var correlationId = GenerateCorrelationId();
            context.Items["CorrelationId"] = correlationId;

            _logger.LogError(exception,
                "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}",
                correlationId, context.Request.Path, context.Request.Method);

            var errorResponse = CreateErrorResponse(exception, correlationId, context);
            var statusCode = DetermineStatusCode(exception);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private ErrorResponse CreateErrorResponse(Exception exception, string correlationId, HttpContext context)
        {
            var errorType = GetErrorType(exception);
            var userFriendlyMessage = GetUserFriendlyMessage(exception);

            return new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = errorType,
                    Type = errorType,
                    Message = userFriendlyMessage,
                    Details = GetErrorDetails(exception)
                },
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path,
                Method = context.Request.Method
            };
        }

        private int DetermineStatusCode(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException => (int)HttpStatusCode.BadRequest,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                ValidationException => (int)HttpStatusCode.UnprocessableEntity,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                NotSupportedException => (int)HttpStatusCode.MethodNotAllowed,
                TimeoutException => (int)HttpStatusCode.RequestTimeout,
                TaskCanceledException => (int)HttpStatusCode.RequestTimeout,
                JsonException => (int)HttpStatusCode.BadRequest,
                InvalidOperationException when IsSoapFault(exception) => (int)HttpStatusCode.BadGateway,
                HttpRequestException => (int)HttpStatusCode.ServiceUnavailable,
                _ => (int)HttpStatusCode.InternalServerError
            };
        }

        private string GetErrorType(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException => "MISSING_REQUIRED_FIELD",
                ArgumentException => "INVALID_ARGUMENT",
                ValidationException => "VALIDATION_ERROR",
                UnauthorizedAccessException => "UNAUTHORIZED",
                NotSupportedException => "METHOD_NOT_ALLOWED",
                TimeoutException => "REQUEST_TIMEOUT",
                TaskCanceledException => "REQUEST_TIMEOUT",
                JsonException => "INVALID_JSON",
                InvalidOperationException when IsSoapFault(exception) => "SOAP_FAULT",
                HttpRequestException => "SERVICE_UNAVAILABLE",
                _ => "INTERNAL_ERROR"
            };
        }

        private string GetUserFriendlyMessage(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException => "Required field is missing",
                ArgumentException => "Invalid request parameters provided",
                ValidationException => "Request validation failed",
                UnauthorizedAccessException => "Authentication required",
                NotSupportedException => "HTTP method not supported for this endpoint",
                TimeoutException => "Request timed out, please try again",
                TaskCanceledException => "Request was cancelled or timed out",
                JsonException => "Invalid JSON format in request body",
                InvalidOperationException when IsSoapFault(exception) => "Backend service error",
                HttpRequestException => "Service temporarily unavailable",
                _ => "An unexpected error occurred"
            };
        }

        private Dictionary<string, object>? GetErrorDetails(Exception exception)
        {
            var details = new Dictionary<string, object>();

            // Add specific details based on exception type
            if (exception is ValidationException validationEx)
            {
                details["validationErrors"] = ExtractValidationErrors(validationEx);
            }
            else if (exception is ArgumentException argEx)
            {
                details["parameterName"] = argEx.ParamName ?? "unknown";
            }
            else if (exception is JsonException jsonEx)
            {
                details["parseError"] = "Invalid JSON syntax";
                if (jsonEx.LineNumber.HasValue)
                {
                    details["line"] = jsonEx.LineNumber.Value;
                }
                if (jsonEx.BytePositionInLine.HasValue)
                {
                    details["position"] = jsonEx.BytePositionInLine.Value;
                }
            }
            else if (IsSoapFault(exception))
            {
                details["backendError"] = ExtractSoapFaultDetails(exception);
            }

            return details.Any() ? details : null;
        }

        private object ExtractValidationErrors(ValidationException validationException)
        {
            // Extract validation errors from ValidationException
            // This would be customized based on the specific validation framework used
            return new
            {
                message = validationException.Message,
                source = "ModelValidation"
            };
        }

        private object ExtractSoapFaultDetails(Exception exception)
        {
            // Extract relevant SOAP fault information while maintaining security
            var details = new Dictionary<string, object>
            {
                ["type"] = "SOAP Fault",
                ["message"] = "Backend service reported an error"
            };

            // Look for specific SOAP fault patterns in the exception message
            var message = exception.Message;
            if (message.Contains("404", StringComparison.OrdinalIgnoreCase))
            {
                details["code"] = "ENDPOINT_NOT_FOUND";
                details["message"] = "Requested operation is not available";
            }
            else if (message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
            {
                details["code"] = "SERVICE_TIMEOUT";
                details["message"] = "Backend service did not respond in time";
            }
            else if (message.Contains("authentication", StringComparison.OrdinalIgnoreCase) ||
                     message.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
            {
                details["code"] = "BACKEND_AUTHENTICATION_FAILED";
                details["message"] = "Backend service authentication failed";
            }
            else if (message.Contains("validation", StringComparison.OrdinalIgnoreCase))
            {
                details["code"] = "BACKEND_VALIDATION_ERROR";
                details["message"] = "Backend service rejected the request data";
            }

            return details;
        }

        private bool IsSoapFault(Exception exception)
        {
            // Detect if this is a SOAP-related exception
            var message = exception.Message?.ToLowerInvariant();
            return message != null && (
                message.Contains("soap") ||
                message.Contains("wsdl") ||
                message.Contains("endpoint not found") ||
                message.Contains("request error") ||
                message.Contains("html error page") ||
                (exception is InvalidOperationException && message.Contains("service"))
            );
        }

        private string GenerateCorrelationId()
        {
            return Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// Custom exception for validation errors
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ValidationException class
        /// </summary>
        /// <param name="message">The error message</param>
        public ValidationException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the ValidationException class
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
