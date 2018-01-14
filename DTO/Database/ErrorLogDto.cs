using System;
using NPoco;

namespace YouRock.DTO.Database
{
    [TableName("tbl_ErrorLog")]
    [PrimaryKey("ErrorLogID")]
    public class ErrorLogDto
    {
        public int ErrorLogID { get; set; }
        public Guid ErrorLogGUID { get; set; }
        public Guid? ErrorLogParentGUID { get; set; }
        public int? ErrorLogLineNumber { get; set; }
        public string ErrorLogMethod { get; set; }
        public string ErrorLogLineMethod { get; set; }
        public string ErrorLogMessage { get; set; }
        public int? ErrorLogHResult { get; set; }
        public string ErrorLogStackTrace { get; set; }
        public string ErrorLogHelpLink { get; set; }
        public string ErrorLogSource { get; set; }
        public DateTime ErrorLogDate { get; set; }
    }
}