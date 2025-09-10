using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using UnisonRestAdapter.Models.Request;

namespace UnisonRestAdapter.Services
{
    /// <summary>
    /// Service for validating JSON requests and converting them to SOAP templates
    /// Implements TASK-002: SOAP Request Validation Templates
    /// </summary>
    public class ValidationService : IValidationService
    {
        private readonly ILogger<ValidationService> _logger;

        // SOAP template for UpdateCard operation
        private const string UPDATE_CARD_SOAP_TEMPLATE = @"
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" 
               xmlns:uni=""http://tempuri.org/"">
  <soap:Header>
    <uni:AuthenticationToken>{Token}</uni:AuthenticationToken>
  </soap:Header>
  <soap:Body>
    <uni:UpdateCard>
      <uni:request>
        <uni:CardId>{CardId}</uni:CardId>
        <uni:UserName>{UserName}</uni:UserName>
        <uni:FirstName>{FirstName}</uni:FirstName>
        <uni:LastName>{LastName}</uni:LastName>
        <uni:Email>{Email}</uni:Email>
        <uni:Department>{Department}</uni:Department>
        <uni:Title>{Title}</uni:Title>
        <uni:IsActive>{IsActive}</uni:IsActive>
        <uni:ExpirationDate>{ExpirationDate}</uni:ExpirationDate>
      </uni:request>
    </uni:UpdateCard>
  </soap:Body>
</soap:Envelope>";

        public ValidationService(ILogger<ValidationService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Validates UpdateCard JSON request and returns validation result
        /// </summary>
        public ValidationResult ValidateUpdateCardRequest(UpdateCardRequest request)
        {
            var validationErrors = new List<string>();
            var correlationId = Guid.NewGuid().ToString();

            try
            {
                _logger.LogInformation("Validating UpdateCard request with CorrelationId: {CorrelationId}", correlationId);

                // Required field validation
                if (string.IsNullOrWhiteSpace(request.CardId))
                {
                    validationErrors.Add("CardId is required and cannot be empty");
                }

                // CardId format validation
                if (!string.IsNullOrWhiteSpace(request.CardId))
                {
                    if (!IsValidCardId(request.CardId))
                    {
                        validationErrors.Add("CardId must be alphanumeric and between 1-50 characters");
                    }
                }

                // Email format validation (if provided)
                if (!string.IsNullOrWhiteSpace(request.Email) && !IsValidEmail(request.Email))
                {
                    validationErrors.Add("Email format is invalid");
                }

                // ExpirationDate validation (if provided)
                if (request.ExpirationDate.HasValue && request.ExpirationDate.Value <= DateTime.Now)
                {
                    validationErrors.Add("ExpirationDate must be in the future when provided");
                }

                var result = new ValidationResult
                {
                    IsValid = validationErrors.Count == 0,
                    Errors = validationErrors,
                    CorrelationId = correlationId,
                    Timestamp = DateTime.UtcNow
                };

                if (result.IsValid)
                {
                    _logger.LogInformation("UpdateCard request validation successful. CorrelationId: {CorrelationId}", correlationId);
                }
                else
                {
                    _logger.LogWarning("UpdateCard request validation failed. Errors: {Errors}, CorrelationId: {CorrelationId}",
                        string.Join(", ", validationErrors), correlationId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during UpdateCard request validation. CorrelationId: {CorrelationId}", correlationId);
                return new ValidationResult
                {
                    IsValid = false,
                    Errors = new List<string> { "Internal validation error occurred" },
                    CorrelationId = correlationId,
                    Timestamp = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Converts validated UpdateCard request to SOAP envelope string
        /// </summary>
        public string GenerateUpdateCardSoapEnvelope(UpdateCardRequest request, string authToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (string.IsNullOrWhiteSpace(authToken))
                throw new ArgumentException("Auth token cannot be null or empty", nameof(authToken));

            var correlationId = Guid.NewGuid().ToString();
            _logger.LogInformation("Generating SOAP envelope for UpdateCard. CorrelationId: {CorrelationId}", correlationId);

            try
            {
                var soapEnvelope = UPDATE_CARD_SOAP_TEMPLATE
                    .Replace("{Token}", System.Security.SecurityElement.Escape(authToken))
                    .Replace("{CardId}", System.Security.SecurityElement.Escape(request.CardId ?? ""))
                    .Replace("{UserName}", System.Security.SecurityElement.Escape(request.UserName ?? ""))
                    .Replace("{FirstName}", System.Security.SecurityElement.Escape(request.FirstName ?? ""))
                    .Replace("{LastName}", System.Security.SecurityElement.Escape(request.LastName ?? ""))
                    .Replace("{Email}", System.Security.SecurityElement.Escape(request.Email ?? ""))
                    .Replace("{Department}", System.Security.SecurityElement.Escape(request.Department ?? ""))
                    .Replace("{Title}", System.Security.SecurityElement.Escape(request.Title ?? ""))
                    .Replace("{IsActive}", (request.IsActive ?? false).ToString().ToLowerInvariant())
                    .Replace("{ExpirationDate}", request.ExpirationDate?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? "");

                _logger.LogDebug("SOAP envelope generated successfully. CorrelationId: {CorrelationId}", correlationId);
                return soapEnvelope;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating SOAP envelope. CorrelationId: {CorrelationId}", correlationId);
                throw;
            }
        }

        /// <summary>
        /// Creates structured JSON error response for validation failures
        /// </summary>
        public object CreateValidationErrorResponse(ValidationResult validationResult)
        {
            if (validationResult == null)
                throw new ArgumentNullException(nameof(validationResult));

            return new
            {
                success = false,
                message = string.Join("; ", validationResult.Errors),
                correlationId = validationResult.CorrelationId,
                timestamp = validationResult.Timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                errors = validationResult.Errors
            };
        }

        /// <summary>
        /// Validates CardId format (alphanumeric, 1-50 characters)
        /// </summary>
        private static bool IsValidCardId(string cardId)
        {
            if (string.IsNullOrWhiteSpace(cardId))
                return false;

            return cardId.Length >= 1 && cardId.Length <= 50 &&
                   cardId.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_');
        }        /// <summary>
                 /// Basic email format validation
                 /// </summary>
        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var emailAttribute = new EmailAddressAttribute();
                return emailAttribute.IsValid(email);
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Interface for validation service
    /// </summary>
    public interface IValidationService
    {
        ValidationResult ValidateUpdateCardRequest(UpdateCardRequest request);
        string GenerateUpdateCardSoapEnvelope(UpdateCardRequest request, string authToken);
        object CreateValidationErrorResponse(ValidationResult validationResult);
    }

    /// <summary>
    /// Validation result model
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string CorrelationId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
