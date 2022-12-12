using SqlKata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseObjects
{
    /// <summary>
    /// Basic requirements for a database object
    /// </summary>
    public interface IDatabase
    {
        /// <inheritdoc cref="IDbConnection.ConnectionString"/>
        string ConnectionString { get; }

        /// <summary>
        /// Create a new <see cref="IDbConnection"/> object using the current settings
        /// </summary>
        /// <returns>new <see cref="IDbConnection"/></returns>
        IDbConnection GetDatabaseConnection();

        /// <summary>
        /// The <see cref="SqlKata.Compilers.Compiler"/> to use when running <see cref=" SqlKata.Query"/> queries.
        /// </summary>
        SqlKata.Compilers.Compiler Compiler { get; }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Run a query that should return a single DataRow</summary>
        /// <returns>
        /// If a single match is found, return it as a <see cref="DataRow"/>. <br/>
        /// If no matches are found, return null. <br/>
        /// If multiple matches are found, throw <see cref="Exception"/>
        /// </returns>
        /// <inheritdoc cref="GetDataTable(Query)"/>
        /// <exception cref="Exception"/>
        DataRow GetDataRow(Query query);

        ///<inheritdoc cref="GetDataRow(Query)"/>
        Task<DataRow> GetDataRowAsync(Query query);

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Run a query that should return a DataTable
        /// </summary>
        /// <param name="query">The query to run</param>
        /// <returns>a new DataTable with the results of the query</returns> 
        DataTable GetDataTable(Query query);

        /// <inheritdoc cref="GetDataTable(Query)"/>
        DataTable GetDataTable(string query);

        /// <inheritdoc cref="GetDataTable(string)"/>
        DataTable GetDataTableByName(string tableName);

        /// <inheritdoc cref="GetDataTableByName(string)"/>
        Task<DataTable> GetDataTableByNameAsync(string tableName);

        ///// <param name="bindings">array of bindings/values that correspond to the binding identifiers within the query</param>
        ///// <inheritdoc cref="GetDataTable(Query)"/>
        ///// <param name="query"/>
        //DataTable GetDataTable(Query query, params object[] bindings);

        ///// <inheritdoc cref="GetDataTable(Query, object[])"/>
        //Task<DataTable> GetDataTableAsync(Query query, params object[] bindings);

        /// <inheritdoc cref="GetDataTable(Query)"/>
        Task<DataTable> GetDataTableAsync(Query query);

        /// <inheritdoc cref="GetDataTable(string)"/>
        Task<DataTable> GetDataTableAsync(string query);

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Run a query designed to return a single value from the table
        /// </summary>
        /// <inheritdoc cref="DBOps.GetValue(IDbConnection, Query, SqlKata.Compilers.Compiler)"/>"
        object GetValue(Query query);

        /// <summary>
        /// Generate a <see cref="Query"/> that will return a single value from the <paramref name="table"/>
        /// </summary>
        /// <inheritdoc cref="DBOps.DataReturn(IDbConnection, string, string, object, string, SqlKata.Compilers.Compiler)"/>
        object GetValue(string table, string LookupColName, object LookupVal, string ReturnColName);

        ///// <param name="query"></param>
        ///// <param name="bindings"></param>
        ///// <inheritdoc cref="DBOps.GetValue(IDbConnection, Query, SqlKata.Compilers.Compiler)"/>"
        //object GetValue(Query query, params object[] bindings);

        /// <inheritdoc cref="GetValue(Query)"/>
        Task<object> GetValueAsync(Query query);

        /// <inheritdoc cref="GetValue(string, string, object, string)"/>
        Task<object> GetValueAsync(string TableName, string LookupColName, object LookupVal, string ReturnColName);


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <inheritdoc cref="DBOps.RunAction(IDatabase, SqlKata.Query)" />
        Task<int> RunAction(SqlKata.Query query);

        /// <inheritdoc cref="DBOps.RunAction(IDbConnection, string)" />
        Task<int> RunAction(string query);

    }
}
