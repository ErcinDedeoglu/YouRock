namespace YouRock.Database.Adapter
{
    public class DatabaseSyncLogAdapter
    {
        public static DTO.Database.DatabaseSyncLogDto GetLastDatabaseSyncLog()
        {
            DTO.Database.DatabaseSyncLogDto databaseSyncLog = null;

            using (NPoco.IDatabase dbContext = new NPoco.Database(DTO.CommonStatic.Database.SQLConnectionMaster))
            {
                bool exist = DatabaseHelper.Executer.IsTableExist("tbl_DatabaseSyncLog");
                if (exist)
                {
                    //databaseSyncLog = dbContext.FirstOrDefault<DatabaseSyncLogDto>("USE " + DatabaseName + " SELECT * FROM tbl_DatabaseSyncLog Order By DatabaseSyncLogID Desc");
                }
            }

            return databaseSyncLog;
        }

        public static DTO.Database.DatabaseSyncLogDto InsertDatabaseSyncLog(DTO.Database.DatabaseSyncLogDto databaseSyncLog)
        {
            using (NPoco.IDatabase dbContext = new NPoco.Database(DTO.CommonStatic.Database.SQLConnectionMaster))
            {
                dbContext.Insert(DTO.CommonStatic.Database.DatabaseName + ".dbo.tbl_DatabaseSyncLog", "DatabaseSyncLogID", databaseSyncLog);
            }

            return databaseSyncLog;
        }
    }
}