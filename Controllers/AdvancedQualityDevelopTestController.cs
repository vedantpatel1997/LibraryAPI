using AutoMapper;
using LibraryManagement.API.Helper;
using LibraryManagement.API.Modal;
using LibraryManagement.API.Repos.Models;
using LibraryManagement.API.Services.Implimentation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
namespace LibraryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdvancedQualityDevelopTestController : ControllerBase
    {
        private readonly ConnectionStringService _connectionStringService;
        private readonly ILogger<AdvancedQualityDevelopTestController> _logger;
        private readonly LibraryManagementContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        public AdvancedQualityDevelopTestController(ILogger<AdvancedQualityDevelopTestController> logger, LibraryManagementContext dbContext, IMapper mapper, IConfiguration configuration, ConnectionStringService connectionStringService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _mapper = mapper;
            _configuration = configuration;
            _connectionStringService = connectionStringService;
        }

        // 1. Endpoint to get API version
        [HttpGet("apiversion")]
        public IActionResult GetAPIVersion()
        {
            var versionResponse = new
            {
                version = _configuration.GetSection("APIInfo").Get<APIInfo>().Version,
                gitHubRepo = "https://github.com/vedantpatel1997/LibraryAPI"

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
        [HttpPost("SwitchDatabase")]
        public IActionResult SwitchDatabase([FromBody] SwitchDatabaseRequest request)
        {
            var response = new APIResponse<string>();
            Console.WriteLine("SwitchDatabase request received");

            // Validate input
            if (request?.DbKey != "old" && request?.DbKey != "new")
            {
                response.ResponseCode = 400;
                response.IsSuccess = false;
                response.ErrorMessage = "Invalid database key. Valid keys are 'old' or 'new'.";
                Console.WriteLine($"Invalid DbKey received: {request?.DbKey}");
                return BadRequest(response);
            }

            // Save the CURRENT DB KEY (not the raw connection string)
            var previousDbKey = _connectionStringService.GetCurrentDbKey();

            try
            {
                // Switch to the new database
                _connectionStringService.SetConnectionString(request.DbKey);

                // Test the new connection
                var connectionString = _connectionStringService.GetConnectionString();
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                }

                // Success response
                response.ResponseCode = 200;
                response.IsSuccess = true;
                response.Data = $"Switched to {request.DbKey} database.";
                Console.WriteLine($"Successfully switched to {request.DbKey} database.");
                return Ok(response);
            }
            catch (SqlException ex)
            {
                // Revert to the PREVIOUS DB KEY
                Console.WriteLine($"Reverting to previous database key: {previousDbKey}");
                _connectionStringService.SetConnectionString(previousDbKey);

                response.ResponseCode = 500;
                response.IsSuccess = false;
                response.ErrorMessage = $"Database connection failed. Reverted to {previousDbKey}.";
                return StatusCode(500, response);
            }
            catch (Exception ex)
            {
                // Revert to the PREVIOUS DB KEY
                Console.WriteLine($"Reverting to previous database key: {previousDbKey}");
                _connectionStringService.SetConnectionString(previousDbKey);

                response.ResponseCode = 500;
                response.IsSuccess = false;
                response.ErrorMessage = $"Unexpected error. Reverted to {previousDbKey}.";
                return StatusCode(500, response);
            }
        }

        [HttpGet("GetCurrentDatabase")]
        public IActionResult GetCurrentDatabase()
        {
            var response = new APIResponse<bool>();
            try
            {
                // Get the current database key ("old" or "new")
                var currentDbKey = _connectionStringService.GetCurrentDbKey();

                // Determine if the current database is "new"
                bool databaseIsNew = currentDbKey == "new";

                response.ResponseCode = 200;
                response.IsSuccess = true;
                response.Data = databaseIsNew;
                Console.WriteLine($"Current database is {(databaseIsNew ? "new" : "old")}.");

                return Ok(response);
            }
            catch (Exception ex)
            {
                response.ResponseCode = 500;
                response.IsSuccess = false;
                response.ErrorMessage = "Failed to retrieve the current database.";
                Console.WriteLine($"Error retrieving current database: {ex.Message}");

                return StatusCode(500, response);
            }
        }



        // Add a model to represent the request body
        public class SwitchDatabaseRequest
        {
            public string DbKey { get; set; }
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
                    Console.WriteLine($"IsCancellationRequested: {cancellationToken.IsCancellationRequested}.");
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
                    Console.WriteLine($"IsCancellationRequested: {cancellationToken.IsCancellationRequested}.");
                    return StatusCode(499, "Request canceled by the client.");
                }
            }

            Console.WriteLine("Operation completed successfully.");
            return Ok("Operation completed successfully.");
        }

        [HttpGet("appsettings")]
        public IActionResult GetAppSettings()
        {
            try
            {
                // Bind the entire configuration to a dictionary
                var appSettings = new Dictionary<string, object>();

                // Add APIInfo
                appSettings["APIInfo"] = _configuration.GetSection("APIInfo").Get<APIInfo>();

                // Add JWTSettings
                appSettings["JWTSettings"] = _configuration.GetSection("JWTSettings").Get<JWTSettings>();

                // Add Logging
                appSettings["Logging"] = _configuration.GetSection("Logging").Get<Logging>();

                // Add KeyVault
                appSettings["KeyVault"] = _configuration.GetSection("KeyVault").Get<KeyVault>();

                // Add EnvCheck
                appSettings["EnvCheck"] = _configuration.GetValue<string>("EnvCheck");

                // Add AllowedHosts
                appSettings["AllowedHosts"] = _configuration.GetValue<string>("AllowedHosts");

                // Add ConnectionStrings
                appSettings["ConnectionStrings"] = _configuration.GetSection("ConnectionStrings").Get<ConnectionStrings>();

                return Ok(appSettings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve appsettings.");
                return StatusCode(500, "An error occurred while retrieving appsettings.");
            }
        }
    }




    // Example Request Data Class for POST body (you can adjust as needed)
    public class SomeRequestData
    {
        public string Name { get; set; }
        public int Number { get; set; }
    }

    public class APIInfo
    {
        public string Version { get; set; }
    }

    public class JWTSettings
    {
        public string securityKey { get; set; }
    }

    public class Logging
    {
        public LogLevel LogLevel { get; set; }
    }

    public class LogLevel
    {
        public string Default { get; set; }
        public string Microsoft_AspNetCore { get; set; }
    }

    public class KeyVault
    {
        public string VaultUrl { get; set; }
    }

    public class ConnectionStrings
    {
        public string APIConnection { get; set; }
        public string SQL_Server_Conn_Conestoga_DB { get; set; }
        public string SQL_Server_Conn_Microsoft_DB { get; set; }
    }
}