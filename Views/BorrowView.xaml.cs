using System;
using System.Data;
using System.Windows;
using LibraryTrackerApp.Services;
using Microsoft.Data.SqlClient;

namespace LibraryTrackerApp.Views
{
    public partial class BorrowView : Window
    {
        private readonly SqlService _sql;

        public BorrowView()
        {
            InitializeComponent();
            _sql = new SqlService();

            LoadMembers();
            LoadBooks();
            LoadBorrowedBooks();
        }

        private async void LoadMembers()
        {
            DataTable dt = await _sql.QueryAsync("SELECT Id, FirstName + ' ' + LastName AS Name FROM Members");
            MemberComboBox.ItemsSource = dt.DefaultView;
            MemberComboBox.DisplayMemberPath = "Name";
            MemberComboBox.SelectedValuePath = "Id"; // <- Important
        }

        private async void LoadBooks()
        {
            DataTable dt = await _sql.QueryAsync("SELECT Id, Title FROM Books");
            BookComboBox.ItemsSource = dt.DefaultView;
            BookComboBox.DisplayMemberPath = "Title";
            BookComboBox.SelectedValuePath = "Id"; // <- Important
        }

        private async void LoadBorrowedBooks()
        {
            string query = @"
                SELECT b.Id, bk.Title AS BookTitle, m.FirstName + ' ' + m.LastName AS MemberName,
                       b.BorrowDate, b.DueDate
                FROM BorrowRecords b
                JOIN Books bk ON b.BookId = bk.Id
                JOIN Members m ON b.MemberId = m.Id
                WHERE b.ReturnDate IS NULL
                ORDER BY b.BorrowDate DESC";

            DataTable dt = await _sql.QueryAsync(query);
            BorrowedGrid.ItemsSource = dt.DefaultView;
        }

        private async void BorrowBook_Click(object sender, RoutedEventArgs e)
        {
            if (MemberComboBox.SelectedValue == null || BookComboBox.SelectedValue == null || DueDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select a member, book, and due date.");
                return;
            }

            int memberId = Convert.ToInt32(MemberComboBox.SelectedValue);
            int bookId = Convert.ToInt32(BookComboBox.SelectedValue);
            DateTime borrowDate = DateTime.Now;
            DateTime dueDate = DueDatePicker.SelectedDate.Value;

            // Prevent duplicate borrow if not returned
            string checkQuery = @"SELECT COUNT(*) FROM BorrowRecords 
                                  WHERE BookId=@BookId AND MemberId=@MemberId AND ReturnDate IS NULL";

            object result = await _sql.ExecuteScalarAsync(checkQuery,
                new SqlParameter("@BookId", bookId),
                new SqlParameter("@MemberId", memberId));

            int activeBorrows = Convert.ToInt32(result ?? 0);
            if (activeBorrows > 0)
            {
                MessageBox.Show("This member already has this book borrowed. Cannot borrow again until it is returned.");
                return;
            }

            string insertQuery = @"INSERT INTO BorrowRecords (BookId, MemberId, BorrowDate, DueDate) 
                                   VALUES (@BookId, @MemberId, @BorrowDate, @DueDate)";

            await _sql.ExecuteAsync(insertQuery,
                new SqlParameter("@BookId", bookId),
                new SqlParameter("@MemberId", memberId),
                new SqlParameter("@BorrowDate", borrowDate),
                new SqlParameter("@DueDate", dueDate));

            MessageBox.Show("Book borrowed successfully!");
            LoadBorrowedBooks();
        }

        private async void ReturnBook_Click(object sender, RoutedEventArgs e)
        {
            if (BorrowedGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select a borrowed book to return.");
                return;
            }

            DataRowView row = BorrowedGrid.SelectedItem as DataRowView;
            if (row == null) return;

            int borrowId = Convert.ToInt32(row["Id"]);

            string query = "UPDATE BorrowRecords SET ReturnDate = @ReturnDate WHERE Id = @Id";

            await _sql.ExecuteAsync(query,
                new SqlParameter("@ReturnDate", DateTime.Now),
                new SqlParameter("@Id", borrowId));

            MessageBox.Show("Book returned successfully!");
            LoadBorrowedBooks();
        }
    }
}
