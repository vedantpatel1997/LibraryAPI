using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Repos.Models;

public partial class Book
{
    [Key]
    public int BookId { get; set; }

    public string Title { get; set; } = null!;

    public string Author { get; set; } = null!;

    public int TotalQuantity { get; set; }

    public int AvailableQuantity { get; set; }

    public int IssuedQuantity { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal Price { get; set; }

    [Column("ImageURL")]
    public string ImageUrl { get; set; } = null!;

    public int CategoryId { get; set; }

    [InverseProperty("Book")]
    public virtual ICollection<BookIssue> BookIssues { get; set; } = new List<BookIssue>();

    [InverseProperty("Book")]
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    [ForeignKey("CategoryId")]
    [InverseProperty("Books")]
    public virtual Category Category { get; set; } = null!;
}
