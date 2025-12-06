using System;

namespace LibraryTrackerApp.Models;

public class BorrowRecord
{
    public int BorrowID { get; set; }
    public int MemberID { get; set; } // FK to Member
    public int BookID { get; set; }   // FK to Book
    public DateTime BorrowDate { get; set; } = DateTime.Now;
    public DateTime DueDate { get; set; }
    public DateTime? ReturnedDate { get; set; } // nullable
    public decimal DailyOverdueAmount { get; set; } = 0;
    public int GetDaysOverdue() =>
    ReturnedDate.HasValue
        ? Math.Max(0, (ReturnedDate.Value - DueDate).Days)
        : Math.Max(0, (DateTime.Now - DueDate).Days);

    public decimal GetTotalDebt() => GetDaysOverdue() * DailyOverdueAmount;

}
