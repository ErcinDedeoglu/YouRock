using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using YouRock.DTO.Database;

namespace YouRock
{
    public class ExceptionHelper
    {
        public static int LineNumber(Exception ex)
        {
            int lineNumber = 0;
            const string lineSearch = ":line ";
            if (ex.StackTrace != null)
            {
                var index = ex.StackTrace.LastIndexOf(lineSearch, StringComparison.Ordinal);
                if (index != -1)
                {
                    int.TryParse(ex.StackTrace.Substring(index + lineSearch.Length), out lineNumber);
                }
            }

            return lineNumber;
        }

        public static List<ErrorLogDto> ErrorList(Exception ex)
        {
            List<DTO.Database.ErrorLogDto> errorLogList = new List<DTO.Database.ErrorLogDto>();

            try
            {
                string methodName = null;
                string errorLineMethodName = null;
                int lineNumber = ExceptionHelper.LineNumber(ex);

                StackFrame[] stackFrameList = new StackTrace(ex, true).GetFrames();
                if (stackFrameList != null && stackFrameList.Length > 1)
                {
                    MethodBase method = stackFrameList[1].GetMethod();

                    if (method != null)
                    {
                        methodName = method.Name;
                        if (method.DeclaringType != null)
                        {
                            methodName = method.DeclaringType.FullName + "." + methodName;
                        }
                    }

                    method = stackFrameList[0].GetMethod();


                    if (method != null)
                    {
                        errorLineMethodName = method.Name;
                        if (method.DeclaringType != null)
                        {
                            errorLineMethodName = method.DeclaringType.FullName + "." + errorLineMethodName;
                        }
                    }
                }
                
                while (ex != null)
                {
                    Guid? parentGUID = null;
                    if (errorLogList.Count > 0)
                    {
                        parentGUID = errorLogList[errorLogList.Count - 1].ErrorLogGUID;
                    }

                    DTO.Database.ErrorLogDto errorLog = new DTO.Database.ErrorLogDto()
                    {
                        ErrorLogGUID = Guid.NewGuid(),
                        ErrorLogParentGUID = parentGUID,
                        ErrorLogStackTrace = ex.StackTrace,
                        ErrorLogDate = DateTime.Now,
                        //ErrorLogHResult = ex.HResult,
                        ErrorLogHelpLink = ex.HelpLink,
                        ErrorLogMessage = ex.Message,
                        ErrorLogSource = ex.Source,
                        ErrorLogMethod = methodName,
                        ErrorLogLineNumber = lineNumber,
                        ErrorLogLineMethod = errorLineMethodName
                    };

                    errorLogList.Add(errorLog);
                    ex = ex.InnerException;
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return errorLogList;
        }


        public static void RecordErrorLog(Exception exception)
        {
            try
            {
                ThreadHelper.ExecuteThread(delegate
                {
                    try
                    {
                        List<DTO.Database.ErrorLogDto> errorLogList = ExceptionHelper.ErrorList(exception);
                        Database.Adapter.ErrorLogAdapter.InsertErrorLogList(errorLogList);
                    }
                    catch
                    {
                        // ignored
                    }
                });
            }
            catch (Exception ex)
            {
                // ignored
            }
        }
    }
}