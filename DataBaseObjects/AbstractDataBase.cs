﻿using SqlKata;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace RFBCodeWorks.DataBaseObjects
{
    /// <summary>
    /// Abstract base class for database structures
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractDataBase<T> : IDatabase where T : DbConnection
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
        public abstract T GetDatabaseConnection();

        IDbConnection IDatabase.GetDatabaseConnection() => GetDatabaseConnection();

        /// <summary>
        /// Attempt to briefly open the connection to the database to check if it is accessible.
        /// </summary>
        /// <returns>TRUE if the connection was opened successfully, otherwise false</returns>
        public virtual bool TestDatabaseConnection(out Exception e)
        {
            try
            {
                using (var db = GetDatabaseConnection())
                {
                    db.Open();
                    db.Close();
                }
                e = null;
                return true;
            }catch(Exception ex)
            {
                e = ex;
                return false;
            }
        }

        /// <inheritdoc cref="TestDatabaseConnection(out Exception)"/>
        public bool TestDatabaseConnection() => TestDatabaseConnection(out _);

        /// <inheritdoc cref="TestDatabaseConnection(out Exception)"/>
        public Task<bool> TestDatabaseConnectionAsync() => Task.Run(TestDatabaseConnection);

        /// <inheritdoc/>
        /// <exception cref="InvalidExpressionException"/>
        public virtual DataRow GetDataRow(Query query)
        {
            var DT = GetDataTable(query);
            if (DT is null) return null;
            if (DT.Rows.Count > 1) throw new InvalidExpressionException("Multiple Matches Found when expected only a single match!");
            if (DT.Rows.Count == 1) return DT.Rows[0];
            return null;
        }

        /// <inheritdoc/>
        public virtual DataTable GetDataTable(Query query)
        {
            string queryString = this.Compiler.Compile(query).ToString();
            return DBOps.GetDataTable(this.GetDatabaseConnection(), queryString);
        }

        /// <inheritdoc/>
        public virtual DataTable GetDataTable(string query)
        {
            return DBOps.GetDataTable(this.GetDatabaseConnection(), query);
        }

        /// <inheritdoc/>
        public virtual object GetValue(Query query)
        {
            return DBOps.GetValue(this.GetDatabaseConnection(), query, Compiler);
        }

        /// <inheritdoc/>
        public virtual object GetValue(string table, string lookupColName, object lookupVal, string returnColName)
        {
            return GetValue(new Query(table).Select(returnColName).Where(lookupColName, lookupVal));
        }

        /// <inheritdoc/>
        public virtual Task<DataTable> GetDataTableAsync(Query query)
        {
            return Task.Run(() => GetDataTable(query));
        }

        /// <inheritdoc/>
        public virtual Task<DataTable> GetDataTableAsync(string query)
        {
            return Task.Run(() => GetDataTable(query));
        }

        /// <inheritdoc/>
        public virtual DataTable GetDataTableByName(string tableName)
        {
            return GetDataTable(new Query(tableName));
        }

        /// <inheritdoc/>
        public Task<DataTable> GetDataTableByNameAsync(string tableName)
        {
            return Task.Run(() => GetDataTableByNameAsync(tableName));
        }

        /// <inheritdoc/>
        public virtual Task<int> RunAction(Query query)
        {
            return DBOps.RunAction(this, query);
        }

        /// <inheritdoc/>
        public virtual Task<int> RunAction(string query)
        {
            return DBOps.RunAction(this.GetDatabaseConnection(), query);
        }

        /// <inheritdoc/>
        public Task<DataRow> GetDataRowAsync(Query query)
        {
            return Task.Run(() => GetDataRow(query));
        }

        /// <inheritdoc/>
        public Task<object> GetValueAsync(Query query)
        {
            return Task.Run(() => GetValue(query));
        }

        /// <inheritdoc/>
        public Task<object> GetValueAsync(string tableName, string lookupColName, object lookupVal, string returnColName)
        {
            return Task.Run(() => GetValue(tableName, lookupColName, lookupVal, returnColName));
        }

    }
}
