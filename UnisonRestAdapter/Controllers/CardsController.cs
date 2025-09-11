using Microsoft.AspNetCore.Mvc;
using UnisonRestAdapter.Models.Request;
using UnisonRestAdapter.Models.Response;
using UnisonRestAdapter.Services;

namespace UnisonRestAdapter.Controllers
{
    /// <summary>
    /// REST API controller for card operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CardsController : ControllerBase
    {
        private readonly IUnisonService _unisonService;
        private readonly ILogger<CardsController> _logger;

        /// <summary>
        /// Initializes a new instance of the CardsController
        /// </summary>
        /// <param name="unisonService">Service for communicating with the SOAP backend</param>
        /// <param name="logger">Logger for tracking controller operations</param>
        public CardsController(IUnisonService unisonService, ILogger<CardsController> logger)
        {
            _unisonService = unisonService;
            _logger = logger;
        }

        /// <summary>
        /// Updates card information
        /// </summary>
        /// <param name="request">Card update request containing the card information to be updated</param>
        /// <returns>Update result indicating success or failure with details</returns>
        /// <response code="200">Card updated successfully</response>
        /// <response code="400">Invalid request data or update failed</response>
        /// <response code="401">Unauthorized - Unison-Token header missing or invalid</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     PUT /api/cards/update
        ///     {
        ///         "cardId": "CARD123",
        ///         "userName": "john.doe",
        ///         "firstName": "John",
        ///         "lastName": "Doe",
        ///         "email": "john.doe@company.com",
        ///         "department": "Engineering",
        ///         "title": "Software Developer",
        ///         "isActive": true,
        ///         "expirationDate": "2025-12-31T00:00:00Z"
        ///     }
        /// 
        /// Requires Unison-Token header with valid authentication token.
        /// </remarks>
        [HttpPut]
        [Route("update")]
        [ProducesResponseType(typeof(UpdateCardResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<UpdateCardResponse>> UpdateCard([FromBody] UpdateCardRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Extract authentication token from header
            var token = HttpContext.Request.Headers["Unison-Token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Unison-Token header is required");
            }

            _logger.LogInformation("Received UpdateCard request for CardId: {CardId}", request.CardId);

            var response = await _unisonService.UpdateCardAsync(request, token);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Updates card information - Alternative route for compatibility
        /// </summary>
        /// <param name="request">Card update request</param>
        /// <returns>Update result</returns>
        [HttpPost]
        [Route("~/updatecard")]
        public async Task<ActionResult<UpdateCardResponse>> UpdateCardPost([FromBody] UpdateCardRequest request)
        {
            return await UpdateCard(request);
        }

        /// <summary>
        /// Gets card information by card ID
        /// </summary>
        /// <param name="cardId">Card identifier to retrieve information for</param>
        /// <returns>Card information if found, otherwise not found response</returns>
        /// <response code="200">Card information retrieved successfully</response>
        /// <response code="404">Card not found</response>
        /// <response code="401">Unauthorized - Unison-Token header missing or invalid</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/cards/CARD123
        /// 
        /// Requires Unison-Token header with valid authentication token.
        /// </remarks>
        [HttpGet]
        [Route("{cardId}")]
        [ProducesResponseType(typeof(UserResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<UserResponse>> GetCard(string cardId)
        {
            var token = HttpContext.Request.Headers["Unison-Token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Unison-Token header is required");
            }

            _logger.LogInformation("Received GetCard request for CardId: {CardId}", cardId);

            var response = await _unisonService.GetUserAsync(cardId, token);

            return response.Success ? Ok(response) : NotFound(response);
        }

        /// <summary>
        /// Validates card information
        /// </summary>
        /// <param name="request">Card validation request</param>
        /// <returns>Validation result with card status information</returns>
        /// <response code="200">Card validation completed</response>
        /// <response code="400">Invalid request data or validation failed</response>
        /// <response code="401">Unauthorized - Unison-Token header missing or invalid</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/cards/validate
        ///     {
        ///         "cardId": "CARD123",
        ///         "userId": "john.doe",
        ///         "profileName": "StandardProfile"
        ///     }
        /// 
        /// Requires Unison-Token header with valid authentication token.
        /// </remarks>
        [HttpPost]
        [Route("validate")]
        [ProducesResponseType(typeof(CardValidationResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<CardValidationResponse>> ValidateCard([FromBody] CardValidationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = HttpContext.Request.Headers["Unison-Token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Unison-Token header is required");
            }

            _logger.LogInformation("Received ValidateCard request for CardId: {CardId}", request.CardId);

            var response = await _unisonService.ValidateCardAsync(request, token);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Activates a card
        /// </summary>
        /// <param name="request">Card activation request</param>
        /// <returns>Activation result</returns>
        /// <response code="200">Card activated successfully</response>
        /// <response code="400">Invalid request data or activation failed</response>
        /// <response code="401">Unauthorized - Unison-Token header missing or invalid</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/cards/activate
        ///     {
        ///         "cardId": "CARD123",
        ///         "userId": "john.doe",
        ///         "profileName": "StandardProfile"
        ///     }
        /// 
        /// Requires Unison-Token header with valid authentication token.
        /// </remarks>
        [HttpPost]
        [Route("activate")]
        [ProducesResponseType(typeof(CardActivationResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<CardActivationResponse>> ActivateCard([FromBody] CardActivationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = HttpContext.Request.Headers["Unison-Token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Unison-Token header is required");
            }

            _logger.LogInformation("Received ActivateCard request for CardId: {CardId}", request.CardId);

            var response = await _unisonService.ActivateCardAsync(request, token);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Deactivates a card
        /// </summary>
        /// <param name="request">Card deactivation request</param>
        /// <returns>Deactivation result</returns>
        /// <response code="200">Card deactivated successfully</response>
        /// <response code="400">Invalid request data or deactivation failed</response>
        /// <response code="401">Unauthorized - Unison-Token header missing or invalid</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/cards/deactivate
        ///     {
        ///         "cardId": "CARD123",
        ///         "userId": "john.doe",
        ///         "profileName": "StandardProfile"
        ///     }
        /// 
        /// Requires Unison-Token header with valid authentication token.
        /// </remarks>
        [HttpPost]
        [Route("deactivate")]
        [ProducesResponseType(typeof(CardActivationResponse), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<CardActivationResponse>> DeactivateCard([FromBody] CardActivationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = HttpContext.Request.Headers["Unison-Token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Unison-Token header is required");
            }

            _logger.LogInformation("Received DeactivateCard request for CardId: {CardId}", request.CardId);

            var response = await _unisonService.DeactivateCardAsync(request, token);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Gets card status by card ID
        /// </summary>
        /// <param name="cardId">Card identifier to get status for</param>
        /// <returns>Card status information</returns>
        /// <response code="200">Card status retrieved successfully</response>
        /// <response code="404">Card not found</response>
        /// <response code="401">Unauthorized - Unison-Token header missing or invalid</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/cards/status/CARD123
        /// 
        /// Requires Unison-Token header with valid authentication token.
        /// </remarks>
        [HttpGet]
        [Route("status/{cardId}")]
        [ProducesResponseType(typeof(CardValidationResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<CardValidationResponse>> GetCardStatus(string cardId)
        {
            var token = HttpContext.Request.Headers["Unison-Token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Unison-Token header is required");
            }

            _logger.LogInformation("Received GetCardStatus request for CardId: {CardId}", cardId);

            var request = new CardValidationRequest { CardId = cardId };
            var response = await _unisonService.ValidateCardAsync(request, token);

            return response.Success ? Ok(response) : NotFound(response);
        }
    }
}
