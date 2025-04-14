using System.Linq;
using System.Windows;
using LibraryWPF.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryWPF
{
    public partial class AdminWindow : Window
    {
        private readonly LibraryDBContext _dbContext;

        public AdminWindow(LibraryDBContext dbContext)
        {
            InitializeComponent();
            _dbContext = dbContext;

            LoadBooks();
            LoadUsers();
        }

        private void LoadBooks()
        {
            var books = _dbContext.Books.Include(b => b.Author).ToList();
            BooksGrid.ItemsSource = books;
        }

        private void LoadUsers()
        {
            var users = _dbContext.Users.ToList();
            UsersGrid.ItemsSource = users;
        }
    }
}
