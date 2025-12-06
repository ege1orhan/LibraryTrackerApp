namespace LibraryTrackerApp.Models;

public class Member
{
    public int MemberID { get; set; }
    public int AccountID { get; set; } // FK to Account
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
