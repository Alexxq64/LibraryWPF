using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using LibraryWPF.Models;
using System.Linq; // Добавлено для проверки пустых полей

namespace LibraryWPF
{
    public partial class AddBookWindow : Window
    {
        private readonly List<Book> _cache;

        public AddBookWindow(List<Book> cache)
        {
            InitializeComponent();
            _cache = cache;
            // Автозаполнение теперь только если поля пустые
            TitleTextBox.Text = string.IsNullOrEmpty(TitleTextBox.Text) ? "Игра престолов" : TitleTextBox.Text;
            AuthorTextBox.Text = string.IsNullOrEmpty(AuthorTextBox.Text) ? "Джордж Мартин" : AuthorTextBox.Text;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка на пустые поля
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text) ||
                string.IsNullOrWhiteSpace(AuthorTextBox.Text))
            {
                MessageBox.Show("Заполните все поля", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var title = TitleTextBox.Text.Trim();
            var authorName = AuthorTextBox.Text.Trim();

            // Создаем новую книгу
            var newBook = new Book
            {
                Title = title,
                Author = new Author { LastName = authorName }
            };

            // Проверяем и инициализируем кэш, если он null
            if (_cache == null)
            {
                MessageBox.Show("Ошибка: кэш не инициализирован", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Добавляем в кэш
            _cache.Add(newBook);
            this.Close();
        }
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ConfirmButton_Click(null, null);
            }
        }
    }
}