using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Repos.Models;

[Table("BillingBooksInfo")]
public partial class BillingBooksInfo
{
    [Key]
    public int BillingBookInfoId { get; set; }

    [Column("bookId")]
    public int? BookId { get; set; }

    [Column("bookName")]
    [StringLength(100)]
    public string BookName { get; set; } = null!;

    [Column("rentDays")]
    public int RentDays { get; set; }

    [Column("estimatedReturnDate")]
    public DateTime EstimatedReturnDate { get; set; }

    [Column("bookAuthor")]
    [StringLength(50)]
    public string BookAuthor { get; set; } = null!;

    [Column("bookCategory")]
    [StringLength(50)]
    public string BookCategory { get; set; } = null!;

    [Column("bookOriginalPrice", TypeName = "decimal(10, 2)")]
    public decimal BookOriginalPrice { get; set; }

    [Column("bookRentPrice", TypeName = "decimal(10, 2)")]
    public decimal BookRentPrice { get; set; }

    [Column("bookImageURL")]
    public string BookImageUrl { get; set; } = null!;

    [Column("billingId")]
    public int BillingId { get; set; }

    [ForeignKey("BillingId")]
    [InverseProperty("BillingBooksInfos")]
    public virtual BillingSummary Billing { get; set; } = null!;
}
