using System;
using System.Linq;
using System.Windows;
using LibraryWPF.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryWPF
{
    public partial class AdminWindow : Window
    {
        private readonly LibraryDBContext _dbContext;

        public AdminWindow(LibraryDBContext dbContext)
        {
            InitializeComponent();
            _dbContext = dbContext;

            // Загружаем данные книг в DataGrid
            LoadBooks();
        }

        private void LoadBooks()
        {
            using (var context = new LibraryDBContext("Server=.;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;"))
            {
                var books = context.Books.Include(b => b.Author).ToList();
                BooksGrid.ItemsSource = books;
            }
        }

        // Обработчик для кнопки "Добавить книгу"
        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new EditBookWindow(); // Используем конструктор для добавления
            var result = addWindow.ShowDialog();
            if (result == true)
            {
                LoadBooks(); // Обновить список после добавления
            }
        }

        // Обработчик для кнопки "Редактировать"
        private void EditBookButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = BooksGrid.SelectedItem as Book;

            if (selectedBook == null)
            {
                MessageBox.Show("Пожалуйста, выберите книгу для редактирования.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var editWindow = new EditBookWindow(selectedBook); // Конструктор для редактирования
            var result = editWindow.ShowDialog();
            if (result == true)
            {
                LoadBooks(); // Обновить список после редактирования
            }
        }

        private void DeleteBookButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = BooksGrid.SelectedItem as Book;

            if (selectedBook == null)
            {
                MessageBox.Show("Пожалуйста, выберите книгу для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var confirmResult = MessageBox.Show($"Вы уверены, что хотите удалить книгу \"{selectedBook.Title}\"?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirmResult == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new LibraryDBContext("Server=.;Database=LibraryDB;Trusted_Connection=True;TrustServerCertificate=True;"))
                    {
                        // Перезапрашиваем книгу из текущего контекста по ID
                        var bookToDelete = context.Books.Find(selectedBook.BookID);
                        if (bookToDelete != null)
                        {
                            context.Books.Remove(bookToDelete);
                            context.SaveChanges();
                        }
                    }

                    LoadBooks(); // Обновить список книг
                    MessageBox.Show("Книга удалена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Обработчик для кнопки "Читать"
        private void ReadBookButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedBook = BooksGrid.SelectedItem as Book;

            if (selectedBook == null)
            {
                MessageBox.Show("Пожалуйста, выберите книгу для чтения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var bookTextWindow = new BookTextWindow(selectedBook.Text ?? "Текст книги отсутствует.");
            bookTextWindow.ShowDialog();
        }
    }
}
