using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RFBCodeWorks.DataBaseObjects.DataBaseTypes
{
    /// <summary>
    /// Represents a data connection to an excel workbook
    /// </summary>
    public partial class ExcelWorkBook : AbstractDataBase<OleDbConnection>
    {
       public ExcelWorkBook(string path, bool hasHeaders = true) : base(GetConnection(path, hasHeaders).ConnectionString)
        {
            this.hasHeaders = hasHeaders;
            workbookPath = path;
        }
        
        private string workbookPath;
        private bool? hasHeaders;

        /// <remarks> Updates the ConnectionString automatically when this value is modified.</remarks>
        public string WorkbookPath { 
            get => workbookPath;
            protected set
            {
                workbookPath = value;
                base.ConnectionString = GetDatabaseConnection().ConnectionString;
            }
        }

        /// <summary>
        /// Treat the first row of each worksheet as if it were the headers (column names) of a table <br/>
        /// </summary>
        /// <remarks>
        /// If <see langword="true"/> - Include ';HDR=Yes' in the generated connection string <br/>
        /// If <see langword="false"/> - Include ';HDR=No' in the generated connection string <br/>
        /// If <see langword="null"/> - the argument is not provided in the generated connection string
        /// </remarks>
        public bool? HasHeaders { 
            get => hasHeaders; 
            set {
                hasHeaders = value;
                if (!string.IsNullOrWhiteSpace(workbookPath)) base.ConnectionString = GetDatabaseConnection().ConnectionString;
            }
        }

        /// <summary>
        /// Check if the workbook exists at the specified <see cref="WorkbookPath"/>
        /// </summary>
        /// <inheritdoc cref="File.Exists(string)"/>
        public bool FileExists => File.Exists(WorkbookPath);

        /// <inheritdoc/>
        public override Compiler Compiler => RFBCodeWorks.SqlKataCompilers.ExcelWorkbookCompiler.ExcelCompiler;

        /// <inheritdoc/>
        public override OleDbConnection GetDatabaseConnection()
        {
           return ExcelOps.GetConnection(WorkbookPath, HasHeaders);
        }

        /// <param name="hasHeaders"><inheritdoc cref="HasHeaders" path="*"/></param>
        /// <inheritdoc cref="GetDatabaseConnection()"/>
        public OleDbConnection GetDatabaseConnection(bool hasHeaders)
        {
            return GetConnection(WorkbookPath, hasHeaders);
        }


        /// <exception cref="ArgumentException"/>
        ///// <exception cref="System.IO.FileNotFoundException"/>
        private static void ValidateWorkbookPath(string workbookPath)
        {
            if (string.IsNullOrWhiteSpace(workbookPath)) throw new ArgumentException("WorkBookPath has no value");
            if (!System.IO.Path.IsPathRooted(workbookPath)) throw new ArgumentException("WorkBookPath is not rooted!");
            if (!System.IO.Path.HasExtension(workbookPath)) throw new ArgumentException("WorkBookPath does not have an extension!");
            //if (!System.IO.File.Exists(workbookPath)) throw new System.IO.FileNotFoundException($"Workbook does not exist at specified location! - Path: \n {workbookPath}");
        }

        /// <summary>
        /// Generate the <see cref="OleDbConnection"/> to the specified <paramref name="workbookPath"/>
        /// </summary>
        /// <param name="workbookPath">Path to the workbook</param>
        /// <param name="hasHeaders"><inheritdoc cref="HasHeaders" path="*"/></param>
        /// <returns></returns>
        /// <inheritdoc cref="ValidateWorkbookPath(string)"/>
        public static OleDbConnection GetConnection(string workbookPath, bool? hasHeaders = null)
        {
            ValidateWorkbookPath(workbookPath);
            if (System.IO.Path.GetExtension(workbookPath) == ".xlsx" || System.IO.Path.GetExtension(workbookPath) == ".xlsm")
                return GetACEConnection(workbookPath, hasHeaders);
            else
                return GetJETConnection(workbookPath, hasHeaders);
        }

        /// <inheritdoc cref="GetConnection" />
        public static OleDbConnection GetACEConnection(string workbookPath, bool? hasHeaders = null)
        {
            ValidateWorkbookPath(workbookPath);
            string HDR = hasHeaders is null ? string.Empty : (bool)hasHeaders ? ";HDR=Yes" : ";HDR=No";
            return new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + workbookPath + ";Extended Properties=\"Excel 12.0" + HDR + ";IMEX=0\"");
        }

        /// <inheritdoc cref="GetConnection" />
        public static OleDbConnection GetJETConnection(string workbookPath, bool? hasHeaders = null)
        {
            ValidateWorkbookPath(workbookPath);
            string HDR = hasHeaders is null ? string.Empty : (bool)hasHeaders ? ";HDR=Yes" : ";HDR=No";
            return new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + workbookPath + ";Extended Properties=\"Excel 8.0" + HDR + ";IMEX=0\"");
        }

    }
    
}
