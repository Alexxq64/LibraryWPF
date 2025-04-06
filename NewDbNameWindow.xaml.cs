using System.Windows;

namespace LibraryWPF
{
    public partial class NewDbNameWindow : Window
    {
        public string DbName { get; private set; }

        public NewDbNameWindow()
        {
            InitializeComponent();
        }

        // Обработка кнопки OK
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DbName = DbNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(DbName))
            {
                MessageBox.Show("Имя базы данных не может быть пустым.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
