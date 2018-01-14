using System.Data.SqlClient;

namespace YouRock.DTO
{
    public class CommonStatic
    {
        public class Database
        {
            public static SqlConnection SQLConnectionMaster { get; set; }
            public static SqlConnection SQLConnection { get; set; }
            public static string SQLConnectionString { get; set; }
            public static string DatabaseName { get; set; }
            public static string AgencyCode { get; set; }
        }
    }
}