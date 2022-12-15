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
        /// Represents an Excel Worksheet that does not specify a primary key
        /// </summary>
        public class WorkSheet : RFBCodeWorks.DataBaseObjects.DataBaseTable
        {

            public WorkSheet(ExcelWorkBook workbook, string sheetName, bool hasHeaders = true) : base(workbook, sheetName)
            {
                HasHeaders = hasHeaders;
            }

            public bool HasHeaders { get; set; }

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
    }
}
