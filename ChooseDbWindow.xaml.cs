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

        private List<string> GetAvailableDatabases()
        {
            var databases = new List<string>();

            try
            {
                // 1. Устанавливаем временно master
                DBTools.DBName = "master";

                // 2. Используем стандартный механизм создания подключения
                using (var connection = DBTools.CreateConnection())
                {
                    connection.Open();

                    string query = @"
                SELECT name 
                FROM sys.databases 
                WHERE database_id > 4  -- исключаем системные базы
                AND state = 0  -- только online базы
                ORDER BY name";

                    using (var command = new SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            databases.Add(reader["name"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении списка БД: {ex.Message}\n\n" +
                              "Убедитесь, что:\n" +
                              "1. SQL Server запущен\n" +
                              "2. Разрешено подключение по Trusted_Connection",
                              "Ошибка подключения",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }

            return databases;
        }

        private void InitializeWindow()
        {
            try
            {
                // Получаем список доступных баз данных
                var availableDbs = GetAvailableDatabases();

                // Настройка интерфейса
                ExistingDbComboBox.ItemsSource = availableDbs;

                if (availableDbs.Any())
                {
                    ExistingDbComboBox.SelectedIndex = 0;
                    UseExistingDbRadioButton.IsChecked = true;
                }
                else
                {
                    CreateNewDbRadioButton.IsChecked = true;
                    MessageBox.Show("Не найдено пользовательских баз данных",
                                  "Информация",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Information);
                }

                UpdateControlsVisibility();

                //// Если пользователь выбирает существующую БД
                //if (UseExistingDbRadioButton.IsChecked == true && ExistingDbComboBox.SelectedItem != null)
                //{
                //    string selectedDb = ExistingDbComboBox.SelectedItem.ToString();

                //    if (!DBTools.TestConnection(selectedDb))
                //    {
                //        MessageBox.Show($"Не удалось подключиться к базе {selectedDb}",
                //                      "Ошибка подключения",
                //                      MessageBoxButton.OK,
                //                      MessageBoxImage.Error);
                //        return;
                //    }

                //    DBTools.DBName = selectedDb;
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка: {ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
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

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (CreateNewDbRadioButton.IsChecked == true)
            {
                // Получаем валидное имя 
                DBTools.DBName = ValidateDatabaseName();
                DBTools.IsCreateNewDb = true;
            }
            else
            {
                
                //string selectedDb = ExistingDbComboBox.SelectedItem.ToString();
                DBTools.DBName = ExistingDbComboBox.SelectedItem.ToString();
                DBTools.IsCreateNewDb = false;
            }
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

            return name; // Возвращаем только валидное имя
            //}
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CloseApplication();
        }

        private void CloseApplication()
        {
            Application.Current.Shutdown();
        }
    }
}