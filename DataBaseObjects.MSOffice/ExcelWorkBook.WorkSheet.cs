using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RFBCodeWorks.DatabaseObjects.DatabaseTypes
{
    public partial class ExcelWorkBook
    {

        /// <summary>
        /// Represents an Excel Worksheet that does not specify a primary key
        /// </summary>
        public class WorkSheet : RFBCodeWorks.DatabaseObjects.DatabaseTable
        {
            /// <summary>
            /// Create a new Worksheet object
            /// </summary>
            /// <param name="workbook">the <see cref="ExcelWorkBook"/> object this worksheet belongs to</param>
            /// <param name="sheetName">The name of the worksheet</param>
            /// <param name="hasHeaders"><inheritdoc cref="HasHeaders" path="*"/></param>
            public WorkSheet(ExcelWorkBook workbook, string sheetName, bool? hasHeaders = true) : base(workbook, sheetName)
            {
                HasHeaders = hasHeaders;
            }

            /// <summary>
            /// Treat the first row as if it were the headers (column names) of a table <br/>
            /// </summary>
            /// <remarks>
            /// If <see langword="true"/> - Include ';HDR=Yes' in the generated connection string <br/>
            /// If <see langword="false"/> - Include ';HDR=No' in the generated connection string <br/>
            /// If <see langword="null"/> - the argument is not provided in the generated connection string
            /// </remarks>
            public bool? HasHeaders { get; set; }

            /// <summary>
            /// The workbook this worksheet belongs to
            /// </summary>
            public ExcelWorkBook ParentWorkBook => (ExcelWorkBook)base.Parent;

            /// <inheritdoc/>
            protected override DbConnection GetDatabaseConnection()
            {
                return ParentWorkBook.GetDatabaseConnection(HasHeaders);
            }


            #region < Insert >

            /// <summary>Insertion cannot be performed on an Excel Workbook</summary>
            /// <exception cref="NotImplementedException"/>
            public override int Insert(IEnumerable<KeyValuePair<string, object>> values)
            {
                throw new NotImplementedException("Cannot perform SQL Insert into Excel Workbooks");
            }

            /// <summary>Insertion cannot be performed on an Excel Workbook</summary>
            /// <exception cref="NotImplementedException"/>
            public override Task<int> InsertAsync(IEnumerable<KeyValuePair<string, object>> values, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException("Cannot perform SQL Insert into Excel Workbooks");
            }

            /// <summary>Insertion cannot be performed on an Excel Workbook</summary>
            /// <exception cref="NotImplementedException"/>
            public override int Insert(IEnumerable<string> columns, IEnumerable<object> values)
            {
                throw new NotImplementedException("Cannot perform SQL Insert into Excel Workbooks");
            }

            /// <summary>Insertion cannot be performed on an Excel Workbook</summary>
            /// <exception cref="NotImplementedException"/>
            public override Task<int> InsertAsync(IEnumerable<string> columns, IEnumerable<object> values, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException("Cannot perform SQL Insert into Excel Workbooks");
            }

            #endregion
        }
    }
}
