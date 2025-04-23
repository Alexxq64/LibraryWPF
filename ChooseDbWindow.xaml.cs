using System;
using System.Diagnostics;
using System.Windows;

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
                ExistingDbComboBox.ItemsSource = new[] { "LibraryDB", "ArchiveDB", "TestDB" };
                ExistingDbComboBox.SelectedIndex = 0;
                UpdateControlsVisibility();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка инициализации: {ex.Message}");
                MessageBox.Show("Ошибка инициализации окна", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                CloseApplication();
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
            try
            {
                if (CreateNewDbRadioButton.IsChecked == true &&
                    string.IsNullOrWhiteSpace(NewDbNameTextBox.Text))
                {
                    MessageBox.Show("Введите имя новой базы данных", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DatabaseSettings.Instance.IsCreateNewDb = CreateNewDbRadioButton.IsChecked == true;
                DatabaseSettings.Instance.SelectedDbName = CreateNewDbRadioButton.IsChecked == true
                    ? NewDbNameTextBox.Text.Trim()
                    : ExistingDbComboBox.SelectedItem?.ToString();

                var dbName = DatabaseSettings.Instance.SelectedDbName;
                string connectionString = $"Server=localhost;Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;";
                LibraryWPF.Services.DbConnectionService.ConnectionString = connectionString;


                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                CloseApplication();
            }
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