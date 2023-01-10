using SqlKata;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RFBCodeWorks.SqlKata.MsOfficeCompilers
{
    internal class MsAccessSqlResult : SqlKata.SqlResult
    {
        public MsAccessSqlResult() { }
        public MsAccessSqlResult(SqlResult ctx)
        {
            base.Query = ctx.Query;
            RawSql = ctx.RawSql;
            Bindings = ctx.Bindings;
            Sql = ctx.Sql;
            NamedBindings = base.NamedBindings;
        }

        private const char sw = '"';
        private const string stringwrapper = "\"";

        //language=Regex
        static Regex ValueRegex { get; } = new Regex(@"^" + stringwrapper + @".+?" + stringwrapper, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        protected override string WrapStringValue(string value)
        {
            //base = "'" + value.ToString().Replace("'", "''") + "'";

            return '"' + value.Replace("\"", "\"\"") + '"';
            //return base.WrapStringValue(value);

            //if (value[0] == sw && value.Last() == sw) return value;
            ////if (ValueRegex.IsMatch(value)) return value;
            //return stringwrapper + value + stringwrapper;
        }
    }
}