using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RFBCodeWorks.DataBaseObjects.Helpers
{
    
    public class StatementBuilderFactory
    {
        /// <summary>
        /// Generate a new <see cref="SelectStatementBuilder"/> object 
        /// </summary>
        /// <returns></returns>
        public SelectStatementBuilder GetSelectStatementBuilder() => new SelectStatementBuilder();

        /// <inheritdoc cref="GetSelectStatementBuilder()"/>
        public SelectStatementBuilder GetSelectStatementBuilder(string tableName) => new SelectStatementBuilder() { From = tableName };

        /// <inheritdoc cref="GetSelectStatementBuilder()"/>
        public SelectStatementBuilder GetSelectStatementBuilder(SqlKata.Query query) => new SelectStatementBuilder() { From = query };
    }

    /// <summary>
    /// Class used to help consumers build select statements
    /// </summary>
    public class SelectStatementBuilder
    {

        public SelectStatementBuilder() { }

        /// <summary>
        /// Treat the <see cref="SelectStatementBuilder"/> as a function to get a query
        /// </summary>
        /// <param name="builder"></param>
        public static implicit operator Func<SqlKata.Query,SqlKata.Query>(SelectStatementBuilder builder) => (o) => builder.GenerateQuery();

        public List<string> ReturnColumns { get; } = new List<string>();
        public object From {
            get => fromValue; 
            set
            {
                if (value is SqlKata.Query | value is string | value is SelectStatementBuilder)
                    fromValue = value;
                else
                    throw new ArgumentException("Invalid Object Type for 'FROM' value - Expected a SqlKata.Query, a string, or a SelectStatementBuilder");
            }
        }

        private object fromValue;
        
        /// <summary>
        /// The collection of 'Where' statements
        /// </summary>
        public List<WhereStatementHelper> WhereStatements { get; } = new List<WhereStatementHelper>();

        /// <summary>
        /// Specify the Alias for this query result to be referenced from other queries
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Compile the parameters from this object into a new Query object
        /// </summary>
        /// <returns>a new <see cref="SqlKata.Query"/> object representing this object</returns>
        public SqlKata.Query GenerateQuery()
        {
            SqlKata.Query qry = new SqlKata.Query();
            qry.Select(ReturnColumns.Count > 0 ? ReturnColumns.ToArray() : new string[] { "*" });

            if (From is string) qry.From((string)From);
            if (From is SqlKata.Query) qry.From((SqlKata.Query)From);
            if (From is SelectStatementBuilder ) 
                qry.From((SelectStatementBuilder)From);

            bool firstStatement = true;
            foreach (var w in WhereStatements)
            {
                w.ApplyStatement(qry, firstStatement);
                firstStatement = false;
            }

            if (!string.IsNullOrWhiteSpace(Alias)) qry.As(Alias);
            return qry;
        }


        /// <summary>
        /// Compile the query using the supplied <see cref="SqlKata.Compilers.Compiler"/>
        /// </summary>
        /// <returns></returns>
        public string GenerateSql(SqlKata.Compilers.Compiler compiler)
        {
            return compiler.Compile(GenerateQuery()).ToString();
        }

        /// <summary>
        /// Compile the query and return it as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.GenerateQuery().ToString();
        }

        /// <summary>
        /// Add a new 'Where' statement to the collection
        /// </summary>
        /// <param name="columnID"></param>
        /// <param name="columnValue"></param>
        /// <param name="op"></param>
        /// <param name="andOr"></param>
        /// <param name="isCaseSensitive"></param>
        public void AddWhereStatement(string columnID, string columnValue, StringOperators op, AndOr andOr = AndOr.AND, bool isCaseSensitive = false)
        {
            this.WhereStatements.Add(new WhereStatementHelper(
                columnName: columnID,
                columnValue: columnValue,
                op: op,
                andOr: andOr,
                isCaseSensitive
                )
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnID"></param>
        /// <param name="expectedValue"></param>
        /// <param name="andor"></param>
        public void AddWhereTrueStatement(string columnID, bool expectedValue, AndOr andor = AndOr.AND)
        {
            this.WhereStatements.Add(new WhereStatementHelper(columnID, expectedValue, andor));
        }

        /// <summary>
        /// Add a new 'Where' statement to the collection
        /// </summary>
        /// <param name="columnID"></param>
        /// <param name="columnValue"></param>
        /// <param name="op"></param>
        /// <param name="andOr"></param>
        public void AddWhereStatement(string columnID, int columnValue, NumericOperators op, AndOr andOr = AndOr.AND)
        {
            this.WhereStatements.Add(new WhereStatementHelper(
                columnName: columnID,
                columnValue: columnValue,
                op: op,
                andOr: andOr
                ));
        }

        ///// <summary>
        ///// Sanitzes a search term
        ///// </summary>
        ///// <param name="searchTerm">search term to sanitize</param>
        ///// <param name="sanitizeOptions"></param>
        ///// <returns>the sanitized search term</returns>
        //public string PreProcessSearchTerm(string searchTerm, SanitizeOptions sanitizeOptions = SanitizeOptions.None)
        //{
        //    if (searchTerm.IsNullOrEmpty()) return string.Empty;
        //    int L;
        //    string c, tmp = string.Empty, retStr = string.Empty;
        //    searchTerm = searchTerm.Trim();
        //    L = searchTerm.Length;


        //    // Initial Processing
        //    for (int i = 0; i < L; i++)
        //    {
        //        c = searchTerm.Substring(i, 1);
        //        switch (c)
        //        {
        //            case "\\":
        //            case "/":
        //            case ",":
        //            case "?":
        //            case "*":
        //            case "%":
        //            case "^":
        //                c = WildCard.ToString();
        //                break;
        //        }
        //        tmp = tmp + c;
        //    }

        //    // Evaluate the Filter Combox(es) to setup the wildcards
        //    switch (true)
        //    {
        //        case true when sanitizeOptions.HasFlag(SanitizeOptions.None): break;
        //        case true when sanitizeOptions.HasFlag(SanitizeOptions.Contains):
        //            tmp = WildCard + tmp + WildCard;
        //            break;
        //        case true when sanitizeOptions.HasFlag(SanitizeOptions.StartsWith):
        //            tmp = tmp + WildCard;
        //            break;
        //    }

        //    //Encapsulate to preserve formatting for SQL statement
        //    tmp = "'" + tmp.ToUpper() + "'";

        //    //Remove Duplicate WildCards
        //    L = tmp.Length;
        //    retStr = string.Empty;
        //    for (int i = 0; i < L; i++)
        //    {
        //        c = tmp.Substring(i, 1); //set character value
        //        if (i == L)
        //        {
        //            //Nothing to do
        //        }
        //        else if (tmp.Substring(i, 2) == $"{WildCard}{WildCard}")
        //        {
        //            //Duplicate Wildcard found
        //            c = WildCard.ToString(); //Convert to single wildcard
        //            i++;  //Increase I count to move past removed wildcard
        //        }
        //        retStr += c;
        //    }
        //    return retStr;
        //}


        //private string GetReturnColumns()
        //{
        //    string r = "";
        //    foreach (string s in ReturnColumns)
        //    {
        //        if (s.IsNullOrEmpty()) continue:
        //                if (r == string.Empty)
        //            r = s.WrapBrackets();
        //        else
        //            r += ", " + s.WrapBrackets();
        //    }
        //    return r;
        //}

        //private string GetWhereStatements()
        //{
        //    if (WhereStatements.Count < 1) return string.Empty;
        //    string r = " WHERE ";
        //    bool firstDone = false;
        //    foreach (string s in WhereStatements)
        //    {
        //        if (s.IsNullOrEmpty()) continue:
        //                if (!firstDone)
        //        {
        //            firstDone = true;
        //            r += s;
        //        }
        //        else
        //            r += " AND " + s;
        //    }
        //    return r;
        //}


    }
}
