using LibraryManagement.API.Helper;
using LibraryManagement.API.Modal;

namespace LibraryManagement.API.Services.Interface
{
    public interface ICategoryService
    {
        Task<APIResponse<List<CategoryModal>>> GetAll();
        Task<APIResponse<CategoryModal>> Create(CategoryModal category);
        Task<APIResponse<CategoryModal>> GetById(int categoryId);
        Task<APIResponse> Remove(int categoryId);
        Task<APIResponse> Update(CategoryModal book, int categoryId);
    }
}
