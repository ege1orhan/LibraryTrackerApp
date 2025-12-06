namespace LibraryTrackerApp.Models;

public class Account
{
    public int AccountID { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirebaseUID { get; set; } = string.Empty; // UID from Firebase
    public string MembershipType { get; set; } = "Standard";
    public int MaxDevices { get; set; } = 1;
}
