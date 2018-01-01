using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace YouRock
{
    public class DatabaseHelper
    {
        public class Executer
        {
            public void ExecuteSql(string sqlConnectionString, string tSql)
            {
                using (NPoco.IDatabase dbContext = new NPoco.Database(sqlConnectionString, NPoco.DatabaseType.SqlServer2012, null))
                {
                    dbContext.Execute(tSql);
                }
            }

            public void ExecuteSqlNormal(string sqlConnectionStringMaster, string tSql)
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionStringMaster))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = tSql;
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }

            public void CreateView(string sqlConnectionStringMaster, string databaseName, string viewName, string tSql)
            {
                string script = string.Format(@"                
                DECLARE @CreateViewStatement NVARCHAR(MAX) 
                SET @CreateViewStatement = '
                USE '+ QUOTENAME('{1}') +';
                IF EXISTS(select * FROM sys.views where name = ''{2}'')
                BEGIN
                    DROP VIEW {2};
                END
                
                EXEC(''{0}'')'
                EXEC (@CreateViewStatement)
                ", tSql, databaseName, viewName);

                ExecuteSqlNormal(sqlConnectionStringMaster, script);
            }

            public void CreateStoredProcedure(string sqlConnectionStringMaster, string databaseName, string storedProcedureName, string tSql)
            {
                string script = string.Format(@"                
                DECLARE @CreateStoredProcedureStatement NVARCHAR(MAX) 
                SET @CreateStoredProcedureStatement = '
                USE '+ QUOTENAME('{1}') +';
                IF (OBJECT_ID(''{2}'') IS NOT NULL)
                BEGIN
                    DROP PROCEDURE {2};
                END
                
                EXEC(''{0}'')'
                EXEC (@CreateStoredProcedureStatement)
                ", tSql.Replace("'", "''''"), databaseName, storedProcedureName);

                ExecuteSqlNormal(sqlConnectionStringMaster, script);
            }
            
            public bool IsTableExist(string sqlConnectionStringMaster, string databaseName, string tableName)
            {
                bool exist = false;

                using (SqlConnection connection = new SqlConnection(sqlConnectionStringMaster))
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "USE " + databaseName + " IF OBJECT_ID(N'dbo." + tableName + "', N'U') IS NULL BEGIN SELECT 0 END ELSE BEGIN SELECT 1 END";
                    using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        if (reader.Read())
                        {
                            string ss = reader[0].ToString();

                            if (ss == "1")
                            {
                                exist = true;
                            }
                        }
                    }

                    connection.Close();
                }

                return exist;
            }
        }

        public class DatabaseSynchronization
        {
            public class DatabaseSyncLogDto
            {
                public int DatabaseSyncLogID { get; set; }
                public int DatabaseSyncLogChangeScriptID { get; set; }
                public DateTime DatabaseSyncLogDate { get; set; }
            }

            public class DatabaseSyncLogAdapter
            {
                private readonly Executer _executer = new Executer();
                public DatabaseSyncLogDto GetLastDatabaseSyncLog(string sqlConnectionString, string databaseName)
                {
                    DatabaseSyncLogDto databaseSyncLog = null;

                    using (NPoco.IDatabase dbContext = new NPoco.Database(sqlConnectionString, NPoco.DatabaseType.SqlServer2012, null))
                    {
                        bool exist = _executer.IsTableExist(sqlConnectionString, databaseName, "tbl_DatabaseSyncLog");
                        if (exist)
                        {
                            databaseSyncLog = dbContext.FirstOrDefault<DatabaseSyncLogDto>("SELECT * FROM tbl_DatabaseSyncLog Order By DatabaseSyncLogID Desc");
                        }
                    }

                    return databaseSyncLog;
                }

                public DatabaseSyncLogDto InsertDatabaseSyncLog(string sqlConnectionString, DatabaseSyncLogDto databaseSyncLog)
                {
                    using (NPoco.IDatabase dbContext = new NPoco.Database(sqlConnectionString, NPoco.DatabaseType.SqlServer2012, null))
                    {
                        dbContext.Insert("tbl_DatabaseSyncLog", "DatabaseSyncLogID", databaseSyncLog);
                    }

                    return databaseSyncLog;
                }
            }

            private readonly Executer _executer = new Executer();
            private readonly DatabaseSyncLogAdapter _databaseSyncLogAdapter = new DatabaseSyncLogAdapter();

            public List<string> ChangeScripts()
            {
                List<string> result = new List<string>();
                string path = AppDomain.CurrentDomain.BaseDirectory + "Database\\Changes\\";

                if (Directory.Exists(path))
                {
                    result = Directory.GetFiles(path, "*.sql").ToList();
                }

                return result;
            }

            public List<Tuple<int, string>> InsertScripts(string agencyCode)
            {
                List<Tuple<int, string>> resultTuples = new List<Tuple<int, string>>();
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string realPath = path + "Database\\Inserts\\";

                if (Directory.Exists(realPath))
                {
                    List<string> fileList = Directory.GetFiles(realPath, "*.sql").ToList();

                    string agencyPath = path + "Database\\Inserts\\" + agencyCode + "\\";

                    if (Directory.Exists(agencyPath))
                    {
                        List<string> customFileList = Directory.GetFiles(agencyPath, "*.sql").ToList();

                        if (customFileList.Any())
                        {
                            fileList.AddRange(customFileList);
                        }
                    }

                    foreach (string file in fileList)
                    {
                        FileInfo fileInfo = new FileInfo(file);
                        if (fileInfo.Exists)
                        {
                            string[] arrFile = fileInfo.Name.Split('-');
                            if (arrFile.Length == 2)
                            {
                                int index;
                                if (int.TryParse(arrFile[0], out index))
                                {
                                    resultTuples.Add(new Tuple<int, string>(index, file));
                                }
                            }
                        }
                    }
                }

                return resultTuples;
            }

            public List<string> ViewScripts(string agencyCode)
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string realPath = path + "Database\\Views\\";
                List<string> result = new List<string>();

                if (Directory.Exists(realPath))
                {
                    string agencyPath = path + "Database\\Views\\" + agencyCode + "\\";

                    if (Directory.Exists(agencyPath))
                    {
                        List<string> customFileList = Directory.GetFiles(agencyPath, "*.sql").ToList();

                        if (customFileList.Any())
                        {
                            result.AddRange(customFileList);
                        }
                    }
                }

                return result;
            }

            public List<string> StoredProcedureScripts(string agencyCode)
            {
                List<string> result = new List<string>();
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string realPath = path + "Database\\StoredProcedures\\";

                if (Directory.Exists(realPath))
                {
                    result = Directory.GetFiles(path + "Database\\StoredProcedures\\", "*.sql").ToList();

                    string agencyPath = path + "Database\\StoredProcedures\\" + agencyCode + "\\";

                    if (Directory.Exists(agencyPath))
                    {
                        List<string> customFileList = Directory.GetFiles(agencyPath, "*.sql").ToList();

                        if (customFileList.Any())
                        {
                            result.AddRange(customFileList);
                        }
                    }
                }

                return result;
            }

            public void Start(string agencyCode, string sqlConnectionStringMaster, string databaseName)
            {
                Console.WriteLine("##Database Sync Started");
                Console.WriteLine(" #Database Sync: Execute Database Create Script");
                ExecuteDatabaseCreateScript(sqlConnectionStringMaster, databaseName);
                Console.WriteLine(" #Database Sync: Execute Stored Procedures");
                ExecuteStoredProcedures(agencyCode, sqlConnectionStringMaster, databaseName);
                Console.WriteLine(" #Database Sync: Execute Change Scripts");
                ExecuteChangeScripts(agencyCode, databaseName, sqlConnectionStringMaster);
                Console.WriteLine(" #Database Sync: Execute Inserts");
                ExecuteInserts(agencyCode, sqlConnectionStringMaster);
                Console.WriteLine(" #Database Sync: Execute Views");
                ExecuteViews(agencyCode, sqlConnectionStringMaster, databaseName);
                Console.WriteLine("##Database Sync Ended");
            }

            public void ExecuteDatabaseCreateScript(string sqlConnectionStringMaster, string databaseName)
            {
                try
                {
                    _executer.ExecuteSqlNormal(sqlConnectionStringMaster, "IF db_id('" + databaseName + "') IS NULL BEGIN CREATE DATABASE " + databaseName + " COLLATE SQL_Latin1_General_CP1_CI_AS END");

                    _executer.ExecuteSqlNormal(sqlConnectionStringMaster, string.Format(@"USE {0} IF OBJECT_ID(N'dbo.tbl_DatabaseSyncLog', N'U') IS NULL
                        BEGIN
                            CREATE TABLE[dbo].[tbl_DatabaseSyncLog](
                            [DatabaseSyncLogID][int] IDENTITY(1, 1) NOT NULL,
                            [DatabaseSyncLogChangeScriptID][int] NOT NULL,
                            [DatabaseSyncLogDate][datetime] NOT NULL DEFAULT(GETUTCDATE()),
                                CONSTRAINT[PK_tbl_DatabaseSyncLog] PRIMARY KEY CLUSTERED([DatabaseSyncLogID] ASC)
                                WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY]
                        END", databaseName));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EXCEPTION: ExecuteDatabaseCreateScript - " + ex.Message);
                    Console.ReadLine();
                }
            }

            public void ExecuteChangeScripts(string agencyCode, string databaseName, string sqlConnectionStringMaster)
            {
                try
                {
                    List<string> changeScripts = ChangeScripts();

                    foreach (string changeScript in changeScripts)
                    {
                        FileInfo info = new FileInfo(changeScript);
                        if (info.Exists)
                        {
                            if (info.Extension == ".sql")
                            {
                                string fileName = info.Name.Replace(info.Extension, "");
                                string[] fileNameArray = fileName.Split('.');

                                if (fileNameArray.Count() == 2 && fileNameArray[0] == "Change")
                                {
                                    int changeScriptID;
                                    if (int.TryParse(fileNameArray[1], out changeScriptID))
                                    {
                                        DatabaseSyncLogDto databaseSyncLog = _databaseSyncLogAdapter.GetLastDatabaseSyncLog(sqlConnectionStringMaster, databaseName);

                                        if (databaseSyncLog == null || changeScriptID > databaseSyncLog.DatabaseSyncLogChangeScriptID)
                                        {
                                            string agencyShortName = null;
                                            if (fileNameArray.Count() == 3)
                                            {
                                                agencyShortName = fileNameArray[1];
                                            }

                                            if (agencyShortName == agencyCode || agencyShortName == null)
                                            {
                                                string readText = File.ReadAllText(info.FullName);

                                                if (!string.IsNullOrWhiteSpace(readText))
                                                {
                                                    readText = "USE " + databaseName + Environment.NewLine + readText;
                                                    _executer.ExecuteSqlNormal(sqlConnectionStringMaster, readText);
                                                }

                                                _databaseSyncLogAdapter.InsertDatabaseSyncLog(sqlConnectionStringMaster, new DatabaseSyncLogDto()
                                                {
                                                    DatabaseSyncLogChangeScriptID = changeScriptID,
                                                    DatabaseSyncLogDate = DateTime.UtcNow
                                                });
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    throw new Exception("File name format is not correct!");
                                }
                            }
                            else
                            {
                                throw new Exception("File extension is different!");
                            }
                        }
                        else
                        {
                            throw new Exception("File not exist!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }

            public void ExecuteInserts(string agencyCode, string sqlConnectionString)
            {
                IOrderedEnumerable<Tuple<int, string>> insertScripts = InsertScripts(agencyCode).OrderBy(a => a.Item1);

                foreach (Tuple<int, string> insertScript in insertScripts)
                {
                    FileInfo info = new FileInfo(insertScript.Item2);
                    if (info.Exists)
                    {
                        if (info.Extension == ".sql")
                        {
                            //string fileName = info.Name.Replace(info.Extension, "");

                            string readText = File.ReadAllText(info.FullName);

                            if (!string.IsNullOrWhiteSpace(readText))
                            {
                                try
                                {
                                    _executer.ExecuteSql(sqlConnectionString, readText);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("EXCEPTION: ExecuteInserts - Insert:" + info.Name + " - Message: " + ex.Message);
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("File extension is different!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("File not exist!");
                    }
                }
            }

            public void ExecuteViews(string agencyCode, string sqlConnectionStringMaster, string databaseName)
            {
                try
                {
                    List<string> viewScripts = ViewScripts(agencyCode);

                    foreach (string viewScript in viewScripts)
                    {
                        FileInfo info = new FileInfo(viewScript);

                        if (info.Exists)
                        {
                            if (info.Extension == ".sql")
                            {
                                string fileName = info.Name.Replace(info.Extension, "");

                                string readText = File.ReadAllText(info.FullName);
                                readText = readText.Replace("  ", " ");

                                if (!string.IsNullOrWhiteSpace(readText))
                                {
                                    _executer.CreateView(sqlConnectionStringMaster, databaseName, fileName, readText);
                                }
                            }
                            else
                            {
                                throw new Exception("File extension is different!");
                            }
                        }
                        else
                        {
                            throw new Exception("File not exist!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EXCEPTION - ExecuteViews: " + ex.Message);
                }
            }

            public void ExecuteStoredProcedures(string agencyCode, string sqlConnectionStringMaster, string databaseName)
            {
                List<string> storedProcedureScripts = StoredProcedureScripts(agencyCode);

                foreach (string storedProcedureScript in storedProcedureScripts)
                {
                    FileInfo info = new FileInfo(storedProcedureScript);

                    if (info.Exists)
                    {
                        if (info.Extension == ".sql")
                        {
                            string fileName = info.Name.Replace(info.Extension, "");

                            string readText = File.ReadAllText(info.FullName);

                            if (!string.IsNullOrWhiteSpace(readText))
                            {
                                _executer.CreateStoredProcedure(sqlConnectionStringMaster, databaseName, fileName, readText);
                            }
                        }
                        else
                        {
                            throw new Exception("File extension is different!");
                        }
                    }
                    else
                    {
                        throw new Exception("File not exist!");
                    }
                }
            }
        }

        public static string BuildConnectionString(string dataSource = null, bool? integratedSecurity = null, string initialCatalog = null, string userID = null, string password = null)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();

            if (dataSource != null)
            {
                builder.DataSource = dataSource;
            }

            if (integratedSecurity != null)
            {
                builder.IntegratedSecurity = (bool)integratedSecurity;
            }

            if (initialCatalog != null)
            {
                builder.InitialCatalog = initialCatalog;
            }

            if (userID != null)
            {
                builder.UserID = userID;
            }

            if (password != null)
            {
                builder.Password = password;
            }

            return builder.ConnectionString;
        }

    }
}