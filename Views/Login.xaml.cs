using System.Windows;
using LibraryTrackerApp.Services;
using Microsoft.Data.SqlClient;

namespace LibraryTrackerApp.Views
{
    public partial class Login : Window
    {
        private readonly SqlService _sql;

        public Login()
        {
            InitializeComponent();
            _sql = new SqlService();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter username and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string passwordHash = PasswordService.HashPassword(password);

                var result = await _sql.QueryAsync(
                    "SELECT Id, Username FROM Users WHERE Username=@Username AND PasswordHash=@PasswordHash",
                    new SqlParameter("@Username", username),
                    new SqlParameter("@PasswordHash", passwordHash));

                if (result.Rows.Count > 0)
                {
                    MessageBox.Show($"Login successful!\nWelcome {username}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Open Dashboard
                    Dashboard dashboard = new Dashboard();
                    dashboard.Show();

                    // Close Login window
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Invalid username or password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            Register registerWindow = new Register();
            registerWindow.ShowDialog(); // opens the Register window modally
        }
    }
}
