using LibraryManagement.API.Helper;
using LibraryManagement.API.Modal;

namespace LibraryManagement.API.Container.Service
{
    public interface IAuthorize
    {
        Task<APIResponse<TokenResponse>> GenerateToken(UserCredentails usercred);
        Task<string> GenerateRefreshToken(string username);
        Task<APIResponse<TokenResponse>> GenerateRefreshToken(TokenResponse tokenResponse);
    }
}
