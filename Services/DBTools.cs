using Microsoft.Data.SqlClient;
using System;

namespace LibraryWPF.Services
{
    public static class DBTools
    {
        private static string _dbName = "LibraryDB";
        private static bool _isCreateNewDb = false;

        public static string DBName
        {
            get => _dbName;
            set => _dbName = string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("Имя БД не может быть пустым")
                : value;
        }

        public static bool IsCreateNewDb
        {
            get => _isCreateNewDb;
            set => _isCreateNewDb = value;
        }

        public static string ConnectionString =>
            $"Server=.;Database={DBName};Trusted_Connection=True;TrustServerCertificate=True;";

        public static SqlConnection CreateConnection() => new SqlConnection(ConnectionString);

        public static bool TestConnection(string dbName)
        {
            try
            {
                using (var testConnection = new SqlConnection(
                    $"Server=.;Database={dbName};Trusted_Connection=True;TrustServerCertificate=True;"))
                {
                    testConnection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}