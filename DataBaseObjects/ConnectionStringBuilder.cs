using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Text;

namespace DataBaseObjects
{
    public static class ConnectionStringBuilders
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Functions specific to Excel Workbook connections
        /// </summary>
        /// *  Connection Strings: https://www.connectionstrings.com/access/
        public static class ExcelWorkbooks
        {
            /// <exception cref="ArgumentException"/>
            /// <exception cref="System.IO.FileNotFoundException"/>
            private static void ValidateWorkbookPath(string workbookPath)
            {
                if (workbookPath.IsNullOrEmpty()) throw new ArgumentException("WorkBookPath has no value");
                if (!System.IO.Path.IsPathRooted(workbookPath)) throw new ArgumentException("WorkBookPath is not rooted!");
                if (!System.IO.Path.HasExtension(workbookPath)) throw new ArgumentException("WorkBookPath does not have an extension!");
                if (!System.IO.File.Exists(workbookPath)) throw new System.IO.FileNotFoundException($"Workbook does not exist at specified location! - Path: \n {workbookPath}");
            }

            /// <summary>
            /// Generate the <see cref="OleDbConnection"/> to the specified <paramref name="workbookPath"/>
            /// </summary>
            /// <param name="workbookPath">path to the workbook</param>
            /// <param name="hasHeaders">treat the first row as table headers</param>
            /// <returns></returns>
            /// <inheritdoc cref="ValidateWorkbookPath(string)"/>
            public static OleDbConnection GetConnection(string workbookPath, bool? hasHeaders = null)
            {
                ValidateWorkbookPath(workbookPath);
                if (System.IO.Path.GetExtension(workbookPath) == ".xlsx" || System.IO.Path.GetExtension(workbookPath) == ".xlsm")
                    return GetACEConnection(workbookPath, hasHeaders);
                else
                    return GetJETConnection(workbookPath, hasHeaders);
            }

            /// <inheritdoc cref="GetConnection" />
            public static OleDbConnection GetACEConnection(string workbookPath, bool? hasHeaders = null)
            {
                ValidateWorkbookPath(workbookPath);
                string HDR = hasHeaders is null ? string.Empty : (bool)hasHeaders ? ";HDR=Yes" : ";HDR=No";
                return new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + workbookPath + ";Extended Properties=\"Excel 12.0" + HDR + ";IMEX=0\"");
            }

            /// <inheritdoc cref="GetConnection" />
            public static OleDbConnection GetJETConnection(string workbookPath, bool? hasHeaders = null)
            {
                ValidateWorkbookPath(workbookPath);
                string HDR = hasHeaders is null ? string.Empty : (bool)hasHeaders ? ";HDR=Yes" : ";HDR=No";
                return new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + workbookPath + ";Extended Properties=\"Excel 8.0" + HDR + ";IMEX=0\"");
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        

        /// <summary>
        /// Functions specific to Microsoft Access Database connections
        /// </summary>
        /// *  Connection Strings: https://www.connectionstrings.com/access/
        public static class AccessDB
        {
            /// <summary>
            /// Generate a new OLEDB.JET database connection string
            /// </summary>
            /// <param name="path">path to the database</param>
            /// <param name="dbPassword">Password to the database</param>
            /// <returns>new <see cref="OleDbConnection"/></returns>
            /// <exception cref="ArgumentException"/>
            public static string GenerateJetConnectionString(string path, string dbPassword = default)
            {
                if (path.IsNullOrEmpty()) throw new ArgumentException("Path has no value");
                if (!System.IO.Path.IsPathRooted(path)) throw new ArgumentException("Path is not rooted!");
                if (!System.IO.Path.HasExtension(path)) throw new ArgumentException("Path does not have an extension!");

                string Conn = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source= {path} ; Persist Security Info = false ;";
                if (dbPassword.IsNotEmpty()) Conn += $" Jet OLEDB:Database Password={dbPassword};";
                return  Conn;
            }

            /// <summary>
            /// Generate a new OLEDB.ACE database connection string
            /// </summary>
            /// <param name="path">path to the database</param>
            /// <param name="dbPassword">Password to the database</param>
            /// <returns>new <see cref="OleDbConnection"/></returns>
            /// <exception cref="ArgumentException"/>
            public static string GenerateACEConnectionString(string path, string dbPassword = default)
            {
                if (path.IsNullOrEmpty()) throw new ArgumentException("Path has no value");
                if (!System.IO.Path.IsPathRooted(path)) throw new ArgumentException("Path is not rooted!");
                if (!System.IO.Path.HasExtension(path)) throw new ArgumentException("Path does not have an extension!");

                string Conn = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source= {path} ; Persist Security Info = false ;";
                if (dbPassword.IsNotEmpty()) Conn += $" Jet OLEDB:Database Password={dbPassword};";
                return Conn;
            }

            /// <summary>
            /// Generate a new OLEDB.ACE database connection
            /// </summary>
            /// <inheritdoc cref="GenerateACEConnectionString"/>
            public static OleDbConnection GetACEConnection(string path, string dbPassword = default)
            {
                return new OleDbConnection(GenerateACEConnectionString(path,dbPassword));
            }

            /// <summary>
            /// Generate a new OLEDB.ACE database connection
            /// </summary>
            /// <inheritdoc cref="GenerateJetConnectionString"/>
            public static OleDbConnection GetJetConnection(string path, string dbPassword = default)
            {
                return new OleDbConnection(GenerateJetConnectionString(path, dbPassword));
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        

        /// <summary>
        /// Functions related to <see cref="SqlConnection"/>
        /// </summary>
        ///  *  Connection Strings: 
        ///  *  https://www.connectionstrings.com/sql-server/
        ///  *  https://www.connectionstrings.com/sqlite/
        public static class SqlConnections
        {
            /// <summary>
            /// Generates a new <see cref="SqlConnectionStringBuilder"/> with the provided parameters - meant for communicating with SQLServer
            /// </summary>
            /// <param name="serverAddress">Server Location to reach the db</param>
            /// <param name="dataSource">database name</param>
            /// <param name="userID"></param>
            /// <param name="password"></param>
            /// <param name="encrypt"></param>
            /// <returns></returns>
            public static SqlConnectionStringBuilder GenerateSqlServerConnectionString(string serverAddress, string dataSource, string userID = "", string password = "", bool encrypt = false)
            {
                var bldr = new SqlConnectionStringBuilder();
                bldr["Server"] = serverAddress;
                bldr.DataSource = dataSource;
                bldr.ConnectRetryCount = 3;
                bldr.Encrypt = encrypt;
                bldr.UserID = userID;
                bldr.Password = password;
                return bldr;
            }

            /// <summary>
            /// Generates a new <see cref="SqlConnectionStringBuilder"/> with the provided parameters - meant for communicating with Sql-Lite
            /// </summary>
            /// <param name="dataSource">path to the database</param>
            /// <param name="encrypt"></param>
            /// <returns></returns>
            public static SqlConnectionStringBuilder GenerateSqlLiteConnectionString(string dataSource)
            {
                var bldr = new SqlConnectionStringBuilder();
                bldr.DataSource = dataSource;
                bldr.ConnectRetryCount = 3;
                return bldr;
            }

            /// <summary>
            /// Generate a new <see cref="SqlConnection"/>
            /// </summary>
            /// <param name="path">path to the database</param>
            /// <param name="password"></param>
            /// <returns>new <see cref="OleDbConnection"/></returns>
            public static SqlConnection GenerateSqlConnection(string connectionString, SqlCredential credentials = null)
            {
                return credentials is null ? new SqlConnection(connectionString) : new SqlConnection(connectionString, credentials);
            }

            /// <summary>
            /// Generate a new <see cref="SqlConnection"/>
            /// </summary>
            /// <param name="path">path to the database</param>
            /// <param name="password"></param>
            /// <returns>new <see cref="OleDbConnection"/></returns>
            public static SqlConnection GenerateSqlConnection(SqlConnectionStringBuilder connectionString, SqlCredential credentials = null)
            {
                return credentials is null ? new SqlConnection(connectionString.ConnectionString) : new SqlConnection(connectionString.ConnectionString, credentials);
            }
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        

    }
}
