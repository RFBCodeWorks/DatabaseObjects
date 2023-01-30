using SqlKata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RFBCodeWorks.DatabaseObjects.DatabaseTypes
{
    public partial class ExcelWorkBook
    {

        /// <summary>
        /// Represents an Excel Worksheet that has a set of columns that act as a compound key
        /// </summary>
        public class CompoundKeyWorkSheet : WorkSheet, ICompositeKeyTable
        {
            /// <summary>
            /// Create a new worksheet object that has a compound primary key
            /// </summary>
            /// <inheritdoc cref="WorkSheet.WorkSheet(ExcelWorkBook, string, bool?)"/>
            /// <param name="workbook"></param>
            /// <param name="sheetName"></param>
            /// <param name="hasHeaders"></param>
            /// <param name="compoundKeyColumns"></param>
            public CompoundKeyWorkSheet(ExcelWorkBook workbook, string sheetName, bool hasHeaders = true, params string[] compoundKeyColumns) : base(workbook, sheetName, hasHeaders)
            {
                if (compoundKeyColumns is null) throw new ArgumentNullException(nameof(compoundKeyColumns));
                if (compoundKeyColumns.Length == 0) throw new ArgumentException("No column names have been specified!", nameof(compoundKeyColumns));
                foreach (string s in compoundKeyColumns)
                {
                    if (string.IsNullOrWhiteSpace(s))
                        throw new ArgumentException("atleast one of the column names in the compoundKeyColumns parameter was null or empty!");
                }
                CompositeKeyColumns = compoundKeyColumns;
            }

            /// <inheritdoc/>
            public string[] CompositeKeyColumns { get; }

            /// <inheritdoc/>
            public int CompositeColumnCount => CompositeKeyColumns.Length;

            /// <inheritdoc/>
            public DataRow GetDataRow(object[] CompoundKeyValues)
            {
                return Parent.GetDataRow(new Query(TableName).Where(DBOps.CreateKeyValuePairs(CompositeKeyColumns, CompoundKeyValues)));
            }

            /// <inheritdoc/>
            public Task<DataRow> GetDataRowAsync(object[] CompoundKeyValues, CancellationToken cancellationToken = default)
            {
                return Parent.GetDataRowAsync(new Query(TableName).Where(DBOps.CreateKeyValuePairs(CompositeKeyColumns, CompoundKeyValues)), cancellationToken);
            }
        }
    }
}
