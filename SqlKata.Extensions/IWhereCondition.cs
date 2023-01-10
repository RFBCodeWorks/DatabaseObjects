using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SK = SqlKata;

namespace RFBCodeWorks.SqlKata.Extensions
{
    /// <summary>
    /// Interface implemented by objects that can apply Where Conditions to a <see cref="SK.Query"/>
    /// </summary>
    public interface IWhereCondition
    {
        /// <summary>
        /// Apply this condition to the specified <paramref name="query"/>
        /// </summary>
        /// <param name="query">The query to apply the statement to</param>
        /// <returns><paramref name="query"/></returns>
        SK.Query ApplyToQuery(SK.Query query);
    }
}
