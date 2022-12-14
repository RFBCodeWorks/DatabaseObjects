using SqlKata.Compilers;
using Microsoft.Data.Sqlite;

namespace RFBCodeWorks.DataBaseObjects.DataBaseTypes
{
    /// <summary>
    /// Base Database class for a SqlLite Database
    /// </summary>
    public class SqlLiteDataBase : AbstractDataBase<SqliteConnection>
    {
        private static readonly Compiler DefaultCompiler = new SqlKata.Compilers.SqliteCompiler();

        /// <summary>
        /// Generate a new <see cref="SqlLiteDataBase"/>
        /// </summary>
        /// <inheritdoc/>
        protected SqlLiteDataBase() : base() { }

        /// <summary>
        /// Generate a new <see cref="SqlLiteDataBase"/>
        /// </summary>
        /// <param name="connectionString">
        /// The connection string to the database. 
        /// <br/>For help see: 
        /// <br/> <see href="https://www.connectionstrings.com/sqlite/"/>
        /// </param>
        public SqlLiteDataBase(string connectionString) : base(connectionString) { }

        /// <inheritdoc/>
        public override Compiler Compiler => DefaultCompiler;

        /// <inheritdoc/>
        public override SqliteConnection GetDatabaseConnection()
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
            var bldr = new SqliteConnectionStringBuilder();
            bldr.DataSource = path;
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
