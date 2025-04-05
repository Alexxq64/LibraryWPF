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
        private List<Book> _cachedBooks = new List<Book>(); // Не-static, чтобы избежать проблем

        public MainWindow()
        {
            InitializeComponent();
            LoadBooks(); // Загрузка при старте
        }

        private void LoadBooks()
        {
            try
            {
                StatusText.Text = "Загрузка книг...";
                DbStatusText.Text = "БД: подключение";

                // Если кэш не пуст — используем его (для отладки)
                if (_cachedBooks.Any())
                {
                    BooksGrid.ItemsSource = _cachedBooks;
                    StatusText.Text = $"Загружено из кэша: {_cachedBooks.Count} книг";
                    DbStatusText.Text = "БД: кэш";
                    return;
                }

                // Подключение к БД
                var options = new DbContextOptionsBuilder<LibraryDBContext>()
                    .UseSqlServer("Server=.;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;")
                    .Options;

                using (var db = new LibraryDBContext(options))
                {
                    DbStatusText.Text = "БД: запрос данных";
                    var books = db.Books
                        .Include(b => b.Author)
                        .OrderBy(b => b.Title)
                        .ToList();

                    _cachedBooks = books; // Сохраняем в кэш
                    BooksGrid.ItemsSource = books;
                    StatusText.Text = $"Загружено {books.Count} книг";
                    DbStatusText.Text = "БД: успешно";
                }
            }
            catch (Exception ex)
            {
                // Если есть кэш — показываем его
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
        }

        // Обработчик кнопки (оставляем как было)
        private void ShowBooksButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBooks();
        }
        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            // Инициализируем кэш, если он null
            if (_cachedBooks == null)
            {
                _cachedBooks = new List<Book>();
            }

            var addBookWindow = new AddBookWindow(_cachedBooks);
            addBookWindow.ShowDialog(); // Убрали проверку DialogResult

            // Обновляем таблицу (изменено только это):
            var tempList = new List<Book>(_cachedBooks); // Создаем копию
            BooksGrid.ItemsSource = null;
            BooksGrid.ItemsSource = tempList; // Привязываем копию

            StatusText.Text = $"Добавлена новая книга. Всего: {_cachedBooks.Count}";
        }
    }
}