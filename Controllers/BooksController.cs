using LibraryManagement.API.Container.Service;
using LibraryManagement.API.Helper;
using LibraryManagement.API.Modal;
using LibraryManagement.API.Repos.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace LibraryManagement.API.Controllers
{
    [EnableCors]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class BooksController : ControllerBase
    {
        private readonly IBooksService _booksSvc;

        public BooksController(IBooksService booksSvc)
        {
            this._booksSvc = booksSvc;
        }
        [AllowAnonymous]
        [HttpGet("GetAllBooks")]
        public async Task<IActionResult> GetAllBooks()
        {
            var data = await _booksSvc.GetAll();
            return Ok(data);
        }

        [AllowAnonymous]
        [HttpGet("GetDate")]
        public async Task<IActionResult> GetDate()
        {
            //var curUTCTIME = DateTime.UtcNow;
            //var utcDateTime = curUTCTIME.AddDays(5);

            //// Convert to India Standard Time (IST)
            //TimeZoneInfo indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            //DateTime indiaDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, indiaTimeZone);

            //// Convert to Canada EST time (Eastern Time)
            //TimeZoneInfo canadaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York"); // or "Eastern Daylight Time" when daylight saving time is in effect
            //DateTime canadaDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, canadaTimeZone);


            //var dates = new
            //{
            //    CurrentUTCTIME = curUTCTIME,
            //    IndiaTime = indiaDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
            //    CanadaEST = canadaDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
            //};
            //TimeZoneInfo easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            //// Check whether daylight saving time is currently in effect
            //bool isDaylightSavingTime = easternTimeZone.IsDaylightSavingTime(DateTime.UtcNow.AddMonths(5));

            //if (isDaylightSavingTime)
            //{
            //    Console.WriteLine("It is currently Eastern Daylight Time (EDT).");
            //}
            //else
            //{
            //    Console.WriteLine("It is currently Eastern Standard Time (EST).");
            //}

            //return Ok(dates);


            string utcDateTimeString = "2023-12-01 09:46:25.6436863";
            DateTime utcDateTime = DateTime.Parse(utcDateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

            string userTimeZoneId = "America/New_York"; // Replace with the actual user's time zone

            DateTimeOffset userDateTimeOffset = TimeZoneConverter.ConvertUtcToTimeZone(utcDateTime, userTimeZoneId);
            string formattedUserDateTime = userDateTimeOffset.ToString("f");

            return Ok(userDateTimeOffset);
        }


        [HttpGet("GetBookById")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var data = await _booksSvc.GetById(id);
            return Ok(data);
        }

        [HttpPost("GetBooksByIds")]
        public async Task<IActionResult> GetBooksByIds(int[] id)
        {
            var data = await _booksSvc.GetBooksByIds(id);
            return Ok(data);
        }


        [HttpPost("Create")]
        public async Task<IActionResult> create([FromBody] BookUpdateModal book)
        {
            var data = await _booksSvc.Create(book);
            return Ok(data);
        }


        [HttpPost("Update")]
        public async Task<IActionResult> Update(BookUpdateModal book, int id)
        {
            var data = await _booksSvc.Update(book, id);
            return Ok(data);
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _booksSvc.Remove(id);
            return Ok(data);
        }

        [HttpGet("GetCartItemsByUserId")]
        public async Task<IActionResult> GetCartItemsByUserId(int userId)
        {
            var data = await _booksSvc.GetCartItemsByUserId(userId);
            return Ok(data);
        }




        [HttpPost("AddToCart")]
        public async Task<IActionResult> AddToCart(CartModal cart)
        {
            var data = await _booksSvc.AddToCart(cart.BookId, cart.UserId);
            return Ok(data);
        }

        [HttpPost("RemoveFromCart")]
        public async Task<IActionResult> RemoveFromCart(CartModal cart)
        {
            var data = await _booksSvc.RemoveFromCart(cart.BookId, cart.UserId);
            return Ok(data);
        }
    }
}

