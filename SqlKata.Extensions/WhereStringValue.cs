using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SK = SqlKata;

namespace RFBCodeWorks.SqlKata.Extensions
{
    /// <summary>
    /// Compare a Column Value to a string
    /// </summary>
    public class WhereStringValue : ColumnCondition, IColumnCondition
    {
        /// <summary>
        /// Create a new <see cref="WhereStringValue"/>
        /// </summary>
        /// <param name="op">
        /// <inheritdoc cref="Operator" path="*"/>
        /// <br/>If <paramref name="op"/> is not specified, uses <see cref="StringOperator.Like"/>
        /// </param>
        /// <param name="isCaseSensitive"><inheritdoc cref="IsCaseSensitive" path="*"/></param>
        /// <param name="columnName"><inheritdoc cref="ColumnCondition.Column" path="*"/></param>
        /// <param name="columnValue"><inheritdoc cref="Value" path="*"/></param>
        public WhereStringValue(string columnName, string columnValue, StringOperator op = default, bool isCaseSensitive = false) : base(columnName)
        {
            Operator = op;
            Value = columnValue;
            IsCaseSensitive = isCaseSensitive;
        }

        /// <summary>
        /// The String Operator.
        /// <br/> If value is set to null, this will default to return <see cref="StringOperator.Like"/>
        /// </summary>
        public StringOperator Operator { 
            get => @operator ?? StringOperator.Like;
            set => @operator = value;
        }
        private StringOperator @operator;

        /// <summary>
        /// The value to compare against
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// If the <see cref="Operator"/> is a <see cref="StringOperator"/>, specify case sensitivity.
        /// </summary>
        public bool IsCaseSensitive { get; set; }

        /// <inheritdoc/>
        public override SK.Query ApplyToQuery(SK.Query query)
        {
            return Operator.ApplyCondition(
                query ?? throw new ArgumentNullException(nameof(query)),
                base.Column,
                Value,
                base.IsOrCondition,
                base.IsWhereNot,
                IsCaseSensitive);
        }
    }
}
