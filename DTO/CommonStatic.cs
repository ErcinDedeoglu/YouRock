using System.Data.SqlClient;

namespace YouRock.DTO
{
    public class CommonStatic
    {
        public class Database
        {
            public static SqlConnection SQLConnectionMaster { get; set; }
            public static SqlConnection SQLConnection { get; set; }
            public static string SQLConnectionMasterString { get; set; }
            public static string SQLConnectionString { get; set; }
            public static string DatabaseServer { get; set; }
            public static string DatabaseName { get; set; }
            public static string DatabaseUsername { get; set; }
            public static string DatabasePassword { get; set; }
            public static string AgencyCode { get; set; }
            public static bool IntegratedSecurity { get; set; }

            public static void SetDatabase(string agencyCode, string databaseName, string databaseServer, bool integratedSecurity = true, string databaseUsername = null, string databasePassword = null)
            {
                AgencyCode = agencyCode;
                DatabaseName = databaseName;
                DatabaseServer = databaseServer;
                DatabaseUsername = databaseUsername;
                DatabasePassword = databasePassword;
                IntegratedSecurity = integratedSecurity;
                SQLConnectionString = YouRock.DatabaseHelper.BuildConnectionString(DatabaseServer, IntegratedSecurity, DatabaseName, DatabaseUsername, DatabasePassword, true);
                SQLConnectionMasterString = YouRock.DatabaseHelper.BuildConnectionString(DatabaseServer, IntegratedSecurity, null, DatabaseUsername, DatabasePassword, true);
                YouRock.DTO.CommonStatic.Database.SQLConnection = new SqlConnection(SQLConnectionString);
                YouRock.DTO.CommonStatic.Database.SQLConnectionMaster = new SqlConnection(SQLConnectionMasterString);
            }
        }
    }
}