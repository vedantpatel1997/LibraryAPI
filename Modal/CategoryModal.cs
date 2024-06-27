using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryManagement.API.Modal
{
    public class CategoryModal
    {
        public int CategoryId { get; set; }

        public string Name { get; set; }
        public string? ImageURL { get; set; }
    }
}
