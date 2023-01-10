using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SK = SqlKata;

namespace RFBCodeWorks.SqlKata.Extensions
{
    /// <summary>
    /// Interface for handling <see cref="WhereNumericValue{T}"/> objects without specifying the subtype
    /// </summary>
    public interface IWhereNumericValue : IColumnCondition, IWhereCondition
    {
        /// <inheritdoc cref="WhereNumericValue{T}.Operator"/>
        NumericOperators Operator { get; }

        /// <inheritdoc cref="WhereNumericValue{T}.Operator"/>
        object Value { get; }

    }

    /// <summary>
    /// A WHERE condition that uses the various <see cref="NumericOperators"/> to determine how to compare the value to the database.
    /// </summary>
    /// <typeparam name="T">A numeric Type, such as <see cref="int"/>. <see cref="DateTime"/> should also work fine here.</typeparam>
    public class WhereNumericValue<T> : ColumnCondition, IWhereNumericValue
        where T: struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Create a new Numeric Condition
        /// </summary>
        /// <param name="op"> <inheritdoc cref="Operator" path="*" /> </param>
        /// <param name="columnValue"><inheritdoc cref="Value" path="*"/></param>
        /// <inheritdoc cref="ColumnCondition.ColumnCondition(string)"/>
        /// <param name="columnName"/>
        public WhereNumericValue(string columnName, T columnValue, NumericOperators op = default) : base(columnName)
        {
            Operator = op ?? NumericOperators.EqualTo;
            Value = columnValue;
        }

        /// <summary>
        /// The operator to use
        /// <br/> If not specified, uses <see cref="NumericOperators.EqualTo"/>
        /// </summary>
        public NumericOperators Operator { 
            get => @operator ?? NumericOperators.EqualTo; 
            set => @operator = value; 
        }
        private NumericOperators @operator;

        /// <summary>
        /// The value to search for
        /// </summary>
        public T Value { get; set; }
        object IWhereNumericValue.Value => Value;

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
