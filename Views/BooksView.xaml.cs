using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;
using LibraryTrackerApp.Services;

namespace LibraryTrackerApp.Views
{
    public partial class BooksView : Window
    {
        private readonly SqlService _sql;

        public BooksView()
        {
            InitializeComponent();

            // Use the parameterless SqlService
            _sql = new SqlService();

            LoadBooks();
        }

        private async void LoadBooks()
        {
            DataTable dt = await _sql.QueryAsync("SELECT * FROM Books");
            BooksGrid.ItemsSource = dt.DefaultView;
        }

        private async void AddBook_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text.Trim();
            string author = AuthorTextBox.Text.Trim();
            string isbn = ISBNTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author))
            {
                MessageBox.Show("Title and Author are required!");
                return;
            }

            await _sql.ExecuteAsync(
                "INSERT INTO Books (Title, Author, ISBN) VALUES (@Title, @Author, @ISBN)",
                new SqlParameter("@Title", title),
                new SqlParameter("@Author", author),
                new SqlParameter("@ISBN", isbn)
            );

            TitleTextBox.Text = "";
            AuthorTextBox.Text = "";
            ISBNTextBox.Text = "";

            LoadBooks();
        }
    }
}
