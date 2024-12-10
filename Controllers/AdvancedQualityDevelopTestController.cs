using AutoMapper;
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
            Console.WriteLine("Here is the API version, printing in the stdout VP.");
            return Ok(versionResponse);
        }

        // 2. Always returns a 500 error
        [HttpGet("BreakApplication500Error")]
        public Task<IActionResult> BreakApplication500Error()
        {
            Console.WriteLine("Running break application command. Vedant 23 October.");
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

        // 6. Cancellation tocken concept
        //[HttpGet("long-running-operation")]
        //public async Task StreamResponse(CancellationToken cancellationToken)
        //{
        //    Response.ContentType = "text/event-stream";  // Set response type for SSE

        //    _logger.LogInformation("StreamResponse started at {Timestamp}", DateTime.UtcNow);

        //    try
        //    {
        //        for (int i = 0; i < 5; i++)
        //        {
        //            // Simulate processing time
        //            await Task.Delay(1000, cancellationToken); // 1-second delay per step

        //            if (cancellationToken.IsCancellationRequested)
        //            {
        //                _logger.LogWarning("StreamResponse was canceled by the client at step {Step} at {Timestamp}", i + 1, DateTime.UtcNow);
        //                await Response.WriteAsync("data: Request was canceled\n\n");
        //                await Response.Body.FlushAsync();
        //                return;
        //            }

        //            // Log progress
        //            _logger.LogInformation("Step {Step}/5 completed at {Timestamp}", i + 1, DateTime.UtcNow);

        //            // Send each part of the response to the client as soon as it's processed
        //            await Response.WriteAsync($"data: Step {i + 1}/5 is done\n\n");
        //            await Response.Body.FlushAsync();
        //        }

        //        // Log completion
        //        _logger.LogInformation("StreamResponse completed successfully at {Timestamp}", DateTime.UtcNow);

        //        // Final response when processing is done
        //        await Response.WriteAsync("data: Processing complete!\n\n");
        //        await Response.Body.FlushAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log any errors
        //        _logger.LogError(ex, "An error occurred while processing StreamResponse at {Timestamp}", DateTime.UtcNow);

        //        // Send error message to the client
        //        await Response.WriteAsync($"data: An error occurred: {ex.Message}\n\n");
        //        await Response.Body.FlushAsync();
        //    }
        //}

        //[HttpGet("long-running-operation")]

        //public async Task<IActionResult> LongRunningOperation()
        //{
        //    // Retrieve cancellation token from HttpContext
        //    var cancellationToken = HttpContext.Items["CancellationToken"] as CancellationToken? ?? HttpContext.RequestAborted;


        //    int iterations = 10;
        //    for (int i = 0; i < iterations; i++)
        //    {
        //        if (cancellationToken.IsCancellationRequested)
        //        {
        //            Console.WriteLine("Request was canceled in iteration.");
        //            return StatusCode(499, "Request canceled by the client.");
        //        }

        //        Console.WriteLine($"Iteration {i + 1} of {iterations}...");
        //        await Task.Delay(1000, cancellationToken);
        //    }

        //    return Ok("Operation completed successfully.");

        //    //catch (TaskCanceledException)
        //    //{
        //    //    Console.WriteLine("Request was canceled.");
        //    //    return StatusCode(499, "Request canceled by the client.");
        //    //}
        //}

        [HttpGet("long-running-operation")]
        public async Task<IActionResult> LongRunningOperation(CancellationToken cancellationToken)
        {
            int iterations = 10; // Number of iterations for the long-running operation
            for (int i = 0; i < iterations; i++)
            {
                // Check if the request has been canceled
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine($"Request was canceled in iteration {i + 1}.");
                    return StatusCode(499, "Request canceled by the client."); // 499: Client Closed Request
                }

                Console.WriteLine($"Iteration {i + 1} of {iterations}... (Server is processing)");

                // Simulate some work with a delay
                try
                {
                    await Task.Delay(1000, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"TaskCanceledException caught in iteration {i + 1}.");
                    return StatusCode(499, "Request canceled by the client.");
                }
            }

            Console.WriteLine("Operation completed successfully.");
            return Ok("Operation completed successfully.");
        }

        [HttpPost("long-running-operation-post")]
        public async Task<IActionResult> LongRunningOperation([FromBody] SomeRequestData requestData, CancellationToken cancellationToken)
        {
            int iterations = 10; // Number of iterations for the long-running operation
            for (int i = 0; i < iterations; i++)
            {
                // Check if the request has been canceled
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine($"Request was canceled in iteration {i + 1}.");
                    return StatusCode(499, "Request canceled by the client."); // 499: Client Closed Request
                }

                Console.WriteLine($"Iteration {i + 1} of {iterations}... (Server is processing)");

                // Simulate some work with a delay
                try
                {
                    await Task.Delay(1000, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine($"TaskCanceledException caught in iteration {i + 1}.");
                    return StatusCode(499, "Request canceled by the client.");
                }
            }

            Console.WriteLine("Operation completed successfully.");
            return Ok("Operation completed successfully.");
        }
    }

    // Example Request Data Class for POST body (you can adjust as needed)
    public class SomeRequestData
    {
        public string Name { get; set; }
        public int Number { get; set; }
    }
}