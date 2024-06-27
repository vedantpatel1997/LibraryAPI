using LibraryManagement.API.Container.Service;
using LibraryManagement.API.Modal;
using LibraryManagement.API.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers
{
    [EnableCors]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class BooksUsersTransactionsController : ControllerBase
    {
        private readonly IBooksUsersTransactions _bUTransactionSvs;

        public BooksUsersTransactionsController(IBooksUsersTransactions BUTransactionSvs)
        {
            _bUTransactionSvs = BUTransactionSvs;
        }
        [HttpGet("GetDueBooks")]
        public async Task<IActionResult> GetDueBooks(int userId)
        {
            var data = await this._bUTransactionSvs.GetDueBooks();
            return Ok(data);
        }
        [HttpGet("GetBooksByUserID")]
        public async Task<IActionResult> GetBooksByUserID(int userId)
        {
            var data = await this._bUTransactionSvs.GetBooksByUserId(userId);
            return Ok(data);
        }
        [HttpGet("GetUsersByBookID")]
        public async Task<IActionResult> GetUsersByBookID(int bookId)
        {
            var data = await this._bUTransactionSvs.GetUsersByBookId(bookId);
            return Ok(data);
        }
        //[HttpPost("IssueBook")]
        //public async Task<IActionResult> IssueBook(IssueDTO issueDTO)
        //{
        //    var data = await this._bUTransactionSvs.IssueBook(issueDTO);
        //    return Ok(data);
        //}
        [HttpPost("IssueBooks")]
        public async Task<IActionResult> IssueBooks(BillingDetails billingDetails)
        {
            var data = await this._bUTransactionSvs.IssueBooks(billingDetails);
            return Ok(data);
        }
        [HttpPost("SubmitBook")]
        public async Task<IActionResult> SubmitBook(SubmitDTO SubmitDTO)
        {
            var data = await this._bUTransactionSvs.SubmitBook(SubmitDTO);
            return Ok(data);
        }

        [HttpGet("GetBooksHistoryByUserId")]
        public async Task<IActionResult> GetBooksHistoryByUserId(int userId)
        {
            var data = await _bUTransactionSvs.GetBooksHistoryByUserId(userId);
            return Ok(data);
        }
        [HttpGet("GetUsersHistoryByBookId")]
        public async Task<IActionResult> GetUsersHistoryByBookId(int bookId)
        {
            var data = await _bUTransactionSvs.GetUsersHistoryByBookId(bookId);
            return Ok(data);
        }

        [HttpGet("SendReminderForPendingBooks")]
        public async Task<IActionResult> SendReminderForPendingBooks(int userId)
        {
            var data = await _bUTransactionSvs.SendReminderForPendingBooks(userId);
            return Ok(data);
        }

        [HttpGet("SendReminderForPendingBook")]
        public async Task<IActionResult> SendReminderForPendingBook(int userId, int bookId)
        {
            var data = await _bUTransactionSvs.SendReminderForPendingBook(userId, bookId);
            return Ok(data);
        }

        [HttpPost("GenerateBill")]
        public async Task<IActionResult> GenerateBill([FromBody] BillingDetails request)
        {
            var data = await _bUTransactionSvs.GenerateBill(request.BillingSummary, request.BillingBooksInfo);
            return Ok(data);
        }

        [HttpGet("GetBillsByUserID")]
        public async Task<IActionResult> GetBillsByUserID(int userId)
        {
            var data = await _bUTransactionSvs.GetBillsByUserID(userId);
            return Ok(data);
        }

        [HttpGet("GetBillByBillID")]
        public async Task<IActionResult> GetBillByBillID(int billId)
        {
            var data = await _bUTransactionSvs.GetBillByBillID(billId);
            return Ok(data);
        }

        [HttpPost("sendPDF")]
        public async Task<IActionResult> sendPDF([FromForm] IFormFile pdf, [FromForm] int userId)
        {
            var data = await _bUTransactionSvs.SendPDF(pdf, userId);
            return Ok(data);
        }


    }
}
