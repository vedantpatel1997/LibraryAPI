using LibraryManagement.API.Container.Service;
using LibraryManagement.API.Modal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [EnableCors]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizeController : ControllerBase
    {
        private readonly IAuthorize _authorize;

        public AuthorizeController(IAuthorize authorize)
        {
            this._authorize = authorize;
        }
        [HttpPost("GenerateToken")]
        public async Task<IActionResult> GenerateToken([FromBody] UserCredentails userCred)
        {
            var data = await _authorize.GenerateToken(userCred);
            return Ok(data);
        }

        [HttpPost("GenerateRefreshToken")]
        public async Task<IActionResult> GenerateRefreshToken([FromBody] TokenResponse tokenResponse)
        {
            var data = await _authorize.GenerateRefreshToken(tokenResponse);
            return Ok(data);
        }
    }
}
