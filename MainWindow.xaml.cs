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
        private LibraryDBContext _dbContext;
        private const string DefaultDbName = "LibraryDB";
        private string _currentDbName = DefaultDbName;
        private User _currentUser;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                InitializeApplication();
            }
            catch (Exception ex)
            {
                HandleCriticalError(ex);
            }
        }

        private void InitializeApplication()
        {
            // 1. Выбор базы данных
            ShowDatabaseSelectionWindow(); // включает создание и подключение к БД

            // 2. Авторизация/регистрация
            var logRegWindow = new LogRegWindow(_dbContext);
            if (logRegWindow.ShowDialog() != true)
            {
                Environment.Exit(0);
                return;
            }

            _currentUser = logRegWindow.LoggedInUser;

            // 3. Проверка прав и переключение на AdminWindow при необходимости
            if (_currentUser.IsAdmin)
            {
                var adminWindow = new AdminWindow(_dbContext);
                adminWindow.Show();
                Close(); // закрываем MainWindow, если админ
                return;
            }

            // 4. Загрузка данных для обычного пользователя
            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    LoadBooks();
                    BooksGrid.MouseDoubleClick += BooksGrid_MouseDoubleClick;
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"Ошибка загрузки данных: {ex.Message}");
                }
            }));
        }



        private void InitializeDefaultDatabase()
        {
            try
            {
                _dbContext = CreateDbContext(DefaultDbName);

                if (!_dbContext.Database.CanConnect())
                {
                    _dbContext.Database.EnsureCreated();
                    AddInitialData();
                    UpdateStatus($"База {DefaultDbName} создана автоматически", "БД: создана");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка инициализации БД: {ex.Message}", ex);
            }
        }

        private void ShowDatabaseSelectionWindow()
        {
            var chooseDbWindow = new ChooseDbWindow();
            if (chooseDbWindow.ShowDialog() != true) return;

            try
            {
                if (chooseDbWindow.IsCreateNewDb)
                {
                    CreateDatabase(chooseDbWindow.NewDbName);
                }
                else
                {
                    ConnectToExistingDatabase(chooseDbWindow.SelectedDbName);
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка: {ex.Message}\nИспользуется {DefaultDbName}");
                ConnectToExistingDatabase(DefaultDbName);
            }
        }

        private void ConnectToExistingDatabase(string dbName)
        {
            _currentDbName = dbName ?? DefaultDbName;
            _dbContext = CreateDbContext(_currentDbName);

            if (!_dbContext.Database.CanConnect())
                throw new Exception($"Не удалось подключиться к базе {_currentDbName}");

            UpdateStatus($"Подключено к {_currentDbName}", "БД: подключено");
        }

        private void CreateDatabase(string dbName)
        {
            try
            {
                _dbContext = CreateDbContext(dbName);
                _dbContext.Database.EnsureCreated();
                AddInitialData();
                _currentDbName = dbName;
                UpdateStatus($"База {dbName} создана", "БД: создана");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания БД: {ex.Message}", ex);
            }
        }

        private LibraryDBContext CreateDbContext(string dbName)
        {
            var optionsBuilder = new DbContextOptionsBuilder<LibraryDBContext>();
            optionsBuilder.UseSqlServer(GetConnectionString(dbName));
            return new LibraryDBContext(optionsBuilder.Options);
        }

        private string GetConnectionString(string dbName)
        {
            return $"Server=.;Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;";
        }

        private void LoadBooks()
        {
            try
            {
                SetLoadingStatus("Загрузка книг...", "БД: подключение");

                if (TryLoadFromCache()) return;

                var books = _dbContext.Books
                    .Include(b => b.Author)
                    .OrderBy(b => b.Title)
                    .AsNoTracking()
                    .ToList();

                _cachedBooks = books;
                BooksGrid.ItemsSource = books;
                UpdateStatus($"Загружено {books.Count} книг", $"БД: {_currentDbName}");
            }
            catch (Exception ex)
            {
                HandleDataLoadingError(ex);
            }
        }

        private bool TryLoadFromCache()
        {
            if (!_cachedBooks.Any()) return false;

            BooksGrid.ItemsSource = _cachedBooks;
            UpdateStatus($"Загружено из кэша: {_cachedBooks.Count} книг", "БД: кэш");
            return true;
        }

        private void HandleDataLoadingError(Exception ex)
        {
            if (_cachedBooks.Any())
            {
                BooksGrid.ItemsSource = _cachedBooks;
                UpdateStatus($"Ошибка БД, но есть кэш: {_cachedBooks.Count} книг", "БД: ошибка (кэш)");
            }
            else
            {
                UpdateStatus("Ошибка загрузки", "БД: ошибка");
            }
            ShowErrorMessage($"Ошибка загрузки данных: {ex.Message}");
        }

        private void AddInitialData()
        {
            if (_dbContext.Books.Any()) return;

            try
            {
                var authors = new[]
                {
                    new Author { FirstName = "Джордж", LastName = "Мартин" },
                    new Author { FirstName = "Дж. К.", LastName = "Роулинг" }
                };

                var books = new[]
                {
                    new Book { Title = "Игра престолов", ISBN = "1234567890", Author = authors[0] },
                    new Book { Title = "Гарри Поттер и философский камень", ISBN = "0987654321", Author = authors[1] }
                };

                _dbContext.Authors.AddRange(authors);
                _dbContext.Books.AddRange(books);
                _dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка добавления тестовых данных: {ex.Message}", ex);
            }
        }

        private void SetLoadingStatus(string status, string dbStatus)
        {
            StatusText.Text = status;
            DbStatusText.Text = dbStatus;
        }

        private void UpdateStatus(string status, string dbStatus)
        {
            StatusText.Text = status;
            DbStatusText.Text = dbStatus;
        }

        private void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void HandleCriticalError(Exception ex)
        {
            MessageBox.Show($"Критическая ошибка приложения: {ex.Message}\nПриложение будет закрыто.",
                "Фатальная ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(1);
        }

        private void ShowBooksButton_Click(object sender, RoutedEventArgs e)
        {
            LoadBooks();
        }

        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new EditBookWindow(_dbContext, null);
                if (window.ShowDialog() == true)
                {
                    LoadBooks(); // Перезагрузка с базы
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при добавлении книги: {ex.Message}");
            }
        }


        private void DeleteBookButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedBook = BooksGrid.SelectedItem as Book;
                if (selectedBook == null)
                {
                    ShowErrorMessage("Пожалуйста, выберите книгу для удаления");
                    return;
                }

                if (MessageBox.Show($"Вы уверены, что хотите удалить книгу \"{selectedBook.Title}\"?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                var bookInDb = _dbContext.Books
                    .Include(b => b.Author)
                    .FirstOrDefault(b => b.BookID == selectedBook.BookID);

                if (bookInDb == null)
                {
                    ShowErrorMessage("Книга не найдена в базе данных");
                    return;
                }

                _dbContext.Books.Remove(bookInDb);
                _dbContext.SaveChanges();

                _cachedBooks.Remove(selectedBook);
                RefreshBooksGrid();
                UpdateStatus($"Книга удалена. Осталось: {_cachedBooks.Count}", "БД: обновлено");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка при удалении книги: {ex.Message}");
            }
        }

        private void RefreshBooksGrid()
        {
            LoadBooks();
        }


        protected override void OnClosed(EventArgs e)
        {
            _dbContext?.Dispose();
            base.OnClosed(e);
        }

        private void BooksGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var selectedBook = BooksGrid.SelectedItem as Book;
            if (selectedBook == null) return;

            if (string.IsNullOrWhiteSpace(selectedBook.Text))
            {
                MessageBox.Show("У этой книги пока нет текста.", "Нет содержимого", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var window = new BookTextWindow(selectedBook.Text);
            window.ShowDialog();
        }


    }
}