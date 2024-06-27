namespace LibraryManagement.API.Modal
{
    public class BookUpdateModal
    {
        public int? BookId { get; set; }

        public string Title { get; set; } = null!;

        public string Author { get; set; } = null!;
        public string ImageURL { get; set; } = null!;

        public int TotalQuantity { get; set; }

        public int? AvailableQuantity { get; set; }

        public int? IssuedQuantity { get; set; }

        public decimal Price { get; set; }
        public int CategoryId { get; set; }
    }
}
