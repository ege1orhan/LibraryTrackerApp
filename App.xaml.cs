using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Data.SqlClient;

namespace LibraryTrackerApp
{
    public partial class App : Application
    {
        private const string DbName = "LibraryDB";
        private const string ConnectionString = @"Server=(localdb)\MSSQLLocalDB;Integrated Security=true;";

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            EnsureDatabase().Wait();
        }

        private async Task EnsureDatabase()
        {
            string checkDbQuery = $"SELECT database_id FROM sys.databases WHERE name = '{DbName}'";

            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                await conn.OpenAsync();

                SqlCommand checkCmd = new SqlCommand(checkDbQuery, conn);
                object? result = await checkCmd.ExecuteScalarAsync();

                // Database does NOT exist → Create it + run the SQL script
                if (result == null)
                {
                    SqlCommand createCmd = new SqlCommand($"CREATE DATABASE {DbName}", conn);
                    await createCmd.ExecuteNonQueryAsync();

                    await RunSqlScript();
                }
            }
        }

        private async Task RunSqlScript()
        {
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sql.sql");

            if (!File.Exists(scriptPath))
            {
                MessageBox.Show("SQL script not found: " + scriptPath);
                return;
            }

            string script = await File.ReadAllTextAsync(scriptPath);

            string dbConn = $@"Server=(localdb)\MSSQLLocalDB;Database={DbName};Integrated Security=true;";

            using SqlConnection conn = new SqlConnection(dbConn);
            await conn.OpenAsync();

            SqlCommand cmd = new SqlCommand(script, conn);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
