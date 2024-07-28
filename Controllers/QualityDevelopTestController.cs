using AutoMapper;
using LibraryManagement.API.Helper;
using LibraryManagement.API.Repos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace LibraryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QualityDevelopTestController : ControllerBase
    {
        private readonly ILogger<QualityDevelopTestController> _logger;
        private readonly LibraryManagementContext _dbContext;
        private readonly IMapper _mapper;

        public QualityDevelopTestController(ILogger<QualityDevelopTestController> logger, LibraryManagementContext dbContext, IMapper mapper)
        {
            _logger = logger;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        // 1. Always returns a 500 error
        [HttpGet("BreakApplication500Error")]
        public Task<IActionResult> BreakApplication500Error()
        {
            // Simulate a 500 Internal Server Error
            throw new Exception("Vedant's custom 500 error to check Auto Heal.");
        }

        // 2. Returns a response after 60 seconds
        [HttpGet("delayedresponse")]
        public async Task<IActionResult> DelayedResponse()
        {
            await Task.Delay(60000); // 60 seconds delay
            return Ok("Response after 60 seconds");
        }

        // 3. Health check endpoint to verify database connection
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
    }
}
