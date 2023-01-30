using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using SqlKata;

namespace RFBCodeWorks.DatabaseObjects.DatabaseTypes
{
    /// <summary>
    /// Abstract base class for Databases that utilize <see cref="OleDbConnection"/> and <see cref="OleDbCommand"/> objects
    /// </summary>
    public abstract class OleDBDatabase : AbstractDatabase<OleDbConnection, OleDbCommand>
    {
        /// <inheritdoc/>
        protected OleDBDatabase() : base() { }

        /// <inheritdoc/>
        protected OleDBDatabase(string connectionString) : base(connectionString) { }

        /// <inheritdoc/>
        public override OleDbConnection GetConnection()
        {
            return new OleDbConnection(ConnectionString);
        }

        /// <remarks>
        /// <see cref="OleDbCommand"/> does not support named parameters. All named parameteres will be replaced with '?'.
        /// <br/><see href="https://learn.microsoft.com/en-us/dotnet/api/system.data.oledb.oledbcommand.parameters?view=dotnet-plat-ext-7.0"/>
        /// </remarks>
        /// <inheritdoc/>
        public override OleDbCommand GetCommand(Query query)
        {
            var cmd = new OleDbCommand();
            var result = Compiler.Compile(query ?? throw new ArgumentNullException(nameof(query)));
            cmd.Connection = this.GetConnection();
            cmd.CommandText = result.Sql;
            foreach (var p in result.NamedBindings)
            {
                cmd.CommandText = cmd.CommandText.Replace(p.Key, "?");
                _ = cmd.Parameters.AddWithValue("?", p.Value);
            }

            return cmd;
        }

        /// <summary>Create a new <see cref="OleDbCommand"/> whose Connection property is a the result of this object's GetConnection() method</summary>
        /// <returns>A new <see cref="OleDbCommand"/></returns>
        /// <remarks>
        /// <inheritdoc cref="GetCommand(Query)"/>
        /// <br/> The order of the <paramref name="keyValuePairs"/> must be in the same order the parameters appear within the <paramref name="query"/>
        /// </remarks>
        /// <param name="query">
        /// The raw sql string to use as the CommandText. 
        /// <para/><see cref="OleDbCommand"/> does not support named parameters. all parameters should be declared using a '?' character.
        /// <br/><see href="https://learn.microsoft.com/en-us/dotnet/api/system.data.oledb.oledbcommand.parameters?view=dotnet-plat-ext-7.0"/>
        /// <br/>See example:
        /// <code>Select * From [MyTable] Where [SomeCol] = ? AND [SomeOtherCol] = ?</code>
        /// </param>
        /// <param name="keyValuePairs">
        /// The parameters to use. These must be supplied in the order they will be appear in the query.
        /// <br/> Ex: 
        /// <code>
        /// 'Select * From [MyTable] Where [SomeCol] = ? AND [SomeOtherCol] = ?'
        /// <br/>("SomeCol",true), ("SomeOtherCol",false)
        /// <br/>'Select * From [MyTable] Where [SomeCol] = true AND [SomeOtherCol] = false'
        /// </code>
        /// </param>
        /// <inheritdoc/>
        public override OleDbCommand GetCommand(string query, IEnumerable<KeyValuePair<string, object>> keyValuePairs)
        {
            var cmd = new OleDbCommand();
            cmd.Connection = this.GetConnection();
            cmd.CommandText = query;
            foreach (var p in keyValuePairs)
            {
                if (!string.IsNullOrWhiteSpace(p.Key))
                {
                    cmd.CommandText = cmd.CommandText.Replace(p.Key, "?");
                }
                _ = cmd.Parameters.AddWithValue("?", p.Value);
            }
            return cmd;
        }

        /// <inheritdoc cref="GetCommand(string, IEnumerable{KeyValuePair{string, object}})"/>
        /// <param name="query"/>
        /// <param name="parameters">
        /// The parameters to use. These must be supplied in the order they will be appear in the query.
        /// <br/> Ex: 
        /// <code>
        /// 'Select * From [MyTable] Where [SomeCol] = ? AND [SomeOtherCol] = ?'
        /// <br/>("SomeCol",true), ("SomeOtherCol",false)
        /// <br/>'Select * From [MyTable] Where [SomeCol] = true AND [SomeOtherCol] = false'
        /// </code>
        /// </param>
        public override OleDbCommand GetCommand(string query, params KeyValuePair<string, object>[] parameters)
            => GetCommand(query, keyValuePairs: parameters);

    }
}
