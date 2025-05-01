using System;
using System.Linq;
using System.Windows;
using LibraryWPF.Models;

namespace LibraryWPF
{
    public partial class AddAuthorWindow : Window
    {
        private readonly LibraryDBContext _dbContext;

        public AddAuthorWindow(LibraryDBContext dbContext)
        {
            InitializeComponent();
            _dbContext = dbContext;
        }
        public AddAuthorWindow()
        {
            InitializeComponent();
            _dbContext = new LibraryDBContext();        }


        private void AddAuthorButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text) || string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, заполните все поля.");
                return;
            }

            var author = new Author
            {
                FirstName = FirstNameTextBox.Text.Trim(),
                LastName = LastNameTextBox.Text.Trim()
            };

            try
            {
                _dbContext.Authors.Add(author);
                _dbContext.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления автора: {ex.Message}");
            }
        }
    }
}
