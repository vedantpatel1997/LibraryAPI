﻿using AutoMapper;
using LibraryManagement.API.Helper;
using LibraryManagement.API.Repos.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

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

        public AdvancedQualityDevelopTestController(ILogger<AdvancedQualityDevelopTestController> logger, LibraryManagementContext dbContext, IMapper mapper, IOptions<APIInfo> apiInfo)
        {
            _logger = logger;
            _dbContext = dbContext;
            _mapper = mapper;
            _apiInfo = apiInfo.Value;
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
            await Task.Delay(240000); // 60 seconds delay
            return Ok("Response after 240 seconds");
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

        // 5. Endpoint to simulate a segmentation fault by triggering a StackOverflowException
        [HttpGet("triggersegfault")]
        public IActionResult TriggerSegFault()
        {
            try
            {
                CauseStackOverflow();
            }
            catch (StackOverflowException)
            {
                // This catch block won't be hit because the process will terminate
            }

            return Ok("This won't be reached.");
        }

        private void CauseStackOverflow()
        {
            Console.WriteLine("Segementation Fault triggered, Recursively call the method to cause a stack overflow. By Vedant Patel");
            CauseStackOverflow(); // Recursively call the method to cause a stack overflow
        }

    }
}
