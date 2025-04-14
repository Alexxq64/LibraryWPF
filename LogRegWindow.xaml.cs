using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using LibraryWPF.Models;

namespace LibraryWPF
{
    public partial class LogRegWindow : Window
    {
        private readonly LibraryDBContext _dbContext;

        public User LoggedInUser { get; private set; }

        public LogRegWindow(LibraryDBContext dbContext)
        {
            InitializeComponent();
            _dbContext = dbContext;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = LoginUsernameBox.Text.Trim();
            string password = LoginPasswordBox.Password;

            var user = _dbContext.Users.FirstOrDefault(u => u.Username == username);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                MessageBox.Show("Неверное имя пользователя или пароль.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            user.LastLoginDate = DateTime.Now;
            _dbContext.SaveChanges();

            LoggedInUser = user;
            DialogResult = true;
            Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = RegisterUsernameBox.Text.Trim();
            string email = RegisterEmailBox.Text.Trim();
            string password = RegisterPasswordBox.Password;
            string firstName = RegisterFirstNameBox.Text.Trim();
            string lastName = RegisterLastNameBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Заполните все обязательные поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_dbContext.Users.Any(u => u.Username == username))
            {
                MessageBox.Show("Пользователь с таким именем уже существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_dbContext.Users.Any(u => u.Email == email))
            {
                MessageBox.Show("Пользователь с таким email уже зарегистрирован.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string hashedPassword = HashPassword(password);

            var newUser = new User
            {
                Username = username,
                Email = email,
                PasswordHash = hashedPassword,
                FirstName = firstName,
                LastName = lastName,
                RegistrationDate = DateTime.Now,
                LastLoginDate = DateTime.Now
            };

            _dbContext.Users.Add(newUser);
            _dbContext.SaveChanges();

            LoggedInUser = newUser;
            MessageBox.Show("Регистрация прошла успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return HashPassword(password) == storedHash;
        }
    }
}
