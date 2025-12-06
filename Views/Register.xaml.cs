using System.Windows;
using LibraryTrackerApp.Services;
using Microsoft.Data.SqlClient;

namespace LibraryTrackerApp.Views
{
    public partial class Register : Window
    {
        private readonly SqlService _sql;

        public Register()
        {
            InitializeComponent();
            _sql = new SqlService();
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // Check if username already exists
                var existing = await _sql.QueryAsync(
                    "SELECT Id FROM Users WHERE Username=@Username",
                    new SqlParameter("@Username", username));

                if (existing.Rows.Count > 0)
                {
                    MessageBox.Show("Username already exists. Choose a different one.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Hash password
                string passwordHash = PasswordService.HashPassword(password);

                // Insert new user
                await _sql.ExecuteAsync(
                    "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)",
                    new SqlParameter("@Username", username),
                    new SqlParameter("@PasswordHash", passwordHash));

                MessageBox.Show("Registration successful! You can now log in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Registration error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
