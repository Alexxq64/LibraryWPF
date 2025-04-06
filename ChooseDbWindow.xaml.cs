using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Diagnostics;

namespace LibraryWPF
{
    public partial class ChooseDbWindow : Window
    {
        public string SelectedDbName { get; private set; } = "LibraryDB";
        public bool IsCreateNewDb { get; private set; }
        public string NewDbName { get; private set; }

        public ChooseDbWindow()
        {
            InitializeComponent();

            // Отложенная инициализация после полной загрузки всех компонентов
            this.ContentRendered += (s, e) =>
            {
                InitializeComponents();
                UpdateControlsVisibility();
            };
        }

        private void InitializeComponents()
        {
            try
            {
                ExistingDbComboBox.Items.Clear();
                ExistingDbComboBox.Items.Add("LibraryDB");
                ExistingDbComboBox.Items.Add("ArchiveDB");
                ExistingDbComboBox.Items.Add("TestDB");
                ExistingDbComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка инициализации: {ex.Message}");
            }
        }

        private void UpdateControlsVisibility()
        {
            // Безопасное обновление видимости
            if (NewDbNameTextBox == null || ExistingDbComboBox == null ||
                CreateNewDbRadioButton == null || UseExistingDbRadioButton == null)
            {
                return;
            }

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
                if (string.IsNullOrWhiteSpace(NewDbNameTextBox.Text))
                {
                    MessageBox.Show("Введите имя новой базы данных", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                IsCreateNewDb = true;
                NewDbName = NewDbNameTextBox.Text.Trim();
                SelectedDbName = NewDbName;
            }
            else
            {
                IsCreateNewDb = false;
                SelectedDbName = ExistingDbComboBox.SelectedItem?.ToString() ?? "LibraryDB";
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}