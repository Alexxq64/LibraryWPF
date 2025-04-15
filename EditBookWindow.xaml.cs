using LibraryWPF.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace LibraryWPF
{
    public partial class EditBookWindow : Window
    {
        private readonly LibraryDBContext _dbContext;
        private readonly Book _book;
        private readonly bool _isNew;

        public EditBookWindow(LibraryDBContext dbContext, Book book = null)
        {
            InitializeComponent();
            _dbContext = dbContext;
            _isNew = book == null;
            _book = book ?? new Book();

            Title = _isNew ? "Добавить книгу" : "Редактировать книгу";
            ConfirmButton.Content = _isNew ? "Добавить книгу" : "Сохранить изменения";

            LoadAuthors();

            if (!_isNew)
            {
                TitleTextBox.Text = _book.Title;
                AuthorComboBox.SelectedValue = _book.AuthorID;
            }
        }


        private void LoadAuthors()
        {
            var authors = _dbContext.Authors
                .AsNoTracking()
                .OrderBy(a => a.LastName)
                .ToList();

            AuthorComboBox.ItemsSource = authors;
        }





        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text) || AuthorComboBox.SelectedItem is not Author selectedAuthor)
            {
                MessageBox.Show("Пожалуйста, заполните все поля.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _book.Title = TitleTextBox.Text.Trim();
            _book.AuthorID = selectedAuthor.AuthorID;

            if (_isNew)
                _dbContext.Books.Add(_book);

            _dbContext.SaveChanges();
            DialogResult = true;
            Close();
        }

        private void AddAuthorButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция добавления автора пока не реализована.");
        }


    }
}
