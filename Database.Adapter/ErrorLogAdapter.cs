using System.Collections.Generic;

namespace YouRock.Database.Adapter
{
    public class ErrorLogAdapter
    {
        public static void InsertErrorLogList(List<DTO.Database.ErrorLogDto> errorLogList)
        {
            using (NPoco.IDatabase dbContext = new NPoco.Database(DTO.CommonStatic.Database.SQLConnection))
            {
                dbContext.InsertBulk(errorLogList);
            }
        }
    }
}