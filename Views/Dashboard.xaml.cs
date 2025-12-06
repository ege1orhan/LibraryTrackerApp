using System.Windows;
using LibraryTrackerApp.ViewModels;

namespace LibraryTrackerApp.Views
{
    public partial class Dashboard : Window
    {
        public Dashboard()
        {
            InitializeComponent();
            DataContext = new DashboardViewModel(); // Bind the ViewModel
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
