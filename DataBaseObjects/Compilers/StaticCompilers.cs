using System;
using System.Collections.Generic;
using System.Text;

namespace SqlKata.Compilers
{
    /// <summary>
    /// Static class that contains the compiler singletons
    /// </summary>
    public static class CompilerSingletons
    {
        private static SqlKata.Compilers.MSAccessCompiler MSAccessCompilerField;
        private static SqlKata.Compilers.FirebirdCompiler FirebirdCompilerField;
        private static SqlKata.Compilers.MySqlCompiler MySqlCompilerField;
        private static SqlKata.Compilers.OracleCompiler OracleCompilerField;
        private static SqlKata.Compilers.PostgresCompiler PostGresCompilerField;
        private static SqlKata.Compilers.SqliteCompiler SqlLiteCompilerField;
        private static SqlKata.Compilers.SqlServerCompiler SqlServerCompilerField;
        private static SqlKata.Compilers.ExcelWorkbookCompiler ExcelWorkbookCompilerField;

        public static SqlKata.Compilers.ExcelWorkbookCompiler ExcelWorkbookCompiler
        {
            get
            {
                if (ExcelWorkbookCompilerField is null) ExcelWorkbookCompilerField = new ExcelWorkbookCompiler();
                return ExcelWorkbookCompilerField;
            }
        }

        public static SqlKata.Compilers.MSAccessCompiler MSAccessCompiler
        {
            get {
                if (MSAccessCompilerField is null) MSAccessCompilerField = new MSAccessCompiler();
                return MSAccessCompilerField; 
            }
        }

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
