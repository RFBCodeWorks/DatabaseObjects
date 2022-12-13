using System;
using System.Collections.Generic;
using System.Text;

namespace RFBCodeWorks.DataBaseObjects.Helpers
{
    

    /// <summary>
    /// Force conditions to AND &amp; OR values
    /// </summary>
    public enum AndOr
    {
        /// <summary> AND operator </summary>
        AND,
        /// <summary> OR operator </summary>
        OR
    }


    public static partial class Extensions
    {
        /// <inheritdoc cref="WhereStatementHelper.ApplyStatement(SqlKata.Query, bool)"/>
        public static SqlKata.Query ApplyWhereStatement(this SqlKata.Query query, WhereStatementHelper statement, bool isFirstWhereStatement = false) => statement.ApplyStatement(query, isFirstWhereStatement);
    }

    /// <summary>
    /// Class to assist with building WHERE statements dynamically
    /// </summary>
    public class WhereStatementHelper
    {
        /// <summary>
        /// Create a new <see cref="WhereStatementHelper"/>
        /// </summary>
        public WhereStatementHelper(string columnName, AbstractSqlOperator op, AndOr andOr = AndOr.AND, object value = null)
        {
            Operator = op;
            AndOr = andOr;
            Column = columnName;
            Value = value;
        }

        /// <summary>
        /// Create a new <see cref="WhereStatementHelper"/> that evaluates a string
        /// </summary>
        public WhereStatementHelper(string columnName, string columnValue, StringOperators op = default, AndOr andOr = AndOr.AND, bool isCaseSensitive = false)
        {
            Operator = op ?? StringOperators.Like;
            AndOr = andOr;
            Column = columnName;
            Value = columnValue;
            IsCaseSensitive = isCaseSensitive;
        }

        /// <summary>
        /// Create a new <see cref="WhereStatementHelper"/> that evaluates an integer
        /// </summary>
        public WhereStatementHelper(string columnName, int columnValue, NumericOperators op = default, AndOr andOr = AndOr.AND)
        {
            Operator = op ?? NumericOperators.EqualTo;
            AndOr = andOr;
            Column = columnName;
            Value = columnValue;
        }

        /// <summary>
        /// Create a new <see cref="WhereStatementHelper"/> that evaluates a bool
        /// </summary>
        public WhereStatementHelper(string columnName, bool expectedBoolValue, AndOr andOr = AndOr.AND)
        {
            Operator = expectedBoolValue ? BoolOperators.IsTrue : BoolOperators.IsFalse;
            AndOr = andOr;
            Column = columnName;
        }

        /// <summary>
        /// The operator to use
        /// </summary>
        public AbstractSqlOperator Operator { get; set; }

        /// <summary>
        /// Specify if this statement is applied using the 'AND' or the 'OR' operator
        /// </summary>
        public AndOr AndOr { get; set; }

        /// <summary>
        /// If the value is a string condition, specify case sensitivity.
        /// </summary>
        public bool IsCaseSensitive { get; set; }

        /// <summary>
        /// The Column Identifier
        /// </summary>
        /// <remarks>
        /// Acceptable inputs: 
        /// <br/> -  ColumnName     ( ex: ID )
        /// <br/> -  [ColumnName]   ( ex: [ID] )
        /// <br/> -  {Source}.[ColumnName] ( ex: Q.[ID] )
        /// </remarks>
        public string Column { get; set; }

        /// <summary>
        /// The value to search for
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Check if the <see cref="Value"/> is a <see cref="int"/>
        /// </summary>
        public bool IsValueInt => Value is int;

        /// <summary>
        /// Check if the <see cref="Value"/> is a <see cref="string"/> object
        /// </summary>
        public bool IsValueString => Value is string;

        /// <summary>
        /// Check if the <see cref="Value"/> is a <see cref="bool"/> object
        /// </summary>
        public bool IsValueBool => Value is bool;

        /// <summary>
        /// Check if the <see cref="Value"/> is a <see cref="double"/>
        /// </summary>
        public bool IsValueDouble => Value is double;

        /// <summary>
        /// Apply this condition to the specified <paramref name="query"/>
        /// </summary>
        /// <param name="query">The query to apply the statement to</param>
        /// <param name="isFirstWhereStatement">If TRUE, use 'WHERE' instead of 'OrWhere'. If False, only uses 'OrWhere' when <see cref="AndOr"/> is set to <see cref="AndOr.OR"/></param>
        /// <returns><paramref name="query"/></returns>
        public SqlKata.Query ApplyStatement(SqlKata.Query query, bool isFirstWhereStatement = false)
        {
            bool AndStatement = isFirstWhereStatement || AndOr == AndOr.AND;
            if (Operator is StringOperators stringOp)
            {
                return stringOp.ApplyCondition(query, Column, Value, isFirstWhereStatement || AndOr == AndOr.AND, IsCaseSensitive);
            }
            else
            {
                return Operator.ApplyCondition(query, Column, Value, isFirstWhereStatement || AndOr == AndOr.AND);
            }
        }
    }
}
