using SqlKata;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using SK = SqlKata;

namespace RFBCodeWorks.SqlKata.Extensions
{
    /// <summary>
    /// Contains extension methods for the <see cref="SK.Query"/> class
    /// </summary>
    public static class Extensions
    {
        /// <param name="updateCol">Column to Update</param>
        /// <param name="newVal">Value for the column</param>
        /// <inheritdoc cref="SK.Query.AsUpdate(IEnumerable{KeyValuePair{string, object}})" />
        /// <param name="qry">The query to apply the clause to</param>
        public static SK.Query AsUpdate(this SK.Query qry, string updateCol, object newVal)
            => qry.AsUpdate(new KeyValuePair<string, object>(updateCol, newVal));

        /// <param name="updateCol">Column to Update</param>
        /// <param name="newVal">Value for the column</param>
        /// <inheritdoc cref="SK.Query.AsInsert(IEnumerable{KeyValuePair{string, object}}, bool)"/>
        /// <param name="qry">The query to apply the clause to</param>
        /// <param name="returnID">If <see langword="true"/>, have the query return the ID of the inserted row</param>
        public static SK.Query AsInsert(this SK.Query qry, string updateCol, object newVal, bool returnID = false)
            => qry.AsInsert(new KeyValuePair<string, object>(updateCol, newVal), returnID);

        /// <inheritdoc cref="IWhereCondition.ApplyToQuery(SK.Query)"/>
        /// <param name="query"/>
        /// <param name="condition">The WHERE condition to apply to the <paramref name="query"/></param>
        public static SK.Query Where(this SK.Query query, IWhereCondition condition)
        {
            if (condition is null) throw new ArgumentNullException(nameof(condition));
            return (condition.ApplyToQuery(query ?? throw new ArgumentNullException(nameof(query))));
        }

        /// <summary>
        /// Apply the collection of <paramref name="conditions"/> to the <paramref name="query"/>
        /// </summary>
        /// <inheritdoc cref="IWhereCondition.ApplyToQuery(SK.Query)"/>
        /// <param name="query"/>
        /// <param name="conditions">The collection of <see cref="IWhereCondition"/>s to apply to the <paramref name="query"/></param>
        public static SK.Query Where(this SK.Query query, IEnumerable<IWhereCondition> conditions)
        {
            if (query is null) throw new ArgumentNullException(nameof(query));
            if (conditions is null) throw new ArgumentNullException(nameof(conditions));

            foreach (var w in conditions)
            {
                w?.ApplyToQuery(query);
            }
            return query;
        }

        /// <summary>
        /// Create a new <see cref="DbCommand"/>, whose CommandText and parameters are acquired from the <paramref name="query"/>
        /// </summary>
        /// <remarks>
        /// The compiled query will have its '<see cref="SqlResult.NamedBindings"/>' applied to the <typeparamref name="T"/>'s parameter collection.
        /// </remarks>
        /// <typeparam name="T">The type of <see cref="DbCommand"/> to create</typeparam>
        /// <param name="query">The query that will be compiled. </param>
        /// <param name="compiler">The compiler that will compile the query</param>
        /// <returns>A new <see cref="DbCommand"/></returns>
        /// <exception cref="ArgumentNullException"/>
        public static T ToDbCommand<T>(this SK.Query query, SK.Compilers.Compiler compiler)
            where T : DbCommand, new()
        {
            if (compiler is null) throw new ArgumentNullException(nameof(compiler));
            var result = compiler.Compile(query ?? throw new ArgumentNullException(nameof(query)));
            var cmd = new T
            {
                CommandText = result.Sql
            };
            foreach (var p in result.NamedBindings)
            {
                _ = cmd.AddParameter(p.Key, p.Value);
            }
            return cmd;
        }

        /// <summary>
        /// Add a new parameter for the to the <paramref name="command"/>
        /// </summary>
        /// <param name="command">The <see cref="DbCommand"/> to add the parameter to</param>
        /// <param name="parameterName">Sets the <see cref="DbParameter.ParameterName"/></param>
        /// <param name="value"><inheritdoc cref="DbParameter.Value" path="*"/></param>
        /// <returns>The created parameter</returns>
        public static DbParameter AddParameter(this DbCommand command, string parameterName, object value)
        {
            var par = command.CreateParameter();
            par.ParameterName = parameterName ?? throw new ArgumentNullException(nameof(parameterName));
            par.Value = value;
            command.Parameters.Add(par);
            return par;
        }
    }
}
