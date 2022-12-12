using SqlKata;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataBaseObjects.Helpers
{
    /// <summary>
    /// abstract Class that frames the required functionality for SQL Operator class enum types
    /// </summary>
    public abstract class AbstractSqlOperator : IComparable
    {
        
        /// <summary>
        /// Instantiate the class value
        /// </summary>
        /// <param name="op">string representation of the SQL operator to pass into SQL factory</param>
        protected AbstractSqlOperator(string op, int enumValue)
        {
            Operator = op;
            EnumValue = enumValue;
        }

        /// <summary>
        /// String Representation of the operator
        /// </summary>
        public string Operator { get; }

        /// <summary>
        /// Underlying value of the object - for use by <see cref="IComparable"/>
        /// </summary>
        public int EnumValue { get; }

        /// <inheritdoc/>
        public static implicit operator string(AbstractSqlOperator op) => op.ToString();

        /// <inheritdoc/>
        public override string ToString() => this.Operator;

        /// <summary>
        /// Apply the operation to the <paramref name="query"/> with the supplied parameters
        /// </summary>
        /// <typeparam name="Q"></typeparam>
        /// <param name="query">Query to apply a where clause to</param>
        /// <param name="column">column Name</param>
        /// <param name="value">Value to compare column value with using the operator</param>
        /// <param name="IsAndCondition">set TRUE to generate an 'AND' condition, or FALSE is this should generate an 'OR' condition. </param>
        /// <returns><paramref name="query"/></returns>
        public abstract Q ApplyCondition<Q>(Q query, string column, object value, bool IsAndCondition = true) where Q : SqlKata.BaseQuery<Q>;

        public int CompareTo(object obj)
        {
            if (this.GetType() == obj.GetType())
            {
                return EnumValue.CompareTo(((AbstractSqlOperator)obj).EnumValue);
            }
            else
            {
                throw new ArgumentException("Unable to compare objects of different types");
            }
        }

        //public static bool operator ==(AbstractSqlOperator x, AbstractSqlOperator y)
        //{
        //    return x.GetType() == y.GetType() && x.EnumValue == y.EnumValue;
        //}

        //public static bool operator !=(AbstractSqlOperator x, AbstractSqlOperator y)
        //{
        //    return x.GetType() != y.GetType() || x.EnumValue != y.EnumValue;
        //}

    }
    
    /// <summary>
    /// Operators for generating statements that evaluate booleans
    /// </summary>
    public class BoolOperators : AbstractSqlOperator
    {
        private BoolOperators(string op, int val) : base(op, val) { }

        public static BoolOperators IsTrue { get; } = new BoolOperators("= true", 0);
        public static BoolOperators IsFalse { get; } = new BoolOperators("= false", 1);

        /// <param name="value">this is ignored since the operator is based on the selected <see cref="BoolOperators"/></param>
        /// <inheritdoc/>
        /// <param name="query"/>
        /// <param name="column"/>
        /// <param name="IsAndCondition"/>
        public override Q ApplyCondition<Q>(Q query, string column, object value, bool IsAndCondition = true)
        {
            switch(true)
            {
                case true when this == IsTrue:
                    if (IsAndCondition)
                        query.WhereTrue(column);
                    else
                        query.OrWhereTrue(column);
                    break;

                case true when this == IsFalse:
                    if (IsAndCondition)
                        query.WhereFalse(column);
                    else
                        query.OrWhereFalse(column);
                    break;

                default:
                    throw new NotImplementedException($"Operator '{Operator}' not set up yet!");
            }
            return query;
        }
    }

    /// <summary>
    /// Numeric Operators for generating WHERE clauses
    /// </summary>
    public class NumericOperators : AbstractSqlOperator
    {
        private NumericOperators(string op, int val) : base(op, val) { }

        /// <summary> = </summary>
        public static NumericOperators EqualTo { get; } = new NumericOperators("=",0);
        /// <summary> > </summary>
        public static NumericOperators GreaterThan { get; } = new NumericOperators(">",1);
        /// <summary> &gt;= </summary>
        public static NumericOperators GreaterThanEqualTo { get; } = new NumericOperators(">=",2);
        /// <summary> &lt; </summary>
        public static NumericOperators LessThan { get; } = new NumericOperators("<",3);
        /// <summary> &lt;= </summary>
        public static NumericOperators LessThanEqualTo { get; } = new NumericOperators("<=",4);
        /// <summary> != </summary>
        public static NumericOperators NotEqualTo { get; } = new NumericOperators("!=",5);
        /// <summary> &lt;=> </summary>
        //public static NumericOperators Between { get; } = new NumericOperators("<=>"); //Not currently supported

        ///<inheritdoc/>
        public override Q ApplyCondition<Q>(Q query, string column, object value, bool IsAndCondition = true)
        {
            switch (true)
            {
                case true when this == EqualTo:
                case true when this == GreaterThan:
                case true when this == GreaterThanEqualTo:
                case true when this == LessThan:
                case true when this == LessThanEqualTo:
                    if (IsAndCondition)
                        query.Where(column, Operator, value);
                    else
                        query.OrWhere(column, Operator, value);
                    break;

                case true when this == NotEqualTo:
                    if (IsAndCondition)
                        query.WhereNot(column, NumericOperators.EqualTo, value);
                    else
                        query.OrWhereNot(column, NumericOperators.EqualTo, value);
                    break;

                default:
                    throw new NotImplementedException($"Operator '{Operator}' not set up yet!");
            }
            return query;
        }
    }

    /// <summary>
    /// String Operators for generating WHERE clauses
    /// </summary>
    public class StringOperators : AbstractSqlOperator
    {
        private StringOperators(string op, int val) : base(op, val) { }

        /// <summary> Column value should is similar to the comparison string. <br/> ex: *search*term* </summary>
        public static StringOperators Like { get; } = new StringOperators("like",0);
        /// <summary> Column value should is not similar to the comparison string. <br/> ex: *Exclude*Match* </summary>
        public static StringOperators NotLike { get; } = new StringOperators("not like",1);
        /// <summary> Column value starts with the comparison string. </summary>
        public static StringOperators StartsWith { get; } = new StringOperators("starts",2);
        /// <summary> Column value ends with the comparison string. </summary>
        public static StringOperators EndsWith { get; } = new StringOperators("ends",3);
        /// <summary> Column value contains the comparison string. </summary>
        public static StringOperators Contains { get; } = new StringOperators("contains",4);
        /// <summary> = </summary>
        public static StringOperators EqualTo { get; } = new StringOperators("=",5);
        /// <summary> != </summary>
        public static StringOperators NotEqualTo { get; } = new StringOperators("!=",6);
        /// <summary> Column value should match the regex string used to evaluate </summary>
        public static StringOperators MatchRegex { get; } = new StringOperators("regexpr",7);
        /// <summary> Column value does not match the regex string used to evaluate it </summary>
        public static StringOperators DoesNotMatchRegex { get; } = new StringOperators("not regexpr",8);
        /// <summary> </summary>
        //public static StringOperators DoesNotContain { get; } = new StringOperators("not contains",9);


        ///<inheritdoc/>
        public override Q ApplyCondition<Q>(Q query, string column, object value, bool IsAndCondition = true)
            => ApplyCondition(query, column, value, IsAndCondition, false);

        ///<inheritdoc cref="ApplyCondition{Q}(Q, string, object, bool)"/>
        public Q ApplyCondition<Q>(Q query, string column, object value, bool IsAndCondition = true, bool IsCaseSensitive = false) where Q : SqlKata.BaseQuery<Q>
        {
            switch (true)
            {
                case true when this == Like:
                    if (IsAndCondition)
                        query.WhereLike(column,value, IsCaseSensitive);
                    else
                        query.OrWhereLike(column, value, IsCaseSensitive);
                    break;

                case true when this == NotLike:
                    if (IsAndCondition)
                        query.WhereNotLike(column, value, IsCaseSensitive);
                    else
                        query.OrWhereNotLike(column, value, IsCaseSensitive);
                    break;

                case true when this == Contains:
                    if (IsAndCondition)
                        query.WhereContains(column, value, IsCaseSensitive);
                    else
                        query.OrWhereContains(column, value, IsCaseSensitive);
                    break;

                case true when this == StartsWith:
                    if (IsAndCondition)
                        query.WhereStarts(column, value, IsCaseSensitive);
                    else
                        query.OrWhereStarts(column, value, IsCaseSensitive);
                    break;

                case true when this == EndsWith:
                    if (IsAndCondition)
                        query.WhereEnds(column, value, IsCaseSensitive);
                    else
                        query.OrWhereEnds(column, value, IsCaseSensitive);
                    break;

                case true when this == EqualTo:
                    if (IsAndCondition)
                        query.Where(column, value);
                    else
                        query.OrWhere(column, value);
                    break;

                case true when this == NotEqualTo:
                    if (IsAndCondition)
                        query.WhereNot(column, value);
                    else
                        query.OrWhereNot(column, value);
                    break;

                case true when this == MatchRegex:
                case true when this == DoesNotMatchRegex:
                    if (IsAndCondition)
                        query.Where(column, Operator, value);
                    else
                        query.OrWhere(column, Operator, value);
                    break;

                default:
                    throw new NotImplementedException($"Operator '{Operator}' not set up yet!");
            }

            return query;
        }

    }
}
