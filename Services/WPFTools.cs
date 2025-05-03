using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using LibraryWPF.Models;

namespace LibraryWPF.Services
{
    internal static class WPFTools
    {
        public static List<Book> GetBooks(LibraryDBContext dbContext, string listType, User user = null)
        {
            if (listType == "Library")
            {
                return dbContext.Books
                    .Include(b => b.Authors)
                    .OrderBy(b => b.Title)
                    .AsNoTracking()
                    .ToList();
            }
            else if (listType == "MyBooks" && user != null)
            {
                return dbContext.ReadingHistories
                    .Include(r => r.Book)
                        .ThenInclude(b => b.Authors)
                    .Where(r => r.UserID == user.UserID)
                    .OrderByDescending(r => r.LastReadDate ?? r.StartDate)
                    .Select(r => r.Book)
                    .Distinct()
                    .ToList();
            }

            return new List<Book>(); // пустой список на случай ошибок
        }
    }
}

