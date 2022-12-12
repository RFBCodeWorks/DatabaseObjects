using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Text;

namespace DataBaseObjects.DataBaseTypes
{
    public class MSAccessDataBase : AbstractDataBase<OleDbConnection>
    {
        /// <summary>
        /// Create an <see cref="MSAccessDataBase"/> connection from the specified <paramref name="connectionString"/>
        /// </summary>
        /// <param name="connectionString"></param>
        public MSAccessDataBase(string connectionString) : base(connectionString) { }

        /// <summary>
        /// Generate an ACE connection to the MS Access database at the specified <paramref name="path"/>
        /// </summary>
        /// <returns/>
        /// <inheritdoc cref="ConnectionStringBuilders.AccessDB.GenerateACEConnectionString(string, string)"/>
        public MSAccessDataBase(string path, string password) : base(GenerateACEConnectionString(path, password)) { }

        /// <inheritdoc/>
        public override Compiler Compiler => CompilerSingletons.MSAccessCompiler;

        /// <inheritdoc/>
        public override OleDbConnection GetDatabaseConnection()
        {
            return new OleDbConnection(ConnectionString);
        }

        /// <inheritdoc cref="ConnectionStringBuilders.AccessDB.GenerateJetConnectionString(string, string)"/>
        public static string GenerateJetConnectionString(string path, string password = "") => ConnectionStringBuilders.AccessDB.GenerateJetConnectionString(path, password);
        
        /// <inheritdoc cref="ConnectionStringBuilders.AccessDB.GenerateACEConnectionString(string, string)"/>
        public static string GenerateACEConnectionString(string path, string password = "") => ConnectionStringBuilders.AccessDB.GenerateACEConnectionString(path, password);

    }
}
