using System;
using System.Linq;
using System.Text.RegularExpressions;
using SqlKata.Compilers;

namespace SqlKata.Compilers
{
    /// <summary>
    /// Compiler to use when working with an Excel Workbook
    /// </summary>
    public class ExcelWorkbookCompiler : SqlKata.Compilers.Compiler
    {

        /*  
         *  https://docs.microsoft.com/en-us/power-automate/desktop-flows/how-to/sql-queries-excel
         *  
         *  Sheet Names MUST be in the following syntax:  [SHEET$]
         *  Column Names are specified as the columns in the first row of the excel worksheet
         *  DELETE does not work in excel! But UPDATE does work
         * 
         */


        public ExcelWorkbookCompiler()
        {
            this.userOperators.Add("alike");
            this.userOperators.Add("not alike");
            OpeningIdentifier = "[";
            ClosingIdentifier = "]";
        }
        
        /// <inheritdoc/>
        public override string EngineCode => "ExcelWorkbook";

        /// <remarks> Generates the SheetName expression </remarks>
        /// <inheritdoc/>
        public override string CompileTableExpression(SqlResult ctx, AbstractFrom from)
        {
            if (from is FromClause fromClause)
            {
                string ret = Wrap(fromClause.Table);
                if (ret.EndsWith("$]")) return ret;
                if (Regex.IsMatch(ret, @"^\[+.+\$\]+", RegexOptions.Compiled)) return ret;
                return Regex.Replace(ret, @"(?<!])]", "$$]", RegexOptions.Compiled).Replace("$$]", "$]"); // Replace the first instance of ']' with '$]'
            }
            else
                return base.CompileTableExpression(ctx, from);
        }

        /// <remarks>
        /// Wraps the SheetNames
        /// </remarks>
        /// <inheritdoc/>
        public override string WrapValue(string value) => base.WrapValue(value);

        public string SanitizeConditionValue(object conditionValue) => "'" + conditionValue.ToString() + "'";

        /// <inheritdoc/>
        protected override string CompileBasicCondition(SqlResult ctx, BasicCondition x)
        {
            x.Value = SanitizeConditionValue(x.Value);
            return base.CompileBasicCondition(ctx, x);
        }

        /// <inheritdoc/>
        //public override string CompileTrue() => SanitizeConditionValue(base.CompileTrue());

        /// <inheritdoc/>
        //public override string CompileFalse() => SanitizeConditionValue(base.CompileFalse());

        /// <inheritdoc/>
        protected override string CompileBasicStringCondition(SqlResult ctx, BasicStringCondition x)
        {
            if (new[] { "like", "ilike", "alike" }.Contains(x.Operator.ToLowerInvariant()))
            {
                // Convert 'LIKE' to 'ALIKE' and replace any asterisks with percent symbols.
                x.Operator = "ALIKE";
                if (x.Value is string)
                    x.Value = ((string)x.Value).Replace('*', '%');
            }
            x.Value = SanitizeConditionValue(x.Value);
            return base.CompileBasicStringCondition(ctx, x);
        }

        public override string CompileLimit(SqlResult ctx)
        {
            // ACCESS does not support the 'Limit X' command, use 'TOP' instead -> See the ColumnCompiler override
            return null;
        }

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
                    return "SELECT DISTINCT TOP (?)" + compiled.Substring(15);
                }

                return "SELECT TOP (?)" + compiled.Substring(6);
            }

            return compiled;
        }

        public override SqlResult GetNewSqlResult()
        {
            return new MSAccessCompiler.MsAccessSqlResult();
        }
    }
}
