using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseObjects.DataBaseTypes
{
    public partial class ExcelWorkBook
    {

        /// <summary>
        /// Represents an Excel Worksheet that does not specify a primary key
        /// </summary>
        public class WorkSheet : DataBaseObjects.DataBaseTable
        {

            public WorkSheet(ExcelWorkBook workbook, string sheetName, bool hasHeaders = true) : base(workbook, sheetName)
            {
                HasHeaders = hasHeaders;
            }

            public bool HasHeaders { get; init; }

            public ExcelWorkBook ParentWorkBook => (ExcelWorkBook)base.Parent;

            protected override IDbConnection GetDatabaseConnection()
            {
                return ParentWorkBook.GetDatabaseConnection(HasHeaders);
            }

            /// <summary>Insertion cannot be performed on an Excel Workbook</summary>
            /// <exception cref="NotImplementedException"/>
            public override int Insert(IEnumerable<string> ColNames, IEnumerable<object> ColValues)
            {
                throw new NotImplementedException("Cannot perform insertion into Excel Workbooks");
            }

            protected override int RunUpsert(string SearchCol, object SearchValue, IEnumerable<KeyValuePair<string, object>> UpdatePairs, bool InsertOnly = false)
            {
                throw new NotImplementedException("Cannot perform insertion into Excel Workbooks");
                //return base.RunUpsert(SearchCol, SearchValue, UpdatePairs, InsertOnly);
            }
        }

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

            public bool GetValueAsBool(object PrimaryKeyValue, string ReturnColName) => GetValue(PrimaryKeyValue, ReturnColName).ConvertToBool();

            public string GetValueAsString(object PrimaryKeyValue, string ReturnColName) => GetValue(PrimaryKeyValue, ReturnColName).ConvertToString();

            public int GetValueAsInt(object PrimaryKeyValue, string ReturnColName) => GetValue(PrimaryKeyValue, ReturnColName).ConvertToInt();

            public Task<object> GetValueAsync(object PrimaryKeyValue, string ReturnColName) => Task.Run(() => GetValue(PrimaryKeyValue, ReturnColName));

            public Task<bool> GetValueAsBoolAsync(object PrimaryKeyValue, string ReturnColName) => Task.Run(() => GetValueAsBool(PrimaryKeyValue, ReturnColName));

            public Task<string> GetValueAsStringAsync(object PrimaryKeyValue, string ReturnColName) => Task.Run(() => GetValueAsString(PrimaryKeyValue, ReturnColName));

            public Task<int> GetValueAsIntAsync(object PrimaryKeyValue, string ReturnColName) => Task.Run(() => GetValueAsInt(PrimaryKeyValue, ReturnColName));

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

        /// <summary>
        /// Represents an Excel Worksheet that has a set of columns that act as a compound key
        /// </summary>
        public class CompoundKeyWorkSheet : WorkSheet, ICompoundKeyDataBaseTable
        {

            public CompoundKeyWorkSheet(ExcelWorkBook workbook, string sheetName, bool hasHeaders = true, params string[] compoundKeyColumns) : base(workbook, sheetName, hasHeaders)
            {
                if (compoundKeyColumns is null) throw new ArgumentNullException(nameof(compoundKeyColumns));
                if (compoundKeyColumns.Length == 0) throw new ArgumentException("No column names have been specified!", nameof(compoundKeyColumns));
                foreach (string s in compoundKeyColumns)
                {
                    if (s.IsNullOrEmpty())
                        throw new ArgumentException("atleast one of the column names in the compoundKeyColumns parameter was null or empty!");
                }
                CompoundKeyColumns = compoundKeyColumns;
            }


            public string[] CompoundKeyColumns { get; }

            public int CompoundKeyColumnCount => CompoundKeyColumns.Length;


            public DataRow GetDataRow(object[] CompoundKeyValues)
            {
                return Parent.GetDataRow(new SqlKata.Query(TableName).Where(Extensions.ConvertToKeyValuePairArray(CompoundKeyColumns, CompoundKeyValues)));
            }

            public Task<DataRow> GetDataRowAsync(object[] CompoundKeyValues) => Task.Run(() => GetDataRow(CompoundKeyValues));
        }
    }
}
