using System;
using System.Collections.Generic;
using System.Text;
using SK = SqlKata;

namespace RFBCodeWorks.SqlKata.Extensions
{

    /// <summary>
    /// The most basic <see cref="ColumnCondition"/>, uses <see cref="SK.BaseQuery{Q}.Where(string, string, object)"/> to generate the conditions
    /// </summary>
    public class WhereColumnValue : ColumnCondition, IWhereCondition, IColumnCondition
    {

        /// <inheritdoc cref="WhereColumnValue.WhereColumnValue(string, string, object)"/>
        public WhereColumnValue(string column) : base(column) { }

        /// <inheritdoc cref="WhereColumnValue.WhereColumnValue(string, string, object)"/>
        public WhereColumnValue(string column, object value ) : this(column)
        {
            Value = value;
        }

        /// <summary>
        /// Create a new Column Condition
        /// </summary>
        /// <param name="column"><inheritdoc cref="ColumnCondition.Column" path="*"/></param>
        /// <param name="op">The operator to use when comparing the <paramref name="value"/></param>
        /// <param name="value"><inheritdoc cref="Value" path="*"/></param>
        public WhereColumnValue(string column, string op, object value) : this(column, value)
        {
            Operator = op;
        }

        /// <summary>
        /// The operator to use when generating the condition
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// The value to compare against
        /// </summary>
        public object Value { get; set; }

        /// <inheritdoc/>
        public override SK.Query ApplyToQuery(SK.Query query)
        {
            if (string.IsNullOrWhiteSpace(Operator))
            {
                if (IsOrCondition)
                    return IsWhereNot ? query.OrWhereNot(Column, Value) : query.OrWhere(Column, Value);
                else
                    return IsWhereNot ? query.WhereNot(Column, Value) : query.Where(Column, Value);
            }
            else
            {
                if (IsOrCondition)
                    return IsWhereNot ? query.OrWhereNot(Column, Operator, Value) : query.OrWhere(Column, Operator, Value);
                else
                    return IsWhereNot ? query.WhereNot(Column, Operator, Value) : query.Where(Column, Operator, Value);
            }
            
        }
    }
}
