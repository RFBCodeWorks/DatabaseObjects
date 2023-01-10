using SqlKata;
using System;
using System.Collections.Generic;
using System.Text;
using SK = SqlKata;

namespace RFBCodeWorks.SqlKata.Extensions
{
    /// <summary>
    /// Interface for handling the <see cref="WhereValueBetween{T}"/> subtypes
    /// </summary>
    public interface IWhereValueBetween : IColumnCondition, IWhereCondition
    {
        /// <inheritdoc cref="WhereValueBetween{T}.Minimum"/>
        object Minimum { get; }

        /// <inheritdoc cref="WhereValueBetween{T}.Maximum"/>
        object Maximum { get; }
    }

    /// <summary>
    /// Helper class that compiles a down to <see cref="BaseQuery{Q}.WhereBetween{T}(string, T, T)"/> statements
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class WhereValueBetween<T> : ColumnCondition, IWhereValueBetween
        where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
    {
        /// <summary>
        /// Create a new WhereBetween condition - Evalautes if the database value is between the <paramref name="minimum"/> anmd <paramref name="maximum"/>
        /// </summary>
        /// <param name="minimum"><inheritdoc cref="Minimum" path="*"/></param>
        /// <param name="maximum"><inheritdoc cref="Maximum" path="*"/></param>
        /// <inheritdoc cref="ColumnCondition.ColumnCondition(string)"/>
        /// <param name="column"/>
        public WhereValueBetween(string column, T minimum, T maximum) : base(column)
        {
            Minimum = minimum;
            Maximum = maximum;
        }

        /// <summary>
        /// The minimum acceptable value
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="BaseQuery{Q}.WhereBetween{T}(string, T, T)" path="/param[@name='lower']" />
        /// </remarks>
        public T Minimum { get; set; }
        object IWhereValueBetween.Minimum => Minimum;

        /// <summary>
        /// The maximum acceptable value
        /// </summary>
        /// <remarks>
        /// <inheritdoc cref="BaseQuery{Q}.WhereBetween{T}(string, T, T)" path="/param[@name='higher']" />
        /// </remarks>
        public T Maximum { get; set; }
        object IWhereValueBetween.Maximum => Maximum;

        /// <inheritdoc/>
        public override Query ApplyToQuery(Query query)
        {
            if (IsOrCondition)
            {
                if (IsWhereNot)
                    return query.WhereNotBetween(Column, Minimum, Maximum);
                else
                    return query.WhereBetween(Column, Minimum, Maximum);
            }
            else
            {
                if (IsWhereNot)
                    return query.OrWhereNotBetween(Column, Minimum, Maximum);
                else
                    return query.OrWhereBetween(Column, Minimum, Maximum);
            }
        }
    }
}
