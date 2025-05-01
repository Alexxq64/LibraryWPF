using LibraryWPF.Services;
using Microsoft.Data.SqlClient;
using System;
using System.Diagnostics;
using System.Windows;
using System.Xml.Linq;

namespace LibraryWPF
{
    public partial class ChooseDbWindow : Window
    {
        public ChooseDbWindow()
        {
            InitializeComponent();
            this.ContentRendered += (s, e) => InitializeWindow();
        }

        private void InitializeWindow()
        {
            try
            {
                // 1. Инициализация списка
                ExistingDbComboBox.ItemsSource = new[] { "LibraryDB", "ArchiveDB (недоступно)", "TestDB (недоступно)" };
                ExistingDbComboBox.SelectedIndex = 0;
                UpdateControlsVisibility();

                // 2. Проверка подключения только к LibraryDB
                string dbName = "LibraryDB"; // Жестко проверяем только основную БД
                if (!DBTools.TestConnection(dbName))
                {
                    MessageBox.Show("Не удалось подключиться к основной базе LibraryDB",
                                  "Ошибка подключения",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }

                // 3. Сохраняем имя БД
                DBTools.DBName = dbName;
            }
            catch
            {
                Application.Current.Shutdown();
            }
        }

        private void UpdateControlsVisibility()
        {
            if (NewDbNameTextBox == null || ExistingDbComboBox == null) return;

            NewDbNameTextBox.Visibility = CreateNewDbRadioButton.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;

            ExistingDbComboBox.Visibility = UseExistingDbRadioButton.IsChecked == true
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            UpdateControlsVisibility();
        }

        //private void OkButton_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (CreateNewDbRadioButton.IsChecked == true &&
        //            string.IsNullOrWhiteSpace(NewDbNameTextBox.Text))
        //        {
        //            MessageBox.Show("Введите имя новой базы данных", "Ошибка",
        //                MessageBoxButton.OK, MessageBoxImage.Warning);
        //            return;
        //        }

        //        DatabaseSettings.Instance.IsCreateNewDb = CreateNewDbRadioButton.IsChecked == true;
        //        DatabaseSettings.Instance.SelectedDbName = CreateNewDbRadioButton.IsChecked == true
        //            ? NewDbNameTextBox.Text.Trim()
        //            : ExistingDbComboBox.SelectedItem?.ToString();

        //        var dbName = DatabaseSettings.Instance.SelectedDbName;
        //        string connectionString = $"Server=localhost;Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;";
        //        LibraryWPF.Services.DbConnectionService.ConnectionString = connectionString;


        //        DialogResult = true;
        //        Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
        //            MessageBoxButton.OK, MessageBoxImage.Error);
        //        CloseApplication();
        //    }
        //}

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (CreateNewDbRadioButton.IsChecked == true)
            {
                // Получаем валидное имя (метод сам обрабатывает все повторы)
                DBTools.DBName = ValidateDatabaseName();
                DatabaseSettings.Instance.IsCreateNewDb = CreateNewDbRadioButton.IsChecked == true;
                DBTools.IsCreateNewDb = CreateNewDbRadioButton.IsChecked == true;
            }

            DatabaseSettings.Instance.SelectedDbName = DBTools.DBName;
            LibraryWPF.Services.DbConnectionService.ConnectionString = DBTools.ConnectionString;
            DialogResult = true;
            Close();
        }

        private string ValidateDatabaseName()
        {
            //while (true)
            //{
            string name = NewDbNameTextBox.Text.Trim();

            //    if (string.IsNullOrWhiteSpace(name))
            //    {
            //        MessageBox.Show("Имя БД не может быть пустым", "Ошибка",
            //                      MessageBoxButton.OK, MessageBoxImage.Error);
            //        continue;
            //    }

            //    if (name.Any(c => !char.IsLetterOrDigit(c) && c != '_'))
            //    {
            //        MessageBox.Show("Имя БД может содержать только буквы, цифры и _", "Ошибка",
            //                      MessageBoxButton.OK, MessageBoxImage.Error);
            //        continue;
            //    }

            //MessageBox.Show(name);
            return "LibraryDB"; // Возвращаем только валидное имя
            //}
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CloseApplication();
        }

        private void CloseApplication()
        {
            // Корректное завершение приложения
            Application.Current.Shutdown();
        }
    }
}