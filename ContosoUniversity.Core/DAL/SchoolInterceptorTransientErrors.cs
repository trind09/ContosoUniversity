using System;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Data.SqlClient;
using System.Reflection;
using System.Linq;
using ContosoUniversity.Core.Logging;
using System.IO;
using System.Text.RegularExpressions;

namespace ContosoUniversity.Core.DAL
{
    public class SchoolInterceptorTransientErrors : DbCommandInterceptor
    {
        private int _counter = 0;
        private ILogger _logger = new Logger();

        public override void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
        {
            bool throwTransientErrors = false;
            if (command.Parameters.Count > 0 && command.Parameters[0].Value.ToString() == "%Throw%")
            {
                throwTransientErrors = true;
                command.Parameters[0].Value = "%an%";
                command.Parameters[1].Value = "%an%";
            }

            if (throwTransientErrors && _counter < 4)
            {
                _logger.Information("Returning transient error for command: {0}", command.CommandText);
                _counter++;
                interceptionContext.Exception = CreateDummySqlException();
                WriteLog(interceptionContext.Exception);
            }
        }

        private SqlException CreateDummySqlException()
        {
            // The instance of SQL Server you attempted to connect to does not support encryption
            var sqlErrorNumber = 20;

            var sqlErrorCtor = typeof(SqlError).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Where(c => c.GetParameters().Count() == 7).Single();
            var sqlError = sqlErrorCtor.Invoke(new object[] { sqlErrorNumber, (byte)0, (byte)0, "", "", "", 1 });

            var errorCollection = Activator.CreateInstance(typeof(SqlErrorCollection), true);
            var addMethod = typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.Instance | BindingFlags.NonPublic);
            addMethod.Invoke(errorCollection, new[] { sqlError });

            var sqlExceptionCtor = typeof(SqlException).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic).Where(c => c.GetParameters().Count() == 4).Single();
            var sqlException = (SqlException)sqlExceptionCtor.Invoke(new object[] { "Dummy", errorCollection, null, Guid.NewGuid() });

            return sqlException;
        }

        private void WriteLog(Exception e)
        {
            MethodBase site = e.TargetSite;//Get the methodname from the exception.
            string methodName = site == null ? "" : site.Name;//avoid null ref if it's null.
            methodName = ExtractBracketed(methodName);

            System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(e, true);
            string className = "";
            int lineNum = 0;
            int colNum = 0;
            if (trace.FrameCount > 0)
            {
                className = trace.GetFrame(0).GetMethod().ReflectedType.FullName;
                lineNum = trace.GetFrame(0).GetFileLineNumber();
                colNum = trace.GetFrame(0).GetFileColumnNumber();
            }

            string path = System.AppDomain.CurrentDomain.BaseDirectory;
            File.AppendAllText(path + "log.txt", "Exception: " + className + "." + methodName + ", Ln " + lineNum + " Col " + colNum + ": " + e.Message + Environment.NewLine);
        }

        private static string ExtractBracketed(string str)
        {
            string s;
            if (str.IndexOf('<') > -1) //using the Regex when the string does not contain <brackets> returns an empty string.
                s = Regex.Match(str, @"\<([^>]*)\>").Groups[1].Value;
            else
                s = str;
            if (s == "")
                return "'Emtpy'"; //for log visibility we want to know if something it's empty.
            else
                return s;

        }
    }
}