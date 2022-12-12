using SqlKata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseObjects
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
            if (tableName.IsNullOrEmpty()) throw new ArgumentException("tableName parameter is null or empty!");
            TableName = tableName;
        }
        
        /// <inheritdoc/>
        public string TableName { get; }

        /// <inheritdoc/>
        public IDatabase Parent { get; }

        /// <summary>
        /// Gets the connection string from the <see cref="Parent"/>
        /// </summary>
        /// <returns><typeparamref name="O"/></returns>
        protected virtual IDbConnection GetDatabaseConnection() => Parent.GetDatabaseConnection();

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

        #region < GetDataTable >

        /// <inheritdoc/>
        public virtual DataTable GetDataTable(params string[] columns)
        {
            return Parent.GetDataTable(this.Select(columns));
        }

        /// <inheritdoc/>
        public virtual DataTable GetDataTable(string[] columns, string WhereString)
        {
            return Parent.GetDataTable(this.Select(columns).WhereRaw(WhereString));
        }

        /// <inheritdoc/>
        public virtual DataTable GetDataTable(string[] columns, string WhereString, params object[] bindings)
        {
            return Parent.GetDataTable(this.Select(columns).WhereRaw(WhereString, bindings));
        }

        /// <inheritdoc/>
        public virtual Task<DataTable> GetDataTableAsync(params string[] columns)
        {
            return Task.Run(() => GetDataTable(columns));
        }

        /// <inheritdoc/>
        public virtual Task<DataTable> GetDataTableAsync(string[] columns, string WhereString)
        {
            return Task.Run(() => GetDataTable(columns, WhereString));
        }

        /// <inheritdoc/>
        public virtual Task<DataTable> GetDataTableAsync(string[] columns, string WhereString, params object[] bindings)
        {
            return Task.Run(() => GetDataTable(columns, WhereString, bindings));
        }

        #endregion

        #region < Insert / Update >

        /// <inheritdoc/>
        public virtual int Insert(IEnumerable<string> ColNames, IEnumerable<object> ColValues)
        {
            using (IDbConnection conn = GetDatabaseConnection())
            {
                using (var cmd = conn.CreateCommand())
                {
                    Query query = new Query(TableName).AsInsert(Extensions.ConvertToKeyValuePairArray(ColNames, ColValues));
                    cmd.CommandText = Parent.Compiler.Compile(query).ToString();
                    conn.Open();
                    int i = cmd.ExecuteNonQuery();
                    conn.Close();
                    return i;                    
                }
            }
        }

        /// <summary>
        /// Run the Upsert commands
        /// </summary>
        /// <inheritdoc cref="Upsert(string, object, IEnumerable{string}, IEnumerable{object}, bool)"/>
        protected virtual int RunUpsert(string SearchCol, object SearchValue, IEnumerable<KeyValuePair<string,object>> UpdatePairs, bool InsertOnly = false)
        {
            using (IDbConnection conn = GetDatabaseConnection())
            {
                using (var cmd = conn.CreateCommand())
                {
                    Query selectQry = new Query(TableName).Select().Where(SearchCol, SearchValue);
                    cmd.CommandText = Parent.Compiler.Compile(selectQry).ToString();
                    conn.Open();
                    object value = cmd.ExecuteScalar();
                    bool isnull = value is null || value is DBNull;

                    if (InsertOnly && !isnull) return 0; //Exit if value is not null and insertOnly = true

                    Query query = isnull ?
                        new Query(TableName).AsInsert(UpdatePairs.Concat(new KeyValuePair<string, object>[] { new(SearchCol, SearchValue)})) :    // Produce INSERT query
                        new Query(TableName).AsUpdate(UpdatePairs).Where(SearchCol, SearchValue);     // Produce UPDATE query

                    cmd.CommandText = Parent.Compiler.Compile(query).ToString();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        /// <inheritdoc/>
        public int Upsert(string SearchCol, object SearchValue, string UpdateColName, object UpdateColValue, bool InsertOnly = false)
        {
            return RunUpsert(SearchCol, SearchValue, Extensions.ConvertToKeyValuePairArray(UpdateColName, UpdateColValue), InsertOnly);
        }

        /// <inheritdoc/>
        public int Upsert(string SearchCol, object SearchValue, IEnumerable<string> UpdateColNames, IEnumerable<object> UpdateColValues, bool InsertOnly = false)
        {
            return RunUpsert(SearchCol, SearchValue, Extensions.ConvertToKeyValuePairArray(UpdateColNames, UpdateColValues), InsertOnly);
        }

        #endregion


        #region < Delete Data >

        /// <summary>
        /// Delete all rows where the <paramref name="searchCol"/> value matches the <paramref name="searchValue"/>
        /// </summary>
        /// <param name="searchCol">The column name to search</param>
        /// <param name="searchValue">The value of the column to search for exactly</param>
        /// <returns>The number of rows deleted</returns>
        public int DeleteRows(string searchCol, object searchValue)
        {
            using (var conn = GetDatabaseConnection())
            {
                using (var cmd = conn.CreateCommand())
                {
                    Query deleteQuery = new Query(TableName, "This will delete data from the table!").Where(searchCol, searchValue).AsDelete();
                    cmd.CommandText = Parent.Compiler.Compile(deleteQuery).ToString();
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        #endregion

    }
}
