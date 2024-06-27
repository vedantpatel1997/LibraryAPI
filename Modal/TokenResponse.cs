namespace LibraryManagement.API.Modal
{
    public class TokenResponse
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public UserModal? userData { get; set; }
    }
}
