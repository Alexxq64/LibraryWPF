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
            if (NewDbNameTextBox == null || ExistingDbComboBox == null ||
                CreateNewDbRadioButton == null || UseExistingDbRadioButton == null)
                return;

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

                DatabaseSettings.Instance.IsCreateNewDb = true;
                DatabaseSettings.Instance.SelectedDbName = NewDbNameTextBox.Text.Trim();
            }
            else
            {
                DatabaseSettings.Instance.IsCreateNewDb = false;
                DatabaseSettings.Instance.SelectedDbName = ExistingDbComboBox.SelectedItem?.ToString() ?? "LibraryDB";
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
