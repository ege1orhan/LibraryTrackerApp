using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Threading.Tasks;

namespace LibraryTrackerApp.Services
{
    public class SqlService
    {
        // Connection string pointing directly to your SQLEXPRESS instance
        private readonly string _connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog = LibraryApp; Integrated Security = True; Connect Timeout = 30; Encrypt=True;Trust Server Certificate=False;Application Intent = ReadWrite; Multi Subnet Failover=False;Command Timeout = 30;";

        public SqlService()
        {
            // No need to pass connection string from outside
        }

        // GENERAL QUERY (SELECT)
        public async Task<DataTable> QueryAsync(string query, params SqlParameter[] parameters)
        {
            var table = new DataTable();

            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            table.Load(reader);

            return table;
        }

        // GENERAL COMMAND (INSERT, UPDATE, DELETE)
        public async Task<int> ExecuteAsync(string query, params SqlParameter[] parameters)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        // RETURN SINGLE VALUE
        public async Task<object?> ExecuteScalarAsync(string query, params SqlParameter[] parameters)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, conn);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            await conn.OpenAsync();
            return await cmd.ExecuteScalarAsync();
        }
    }
}
