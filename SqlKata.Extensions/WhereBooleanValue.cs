using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SK = SqlKata;

namespace RFBCodeWorks.SqlKata.Extensions
{
    /// <summary>
    /// Where Condition that checks if the column's value is TRUE or FALSE
    /// </summary>
    public class WhereBooleanValue : ColumnCondition, IColumnCondition, IWhereCondition
    {
        /// <summary>
        /// Create a new <see cref="WhereBooleanValue"/>
        /// </summary>
        /// <param name="expectedValue">
        /// Set this to the expected boolean value within the database. 
        /// <br/>Shown below are example results based on the input value for this parameter:
        /// <br/>- If <see langword="true"/> : WHERE [<paramref name="column"/>] = true
        /// <br/>- If <see langword="false"/> : WHERE [<paramref name="column"/>] = false
        /// </param>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <inheritdoc cref="ColumnCondition.ColumnCondition(string)"/>
        /// <param name="column"/>
        public WhereBooleanValue(string column, bool? expectedValue) : base(column)
        {
            Operator = expectedValue.HasValue ? (expectedValue.Value ? BoolOperators.IsTrue : BoolOperators.IsFalse) : BoolOperators.IsNull;
            Value = expectedValue;
        }

        /// <summary>
        /// Create a new <see cref="WhereBooleanValue"/>
        /// </summary>
        /// <exception cref="ArgumentException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <inheritdoc cref="ColumnCondition.ColumnCondition(string)"/>
        public WhereBooleanValue(string column, BoolOperators op) : base(column)
        {
            Operator = op;
            Value = op.Value;
        }

        /// <summary>
        /// The operator to use
        /// </summary>
        public BoolOperators Operator {
            get => @operator; 
            set => @operator = value ?? throw new ArgumentNullException("WhereBooleanValue.Operator cannot be null - Must specify one of the static options from that type."); 
        }
        private BoolOperators @operator;

        /// <summary>
        /// The value to search for
        /// </summary>
        public bool? Value { get; set; }

        /// <inheritdoc/>
        public override SK.Query ApplyToQuery(SK.Query query)
        {
            return Operator.ApplyCondition(
                query ?? throw new ArgumentNullException(nameof(query)),
                base.Column, 
                Value, 
                base.IsOrCondition,
                base.IsWhereNot);
        }
    }
}
