using Microsoft.AspNetCore.Mvc;
using UnisonRestAdapter.Models.Response;
using UnisonRestAdapter.Services;

namespace UnisonRestAdapter.Controllers
{
    /// <summary>
    /// REST API controller for user operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUnisonService _unisonService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUnisonService unisonService, ILogger<UsersController> logger)
        {
            _unisonService = unisonService;
            _logger = logger;
        }

        /// <summary>
        /// Gets user information by user ID
        /// </summary>
        /// <param name="userId">User identifier</param>
        /// <returns>User information</returns>
        [HttpGet]
        [Route("{userId}")]
        public async Task<ActionResult<UserResponse>> GetUser(string userId)
        {
            var token = HttpContext.Request.Headers["Unison-Token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Unison-Token header is required");
            }

            _logger.LogInformation("Received GetUser request for UserId: {UserId}", userId);

            var response = await _unisonService.GetUserAsync(userId, token);

            return response.Success ? Ok(response) : NotFound(response);
        }
    }
}
