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

        private void DeleteBookButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = GetSelectedBook();
            if (selectedBook == null)
                return;

            if (!ConfirmDeletion(selectedBook))
                return;

            if (!TryDeleteBookFromDatabase(selectedBook))
                return;

            RemoveBookFromCache(selectedBook);
            RefreshBooksGridAfterDelete();
        }


        private Book? GetSelectedBook()
        {
            if (BooksGrid.SelectedItem is Book book)
                return book;

            MessageBox.Show("Пожалуйста, выберите книгу для удаления.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        private bool ConfirmDeletion(Book book)
        {
            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить книгу \"{book.Title}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            return result == MessageBoxResult.Yes;
        }

        private bool TryDeleteBookFromDatabase(Book book)
        {
            try
            {
                var bookInDb = _dbContext.Books
                    .Include(b => b.Author)
                    .FirstOrDefault(b => b.BookID == book.BookID);

                if (bookInDb == null)
                {
                    MessageBox.Show("Книга не найдена в базе данных.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }

                _dbContext.Books.Remove(bookInDb);
                _dbContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void RemoveBookFromCache(Book book)
        {
            _cachedBooks.Remove(book);
        }

        private void RefreshBooksGridAfterDelete()
        {
            BooksGrid.ItemsSource = null;
            BooksGrid.ItemsSource = _cachedBooks;
            StatusText.Text = $"Книга удалена. Осталось: {_cachedBooks.Count}";
            DbStatusText.Text = "БД: удаление успешно";
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
