using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using LibraryTrackerApp.Services;
using LibraryTrackerApp.Views;
using Microsoft.Data.SqlClient;

namespace LibraryTrackerApp.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private string _username = "";
        private string _password = "";
        private readonly SqlService _sql;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            _sql = new SqlService();
            LoginCommand = new RelayCommand(async _ => await LoginAsync());
        }

        private async System.Threading.Tasks.Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Please enter username and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string passwordHash = PasswordService.HashPassword(Password);

                var result = await _sql.QueryAsync(
                    "SELECT Id, Username FROM Users WHERE Username=@Username AND PasswordHash=@PasswordHash",
                    new SqlParameter("@Username", Username),
                    new SqlParameter("@PasswordHash", passwordHash));

                if (result.Rows.Count > 0)
                {
                    MessageBox.Show($"Login successful!\nWelcome {Username}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Open Dashboard
                    var dashboard = new Dashboard();
                    dashboard.Show();

                    // Close Login window
                    foreach (Window window in Application.Current.Windows)
                    {
                        if (window is Login)
                        {
                            window.Close();
                            break;
                        }
                    }
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

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
