using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Collections.Generic;
using DataBaseObjects.Exceptions;
using System.Threading.Tasks;
using SqlKata;

namespace DataBaseObjects
{
    /// <summary>
    /// Methods for interacting with databases
    /// </summary>
    public static partial class DBOps
    {
        #region < Helper Methods  >

        /// <inheritdoc cref="ObjectExtensions.ConvertToString{T}(T, IFormatProvider?)"/>
        private static string CStr(object obj) => obj.ConvertToString();
        
        /// <inheritdoc cref="ObjectExtensions.ConvertToInt{T}(T)"/>
        private static int ConvertToInt(object obj) => obj?.ConvertToInt() ?? 0;
        
        /// <inheritdoc cref="ObjectExtensions.ConvertToBool(object?)"/>
        private static bool CBool(object obj) => obj?.ConvertToBool() ?? false;

        /// <summary>
        /// Check the <see cref="IDbConnection.State"/> and report if the connection is closed
        /// </summary>
        /// <param name="connection"></param>
        /// <returns>TRUE if the connection state is <see cref="ConnectionState.Closed"/></returns>
        public static bool IsClosed(this IDbConnection connection) => connection.State == ConnectionState.Closed;

        /// <summary>
        /// Checks the <see cref="IDbConnection.State"/> for <see cref="ConnectionState.Closed"/>, and if true opens the connection
        /// </summary>
        /// <param name="connection"></param>
        public static void OpenIfClosed(this IDbConnection connection) { if (connection.IsClosed()) connection.Open(); }

        /// <summary>
        /// Closes the connection, then opens it back up.
        /// </summary>
        /// <param name="connection"></param>
        public static void CloseAndReOpen(this IDbConnection connection) { 
            if (!connection.IsClosed()) connection.Close();
            connection.OpenIfClosed();
        }

        /// <summary>
        /// Evaluate the connection string and get the compiler to use for that type of connection
        /// </summary>
        /// <param name="connection"></param>
        /// <returns>
        /// If <see cref="OleDbConnection"/> : returns <see cref="SqlKata.Compilers.CompilerSingletons.MSAccessCompiler"/>  or <see cref="SqlKata.Compilers.CompilerSingletons.ExcelWorkbookCompiler"/> <br/>
        /// If <see cref="SqlConnection"/>  : returns <see cref="SqlKata.Compilers.CompilerSingletons.SqlServerCompiler"/> or <see cref="SqlKata.Compilers.CompilerSingletons.SqliteCompiler"/>
        /// </returns>
        public static SqlKata.Compilers.Compiler GetCompilerForConnection(IDbConnection connection)
        {
            if (connection is OleDbConnection ole)
            {
                if (ole.DataSource.Contains(".xls")) return SqlKata.Compilers.CompilerSingletons.ExcelWorkbookCompiler; // Check if database has '.xls' in it to determine its an excel file
                return SqlKata.Compilers.CompilerSingletons.MSAccessCompiler;   // Assume Access Database
            }

            if (connection is SqlConnection sql)
            {
                if (connection.ConnectionString.StartsWith("Data Source") && sql.DataSource.EndsWith(".db"))
                    return SqlKata.Compilers.CompilerSingletons.SqliteCompiler;

                return SqlKata.Compilers.CompilerSingletons.SqlServerCompiler; // default functionality
            }
            throw new NotImplementedException($"{connection.GetType()} currently not supported by this method");
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        #region < TestConnection >

        /// <summary>
        /// Test an <see cref="IDbConnection"/> connection by opening then closing the connection
        /// </summary>
        /// <param name="Conn"></param>
        /// <returns>TRUE if connection was successfully opened then closed, otherwise false.</returns>
        public static bool TestConnection(this IDbConnection Conn)
        {
            bool result = false;
            try
            {
                using (Conn)
                {
                    Conn.Open();
                    if (Conn.State == System.Data.ConnectionState.Open) result = true;
                    Conn.Close();
                }
            }
            catch
            {

            }
            return result;
        }

        /// <inheritdoc cref="TestConnection(IDbConnection)"/>
        public static Task<bool> TestConnectionAsync(this IDbConnection Conn)
        {
            Func<bool> func = new(() => TestConnection(Conn));
            return Task<bool>.Run<bool>(func);
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region < Functions to Build DataTables>

        /// <summary>
        /// Fill up a datatable with the results of a database Call
        /// </summary>
        /// <param name="Conn">DataBase Connection String</param>
        /// <param name="Query">Pre-Built SQL Query to run against the database</param>
        /// <param name="disposeConnection">If TRUE, wraps the connection in a 'using' statement to automatically dispose of it after completing the command</param>
        /// <returns></returns>
        public static DataTable GetDataTable(IDbConnection Conn, string Query, bool disposeConnection = true)
        {
            try
            {
                Func<DataTable> getDT = () =>
                {
                    Conn.OpenIfClosed();
                    using (var Cmd = Conn.CreateCommand())
                    {
                        Cmd.CommandText = Query;
                        using (var DR = Cmd.ExecuteReader())
                        {
                            DataTable DT = new DataTable();
                            DT.Load(DR);
                            return DT;
                        }
                    }
                };

                if (disposeConnection)
                    using (Conn)
                        return getDT();
                else
                    return getDT();
            }
            catch (Exception E)
            {
                //E.AddVariableData(nameof(Conn), Conn.ConnectionString);
                E.AddVariableData(nameof(Query), Query);
                throw E;
            }
        }

        public static DataTable GetDataTable(IDatabase database, SqlKata.Query query)
        {
            return GetDataTable(database.GetDatabaseConnection(), database.Compiler.Compile(query).ToString());
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region < RunAction >

        /// <inheritdoc cref="IDbCommand.ExecuteNonQuery" />
        /// <param name="query">The query to run</param>
        /// <param name="Conn">The <see cref="IDbConnection"/> to run the query against</param>
        public static Task<int> RunAction(IDbConnection Conn, string query)
        {
            return Task.Run(() =>
           {
               try
               {
                   using (Conn)
                   {
                       Conn.OpenIfClosed();
                       using (var Cmd = Conn.CreateCommand())
                       {
                           Cmd.CommandText = query;
                           return Cmd.ExecuteNonQuery();
                       }
                   }
               }
               catch (Exception E)
               {
                   //E.AddVariableData(nameof(Conn), Conn.ConnectionString);
                   E.AddVariableData(nameof(query), query);
                   throw E;
               }
           });
        }

        /// <inheritdoc cref="IDbCommand.ExecuteNonQuery" />
        /// <param name="query">The query to run</param>
        /// <param name="database">The <see cref="IDatabase"/> to run the query against</param>
        public static Task<int> RunAction(IDatabase database, SqlKata.Query query)
            => RunAction(database.GetDatabaseConnection(), database.Compiler.Compile(query).ToString());

        /// <inheritdoc cref="IDbCommand.ExecuteNonQuery" />
        public static Task<int> RunAction(IDbConnection Conn, string Query, params object[] parameters)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (Conn)
                    {
                        Conn.OpenIfClosed();
                        using (var Cmd = Conn.CreateCommand())
                        {
                            Cmd.CommandText = Query;
                            foreach(object o in parameters)
                                Cmd.Parameters.Add(o);
                            return Cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception E)
                {
                    //E.AddVariableData(nameof(Conn), Conn.ConnectionString);
                    E.AddVariableData(nameof(Query), Query);
                    throw E;
                }
            });
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region < Get return from DataTables / DB Connections >
        
        /// <summary>
        /// Compile the <paramref name="query"/> and run it against the <paramref name="DB"/>
        /// </summary>
        /// <param name="DB">database connection</param>
        /// <param name="query">query designed to return a single value from the database</param>
        /// <param name="compiler">compiler used to compile the query into the SQL statement</param>
        /// <returns></returns>
        public static object GetValue(IDbConnection DB, Query query, SqlKata.Compilers.Compiler compiler)
        {
            using (DB)
            {
                if (DB.State == ConnectionState.Closed) DB.Open();
                var C = DB.CreateCommand();
                C.CommandText = compiler.Compile(query).ToString();
                using (var R = C.ExecuteReader())
                {
                    while (R.Read())
                    {
                        if (R.IsDBNull(0)) return null;
                        return R.GetValue(0);
                    }
                }
            }
            return null;
        }

        /// <returns>True/False</returns>
        /// <inheritdoc cref="GetValue(IDbConnection, Query, SqlKata.Compilers.Compiler)"/>
        public static bool GetValueAsBool(IDbConnection DB, Query query, SqlKata.Compilers.Compiler compiler) =>
            CBool(DBOps.GetValue(DB, query, compiler));


        /// <returns> string </returns>
        /// <inheritdoc cref="GetValue(IDbConnection, Query, SqlKata.Compilers.Compiler)"/>
        [System.Diagnostics.DebuggerHidden]
        public static string GetValueAsString(IDbConnection DB, Query query, SqlKata.Compilers.Compiler compiler) =>
            CStr(DBOps.GetValue(DB, query, compiler));

        /// <returns> int </returns>
        /// <inheritdoc cref="GetValue(IDbConnection, Query, SqlKata.Compilers.Compiler)"/>
        [System.Diagnostics.DebuggerHidden]
        public static int GetValueAsInt(IDbConnection DB, Query query, SqlKata.Compilers.Compiler compiler) =>
            ConvertToInt(DBOps.GetValue(DB, query, compiler));

        /// <summary>Return a single value from a DB Table</summary>
        /// <param name="DB">Some DB Connection</param>
        /// <param name="TableName">Table to query</param>
        /// <param name="LookupColName">Column to to find a value in</param>
        /// <param name="LookupVal">Value to find in supplied column</param>
        /// <param name="ReturnColName">Return the value from this column</param>
        /// <returns><see cref="IDataReader"/>.GetValue() object. -- sanitizes <see cref="DBNull"/> to null. </returns>
        public static object GetValue(IDbConnection DB, string TableName, string LookupColName, object LookupVal, string ReturnColName, SqlKata.Compilers.Compiler QueryCompiler = null)
        {
            SqlKata.Query qry = new SqlKata.Query().Select(ReturnColName).From(TableName).Where(LookupColName, LookupVal);
            return GetValue(DB, qry, QueryCompiler ?? GetCompilerForConnection(DB));
        }

        /// <returns>True/False</returns>
        /// <inheritdoc cref="GetValue(IDbConnection, string, string, object, string, SqlKata.Compilers.Compiler)"/>
        public static bool GetValueAsBool(IDbConnection DB, string TableName, string LookupColName, string LookupVal, string ReturnColName, SqlKata.Compilers.Compiler compiler = null) =>
            CBool(DBOps.GetValue(DB, TableName, LookupColName, LookupVal, ReturnColName, compiler));


        /// <returns> string </returns>
        /// <inheritdoc cref="GetValue(IDbConnection, string, string, object, string, SqlKata.Compilers.Compiler)"/>
        [System.Diagnostics.DebuggerHidden]
        public static string GetValueAsString(IDbConnection DB, string TableName, string LookupColName, string LookupVal, string ReturnColName, SqlKata.Compilers.Compiler compiler = null) =>
            CStr(DBOps.GetValue(DB, TableName, LookupColName, LookupVal, ReturnColName, compiler));

        /// <returns> int </returns>
        /// <inheritdoc cref="GetValue(IDbConnection, string, string, object, string, SqlKata.Compilers.Compiler)"/>
        [System.Diagnostics.DebuggerHidden]
        public static int GetValueAsInt(IDbConnection DB, string TableName, string LookupColName, string LookupVal, string ReturnColName, SqlKata.Compilers.Compiler compiler = null) =>
            ConvertToInt(DBOps.GetValue(DB, TableName, LookupColName, LookupVal, ReturnColName, compiler));


        #endregion < Get return from DataTables / DB Connections >

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region < IDbConnection Dictionaries >

        /// <summary>Create Dictionary of all rows of all from the query results. 1st column is Key, 2nd column is Value.</summary>
        [System.Diagnostics.DebuggerHidden]
        public static void BuildDictionary(out Dictionary<int, string> Dict, IDbConnection Conn, string Query) => DBOps.GetDataTable(Conn, Query).BuildDictionary(out Dict);

        /// <summary>Create Dictionary of all rows of all from the query results. 1st column is Key, 2nd column is Value.</summary>
        [System.Diagnostics.DebuggerHidden]
        public static void BuildDictionary(out Dictionary<string, string> Dict, IDbConnection Conn, string Query) => DBOps.GetDataTable(Conn, Query).BuildDictionary(out Dict);

        /// <summary>Create Dictionary of all from the excel table. First Col in table is the Key, all other Cols are added to an array. </summary>
        //public static void BuildDictionary(out Dictionary<string, string[]> Dict, string ExcelWorkBookPath, string TableName) => BuildDictionary(out Dict, ExcelOps.GetDataTable(ExcelWorkBookPath, TableName));
        /// <summary>Create Dictionary of all from the query results. First Col in table is the Key, all other Cols are added to an array. </summary>
        public static void BuildDictionary(out Dictionary<string, string[]> Dict, IDbConnection Conn, string Query) => DBOps.GetDataTable(Conn, Query).BuildDictionary(out Dict);

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region < Update DataBase >

        /// <summary>Update the database</summary>
        /// <param name="Conn"></param>
        /// <param name="Tbl"></param>
        private static void UpdateDatabase(IDbConnection Conn, DataTable Tbl) { }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    }

}

