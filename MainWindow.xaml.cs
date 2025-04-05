using LibraryWPF.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace LibraryWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadBooks(); // Автоматическая загрузка при старте
        }

        private void LoadBooks()
        {
            try
            {
                StatusText.Text = "Загрузка книг...";
                DbStatusText.Text = "БД: подключение";

                var optionsBuilder = new DbContextOptionsBuilder<LibraryDBContext>();
                optionsBuilder.UseSqlServer(
                    "Server=.;Database=LibraryDB;Trusted_Connection=True;" +
                    "TrustServerCertificate=True;");

                using (var db = new LibraryDBContext(optionsBuilder.Options))
                {
                    DbStatusText.Text = "БД: запрос данных";
                    var books = db.Books
                        .Include(b => b.Author)
                        .OrderBy(b => b.Title)
                        .ToList();

                    BooksGrid.ItemsSource = books;
                    StatusText.Text = $"Загружено {books.Count} книг";
                    DbStatusText.Text = "БД: успешно";
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = "Ошибка загрузки";
                DbStatusText.Text = "БД: ошибка";
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ShowBooksButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBooks(); // Обновление списка по кнопке
        }
    }
}