using SqlKata;
using System;
using System.Collections.Generic;
using System.Text;

namespace SqlKata.Compilers
{
    /// <summary>
    /// Extensions copied from SqlKata github that we need to override for compilers usage
    /// </summary>
    internal static class InternalExtensions
    {
        // https://github.com/sqlkata/querybuilder/blob/c322d898547863f1a94f0058dc933d1e975cc533/QueryBuilder/Query.cs#L35
        internal static long GetOffset(this Query query, SqlKata.Compilers.Compiler engine)
        {
            var offset = query.GetOneComponent<OffsetClause>("offset", engine.EngineCode);
            return offset?.Offset ?? 0;
        }

        // https://github.com/sqlkata/querybuilder/blob/c322d898547863f1a94f0058dc933d1e975cc533/QueryBuilder/Query.cs#L43
        internal static int GetLimit(this Query query, SqlKata.Compilers.Compiler engine)
        {
            var limit = query.GetOneComponent<LimitClause>("limit", engine.EngineCode);
            return limit?.Limit ?? 0;
        }
    }
}
