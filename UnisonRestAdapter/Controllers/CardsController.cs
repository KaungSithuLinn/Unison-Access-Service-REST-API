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

        public CardsController(IUnisonService unisonService, ILogger<CardsController> logger)
        {
            _unisonService = unisonService;
            _logger = logger;
        }

        /// <summary>
        /// Updates card information
        /// </summary>
        /// <param name="request">Card update request</param>
        /// <returns>Update result</returns>
        [HttpPut]
        [Route("update")]
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
        /// <param name="cardId">Card identifier</param>
        /// <returns>Card information</returns>
        [HttpGet]
        [Route("{cardId}")]
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
    }
}
