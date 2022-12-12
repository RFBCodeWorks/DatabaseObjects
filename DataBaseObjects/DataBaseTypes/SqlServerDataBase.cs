using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace DataBaseObjects.DataBaseTypes
{
    public class SqlServerDataBase : AbstractDataBase<SqlConnection>
    {
        /// <summary>
        /// Generate a new <see cref="SqlServerDataBase"/>
        /// </summary>
        public SqlServerDataBase() : base() { }

        /// <summary>
        /// Generate a new <see cref="SqlServerDataBase"/>
        /// </summary>
        public SqlServerDataBase(string connectionString) : base(connectionString) { }

        /// <inheritdoc/>
        public override Compiler Compiler => CompilerSingletons.SqlServerCompiler;

        /// <inheritdoc/>
        public override SqlConnection GetDatabaseConnection()
        {
            return new SqlConnection(this.ConnectionString);
        }
    }
}
