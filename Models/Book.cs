﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace LibraryWPF.Models;

[Index("ISBN", Name = "IX_Books_ISBN")]
[Index("Title", Name = "IX_Books_Title")]
[Index("ISBN", Name = "UQ_Books_ISBN", IsUnique = true)]
public partial class Book
{
    [Key]
    public int BookID { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; }

    public string Description { get; set; }

    [Required]
    [StringLength(20)]
    public string ISBN { get; set; }

    public int? PublicationYear { get; set; }

    [Column(TypeName = "decimal(3, 2)")]
    public decimal? AverageRating { get; set; }

    public bool? IsFree { get; set; }

    public int? TotalPages { get; set; }

    public int? AuthorID { get; set; }

    public int? SeriesID { get; set; }

    [ForeignKey("AuthorID")]
    [InverseProperty("Books")]
    public virtual Author Author { get; set; }

    [InverseProperty("Book")]
    public virtual ICollection<ReadingHistory> ReadingHistories { get; set; } = new List<ReadingHistory>();

    [InverseProperty("Book")]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    [ForeignKey("SeriesID")]
    [InverseProperty("Books")]
    public virtual BookSeries Series { get; set; }

    [InverseProperty("Book")]
    public virtual ICollection<UserFavorite> UserFavorites { get; set; } = new List<UserFavorite>();

    [ForeignKey("BookID")]
    [InverseProperty("BooksNavigation")]
    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();

    [ForeignKey("BookID")]
    [InverseProperty("Books")]
    public virtual ICollection<Genre> Genres { get; set; } = new List<Genre>();
    public string Text { get; set; } // просто добавляем в модель
}