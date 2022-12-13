using System;
using System.Collections.Generic;
using System.Text;

namespace SqlKata.Compilers
{
    /// <summary>
    /// Static class that contains the compiler singletons
    /// </summary>
    internal static class CompilerSingletons
    {
        private static SqlKata.Compilers.FirebirdCompiler FirebirdCompilerField;
        private static SqlKata.Compilers.MySqlCompiler MySqlCompilerField;
        private static SqlKata.Compilers.OracleCompiler OracleCompilerField;
        private static SqlKata.Compilers.PostgresCompiler PostGresCompilerField;
        private static SqlKata.Compilers.SqliteCompiler SqlLiteCompilerField;
        private static SqlKata.Compilers.SqlServerCompiler SqlServerCompilerField;
        
        public static SqlKata.Compilers.FirebirdCompiler FirebirdCompiler
        {
            get
            {
                if (FirebirdCompilerField is null) FirebirdCompilerField = new FirebirdCompiler();
                return FirebirdCompilerField;
            }
        }

        public static SqlKata.Compilers.MySqlCompiler MySqlCompiler
        {
            get
            {
                if (MySqlCompilerField is null) MySqlCompilerField= new MySqlCompiler();
                return MySqlCompilerField;
            }
        }

        public static SqlKata.Compilers.OracleCompiler OracleCompiler
        {
            get
            {
                if (OracleCompilerField is null) OracleCompilerField = new OracleCompiler();
                return OracleCompilerField;
            }
        }

        public static SqlKata.Compilers.PostgresCompiler PostgresCompiler
        {
            get
            {
                if (PostGresCompilerField is null) PostGresCompilerField = new PostgresCompiler();
                return PostGresCompilerField;
            }
        }

        public static SqlKata.Compilers.SqliteCompiler SqliteCompiler
        {
            get
            {
                if (SqlLiteCompilerField is null) SqlLiteCompilerField = new SqliteCompiler();
                return SqlLiteCompilerField;
            }
        }

        public static SqlKata.Compilers.SqlServerCompiler SqlServerCompiler
        {
            get
            {
                if (SqlServerCompilerField is null) SqlServerCompilerField = new SqlServerCompiler();
                return SqlServerCompilerField;
            }
        }
    }
}
