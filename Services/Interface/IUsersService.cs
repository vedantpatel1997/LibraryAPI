using LibraryManagement.API.Helper;
using LibraryManagement.API.Modal;

namespace LibraryManagement.API.Container.Service
{
    public interface IUsersService
    {
        Task<APIResponse<List<UserModal>>> GetAll();
        Task<APIResponse<UserModal>> Create(UserModal user);
        Task<APIResponse<UserModal>> GetById(int userId);
        Task<APIResponse> Remove(int userId);
        Task<APIResponse> Update(UserModal user, int id);
        Task<APIResponse> UpdatePassword(UpdatePassword password);
        Task<APIResponse<AddressModal>> CreateAddress(AddressModal addressData, int userId);
        Task<APIResponse<AddressModal>> UpdateAddress(AddressModal addressData, int userId);
        Task<APIResponse<AddressModal>> GetAddressByUserId(int userId);
        Task<APIResponse> sendPersonalInfo(int userId);
        Task<APIResponse> SendResetPassword(int userId);
        Task<APIResponse<string>> MakeUser(int userId);
        Task<APIResponse<string>> MakeAdmin(int userId);
    }
}
