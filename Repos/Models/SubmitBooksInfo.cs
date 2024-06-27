using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Repos.Models;

[Table("SubmitBooksInfo")]
public partial class SubmitBooksInfo
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("bookId")]
    public int BookId { get; set; }

    [Column("bookTitle")]
    public string BookTitle { get; set; } = null!;

    [Column("userId")]
    public int UserId { get; set; }

    [Column("issueDate")]
    public DateTime IssueDate { get; set; }

    [Column("returnDate")]
    public DateTime ReturnDate { get; set; }

    [Column("days")]
    public int Days { get; set; }
}
