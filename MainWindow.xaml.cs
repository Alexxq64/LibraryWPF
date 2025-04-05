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
        private readonly LibraryDBContext _dbContext; // Единый контекст для всего окна

        public MainWindow()
        {
            // Инициализация подключения к БД
            var optionsBuilder = new DbContextOptionsBuilder<LibraryDBContext>();
            optionsBuilder.UseSqlServer("Server=.;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;");
            _dbContext = new LibraryDBContext(optionsBuilder.Options);

            InitializeComponent();
            LoadBooks(); // Первоначальная загрузка данных
        }

        private void LoadBooks()
        {
            try
            {
                StatusText.Text = "Загрузка книг...";
                DbStatusText.Text = "БД: подключение";

                // Если есть данные в кэше - используем их
                if (_cachedBooks.Any())
                {
                    BooksGrid.ItemsSource = _cachedBooks;
                    StatusText.Text = $"Загружено из кэша: {_cachedBooks.Count} книг";
                    DbStatusText.Text = "БД: кэш";
                    return;
                }

                // Загрузка из БД
                var books = _dbContext.Books
                    .Include(b => b.Author)
                    .OrderBy(b => b.Title)
                    .ToList();

                _cachedBooks = books;
                BooksGrid.ItemsSource = books;
                StatusText.Text = $"Загружено {books.Count} книг";
                DbStatusText.Text = "БД: успешно";
            }
            catch (Exception ex)
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
        }

        private void ShowBooksButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBooks(); // Повторная загрузка по кнопке
        }

        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            var addBookWindow = new AddBookWindow(_cachedBooks, _dbContext);
            if (addBookWindow.ShowDialog() == true)
            {
                // Обновление отображения после добавления
                BooksGrid.ItemsSource = null;
                BooksGrid.ItemsSource = _cachedBooks;
                StatusText.Text = $"Добавлена новая книга. Всего: {_cachedBooks.Count}";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _dbContext?.Dispose(); // Освобождение ресурсов
            base.OnClosed(e);
        }
    }
}