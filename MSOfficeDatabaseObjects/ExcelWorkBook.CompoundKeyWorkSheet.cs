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
                    if (string.IsNullOrWhiteSpace(s))
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
