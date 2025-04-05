using System.Windows;
using LibraryWPF.Models;

namespace LibraryWPF
{
    public partial class AddBookWindow : Window
    {
        private readonly List<Book> _cache;
        private readonly LibraryDBContext _dbContext;

        public AddBookWindow(List<Book> cache, LibraryDBContext dbContext)
        {
            InitializeComponent();
            _cache = cache;
            _dbContext = dbContext;
            TitleTextBox.Text = "Игра престолов";
            AuthorTextBox.Text = "Джордж Мартин";
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // Генерация ISBN (пример: "978-3-16-148410-0")
            string GenerateISBN() => $"{new Random().Next(100, 999)}-{new Random().Next(0, 9)}-{new Random().Next(10, 99)}-{new Random().Next(100000, 999999)}-{new Random().Next(0, 9)}";

            var newBook = new Book
            {
                Title = TitleTextBox.Text.Trim(),
                ISBN = GenerateISBN(), // Заполняем обязательное поле
                Author = new Author
                {
                    FirstName = "Не указано",
                    LastName = AuthorTextBox.Text.Trim()
                }
            };

            try
            {
                _dbContext.Books.Add(newBook);
                _dbContext.SaveChanges();

                _cache.Add(newBook);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
    }
}