using SqlKata.Compilers;
using Microsoft.Data.Sqlite;

namespace RFBCodeWorks.DatabaseObjects.DatabaseTypes
{
    /// <summary>
    /// Base Database class for a SqlLite Database
    /// </summary>
    public class SqliteDataBase : AbstractDatabase<SqliteConnection, SqliteCommand>
    {
        private static readonly Compiler DefaultCompiler = new CorrectedCompiler();

        // Corrects the following issue: https://github.com/sqlkata/querybuilder/issues/655
        private class CorrectedCompiler : SqliteCompiler 
        { 
            public CorrectedCompiler() : base()
            { 
                this.OpeningIdentifier = "[";
                this.ClosingIdentifier = "]";
            } 
        }

        /// <summary>
        /// Generate a new <see cref="SqliteDataBase"/>
        /// </summary>
        /// <inheritdoc/>
        protected SqliteDataBase() : base() { }

        /// <summary>
        /// Generate a new <see cref="SqliteDataBase"/>
        /// </summary>
        /// <param name="connectionString">
        /// The connection string to the database. 
        /// <br/>For help see: 
        /// <br/> <see href="https://www.connectionstrings.com/sqlite/"/>
        /// </param>
        public SqliteDataBase(string connectionString) : base(connectionString) {  }

        /// <inheritdoc/>
        public override Compiler Compiler => DefaultCompiler;

        /// <inheritdoc/>
        public override SqliteConnection GetConnection()
        {
            return new SqliteConnection(this.ConnectionString);
        }

        /// <summary>
        /// Generates a new <see cref="SqliteConnectionStringBuilder"/> with the provided parameters
        /// </summary>
        /// <param name="path">Path to the database</param>
        /// <returns>A new <see cref="SqliteConnectionStringBuilder"/> with the DataSource set</returns>
        public static SqliteConnectionStringBuilder GenerateSqlLiteConnectionString(string path)
        {
            var bldr = new SqliteConnectionStringBuilder
            {
                DataSource = path
            };
            return bldr;
        }


        /// <param name="password">The database Password</param>
        /// <returns>A new <see cref="SqliteConnectionStringBuilder"/> with the DataSource and password set</returns>
        /// <inheritdoc cref="GenerateSqlLiteConnectionString(string)"/>
        /// <param name="path"/>
        public static SqliteConnectionStringBuilder GenerateSqlLiteConnectionString(string path, string password = "")
        {
            var bldr = GenerateSqlLiteConnectionString(path);
            bldr.Password = password;
            return bldr;
        }
    }
}
