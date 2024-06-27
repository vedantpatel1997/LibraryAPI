using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Repos.Models;

[PrimaryKey("CartId", "BookId", "UserId")]
[Table("Cart")]
public partial class Cart
{
    [Key]
    public int CartId { get; set; }

    [Key]
    public int BookId { get; set; }

    [Key]
    public int UserId { get; set; }

    [ForeignKey("BookId")]
    [InverseProperty("Carts")]
    public virtual Book Book { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("Carts")]
    public virtual User User { get; set; } = null!;
}
