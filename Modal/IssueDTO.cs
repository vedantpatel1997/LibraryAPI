namespace LibraryManagement.API.Modal
{
    public class IssueDTO
    {
        public int BookId { get; set; }
        public int UserId { get; set; }
        public int Days { get; set; }
        public DateTime? IssueDate { get; set; }
        public BookModal? Book { get; set; }
        public UserModal? User { get; set; }
    }


}
