using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Collections.Generic;
using RFBCodeWorks.DataBaseObjects.Exceptions;
using System.Threading.Tasks;
using SqlKata;
using System.Runtime.CompilerServices;

namespace RFBCodeWorks.DataBaseObjects
{
    /// <summary>
    /// Methods for interacting with databases
    /// </summary>
    public static partial class DBOps
    {
        #region < Helper Methods  >

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
            Func<bool> func = new Func<bool>(() => TestConnection(Conn));
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
        [System.Diagnostics.DebuggerHidden]
        public static bool GetValueAsBool(IDbConnection DB, Query query, SqlKata.Compilers.Compiler compiler) =>
            Extensions.ConvertToBool(DBOps.GetValue(DB, query, compiler));


        /// <returns> string </returns>
        /// <inheritdoc cref="GetValue(IDbConnection, Query, SqlKata.Compilers.Compiler)"/>
        [System.Diagnostics.DebuggerHidden]
        public static string GetValueAsString(IDbConnection DB, Query query, SqlKata.Compilers.Compiler compiler) =>
            Extensions.ConvertToString(DBOps.GetValue(DB, query, compiler));

        /// <returns> int </returns>
        /// <inheritdoc cref="GetValue(IDbConnection, Query, SqlKata.Compilers.Compiler)"/>
        [System.Diagnostics.DebuggerHidden]
        public static int GetValueAsInt(IDbConnection DB, Query query, SqlKata.Compilers.Compiler compiler) =>
            Extensions.ConvertToInt(DBOps.GetValue(DB, query, compiler));

        /// <summary>Return a single value from a DB Table</summary>
        /// <param name="DB">Some DB Connection</param>
        /// <param name="tableName">Table to query</param>
        /// <param name="lookupColName">Column to to find a value in</param>
        /// <param name="lookupVal">Value to find in supplied column</param>
        /// <param name="returnColName">Return the value from this column</param>
        /// <param name="compiler">The SqlKata Compiler to use to compile the query</param>
        /// <returns><see cref="IDataReader"/>.GetValue() object. -- sanitizes <see cref="DBNull"/> to null. </returns>
        public static object GetValue(IDbConnection DB, string tableName, string lookupColName, object lookupVal, string returnColName, SqlKata.Compilers.Compiler compiler)
        {
            SqlKata.Query qry = new SqlKata.Query().Select(returnColName).From(tableName).Where(lookupColName, lookupVal);
            return GetValue(DB, qry, compiler ?? throw new ArgumentNullException(nameof(compiler)));
        }

        /// <returns>True/False</returns>
        /// <inheritdoc cref="GetValue(IDbConnection, string, string, object, string, SqlKata.Compilers.Compiler)"/>
        [System.Diagnostics.DebuggerHidden]
        public static bool GetValueAsBool(IDbConnection DB, string tableName, string lookupColName, string lookupVal, string returnColName, SqlKata.Compilers.Compiler compiler) =>
            Extensions.ConvertToBool(DBOps.GetValue(DB, tableName, lookupColName, lookupVal, returnColName, compiler));


        /// <returns> string </returns>
        /// <inheritdoc cref="GetValue(IDbConnection, string, string, object, string, SqlKata.Compilers.Compiler)"/>
        [System.Diagnostics.DebuggerHidden]
        public static string GetValueAsString(IDbConnection DB, string tableName, string lookupColName, string lookupVal, string returnColName, SqlKata.Compilers.Compiler compiler) =>
            Extensions.ConvertToString(DBOps.GetValue(DB, tableName, lookupColName, lookupVal, returnColName, compiler));

        /// <returns> int </returns>
        /// <inheritdoc cref="GetValue(IDbConnection, string, string, object, string, SqlKata.Compilers.Compiler)"/>
        [System.Diagnostics.DebuggerHidden]
        public static int GetValueAsInt(IDbConnection DB, string tableName, string lookupColName, string lookupVal, string returnColName, SqlKata.Compilers.Compiler compiler) =>
            Extensions.ConvertToInt(DBOps.GetValue(DB, tableName, lookupColName, lookupVal, returnColName, compiler));


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

