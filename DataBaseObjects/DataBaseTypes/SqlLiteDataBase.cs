using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace RFBCodeWorks.DataBaseObjects.DataBaseTypes
{
    public class SqlLiteDataBase : AbstractDataBase<SqlConnection>
    {
        /// <summary>
        /// Generate a new <see cref="SqlLiteDataBase"/>
        /// </summary>
        public SqlLiteDataBase () : base() { }

        /// <summary>
        /// Generate a new <see cref="SqlLiteDataBase"/>
        /// </summary>
        public SqlLiteDataBase(string connectionString) : base(connectionString) { }

        /// <inheritdoc/>
        public override Compiler Compiler => CompilerSingletons.SqlServerCompiler;

        /// <inheritdoc/>
        public override SqlConnection GetDatabaseConnection()
        {
            return new SqlConnection(this.ConnectionString);
        }
    }
}
