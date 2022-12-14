using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace RFBCodeWorks.DataBaseObjects.DataBaseTypes
{
    public partial class ExcelWorkBook
    {

        /// <summary>
        /// Represents an Excel Worksheet that has a column that acts as a primary key
        /// </summary>
        public class SimpleKeyWorkSheet : WorkSheet, ISimpleKeyDataBaseTable
        {

            public SimpleKeyWorkSheet(ExcelWorkBook workbook, string sheetName, string primaryKey, bool hasHeaders = true) : base(workbook, sheetName, hasHeaders)
            {
                PrimaryKey = primaryKey.IsNotEmpty() ? primaryKey : throw new ArgumentException("Invalid Primary Key");
            }

            public string PrimaryKey { get; init; }

            public object GetValue(object PrimaryKeyValue, string ReturnColName) => Parent.GetValue(TableName, PrimaryKey, PrimaryKeyValue, ReturnColName);

            public bool? GetValueAsBool(object PrimaryKeyValue, string ReturnColName)
            {
                var val = GetValue(PrimaryKeyValue, ReturnColName);
                if (val is bool b) return b;
                if (val is DBNull) return null;
                if (val is string str)
                {
                    switch(str.ToLower().Trim())
                    {
                        case "0":
                        case "false":
                        case "":
                            return false;
                        case "1":
                        case "true":
                            return true;
                    }
                }
                throw new InvalidOperationException("Unable to convert ");
            }

            public string GetValueAsString(object PrimaryKeyValue, string ReturnColName) => GetValue(PrimaryKeyValue, ReturnColName).ConvertToString();

            public int? GetValueAsInt(object PrimaryKeyValue, string ReturnColName) => GetValue(PrimaryKeyValue, ReturnColName).SanitizeToInt();

            public Task<object> GetValueAsync(object PrimaryKeyValue, string ReturnColName) => Task.Run(() => GetValue(PrimaryKeyValue, ReturnColName));

            public Task<bool?> GetValueAsBoolAsync(object PrimaryKeyValue, string ReturnColName) => Task.Run(() => GetValueAsBool(PrimaryKeyValue, ReturnColName));

            public Task<string> GetValueAsStringAsync(object PrimaryKeyValue, string ReturnColName) => Task.Run(() => GetValueAsString(PrimaryKeyValue, ReturnColName));

            public Task<int?> GetValueAsIntAsync(object PrimaryKeyValue, string ReturnColName) => Task.Run(() => GetValueAsInt(PrimaryKeyValue, ReturnColName));

            public DataRow GetDataRow(object PrimaryKeyValue) => Parent.GetDataRow(new SqlKata.Query(TableName).Where(PrimaryKey, PrimaryKeyValue));

            public Task<DataRow> GetDataRowAsync(object PrimaryKeyValue) => Task.Run(() => GetDataRow(PrimaryKeyValue));

            /// <inheritdoc/>
            public virtual Dictionary<int, string> GetDictionary(string column)
            {
                var tbl = GetDataTable(PrimaryKey, column);
                if (tbl is null) throw new Exception("Failed to retrieve DataTable from Database - Cannot construct Dictionary");
                var Dict = new Dictionary<int, string>();
                using (DataTableReader Rdr = tbl.CreateDataReader())
                {
                    //Populate the first collection in dictionary with table information
                    //Dict.Add("ColumnNames", Tbl.Columns[KeyColumn].ColumnName + "|" + Tbl.Columns[ValueColumn].ColumnName);
                    //populate the dictionary
                    while (Rdr.Read())
                    {
                        string val;
                        if (Rdr.GetValue(0).ConvertToString().IsNullOrEmpty()) continue;
                        int key = ObjectExtensions.ConvertToInt(Rdr.GetValue(0));
                        if (Rdr.IsDBNull(1)) { val = string.Empty; } else { val = Rdr.GetValue(1).ConvertToString(); }
                        Dict.Add(key, val);
                    }
                }
                return Dict;
            }

            /// <summary>
            /// Create a dictionary where the key and values are both strings
            /// </summary>
            /// <param name="column">the VALUE column</param>
            /// <returns></returns>
            public virtual Dictionary<string, string> GetStringDictionary(string column)
            {
                var tbl = GetDataTable(PrimaryKey, column);
                if (tbl is null) throw new Exception("Failed to retrieve DataTable from Database - Cannot construct Dictionary");
                var Dict = new Dictionary<string, string>();
                using (DataTableReader Rdr = tbl.CreateDataReader())
                {
                    //Populate the first collection in dictionary with table information
                    //Dict.Add("ColumnNames", Tbl.Columns[KeyColumn].ColumnName + "|" + Tbl.Columns[ValueColumn].ColumnName);
                    //populate the dictionary
                    while (Rdr.Read())
                    {
                        string val;
                        string key = ObjectExtensions.ConvertToString(Rdr.GetValue(0));
                        if (Rdr.IsDBNull(1)) { val = string.Empty; } else { val = Rdr.GetValue(1).ConvertToString(); }
                        if (key.IsNotEmpty()) Dict.Add(key, val);
                    }
                }
                return Dict;
            }

            /// <inheritdoc/>
            public virtual Dictionary<X, Y> GetDictionary<X, Y>(Func<DataTable, Dictionary<X, Y>> dictionaryBuilder, params string[] valueColumns)
            {
                return dictionaryBuilder(GetDataTable(new string[] { PrimaryKey }.AddRange(valueColumns)));
            }

        }

    }
}
