using System;
using System.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using SqlKata;
using SqlKata.Compilers;
using System.Data.Common;
using System.Threading;
using System.Linq;

namespace RFBCodeWorks.DatabaseObjects
{
    /// <summary>
    /// Extension Methods for interacting with databases
    /// </summary>
    public static partial class DBOps
    {
        #region < Helper Methods  >

        /// <summary>
        /// Generate a new <see cref="KeyValuePair{TKey, TValue}"/> array that consists of a single pair
        /// </summary>
        public static KeyValuePair<T, O>[] CreateKeyValuePair<T, O>(T key, O value)
        {
            return new KeyValuePair<T, O>[] { new KeyValuePair<T, O>(key, value) };
        }

        /// <summary>
        /// Generate a new KeyValuePair array contains all the keys and values.
        /// <br/>The number of <paramref name="keys"/> and <paramref name="values"/> are expected to match.
        /// <br/>The first parameter will be the <paramref name="keys"/>. When used as an extension method, this will be the calling collection.
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <param name="keys">The keys that will be used when generating the new collection</param>
        /// <param name="values">The values that will be used</param>
        /// <param name="token">Optional token that can be used if this method is called within a Task. Not required.</param>
        public static KeyValuePair<T, O>[] CreateKeyValuePairs<T, O>(this IEnumerable<T> keys, IEnumerable<O> values, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var keyList = keys.ToArray();
            var valueList = values.ToArray();
            if (keyList.Length != valueList.Length) throw new ArgumentException("Cannot convert to KeyValuePair array - Number of keys does not match number of values");
            
            int count = keyList.Length;
            Dictionary<T, O> dic = new Dictionary<T, O>();
            if (token.CanBeCanceled)
            {
                for (int i = 0; i < count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    dic.Add(keyList[i], valueList[i]);
                }
                return dic.ToArray();
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    dic.Add(keyList[i], valueList[i]);
                }
                return dic.ToArray();
            }
        }

        /// <summary>
        /// Check the <see cref="IDbConnection.State"/> and report if the connection is closed
        /// </summary>
        /// <param name="connection"></param>
        /// <returns>TRUE if the connection state is <see cref="ConnectionState.Closed"/></returns>
        public static bool IsClosed(this IDbConnection connection) => connection.State == ConnectionState.Closed;

        /// <summary>
        /// Evaluates the <see cref="IDbConnection.State"/> and determines what to do when requesting to open the connection.
        /// </summary>
        /// <remarks>
        /// <see cref="ConnectionState.Connecting"/> is unexpected, and will return <see langword="false"/>.
        /// </remarks>
        /// <param name="connection">The connection to open</param>
        /// <returns>
        /// When this methods returns successfully, the database should be open for communication. Return values are for if this method opened the connection.
        /// <br/>- If this method opened the connection: return <see langword="true"/>
        /// <br/>- If the connection was already open: return <see langword="false"/>
        /// </returns>
        public static bool OpenSafely(this IDbConnection connection) 
        {
            switch (true)
            {
                //Broken - Must Re-Open
                case true when connection.State.HasFlag(ConnectionState.Broken):
                    if (connection.State != ConnectionState.Closed) connection.Close();
                    connection.Open();
                    return true;
                
                //Already Open Cases
                case true when connection.State.HasFlag(ConnectionState.Open):
                case true when connection.State.HasFlag(ConnectionState.Executing):
                case true when connection.State.HasFlag(ConnectionState.Fetching):
                    return false;

                case true when connection.State.HasFlag(ConnectionState.Connecting):
                    return false;

                // This is the expected default case - Open the connection and return
                default:
                case true when connection.State.HasFlag(ConnectionState.Closed):
                    connection.Open();
                    return true;
            }
        }

        /// <inheritdoc cref="OpenSafely(IDbConnection)"/>
        /// <inheritdoc cref="DbConnection.OpenAsync(CancellationToken)"/>
        public static async Task<bool> OpenSafelyAsync(this DbConnection connection, CancellationToken cancellationToken = default)
        {
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();
            switch (true)
            {
                //Broken - Must Re-Open
                case true when connection.State.HasFlag(ConnectionState.Broken):
                    if (connection.State != ConnectionState.Closed) connection.Close();
                    await connection.OpenAsync(cancellationToken);
                    return true;

                //Already Open Cases
                case true when connection.State.HasFlag(ConnectionState.Open):
                case true when connection.State.HasFlag(ConnectionState.Executing):
                case true when connection.State.HasFlag(ConnectionState.Fetching):
                    return false;

                //Wait until open - this case is unexpected
                case true when connection.State.HasFlag(ConnectionState.Connecting):
                    while (connection.State.HasFlag(ConnectionState.Connecting))
                    {
                        await Task.Delay(10, cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                    if (connection.State == ConnectionState.Broken)
                        return await OpenSafelyAsync(connection, cancellationToken);  // This allows reopening a broken connection
                    else
                        return false;

                // This is the expected default case - Open the connection and return
                default:
                case true when connection.State.HasFlag(ConnectionState.Closed):
                    await connection.OpenAsync(cancellationToken);
                    return true;
            }
        }

        #endregion

        

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region < TestConnection >

        /// <summary>
        /// Test an <see cref="IDbConnection"/> connection by briefly opening then closing the connection
        /// </summary>
        /// <param name="connection">The connection to test. Will be disposed of at the end of the test.</param>
        /// <param name="e">If the connection failed and exception was thrown, it will be returned here. If successful, this value will be null.</param>
        /// <returns>TRUE if connection was successfully opened then closed, otherwise false.</returns>
        public static bool TestConnection(this IDbConnection connection, out Exception e)
        {
            bool result = false;
            e = null;
            try
            {
                using (connection)
                {
                    connection.OpenSafely();
                    if (connection.State.HasFlag(System.Data.ConnectionState.Open)) result = true;
                    connection.Close();
                }
            }
            catch(Exception err)
            {
                e = err;
                result = false;
            }
            return result;
        }

        /// <inheritdoc cref="TestConnection(IDbConnection, out Exception)"/>
        public static bool TestConnection(this IDbConnection connection) => TestConnection(connection, out _);

        /// <param name="cancellationToken">Token to monitor while waiting for the connection. If not specified, uses <see cref="CancellationToken.None"/></param>
        /// <returns>
        /// If the test was successfull, return (<see langword="true"/>, <see langword="null"/>)
        /// <br/>If the test errored out, return (<see langword="false"/>, [The Exception that was thrown] )
        /// <br/>If the test cancelled, throw <see cref="OperationCanceledException"/>
        /// </returns>
        /// <inheritdoc cref="TestConnection(IDbConnection)"/>
        /// <param name="connection"/>
        /// <exception cref="OperationCanceledException"/>
        public static async Task<(bool ConnectionSuccess, Exception ConnectionError)> TestConnectionAsync(this DbConnection connection, CancellationToken cancellationToken= default)
        {
            bool result = false;
            Exception error = null;
            try
            {
                using (connection)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await connection.OpenSafelyAsync(cancellationToken);
                    if (connection.State.HasFlag(System.Data.ConnectionState.Open)) result = true;
                    connection.Close();
                }
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception err)
            {
                error = err;
                result = false;
            }
            return (result, error);
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region < Functions to Build DataTables>

        /// <summary>
        /// Fill up a datatable with the results of a database Call
        /// </summary>
        /// <param name="connection">Database Connection object</param>
        /// <param name="command">The command that will return the <see cref="DataTable"/></param>
        /// <returns></returns>
        public static DataTable GetDataTable(DbConnection connection, DbCommand command)
        {
            using (command)
            {
                var shouldClose = connection.OpenSafely();
                try
                {
                    using (var dr = command.ExecuteReader())
                    {
                        DataTable DT = new DataTable();
                        DT.Load(dr);
                        return DT;
                    }
                }
                catch (Exception e)
                {
                    e.AddVariableData("Query Text: ", command.CommandText);
                    foreach (IDbDataParameter p in command.Parameters)
                        e.AddVariableData($"Parameter ' {p.ParameterName} ': ", p.Value?.ToString() ?? "NULL VALUE");
                    throw;
                }
                finally
                {
                    if (shouldClose && connection.State != ConnectionState.Closed) connection.Close();
                }
            }
        }

        /// <inheritdoc cref="GetDataTable(DbConnection, DbCommand)"/>
        /// <inheritdoc cref="DBCommands.CreateCommand(DbConnection, string, KeyValuePair{string, object}[])"/>
        public static DataTable GetDataTable(DbConnection connection, string query, params KeyValuePair<string, object>[] parameters)
        {
            return GetDataTable(connection, connection.CreateCommand(query, parameters));
        }

        /// <summary>
        /// Fill up a datatable with the results of a database Call
        /// </summary>
        /// <param name="connection">Database Connection object</param>
        /// <param name="command">The command that will return the <see cref="DataTable"/></param>
        /// <param name="cancellationToken">The cancellationToken to use. If not specified, uses <see cref="CancellationToken.None"/></param>
        /// <returns></returns>
        /// <inheritdoc cref="GetDataTable(DbConnection, DbCommand)"/>
        public static async Task<DataTable> GetDataTableAsync(DbConnection connection, DbCommand command, CancellationToken cancellationToken = default)
        {
            using (command)
            {
                var shouldClose = connection.OpenSafely();
                try
                {
                    using (var dr = await command.ExecuteReaderAsync(cancellationToken))
                    {
                        DataTable DT = new DataTable();
                        DT.Load(dr);
                        return DT;
                    }
                }
                catch (OperationCanceledException) { throw; } //No logging required
                catch (Exception e)
                {
                    e.AddVariableData("Query Text: ", command.CommandText);
                    foreach (IDbDataParameter p in command.Parameters)
                        e.AddVariableData($"Parameter ' {p.ParameterName} ': ", p.Value?.ToString() ?? "NULL VALUE");
                    throw;
                }
                finally
                {
                    if (shouldClose && connection.State != ConnectionState.Closed) connection.Close();
                }
            }
        }

        /// <inheritdoc cref="GetDataTableAsync(DbConnection, DbCommand,CancellationToken)"/>
        /// <inheritdoc cref="DBCommands.CreateCommand(DbConnection, string, KeyValuePair{string, object}[])"/>
        public static Task<DataTable> GetDataTableAsync(DbConnection connection, string query, CancellationToken cancellationToken = default, params KeyValuePair<string, object>[] parameters)
        {
            return GetDataTableAsync(connection, connection.CreateCommand(query, parameters), cancellationToken);
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region < RunAction >

        /// <summary>
        /// Assign the <see cref="IDbCommand.Connection"/> to the <paramref name="connection"/>, then execute the command.
        /// </summary>
        /// <param name="connection">
        /// The database connection
        /// <br/> - This will open the <paramref name="connection"/> if it is closed, but will not close the connection. As such, the caller should be wrapping the <paramref name="connection"/> inside a <see langword="using"/> statment.
        /// </param>
        /// <param name="command">
        /// The <see cref="IDbCommand"/> to execute. 
        /// <br/> - The <paramref name="command"/> will be disposed of after it has run (this is wrapped inside a using statement).
        /// </param>
        /// <inheritdoc cref="IDbCommand.ExecuteNonQuery" />
        public static int RunAction(DbConnection connection, DbCommand command)
        {
            if (command is null) throw new ArgumentNullException(nameof(command));
            command.Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            var shouldClose = connection.OpenSafely();
            using (command)
            {
                try
                {
                    return command.ExecuteNonQuery();
                } 
                catch (Exception e)
                {
                    e.AddVariableData("Query Text: ", command.CommandText);
                    foreach (IDbDataParameter p in command.Parameters)
                        e.AddVariableData($"Parameter ' {p.ParameterName} ': ", p.Value?.ToString() ?? "NULL VALUE");
                    throw;
                }
                finally
                {
                    if (shouldClose && connection.State != ConnectionState.Closed) connection.Close();
                }
            }
        }

        /// <summary>
        /// Create a new <see cref="DbCommand"/> from the provided <paramref name="query"/> and <paramref name="parameters"/>, then run it via <see cref="RunAction(DbConnection, DbCommand)"/>
        /// </summary>
        /// <param name="query">The query string</param>
        /// <inheritdoc cref="RunAction(DbConnection, DbCommand)" />
        /// <inheritdoc cref="DBCommands.CreateCommand(DbConnection, string, KeyValuePair{string, object}[])"/>
        /// <param name="connection"/><param name="parameters"/>
        public static int RunAction(DbConnection connection, string query, params KeyValuePair<string, object>[] parameters)
        {
            DbCommand cmd = null;
            try{

                cmd = connection.CreateCommand(query, parameters);
                return RunAction(connection, cmd);
            }
            finally
            {
                cmd?.Dispose();
            }
        }

        /// <inheritdoc cref="RunAction(DbConnection, DbCommand)"/>
        /// <param name="command"/>
        /// <param name="connection"/>
        /// <param name="cancellationToken">An optional cancellation token. If not provided, uses <see cref="CancellationToken.None"/></param>
        public static async Task<int> RunActionAsync(DbConnection connection, DbCommand command, CancellationToken cancellationToken = default)
        {
            if (command is null) throw new ArgumentNullException(nameof(command));
            command.Connection = connection ?? throw new ArgumentNullException(nameof(connection));

            await connection.OpenSafelyAsync();
            using (command)
            {
                try
                {
                    return await command.ExecuteNonQueryAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw; // Operation was cancelled, no reason to log variable data
                }
                catch (Exception e)
                {
                    e.AddVariableData("Query Text: ", command.CommandText);
                    foreach (IDbDataParameter p in command.Parameters)
                        e.AddVariableData($"Parameter ' {p.ParameterName} ': ", p.Value?.ToString() ?? "NULL VALUE");
                    throw;
                }
            }
        }

        /// <inheritdoc cref="RunAction(DbConnection, string, KeyValuePair{string, object}[])"/>
        /// <inheritdoc cref="RunActionAsync(DbConnection, DbCommand, CancellationToken)"/>
        public static Task<int> RunActionAsync(DbConnection connection, string query, CancellationToken cancellationToken = default, params KeyValuePair<string, object>[] parameters) 
        {
            return RunActionAsync(connection, connection.CreateCommand(query, parameters), cancellationToken);
        }

        #endregion

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region < Get return from DataTables / DB Connections >

        /// <summary>
        /// Compile the <paramref name="query"/> and run it against the <paramref name="connection"/>
        /// </summary>
        /// <param name="connection">database connection</param>
        /// <param name="query">query designed to return a single value from the database</param>
        /// <param name="compiler">compiler used to compile the query into the SQL statement</param>
        /// <returns>
        /// The first value read from the database. This was written when the goal of the query returning a single cell. 
        /// <br/>Note: <see cref="DBNull"/> will be sanitized to <see langword="null"/>
        /// </returns>
        public static object GetValue(DbConnection connection, Query query, Compiler compiler)
        {
            using (var cmd = connection.CreateCommand(query, compiler))
            {
                var shouldClose = connection.OpenSafely();
                try
                {
                    return cmd.ExecuteScalar();
                }
                finally
                {
                    if (shouldClose && connection.State != ConnectionState.Closed) connection.Close();
                }
            }
        }

        /// <summary>Return a single value from a DB Table</summary>
        /// <param name="connection">Some DB Connection</param>
        /// <param name="tableName">Table to query</param>
        /// <param name="lookupColName">Column to to find a value in</param>
        /// <param name="lookupVal">Value to find in supplied column</param>
        /// <param name="returnColName">Return the value from this column</param>
        /// <param name="compiler">The SqlKata Compiler to use to compile the query</param>
        /// <returns><see cref="IDataReader"/>.GetValue() object. -- sanitizes <see cref="DBNull"/> to null. </returns>
        public static object GetValue(DbConnection connection, string tableName, string lookupColName, object lookupVal, string returnColName, Compiler compiler)
        {
            Query qry = new Query().Select(returnColName).From(tableName).Where(lookupColName, lookupVal);
            return GetValue(connection, qry, compiler ?? throw new ArgumentNullException(nameof(compiler)));
        }


        /// <summary>
        /// Compile the <paramref name="query"/> and run it against the <paramref name="connection"/>
        /// </summary>
        /// <param name="connection">database connection</param>
        /// <param name="query">query designed to return a single value from the database</param>
        /// <param name="compiler">compiler used to compile the query into the SQL statement</param>
        /// <param name="cancellationToken">The cancellation</param>
        /// <returns>
        /// The first value read from the database. This was written when the goal of the query returning a single cell. 
        /// <br/>Note: <see cref="DBNull"/> will be sanitized to <see langword="null"/>
        /// </returns>
        public static async Task<object> GetValueAsync(DbConnection connection, Query query, Compiler compiler, CancellationToken cancellationToken = default)
        {
            using (var cmd = connection.CreateCommand(query, compiler))
            {
                var shouldClose = await connection.OpenSafelyAsync(cancellationToken);
                try
                {
                    return await cmd.ExecuteScalarAsync(cancellationToken);
                }
                finally
                {
                    if (shouldClose && connection.State != ConnectionState.Closed) connection.Close();
                }
            }
        }

        /// <inheritdoc cref="GetValueAsync(DbConnection, Query, Compiler, CancellationToken)"/>
        /// <inheritdoc cref="GetValue(DbConnection, string, string, object, string, Compiler)"/>
        public static Task<object> GetValueAsync(DbConnection connection, string tableName, string lookupColName, object lookupVal, string returnColName, Compiler compiler, CancellationToken cancellationToken = default)
        {
            Query qry = new Query().Select(returnColName).From(tableName).Where(lookupColName, lookupVal);
            return GetValueAsync(connection, qry, compiler ?? throw new ArgumentNullException(nameof(compiler)),cancellationToken);
        }

        #endregion < Get return from DataTables / DB Connections >

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region < Upsert >

        /// <summary>
        /// Tests for the <paramref name="table"/>'s <paramref name="primaryKey"/> existing. 
        /// <br/>If it does not exist, create an insert query and execute it.
        /// <br/>If the record does exist, perform an update query.
        /// </summary>
        /// <remarks>
        /// This is not an object method or extension simply because Upserting may not always be a good idea, so I left it out of the default object functionality.
        /// <br/> This method is not written to be the most efficient method, but universal. Better methods for upsertion likely exist depending on the database, such as SqlServer MERGE command.
        /// </remarks>
        /// <param name="table">The table to Insert/Update</param>
        /// <param name="primaryKey">The row ID to search for within the <paramref name="table"/>'s primary key column</param>
        /// <param name="columnValues">A dictionary of column names/values to update/insert</param>
        /// <param name="insertPkey">When set true, add the <paramref name="primaryKey"/> value to the table's primary key column. Default is false to account for AutoNumber primary keys.</param>
        /// <returns>The number of rows affected.</returns>
        public static int Upsert(PrimaryKeyTable table, object primaryKey, IEnumerable<KeyValuePair<string, object>> columnValues, bool insertPkey = false)
        {
            using (DbConnection conn = table.Parent.GetConnection())
            {
                // Test is the value exists within the table
                using (var cmd = conn.CreateCommand(table.Select().Where(table.PrimaryKey, primaryKey), table.Parent.Compiler))
                {
                    conn.Open();
                    object value = cmd.ExecuteScalar();
                    cmd.Dispose();

                    //Decide which query to generate based on the result of the above query (If result is null, or value is null, do an insertion, otherwise update)
                    Query query = (value is null || value is DBNull)
                        /* INSERT Query  */ ? new Query(table.TableName, "INSERT").AsInsert(!insertPkey ? columnValues : columnValues.Concat(new KeyValuePair<string, object>[] { new KeyValuePair<string, object>(table.PrimaryKey, primaryKey) }))
                        /* UPDATE Query  */ : new Query(table.TableName, "UPDATE").AsUpdate(columnValues).Where(table.PrimaryKey, primaryKey);

                    using (var cmd2 = conn.CreateCommand(query, table.Parent.Compiler))
                        return cmd2.ExecuteNonQuery();
                }
            }
        }

        /// <inheritdoc cref="Upsert(PrimaryKeyTable, object, IEnumerable{KeyValuePair{string, object}}, bool)"/>
        /// <param name="updateColumn">The column to update</param>
        /// <param name="updateValue">The value to set the <paramref name="updateColumn"/> to</param>
        /// <param name="insertPkey"/><param name="primaryKey"/><param name="table"/>
        public static int Upsert(PrimaryKeyTable table, object primaryKey, string updateColumn, object updateValue, bool insertPkey = false)
        => Upsert(table, primaryKey, DBOps.CreateKeyValuePair(updateColumn, updateValue),insertPkey);


        /// <inheritdoc cref="Upsert(PrimaryKeyTable, object, IEnumerable{KeyValuePair{string, object}}, bool)"/>
        /// <param name="columns">The column to update</param>
        /// <param name="values">The values to set the <paramref name="columns"/> to</param>
        /// <param name="insertPkey"/><param name="primaryKey"/><param name="table"/>
        public static int Upsert(PrimaryKeyTable table, object primaryKey, IEnumerable<string> columns    , IEnumerable<object> values, bool insertPkey = false)
            =>Upsert(table, primaryKey, DBOps.CreateKeyValuePairs(columns, values),insertPkey);
        

        #endregion

    }

}

