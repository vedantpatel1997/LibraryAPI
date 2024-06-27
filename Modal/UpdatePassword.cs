namespace LibraryManagement.API.Modal
{
    public class UpdatePassword
    {
        public string oldPassword { get; set; }
        public string newPassword { get; set; }
        public int userId { get; set; }
    }
}
