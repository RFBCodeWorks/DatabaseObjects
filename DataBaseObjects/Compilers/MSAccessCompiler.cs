using System;
using System.Linq;
using System.Text.RegularExpressions;
using SqlKata.Compilers;

namespace SqlKata.Compilers
{
    /// <summary>
    /// Compiler to use when working with an MS Access DB
    /// </summary>
    public class MSAccessCompiler : SqlKata.Compilers.Compiler
    {
        public MSAccessCompiler()
        {
            this.userOperators.Add("alike");
            this.userOperators.Add("not alike");
            OpeningIdentifier = "[";
            ClosingIdentifier = "]";
        }

        /// <inheritdoc/>
        public override string EngineCode => "MSAccess";

        public override string CompileJoin(SqlResult ctx, Join join, bool isNested = false)
        {
            var val = base.CompileJoin(ctx, join, isNested);
            if (val is null) return null;
            
            Regex OnClause = new(@".+?\sON\s(?<CLAUSE>.*)", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            if (OnClause.IsMatch(val, out var Match))
            {
                string clause = Match.Groups["CLAUSE"].Value;
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

        /// <remarks> MS Access does not support this function - ColumnCompiler uses 'TOP' instead</remarks>
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
            if (ColumnRegex.IsMatch(value)) return value;
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

        public override SqlResult GetNewSqlResult()
        {
            return new MsAccessSqlResult();
        }

        internal class MsAccessSqlResult : SqlKata.SqlResult
        {
            public MsAccessSqlResult() { }
            public MsAccessSqlResult(SqlResult ctx)
            {
                base.Query = ctx.Query;
                RawSql = ctx.RawSql;
                Bindings = ctx.Bindings;
                Sql = ctx.Sql;
                NamedBindings = base.NamedBindings;
            }

            private const string stringwrapper = "\"";
            //language=Regex
            static Regex ValueRegex { get; } = new Regex(@"^" + stringwrapper + @".+?" + stringwrapper, RegexOptions.Compiled);

            protected override string WrapStringValue(string value)
            {
                if (ValueRegex.IsMatch(value)) return value;
                return stringwrapper + value + stringwrapper;
            }

        }
    }
}
