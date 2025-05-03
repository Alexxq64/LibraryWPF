using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using LibraryWPF.Models;
using System.Windows.Controls;
using System.Windows;

namespace LibraryWPF.Services
{
    internal static class WPFTools
    {
        private static void LoadBooks(
            LibraryDBContext dbContext,
            DataGrid booksGrid,
            TextBlock statusText,
            TextBlock dbStatusText,
            string currentDbName)
        {
            try
            {
                // Обновление статуса
                statusText.Text = "Загрузка книг...";
                dbStatusText.Text = "БД: подключение";

                // Получаем книги из базы данных
                var books = dbContext.Books
                    .Include(b => b.Authors)
                    .OrderBy(b => b.Title)
                    .AsNoTracking()
                    .ToList();

                // Обновление источника данных для DataGrid
                booksGrid.ItemsSource = books;

                // Обновление статуса
                statusText.Text = $"Загружено {books.Count} книг";
                dbStatusText.Text = $"БД: {currentDbName}";
            }
            catch (Exception ex)
            {
                // Обработка ошибок
                HandleDataLoadingError(ex);
            }
        }

        private static void HandleDataLoadingError(Exception ex)
        {
            ShowErrorMessage($"Ошибка загрузки данных: {ex.Message}");
            //UpdateStatus("Ошибка загрузки", "БД: сбой");
        }

        private static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}

