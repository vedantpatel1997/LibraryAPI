using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.API.Repos.Models;

[Table("AuthenticationRefreshToken")]
public partial class AuthenticationRefreshToken
{
    [Key]
    [Column("userId")]
    [StringLength(50)]
    public string UserId { get; set; } = null!;

    [Column("tokenId")]
    [StringLength(50)]
    public string TokenId { get; set; } = null!;

    [Column("refreshToken")]
    public string RefreshToken { get; set; } = null!;
}
