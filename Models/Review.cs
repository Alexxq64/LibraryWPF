﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryWPF.Models;

[Index("BookID", "UserID", Name = "UQ_UserBookReview", IsUnique = true)]
public partial class Review
{
    [Key]
    public int ReviewID { get; set; }

    public int BookID { get; set; }

    public int UserID { get; set; }

    public byte Rating { get; set; }

    [StringLength(1000)]
    public string Comment { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("BookID")]
    [InverseProperty("Reviews")]
    public virtual Book Book { get; set; }

    [ForeignKey("UserID")]
    [InverseProperty("Reviews")]
    public virtual User User { get; set; }
}