using System.Windows;

namespace LibraryWPF
{
    public partial class BookTextWindow : Window
    {
        public BookTextWindow(string bookText, string bookTitle = "Без названия")
        {
            InitializeComponent();

            BookTitleText.Text = bookTitle;
            BookContentText.Text = string.IsNullOrWhiteSpace(bookText)
                ? "Текст книги отсутствует."
                : bookText;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
