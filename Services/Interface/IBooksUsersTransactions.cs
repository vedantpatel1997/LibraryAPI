using LibraryManagement.API.Helper;
using LibraryManagement.API.Modal;
using LibraryManagement.API.Repos.Models;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Container.Service
{
    public interface IBooksUsersTransactions
    {
        Task<APIResponse<List<IssueDTO>>> GetBooksByUserId(int userId);
        Task<APIResponse<List<IssueDTO>>> GetDueBooks();
        Task<APIResponse<List<IssueDTO>>> GetUsersByBookId(int bookId);
        Task<APIResponse<List<SubmitBooksInfo>>> GetBooksHistoryByUserId(int bookId);
        Task<APIResponse<List<HistoryModal>>> GetUsersHistoryByBookId(int bookId);
        Task<APIResponse> IssueBook(IssueDTO issueDTO);
        Task<APIResponse> IssueBooks(BillingDetails billingDetails);
        Task<APIResponse> SubmitBook(SubmitDTO SubmitDTO);
        Task<APIResponse> SendReminderForPendingBooks(int userId);
        Task<APIResponse> SendReminderForPendingBook(int userId, int bookId);
        Task<APIResponse> SendTemp(int userId);
        Task<APIResponse> GenerateBill(BillingSummaryModal billingSummary, List<BillingBooksInfoModal> booksInfo);
        Task<APIResponse<List<BillingSummaryModal>>> GetBillsByUserID(int userId);
        Task<APIResponse<BillingSummaryModal>> GetBillByBillID(int billId);
        Task<APIResponse> SendPDF(IFormFile pdf, int userId);

    }
}
