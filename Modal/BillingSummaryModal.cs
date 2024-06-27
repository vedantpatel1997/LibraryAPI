using LibraryManagement.API.Repos.Models;

namespace LibraryManagement.API.Modal
{
    public class BillingSummaryModal
    {

        public int BillingId { get; set; }
        public int UserId { get; set; }
        public string? UserFirstName { get; set; }
        public string? UserLastName { get; set; }

        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public DateTime? Date { get; set; }
        public int BookQuantity { get; set; }

        public bool? Delivery { get; set; }

        public int? AddressId { get; set; }
        public bool? Pickup { get; set; }

        public decimal Tax { get; set; }

        public decimal TotalAmount { get; set; }
        public AddressModal? Address { get; set; }
        public  ICollection<BillingBooksInfoModal>? BillingBooksInfos { get; set; } 


    }
}
