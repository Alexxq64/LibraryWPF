using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using LibraryWPF.Models;

namespace LibraryWPF
{
    public partial class MainWindow : Window
    {
        private List<Book> _cachedBooks = new List<Book>();
        private readonly LibraryDBContext _dbContext;

        public MainWindow()
        {
            _dbContext = CreateDbContext();
            InitializeComponent();
            LoadBooks();
        }

        // 📌 Инициализация контекста БД
        private LibraryDBContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<LibraryDBContext>();
            optionsBuilder.UseSqlServer("Server=.;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;");
            return new LibraryDBContext(optionsBuilder.Options);
        }

        // 📥 Загрузка книг (из кэша или БД)
        private void LoadBooks()
        {
            try
            {
                ShowLoadingStatus();

                if (TryLoadFromCache())
                    return;

                LoadFromDatabase();
            }
            catch (Exception ex)
            {
                HandleLoadingError(ex);
            }
        }

        // ⏳ Отображение статуса загрузки
        private void ShowLoadingStatus()
        {
            StatusText.Text = "Загрузка книг...";
            DbStatusText.Text = "БД: подключение";
        }

        // 💾 Попытка загрузки из кэша
        private bool TryLoadFromCache()
        {
            if (_cachedBooks.Any())
            {
                BooksGrid.ItemsSource = _cachedBooks;
                StatusText.Text = $"Загружено из кэша: {_cachedBooks.Count} книг";
                DbStatusText.Text = "БД: кэш";
                return true;
            }
            return false;
        }

        // 🗃 Загрузка данных из БД
        private void LoadFromDatabase()
        {
            var books = _dbContext.Books
                .Include(b => b.Author)
                .OrderBy(b => b.Title)
                .ToList();

            _cachedBooks = books;
            BooksGrid.ItemsSource = books;
            StatusText.Text = $"Загружено {books.Count} книг";
            DbStatusText.Text = "БД: успешно";
        }

        // ⚠️ Обработка ошибок загрузки
        private void HandleLoadingError(Exception ex)
        {
            if (_cachedBooks.Any())
            {
                BooksGrid.ItemsSource = _cachedBooks;
                StatusText.Text = $"Ошибка БД, но есть кэш: {_cachedBooks.Count} книг";
                DbStatusText.Text = "БД: ошибка (кэш)";
            }
            else
            {
                StatusText.Text = "Ошибка загрузки";
                DbStatusText.Text = "БД: ошибка";
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 🔘 Обновление книг по кнопке
        private void ShowBooksButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBooks();
        }

        // ➕ Добавление книги
        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            var addBookWindow = new AddBookWindow(_cachedBooks, _dbContext);
            if (addBookWindow.ShowDialog() == true)
            {
                RefreshBooksGridAfterAdd();
            }
        }

        // 🔄 Обновление UI после добавления
        private void RefreshBooksGridAfterAdd()
        {
            BooksGrid.ItemsSource = null;
            BooksGrid.ItemsSource = _cachedBooks;
            StatusText.Text = $"Добавлена новая книга. Всего: {_cachedBooks.Count}";
        }

        // 🧹 Очистка ресурсов при закрытии
        protected override void OnClosed(EventArgs e)
        {
            _dbContext?.Dispose();
            base.OnClosed(e);
        }
    }
}
