using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Text;

namespace RFBCodeWorks.DatabaseObjects.DatabaseTypes
{
    /// <summary>
    /// Create a new Database Object that represents an MS Access Database, and utilizes the <see cref="RFBCodeWorks.SqlKata.MsOfficeCompilers.MSAccessCompiler"/>
    /// </summary>
    public class MSAccessDataBase : OleDBDatabase
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
        /// <inheritdoc cref="GenerateACEConnectionString(string, string)"/>
        public MSAccessDataBase(string path, string password) : base(GenerateACEConnectionString(path, password)) { }


        /// <inheritdoc/>
        public override Compiler Compiler => RFBCodeWorks.SqlKata.MsOfficeCompilers.MSAccessCompiler.AccessCompiler;


        #region < Connection Strings >

        /// <summary>
        /// Generate a new OLEDB.JET database connection string
        /// </summary>
        /// <param name="path">path to the database</param>
        /// <param name="dbPassword">Password to the database</param>
        /// <returns>new <see cref="OleDbConnection"/></returns>
        /// <exception cref="ArgumentException"/>
        public static string GenerateJetConnectionString(string path, string dbPassword = default)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path has no value");
            if (!System.IO.Path.IsPathRooted(path)) throw new ArgumentException("Path is not rooted!");
            if (!System.IO.Path.HasExtension(path)) throw new ArgumentException("Path does not have an extension!");

            string Conn = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source= {path} ; Persist Security Info = false ;";
            if (!string.IsNullOrWhiteSpace(dbPassword)) Conn += $" Jet OLEDB:Database Password={dbPassword};";
            return Conn;
        }

        /// <summary>
        /// Generate a new OLEDB.ACE database connection string
        /// </summary>
        /// <param name="path">path to the database</param>
        /// <param name="dbPassword">Password to the database</param>
        /// <returns>new <see cref="OleDbConnection"/></returns>
        /// <exception cref="ArgumentException"/>
        public static string GenerateACEConnectionString(string path, string dbPassword = default)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path has no value");
            if (!System.IO.Path.IsPathRooted(path)) throw new ArgumentException("Path is not rooted!");
            if (!System.IO.Path.HasExtension(path)) throw new ArgumentException("Path does not have an extension!");

            string Conn = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path}; OLE DB Services=-1;Persist Security Info=False;";
            if (!string.IsNullOrWhiteSpace(dbPassword)) Conn += $" Jet OLEDB:Database Password={dbPassword};";
            return Conn;
        }

        /// <summary>
        /// Generate a new OLEDB.ACE database connection
        /// </summary>
        /// <inheritdoc cref="GenerateACEConnectionString"/>
        public static OleDbConnection GetACEConnection(string path, string dbPassword = default)
        {
            return new OleDbConnection(GenerateACEConnectionString(path, dbPassword));
        }

        /// <summary>
        /// Generate a new OLEDB.ACE database connection
        /// </summary>
        /// <inheritdoc cref="GenerateJetConnectionString"/>
        public static OleDbConnection GetJetConnection(string path, string dbPassword = default)
        {
            return new OleDbConnection(GenerateJetConnectionString(path, dbPassword));
        }

        #endregion
    }
}
