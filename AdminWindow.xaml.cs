﻿using System;
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

        // Загружает книги из базы данных в DataGrid
        private void LoadBooks()
        {
            var books = _dbContext.Books.Include(b => b.Author).ToList();
            BooksGrid.ItemsSource = books; // Привязываем данные к DataGrid
        }

        // Заглушка для кнопки "Добавить"
        private void AddBookButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Кнопка 'Добавить' нажата", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Заглушка для кнопки "Изменить"
        private void EditBookButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Кнопка 'Изменить' нажата", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Заглушка для кнопки "Удалить"
        private void DeleteBookButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Кнопка 'Удалить' нажата", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Заглушка для кнопки "Читать"
        private void ReadBookButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Кнопка 'Читать' нажата", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
