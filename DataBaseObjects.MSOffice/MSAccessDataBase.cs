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

        /// <inheritdoc cref="MSAccessDataBase.MSAccessDataBase(string, string, MSOfficeConnectionProvider)"/>
        public MSAccessDataBase(string path, string dbPassword) : base(GetConnectionString(path, dbPassword, default)) { }

        /// <summary>
        /// Generate a connection string to the MS Access database at the specified <paramref name="path"/>
        /// </summary>
        /// <returns/>
        /// <inheritdoc cref="GetConnectionString"/>
        public MSAccessDataBase(string path, string dbPassword, MSOfficeConnectionProvider provider) : base(GetConnectionString(path, dbPassword, provider)) { }

        /// <inheritdoc/>
        public override Compiler Compiler => RFBCodeWorks.SqlKata.MsOfficeCompilers.MSAccessCompiler.AccessCompiler;


        #region < Connection Strings >

        /// <summary>
        /// Generate a Connection string via <see cref="GetConnectionString"/>, and transform it into a new OleDBConnection
        /// </summary>
        /// <returns>A new <see cref="OleDbConnection"/> object</returns>
        /// <inheritdoc cref="GetConnectionString"/>
        public static OleDbConnection GetConnection(string path, string dbPassword = default, MSOfficeConnectionProvider provider = default)
        {
            return new OleDbConnection(GetConnectionString(path, dbPassword, provider));
        }

        /// <summary>
        /// Generate a new OLEDB connection string
        /// </summary>
        /// <param name="path">File path to the database - must be fully qualified</param>
        /// <param name="dbPassword">Password to the database - optional</param>
        /// <param name="provider">The selected Connection provider </param>
        /// <returns>new string</returns>
        /// <exception cref="ArgumentException"/>
        public static string GetConnectionString(string path, string dbPassword = default, MSOfficeConnectionProvider provider = default)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Path has no value");
            if (!System.IO.Path.IsPathRooted(path)) throw new ArgumentException("Path is not rooted!");
            if (!System.IO.Path.HasExtension(path)) throw new ArgumentException("Path does not have an extension!");
            
            StringBuilder connection = new StringBuilder();
            connection.Append(provider switch
            {
#if _WIN32
                MSOfficeConnectionProvider.Jet4 => $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={path};Persist Security Info=false;",
#else
                (MSOfficeConnectionProvider)1 => throw new InvalidOperationException("Jet4.0 is not compatible with 64-Bit assemblies."),
#endif
                MSOfficeConnectionProvider.Default or
                MSOfficeConnectionProvider.Ace12 => $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path}; OLE DB Services=-1;Persist Security Info=False;",
                MSOfficeConnectionProvider.Ace16 => $"Provider=Microsoft.ACE.OLEDB.16.0;Data Source={path}; OLE DB Services=-1;Persist Security Info=False;",
                _ => throw new NotImplementedException($"Enum Value {provider} has not been implemented yet")
            });

            if (!string.IsNullOrWhiteSpace(dbPassword)) 
                connection.Append($" Jet OLEDB:Database Password={dbPassword};");
            
            return connection.ToString();
        }

        /// <inheritdoc cref="GetConnectionString"/>
        [Obsolete("Please use MSAccessDatabase.GenerateConnectionString() instead.", true)]
        public static string GenerateACEConnectionString(string path, string dbPassword = default) => GetConnectionString(path, dbPassword, MSOfficeConnectionProvider.Ace12);

        /// <summary>
        /// Generate a new OLEDB.ACE database connection
        /// </summary>
        /// <inheritdoc cref="GetConnection"/>
        [Obsolete("Please use MSAccessDatabase.GetConnection(string, MSOfficeConnectionProvider, string) instead.", true)]
        public static OleDbConnection GetACEConnection(string path, string dbPassword = default) => GetConnection(path, dbPassword, MSOfficeConnectionProvider.Ace12);

        /// <inheritdoc cref="GetConnectionString"/>
        [Obsolete("Please use MSAccessDatabase.GenerateConnectionString() instead.", true)]
        public static string GenerateJetConnectionString(string path, string dbPassword = default) => GetConnectionString(path, dbPassword, MSOfficeConnectionProvider.Jet4);

        /// <summary>
        /// Generate a new OLEDB.ACE database connection
        /// </summary>
        /// <inheritdoc cref="GetConnection"/>
        [Obsolete("Please use MSAccessDatabase.GetConnection(string, MSOfficeConnectionProvider, string) instead.", true)]
        public static OleDbConnection GetJetConnection(string path, string dbPassword = default) => GetConnection(path, dbPassword, MSOfficeConnectionProvider.Jet4);

        #endregion
    }
}
