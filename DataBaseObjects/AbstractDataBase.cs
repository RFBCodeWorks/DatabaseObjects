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

namespace RFBCodeWorks.DataBaseObjects
{
    /// <summary>
    /// Abstract base class for database structures
    /// </summary>
    /// <typeparam name="TConnectionType">The type of <see cref="DbConnection"/> this database utilizes</typeparam>
    /// <typeparam name="TCommandType">The type of <see cref="DbCommand"/> that can be used with this <typeparamref name="TConnectionType"/></typeparam>
    public abstract class AbstractDataBase<TConnectionType, TCommandType> : IDatabase 
        where TConnectionType : DbConnection 
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
        public abstract TConnectionType GetConnection();

        DbConnection IDatabase.GetConnection() => GetConnection();


        #region < Test Connection >

        /// <inheritdoc cref="DBOps.TestConnection(IDbConnection, out Exception)"/>
        public bool TestConnection(out Exception e) => DBOps.TestConnection(this.GetConnection(), out e);

        /// <inheritdoc cref="TestConnection(out Exception)"/>
        public bool TestConnection() => DBOps.TestConnection(this.GetConnection());

        /// <inheritdoc cref="DBOps.TestConnectionAsync(DbConnection, CancellationToken)"/>
        public Task<(bool, Exception)> TestConnectionAsync(CancellationToken cancellationToken = default) => DBOps.TestConnectionAsync(this.GetConnection(), cancellationToken);

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
            using (var conn = this.GetConnection())
            {
                using (var cmd = conn.CreateCommand(query, this.Compiler))
                    return DBOps.GetDataTable(conn, cmd);
            }
        }

        /// <inheritdoc/>
        public virtual DataTable GetDataTable(string query, params KeyValuePair<string, object>[] parameters)
        {
            using (var conn = this.GetConnection())
                return DBOps.GetDataTable(conn, query, parameters);
        }

        /// <inheritdoc/>
        public virtual Task<DataTable> GetDataTableAsync(Query query, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var conn = this.GetConnection())
            {
                using (var cmd = conn.CreateCommand(query, this.Compiler))
                    return DBOps.GetDataTableAsync(conn, cmd, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public virtual Task<DataTable> GetDataTableAsync(string query, CancellationToken cancellationToken = default, params KeyValuePair<string, object>[] parameters)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (var conn = GetConnection())
            {
                using (var cmd = conn.CreateCommand(query, parameters))
                {
                    return DBOps.GetDataTableAsync(conn, cmd, cancellationToken);
                }
            }
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
            using (var conn = this.GetConnection())
                return DBOps.GetValue(conn, query, Compiler);
        }

        /// <inheritdoc/>
        public object GetValue(string tableName, string lookupColName, object lookupVal, string returnColName)
        {
            return GetValue(new Query(tableName).Select(returnColName).Where(lookupColName, lookupVal));
        }

        /// <inheritdoc/>
        public virtual Task<object> GetValueAsync(Query query, CancellationToken cancellationToken = default)
        {
            using (var conn = this.GetConnection())
                return DBOps.GetValueAsync(conn, query, this.Compiler, cancellationToken);
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
            using (command)
            using (var conn = this.GetConnection())
            {
                command.Connection = conn;
                conn.Open();
                return command.ExecuteNonQuery();
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Executes via <see cref="RunAction(TCommandType)"/>
        /// </remarks>
        public int RunAction(Query query)
        {
            return RunAction(query.ToDbCommand<TCommandType>(Compiler));
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Executes via <see cref="RunAction(TCommandType)"/>
        /// </remarks>
        public int RunAction(string query, params KeyValuePair<string, object>[] parameters)
        {
            return RunAction(DBCommands.CreateCommand<TCommandType>(query, parameters));
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
            return RunActionAsync(query.ToDbCommand<TCommandType>(Compiler), cancellationToken);
        }

        /// <inheritdoc/>
        public Task<int> RunActionAsync(string query, CancellationToken cancellationToken = default, params KeyValuePair<string, object>[] parameters)
        {
            return RunActionAsync(DBCommands.CreateCommand<TCommandType>(query, parameters), cancellationToken);
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
