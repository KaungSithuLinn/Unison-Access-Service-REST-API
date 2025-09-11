using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net;
using UnisonRestAdapter.Models.Responses;
using System.Text.RegularExpressions;

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

            // Enhanced logging with structured data
            _logger.LogError(exception,
                "Unhandled exception occurred. CorrelationId: {CorrelationId}, Path: {Path}, Method: {Method}, User: {User}, RequestId: {RequestId}",
                correlationId, context.Request.Path, context.Request.Method,
                context.User?.Identity?.Name ?? "Anonymous", context.TraceIdentifier);

            var errorResponse = CreateErrorResponse(exception, correlationId, context);
            var statusCode = DetermineStatusCode(exception);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            // Add correlation ID to response headers for tracing
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            context.Response.Headers["X-Request-ID"] = context.TraceIdentifier;

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
            // Enhanced SOAP fault information extraction with better pattern detection
            var details = new Dictionary<string, object>
            {
                ["type"] = "SOAP Fault",
                ["source"] = "Backend Service"
            };

            var message = exception.Message;
            var innerMessage = exception.InnerException?.Message;
            var fullMessage = $"{message} {innerMessage}".ToLowerInvariant();

            // Enhanced SOAP fault pattern detection
            if (ContainsAny(fullMessage, new[] { "404", "not found", "endpoint not found" }))
            {
                details["code"] = "ENDPOINT_NOT_FOUND";
                details["message"] = "The requested SOAP operation or endpoint is not available";
                details["httpStatusCode"] = 404;
            }
            else if (ContainsAny(fullMessage, new[] { "timeout", "request timeout", "operation timeout" }))
            {
                details["code"] = "SERVICE_TIMEOUT";
                details["message"] = "Backend service did not respond within the expected time";
                details["httpStatusCode"] = 408;
            }
            else if (ContainsAny(fullMessage, new[] { "authentication", "unauthorized", "access denied", "invalid token" }))
            {
                details["code"] = "BACKEND_AUTHENTICATION_FAILED";
                details["message"] = "Backend service authentication failed";
                details["httpStatusCode"] = 401;
            }
            else if (ContainsAny(fullMessage, new[] { "validation", "invalid", "bad request", "malformed" }))
            {
                details["code"] = "BACKEND_VALIDATION_ERROR";
                details["message"] = "Backend service rejected the request data format";
                details["httpStatusCode"] = 400;
            }
            else if (ContainsAny(fullMessage, new[] { "server error", "internal error", "fault", "500" }))
            {
                details["code"] = "BACKEND_INTERNAL_ERROR";
                details["message"] = "Backend service encountered an internal error";
                details["httpStatusCode"] = 500;
            }
            else if (ContainsAny(fullMessage, new[] { "service unavailable", "503", "connection refused", "cannot connect" }))
            {
                details["code"] = "SERVICE_UNAVAILABLE";
                details["message"] = "Backend service is temporarily unavailable";
                details["httpStatusCode"] = 503;
            }
            else
            {
                details["code"] = "UNKNOWN_SOAP_FAULT";
                details["message"] = "Backend service reported an unspecified error";
                details["httpStatusCode"] = 502;
            }

            // Extract additional SOAP fault details if available
            if (TryExtractSoapFaultCode(message, out var faultCode))
            {
                details["soapFaultCode"] = faultCode;
            }

            if (TryExtractSoapFaultReason(message, out var faultReason))
            {
                details["soapFaultReason"] = faultReason;
            }

            // Add sanitized error context (avoid exposing sensitive info)
            details["context"] = CreateSanitizedErrorContext(exception);

            return details;
        }

        private bool ContainsAny(string text, string[] patterns)
        {
            return patterns.Any(pattern => text.Contains(pattern, StringComparison.OrdinalIgnoreCase));
        }

        private bool TryExtractSoapFaultCode(string message, out string faultCode)
        {
            faultCode = string.Empty;

            // Pattern to match SOAP fault codes like "soap:Server", "soap:Client", etc.
            var faultCodePattern = @"(?:soap:|env:)?(\w+):(\w+)";
            var match = Regex.Match(message, faultCodePattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                faultCode = match.Groups[0].Value;
                return true;
            }

            // Alternative pattern for fault codes in angle brackets
            var bracketPattern = @"<faultcode>(.*?)</faultcode>";
            match = Regex.Match(message, bracketPattern, RegexOptions.IgnoreCase);

            if (match.Success)
            {
                faultCode = match.Groups[1].Value;
                return true;
            }

            return false;
        }

        private bool TryExtractSoapFaultReason(string message, out string faultReason)
        {
            faultReason = string.Empty;

            // Pattern to match SOAP fault strings
            var reasonPatterns = new[]
            {
                @"<faultstring>(.*?)</faultstring>",
                @"<env:Reason>(.*?)</env:Reason>",
                @"""faultstring""\s*:\s*""([^""]+)"""
            };

            foreach (var pattern in reasonPatterns)
            {
                var match = Regex.Match(message, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                if (match.Success)
                {
                    faultReason = match.Groups[1].Value.Trim();
                    return true;
                }
            }

            return false;
        }

        private object CreateSanitizedErrorContext(Exception exception)
        {
            return new
            {
                exceptionType = exception.GetType().Name,
                hasInnerException = exception.InnerException != null,
                stackTraceAvailable = !string.IsNullOrEmpty(exception.StackTrace),
                // Don't include actual stack trace or sensitive info in production
                timestamp = DateTime.UtcNow
            };
        }

        private bool IsSoapFault(Exception exception)
        {
            // Enhanced SOAP fault detection with comprehensive pattern matching
            var message = exception.Message?.ToLowerInvariant();
            var innerMessage = exception.InnerException?.Message?.ToLowerInvariant();
            var fullMessage = $"{message} {innerMessage}";

            if (string.IsNullOrEmpty(fullMessage))
                return false;

            // Primary SOAP indicators
            var soapIndicators = new[]
            {
                "soap",
                "wsdl",
                "soap:fault",
                "env:fault",
                "soap envelope",
                "faultcode",
                "faultstring"
            };

            // Service communication indicators
            var serviceIndicators = new[]
            {
                "endpoint not found",
                "request error",
                "html error page",
                "service error",
                "web service",
                "xml parse",
                "soap action"
            };

            // HTTP status indicators that might be SOAP-related
            var httpIndicators = new[]
            {
                "500 internal server error",
                "502 bad gateway",
                "503 service unavailable",
                "404 not found"
            };

            var isIndicatorMatch = soapIndicators.Any(indicator => fullMessage.Contains(indicator)) ||
                                   serviceIndicators.Any(indicator => fullMessage.Contains(indicator)) ||
                                   httpIndicators.Any(indicator => fullMessage.Contains(indicator));

            // Exception type-based detection
            var isSoapExceptionType = exception is InvalidOperationException ||
                                      exception is HttpRequestException ||
                                      exception is TaskCanceledException ||
                                      exception is TimeoutException;

            return isIndicatorMatch && isSoapExceptionType;
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
