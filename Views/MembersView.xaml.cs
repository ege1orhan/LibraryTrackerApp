using System;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;
using LibraryTrackerApp.Services;

namespace LibraryTrackerApp.Views
{
    public partial class MembersView : Window
    {
        private readonly SqlService _sql;

        public MembersView()
        {
            InitializeComponent();
            _sql = new SqlService();
            LoadMembers();
        }

        // Load Members into the DataGrid
        private async void LoadMembers()
        {
            // Select Id as MemberID and combine first + last name
            DataTable dt = await _sql.QueryAsync(
                "SELECT Id AS MemberID, FirstName + ' ' + LastName AS FullName, Email FROM Members"
            );
            MembersGrid.ItemsSource = dt.DefaultView;
        }

        // Add new member
        private async void AddMember_Click(object sender, RoutedEventArgs e)
        {
            string fullName = NameTextBox.Text.Trim();
            string email = EmailTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Full Name and Email are required!");
                return;
            }

            // Split full name into first and last
            string[] parts = fullName.Split(' ', 2); // at most 2 parts
            string firstName = parts[0];
            string lastName = parts.Length > 1 ? parts[1] : "";

            // Insert into database
            await _sql.ExecuteAsync(
                "INSERT INTO Members (FirstName, LastName, Email) VALUES (@FirstName, @LastName, @Email)",
                new SqlParameter("@FirstName", firstName),
                new SqlParameter("@LastName", lastName),
                new SqlParameter("@Email", email)
            );

            // Clear input fields
            NameTextBox.Text = "";
            EmailTextBox.Text = "";

            // Refresh grid
            LoadMembers();
        }
    }
}
