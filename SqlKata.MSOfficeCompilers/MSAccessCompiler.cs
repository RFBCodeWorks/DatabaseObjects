using System;
using System.Linq;
using System.Text.RegularExpressions;
using SqlKata.Compilers;
using SqlKata;

namespace RFBCodeWorks.SqlKata.MsOfficeCompilers
{
    /// <summary>
    /// Compiler to use when working with an MS Access DB
    /// </summary>
    public class MSAccessCompiler : Compiler
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

        /// <inheritdoc cref="MSAccessCompiler.MSAccessCompiler()"/>
        /// <param name="daoMode"><inheritdoc cref="DaoMode" path="*"/></param>
        public MSAccessCompiler(bool daoMode) : this() 
        { 
            DaoMode = daoMode;
        }

        /// <inheritdoc/>
        public override string EngineCode => "MSAccess";

        /// <summary>
        /// Connecting to MSAccess using an OLEDBConnection requires use of the 'ALIKE' operator, and '%' wildcards. This is the default functionality. 
        /// <br/> When set <see langword="false"/> (default): Sanitize 'like' and 'ilike' to 'ALIKE'. Any string conditions will be sanitized through <see cref="SanitizeWildCards(string)"/>
        /// <br/> If this is set <see langword="true"/>, do not sanitize as detailed above.
        /// </summary>
        /// <remarks>
        /// DaoMode (true) = Ansi-89
        /// <br/> Default (false) = Ansi-92
        /// <br/><see href="https://support.microsoft.com/en-us/office/access-wildcard-character-reference-af00c501-7972-40ee-8889-e18abaad12d1#bmansi89"/>
        /// </remarks>
        public bool DaoMode { get; set; }

        /// <inheritdoc/>
        protected override string CompileBasicStringCondition(SqlResult ctx, BasicStringCondition x)
        {
            if (!DaoMode) SanitizeWildCards(x);
            return base.CompileBasicStringCondition(ctx, x);
        }

        private static readonly string[] likeOperators = new string[] { "like", "ilike", "alike" };

        /// <summary>
        /// Modifies the 'like' and 'ilike'  operators to 'alike' for Access / Excel compatibility
        /// </summary>
        internal static void SanitizeWildCards(BasicStringCondition x)
        {
            if (likeOperators.Contains(x.Operator.ToLowerInvariant()))
            {
                x.Operator = "ALIKE";
                if (x.Value is string val)
                {
                    x.Value = SanitizeWildCards(val);
                }
            }
        }

        //regex to locate unescaped wildcards to ensure the escaped ones aren't replaced.
        // Ex: Search[*]Term --> Search[*]Term  (Unaffected since no match)
        // Ex: Search*Term --> Search%Term      (Wildcard replaced)                         // -89  -92
        private const RegexOptions regexOptions = RegexOptions.CultureInvariant;
        private static readonly Regex wcA = new Regex(@"(?<!\[)(\*(?!=\]))", regexOptions);  //  *   %
        private static readonly Regex wcQ = new Regex(@"(?<!\[)(\?(?!=\]))", regexOptions);  //  ?   _
        private static readonly Regex wcE = new Regex(@"(\[!(?!=\]))", regexOptions);        //  !   ^
        private static readonly Regex wcP = new Regex(@"(?<!\[)(\#(?!=\]))", regexOptions);  //  #   Doesn't Exist - Subsitute for [0-9]
        // Regex to locate underscores and percent symbols that are literal in ansi-89, but need to be escaped for ansi-92
        private static readonly Regex wc92P = new Regex(@"(?<!\[)(%(?!=\]))", regexOptions);
        private static readonly Regex wc92U = new Regex(@"(?<!\[)(_(?!=\]))", regexOptions);
        //Regex to locate ansi-89 escaped characters so they can be converted to their literal ansi-92 forms
        private static readonly Regex wc89 = new Regex(@"(\[[\*\?\!\#]\])", regexOptions);

        /// <summary>
        /// Runs the <paramref name="searchTerm"/> through several regex.Replace() calls to sanitize the string from the ANSI-89 format to the ANSI-92 format.
        /// </summary>
        /// <param name="searchTerm">The string to sanitize</param>
        /// <returns>An ANSI-92 compatible string</returns>
        /// <remarks>
        /// Replaces the wildcards per  the following documentation page:
        /// <br/><see href="https://support.microsoft.com/en-us/office/access-wildcard-character-reference-af00c501-7972-40ee-8889-e18abaad12d1#bmansi89"/>
        /// </remarks>
        public static string SanitizeWildCards(string searchTerm)
        {
            //Any characters not already escaped
            searchTerm = wc92P.Replace(searchTerm, "[%]");
            searchTerm = wc92U.Replace(searchTerm, "[_]");

            //Convert Wildcards
            searchTerm = wcA.Replace(searchTerm, "%");
            searchTerm = wcQ.Replace(searchTerm, "_");
            searchTerm = wcE.Replace(searchTerm, "[^");
            searchTerm = wcP.Replace(searchTerm, "[0-9]");

            //Deconvert ANSI-89 escaped wildcards to standard string
            var matches = wc89.Matches(searchTerm);
            foreach (Match m in matches)
                searchTerm = searchTerm.Replace(m.Value, m.Value[1].ToString());
            return searchTerm;
        }
        

        /// <inheritdoc/>
        public override string CompileJoin(SqlResult ctx, Join join, bool isNested = false)
        {
            var val = base.CompileJoin(ctx, join, isNested);
            if (val is null) return null;
            return CompileJoin(val);
        }

        /// <summary>
        /// Takes the result of <see cref="CompileJoin(SqlResult, Join, bool)"/> and sanitizes it for Access &amp; Excel
        /// </summary>
        internal static string CompileJoin(string compiledJoin)
        {
            Regex CompileJoinOnRegex = new Regex(@".+?\sON\s(?<CLAUSE>.*)", RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            var match = CompileJoinOnRegex.Match(compiledJoin);
            if (match.Success)
            {
                string clause = match.Groups["CLAUSE"].Value;
                return compiledJoin.Replace(clause, "(" + clause + ")");
            }
            else
            {
                return compiledJoin;
            }
        }

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

        /// <remarks> MS Access does not support this function </remarks>
        /// <inheritdoc/>
        public override string CompileLower(string value) => value;

        /// <remarks> MS Access does not support this function </remarks>
        /// <inheritdoc/>
        public override string CompileUpper(string value) => value;

        ////language=Regex
        //static Regex ColumnRegex { get; } = new Regex(@"^\[.+?\]", RegexOptions.Compiled);

        /// <inheritdoc/>
        public override string WrapValue(string value)
        {
            if (value.StartsWith(OpeningIdentifier) && value.EndsWith(ClosingIdentifier)) return value;
            //if (ColumnRegex.IsMatch(value)) return value;
            return base.WrapValue(value);
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
    }
}
