using LibraryManagement.API.Helper;
using LibraryManagement.API.Modal;

namespace LibraryManagement.API.Container.Service
{
    public interface IBooksService
    {
        Task<APIResponse<List<BookModal>>> GetAll();
        Task<APIResponse<BookUpdateModal>> Create(BookUpdateModal book);
        Task<APIResponse<BookModal>> GetById(int BookId);
        Task<APIResponse<List<BookModal>>> GetBooksByIds(int[] bookIds);
        Task<APIResponse> Remove(int BookId);
        Task<APIResponse> Update(BookUpdateModal book, int id);
        Task<APIResponse> AddToCart(int bookId, int userId);
        Task<APIResponse> RemoveFromCart(int bookId, int userId);
        Task<APIResponse<List<BookModal>>> GetCartItemsByUserId(int userId);
    }


}
