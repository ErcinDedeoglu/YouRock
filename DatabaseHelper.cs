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
            private SqlConnection SQLConnectionMaster { get; set; }
            //private SqlConnection SQLConnection { get; set; }
            private string DatabaseName { get; set; }

            public Executer(SqlConnection sqlConnectionMaster, string databaseName)
            {
                SQLConnectionMaster = sqlConnectionMaster;
                //SQLConnection = sqlConnection;
                DatabaseName = databaseName;
            }

            //public void ExecuteSql(string tSql)
            //{
            //    using (NPoco.IDatabase dbContext = new NPoco.Database(SQLConnectionMaster))
            //    {
            //        dbContext.Execute(tSql);
            //    }
            //}

            public void ExecuteSqlNormal(string tSql)
            {
                SqlCommand command = SQLConnectionMaster.CreateCommand();
                command.CommandText = tSql;
                command.ExecuteNonQuery();
            }

            public void CreateView(string viewName, string tSql)
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
                ", tSql, DatabaseName, viewName);

                ExecuteSqlNormal(script);
            }

            public void CreateStoredProcedure(string storedProcedureName, string tSql)
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
                ", tSql.Replace("'", "''''"), DatabaseName, storedProcedureName);

                ExecuteSqlNormal(script);
            }
            
            public bool IsTableExist(string tableName)
            {
                bool exist = false;

                var command = SQLConnectionMaster.CreateCommand();
                command.CommandText = "IF OBJECT_ID(N'" + DatabaseName + ".dbo." + tableName + "', N'U') IS NULL BEGIN SELECT 0 END ELSE BEGIN SELECT 1 END";

                using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (reader.Read())
                    {
                        if (reader[0].ToString() == "1")
                        {
                            exist = true;
                        }
                    }
                }

                return exist;
            }
        }

        public class DatabaseSynchronization
        {
            private Executer Executer { get; set; }
            private SqlConnection SQLConnectionMaster { get; set; }
            //private SqlConnection SQLConnection { get; set; }
            private string DatabaseName { get; set; }
            private string AgencyCode { get; set; }
            private DatabaseSyncLogAdapter _databaseSyncLogAdapter { get; set; }

            public DatabaseSynchronization(SqlConnection sqlConnectionMaster, string databaseName, string agencyCode)
            {
                SQLConnectionMaster = sqlConnectionMaster;
                DatabaseName = databaseName;
                AgencyCode = agencyCode;
                Executer = new Executer(sqlConnectionMaster, databaseName);
                _databaseSyncLogAdapter = new DatabaseSyncLogAdapter(sqlConnectionMaster, databaseName);

                ExecuteDatabaseCreateScript();
                ExecuteStoredProcedures();
                ExecuteChangeScripts();
                ExecuteInserts();
                ExecuteViews();
            }

            public class DatabaseSyncLogDto
            {
                public int DatabaseSyncLogID { get; set; }
                public int DatabaseSyncLogChangeScriptID { get; set; }
                public DateTime DatabaseSyncLogDate { get; set; }
            }

            public class DatabaseSyncLogAdapter
            {
                private SqlConnection SQLConnectionMaster { get; set; }
                private string DatabaseName { get; set; }
                private Executer Executer { get; set; }

                public DatabaseSyncLogAdapter(SqlConnection sqlConnectionMaster, string databaseName)
                {
                    SQLConnectionMaster = sqlConnectionMaster;
                    DatabaseName = databaseName;
                    Executer = new Executer(sqlConnectionMaster, databaseName);
                }

                public DatabaseSyncLogDto GetLastDatabaseSyncLog()
                {
                    DatabaseSyncLogDto databaseSyncLog = null;

                    using (NPoco.IDatabase dbContext = new NPoco.Database(SQLConnectionMaster))
                    {
                        bool exist = Executer.IsTableExist("tbl_DatabaseSyncLog");
                        if (exist)
                        {
                            //databaseSyncLog = dbContext.FirstOrDefault<DatabaseSyncLogDto>("USE " + DatabaseName + " SELECT * FROM tbl_DatabaseSyncLog Order By DatabaseSyncLogID Desc");
                        }
                    }

                    return databaseSyncLog;
                }

                public DatabaseSyncLogDto InsertDatabaseSyncLog(DatabaseSyncLogDto databaseSyncLog)
                {
                    using (NPoco.IDatabase dbContext = new NPoco.Database(SQLConnectionMaster))
                    {
                        dbContext.Insert(DatabaseName + ".dbo.tbl_DatabaseSyncLog", "DatabaseSyncLogID", databaseSyncLog);
                    }

                    return databaseSyncLog;
                }
            }


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

            public List<Tuple<int, string>> InsertScripts()
            {
                List<Tuple<int, string>> resultTuples = new List<Tuple<int, string>>();
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string realPath = path + "Database\\Inserts\\";

                if (Directory.Exists(realPath))
                {
                    List<string> fileList = Directory.GetFiles(realPath, "*.sql").ToList();

                    string agencyPath = path + "Database\\Inserts\\" + AgencyCode + "\\";

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

            public List<string> ViewScripts()
            {
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string realPath = path + "Database\\Views\\";
                List<string> result = new List<string>();

                if (Directory.Exists(realPath))
                {
                    string agencyPath = path + "Database\\Views\\" + AgencyCode + "\\";

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

            public List<string> StoredProcedureScripts()
            {
                List<string> result = new List<string>();
                string path = AppDomain.CurrentDomain.BaseDirectory;
                string realPath = path + "Database\\StoredProcedures\\";

                if (Directory.Exists(realPath))
                {
                    result = Directory.GetFiles(path + "Database\\StoredProcedures\\", "*.sql").ToList();

                    string agencyPath = path + "Database\\StoredProcedures\\" + AgencyCode + "\\";

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

            public void ExecuteDatabaseCreateScript()
            {
                try
                {
                    Executer.ExecuteSqlNormal("IF db_id('" + DatabaseName + "') IS NULL BEGIN CREATE DATABASE " + DatabaseName + " COLLATE SQL_Latin1_General_CP1_CI_AS END");

                    Executer.ExecuteSqlNormal(string.Format(@"USE {0} IF OBJECT_ID(N'dbo.tbl_DatabaseSyncLog', N'U') IS NULL
                        BEGIN
                            CREATE TABLE[dbo].[tbl_DatabaseSyncLog](
                            [DatabaseSyncLogID][int] IDENTITY(1, 1) NOT NULL,
                            [DatabaseSyncLogChangeScriptID][int] NOT NULL,
                            [DatabaseSyncLogDate][datetime] NOT NULL DEFAULT(GETUTCDATE()),
                                CONSTRAINT[PK_tbl_DatabaseSyncLog] PRIMARY KEY CLUSTERED([DatabaseSyncLogID] ASC)
                                WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY]
                        END", DatabaseName));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("EXCEPTION: ExecuteDatabaseCreateScript - " + ex.Message);
                    Console.ReadLine();
                }
            }

            public void ExecuteChangeScripts()
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
                                        DatabaseSyncLogDto databaseSyncLog = _databaseSyncLogAdapter.GetLastDatabaseSyncLog();

                                        if (databaseSyncLog == null || changeScriptID > databaseSyncLog.DatabaseSyncLogChangeScriptID)
                                        {
                                            string agencyShortName = null;
                                            if (fileNameArray.Count() == 3)
                                            {
                                                agencyShortName = fileNameArray[1];
                                            }

                                            if (agencyShortName == AgencyCode || agencyShortName == null)
                                            {
                                                string tSql = File.ReadAllText(info.FullName);

                                                if (!string.IsNullOrWhiteSpace(tSql))
                                                {
                                                    tSql = "USE " + DatabaseName + Environment.NewLine + tSql;
                                                    Executer.ExecuteSqlNormal(tSql);
                                                }


                                                _databaseSyncLogAdapter.InsertDatabaseSyncLog(new DatabaseSyncLogDto()
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

            public void ExecuteInserts()
            {
                IOrderedEnumerable<Tuple<int, string>> insertScripts = InsertScripts().OrderBy(a => a.Item1);

                foreach (Tuple<int, string> insertScript in insertScripts)
                {
                    FileInfo info = new FileInfo(insertScript.Item2);
                    if (info.Exists)
                    {
                        if (info.Extension == ".sql")
                        {
                            //string fileName = info.Name.Replace(info.Extension, "");

                            string tSql = File.ReadAllText(info.FullName);

                            if (!string.IsNullOrWhiteSpace(tSql))
                            {
                                try
                                {
                                    tSql = "USE " + DatabaseName + Environment.NewLine + tSql;
                                    Executer.ExecuteSqlNormal(tSql);
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

            public void ExecuteViews()
            {
                try
                {
                    List<string> viewScripts = ViewScripts();

                    foreach (string viewScript in viewScripts)
                    {
                        FileInfo info = new FileInfo(viewScript);

                        if (info.Exists)
                        {
                            if (info.Extension == ".sql")
                            {
                                string fileName = info.Name.Replace(info.Extension, "");

                                string tSql = File.ReadAllText(info.FullName);
                                tSql = tSql.Replace("  ", " ");

                                if (!string.IsNullOrWhiteSpace(tSql))
                                {
                                    Executer.CreateView(fileName, tSql);
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

            public void ExecuteStoredProcedures()
            {
                List<string> storedProcedureScripts = StoredProcedureScripts();

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
                                Executer.CreateStoredProcedure(fileName, readText);
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

        public static string BuildConnectionString(string dataSource = null, bool? integratedSecurity = null, string initialCatalog = null, string userID = null, string password = null, bool multipleActiveResultSets = false)
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

            builder.MultipleActiveResultSets = multipleActiveResultSets;

            return builder.ConnectionString;
        }

    }
}