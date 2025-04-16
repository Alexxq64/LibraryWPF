using System;
using System.Windows;
using LibraryWPF.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryWPF
{
    public partial class EditUserWindow : Window
    {
        private readonly User _user;
        private readonly bool _isEditMode;
        private const string ConnectionString = "Server=.;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public EditUserWindow(User? user = null)
        {
            InitializeComponent();

            _user = user ?? new User();
            _isEditMode = user != null;

            // Настройка интерфейса
            Title = _isEditMode ? "Редактировать пользователя" : "Добавить пользователя";

            // Заполняем поля, если это редактирование
            if (_isEditMode)
            {
                UsernameBox.Text = _user.Username;
                EmailBox.Text = _user.Email;
                FirstNameBox.Text = _user.FirstName;
                LastNameBox.Text = _user.LastName;
                IsAdminBox.IsChecked = _user.IsAdmin;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                MessageBox.Show("Email обязателен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var context = new LibraryDBContext(ConnectionString))
                {
                    if (_isEditMode)
                    {
                        // Получаем пользователя из БД через контекст
                        var userToUpdate = context.Users.Find(_user.UserID);
                        if (userToUpdate == null)
                        {
                            MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Обновляем все поля, включая Username
                        userToUpdate.Username = UsernameBox.Text.Trim();  // Добавлено обновление Username
                        userToUpdate.Email = EmailBox.Text.Trim();
                        userToUpdate.FirstName = FirstNameBox.Text.Trim();
                        userToUpdate.LastName = LastNameBox.Text.Trim();
                        userToUpdate.IsAdmin = IsAdminBox.IsChecked ?? false;

                        // Сохраняем изменения в БД
                        context.SaveChanges();

                        // Обновляем исходный объект _user
                        _user.Username = userToUpdate.Username;  // Обновляем Username
                        _user.Email = userToUpdate.Email;
                        _user.FirstName = userToUpdate.FirstName;
                        _user.LastName = userToUpdate.LastName;
                        _user.IsAdmin = userToUpdate.IsAdmin;
                    }
                    else
                    {
                        // Для нового пользователя генерируем Username
                        var newUser = new User
                        {
                            Username = UsernameBox.Text.Trim(),
                            Email = EmailBox.Text.Trim(),
                            FirstName = FirstNameBox.Text.Trim(),
                            LastName = LastNameBox.Text.Trim(),
                            IsAdmin = IsAdminBox.IsChecked ?? false,
                            RegistrationDate = DateTime.Now,
                            PasswordHash = HashPassword("default")
                        };

                        context.Users.Add(newUser);
                        context.SaveChanges();

                        // Заполняем ID нового пользователя
                        _user.UserID = newUser.UserID;
                    }

                    DialogResult = true;
                    Close();
                }
            }
            catch (DbUpdateException dbEx)
            {
                MessageBox.Show($"Ошибка базы данных: {dbEx.InnerException?.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string HashPassword(string password)
        {
            // В реальном приложении используйте надежное хеширование
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(password));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
