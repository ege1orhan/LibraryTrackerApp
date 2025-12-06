using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LibraryTrackerApp.Services;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using OfficeOpenXml;
using System.IO;
using LibraryTrackerApp.Views; // For opening other windows

namespace LibraryTrackerApp.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly SqlService _sql;

        // Dashboard stats
        private int _totalBooks;
        public int TotalBooks { get => _totalBooks; set { _totalBooks = value; OnPropertyChanged(); } }

        private int _totalMembers;
        public int TotalMembers { get => _totalMembers; set { _totalMembers = value; OnPropertyChanged(); } }

        private int _borrowedBooks;
        public int BorrowedBooks { get => _borrowedBooks; set { _borrowedBooks = value; OnPropertyChanged(); } }

        private int _overdueBooks;
        public int OverdueBooks { get => _overdueBooks; set { _overdueBooks = value; OnPropertyChanged(); } }

        // Borrowed items list
        public ObservableCollection<BorrowedItem> BorrowedList { get; } = new();

        // Commands
        public ICommand RefreshCommand { get; }
        public ICommand AddBookCommand { get; }
        public ICommand AddMemberCommand { get; }
        public ICommand BorrowReturnCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand SettingsCommand { get; }

        // Constructor
        public DashboardViewModel()
        {
            _sql = new SqlService();

            RefreshCommand = new RelayCommand(async _ => await RefreshDashboardAsync());
            AddBookCommand = new RelayCommand(_ => OpenBooksView());
            AddMemberCommand = new RelayCommand(_ => OpenMembersView());
            BorrowReturnCommand = new RelayCommand(_ => OpenBorrowView());
            ExportCommand = new RelayCommand(async _ => await ExportDataAsync());
            ImportCommand = new RelayCommand(async _ => await ImportDataAsync());

            // Settings button
            SettingsCommand = new RelayCommand(_ =>
            {
                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.Show();
            });
        }

        // Open other windows
        private void OpenBooksView() => new BooksView().Show();
        private void OpenMembersView() => new MembersView().Show();
        private void OpenBorrowView() => new BorrowView().Show();

        // Refresh dashboard data
        public async Task RefreshDashboardAsync()
        {
            TotalBooks = Convert.ToInt32(await _sql.ExecuteScalarAsync("SELECT COUNT(*) FROM Books"));
            TotalMembers = Convert.ToInt32(await _sql.ExecuteScalarAsync("SELECT COUNT(*) FROM Members"));
            BorrowedBooks = Convert.ToInt32(await _sql.ExecuteScalarAsync(
                "SELECT COUNT(*) FROM BorrowRecords WHERE ReturnDate IS NULL"));
            OverdueBooks = Convert.ToInt32(await _sql.ExecuteScalarAsync(
                "SELECT COUNT(*) FROM BorrowRecords WHERE ReturnDate IS NULL AND DueDate < GETDATE()"));

            DataTable dt = await _sql.QueryAsync(@"
                SELECT b.Id, bk.Title AS BookTitle, m.FirstName + ' ' + m.LastName AS MemberName,
                       b.BorrowDate, b.DueDate,
                       CASE WHEN b.ReturnDate IS NULL AND b.DueDate < GETDATE() THEN 'Overdue'
                            WHEN b.ReturnDate IS NULL THEN 'Borrowed'
                            ELSE 'Returned' END AS Status
                FROM BorrowRecords b
                JOIN Books bk ON b.BookId = bk.Id
                JOIN Members m ON b.MemberId = m.Id
                ORDER BY b.BorrowDate DESC");

            BorrowedList.Clear();
            foreach (DataRow row in dt.Rows)
            {
                if (row["Status"].ToString() != "Returned")
                {
                    BorrowedList.Add(new BorrowedItem
                    {
                        Id = Convert.ToInt32(row["Id"]),
                        BookTitle = row["BookTitle"].ToString()!,
                        MemberName = row["MemberName"].ToString()!,
                        BorrowDate = Convert.ToDateTime(row["BorrowDate"]),
                        DueDate = Convert.ToDateTime(row["DueDate"]),
                        Status = row["Status"].ToString()!
                    });
                }
            }
        }

        // Export data to Excel (fixed duplicate worksheet issue)
        public async Task ExportDataAsync()
        {
            ExcelPackage.License.SetNonCommercialPersonal("LibraryApp");

            DataTable books = await _sql.QueryAsync("SELECT * FROM Books");
            DataTable members = await _sql.QueryAsync("SELECT * FROM Members");
            DataTable borrowed = await _sql.QueryAsync("SELECT * FROM BorrowRecords");

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = "LibraryData.xlsx"
            };
            if (sfd.ShowDialog() != true) return;

            using var package = new ExcelPackage(new FileInfo(sfd.FileName));

            void AddOrReplaceWorksheet(string sheetName, DataTable data)
            {
                var ws = package.Workbook.Worksheets[sheetName];
                if (ws != null)
                    package.Workbook.Worksheets.Delete(ws);

                package.Workbook.Worksheets.Add(sheetName).Cells["A1"].LoadFromDataTable(data, true);
            }

            AddOrReplaceWorksheet("Books", books);
            AddOrReplaceWorksheet("Members", members);
            AddOrReplaceWorksheet("BorrowRecords", borrowed);

            package.Save();
        }

        // Import data from Excel
        public async Task ImportDataAsync()
        {
            ExcelPackage.License.SetNonCommercialPersonal("LibraryApp");

            OpenFileDialog ofd = new OpenFileDialog { Filter = "Excel Files (*.xlsx)|*.xlsx" };
            if (ofd.ShowDialog() != true) return;

            using var package = new ExcelPackage(new FileInfo(ofd.FileName));

            // Books
            var wsBooks = package.Workbook.Worksheets["Books"];
            if (wsBooks != null)
                for (int row = 2; row <= wsBooks.Dimension.End.Row; row++)
                    if (!string.IsNullOrWhiteSpace(wsBooks.Cells[row, 2].Text))
                        await _sql.ExecuteAsync("INSERT INTO Books (Title) VALUES (@Title)",
                            new SqlParameter("@Title", wsBooks.Cells[row, 2].Text));

            // Members
            var wsMembers = package.Workbook.Worksheets["Members"];
            if (wsMembers != null)
                for (int row = 2; row <= wsMembers.Dimension.End.Row; row++)
                    if (!string.IsNullOrWhiteSpace(wsMembers.Cells[row, 2].Text) &&
                        !string.IsNullOrWhiteSpace(wsMembers.Cells[row, 3].Text))
                        await _sql.ExecuteAsync("INSERT INTO Members (FirstName, LastName) VALUES (@FirstName, @LastName)",
                            new SqlParameter("@FirstName", wsMembers.Cells[row, 2].Text),
                            new SqlParameter("@LastName", wsMembers.Cells[row, 3].Text));

            // BorrowRecords
            var wsBorrowed = package.Workbook.Worksheets["BorrowRecords"];
            if (wsBorrowed != null)
                for (int row = 2; row <= wsBorrowed.Dimension.End.Row; row++)
                    if (int.TryParse(wsBorrowed.Cells[row, 2].Text, out int bookId) &&
                        int.TryParse(wsBorrowed.Cells[row, 3].Text, out int memberId) &&
                        DateTime.TryParse(wsBorrowed.Cells[row, 4].Text, out DateTime borrowDate) &&
                        DateTime.TryParse(wsBorrowed.Cells[row, 5].Text, out DateTime dueDate))
                    {
                        await _sql.ExecuteAsync(
                            "INSERT INTO BorrowRecords (BookId, MemberId, BorrowDate, DueDate) VALUES (@BookId, @MemberId, @BorrowDate, @DueDate)",
                            new SqlParameter("@BookId", bookId),
                            new SqlParameter("@MemberId", memberId),
                            new SqlParameter("@BorrowDate", borrowDate),
                            new SqlParameter("@DueDate", dueDate));
                    }

            await RefreshDashboardAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Borrowed item model
    public class BorrowedItem
    {
        public int Id { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
