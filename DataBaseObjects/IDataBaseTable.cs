using SqlKata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RFBCodeWorks.SqlKata.Extensions;
using SqlKata.Compilers;

namespace RFBCodeWorks.DatabaseObjects
{
    /// <summary>
    /// Represents the functions a database table should provide
    /// </summary>
    public interface IDatabaseTable
    {
        /// <summary>
        /// The reference to the parent database
        /// </summary>
        IDatabase Parent { get; }

        /// <summary>
        /// Name of the Table
        /// </summary>
        string TableName { get; }

        #region < Generate SQL Statements >

        /// <summary>Generate a 'Select * From [TableName]' query</summary>
        /// <inheritdoc cref="Query.Select(string[])"/>
        Query Select();

        /// <summary>Generate a 'Select <paramref name="columns"/> From [TableName]' query</summary>
        /// <param name="columns">the array of columns to select from the table</param>
        Query Select(params string[] columns);

        /// <summary>
        /// Generate a 'SELECT <paramref name="columns"/> FROM [TableName] WHERE <paramref name="searchColumn"/> == <paramref name="searchValue"/>' query
        /// </summary>
        /// <param name="searchColumn">Column Name to search for the value</param>
        /// <param name="searchValue">Value to search for within the <paramref name="searchColumn"/></param>
        /// <inheritdoc cref="Select(string[])"/>
        /// <inheritdoc cref="BaseQuery{Q}.Where(string, object)"/>
        /// <param name="columns"/>
        Query SelectWhere(string[] columns, string searchColumn, object searchValue);

        /// <inheritdoc cref="SelectWhere(string[], string, object)"/>
        /// <param name="op">The operator to compare the value of the <paramref name="searchColumn"/> and <paramref name="searchValue"/></param>
        /// <inheritdoc cref="BaseQuery{Q}.Where(string, string, Func{Q, Q})"/>
        /// <param name="columns"/>
        /// <param name="searchColumn"/>
        /// <param name="searchValue"/>
        Query SelectWhere(string[] columns, string searchColumn, string op, object searchValue);

        /// <inheritdoc cref="SelectWhere(string[], string, object)"/>
        /// <param name="values">
        /// Column/Value pairs to constrain the query. 
        /// <br/>Ex: ("State", "WI") will select all results where the 'State' column has a value of 'WI'
        /// </param>
        /// <inheritdoc cref="BaseQuery{Q}.Where(IEnumerable{KeyValuePair{string, object}})"/>
        /// <param name="columns"/>
        Query SelectWhere(string[] columns, params KeyValuePair<string, object>[] values);

        #endregion

        #region < GetDataTable >

        /// <summary>
        /// Request the columns from the table via a <see cref="Query"/>
        /// </summary>
        /// <param name="columns">The columns to retrieve. 
        /// <br/> Note: <see langword="null"/>, an empty array, or "*" will return the entire column list.
        /// <br/> <see cref="Query.Select(string[])"/>
        /// </param>
        /// <returns>a new DataTable with the results of the query</returns>
        /// <remarks>
        /// Uses the following queries to build the statement:
        /// <br/> - <see cref="Query.Select(string[])"/>
        /// </remarks>
        DataTable GetDataTable(params string[] columns);

        /// <inheritdoc cref="GetDataTable(string[])"/>
        Task<DataTable> GetDataTableAsync(CancellationToken cancellationToken = default, params string[] columns);

        #endregion

        #region < GetValue >

        /// <inheritdoc cref="DBOps.GetValue(System.Data.Common.DbConnection, string, string, object, string, Compiler)"/>
        object GetValue(string lookupColName, object lookupVal, string returnColName);

        /// <inheritdoc cref="DBOps.GetValueAsync(System.Data.Common.DbConnection, string, string, object, string, Compiler, System.Threading.CancellationToken)"/>
        Task<object> GetValueAsync(string lookupColName, object lookupVal, string returnColName, System.Threading.CancellationToken cancellationToken = default);

        #endregion

        #region < Insert  >

        /// <summary>Insert a new row into the table.</summary>
        /// <param name="values">a collection of ColumnName (key) and values</param>
        /// <inheritdoc cref="DbCommand.ExecuteNonQuery"/>
        int Insert(IEnumerable<KeyValuePair<string,object>> values);

        /// <summary>Insert a new row into the table.</summary>
        /// <param name="columns">array of column names. This should be all of the required columns within the table. The number of items must match (and be in the same order as) the <paramref name="values"/>values</param>
        /// <param name="values">array of values to pass into the table</param>
        /// <inheritdoc cref="DbCommand.ExecuteNonQuery"/>
        int Insert(IEnumerable<string> columns, IEnumerable<object> values);

        /// <inheritdoc cref="Insert(IEnumerable{KeyValuePair{string, object}})"/>
        /// <inheritdoc cref="DbCommand.ExecuteNonQueryAsync(CancellationToken)"/>
        Task<int> InsertAsync(IEnumerable<KeyValuePair<string, object>> values, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="Insert(IEnumerable{string}, IEnumerable{object})"/>
        /// <inheritdoc cref="DbCommand.ExecuteNonQueryAsync(CancellationToken)"/>
        Task<int> InsertAsync(IEnumerable<string> columns, IEnumerable<object> values, CancellationToken cancellationToken = default);

        #endregion

        #region < Update >

        /// <summary>
        /// Update the table's values. Any rows that are matched by the <paramref name="whereStatements"/> will be updated.
        /// </summary>
        /// <param name="values">a collection of ColumnName (key) and values</param>
        /// <param name="whereStatements">The collection of <see cref="IWhereCondition"/>s that will be used to generate the sql</param>
        /// <inheritdoc cref="DbCommand.ExecuteNonQuery"/>
        int Update(IEnumerable<KeyValuePair<string, object>> values, params IWhereCondition[] whereStatements);

        /// <summary>
        /// Update the table's values. Any rows that are matched by the <paramref name="whereStatements"/> will be updated.
        /// </summary>
        /// <param name="columns">array of column names. This should be all of the required columns within the table. The number of items must match (and be in the same order as) the <paramref name="values"/>values</param>
        /// <param name="values">array of values to pass into the table.</param>
        /// <param name="whereStatements">The collection of <see cref="IWhereCondition"/>s that will be used to generate the sql</param>
        /// <inheritdoc cref="Insert(IEnumerable{string}, IEnumerable{object})"/>
        /// <inheritdoc cref="DbCommand.ExecuteNonQuery"/>
        int Update(IEnumerable<string> columns, IEnumerable<object> values, params IWhereCondition[] whereStatements);

        /// <inheritdoc cref="Update(IEnumerable{KeyValuePair{string, object}}, IWhereCondition[])"/>
        /// <inheritdoc cref="DbCommand.ExecuteNonQueryAsync(CancellationToken)"/>
        Task<int> UpdateAsync(IEnumerable<KeyValuePair<string, object>> values, CancellationToken cancellationToken = default, params IWhereCondition[] whereStatements);

        /// <inheritdoc cref="Update(IEnumerable{string}, IEnumerable{object}, IWhereCondition[])"/>
        /// <inheritdoc cref="DbCommand.ExecuteNonQueryAsync(CancellationToken)"/>
        Task<int> UpdateAsync(IEnumerable<string> columns, IEnumerable<object> values, CancellationToken cancellationToken = default, params IWhereCondition[] whereStatements);

        #endregion

    }

    /// <summary>
    /// Database table that has a single column as a primary key
    /// </summary>
    public interface IPrimaryKeyTable : IDatabaseTable
    {
        /// <summary>
        /// Name of the PrimaryKey Column
        /// </summary>
        string PrimaryKey { get; }

        #region < GetValue >

        /// <summary>
        /// Search the table's primary key column for the <paramref name="primaryKey"/>, then return the result from the <paramref name="returnColumn"/>
        /// </summary>
        /// <param name="primaryKey">Search the primary key column for this value</param>
        /// <param name="returnColumn">Name of the column to return</param>
        /// <returns>If successful, return the object. Otherwise return null.</returns>
        object GetValue(object primaryKey, string returnColumn);

        /// <inheritdoc cref="GetValue(object, string)"/>
        /// <inheritdoc cref="DBOps.GetValueAsync(System.Data.Common.DbConnection, string, string, object, string, Compiler, CancellationToken)"/>
        Task<object> GetValueAsync(object primaryKey, string returnColumn, CancellationToken cancellationToken = default);

        #endregion

        #region < GetDataTable >

        /// <summary>
        /// Search the table's primary key column for the <paramref name="primaryKey"/>, then return the entire row.
        /// </summary>
        /// <returns>
        /// If a matching primary key is found, return the corresponding <see cref="DataRow"/>. Otherwise return null.
        /// </returns>
        /// <inheritdoc cref="GetValue(object, string)"/>
        DataRow GetDataRow(object primaryKey);

        /// <inheritdoc cref="GetDataRow(object)"/>
        /// <inheritdoc cref="IDatabase.GetDataRowAsync(Query, CancellationToken)"/>
        Task<DataRow> GetDataRowAsync(object primaryKey, CancellationToken cancellationToken = default);

        #endregion

        #region < GetDictionary >

        /// <summary>
        /// Build a dictionary object where the Key is the value from the <see cref="PrimaryKey"/> column, and the values are from the specified <paramref name="valueColumn"/>
        /// </summary>
        /// <param name="valueColumn">name of the value column</param>
        /// <returns>new dictionary{int, string}</returns>
        Dictionary<int, string> GetDictionary(string valueColumn);

        /// <summary>
        /// Get the DataTable from the database, then pass it to the <paramref name="dictionaryBuilder"/> functionary to build a dictionary of some type.
        /// </summary>
        /// <typeparam name="X">Dictionary Key Type</typeparam>
        /// <typeparam name="Y">Dictionary Value Type</typeparam>
        /// <param name="dictionaryBuilder">Function that loops through a DataTable and produces a new Dictionary. </param>
        /// <param name="valueColumns">
        /// <inheritdoc cref="IDatabaseTable.GetDataTable(string[])" path="/param[@name='columns']" />
        /// </param>
        /// <returns>The result of <paramref name="dictionaryBuilder"/></returns>
        Dictionary<X, Y> GetDictionary<X, Y>(Func<DataTable, Dictionary<X, Y>> dictionaryBuilder, params string[] valueColumns);

        #endregion

    }

    /// <summary>
    /// Database table that has a compound / composite key consisting of 2 or more columns
    /// </summary>
    public interface ICompositeKeyTable : IDatabaseTable
    {
        /// <summary>
        /// The array of column names that make up the compound key
        /// </summary>
        string[] CompositeKeyColumns { get; }

        /// <summary>
        /// The number of columns that make up the compound key
        /// </summary>
        int CompositeColumnCount { get; }

        #region < GetDataRow >

        /// <summary>
        /// Search the table's primary key columns for the <paramref name="CompoundKeyValues"/>, then return the entire row.
        /// </summary>
        /// <returns>
        /// If a matching primary key is found, return the corresponding <see cref="DataRow"/>. Otherwise return null.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        DataRow GetDataRow(object[] CompoundKeyValues);

        /// <inheritdoc cref="GetDataRow(object[])"/>
        Task<DataRow> GetDataRowAsync(object[] CompoundKeyValues, CancellationToken cancellationToken = default);

        #endregion
    }
}
