using Microsoft.Data.SqlClient;
using SqlKata.Compilers;

namespace RFBCodeWorks.DataBaseObjects.DataBaseTypes
{
    /// <summary>
    /// Base Database class for a Sql Server Database
    /// <br/> Utilizes <see cref="Microsoft.Data.SqlClient.SqlConnection" />
    /// </summary>
    public class SqlServerDataBase : AbstractDataBase<SqlConnection>
    {
        private static readonly Compiler DefaultCompiler = new SqlKata.Compilers.SqlServerCompiler();

        /// <summary>
        /// Generate a new <see cref="SqlServerDataBase"/>
        /// </summary>
        /// <inheritdoc/>
        protected SqlServerDataBase() : base() { }

        /// <summary>
        /// Generate a new <see cref="SqlServerDataBase"/>
        /// </summary>
        /// <param name="connectionString">
        /// The connection string to the database. 
        /// <br/>For help see: 
        /// <br/> <see href="https://www.connectionstrings.com/sql-server/"/>
        /// </param>
        public SqlServerDataBase(string connectionString) : base(connectionString) { }

        /// <inheritdoc/>
        public override Compiler Compiler => DefaultCompiler;

        /// <inheritdoc/>
        public override SqlConnection GetDatabaseConnection()
        {
            return new SqlConnection(this.ConnectionString);
        }

        /// <summary>
        /// Generate a new <see cref="SqlConnection"/>
        /// </summary>
        /// <param name="path">path to the database</param>
        /// <param name="password"></param>
        /// <returns>new <see cref="OleDbConnection"/></returns>
        public static SqlConnection GenerateSqlConnection(string connectionString, SqlCredential credentials = null)
        {
            return credentials is null ? new SqlConnection(connectionString) : new SqlConnection(connectionString, credentials);
        }

        /// <summary>
        /// Generate a new <see cref="SqlConnection"/>
        /// </summary>
        /// <param name="path">path to the database</param>
        /// <param name="password"></param>
        /// <returns>new <see cref="OleDbConnection"/></returns>
        public static SqlConnection GenerateSqlConnection(SqlConnectionStringBuilder connectionString, SqlCredential credentials = null)
        {
            return credentials is null ? new SqlConnection(connectionString.ConnectionString) : new SqlConnection(connectionString.ConnectionString, credentials);
        }

        /// <summary>
        /// Generates a new <see cref="SqlConnectionStringBuilder"/> with the provided parameters - meant for communicating with SQLServer
        /// </summary>
        /// <param name="serverAddress">Server Location to reach the db</param>
        /// <param name="dataSource">database name</param>
        /// <param name="userID"></param>
        /// <param name="password"></param>
        /// <param name="encrypt"></param>
        /// <returns></returns>
        public static SqlConnectionStringBuilder GenerateSqlServerConnectionString(string serverAddress, string dataSource, string userID = "", string password = "", bool encrypt = false)
        {
            var bldr = new SqlConnectionStringBuilder();
            bldr["Server"] = serverAddress;
            bldr.DataSource = dataSource;
            bldr.ConnectRetryCount = 3;
            bldr.Encrypt = encrypt;
            bldr.UserID = userID;
            bldr.Password = password;
            return bldr;
        }
    }
}
