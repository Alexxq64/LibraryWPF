using LibraryWPF.Models;
using System.Windows;

namespace LibraryWPF
{
    public partial class BookTextWindow : Window
    {
        public BookTextWindow(Book book)
        {
            InitializeComponent();
            Title = book.Title;
            BookTextBox.Text = string.IsNullOrWhiteSpace(book.Text)
                ? "Текст отсутствует."
                : book.Text;
        }
    }
}
