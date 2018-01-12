using System;

namespace YouRock.DTO.Database
{
    public class DatabaseSyncLogDto
    {
        public int DatabaseSyncLogID { get; set; }
        public int DatabaseSyncLogChangeScriptID { get; set; }
        public DateTime DatabaseSyncLogDate { get; set; }
    }
}