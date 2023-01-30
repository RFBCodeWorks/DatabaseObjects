using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Common;
using SqlKata.Compilers;
using SqlKata;
using System.Linq;

namespace RFBCodeWorks.DataBaseObjects
{
    /// <summary>
    /// Class that contains methods for producing <see cref="System.Data.Common.DbCommand"/> objects that contain simple prebuilt  SQL statements
    /// </summary>
    public static class DBCommands
    {


#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        
        public const string LocalTable = "@LocalTable";
        public const string Column = "@Col";
        public const string Columns = "@Columns";
        public const string RemoteTable = "@RemoteTable";
        public const string PrimaryKey = "@PK";
        //public const string SearchTerm = "@SearchTerm";
        //public const string SearchCol = "@SearchCol";

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member


        #region < CreateCommand >

        /// <summary>
        /// Add a new parameter for the to the <paramref name="command"/> - Effectively same as 'AddWithValue'
        /// </summary>
        /// <param name="command">The <see cref="DbCommand"/> to add the parameter to</param>
        /// <param name="name">Sets the <see cref="DbParameter.ParameterName"/></param>
        /// <param name="value"><inheritdoc cref="DbParameter.Value" path="*"/></param>
        /// <returns>The created parameter</returns>
        public static DbParameter AddParameter(this DbCommand command, string name, object value)
        {
            var par = command.CreateParameter();
            par.ParameterName = name;
            par.Value = value;
            command.Parameters.Add(par);
            return par;
        }

        /// <summary>
        /// Create a new <see cref="DbCommand"/> with the provided <paramref name="query"/> and <paramref name="keyValuePairs"/>
        /// </summary>
        /// <param name="connection">The connection object - will only be used for <see cref="DbConnection.CreateCommand"/></param>
        /// <param name="query">The query string</param>
        /// <param name="keyValuePairs">
        /// The parameters to apply to the new <see cref="DbCommand"/>
        /// <br/> - The Key string should be the name of the parameter as it appears within the <paramref name="query"/> string. 
        /// <br/> - The Value object should be the value of the parameter.
        /// <para/> Example:  'Select @Col from @Table WHERE @PK = @SearchTerm'
        /// <br/> ("@Col", "*")     // select all columns
        /// <br/> ("@Table", "Student")
        /// <br/> ("@PK", "StudentID")
        /// <br/> ("@SearchTerm", 129001) // column is an <see cref="int"/> column, so no wrapping needed
        /// </param>
        /// <returns>A new <see cref="DbCommand"/></returns>
        public static DbCommand CreateCommand(this DbConnection connection, string query, IEnumerable<KeyValuePair<string, object>> keyValuePairs)
        {

            var cmd = connection.CreateCommand();
            cmd.CommandText = query;
            //if (cmd.GetType().Name == "OleDbCommand") // System.Data.OleDbCommand does not accept named parameters, and must use '?' instead.
            //{
            //    foreach (var p in parameters)
            //    {
            
            //    }
            //}
            //else
            //{
                foreach (var p in keyValuePairs)
                {
                    _ = cmd.AddParameter(p.Key, p.Value);
                }
            //}
            return cmd;
        }

        /// <inheritdoc cref="CreateCommand(DbConnection, string, IEnumerable{KeyValuePair{string, object}})"/>
        /// <param name="connection"/><param name="query"/>
        /// <param name="parameters"> <inheritdoc cref="CreateCommand(DbConnection, string, IEnumerable{KeyValuePair{string, object}})" path="/param[@name='keyValuePairs']" /> </param>
        public static DbCommand CreateCommand(this DbConnection connection, string query, params KeyValuePair<string, object>[] parameters) 
            => CreateCommand(connection, query, keyValuePairs: parameters);

        /// <summary>
        /// Create a new <see cref="DbCommand"/> associated with the <paramref name="connection"/>, whose CommandText and parameters are acquired from the <paramref name="query"/>
        /// </summary>
        /// <param name="connection">The database connection to generate a commnd for</param>
        /// <param name="query">The query that provides the parameters for the generated command</param>
        /// <param name="compiler">The compiler that will compile the query</param>
        /// <returns>A new <see cref="DbCommand"/></returns>
        /// <exception cref="ArgumentNullException"/>
        
        public static DbCommand CreateCommand(this DbConnection connection, Query query, Compiler compiler)
        {
            if (connection is null) throw new ArgumentNullException(nameof(connection));
            if (compiler is null) throw new ArgumentNullException(nameof(compiler));
            var result = compiler.Compile(query ?? throw new ArgumentNullException(nameof(query)));
            var cmd = connection.CreateCommand();
            cmd.CommandText = result.Sql;

            //if (cmd.GetType().Name == "OleDbCommand") // System.Data.OleDbCommand does not accept named parameters, and must use '?' instead.
            //{
            //    foreach (var p in result.NamedBindings)
            //    {
            //        cmd.CommandText = cmd.CommandText.Replace(p.Key, "?");
            //        _ = cmd.AddParameter("?", p.Value);
            //    }
            //}
            //else
            //{
                foreach (var p in result.NamedBindings)
                {
                    _ = cmd.AddParameter(p.Key, p.Value);
                }
            //}
            return cmd;
        }

        /// <summary>Create a new <typeparamref name="T"/></summary>
        /// <returns>A new DbCommand of type: <typeparamref name="T"/></returns>
        /// <inheritdoc cref="CreateCommand(DbConnection, string, KeyValuePair{string, object}[])"/>
        public static T CreateCommand<T>(string query, IEnumerable<KeyValuePair<string, object>> keyValuePairs) 
            where T : DbCommand, new()
        {

            var cmd = new T
            {
                CommandText = query
            };
            //if (cmd.GetType().Name == "OleDbCommand") // System.Data.OleDbCommand does not accept named parameters, and must use '?' instead.
            //{
            //    foreach (var p in keyValuePairs)
            //    {
            //        cmd.CommandText = cmd.CommandText.Replace(p.Key, "?");
            //        _ = cmd.AddParameter("?", p.Value);
            //    }
            //}
            //else
            //{
            foreach (var p in keyValuePairs)
                {
                    _ = cmd.AddParameter(p.Key, p.Value);
                }
            //}
            return cmd;
        }

        /// <inheritdoc cref="CreateCommand{T}(string, IEnumerable{KeyValuePair{string, object}})"/>
        public static T CreateCommand<T>(string query, params KeyValuePair<string, object>[] parameters) where T : DbCommand, new() 
            => CreateCommand<T>(query, keyValuePairs: parameters);

        #endregion

        #region < Update Column From Upstream >

        /// <inheritdoc cref="UpdateColumnFromUpstream{T}(T, string, string, string, string)"/>
        public static DbCommand UpdateColumnFromUpstream(DbConnection db, string localTable = "", string pKeyColumn = "Id",  string remoteTable = "", string updateColumn = "")
        {
            return UpdateColumnFromUpstream(db.CreateCommand(), localTable, pKeyColumn, remoteTable, updateColumn);
        }

        /// <inheritdoc cref="UpdateColumnFromUpstream{T}(T, string, string, string, string)"/>
        public static T UpdateColumnFromUpstream<T>(string localTable = "", string pKeyColumn = "Id", string remoteTable = "", string updateColumn = "")
            where T: DbCommand, new()
        {
            return UpdateColumnFromUpstream(new T(), localTable, pKeyColumn, remoteTable, updateColumn);
        }

        /// <summary>
        /// Create a new IDBCommand that will update a table's column with the data from another table where the primary keys match.
        /// <br/> - This assumes that the column names in the <paramref name="localTable"/> and the <paramref name="remoteTable"/> are identical for both the <paramref name="pKeyColumn"/> and the <paramref name="updateColumn"/>
        /// <br/> - NOTE: This DOES NOT run the command!
        /// </summary>
        /// <typeparam name="T">The type of <see cref="DbCommand"/> to create</typeparam>
        /// <param name="cmd"></param>
        /// <param name="localTable">The name of the local table to update within the database this command will run against</param>
        /// <param name="pKeyColumn">The name of the Primary Key column - Both local and remote tables are expected to have the same column name here</param>
        /// <param name="remoteTable">The location of the remote table. Can be a table name, or a connection string pointing to a table.</param>
        /// <param name="updateColumn">The name of the column to update - Both local and remote tables are expected to have the same column name here</param>
        /// <returns>
        /// A new <see cref="DbCommand"/> with the following parameters:
        /// <br/> - @LocalTable -- <paramref name="localTable"/>
        /// <br/> - @Col -- <paramref name="updateColumn"/>
        /// <br/> - @RemoteTable -- <paramref name="remoteTable"/>
        /// <br/> - @PK -- <paramref name="pKeyColumn"/>
        /// </returns>
        private static T UpdateColumnFromUpstream<T>(T cmd, string localTable = "", string pKeyColumn = "Id", string remoteTable = "", string updateColumn = "") where T: DbCommand
        {
            cmd.CommandText = "UPDATE " + LocalTable + " AS LOCAL " +
                "\nSET LOCAL." + Column + " = REMOTE." + Column +
                "\nFROM (SELECT " + Column + " FROM " + RemoteTable + ") AS REMOTE " +
                "\nWHERE LOCAL." + PrimaryKey + " = REMOTE." + PrimaryKey;
            cmd.Parameters.Clear();
            AddParameter(cmd, LocalTable, localTable);
            AddParameter(cmd, Column, updateColumn);
            AddParameter(cmd, RemoteTable, remoteTable);
            AddParameter(cmd, PrimaryKey, pKeyColumn);            
            return cmd;
        }


        #endregion


        #region < InsertIntoFrom >


        /// <inheritdoc cref="InsertIntoFrom{T}(T, string, string, string)"/>
        public static DbCommand InsertIntoFrom(DbConnection db, string localTable = "", string remoteTable = "", string updateColumn = "")
        {
            return InsertIntoFrom(db.CreateCommand(), localTable, remoteTable, updateColumn);
        }

        /// <inheritdoc cref="InsertIntoFrom{T}(T, string, string, string)"/>
        public static T InsertIntoFrom<T>(string localTable = "", string remoteTable = "", string updateColumn = "")
            where T : DbCommand, new()
        {
            return InsertIntoFrom(new T(), localTable, remoteTable, updateColumn);
        }

        /// <summary>
        /// Create a new IDBCommand that will create a new table by pulling the columns from another table
        /// <br/> - NOTE: This DOES NOT run the command!
        /// </summary>
        /// <typeparam name="T">The type of <see cref="DbCommand"/> to create</typeparam>
        /// <param name="cmd"></param>
        /// <param name="localTable">The name of the local table to update within the database this command will run against</param>
        /// <param name="remoteTable">The location of the remote table. Can be a table name, or a connection string pointing to a table.</param>
        /// <param name="columns">The name of the column to update - Both local and remote tables are expected to have the same column name here</param>
        /// <returns>
        /// A new <see cref="DbCommand"/> with the following parameters:
        /// <br/> - @LocalTable -- <paramref name="localTable"/>
        /// <br/> - @Columns -- <paramref name="columns"/>
        /// <br/> - @RemoteTable -- <paramref name="remoteTable"/>
        /// </returns>
        private static T InsertIntoFrom<T>(T cmd, string localTable = "", string remoteTable = "", string columns = "*") where T : DbCommand
        {
            cmd.CommandText = "INSERT " + Columns + " FROM " + RemoteTable + " INTO " + LocalTable;
            cmd.Parameters.Clear();
            AddParameter(cmd, LocalTable, localTable);
            AddParameter(cmd, Columns, columns ?? "*");
            AddParameter(cmd, RemoteTable, remoteTable);
            return cmd;
        }

        #endregion


        #region < InsertMissingRecords >

        /// <inheritdoc cref="InsertMissingRecords{T}(T, string, string, string)"/>
        public static DbCommand InsertMissingRecords(DbConnection db, string localTable = "", string remoteTable = "", string updateColumn = "")
        {
            return InsertMissingRecords(db.CreateCommand(), localTable, remoteTable, updateColumn);
        }

        /// <inheritdoc cref="InsertMissingRecords{T}(T, string, string, string)"/>
        public static T InsertMissingRecords<T>(string localTable = "", string remoteTable = "", string updateColumn = "")
            where T : DbCommand, new()
        {
            return InsertMissingRecords(new T(), localTable, remoteTable, updateColumn);
        }

        /// <summary>
        /// Create a new IDBCommand that compares a local table and a remote table. Any records missing from the local table will be inserted. (Comparison using a PrimaryKey)
        /// <br/> - NOTE: This DOES NOT run the command!
        /// </summary>
        /// <typeparam name="T">The type of <see cref="DbCommand"/> to create</typeparam>
        /// <param name="cmd"></param>
        /// <param name="localTable">The name of the local table to update within the database this command will run against</param>
        /// <param name="remoteTable">The location of the remote table. Can be a table name, or a connection string pointing to a table.</param>
        /// <param name="pKeyColumn">The name of the Primary Key column - Both local and remote tables are expected to have the same column name here</param>
        /// <returns>
        /// A new <see cref="DbCommand"/> with the following parameters:
        /// <br/> - @LocalTable -- <paramref name="localTable"/>
        /// <br/> - @RemoteTable -- <paramref name="remoteTable"/>
        /// </returns>
        private static T InsertMissingRecords<T>(T cmd, string localTable = "", string remoteTable = "", string pKeyColumn = "Id") where T : DbCommand
        {
            cmd.CommandText = "INSERT INTO "+ LocalTable + "  as LOCAL " +
                "\nSELECT * FROM " + RemoteTable + " WHERE NOT IN (" +
                "SELECT * FROM " + RemoteTable + " as REMOTE" +
                "\nWHERE LOCAL." + PrimaryKey + " = REMOTE." + PrimaryKey + ")";
            cmd.Parameters.Clear();
            AddParameter(cmd, LocalTable, localTable);
            AddParameter(cmd, RemoteTable, remoteTable);
            AddParameter(cmd, PrimaryKey, pKeyColumn);
            return cmd;
        }

        #endregion


        #region < RemoveLoneyRecords >

        /// <inheritdoc cref="RemoveLonelyRecords{T}(T, string, string, string)"/>
        public static DbCommand RemoveLonelyRecords(DbConnection db, string localTable = "", string remoteTable = "", string updateColumn = "")
        {
            return RemoveLonelyRecords(db.CreateCommand(), localTable, remoteTable, updateColumn);
        }

        /// <inheritdoc cref="RemoveLonelyRecords{T}(T, string, string, string)"/>
        public static T RemoveLonelyRecords<T>(string localTable = "", string remoteTable = "", string updateColumn = "")
            where T : DbCommand, new()
        {
            return RemoveLonelyRecords(new T(), localTable, remoteTable, updateColumn);
        }

        /// <summary>
        /// Create a new IDBCommand that removes all lonely records from the local table.
        /// <br/> - A lonely record would be a record that resides local table but does not reside in the remote table, as determined by primary key comparison
        /// <br/> - NOTE: This DOES NOT run the command!
        /// </summary>
        /// <typeparam name="T">The type of <see cref="DbCommand"/> to create</typeparam>
        /// <param name="cmd"></param>
        /// <param name="localTable">The name of the local table to update within the database this command will run against</param>
        /// <param name="remoteTable">The location of the remote table. Can be a table name, or a connection string pointing to a table.</param>
        /// <param name="pKeyColumn">The name of the Primary Key column - Both local and remote tables are expected to have the same column name here</param>
        /// <returns>
        /// A new <see cref="DbCommand"/> with the following parameters:
        /// <br/> - @LocalTable -- <paramref name="localTable"/>
        /// <br/> - @RemoteTable -- <paramref name="remoteTable"/>
        /// </returns>
        private static T RemoveLonelyRecords<T>(T cmd, string localTable = "", string remoteTable = "", string pKeyColumn = "Id") where T : DbCommand
        {
            cmd.CommandText = "DELETE FROM" + LocalTable + " as LOCAL " +
                "\nWHERE NOT NOT IN ( SELECT " + PrimaryKey + " FROM  " + RemoteTable + " )";
            cmd.Parameters.Clear();
            AddParameter(cmd, LocalTable, localTable);
            AddParameter(cmd, RemoteTable, remoteTable);
            AddParameter(cmd, PrimaryKey, pKeyColumn);
            return cmd;
        }

        #endregion

    }
}
