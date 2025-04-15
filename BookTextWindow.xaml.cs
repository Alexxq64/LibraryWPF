using System.Windows;

namespace LibraryWPF
{
    public partial class BookTextWindow : Window
    {
        public BookTextWindow(string bookText)
        {
            InitializeComponent();
            BookTextBox.Text = bookText; // Устанавливаем текст книги в TextBox
        }
    }
}
