using System;
using System.Linq;
using System.Text.RegularExpressions;
using SqlKata.Compilers;
using SqlKata;

namespace RFBCodeWorks.SqlKataCompilers
{
    /// <summary>
    /// Compiler to use when working with an MS Access DB
    /// </summary>
    public class MSAccessCompiler : SqlKata.Compilers.Compiler
    {

        /// <summary>
        /// A singleton compiler for MS Access
        /// </summary>
        public static MSAccessCompiler AccessCompiler
        {
            get
            {
                if (MSAccessCompilerField is null) MSAccessCompilerField = new MSAccessCompiler();
                return MSAccessCompilerField;
            }
        }
        private static MSAccessCompiler MSAccessCompilerField;

        /// <summary>
        /// Create the SqlKata Compiler for an Access database
        /// </summary>
        public MSAccessCompiler()
        {
            this.userOperators.Add("alike");
            this.userOperators.Add("not alike");
            OpeningIdentifier = "[";
            ClosingIdentifier = "]";
        }

        /// <inheritdoc/>
        public override string EngineCode => "MSAccess";

        private static Regex CompileJoinOnRegex = new Regex(@".+?\sON\s(?<CLAUSE>.*)", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string CompileJoin(SqlResult ctx, Join join, bool isNested = false)
        {
            var val = base.CompileJoin(ctx, join, isNested);
            if (val is null) return null;

            var match = CompileJoinOnRegex.Match(val);
            if (match.Success)
            {
                string clause = match.Groups["CLAUSE"].Value;
                return val.Replace(clause, "(" + clause + ")");
            }
            else
            {
                return val;
            }
        }

        /// <inheritdoc/>
        protected override string CompileBasicStringCondition(SqlResult ctx, BasicStringCondition x)
        {
            if (new[] { "like", "ilike", "alike" }.Contains(x.Operator.ToLowerInvariant()))
            {
                // Convert 'LIKE' to 'ALIKE' and replace any asterisks with percent symbols.
                x.Operator = "ALIKE";
                if (x.Value is string val)
                {
                    if (!val.StartsWith("\""))
                        val = "%" + val + "%";
                    x.Value = val.Replace('*', '%').Replace("%%", "%");

                }
            }
            string baseRet = base.CompileBasicStringCondition(ctx, x);
            return baseRet;
        }

        /// <remarks> MS Access does not support this function </remarks>
        /// <inheritdoc/>
        public override string CompileLower(string value) => value;

        /// <remarks> MS Access does not support this function </remarks>
        /// <inheritdoc/>
        public override string CompileUpper(string value) => value;


        /// <summary>
        /// MS Access does not support this function - ColumnCompiler uses 'TOP' instead<br/>
        /// This translation is already handled by this compiler.
        /// </summary>
        /// <returns><see langword="null"/></returns>
        /// <inheritdoc/>
        public override string CompileLimit(SqlResult ctx)
        {
            // ACCESS does not support the 'Limit X' command, use 'TOP' instead -> See the ColumnCompiler override
            return null;
        }

        /// <inheritdoc/>
        public override string Wrap(string value)
        {
            return base.Wrap(value);
        }

        /// <inheritdoc/>
        public override string WrapIdentifiers(string input)
        {
            return base.WrapIdentifiers(input);
        }

        //language=Regex
        static Regex ColumnRegex { get; } = new Regex(@"^\[.+?\]", RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string WrapValue(string value)
        {
            if (value.StartsWith(OpeningIdentifier) && value.EndsWith(ClosingIdentifier)) return value;
            //if (ColumnRegex.IsMatch(value)) return value;
            return base.WrapValue(value);
        }

        /// <inheritdoc/>
        protected override string CompileBasicCondition(SqlResult ctx, BasicCondition x)
        {
            return base.CompileBasicCondition(ctx, x);
        }

        /// <inheritdoc/>
        protected override string CompileColumns(SqlResult ctx)
        {
            // Add in the 'TOP' command, similar to legacy mode sql server: https://github.com/sqlkata/querybuilder/blob/044c7ae48591ed956ab0ffb33c556b9e59ea9d6d/QueryBuilder/Compilers/SqlServerCompiler.cs#L73

            var compiled = base.CompileColumns(ctx);

            // If there is a limit on the query, but not an offset, we will add the top
            // clause to the query, which serves as a "limit" type clause within the
            // SQL Server system similar to the limit keywords available in MySQL.
            var limit = ctx.Query.GetLimit(this);
            var offset = ctx.Query.GetOffset(this);

            if (limit > 0 && offset == 0)
            {
                // top bindings should be inserted first
                ctx.Bindings.Insert(0, limit);

                ctx.Query.ClearComponent("limit");

                // handle distinct
                if (compiled.IndexOf("SELECT DISTINCT") == 0)
                {
                    return "SELECT DISTINCT TOP ?" + compiled.Substring(15);
                }

                return "SELECT TOP ?" + compiled.Substring(6);
            }

            return compiled;
        }

        //public override SqlResult GetNewSqlResult()
        //{
        //    return new MsAccessSqlResult();
        //}

        //internal class MsAccessSqlResult : SqlKata.SqlResult
        //{
        //    public MsAccessSqlResult() { }
        //    public MsAccessSqlResult(SqlResult ctx)
        //    {
        //        base.Query = ctx.Query;
        //        RawSql = ctx.RawSql;
        //        Bindings = ctx.Bindings;
        //        Sql = ctx.Sql;
        //        NamedBindings = base.NamedBindings;
        //    }

        //    //private const char sw = '"';
        //    //private const string stringwrapper = "\"";
        //    ////language=Regex
        //    //static Regex ValueRegex { get; } = new Regex(@"^" + stringwrapper + @".+?" + stringwrapper, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        //    protected override string WrapStringValue(string value)
        //    {
        //        //base = "'" + value.ToString().Replace("'", "''") + "'";

        //        return '"' + value.Replace("\"", "\"\"") + '"';
        //        //return base.WrapStringValue(value);
                
        //        //if (value[0] == sw && value.Last() == sw) return value;
        //        ////if (ValueRegex.IsMatch(value)) return value;
        //        //return stringwrapper + value + stringwrapper;
        //    }

        //}
    }
}
