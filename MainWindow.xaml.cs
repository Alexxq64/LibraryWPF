using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using LibraryWPF.Models;
using LibraryWPF.Services;

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
            ShowDatabaseSelectionWindow();

            //var logRegWindow = new LogRegWindow(_dbContext);
            var logRegWindow = new LogRegWindow();
            if (logRegWindow.ShowDialog() != true)
            {
                Environment.Exit(0);
                return;
            }

            _currentUser = logRegWindow.LoggedInUser;

            if (_currentUser.IsAdmin)
            {
                var adminWindow = new AdminWindow(); // Используется конструктор без параметров
                //var adminWindow = new AdminWindow(_dbContext);
                adminWindow.Show();
                Close();
                return;
            }

            Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    LoadBooks();
                    LoadMyBooks();
                    BooksGrid.MouseDoubleClick += BooksGrid_MouseDoubleClick;
                }
                catch (Exception ex)
                {
                    ShowErrorMessage($"Ошибка загрузки данных: {ex.Message}");
                }
            }));
        }

        private void ShowDatabaseSelectionWindow()
        {
            var chooseDbWindow = new ChooseDbWindow();
            if (chooseDbWindow.ShowDialog() != true) return;

            try
            {
                //if (DatabaseSettings.Instance.IsCreateNewDb)
                if (DBTools.IsCreateNewDb)
                {
                    CreateDatabase();
                }
                else
                {
                    ConnectToExistingDatabase();
                }

            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка: {ex.Message}\nИспользуется {DefaultDbName}");
                ConnectToExistingDatabase();
            }
        }

        private void ConnectToExistingDatabase()
        {
            //_currentDbName = DatabaseSettings.Instance.SelectedDbName;
            //_dbContext = CreateDbContext();
            _dbContext = new LibraryDBContext();

            if (!_dbContext.Database.CanConnect())
                throw new Exception($"Не удалось подключиться к базе {DBTools.DBName}");

            UpdateStatus($"Подключено к {DBTools.DBName}", "БД: подключено");
        }


        private void CreateDatabase()
        {
            try
            {
                //var dbName = DatabaseSettings.Instance.SelectedDbName;
                //_dbContext = CreateDbContext();
                _dbContext = new LibraryDBContext();
                _dbContext.Database.EnsureCreated();
                AddInitialData();
                //_currentDbName = DBTools.DBName;
                UpdateStatus($"База {DBTools.DBName} создана", "БД: создана");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания БД: {ex.Message}", ex);
            }
        }

        //private LibraryDBContext CreateDbContext()
        //{
        //    var dbName = DatabaseSettings.Instance.SelectedDbName;
        //    var optionsBuilder = new DbContextOptionsBuilder<LibraryDBContext>();
        //    optionsBuilder.UseSqlServer(GetConnectionString(dbName));
        //    return new LibraryDBContext(optionsBuilder.Options);
        //}


        //private string GetConnectionString(string dbName)
        //{
        //    return $"Server=.;Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;";
        //}

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

        private void LoadMyBooks()
        {
            try
            {
                var history = _dbContext.ReadingHistories
                    .Include(r => r.Book)
                        .ThenInclude(b => b.Author)
                    .Where(r => r.UserID == _currentUser.UserID)
                    .OrderByDescending(r => r.LastReadDate ?? r.StartDate)
                    .ToList();

                MyBooksGrid.ItemsSource = history;
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка загрузки \"моих книг\": {ex.Message}");
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

        private void AddToMyBooks_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = BooksGrid.SelectedItem as Book;
            if (selectedBook == null)
            {
                ShowErrorMessage("Сначала выберите книгу.");
                return;
            }

            bool alreadyExists = _dbContext.ReadingHistories
                .Any(r => r.UserID == _currentUser.UserID && r.BookID == selectedBook.BookID);

            if (alreadyExists)
            {
                MessageBox.Show("Эта книга уже есть в вашем списке.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var entry = new ReadingHistory
            {
                UserID = _currentUser.UserID,
                BookID = selectedBook.BookID,
                StartDate = DateTime.Now,
                LastReadDate = DateTime.Now,
                ProgressPercent = 0,
                IsFinished = false
            };

            _dbContext.ReadingHistories.Add(entry);
            _dbContext.SaveChanges();
            LoadMyBooks();
        }

        private void ReadMyBook_Click(object sender, RoutedEventArgs e)
        {
            var selected = MyBooksGrid.SelectedItem as ReadingHistory;
            if (selected?.Book == null || string.IsNullOrWhiteSpace(selected.Book.Text))
            {
                ShowErrorMessage("Нет текста для отображения.");
                return;
            }

            selected.LastReadDate = DateTime.Now;
            _dbContext.SaveChanges();

            var window = new BookTextWindow(selected.Book.Text, selected.Book.Title);
            window.ShowDialog();

            LoadMyBooks();
        }

        private void RemoveFromMyBooks_Click(object sender, RoutedEventArgs e)
        {
            var selected = MyBooksGrid.SelectedItem as ReadingHistory;
            if (selected == null)
            {
                ShowErrorMessage("Выберите книгу для удаления.");
                return;
            }

            _dbContext.ReadingHistories.Remove(selected);
            _dbContext.SaveChanges();
            LoadMyBooks();
        }

        private void BooksGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedBook = BooksGrid.SelectedItem as Book;
            if (selectedBook == null) return;

            if (string.IsNullOrWhiteSpace(selectedBook.Text))
            {
                MessageBox.Show("У этой книги пока нет текста.", "Нет содержимого", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var window = new BookTextWindow(selectedBook.Text, selectedBook.Title);
            window.ShowDialog();
        }

        private void MyBooksGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ReadMyBook_Click(sender, e);
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

        protected override void OnClosed(EventArgs e)
        {
            _dbContext?.Dispose();
            base.OnClosed(e);
        }
    }
}
