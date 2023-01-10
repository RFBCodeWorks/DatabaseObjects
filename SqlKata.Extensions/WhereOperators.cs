using SqlKata;
using System;
using System.Collections.Generic;
using System.Text;
using SK = SqlKata;

namespace RFBCodeWorks.SqlKata.Extensions
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
        /// <param name="enumValue"></param>
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
        /// <param name="isOrCondition">set <see langword="true"/> to generate an 'OR' condition, or <see langword="false"/> is this should generate an 'AND' condition. </param>
        /// <param name="whereNot">
        /// Set <see langword="true"/> to use 'WhereNot' conditions. 
        /// <br/> Default is <see langword="false"/>, which uses 'Where' conditions.
        /// </param>
        /// <returns><paramref name="query"/></returns>
        public abstract Q ApplyCondition<Q>(Q query, string column, object value, bool isOrCondition = false, bool whereNot = false) where Q : SK.BaseQuery<Q>;

        /// <inheritdoc/>
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
    public sealed class BoolOperators : AbstractSqlOperator, IComparable<BoolOperators>
    {
        private BoolOperators(string op, int val) : base(op, val) { }

        /// <summary>
        /// "= <see langword="true"/>"
        /// </summary>
        public static BoolOperators IsTrue { get; } = new BoolOperators("= true", 0);

        /// <summary>
        /// "= <see langword="false"/>"
        /// </summary>
        public static BoolOperators IsFalse { get; } = new BoolOperators("= false", 1);

        /// <summary>
        /// "= <see langword="null"/>" -- Equivalent to checking for <see cref="DBNull"/>
        /// </summary>
        public static BoolOperators IsNull { get; } = new BoolOperators("= null", -1);

        /// <summary>
        /// The boolean value of this <see cref="BoolOperators"/>
        /// </summary>
        public bool? Value
        {
            get
            {
                if (this.EnumValue == 0) return false;
                if (this.EnumValue == 1) return true;
                if (this.EnumValue == -1) return null;
                throw new NotImplementedException("Undefined");
            }
        }

        /// <param name="value">this is ignored since the operator is based on the selected <see cref="BoolOperators"/></param>
        /// <inheritdoc/>
        /// <param name="query"/>
        /// <param name="column"/>
        /// <param name="isOrCondition"/>
        /// <param name="whereNot"/>
        public override Q ApplyCondition<Q>(Q query, string column, object value, bool isOrCondition = false, bool whereNot = false)
        {
            switch(true)
            {
                case true when this == IsNull:
                    if (isOrCondition)
                    {
                        return whereNot ? query.OrWhereNotNull(column) : query.OrWhereNull(column);
                    }
                    else
                    {
                        return whereNot ? query.WhereNotNull(column) : query.WhereNull(column);
                    }

                case true when this == IsTrue:
                    if (isOrCondition)
                    {
                        return whereNot ? query.OrWhereFalse(column): query.OrWhereTrue(column);
                    }
                    else
                    {
                        return whereNot ? query.WhereFalse(column) : query.WhereTrue(column);
                    }

                case true when this == IsFalse:
                    if (isOrCondition)
                    {
                        return whereNot ? query.OrWhereTrue(column) : query.OrWhereFalse(column);
                    }
                    else
                    {
                        return whereNot ? query.WhereTrue(column) : query.WhereFalse(column);
                    }
                default:
                    throw new NotImplementedException($"Operator '{Operator}' not set up yet!");
            }
        }

        int IComparable<BoolOperators>.CompareTo(BoolOperators other)
        {
            return this.CompareTo(other);
        }
    }

    /// <summary>
    /// Numeric Operators for generating WHERE clauses
    /// </summary>
    public sealed class NumericOperators : AbstractSqlOperator ,IComparable<NumericOperators>
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
        
        ///// <summary> &lt;=> </summary>
        //public static NumericOperators Between { get; } = new NumericOperators("<=>"); // Consumers should be using the WhereBetween class

        ///<inheritdoc/>
        public override Q ApplyCondition<Q>(Q query, string column, object value, bool isOrCondition = false, bool whereNot = false)
        {
            //For '!=' operator -> Invert the 'WhereNot' condition, when apply using 'EqualTo' operator for better compatibility with SqlKata
            if (this == NumericOperators.NotEqualTo)
            {
                //This will transform != into 'WhereNot = '
                return NumericOperators.EqualTo.ApplyCondition(query, column, value, isOrCondition, !whereNot);
            }

            //For all other operators, evaluate and apply
            if (isOrCondition)
            {
                if (whereNot)
                    query.OrWhereNot(column, Operator, value);
                else
                    query.OrWhere(column, Operator, value);
            }
            else
            {
                if (whereNot)
                    query.WhereNot(column, Operator, value);
                else
                    query.Where(column, Operator, value);
            }
            return query;
        }

        int IComparable<NumericOperators>.CompareTo(NumericOperators other)
        {
            return this.CompareTo(other);
        }
    }

    /// <summary>
    /// String Operators for generating WHERE clauses
    /// </summary>
    public class StringOperator : AbstractSqlOperator, IComparable<StringOperator>
    {
        /// <summary>
        /// Define a new StringOperator
        /// </summary>
        /// <param name="op">The string that dictates the comparison operator</param>
        /// <param name="val">The value for the <see cref="IComparable"/> interface </param>
        protected StringOperator(string op, int val) : base(op, val) { }

        /// <summary> Column value should is similar to the comparison string. <br/> ex: *search*term* </summary>
        public static StringOperator Like { get; } = new StringOperator("like",0);
        /// <summary> Column value should is not similar to the comparison string. <br/> ex: *Exclude*Match* </summary>
        public static StringOperator NotLike { get; } = new StringOperator("not like", 1);
        /// <summary> Column value starts with the comparison string. </summary>
        public static StringOperator StartsWith { get; } = new StringOperator("starts",2);
        /// <summary> Column value ends with the comparison string. </summary>
        public static StringOperator EndsWith { get; } = new StringOperator("ends",3);
        /// <summary> Column value contains the comparison string. </summary>
        public static StringOperator Contains { get; } = new StringOperator("contains",4);
        /// <summary> = </summary>
        public static StringOperator EqualTo { get; } = new StringOperator("=",5);
        /// <summary> != </summary>
        public static StringOperator NotEqualTo { get; } = new StringOperator("!=",6);
        /// <summary> Column value should match the regex string used to evaluate </summary>
        public static StringOperator MatchRegex { get; } = new StringOperator("regexpr",7);
        /// <summary> Column value does not match the regex string used to evaluate it </summary>
        public static StringOperator DoesNotMatchRegex { get; } = new StringOperator("not regexpr",8);


        ///<inheritdoc/>
        public override Q ApplyCondition<Q>(Q query, string column, object value, bool isOrCondition = false, bool whereNot = false)
            => ApplyCondition(query, column, value, isOrCondition, whereNot, false);

        ///<inheritdoc cref="ApplyCondition{Q}(Q, string, object, bool, bool)"/>
        public Q ApplyCondition<Q>(Q query, string column, object value, bool isOrCondition = false, bool whereNot = false, bool IsCaseSensitive = false) where Q : SK.BaseQuery<Q>
        {
            switch (true)
            {
                case true when this == NotLike:
                case true when this == Like:
                    // Invert the whereNot variable if using NotLike
                    whereNot = this == Like ? whereNot : !whereNot; 
                    if (isOrCondition)
                        return whereNot ? query.OrWhereNotLike(column, value, IsCaseSensitive) : query.OrWhereLike(column,value, IsCaseSensitive);
                    else
                        return whereNot ? query.WhereNotLike(column, value, IsCaseSensitive) : query.WhereLike(column, value, IsCaseSensitive);

                case true when this == Contains:
                    if (isOrCondition)
                        return whereNot ? query.OrWhereNotContains(column, value, IsCaseSensitive) : query.OrWhereContains(column, value, IsCaseSensitive); 
                    else
                        return whereNot ? query.WhereNotContains(column, value, IsCaseSensitive) : query.WhereContains(column, value, IsCaseSensitive);

                case true when this == StartsWith:
                    if (isOrCondition)
                        return whereNot ? query.OrWhereNotStarts(column, value, IsCaseSensitive) : query.OrWhereStarts(column, value, IsCaseSensitive);
                    else
                        return whereNot ? query.WhereNotStarts(column, value, IsCaseSensitive) : query.WhereStarts(column, value, IsCaseSensitive);

                case true when this == EndsWith:
                    if (isOrCondition)
                        return whereNot ? query.OrWhereNotEnds(column, value, IsCaseSensitive) : query.OrWhereEnds(column, value, IsCaseSensitive);
                    else
                        return whereNot ? query.WhereNotEnds(column, value, IsCaseSensitive) : query.WhereEnds(column, value, IsCaseSensitive);

                case true when this == EqualTo:
                case true when this == NotEqualTo:
                    // Invert the whereNot variable if using NotEqualTo
                    whereNot = this == EqualTo ? whereNot : !whereNot;
                    if (isOrCondition)
                    {
                        return whereNot ? query.OrWhereNot(column, value) : query.OrWhere(column, value);
                    }
                    else
                    {
                        return whereNot ? query.WhereNot(column, value) : query.Where(column, value);
                    }

                case true when this == MatchRegex:
                case true when this == DoesNotMatchRegex:
                    // Invert the whereNot variable if using DoesNotMatchRegex
                    whereNot = this == MatchRegex ? whereNot : !whereNot;
                    if (isOrCondition)
                        return whereNot ? query.OrWhereNot(column, MatchRegex, value) : query.OrWhere(column, MatchRegex, value);
                    else
                        return whereNot ? query.WhereNot(column, MatchRegex, value) : query.Where(column, MatchRegex, value);

                default:
                    throw new NotImplementedException($"Operator '{Operator}' not set up yet!");
            }
        }

        int IComparable<StringOperator>.CompareTo(StringOperator other)
        {
            return this.CompareTo(other);
        }
    }
}
