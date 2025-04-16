using System;
using System.Linq;
using System.Windows;
using LibraryWPF.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryWPF
{
    public partial class EditBookWindow : Window
    {
        private readonly Book _book;
        private readonly bool _isEditMode;
        private const string DatabaseName = "LibraryDB"; // Имя вашей БД

        public EditBookWindow(Book? book = null)
        {
            InitializeComponent();

            if (book == null)
            {
                _isEditMode = false;
                _book = new Book();
                Title = "Добавить книгу";
                SaveBookButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                _isEditMode = true;
                _book = book;
                Title = "Редактировать книгу";
                AddBookButton.Visibility = Visibility.Collapsed;

                TitleTextBox.Text = _book.Title;
                ISBNTextBox.Text = _book.ISBN;
                DescriptionTextBox.Text = _book.Description;
                PublicationYearTextBox.Text = _book.PublicationYear?.ToString();
                TotalPagesTextBox.Text = _book.TotalPages?.ToString();
                IsFreeCheckBox.IsChecked = _book.IsFree;
                TextTextBox.Text = _book.Text;
            }

            LoadAuthors();
        }

        private string GetConnectionString(string dbName)
        {
            return $"Server=.;Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;";
        }

        private bool CheckDatabaseConnection()
        {
            try
            {
                using (var context = new LibraryDBContext(GetConnectionString(DatabaseName)))
                {
                    return context.Database.CanConnect();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void LoadAuthors()
        {
            if (!CheckDatabaseConnection())
            {
                return;
            }

            try
            {
                using (var context = new LibraryDBContext(GetConnectionString(DatabaseName)))
                {
                    var authors = context.Authors
                        .Select(a => new { a.AuthorID, FullName = $"{a.FirstName} {a.LastName}" })
                        .ToList();

                    AuthorComboBox.ItemsSource = authors;
                    AuthorComboBox.DisplayMemberPath = "FullName";
                    AuthorComboBox.SelectedValuePath = "AuthorID";

                    if (_isEditMode)
                    {
                        AuthorComboBox.SelectedValue = _book.AuthorID;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки авторов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs() || !CheckDatabaseConnection()) return;

            try
            {
                using (var context = new LibraryDBContext(GetConnectionString(DatabaseName)))
                {
                    context.Books.Add(new Book
                    {
                        Title = TitleTextBox.Text,
                        ISBN = ISBNTextBox.Text,
                        Description = DescriptionTextBox.Text,
                        PublicationYear = ParseNullableInt(PublicationYearTextBox.Text),
                        TotalPages = ParseNullableInt(TotalPagesTextBox.Text),
                        IsFree = IsFreeCheckBox.IsChecked ?? false,
                        Text = TextTextBox.Text,
                        AuthorID = (int?)AuthorComboBox.SelectedValue
                    });
                    context.SaveChanges();
                }

                MessageBox.Show("Книга добавлена!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveBookButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs() || !CheckDatabaseConnection()) return;

            try
            {
                using (var context = new LibraryDBContext(GetConnectionString(DatabaseName)))
                {
                    var bookToUpdate = context.Books.Find(_book.BookID);
                    if (bookToUpdate == null) return;

                    bookToUpdate.Title = TitleTextBox.Text;
                    bookToUpdate.ISBN = ISBNTextBox.Text;
                    bookToUpdate.Description = DescriptionTextBox.Text;
                    bookToUpdate.PublicationYear = ParseNullableInt(PublicationYearTextBox.Text);
                    bookToUpdate.TotalPages = ParseNullableInt(TotalPagesTextBox.Text);
                    bookToUpdate.IsFree = IsFreeCheckBox.IsChecked ?? false;
                    bookToUpdate.Text = TextTextBox.Text;
                    bookToUpdate.AuthorID = (int?)AuthorComboBox.SelectedValue;

                    context.SaveChanges();
                }

                MessageBox.Show("Изменения сохранены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Введите название книги", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private int? ParseNullableInt(string text)
        {
            return int.TryParse(text, out int result) ? result : null;
        }
    }
}