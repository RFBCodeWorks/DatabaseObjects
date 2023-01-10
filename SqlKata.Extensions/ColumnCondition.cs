using SqlKata;
using System;
using System.Collections.Generic;
using System.Text;
using SK = SqlKata;

namespace RFBCodeWorks.SqlKata.Extensions
{

    /// <summary>
    /// Interface for the <see cref="WhereColumnValue"/> objects
    /// </summary>
    public interface IColumnCondition : IWhereCondition
    {
        /// <inheritdoc cref="ColumnCondition.Column"/>
        string Column { get; set; }

        /// <inheritdoc cref="ColumnCondition.IsOrCondition"/>
        bool IsOrCondition { get; set; }

        /// <inheritdoc cref="ColumnCondition.IsWhereNot"/>
        bool IsWhereNot { get; set; }
    }

    /// <summary>
    /// An abstract class meant to be used as a base class for condition statements that deal with evaluating a single column's value.
    /// <br/> Such as: " [Column] like 'SearchTerm' "
    /// </summary>
    public abstract class ColumnCondition : IColumnCondition, IWhereCondition
    {
        /// <summary>
        /// Instaniate the column condition
        /// </summary>
        /// <param name="column"><inheritdoc cref="Column" path="*"/></param>
        /// <exception cref="ArgumentException"/>
        protected ColumnCondition(string column)
        {
            Column = column;
        }

        /// <summary>
        /// The Column Name
        /// </summary>
        /// <remarks>
        /// Example inputs: 
        /// <br/> -  ColumnName     ( ex: ID )
        /// <br/> -  [ColumnName]   ( ex: [ID] )
        /// <br/> -  {Source}.[ColumnName] ( ex: Q.[ID] )
        /// </remarks>
        public string Column
        {
            get => column;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Column Name cannot be null or empty!");
                column = value;
            }
        }
        private string column;

        /// <summary>
        /// Specify if this statement is applied using the 'AND' or the 'OR' operator.
        /// <br/> - <see langword="false"/> (default) -- Create an 'AND' condition
        /// <br/> - <see langword="true"/> -- Create an 'OR' condition
        /// </summary>
        public bool IsOrCondition { get; set; }

        /// <summary>
        /// Specify if this condition should be inverted.
        /// <br/> - <see langword="false"/> (default) -- use a 'Where' statement. Ex: <see cref="BaseQuery{Q}.Where(string, object)"/>
        /// <br/> - <see langword="true"/> -- use a 'WhereNot' statement. Ex: <see cref="BaseQuery{Q}.WhereNot(string, object)"/>
        /// </summary>
        public bool IsWhereNot { get; set; }

        /// <inheritdoc/>
        public abstract Query ApplyToQuery(Query query);
    }
}
