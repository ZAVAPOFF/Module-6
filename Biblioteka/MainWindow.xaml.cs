using System.Windows;
using System.Collections.Generic;
using Biblioteka;

namespace Biblioteka
{
    public partial class MainWindow : Window
    {
        private LibraryDatabase _libraryDatabase;
        private int _currentUserID = 1;

        public MainWindow()
        {
            InitializeComponent();
            _libraryDatabase = new LibraryDatabase("Library_db.db");
            LoadBooks();
        }

        private void LoadBooks(string searchQuery = "")
        {
            List<Book> books = _libraryDatabase.GetBooks(searchQuery);
            BooksDataGrid.ItemsSource = books;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchQuery = SearchTextBox.Text;
            LoadBooks(searchQuery);
        }

        private void RentButton_Click(object sender, RoutedEventArgs e)
        {
            if (BooksDataGrid.SelectedItem is Book selectedBook && selectedBook.IsAvailiable)
            {
                _libraryDatabase.RentBook(selectedBook.BookID, _currentUserID);
                MessageBox.Show($"Вы арендовали '{selectedBook.Title}'");
                LoadBooks();
            }
            else
            {
                MessageBox.Show("Выберете доступную книгу");
            }
        }
    }
}
