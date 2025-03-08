using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var message = "Backend is running. Access the Swagger UI for API documentation: ";
            var swaggerUrl = $"{Request.Scheme}://{Request.Host}/swagger/index.html";
            var response = $"{message}<a href='{swaggerUrl}'>{swaggerUrl}</a>";

            return new ContentResult
            {
                Content = response,
                ContentType = "text/html"
            };
        }
    }
}