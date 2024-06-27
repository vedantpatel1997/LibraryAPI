using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.API.Modal
{
    public class CartModal
    {
        public int BookId { get; set; }

        public int UserId { get; set; }
    }
}
