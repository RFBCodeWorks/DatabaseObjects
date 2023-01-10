using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using SK = SqlKata;

namespace RFBCodeWorks.SqlKata.Extensions
{
    
    /// <summary>
    /// Helper Class used to help consumers build select statements
    /// </summary>
    public class SelectStatementBuilder
    {

        private SelectStatementBuilder() { }

        /// <summary>
        /// Create a new SelectStatementBuilder whose 'FROM' statement is the <paramref name="tableName"/>
        /// </summary>
        /// <param name="tableName">The name of the table to select from</param>
        public SelectStatementBuilder(string tableName) : this()
        {
            From = tableName;
        }

        /// <summary>
        /// Create a new SelectStatementBuilder whose 'FROM' statement is the <paramref name="query"/>
        /// </summary>
        /// <param name="query">The SqlKata Query that provides the inner query to select from</param>
        public SelectStatementBuilder(SK.Query query)  : this()
        {
            From = query;
        }

        /// <summary>
        /// Create a new SelectStatementBuilder whose 'FROM' statement is the <paramref name="builder"/>
        /// </summary>
        /// <param name="builder">The builder that provides the inner query to select from</param>
        public SelectStatementBuilder(SelectStatementBuilder builder) : this()
        {
            From = builder;
        }

        /// <summary>
        /// Treat the <see cref="SelectStatementBuilder"/> as a function to get a query
        /// </summary>
        /// <param name="builder"></param>
        public static implicit operator Func<SK.Query,SK.Query>(SelectStatementBuilder builder) => (o) => builder.ToQuery();

        /// <summary>
        /// The list of column names to return as a result of the query
        /// </summary>
        public List<string> ReturnColumns { get; } = new List<string>();

        /// <summary>
        /// Where to select data from. 
        /// <br/> One of the following: 
        /// <br/> - <see cref="string"/> (table name)
        /// <br/> - <see cref="SK.Query"/>
        /// <br/> - <see cref="SelectStatementBuilder"/>
        /// </summary>
        public object From {
            get => fromValue; 
            set
            {
                if (value is SK.Query | value is SelectStatementBuilder)
                    fromValue = value;
                else if (value is string str)
                {
                    if (string.IsNullOrWhiteSpace(str)) throw new ArgumentException("string value for 'From' property cannot be null! A string value representing the name of a table was expected.");
                    fromValue = str;
                }
                else
                    throw new ArgumentException("Invalid Object Type for 'FROM' value - Expected a SK.Query, a string, or a SelectStatementBuilder");
            }
        }
        private object fromValue;
        
        /// <summary>
        /// The collection of 'Where' statements
        /// </summary>
        public List<IWhereCondition> WhereStatements { get; } = new List<IWhereCondition>();

        /// <summary>
        /// Specify the Alias for this query result to be referenced from other queries
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Compile the parameters from this object into a new Query object
        /// </summary>
        /// <returns>a new <see cref="SK.Query"/> object representing this object</returns>
        public SK.Query ToQuery()
        {
            SK.Query qry = new SK.Query();
            qry.Select(ReturnColumns.Count > 0 ? ReturnColumns.ToArray() : new string[] { "*" });

            if (From is string str) 
                qry.From(str);
            else if (From is SK.Query q) 
                qry.From(q);
            else if (From is SelectStatementBuilder b) 
                qry.From(b);

            Extensions.Where(qry, WhereStatements);

            if (!string.IsNullOrWhiteSpace(Alias)) qry.As(Alias);
            return qry;
        }

        /// <summary>
        /// Compile the query and return it as a string 
        /// <br/> - Not Recommended for use with Database Calls! 
        /// <br/> - Use <see cref="ToDbCommand{T}(Compiler)"/> instead
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.ToQuery().ToString();
        }

        /// <summary>
        /// Compile the query and return the resulting sql statement.
        /// </summary>
        public string ToString(SK.Compilers.Compiler compiler)
        {
            return compiler.Compile(this.ToQuery()).ToString();
        }

        /// <inheritdoc cref="Extensions.ToDbCommand{T}(SK.Query, Compiler)"/>
        public T ToDbCommand<T>(SK.Compilers.Compiler compiler) where T: DbCommand, new() => this.ToQuery().ToDbCommand<T>(compiler);

        /// <summary>
        /// Adds a new <paramref name="condition"/> to the <see cref="WhereStatements"/> collection
        /// </summary>
        /// <param name="condition"></param>
        public void AddCondition(IWhereCondition condition) => this.WhereStatements.Add(condition);

        /// <summary>
        /// Add a new 'Where' statement to the collection
        /// </summary>
        /// <inheritdoc cref="WhereStringValue.WhereStringValue(string, string, StringOperator, bool)"/>
        /// <param name="columnName"><inheritdoc cref="ColumnCondition.Column" path="*"/></param>
        /// <param name="columnValue"><inheritdoc cref="WhereStringValue.Value" path="*"/></param>
        /// <param name="isCaseSensitive"></param>
        /// <param name="isOrCondition"><inheritdoc cref="ColumnCondition.IsOrCondition" path="*"/></param>
        /// <param name="iswhereNot"><inheritdoc cref="ColumnCondition.IsWhereNot" path="*"/></param>
        /// <param name="op"></param>
        public void AddCondition(string columnName, string columnValue, StringOperator op, bool isOrCondition = false, bool iswhereNot = false, bool isCaseSensitive = false)
        {
            this.WhereStatements.Add(
                new WhereStringValue(
                columnName: columnName,
                columnValue: columnValue,
                op: op,
                isCaseSensitive
                )
                {
                    IsOrCondition = isOrCondition,
                    IsWhereNot = iswhereNot
                });
        }

        /// <summary>
        /// Add a new 'Where [<paramref name="columnName"/>] = <paramref name="expectedValue"/>' statement to the collection
        /// </summary>
        /// <inheritdoc cref="WhereBooleanValue.WhereBooleanValue(string, bool?)"/>
        /// <param name="columnName"><inheritdoc cref="ColumnCondition.Column" path="*"/></param>
        /// <param name="isOrCondition"><inheritdoc cref="ColumnCondition.IsOrCondition" path="*"/></param>
        /// <param name="iswhereNot"><inheritdoc cref="ColumnCondition.IsWhereNot" path="*"/></param>
        /// <param name="expectedValue"/>
        public void AddCondition(string columnName, bool? expectedValue, bool isOrCondition = false, bool iswhereNot = false)
        {
            this.WhereStatements.Add(new WhereBooleanValue(columnName, expectedValue) {  IsOrCondition = isOrCondition, IsWhereNot = iswhereNot});
        }

        /// <summary>
        /// Add a new 'Where' statement to the collection
        /// </summary>
        /// <inheritdoc cref="WhereNumericValue{T}.WhereNumericValue(string, T, NumericOperators)"/>
        /// <param name="columnName"><inheritdoc cref="ColumnCondition.Column" path="*"/></param>
        /// <param name="isOrCondition"><inheritdoc cref="ColumnCondition.IsOrCondition" path="*"/></param>
        /// <param name="iswhereNot"><inheritdoc cref="ColumnCondition.IsWhereNot" path="*"/></param>
        /// <param name="op"/>
        /// <param name="columnValue"/>
        public void AddCondition(string columnName, int columnValue, NumericOperators op = default, bool isOrCondition = false, bool iswhereNot = false)
        {
            this.WhereStatements.Add(
                new WhereNumericValue<int>(columnName, columnValue, op) { IsOrCondition = isOrCondition, IsWhereNot = iswhereNot }
                );
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
