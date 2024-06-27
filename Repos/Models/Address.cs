using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Repos.Models;

[Table("Address")]
public partial class Address
{
    [Key]
    public int AddressId { get; set; }

    [StringLength(50)]
    public string City { get; set; } = null!;

    [StringLength(50)]
    public string Province { get; set; } = null!;

    [StringLength(50)]
    public string Country { get; set; } = null!;

    [Column("POSTALCODE")]
    [StringLength(10)]
    public string Postalcode { get; set; } = null!;

    public string AddressLine1 { get; set; } = null!;

    public string? AddressLine2 { get; set; }

    [JsonIgnore]
    [InverseProperty("Address")]
    public virtual ICollection<BillingSummary> BillingSummaries { get; set; } = new List<BillingSummary>();

    [JsonIgnore]
    [InverseProperty("Address")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
