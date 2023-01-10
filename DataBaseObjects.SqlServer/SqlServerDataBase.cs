using Microsoft.Data.SqlClient;
using SqlKata.Compilers;

namespace RFBCodeWorks.DataBaseObjects.DataBaseTypes
{
    /// <summary>
    /// Base Database class for a Sql Server Database
    /// <br/> Utilizes <see cref="Microsoft.Data.SqlClient.SqlConnection" />
    /// </summary>
    public class SqlServerDataBase : AbstractDataBase<SqlConnection, SqlCommand>
    {
        private static readonly Compiler DefaultCompiler = new SqlServerCompiler();

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
        public override SqlConnection GetConnection()
        {
            return new SqlConnection(this.ConnectionString);
        }

        /// <summary>
        /// Generate a new <see cref="SqlConnection"/>
        /// </summary>
        /// <returns>new <see cref="SqlConnection"/></returns>
        /// <inheritdoc cref="SqlConnection.SqlConnection(string, SqlCredential)"/>
        public static SqlConnection GenerateSqlConnection(string connectionString, SqlCredential credential = null)
        {
            return credential is null ? new SqlConnection(connectionString) : new SqlConnection(connectionString, credential);
        }

        /// <summary>
        /// Generate a new <see cref="SqlConnection"/>
        /// </summary>
        /// <param name="connectionString">A <see cref="SqlConnectionStringBuilder"/> that will provide its connection string</param>
        /// <returns>new <see cref="SqlConnection"/></returns>
        /// <inheritdoc cref="SqlConnection.SqlConnection(string, SqlCredential)"/>
        /// <param name="credential"/>
        public static SqlConnection GenerateSqlConnection(SqlConnectionStringBuilder connectionString, SqlCredential credential = null)
        {
            return credential is null ? new SqlConnection(connectionString.ConnectionString) : new SqlConnection(connectionString.ConnectionString, credential);
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
            return new SqlConnectionStringBuilder
            {
                ["Server"] = serverAddress,
                DataSource = dataSource,
                ConnectRetryCount = 3,
                Encrypt = encrypt,
                UserID = userID ?? string.Empty,
                Password = password
            };
        }
    }
}
