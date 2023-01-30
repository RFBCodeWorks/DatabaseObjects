using SqlKata;
using SqlKata.Compilers;
using SQC = SqlKata.Compilers;
using RSQ = RFBCodeWorks.SqlKata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RFBCodeWorks.DatabaseObjects
{
    /// <summary>
    /// Basic requirements for a database object
    /// </summary>
    public interface IDatabase
    {
        ///// <inheritdoc cref="IDbConnection.ConnectionString"/>
        //string ConnectionString { get; }

        /// <summary>
        /// Create a new <see cref="DbConnection"/> object using the current settings
        /// </summary>
        /// <returns>new <see cref="DbConnection"/></returns>
        DbConnection GetConnection();

        /// <summary>
        /// The <see cref="SQC.Compiler"/> to use when running <see cref="Query"/> queries.
        /// </summary>
        Compiler Compiler { get; }

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Run a query that should return a single DataRow</summary>
        /// <returns>
        /// If a single match is found, return it as a <see cref="DataRow"/>. <br/>
        /// If no matches are found, return null. <br/>
        /// If multiple matches are found, throw <see cref="InvalidExpressionException"/>
        /// </returns>
        /// <inheritdoc cref="GetDataTable(Query)"/>
        /// <exception cref="InvalidExpressionException"/>
        DataRow GetDataRow(Query query);

        /// <inheritdoc cref="GetDataRow(Query)"/>
        /// <inheritdoc cref="DbCommand.ExecuteReaderAsync(CancellationToken)"/>
        Task<DataRow> GetDataRowAsync(Query query, CancellationToken cancellationToken = default);

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Run a query that should return a DataTable
        /// </summary>
        /// <param name="query">The query to run</param>
        /// <returns>a new DataTable with the results of the query</returns> 
        DataTable GetDataTable(Query query);

        /// <inheritdoc cref="GetDataTable(Query)"/>
        DataTable GetDataTable(string query, params KeyValuePair<string, object>[] parameters);

        /// <summary>
        /// Gets a <see cref="DataTable"/> representing a table by its <paramref name="tableName"/>
        /// </summary>
        /// <param name="tableName">The name of the table to retrieve</param>
        /// <returns>A <see cref="DataTable"/> representing the <paramref name="tableName"/></returns>
        DataTable GetDataTableByName(string tableName);

        /// <inheritdoc cref="GetDataTableByName(string)"/>
        /// <inheritdoc cref="GetDataTableAsync(Query, CancellationToken)"/>
        Task<DataTable> GetDataTableByNameAsync(string tableName, CancellationToken cancellationToken = default);

        /// <summary>
        /// Run a query that should return a DataTable
        /// </summary>
        /// <param name="query">The query to run</param>
        /// <returns>a new DataTable with the results of the query</returns> 
        /// <inheritdoc cref="DBOps.GetDataTableAsync(DbConnection, DbCommand, CancellationToken)"/>
        /// <param name="cancellationToken"/>
        Task<DataTable> GetDataTableAsync(Query query, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="DBOps.GetDataTableAsync(DbConnection, string, CancellationToken, KeyValuePair{string, object}[])"/>
        Task<DataTable> GetDataTableAsync(string query, CancellationToken cancellationToken = default, params KeyValuePair<string, object>[] parameters);

        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Run a query designed to return a single value from the table
        /// </summary>
        /// <inheritdoc cref="DBOps.GetValue(DbConnection, Query, Compiler)"/>"
        object GetValue(Query query);

        /// <summary>
        /// Generate a <see cref="Query"/> that will return a single value from the <paramref name="table"/>
        /// </summary>
        /// <inheritdoc cref="DBOps.GetValue(DbConnection, string, string, object, string, Compiler)"/>
        object GetValue(string table, string lookupColName, object lookupVal, string returnColName);

        /// <inheritdoc cref="DBOps.GetValueAsync(DbConnection, Query, Compiler, CancellationToken)"/>
        Task<object> GetValueAsync(Query query, CancellationToken cancellationToken = default);

        /// <inheritdoc cref="DBOps.GetValueAsync(DbConnection, string, string, object, string, Compiler, CancellationToken)"/>
        Task<object> GetValueAsync(string tableName, string lookupColName, object lookupVal, string returnColName, CancellationToken cancellationToken = default);


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Associate the <paramref name="command"/> to this connection, then run the action provided by the command.
        /// </summary>
        /// <param name="command">The DbCommand to execute against the database</param>
        /// <inheritdoc cref="DbCommand.ExecuteNonQuery"/>
        int RunAction(DbCommand command);

        /// <summary>
        /// Compile the <paramref name="query"/>, then run it against the database
        /// </summary>
        /// <inheritdoc cref="IDbCommand.ExecuteNonQuery" />
        /// <param name="query">The query to run</param>
        int RunAction(Query query);

        /// <inheritdoc cref="DBOps.RunAction(DbConnection, string, KeyValuePair{string, object}[])" />
        int RunAction(string query, params KeyValuePair<string,object>[] parameters);

        /// <inheritdoc cref="DBOps.RunActionAsync(DbConnection, DbCommand, System.Threading.CancellationToken)" />
        Task<int> RunActionAsync(DbCommand command, System.Threading.CancellationToken cancellationToken = default);

        /// <inheritdoc cref="RunAction(Query)"/>
        /// <inheritdoc cref="DBOps.RunActionAsync(DbConnection, DbCommand, CancellationToken)"/>
        Task<int> RunActionAsync(Query query, System.Threading.CancellationToken cancellationToken = default);

        /// <inheritdoc cref="DBOps.RunActionAsync(DbConnection, string, System.Threading.CancellationToken, KeyValuePair{string, object}[])" />
        Task<int> RunActionAsync(string query, System.Threading.CancellationToken cancellationToken = default, params KeyValuePair<string, object>[] parameters);

    }
}
