using RFBCodeWorks.SqlKata.Extensions;
using SqlKata.Compilers;
using SqlKata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RFBCodeWorks.DataBaseObjects
{
    /// <summary>
    /// Abstract base class for a Table within a database
    /// </summary>
    public class DataBaseTable : IDataBaseTable
    { 
        /// <summary>
        /// Create a new <see cref="DataBaseTable"/>
        /// </summary>
        /// <param name="parent">parent <see cref="IDatabase"/> object this table belongs to</param>
        /// <param name="tableName"><inheritdoc cref="TableName" path="*"/></param>
        public DataBaseTable(IDatabase parent, string tableName)
        {
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentException("tableName parameter is null or empty!");
            TableName = tableName;
        }
        
        /// <inheritdoc/>
        public string TableName { get; }

        /// <inheritdoc/>
        public IDatabase Parent { get; }

        /// <summary>
        /// Gets the connection string from the <see cref="Parent"/>
        /// </summary>
        protected virtual DbConnection GetDatabaseConnection() => Parent.GetConnection();

        #region < Generate SQL Statements >

        /// <inheritdoc/>
        public Query Select()
        {
            return new Query(TableName).Select();
        }

        /// <inheritdoc/>
        public Query Select(params string[] columns)
        {
            return new Query(TableName).Select(columns);
        }

        /// <inheritdoc/>
        public Query SelectWhere(string[] columns, string searchColumn, object searchValue)
        {
            return Select(columns).Where(searchColumn, searchValue);
        }

        /// <inheritdoc/>
        public Query SelectWhere(string[] columns, string searchColumn, string op, object searchValue)
        {
            return Select(columns).Where(searchColumn, op, searchValue);
        }

        /// <inheritdoc/>
        public Query SelectWhere(string[] columns, params KeyValuePair<string, object>[] keyValuePairs)
        {
            return Select(columns).Where(keyValuePairs);
        }

        #endregion

        #region < Get Data >

        /// <inheritdoc/>
        public virtual DataTable GetDataTable(params string[] columns)
        {
            return Parent.GetDataTable(this.Select(columns));
        }

        /// <summary>
        /// Request a set of columns as a DataTable using a 'WhereRaw' statement
        /// </summary>
        /// <param name="columns">The columns to request from the table</param>
        /// <param name="whereRaw">
        /// The raw 'WHERE' statement. 
        /// <br/> - Any column names specified should be properly wrapped for this database type
        /// <br/> - Each <paramref name="bindings"/> should be indiciated by a single '?' within the string.
        /// <para/>
        /// Examples:
        /// <br/>"[Id] = ?"   --> The '?' will be replaced by the first object within <paramref name="bindings"/>
        /// <br/>"? = ?"   --> A binding array of { "Id", 0 } would result to sql of:  'Id' = 0, which may be invalid.
        /// <br/>"?? = ?"   --> A binding array of { "Id", 0 } will throw an exception since 3 <paramref name="bindings"/> were expected.
        /// </param>
        /// <param name="bindings">
        /// An array of values to apply to the raw 'Where' statement. They will be applied in the order they appear, replacing each '?' as it arrives within the string.
        /// </param>
        /// <returns>A datatable that is the result of the query</returns>
        /// <remarks>
        /// Uses the following queries to build the statement:
        /// <br/> - <inheritdoc cref="GetDataTable(string[])"/>
        /// <br/> - <inheritdoc cref="BaseQuery{Q}.WhereRaw(string, object[])"/>
        /// </remarks>
        public virtual DataTable GetDataTable(string[] columns, string whereRaw, params object[] bindings)
        {
            return Parent.GetDataTable(this.Select(columns).WhereRaw(whereRaw, bindings));
        }

        /// <inheritdoc/>
        public virtual Task<DataTable> GetDataTableAsync(CancellationToken cancellationToken = default, params string[] columns)
        {
            return Parent.GetDataTableAsync(this.Select(columns), cancellationToken);
        }

        /// <inheritdoc cref="GetDataTable(string[], string, object[])"/>
        public virtual Task<DataTable> GetDataTableAsync(string[] columns, string whereRaw, object[] bindings, CancellationToken cancellationToken = default)
        {
            return Parent.GetDataTableAsync(this.Select(columns).WhereRaw(whereRaw, bindings), cancellationToken);
        }

        /// <inheritdoc/>
        public virtual object GetValue(string lookupColName, object lookupVal, string returnColName)
        {
            return Parent.GetValue(TableName, lookupColName, lookupVal, returnColName);
        }

        /// <inheritdoc/>
        public virtual Task<object> GetValueAsync(string lookupColName, object lookupVal, string returnColName, CancellationToken cancellationToken = default)
        {
            return Parent.GetValueAsync(this.TableName, lookupColName, lookupVal, returnColName, cancellationToken);
        }

        #endregion

        #region < Insert >

        /// <inheritdoc/>
        public virtual int Insert(IEnumerable<KeyValuePair<string, object>> values)
        {
            return Parent.RunAction(new Query(TableName).AsInsert(values));
        }

        /// <inheritdoc/>
        public virtual int Insert(IEnumerable<string> columns, IEnumerable<object> values)
        {
            return Insert(columns.CreateKeyValuePairs(values));
        }

        /// <inheritdoc/>
        public virtual Task<int> InsertAsync(IEnumerable<KeyValuePair<string, object>> values, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Parent.RunActionAsync(new Query(TableName).AsInsert(values), cancellationToken);            
        }

        /// <inheritdoc/>
        public virtual Task<int> InsertAsync(IEnumerable<string> columns, IEnumerable<object> values, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return InsertAsync(columns.CreateKeyValuePairs(values, cancellationToken), cancellationToken);
        }

        #endregion

        #region < Update >

        /// <inheritdoc/>
        public virtual int Update(IEnumerable<KeyValuePair<string, object>> values, params IWhereCondition[] whereStatements)
        {
            Query query = new Query(TableName, "Update_Query").AsUpdate(values);
            foreach (IWhereCondition where in whereStatements)
            {
                where?.ApplyToQuery(query);
            }
            return Parent.RunAction(query);
        }

        /// <inheritdoc/>
        public int Update(IEnumerable<string> columns, IEnumerable<object> values, params IWhereCondition[] whereStatements)
        {
            return Update(columns.CreateKeyValuePairs(values), whereStatements);
        }

        /// <inheritdoc/>
        public virtual Task<int> UpdateAsync(IEnumerable<KeyValuePair<string, object>> values, CancellationToken cancellationToken = default, params IWhereCondition[] whereStatements)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Query query = new Query(TableName, "Update_Query").AsUpdate(values);
            foreach (IWhereCondition where in whereStatements)
            {
                cancellationToken.ThrowIfCancellationRequested();
                where?.ApplyToQuery(query);
            }
            return Parent.RunActionAsync(query, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<int> UpdateAsync(IEnumerable<string> columns, IEnumerable<object> values, CancellationToken cancellationToken = default, params IWhereCondition[] whereStatements)
        {
            return UpdateAsync(columns.CreateKeyValuePairs(values, cancellationToken), cancellationToken, whereStatements);
        }

        #endregion

        #region < Delete Data >

        /// <summary>
        /// Delete all rows where the <paramref name="searchCol"/> value matches the <paramref name="searchValue"/>
        /// </summary>
        /// <param name="searchCol">The column name to search</param>
        /// <param name="searchValue">The value of the column to search for exactly</param>
        /// <returns>The number of rows deleted</returns>
        public virtual int DeleteRows(string searchCol, object searchValue)
        {
            return Parent.RunAction(new Query(TableName, "This will delete data from the table!").Where(searchCol, searchValue).AsDelete());
        }

        /// <summary>
        /// Delete all rows where the <paramref name="searchCol"/> value matches the <paramref name="searchValue"/>
        /// </summary>
        /// <param name="searchCol">The column name to search</param>
        /// <param name="searchValue">The value of the column to search for exactly</param>
        /// <param name="cancellationToken"></param>
        /// <returns>The number of rows deleted</returns>
        /// <inheritdoc cref="DbCommand.ExecuteNonQueryAsync(CancellationToken)"/>
        public virtual Task<int> DeleteRowsAsync(string searchCol, object searchValue, CancellationToken cancellationToken = default)
        {
            return Parent.RunActionAsync(new Query(TableName, "This will delete data from the table!").Where(searchCol, searchValue).AsDelete());
        }

        #endregion

    }
}
