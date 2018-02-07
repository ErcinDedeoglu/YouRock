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
            //public void ExecuteSql(string tSql)
            //{
            //    using (NPoco.IDatabase dbContext = new NPoco.Database(SQLConnectionMaster))
            //    {
            //        dbContext.Execute(tSql);
            //    }
            //}

            public static void ExecuteSqlNormal(string tSql)
            {
                SqlCommand command = DTO.CommonStatic.Database.SQLConnectionMaster.CreateCommand();
                command.CommandText = tSql;
                command.ExecuteNonQuery();
            }

            public static void CreateView(string viewName, string tSql)
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
                ", tSql, DTO.CommonStatic.Database.DatabaseName, viewName);

                ExecuteSqlNormal(script);
            }

            public static void CreateStoredProcedure(string storedProcedureName, string tSql)
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
                ", tSql.Replace("'", "''''"), DTO.CommonStatic.Database.DatabaseName, storedProcedureName);

                ExecuteSqlNormal(script);
            }
            
            public static bool IsTableExist(string tableName)
            {
                bool exist = false;

                var command = DTO.CommonStatic.Database.SQLConnectionMaster.CreateCommand();
                command.CommandText = "IF OBJECT_ID(N'" + DTO.CommonStatic.Database.DatabaseName + ".dbo." + tableName + "', N'U') IS NULL BEGIN SELECT 0 END ELSE BEGIN SELECT 1 END";

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
            public DatabaseSynchronization()
            {
                if (YouRock.DTO.CommonStatic.Database.SQLConnectionMaster.State == ConnectionState.Closed)
                {
                    YouRock.DTO.CommonStatic.Database.SQLConnectionMaster.Open();
                }

                ExecuteDatabaseCreateScript();
                ExecuteStoredProcedures();
                ExecuteChangeScripts();
                ExecuteInserts();
                ExecuteViews();

                if (YouRock.DTO.CommonStatic.Database.SQLConnectionMaster.State == ConnectionState.Closed)
                {
                    YouRock.DTO.CommonStatic.Database.SQLConnection.Open();
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

                    string agencyPath = path + "Database\\Inserts\\" + DTO.CommonStatic.Database.AgencyCode + "\\";

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
                    string agencyPath = path + "Database\\Views\\" + DTO.CommonStatic.Database.AgencyCode + "\\";

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

                    string agencyPath = path + "Database\\StoredProcedures\\" + DTO.CommonStatic.Database.AgencyCode + "\\";

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
                    Executer.ExecuteSqlNormal("IF db_id('" + DTO.CommonStatic.Database.DatabaseName + "') IS NULL BEGIN CREATE DATABASE " + DTO.CommonStatic.Database.DatabaseName + " COLLATE SQL_Latin1_General_CP1_CI_AS END");

                    Executer.ExecuteSqlNormal(string.Format(@"USE {0} IF OBJECT_ID(N'dbo.tbl_DatabaseSyncLog', N'U') IS NULL
                        BEGIN
                            CREATE TABLE[dbo].[tbl_DatabaseSyncLog](
                            [DatabaseSyncLogID][int] IDENTITY(1, 1) NOT NULL,
                            [DatabaseSyncLogChangeScriptID][int] NOT NULL,
                            [DatabaseSyncLogDate][datetime] NOT NULL DEFAULT(GETUTCDATE()),
                                CONSTRAINT[PK_tbl_DatabaseSyncLog] PRIMARY KEY CLUSTERED([DatabaseSyncLogID] ASC)
                                WITH(PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON[PRIMARY]) ON[PRIMARY]
                        END", DTO.CommonStatic.Database.DatabaseName));

                    Executer.ExecuteSqlNormal(string.Format(@"USE {0} IF OBJECT_ID(N'dbo.tbl_ErrorLog', N'U') IS NULL
                        BEGIN
	                        CREATE TABLE [DBO].[tbl_ErrorLog](
	                        [ErrorLogID] [INT] IDENTITY(1,1) NOT NULL,
	                        [ErrorLogGUID] [uniqueidentifier] NOT NULL,
	                        [ErrorLogParentGUID] [uniqueidentifier] NULL,
	                        [ErrorLogLineNumber] [INT] NULL,
	                        [ErrorLogMethod] [NVARCHAR](MAX) NULL,
	                        [ErrorLogLineMethod] [NVARCHAR](MAX) NULL,
	                        [ErrorLogMessage] [NVARCHAR](MAX) NULL,
	                        [ErrorLogHResult] [INT] NULL,
	                        [ErrorLogStackTrace] [NVARCHAR](MAX) NULL,
	                        [ErrorLogHelpLink] [NVARCHAR](MAX) NULL,
	                        [ErrorLogSource] [NVARCHAR](MAX) NULL,
	                        [ErrorLogDate] [DATETIME] NOT NULL,
	                        CONSTRAINT [PK_tbl_ErrorLog] PRIMARY KEY CLUSTERED ([ErrorLogID] ASC)
	                        WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY])
	                        ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
                        END", DTO.CommonStatic.Database.DatabaseName));
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
                                        DTO.Database.DatabaseSyncLogDto databaseSyncLog = Database.Adapter.DatabaseSyncLogAdapter.GetLastDatabaseSyncLog();

                                        if (databaseSyncLog == null || changeScriptID > databaseSyncLog.DatabaseSyncLogChangeScriptID)
                                        {
                                            string agencyShortName = null;
                                            if (fileNameArray.Count() == 3)
                                            {
                                                agencyShortName = fileNameArray[1];
                                            }

                                            if (agencyShortName == DTO.CommonStatic.Database.AgencyCode || agencyShortName == null)
                                            {
                                                string tSql = File.ReadAllText(info.FullName);

                                                if (!string.IsNullOrWhiteSpace(tSql))
                                                {
                                                    tSql = "USE " + DTO.CommonStatic.Database.DatabaseName + Environment.NewLine + tSql;
                                                    Executer.ExecuteSqlNormal(tSql);
                                                }


                                                Database.Adapter.DatabaseSyncLogAdapter.InsertDatabaseSyncLog(new DTO.Database.DatabaseSyncLogDto()
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
                                    tSql = "USE " + DTO.CommonStatic.Database.DatabaseName + Environment.NewLine + tSql;
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