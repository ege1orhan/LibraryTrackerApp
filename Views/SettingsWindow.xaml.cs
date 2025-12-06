using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Data.SqlClient;
using LibraryTrackerApp.Services;
using System.Threading.Tasks;

namespace LibraryTrackerApp.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly SqlService _sql;

        public SettingsWindow()
        {
            InitializeComponent();
            _sql = new SqlService();
            LoadSettings();
        }

        private async void LoadSettings()
        {
            DataTable dt = await _sql.QueryAsync(
                "SELECT SettingKey, SettingValue FROM Settings WHERE SettingKey IN ('OverdueDays','OverdueFee')"
            );

            foreach (DataRow row in dt.Rows)
            {
                string? key = row["SettingKey"]?.ToString();
                string? value = row["SettingValue"]?.ToString();

                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value)) continue;

                if (key == "OverdueDays")
                    OverdueDaysTextBox.Text = value;
                else if (key == "OverdueFee")
                    OverdueFeeTextBox.Text = value;
            }
        }

        private async void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(OverdueDaysTextBox.Text, out int days) ||
                !decimal.TryParse(OverdueFeeTextBox.Text, out decimal fee))
            {
                MessageBox.Show("Enter valid numeric values.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await SaveSettingAsync("OverdueDays", days.ToString());
            await SaveSettingAsync("OverdueFee", fee.ToString());

            MessageBox.Show($"Settings saved!\nOverdue Days: {days}\nOverdue Fee: ${fee}", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }

        private async Task SaveSettingAsync(string key, string value)
        {
            await _sql.ExecuteAsync(
                @"IF EXISTS (SELECT 1 FROM Settings WHERE SettingKey = @Key)
                  UPDATE Settings SET SettingValue = @Value WHERE SettingKey = @Key
                  ELSE
                  INSERT INTO Settings (SettingKey, SettingValue) VALUES (@Key, @Value);",
                new SqlParameter("@Key", key),
                new SqlParameter("@Value", value)
            );
        }

        private void NumberOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private void DecimalOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]*(?:\.[0-9]*)?$");
        }
    }
}
