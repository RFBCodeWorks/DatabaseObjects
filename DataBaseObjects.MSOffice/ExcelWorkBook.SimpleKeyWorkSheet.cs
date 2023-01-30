using SqlKata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RFBCodeWorks.DatabaseObjects.DatabaseTypes
{
    public partial class ExcelWorkBook
    {

        /// <summary>
        /// Represents an Excel Worksheet that has a column that acts as a primary key
        /// </summary>
        public class PrimaryKeyWorksheet : WorkSheet, IPrimaryKeyTable
        {

            /// <summary>
            /// Create a new Worksheet object that has a Primary-Key Column
            /// </summary>
            /// <inheritdoc cref="WorkSheet.WorkSheet(ExcelWorkBook, string, bool?)"/>
            /// <param name="workbook"/>
            /// <param name="sheetName"/>
            /// <param name="primaryKey"><inheritdoc cref="PrimaryKey" path="*"/></param>
            /// <param name="hasHeaders"/>
            public PrimaryKeyWorksheet(ExcelWorkBook workbook, string sheetName, string primaryKey, bool? hasHeaders = true) : base(workbook, sheetName, hasHeaders)
            {
                PrimaryKey = !string.IsNullOrWhiteSpace(primaryKey) ? primaryKey : throw new ArgumentException("Invalid Primary Key");
            }

            /// <inheritdoc/>
            public string PrimaryKey { get; set; }

            /// <inheritdoc/>
            public object GetValue(object PrimaryKeyValue, string ReturnColName) => Parent.GetValue(TableName, PrimaryKey, PrimaryKeyValue, ReturnColName);

            /// <inheritdoc/>
            public Task<object> GetValueAsync(object primaryKey, string returnColumn, CancellationToken cancellationToken) => Parent.GetValueAsync(TableName, PrimaryKey, primaryKey, returnColumn, cancellationToken);


            /// <summary>
            /// Gets a value from the worksheet, then attempts to convert it to a bool?
            /// </summary>
            /// <returns>A boolean value, or null if the cell is empty</returns>
            /// <exception cref="InvalidOperationException"/>
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
                var e = new InvalidOperationException("Unable to convert value to boolean");
                e.Data.Add("Worksheet Value: ", val);
                e.Data.Add(nameof(PrimaryKeyValue), PrimaryKeyValue);
                e.Data.Add(nameof(ReturnColName), ReturnColName);
                throw e;
            }

            /// <summary>
            /// Gets a string value from the worksheet
            /// </summary>
            /// <returns>The string value of the cell</returns>
            /// <exception cref="InvalidOperationException"/>
            public string GetValueAsString(object PrimaryKeyValue, string ReturnColName) => GetValue(PrimaryKeyValue, ReturnColName).ToString();

            /// <summary>
            /// Gets a value from the worksheet, then attempts to convert it to an integer
            /// </summary>
            /// <returns>If successfull, returns an integer. If the cell is empty, returns null.</returns>
            /// <inheritdoc cref="ObjectSanitizing.SanitizeToInt(object)"/>
            public int? GetValueAsInt(object PrimaryKeyValue, string ReturnColName) => GetValue(PrimaryKeyValue, ReturnColName).SanitizeToInt();

            /// <inheritdoc/>
            public DataRow GetDataRow(object PrimaryKeyValue) => Parent.GetDataRow(new Query(TableName).Where(PrimaryKey, PrimaryKeyValue));

            /// <inheritdoc/>
            public Task<DataRow> GetDataRowAsync(object PrimaryKeyValue, CancellationToken cancellationToken = default) => Parent.GetDataRowAsync(new Query(TableName).Where(PrimaryKey, PrimaryKeyValue), cancellationToken);

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
                        if (string.IsNullOrWhiteSpace(Rdr.GetValue(0).SanitizeToString())) continue;
                        int key = Rdr.GetValue(0).SanitizeToInt() ?? throw new InvalidOperationException("Null value was found in the DataTable when expected an integer");
                        if (Rdr.IsDBNull(1)) { val = string.Empty; } else { val = Rdr.GetValue(1).SanitizeToString(); }
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
                        string key = ObjectSanitizing.SanitizeToString(Rdr.GetValue(0));
                        if (Rdr.IsDBNull(1)) { val = string.Empty; } else { val = Rdr.GetValue(1).SanitizeToString(); }
                        if (!string.IsNullOrWhiteSpace(key)) Dict.Add(key, val);
                    }
                }
                return Dict;
            }

            /// <inheritdoc/>
            public virtual Dictionary<X, Y> GetDictionary<X, Y>(Func<DataTable, Dictionary<X, Y>> dictionaryBuilder, params string[] valueColumns)
            {
                if (valueColumns is null || valueColumns.Length == 0) 
                    valueColumns = new string[] { };
                else
                {
                    var list = valueColumns.ToList();
                    list.Remove(PrimaryKey);
                    list.Insert(0, PrimaryKey);
                    valueColumns = list.ToArray();
                }

                return dictionaryBuilder(GetDataTable(valueColumns));
            }
        }

    }
}
