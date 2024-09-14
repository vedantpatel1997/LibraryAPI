using AutoMapper;
using LibraryManagement.API.Helper;
using LibraryManagement.API.Repos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
namespace LibraryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvancedQualityDevelopTestController : ControllerBase
    {
        private readonly ILogger<AdvancedQualityDevelopTestController> _logger;
        private readonly LibraryManagementContext _dbContext;
        private readonly IMapper _mapper;
        private readonly APIInfo _apiInfo;
        private readonly IConfiguration _configuration;
        public AdvancedQualityDevelopTestController(ILogger<AdvancedQualityDevelopTestController> logger, LibraryManagementContext dbContext, IMapper mapper, IOptions<APIInfo> apiInfo, IConfiguration configuration)
        {
            _logger = logger;
            _dbContext = dbContext;
            _mapper = mapper;
            _apiInfo = apiInfo.Value;
            _configuration = configuration;
        }

        // 1. Endpoint to get API version
        [HttpGet("apiversion")]
        public IActionResult GetAPIVersion()
        {
            var versionResponse = new
            {
                version = _apiInfo.Version
            };
            return Ok(versionResponse);
        }

        // 2. Always returns a 500 error
        [HttpGet("BreakApplication500Error")]
        public Task<IActionResult> BreakApplication500Error()
        {
            // Simulate a 500 Internal Server Error
            throw new Exception("Vedant's custom 500 error to check Auto Heal.");
        }

        // 3. Returns a response after 60 seconds
        [HttpGet("delayedresponse")]
        public async Task<IActionResult> DelayedResponse()
        {
            await Task.Delay(60000); // 60 seconds delay
            return Ok("Response after 60 seconds");
        }

        // 4. Health check endpoint to verify database connection
        [HttpGet("health")]
        public async Task<IActionResult> HealthCheck()
        {
            try
            {
                var canConnect = await _dbContext.Database.CanConnectAsync();
                if (canConnect)
                {
                    var healthResponse = new
                    {
                        status = "Healthy",
                        dbConnection = "Successful"
                    };
                    return Ok(healthResponse);
                }
                else
                {
                    var healthResponse = new
                    {
                        status = "Unhealthy",
                        dbConnection = "Failed"
                    };
                    return StatusCode(500, healthResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed.");
                var healthResponse = new
                {
                    status = "Unhealthy",
                    dbConnection = "Failed",
                    errorMessage = ex.Message
                };
                return StatusCode(500, healthResponse);
            }
        }

        // 5. Get the value of EnvCheck
        [HttpGet("envcheck")]
        public IActionResult GetEnvCheck()
        {
            var envCheckValue = _configuration["EnvCheck"];
            var response = new
            {
                EnvCheck = envCheckValue
            };
            return Ok(response);
        }
    }
}
