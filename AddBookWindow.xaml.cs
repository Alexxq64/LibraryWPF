using System;
using System.Collections.Generic;
using System.Windows;
using LibraryWPF.Models;

namespace LibraryWPF
{
    public partial class AddBookWindow : Window
    {
        private readonly List<Book> _cache;
        private readonly LibraryDBContext _dbContext;

        public AddBookWindow(List<Book> cache, LibraryDBContext dbContext)
        {
            InitializeComponent();
            _cache = cache;
            _dbContext = dbContext;
            FillDefaultInput(); // Заполняем тестовые данные
        }

        // 📝 Заполнение полей по умолчанию
        private void FillDefaultInput()
        {
            TitleTextBox.Text = "Игра престолов";
            AuthorFirstNameTextBox.Text = "Джордж";
            AuthorLastNameTextBox.Text = "Мартин";
        }

        // ✅ Обработчик кнопки подтверждения
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AreInputsValid())
            {
                MessageBox.Show("Заполните все обязательные поля");
                return;
            }

            var cachedBook = CreateCachedBook();
            var dbBook = CreateDbBook();

            try
            {
                SaveBookToDatabase(dbBook);
                _cache.Add(cachedBook);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex);
            }
        }

        // 📌 Проверка введённых данных
        private bool AreInputsValid()
        {
            return !string.IsNullOrWhiteSpace(TitleTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(AuthorFirstNameTextBox.Text) &&
                   !string.IsNullOrWhiteSpace(AuthorLastNameTextBox.Text);
        }

        // 🔄 Создание книги для отображения (кэш)
        private Book CreateCachedBook()
        {
            return new Book
            {
                Title = TitleTextBox.Text.Trim(),
                Author = new Author
                {
                    LastName = AuthorLastNameTextBox.Text.Trim()
                }
            };
        }

        // 💾 Создание полной книги для БД
        private Book CreateDbBook()
        {
            return new Book
            {
                Title = TitleTextBox.Text.Trim(),
                ISBN = GenerateISBN(),
                Author = new Author
                {
                    FirstName = AuthorFirstNameTextBox.Text.Trim(),
                    LastName = AuthorLastNameTextBox.Text.Trim()
                }
            };
        }

        // 🧠 Генерация случайного ISBN
        private string GenerateISBN()
        {
            var random = new Random();
            return $"{random.Next(100, 999)}-{random.Next(1000000, 9999999)}";
        }

        // 💽 Сохраняем книгу в базу данных
        private void SaveBookToDatabase(Book book)
        {
            _dbContext.Books.Add(book);
            _dbContext.SaveChanges();
        }

        // ⚠️ Показываем сообщение об ошибке
        private void ShowErrorMessage(Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.InnerException?.Message ?? ex.Message}");
        }
    }
}
