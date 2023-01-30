using SqlKata;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RFBCodeWorks.SqlKata.Extensions;

namespace RFBCodeWorks.DataBaseObjects.DataBaseTypes
{
    /// <summary>
    /// Abstract base class for database structures
    /// </summary>
    /// <typeparam name="TConnectionType">The type of <see cref="DbConnection"/> this database utilizes</typeparam>
    /// <typeparam name="TCommandType">The type of <see cref="DbCommand"/> that can be used with this <typeparamref name="TConnectionType"/></typeparam>
    public abstract class AbstractDataBase<TConnectionType, TCommandType> : IDatabase 
        where TConnectionType : DbConnection, new()
        where TCommandType: DbCommand, new()
    {
        /// <summary>
        /// Create a new abstract database
        /// </summary>
        /// <remarks>
        /// <see cref="ConnectionString"/> must be set by derived class!
        /// </remarks>
        protected AbstractDataBase() { }

        /// <summary>
        /// Create a new Database object with the supplied connection string or database name
        /// </summary>
        /// <param name="connectionString">The connection string to the database</param>
        protected AbstractDataBase(string connectionString) { ConnectionString = connectionString; }

        /// <summary>
        /// Specify the compiler to use with this database object
        /// </summary>
        public abstract Compiler Compiler { get; }

        /// <summary>
        /// The database connection string
        /// </summary> 
        public string ConnectionString { get; protected set; }

        /// <summary>
        /// Create a new <see cref="DbConnection"/>
        /// </summary>
        /// <returns>new object whose type is derived from <see cref="DbConnection"/></returns>
        public virtual TConnectionType GetConnection()
        {
            return new TConnectionType()
            {
                ConnectionString = this.ConnectionString
            };
        }

        DbConnection IDatabase.GetConnection() => GetConnection();

        /// <summary>
        /// Create a new <typeparamref name="TCommandType"/> object associated with this database's ConnectionString.
        /// </summary>
        /// <returns>A new <typeparamref name="TCommandType"/> object</returns>
        public TCommandType GetCommand()
        {
            var cmd = new TCommandType();
            cmd.Connection = this.GetConnection();
            return cmd;
        }

        /// <inheritdoc cref="GetCommand()"/>
        /// <inheritdoc cref="DBCommands.CreateCommand(DbConnection, string, IEnumerable{KeyValuePair{string, object}})"/>
        public virtual TCommandType GetCommand(string query, IEnumerable<KeyValuePair<string, object>> keyValuePairs)
        {
            var cmd = DBCommands.CreateCommand<TCommandType>(query, keyValuePairs);
            cmd.Connection = this.GetConnection();
            return cmd;
        }

        /// <inheritdoc cref="GetCommand(string, IEnumerable{KeyValuePair{string, object}})"/>
        /// <inheritdoc cref="DBCommands.CreateCommand{T}(string, KeyValuePair{string, object}[])"/>
        public virtual TCommandType GetCommand(string query, params KeyValuePair<string, object>[] parameters)
            => GetCommand(query, keyValuePairs: parameters);

        /// <inheritdoc cref="DBCommands.CreateCommand(DbConnection, Query, Compiler)"/>
        public virtual TCommandType GetCommand(Query query)
        {
            return (TCommandType)this.GetConnection().CreateCommand(query, this.Compiler);
        }

        #region < Test Connection >

        /// <inheritdoc cref="DBOps.TestConnection(IDbConnection, out Exception)"/>
        public virtual bool TestConnection(out Exception e) => DBOps.TestConnection(this.GetConnection(), out e);

        /// <inheritdoc cref="TestConnection(out Exception)"/>
        public virtual bool TestConnection() => DBOps.TestConnection(this.GetConnection());

        /// <inheritdoc cref="DBOps.TestConnectionAsync(DbConnection, CancellationToken)"/>
        public virtual Task<(bool, Exception)> TestConnectionAsync(CancellationToken cancellationToken = default) => DBOps.TestConnectionAsync(this.GetConnection(), cancellationToken);

        #endregion


        #region < Get DataRow >

        /// <inheritdoc/>
        /// <exception cref="InvalidExpressionException"/>
        public virtual DataRow GetDataRow(Query query)
        {
            var dt = GetDataTable(query);
            if (dt is null) return null;
            if (dt.Rows.Count > 1) throw new InvalidExpressionException("Query returned multiple rows when a single row was expected!");
            if (dt.Rows.Count == 1) return dt.Rows[0];
            return null;
        }

        /// <inheritdoc/>
        public virtual async Task<DataRow> GetDataRowAsync(Query query, CancellationToken cancellationToken = default)
        {
            var dt = await GetDataTableAsync(query, cancellationToken);
            if (dt is null) return null;
            if (dt.Rows.Count > 1) throw new InvalidExpressionException("Query returned multiple rows when a single row was expected!");
            if (dt.Rows.Count == 1) return dt.Rows[0];
            return null;
        }

        #endregion


        #region < Get DataTable >

        /// <inheritdoc/>
        public virtual DataTable GetDataTable(Query query)
        {
            using (var cmd = GetCommand(query)) // Ensures command gets disposesd
            using (cmd.Connection) // Ensures connection gets disposed
                return DBOps.GetDataTable(cmd.Connection, cmd);
        }

        /// <inheritdoc/>
        public virtual DataTable GetDataTable(string query, params KeyValuePair<string, object>[] parameters)
        {
            using (var cmd = GetCommand(query, parameters)) // Ensures command gets disposesd
            using (cmd.Connection) // Ensures connection gets disposed
                return DBOps.GetDataTable(cmd.Connection, cmd);
        }

        /// <inheritdoc/>
        public virtual Task<DataTable> GetDataTableAsync(Query query, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var cmd = GetCommand(query)) // Ensures command gets disposesd
            using (cmd.Connection) // Ensures connection gets disposed
                return DBOps.GetDataTableAsync(cmd.Connection, cmd, cancellationToken);
        }

        /// <inheritdoc/>
        public virtual Task<DataTable> GetDataTableAsync(string query, CancellationToken cancellationToken = default, params KeyValuePair<string, object>[] parameters)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var cmd = GetCommand(query, parameters)) // Ensures command gets disposesd
            using (cmd.Connection)
                return DBOps.GetDataTableAsync(cmd.Connection, cmd, cancellationToken);
        }

        /// <inheritdoc/>
        public DataTable GetDataTableByName(string tableName)
        {
            return GetDataTable(new Query(tableName));
        }

        /// <inheritdoc/>
        public Task<DataTable> GetDataTableByNameAsync(string tableName, CancellationToken cancellationToken = default)
        {
            return GetDataTableAsync(new Query(tableName), cancellationToken);
        }

        #endregion


        #region < GetValue >

        /// <inheritdoc/>
        public virtual object GetValue(Query query)
        {
            using (var cmd = GetCommand(query))
            using (var conn = cmd.Connection)
            {
                conn.Open();
                return cmd.ExecuteScalar();
            }
                
        }

        /// <inheritdoc/>
        public object GetValue(string tableName, string lookupColName, object lookupVal, string returnColName)
        {
            return GetValue(new Query(tableName).Select(returnColName).Where(lookupColName, lookupVal));
        }

        /// <inheritdoc/>
        public virtual async Task<object> GetValueAsync(Query query, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            object ret = null;
            using (var cmd = GetCommand(query))
            using (var conn = cmd.Connection)
            {
                await conn.OpenAsync(cancellationToken);
                ret = await cmd.ExecuteScalarAsync(cancellationToken);
            }
            return ret;
        }

        /// <inheritdoc/>
        public Task<object> GetValueAsync(string tableName, string lookupColName, object lookupVal, string returnColName, CancellationToken cancellationToken = default)
        {
            return GetValueAsync(new Query(tableName).Select(returnColName).Where(lookupColName, lookupVal), cancellationToken);
        }

        #endregion


        #region < RunAction >

        /// <inheritdoc/>
        public virtual int RunAction(TCommandType command)
        {
            //int i = 0;
            using (command)
            {
                using (var conn = this.GetConnection())
                {
                    command.Connection = conn;
                    conn.Open();
                    return command.ExecuteNonQuery();
                    //conn.Close();
                    //conn.Dispose();
                }
            }
            //command.Dispose();
            //return i; 
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Executes via <see cref="RunAction(TCommandType)"/>
        /// </remarks>
        public int RunAction(Query query)
        {
            return RunAction(GetCommand(query));
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Executes via <see cref="RunAction(TCommandType)"/>
        /// </remarks>
        public int RunAction(string query, params KeyValuePair<string, object>[] parameters)
        {
            return RunAction(GetCommand(query, parameters));
        }

        /// <inheritdoc/>
        public virtual async Task<int> RunActionAsync(TCommandType command, CancellationToken cancellationToken = default)
        {
            using (command)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (var conn = this.GetConnection())
                {
                    command.Connection = conn;
                    await conn.OpenAsync(cancellationToken);
                    return await command.ExecuteNonQueryAsync(cancellationToken);
                }
            }
        }

        /// <inheritdoc/>
        public Task<int> RunActionAsync(Query query, CancellationToken cancellationToken = default)
        {
            return RunActionAsync(GetCommand(query), cancellationToken);
        }

        /// <inheritdoc/>
        public Task<int> RunActionAsync(string query, CancellationToken cancellationToken = default, params KeyValuePair<string, object>[] parameters)
        {
            return RunActionAsync(GetCommand(query, parameters), cancellationToken);
        }

        #endregion


        #region < Explicit Interface >

        int IDatabase.RunAction(DbCommand command)
        {
            if (command is TCommandType cmd)
                return RunAction(cmd);
            else
                throw new ArgumentException("DbCommand object was not of a compatible type for this database");
        }

        Task<int> IDatabase.RunActionAsync(DbCommand command, CancellationToken cancellationToken)
        {
            if (command is TCommandType cmd)
                return RunActionAsync(cmd, cancellationToken);
            else
                throw new ArgumentException("DbCommand object was not of a compatible type for this database");
        }

        #endregion


    }
}
