using SqlKata;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RFBCodeWorks.DataBaseObjects.DataBaseTypes
{
    /// <summary>
    /// Represents a data connection to an excel workbook
    /// </summary>
    public partial class ExcelWorkBook : AbstractDataBase<OleDbConnection, OleDbCommand>
    {
        /// <summary>
        /// Create an object that represents an <see cref="OleDbConnection"/> to some Excel Workbook
        /// </summary>
        /// <param name="path">The path to the excel workbook</param>
        /// <param name="hasHeaders"><inheritdoc cref="HasHeaders" path="*"/></param>
       public ExcelWorkBook(string path, bool? hasHeaders = true) : base(GetConnection(path, hasHeaders).ConnectionString)
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
                base.ConnectionString = GetConnection().ConnectionString;
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
                if (!string.IsNullOrWhiteSpace(workbookPath)) base.ConnectionString = GetConnection().ConnectionString;
            }
        }

        /// <summary>
        /// Check if the workbook exists at the specified <see cref="WorkbookPath"/>
        /// </summary>
        /// <inheritdoc cref="File.Exists(string)"/>
        public bool FileExists => File.Exists(WorkbookPath);

        /// <inheritdoc/>
        public override Compiler Compiler => RFBCodeWorks.SqlKata.MsOfficeCompilers.ExcelWorkbookCompiler.ExcelCompiler;


        #region < Get Database Connection >

        /// <inheritdoc/>
        public override OleDbConnection GetConnection()
        {
           return GetConnection(WorkbookPath, HasHeaders);
        }

        /// <param name="hasHeaders"><inheritdoc cref="HasHeaders" path="*"/></param>
        /// <inheritdoc cref="GetConnection()"/>
        public OleDbConnection GetDatabaseConnection(bool? hasHeaders)
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
            //ValidateWorkbookPath(workbookPath);
            if (System.IO.Path.GetExtension(workbookPath) == ".xlsx" || System.IO.Path.GetExtension(workbookPath) == ".xlsm")
                return GetACEConnection(workbookPath, hasHeaders);
            else
                return GetJETConnection(workbookPath, hasHeaders);
        }

        /// <inheritdoc cref="GetConnection(string, bool?)" />
        public static OleDbConnection GetACEConnection(string workbookPath, bool? hasHeaders = null)
        {
            ValidateWorkbookPath(workbookPath);
            string HDR = hasHeaders is null ? string.Empty : (bool)hasHeaders ? ";HDR=Yes" : ";HDR=No";
            return new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + workbookPath + ";Extended Properties=\"Excel 12.0" + HDR + ";IMEX=0\"");
        }

        /// <inheritdoc cref="GetConnection(string, bool?)" />
        public static OleDbConnection GetJETConnection(string workbookPath, bool? hasHeaders = null)
        {
            ValidateWorkbookPath(workbookPath);
            string HDR = hasHeaders is null ? string.Empty : (bool)hasHeaders ? ";HDR=Yes" : ";HDR=No";
            return new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + workbookPath + ";Extended Properties=\"Excel 8.0" + HDR + ";IMEX=0\"");
        }

        /// <summary>
        /// Provide a Connection String for a MS Access DB linked table from an excel file 
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="Headers"></param>
        /// <returns></returns>
        private static string ConnStr_excel(string FilePath, bool Headers = true)
        {
            string Conn = "";
            string Hdr = (Headers) ? "YES" : "NO";
            switch (true)
            {
                case true when Path.GetExtension(FilePath) == ".xlsm":
                    Conn = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={FilePath};Excel 12.0 Macro;HDR={Hdr};IMEX=1;";
                    break;
                case true when Path.GetExtension(FilePath) == ".xlsx":
                    Conn = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={FilePath};Excel 12.0 Xml;HDR={Hdr};IMEX=1;";
                    break;
                case true when Path.GetExtension(FilePath) == ".xls":
                    //Only available on 32-bit version! 64-bit driver does not include support for JET
                    Conn = $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={FilePath};Excel 8.0;HDR={Hdr};IMEX=1;";
                    break;
            }
            return Conn;
        }

        #endregion


        #region < Overrides >

        private const string WorksheetNotFoundCatchMsg = "is not a valid name.";
        private const string MissingDollarSignCatchMsg = "Microsoft Access database engine could not find the object";
        //lang=regex
        private const string WorksheetNotFoundRegex = ".*?(?<name>['].*?['])\\s*.*";
        //lang=regex
        private const string MissingDollarSignRegex = ".*(?<name>['].*?['])[.].*";

        private const string MissingDollarSignError = "The sheet was not found within the Excel Document. " +
            "Verify that the sheet name is followed by dollar sign. " +
            "Ex: [Sheet1$]  Supplied sheet name: ";

        private T TryCatchMissingSheet<T>(Func<T> func)
        {
            try
            {
                return func();
            }
            catch (OleDbException oe) when (oe.Message.Contains(WorksheetNotFoundCatchMsg))
            {
                Regex extractTableName = new Regex(WorksheetNotFoundRegex, RegexOptions.Compiled);
                var tblName = extractTableName.Match(oe.Message).Groups["name"].Value;
                var e = new ExcelTableNotFoundException(tblName, this.WorkbookPath, oe);
                e.Data.Add("WorkbookPath", WorkbookPath);
                e.Data.Add("Table Name", tblName);
                throw e;
            }
            catch (OleDbException oe) when (oe.Message.Contains(MissingDollarSignCatchMsg))
            {
                Regex extractTableName = new Regex(MissingDollarSignRegex, RegexOptions.Compiled);
                var tblName = extractTableName.Match(oe.Message).Groups["name"].Value;
                var e = new ExcelTableNotFoundException(MissingDollarSignError + tblName, oe);
                e.Data.Add("WorkbookPath", WorkbookPath);
                e.Data.Add("Table Name", tblName);
                Console.WriteLine(e.Message);
                throw e;
            }
        }

        private async Task<T> TryCatchMissingSheet<T>(Task<T> func)
        {
            try
            {
                return await func;
            }
            catch (OleDbException oe) when (oe.Message.Contains(WorksheetNotFoundCatchMsg))
            {
                Regex extractTableName = new Regex(WorksheetNotFoundRegex, RegexOptions.Compiled);
                var tblName = extractTableName.Match(oe.Message).Groups["name"].Value;
                var e = new ExcelTableNotFoundException(tblName, this.WorkbookPath, oe);
                e.Data.Add("WorkbookPath", WorkbookPath);
                e.Data.Add("Table Name", tblName);
                throw e;
            }
            catch (OleDbException oe) when (oe.Message.Contains(MissingDollarSignCatchMsg))
            {
                Regex extractTableName = new Regex(MissingDollarSignRegex, RegexOptions.Compiled);
                var tblName = extractTableName.Match(oe.Message).Groups["name"].Value;
                var e = new ExcelTableNotFoundException(MissingDollarSignError + tblName, oe); 
                e.Data.Add("WorkbookPath", WorkbookPath);
                e.Data.Add("Table Name", tblName);
                throw e;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="ExcelTableNotFoundException"/>
        public override DataTable GetDataTable(Query query)
        {
            return TryCatchMissingSheet(() => base.GetDataTable(query));
        }

        /// <inheritdoc/>
        /// <exception cref="ExcelTableNotFoundException"/>
        public override DataTable GetDataTable(string query, params KeyValuePair<string, object>[] parameters)
        {
            return TryCatchMissingSheet(() => base.GetDataTable(query, parameters));
        }

        /// <inheritdoc/>
        public override Task<DataTable> GetDataTableAsync(Query query, CancellationToken cancellationToken = default)
        {
            return TryCatchMissingSheet(base.GetDataTableAsync(query, cancellationToken));
        }

        /// <inheritdoc/>
        public override Task<DataTable> GetDataTableAsync(string query, CancellationToken cancellationToken = default, params KeyValuePair<string, object>[] parameters)
        {
            return TryCatchMissingSheet(base.GetDataTableAsync(query, cancellationToken, parameters));
        }

        /// <inheritdoc/>
        public override object GetValue(Query query)
        {
            return TryCatchMissingSheet(() => base.GetValue(query));
        }

        /// <inheritdoc/>
        public override Task<object> GetValueAsync(Query query, CancellationToken cancellationToken = default)
        {
            return TryCatchMissingSheet(base.GetValueAsync(query, cancellationToken));
        }

        #endregion

    }

}
