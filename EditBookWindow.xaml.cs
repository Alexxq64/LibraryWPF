using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using LibraryWPF.Models;
using LibraryWPF.Services;
using Microsoft.EntityFrameworkCore;

namespace LibraryWPF
{
    public partial class EditBookWindow : Window
    {
        private readonly Book _book;
        private readonly bool _isEditMode;
        private const string DatabaseName = "LibraryDB";

        public EditBookWindow(Book? book = null)
        {
            InitializeComponent();

            if (book == null)
            {
                _isEditMode = false;  // Режим добавления
                _book = new Book();
                Title = "Добавить книгу";
                AddOrSaveButton.Content = "Добавить";  // Кнопка будет называться "Добавить"
            }
            else
            {
                _isEditMode = true;  // Режим редактирования
                _book = book;
                Title = "Редактировать книгу";
                AddOrSaveButton.Content = "Сохранить";  // Кнопка будет называться "Сохранить"

                // Заполнение полей данными книги
                TitleTextBox.Text = _book.Title;
                ISBNTextBox.Text = _book.ISBN;
                DescriptionTextBox.Text = _book.Description;
                PublicationYearTextBox.Text = _book.PublicationYear?.ToString();
                TotalPagesTextBox.Text = _book.TotalPages?.ToString();
                IsFreeCheckBox.IsChecked = _book.IsFree;
                TextTextBox.Text = _book.Text;
            }

            LoadAuthors();  // Загружаем авторов
        }

        private void LoadAuthors()
        {
            if (!DBTools.TestConnection()) return;

            try
            {
                using (var context = new LibraryDBContext())
                {
                    var authors = context.Authors.ToList();
                    AuthorsListBox.ItemsSource = authors;

                    if (_isEditMode)  // Если в режиме редактирования, загружаем авторов книги
                    {
                        var bookWithAuthors = context.Books
                            .Include(b => b.Authors)
                            .FirstOrDefault(b => b.BookID == _book.BookID);

                        if (bookWithAuthors != null)
                        {
                            foreach (var author in bookWithAuthors.Authors)
                            {
                                var match = authors.FirstOrDefault(a => a.AuthorID == author.AuthorID);
                                if (match != null)
                                    AuthorsListBox.SelectedItems.Add(match);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки авторов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddOrSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs() || !DBTools.TestConnection()) return;

            try
            {
                using (var context = new LibraryDBContext())
                {
                    var selectedAuthors = AuthorsListBox.SelectedItems.Cast<Author>().ToList();

                    if (_isEditMode)  // Если редактируем, обновляем книгу
                    {
                        var bookToUpdate = context.Books
                            .Include(b => b.Authors)
                            .FirstOrDefault(b => b.BookID == _book.BookID);

                        if (bookToUpdate == null) return;

                        bookToUpdate.Title = TitleTextBox.Text;
                        bookToUpdate.ISBN = ISBNTextBox.Text;
                        bookToUpdate.Description = DescriptionTextBox.Text;
                        bookToUpdate.PublicationYear = ParseNullableInt(PublicationYearTextBox.Text);
                        bookToUpdate.TotalPages = ParseNullableInt(TotalPagesTextBox.Text);
                        bookToUpdate.IsFree = IsFreeCheckBox.IsChecked ?? false;
                        bookToUpdate.Text = TextTextBox.Text;

                        // Удаляем старые связи с авторами
                        bookToUpdate.Authors.Clear();

                        // Добавляем новые связи
                        foreach (var author in selectedAuthors)
                        {
                            bookToUpdate.Authors.Add(author);
                        }

                        context.SaveChanges();
                        MessageBox.Show("Изменения сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else  // Если добавляем, создаем новую книгу
                    {
                        var newBook = new Book
                        {
                            Title = TitleTextBox.Text,
                            ISBN = ISBNTextBox.Text,
                            Description = DescriptionTextBox.Text,
                            PublicationYear = ParseNullableInt(PublicationYearTextBox.Text),
                            TotalPages = ParseNullableInt(TotalPagesTextBox.Text),
                            IsFree = IsFreeCheckBox.IsChecked ?? false,
                            Text = TextTextBox.Text,
                            Authors = selectedAuthors
                        };

                        context.Books.Add(newBook);
                        context.SaveChanges();
                        MessageBox.Show("Книга добавлена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
