using System;
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

            // Загружаем данные книг в DataGrid
            LoadBooks();
            LoadUsers();
        }

        public AdminWindow()
        {
            InitializeComponent();
            _dbContext = new LibraryDBContext(); // Здесь инициализируем dbContext без параметра

            // Загружаем данные книг в DataGrid
            LoadBooks();
            LoadUsers();
        }


        private void LoadBooks()
        {
            //using (var context = new LibraryDBContext("Server=.;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;"))
            using (var context = new LibraryDBContext())
            {
                var books = context.Books.Include(b => b.Author).ToList();
                BooksGrid.ItemsSource = books;
            }
        }

        // Обработчик для кнопки "Добавить книгу"
        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new EditBookWindow(); // Используем конструктор для добавления
            var result = addWindow.ShowDialog();
            if (result == true)
            {
                LoadBooks(); // Обновить список после добавления
            }
        }

        // Обработчик для кнопки "Редактировать"
        private void EditBookButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = BooksGrid.SelectedItem as Book;

            if (selectedBook == null)
            {
                MessageBox.Show("Пожалуйста, выберите книгу для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var editWindow = new EditBookWindow(selectedBook); // Конструктор для редактирования
            var result = editWindow.ShowDialog();
            if (result == true)
            {
                LoadBooks(); // Обновить список после редактирования
            }
        }

        private void DeleteBookButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = BooksGrid.SelectedItem as Book;

            if (selectedBook == null)
            {
                MessageBox.Show("Пожалуйста, выберите книгу для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var confirmResult = MessageBox.Show($"Вы уверены, что хотите удалить книгу \"{selectedBook.Title}\"?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirmResult == MessageBoxResult.Yes)
            {
                try
                {
                    //using (var context = new LibraryDBContext("Server=.;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;"))
                    using (var context = new LibraryDBContext())
                    {
                        // Перезапрашиваем книгу из текущего контекста по ID
                        var bookToDelete = context.Books.Find(selectedBook.BookID);
                        if (bookToDelete != null)
                        {
                            context.Books.Remove(bookToDelete);
                            context.SaveChanges();
                        }
                    }

                    LoadBooks(); // Обновить список книг
                    MessageBox.Show("Книга удалена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обработчик для кнопки "Читать"
        private void ReadBookButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = BooksGrid.SelectedItem as Book;

            if (selectedBook == null)
            {
                MessageBox.Show("Пожалуйста, выберите книгу для чтения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var bookTextWindow = new BookTextWindow(selectedBook.Text ?? "Текст книги отсутствует.");
            bookTextWindow.ShowDialog();
        }

        private void LoadUsers()
        {
            //using (var context = new LibraryDBContext("Server=.;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;"))
            using (var context = new LibraryDBContext())
            {
                var users = context.Users.ToList();
                UsersGrid.ItemsSource = users;
            }
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            // Создаем окно для добавления нового пользователя с пустыми полями
            var editUserWindow = new EditUserWindow();
            if (editUserWindow.ShowDialog() == true) // Если пользователь нажал "Сохранить"
            {
                LoadUsers(); // Обновляем список пользователей в DataGrid
            }
        }


        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersGrid.SelectedItem as User;

            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var editWindow = new EditUserWindow(selectedUser);
            if (editWindow.ShowDialog() == true)
            {
                //using (var context = new LibraryDBContext("Server=.;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;"))
                using (var context = new LibraryDBContext())
                {
                    var userToUpdate = context.Users.Find(selectedUser.UserID);
                    if (userToUpdate != null)
                    {
                        userToUpdate.Email = selectedUser.Email;
                        userToUpdate.FirstName = selectedUser.FirstName;
                        userToUpdate.LastName = selectedUser.LastName;
                        userToUpdate.IsAdmin = selectedUser.IsAdmin;

                        context.SaveChanges();
                    }
                }

                LoadUsers();
                MessageBox.Show("Пользователь обновлен.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersGrid.SelectedItem as User;

            if (selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (MessageBox.Show($"Удалить пользователя \"{selectedUser.Username}\"?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                using (var context = new LibraryDBContext())
                //using (var context = new LibraryDBContext("Server=.;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;"))
                {
                    var userToDelete = context.Users.Find(selectedUser.UserID);
                    if (userToDelete != null)
                    {
                        context.Users.Remove(userToDelete);
                        context.SaveChanges();
                        LoadUsers();
                        MessageBox.Show("Пользователь удалён.", "Инфо", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
        }

    }
}
