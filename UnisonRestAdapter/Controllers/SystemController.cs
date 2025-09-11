using Microsoft.AspNetCore.Mvc;
using UnisonRestAdapter.Models.Response;
using UnisonRestAdapter.Services;

namespace UnisonRestAdapter.Controllers
{
    /// <summary>
    /// REST API controller for system operations
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController : ControllerBase
    {
        private readonly IUnisonService _unisonService;
        private readonly ILogger<SystemController> _logger;

        /// <summary>
        /// Initializes a new instance of the SystemController
        /// </summary>
        /// <param name="unisonService">Service for communicating with the SOAP backend</param>
        /// <param name="logger">Logger for tracking controller operations</param>
        public SystemController(IUnisonService unisonService, ILogger<SystemController> logger)
        {
            _unisonService = unisonService;
            _logger = logger;
        }

        /// <summary>
        /// Gets system version information
        /// </summary>
        /// <returns>Version information including API and backend versions</returns>
        /// <response code="200">Version information retrieved successfully</response>
        /// <response code="401">Unauthorized - Unison-Token header missing or invalid</response>
        /// <response code="500">Internal server error retrieving version information</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/system/version
        /// 
        /// Requires Unison-Token header with valid authentication token.
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "success": true,
        ///         "message": "Version information retrieved",
        ///         "apiVersion": "1.0.0",
        ///         "backendVersion": "Connected",
        ///         "buildInfo": {
        ///             "buildDate": "2025-09-11T10:00:00Z",
        ///             "environment": "Development",
        ///             "runtimeVersion": "9.0.0"
        ///         },
        ///         "timestamp": "2025-09-11T10:00:00Z"
        ///     }
        /// </remarks>
        [HttpGet]
        [Route("version")]
        [ProducesResponseType(typeof(VersionResponse), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<VersionResponse>> GetVersion()
        {
            var token = HttpContext.Request.Headers["Unison-Token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Unison-Token header is required");
            }

            _logger.LogInformation("Received GetVersion request");

            try
            {
                var response = await _unisonService.GetVersionAsync(token);
                return response.Success ? Ok(response) : StatusCode(500, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing GetVersion request");
                return StatusCode(500, new VersionResponse
                {
                    Success = false,
                    Message = $"Internal server error: {ex.Message}",
                    ApiVersion = "1.0.0"
                });
            }
        }

        /// <summary>
        /// Pings the backend service to test connectivity
        /// </summary>
        /// <returns>Ping result indicating backend service connectivity status</returns>
        /// <response code="200">Backend service is reachable</response>
        /// <response code="401">Unauthorized - Unison-Token header missing or invalid</response>
        /// <response code="503">Backend service is not reachable</response>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/system/ping
        /// 
        /// Requires Unison-Token header with valid authentication token.
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "success": true,
        ///         "message": "Backend service is reachable",
        ///         "timestamp": "2025-09-11T10:00:00Z"
        ///     }
        /// </remarks>
        [HttpGet]
        [Route("ping")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(503)]
        public async Task<ActionResult<object>> Ping()
        {
            var token = HttpContext.Request.Headers["Unison-Token"].FirstOrDefault();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Unison-Token header is required");
            }

            _logger.LogInformation("Received Ping request");

            try
            {
                var isConnected = await _unisonService.PingAsync(token);

                var response = new
                {
                    success = isConnected,
                    message = isConnected ? "Backend service is reachable" : "Backend service is not reachable",
                    timestamp = DateTime.UtcNow
                };

                return isConnected ? Ok(response) : StatusCode(503, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Ping request");
                var response = new
                {
                    success = false,
                    message = $"Internal server error: {ex.Message}",
                    timestamp = DateTime.UtcNow
                };
                return StatusCode(503, response);
            }
        }
    }
}
