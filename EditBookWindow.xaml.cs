using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using LibraryWPF.Models;
using LibraryWPF.Services;
using Microsoft.EntityFrameworkCore;
using System.Windows.Controls;

namespace LibraryWPF
{
    public partial class EditBookWindow : Window
    {
        private Book _book;
        private readonly bool _isEditMode;
        private const string DatabaseName = "LibraryDB";

        //public EditBookWindow(Book? book = null)
        //{
        //    InitializeComponent();

        //    if (book == null)
        //    {
        //        _isEditMode = false;  // Режим добавления
        //        _book = new Book();
        //        Title = "Добавить книгу";
        //        AddOrSaveButton.Content = "Добавить";  // Кнопка будет называться "Добавить"
        //    }
        //    else
        //    {
        //        _isEditMode = true;  // Режим редактирования
        //        _book = book;
        //        Title = "Редактировать книгу";
        //        AddOrSaveButton.Content = "Сохранить";  // Кнопка будет называться "Сохранить"

        //        // Заполнение полей данными книги
        //        TitleTextBox.Text = _book.Title;
        //        ISBNTextBox.Text = _book.ISBN;
        //        DescriptionTextBox.Text = _book.Description;
        //        PublicationYearTextBox.Text = _book.PublicationYear?.ToString();
        //        TotalPagesTextBox.Text = _book.TotalPages?.ToString();
        //        IsFreeCheckBox.IsChecked = _book.IsFree;
        //        TextTextBox.Text = _book.Text;
        //    }

        //    LoadAuthors();  // Загружаем авторов
        //}

        public EditBookWindow()
        {
            InitializeComponent();

            _isEditMode = false;
            _book = new Book();

            Title = "Добавить книгу";
            AddOrSaveButton.Content = "Добавить";

            LoadAuthors();  // просто загружаем всех авторов
        }

        public EditBookWindow(int bookId)
        {
            InitializeComponent();

            _isEditMode = true;
            Title = "Редактировать книгу";
            AddOrSaveButton.Content = "Сохранить";

            LoadBook(bookId);    // загружаем книгу и данные
            LoadAuthors();       // загружаем всех авторов и выделяем нужных
        }

        private void LoadBook(int bookId)
        {
            using (var context = new LibraryDBContext())
            {
                _book = context.Books
                               .Include(b => b.Authors)
                               .FirstOrDefault(b => b.BookID == bookId);

                if (_book != null)
                {
                    TitleTextBox.Text = _book.Title;
                    ISBNTextBox.Text = _book.ISBN;
                    DescriptionTextBox.Text = _book.Description;
                    PublicationYearTextBox.Text = _book.PublicationYear?.ToString();
                    TotalPagesTextBox.Text = _book.TotalPages?.ToString();
                    IsFreeCheckBox.IsChecked = _book.IsFree;
                    TextTextBox.Text = _book.Text;
                }
            }
        }


        //private void LoadAuthors()
        //{
        //    if (!DBTools.TestConnection()) return;

        //    try
        //    {
        //        using (var context = new LibraryDBContext())
        //        {
        //            var authors = context.Authors.ToList();
        //            AuthorsListBox.ItemsSource = authors;

        //            if (_isEditMode)  // Если в режиме редактирования, загружаем авторов книги
        //            {
        //                var bookWithAuthors = context.Books
        //                    .Include(b => b.Authors)
        //                    .FirstOrDefault(b => b.BookID == _book.BookID);

        //                if (bookWithAuthors != null)
        //                {
        //                    foreach (var author in bookWithAuthors.Authors)
        //                    {
        //                        var match = authors.FirstOrDefault(a => a.AuthorID == author.AuthorID);
        //                        if (match != null)
        //                            AuthorsListBox.SelectedItems.Add(match);
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка загрузки авторов: {ex.Message}", "Ошибка",
        //            MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        private void LoadAuthors()
        {
            using (var context = new LibraryDBContext())
            {
                var authors = context.Authors.ToList();

                AuthorsListBox.ItemsSource = authors;

                if (_isEditMode)
                {
                    var selectedIds = _book.Authors.Select(a => a.AuthorID).ToList();

                    foreach (var author in authors)
                    {
                        if (selectedIds.Contains(author.AuthorID))
                        {
                            AuthorsListBox.SelectedItems.Add(author);
                        }
                    }
                }
            }
        }


        private void AddOrSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs() || !DBTools.TestConnection()) return;

            try
            {
                using (var context = new LibraryDBContext())
                {
                    //var selectedAuthors = AuthorsListBox.SelectedItems.Cast<Author>().ToList();
                    var selectedAuthorIds = AuthorsListBox.SelectedItems.Cast<Author>().Select(a => a.AuthorID).ToList();
                    var selectedAuthors = context.Authors.Where(a => selectedAuthorIds.Contains(a.AuthorID)).ToList();

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

        //private void AuthorsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    var selectedAuthors = AuthorsListBox.SelectedItems.Cast<Author>().ToList();
        //    MessageBox.Show($"Выбрано авторов: {selectedAuthors.Count}", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        //}


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
