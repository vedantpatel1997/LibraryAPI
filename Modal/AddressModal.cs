using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.API.Modal
{
    public class AddressModal
    {
        public int? AddressId { get; set; }

        public string City { get; set; } = null!;

        public string Province { get; set; } = null!;

        public string Country { get; set; } = null!;

        public string Postalcode { get; set; } = null!;

        public string AddressLine1 { get; set; } = null!;

        public string? AddressLine2 { get; set; }
    }
}
