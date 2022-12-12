using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DataBaseObjects;
using SqlKata;

namespace DataBaseObjects
{
    public static partial class Extensions
    {

        /// <summary>
        /// Generate a new KeyValuePair array that consists of a single pair
        /// </summary>
        public static KeyValuePair<T,O>[] ConvertToKeyValuePairArray<T,O>(T key, O value)
        {
            return new KeyValuePair<T, O>[] { new KeyValuePair<T, O>(key, value) };
        }

        /// <summary>
        /// Generate a new KeyValuePair array that consists of a single pair
        /// </summary>
        public static KeyValuePair<T, O>[] ConvertToKeyValuePairArray<T, O>(IEnumerable<T> keys, IEnumerable<O> values)
        {
            if (keys.Count() != values.Count()) throw new ArgumentException("Cannot convert to KeyValuePair array - Number of keys does not match number of values");

            int count = keys.Count();
            var list = new List<KeyValuePair<T, O>>();
            var keyList = keys.ToArray();
            var valueList = values.ToArray();
            for (int i = 0; i < count; i++)
            {
                list.Add(new KeyValuePair<T, O>(keyList[i], valueList[i]));
            }
            return list.ToArray();
        }
    }
}

namespace SqlKata
{
    public static class QueryAsExtensions
    {
        /// <param name="updateCol">Column to Update</param>
        /// <param name="newVal">Value for the column</param>
        /// <inheritdoc cref="SqlKata.Query.AsInsert(IEnumerable{KeyValuePair{string, object}}, bool)"/>
        /// <param name="qry"/>
        public static SqlKata.Query AsInsert(this SqlKata.Query qry, string updateCol, object newVal, bool returnID = false)
            => qry.AsInsert(DataBaseObjects.Extensions.ConvertToKeyValuePairArray(updateCol, newVal), returnID);

        /// <param name="updateCol">Column to Update</param>
        /// <param name="newVal">Value for the column</param>
        /// <inheritdoc cref="SqlKata.Query.AsUpdate(IEnumerable{KeyValuePair{string, object}})" />
        /// <param name="qry"/>
        public static SqlKata.Query AsUpdate(this SqlKata.Query qry, string updateCol, object newVal)
            => qry.AsUpdate(DataBaseObjects.Extensions.ConvertToKeyValuePairArray(updateCol, newVal));

    }

    /// <summary>
    /// Extensions copied from SqlKata github that we need to override for compilers usage
    /// </summary>
    internal static class InternalExtensions
    {
        // https://github.com/sqlkata/querybuilder/blob/044c7ae48591ed956ab0ffb33c556b9e59ea9d6d/QueryBuilder/Query.cs#L35
        internal static int GetOffset(this Query query, SqlKata.Compilers.Compiler engine)
        {
            var offset = query.GetOneComponent<OffsetClause>("offset", engine.EngineCode);
            return offset?.Offset ?? 0;
        }
        // https://github.com/sqlkata/querybuilder/blob/044c7ae48591ed956ab0ffb33c556b9e59ea9d6d/QueryBuilder/Query.cs#L35
        internal static int GetLimit(this Query query, SqlKata.Compilers.Compiler engine)
        {
            var limit = query.GetOneComponent<LimitClause>("limit", engine.EngineCode);
            return limit?.Limit ?? 0;
        }
    }
}
