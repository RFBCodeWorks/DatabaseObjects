using SqlKata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace RFBCodeWorks.DataBaseObjects
{
    /// <summary>
    /// Represents the functions a database table should provide
    /// </summary>
    public interface IDataBaseTable
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
        /// Request the columns from the table.
        /// </summary>
        /// <param name="columns">The columns retrieve. <br/> <see langword="null"/>, an empty array, or "*" will return the entire column list.</param>
        /// <returns>a new DataTable with the results of the query</returns>
        DataTable GetDataTable(params string[] columns);

        /// <inheritdoc cref="GetDataTable(string[])"/>
        /// <inheritdoc cref="BaseQuery{Q}.WhereRaw(string, object[])"/>
        /// <param name="columns"/>
        /// <param name="WhereString">the raw 'WHERE' statement</param>
        DataTable GetDataTable(string[] columns, string WhereString);

        /// <param name="bindings">The bindings to apply to the DBCommand</param>
        /// <inheritdoc cref="GetDataTable(string[], string)"/>
        /// <param name="WhereString"/>
        /// <param name="columns"/>
        DataTable GetDataTable(string[] columns, string WhereString, params object[] bindings);

        /// <inheritdoc cref="GetDataTable(string[])"/>
        Task<DataTable> GetDataTableAsync(params string[] columns);

        /// <inheritdoc cref="GetDataTable(string[], string)"/>
        Task<DataTable> GetDataTableAsync(string[] columns, string WhereString);

        #endregion

        #region < Insert / UpSert >

        /// <param name="ColNames">array of column names to update</param>
        /// <param name="ColValues">array of values to pass into the table</param>
        /// <inheritdoc cref="Upsert(string, object, IEnumerable{string}, IEnumerable{object}, bool)"/>
        int Insert(IEnumerable<string> ColNames, IEnumerable<object> ColValues);

        /// <summary>
        /// Check the table if a value exists. If it does, update it. If it does not, insert the value into a new row. <br/>
        /// If multiple matches are found, updates all the values accordingly.
        /// </summary>
        /// <param name="SearchCol">column name to search within</param>
        /// <param name="SearchValue">value to search for within the <paramref name="SearchCol"/></param>
        /// <param name="UpdateColName">column name to apply the <paramref name="UpdateColValue"/> into</param>
        /// <param name="UpdateColValue">new value for the field</param>
        /// <param name="InsertOnly">If TRUE, will not update the value in the table if any value already exists</param>
        /// <returns><inheritdoc cref="IDbCommand.ExecuteNonQuery"/></returns>
        int Upsert(string SearchCol, object SearchValue, string UpdateColName, object UpdateColValue, bool InsertOnly = false);

        /// <param name="UpdateColNames">array of column names to update</param>
        /// <param name="UpdateColValues">array of values to pass into the table</param>
        /// <inheritdoc cref="Upsert(string, object, string, object, bool)"/>
        /// <param name="SearchCol"/> <param name="SearchValue"/> <param name="InsertOnly"/>
        int Upsert(string SearchCol, object SearchValue, IEnumerable<string> UpdateColNames, IEnumerable<object> UpdateColValues, bool InsertOnly = false);

        #endregion
    }

    /// <summary>
    /// DataBase table that has a single column as a primary key
    /// </summary>
    public interface ISimpleKeyDataBaseTable : IDataBaseTable
    {
        /// <summary>
        /// Name of the PrimaryKey Column
        /// </summary>
        string PrimaryKey { get; }

        #region < DataReturn >

        /// <summary>
        /// Search the table for the value, then return the result
        /// </summary>
        /// <param name="primaryKeyValue">Search the primary key column for this value</param>
        /// <param name="returnColName">Name of the column to return</param>
        /// <returns>If successful, return the object. Otherwise return null.</returns>
        object GetValue(object primaryKeyValue, string returnColName);

        /// <returns>If successful, return the <see langword="bool"/> value. <see cref="DBNull"/> converts to <see langword="null"/>.</returns>
        /// <inheritdoc cref="GetValue"/>
        bool? GetValueAsBool(object primaryKeyValue, string returnColName);

        /// <returns>If successful, return the <see cref="string"/> value. <see cref="DBNull"/> converts to <see langword="null"/>.</returns>
        /// <inheritdoc cref="GetValue"/>
        string GetValueAsString(object primaryKeyValue, string returnColName);

        /// <returns>If successful, return the <see cref="int"/> value. <see cref="DBNull"/> converts to <see langword="null"/>.</returns>
        /// <inheritdoc cref="GetValue"/>
        int? GetValueAsInt(object primaryKeyValue, string returnColName);

        /// <returns>A task that <inheritdoc cref="GetValue(object, string)"/></returns>
        /// <inheritdoc cref="GetValue"/>
        Task<object> GetValueAsync(object primaryKeyValue, string returnColName);

        /// <returns>A task that <inheritdoc cref="GetValueAsBool(object, string)"/></returns>
        /// <inheritdoc cref="GetValueAsync"/>
        Task<bool?> GetValueAsBoolAsync(object primaryKeyValue, string returnColName);

        /// <returns>A task that <inheritdoc cref="GetValueAsString(object, string)"/></returns>
        /// <inheritdoc cref="GetValueAsync"/>
        Task<string> GetValueAsStringAsync(object primaryKeyValue, string returnColName);

        /// <returns>A task that <inheritdoc cref="GetValueAsInt(object, string)"/></returns>
        /// <inheritdoc cref="GetValueAsync"/>
        Task<int?> GetValueAsIntAsync(object primaryKeyValue, string returnColName);

        #endregion

        #region < GetDataTable >

        /// <summary>
        /// Search the table's primary key column for the <paramref name="primaryKeyValue"/>, then return the entire row.
        /// </summary>
        /// <returns>
        /// If a matching primary key is found, return the corresponding <see cref="DataRow"/>. Otherwise return null.
        /// </returns>
        /// <inheritdoc cref="GetValue(object, string)"/>
        DataRow GetDataRow(object primaryKeyValue);

        /// <inheritdoc cref="GetDataRow(object)"/>
        Task<DataRow> GetDataRowAsync(object primaryKeyValue);

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
        /// <inheritdoc cref="IDataBaseTable.GetDataTable(string[])" path="/param[@name='columns']" />
        /// </param>
        /// <returns>The result of <paramref name="dictionaryBuilder"/></returns>
        Dictionary<X, Y> GetDictionary<X, Y>(Func<DataTable, Dictionary<X, Y>> dictionaryBuilder, params string[] valueColumns);

        #endregion
    }

    /// <summary>
    /// DataBase table that has a compound / composite key consisting of 2 or more columns
    /// </summary>
    public interface ICompoundKeyDataBaseTable : IDataBaseTable
    {
        /// <summary>
        /// The array of column names that make up the compound key
        /// </summary>
        string[] CompoundKeyColumns { get; }

        /// <summary>
        /// The number of columns that make up the compound key
        /// </summary>
        int CompoundKeyColumnCount { get; }

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
        Task<DataRow> GetDataRowAsync(object[] CompoundKeyValues);

        #endregion
    }
}
