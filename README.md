Library Tracker App

This is a simple desktop application for tracking library books, members, and borrow records.

Requirements

Windows 10 or later

.NET 10.0 Runtime

SQL Server Express LocalDB

Setup Instructions

Copy the project folder
Place the folder anywhere on your computer. For example:

C:\LibraryApp\


Install SQL Server LocalDB
If not installed, download and install from Microsoft’s website.

Create the LocalDB instance
Open Command Prompt and run:

sqllocaldb create "LibraryAppInstance"
sqllocaldb start "LibraryAppInstance"


Run the database script
Execute the SQL script to create the database and tables:

sqlcmd -S (localdb)\LibraryAppInstance -i "C:\LibraryApp\sql.sql"


Make sure to adjust the path to where you placed the folder.

Run the application
Double-click LibraryTrackerApp.exe inside the folder.

Licensing

This project uses EPPlus for Excel export/import.

EPPlus is licensed under NonCommercial, meaning this app cannot be sold or used commercially without a commercial license.

Optional

Create a shortcut for easier access.

Ensure all DLLs in the folder stay together with the .exe file.

Notes

This is a self-contained project; there is no installer.

Ensure your user account has permission to access LocalDB and the folder location.

The app connects to the LocalDB instance named LibraryAppInstance.
