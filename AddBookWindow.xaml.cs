using System;
using System.Collections.Generic;
using System.Linq;
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
            FillAuthorsComboBox(); // Заполняем ComboBox с авторами при инициализации окна
        }

        // 📝 Заполнение ComboBox с авторами
        private void FillAuthorsComboBox()
        {
            var authors = _dbContext.Authors.ToList(); // Загружаем всех авторов из базы данных
            var formattedAuthors = authors.Select(a => new
            {
                Author = a,
                Name = $"{a.FirstName} {a.LastName}" // Форматируем имя и фамилию для отображения
            }).ToList();

            AuthorComboBox.ItemsSource = formattedAuthors;
            AuthorComboBox.DisplayMemberPath = "Name";
            AuthorComboBox.SelectedValuePath = "Author"; // Устанавливаем путь для получения данных
        }

        // ✅ Обработчик кнопки "Добавить автора"
        private void AddAuthorButton_Click(object sender, RoutedEventArgs e)
        {
            var addAuthorWindow = new AddAuthorWindow(_dbContext); // Открываем окно добавления автора
            if (addAuthorWindow.ShowDialog() == true) // Если автор успешно добавлен
            {
                FillAuthorsComboBox(); // Обновляем список авторов в ComboBox
            }
        }

        // ✅ Обработчик кнопки "Добавить книгу"
        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (!AreInputsValid())
            {
                MessageBox.Show("Заполните все обязательные поля");
                return;
            }

            var selectedAuthor = AuthorComboBox.SelectedValue as Author; // Получаем выбранного автора

            if (selectedAuthor == null)
            {
                MessageBox.Show("Выберите автора");
                return;
            }

            var book = CreateDbBook(selectedAuthor); // Создаем книгу с выбранным автором

            try
            {
                SaveBookToDatabase(book); // Сохраняем книгу в базе данных
                _cache.Add(book); // Добавляем книгу в кэш
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(ex); // Показываем сообщение об ошибке
            }
        }

        // 📌 Проверка введённых данных
        private bool AreInputsValid()
        {
            return !string.IsNullOrWhiteSpace(TitleTextBox.Text);
        }

        // 🔄 Создание полной книги для БД
        private Book CreateDbBook(Author author)
        {
            return new Book
            {
                Title = TitleTextBox.Text.Trim(),
                ISBN = GenerateISBN(),
                Author = author // Присваиваем выбранного автора
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
