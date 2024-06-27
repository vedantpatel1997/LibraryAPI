using LibraryManagement.API.Container.Service;
using LibraryManagement.API.Modal;
using LibraryManagement.API.Repos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [EnableCors]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersSvc;

        public UsersController(IUsersService usersSvc)
        {
            this._usersSvc = usersSvc;
        }

        [HttpGet("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            var data = await _usersSvc.GetAll();
            return Ok(data);
        }

        [HttpGet("GetById")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _usersSvc.GetById(id);
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] UserModal user)
        {
            var data = await _usersSvc.Create(user);
            return Ok(data);
        }

        [HttpPost("Update")]
        public async Task<IActionResult> Update(UserModal user, int id)
        {
            var data = await _usersSvc.Update(user, id);
            return Ok(data);
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _usersSvc.Remove(id);
            return Ok(data);
        }

        [HttpPost("CreateAddress")]
        public async Task<IActionResult> CreateAddress([FromBody] AddressModal addressData, int userId)
        {
            var data = await _usersSvc.CreateAddress(addressData, userId);
            return Ok(data);
        }
        [HttpPost("UpdateAddress")]
        public async Task<IActionResult> UpdateAddress([FromBody] AddressModal addressData, int userId)
        {
            var data = await _usersSvc.UpdateAddress(addressData, userId);
            return Ok(data);
        }

        [HttpGet("GetAddressByUserId")]
        public async Task<IActionResult> GetAddressByUserId(int userId)
        {
            var data = await _usersSvc.GetAddressByUserId(userId);
            return Ok(data);
        }

        [HttpPost("UpdatePassword")]
        public async Task<IActionResult> UpdatePassword([FromBody] UpdatePassword password)
        {
            var data = await _usersSvc.UpdatePassword(password);
            return Ok(data);
        }
        
        [HttpGet("SendInfo")]
        public async Task<IActionResult> SendInfo(int userId)
        {
            var data = await _usersSvc.sendPersonalInfo(userId);
            return Ok(data);
        }

        [HttpGet("SendResetPassword")]
        public async Task<IActionResult> SendResetPassword(int userId)
        {
            var data = await _usersSvc.SendResetPassword(userId);
            return Ok(data);
        }
        
        [HttpGet("MakeUser")]
        public async Task<IActionResult> MakeUser(int userId)
        {
            var data = await _usersSvc.MakeUser(userId);
            return Ok(data);
        }
        [HttpGet("MakeAdmin")]
        public async Task<IActionResult> MakeAdmin( int userId)
        {
            var data = await _usersSvc.MakeAdmin(userId);
            return Ok(data);
        }
    }
}
