using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Repos.Models;

[Table("BillingSummary")]
public partial class BillingSummary
{
    [Key]
    [Column("billingId")]
    public int BillingId { get; set; }

    [Column("userId")]
    public int? UserId { get; set; }

    [Column("userFirstName")]
    [StringLength(100)]
    public string UserFirstName { get; set; } = null!;

    [Column("userLastName")]
    [StringLength(100)]
    public string UserLastName { get; set; } = null!;

    [Column("userEmail")]
    [StringLength(100)]
    public string UserEmail { get; set; } = null!;

    [Column("userPhone")]
    [StringLength(10)]
    public string UserPhone { get; set; } = null!;

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("bookQuantity")]
    public int BookQuantity { get; set; }

    [Column("delivery")]
    public bool? Delivery { get; set; }

    [Column("addressId")]
    public int AddressId { get; set; }

    [Column("pickup")]
    public bool? Pickup { get; set; }

    [Column("tax", TypeName = "decimal(10, 2)")]
    public decimal Tax { get; set; }

    [Column("totalAmount", TypeName = "decimal(10, 2)")]
    public decimal TotalAmount { get; set; }

    [ForeignKey("AddressId")]
    [InverseProperty("BillingSummaries")]
    public virtual Address Address { get; set; } = null!;

    [InverseProperty("Billing")]
    public virtual ICollection<BillingBooksInfo> BillingBooksInfos { get; set; } = new List<BillingBooksInfo>();
}
