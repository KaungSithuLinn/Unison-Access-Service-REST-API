using Microsoft.AspNetCore.Mvc;
using UnisonRestAdapter.Models.Response;
using UnisonRestAdapter.Services;

namespace UnisonRestAdapter.Controllers
{
    /// <summary>
    /// REST API controller for health check operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IUnisonService _unisonService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IUnisonService unisonService, ILogger<HealthController> logger)
        {
            _unisonService = unisonService;
            _logger = logger;
        }

        /// <summary>
        /// Performs health check on the Unison service
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet]
        public async Task<ActionResult<HealthResponse>> GetHealth()
        {
            var token = HttpContext.Request.Headers["Unison-Token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Unison-Token header is required");
            }

            _logger.LogInformation("Received health check request");

            var response = await _unisonService.CheckHealthAsync(token);

            return response.IsHealthy ? Ok(response) : StatusCode(503, response);
        }

        /// <summary>
        /// Simple health check that doesn't require authentication
        /// </summary>
        /// <returns>Basic health status</returns>
        [HttpGet]
        [Route("ping")]
        public ActionResult<object> Ping()
        {
            return Ok(new
            {
                Status = "Healthy",
                Timestamp = DateTime.UtcNow,
                Service = "UnisonRestAdapter"
            });
        }
    }
}
