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

namespace RFBCodeWorks.DatabaseObjects.DatabaseTypes
{
    /// <summary>
    /// Represents a data connection to an excel workbook
    /// </summary>
    public partial class ExcelWorkBook : OleDBDatabase
    {
        /// <summary>
        /// Set the Default Provider for the library to use when generating connection strings for Excel workbooks
        /// </summary>
        public static MSOfficeConnectionProvider DefaultProvider { get; set; } = MSOfficeConnectionProvider.Default;

        /// <inheritdoc cref="ExcelWorkBook.ExcelWorkBook(string, bool?, MSOfficeConnectionProvider)"/>
        public ExcelWorkBook(string path) : this(path, null, default) { }

        /// <inheritdoc cref="ExcelWorkBook.ExcelWorkBook(string, bool?, MSOfficeConnectionProvider)"/>
        public ExcelWorkBook(string path, bool? hasHeaders) : this(path, hasHeaders, default) { }

        /// <summary>
        /// Create an object that represents an <see cref="OleDbConnection"/> to some Excel Workbook
        /// </summary>
        /// <param name="path">Path to the workbook - must be fully qualified</param>
        /// <inheritdoc cref="ExcelWorkBook.GetConnection(string, bool?, MSOfficeConnectionProvider?)"/>
        /// <param name="hasHeaders"/><param name="provider"/>
        public ExcelWorkBook(string path, bool? hasHeaders, MSOfficeConnectionProvider provider) : base(GetConnectionString(path, hasHeaders, provider))
        {
            this.hasHeaders = hasHeaders;
            workbookPath = path;
            Provider = provider;
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

        /// <summary>
        /// The Connection Provider to use when generating a new connection string
        /// </summary>
        public MSOfficeConnectionProvider Provider { get; set; }


        #region < Get Database Connection >

        /// <inheritdoc/>
        public override OleDbConnection GetConnection()
        {
           return GetConnection(WorkbookPath, HasHeaders, Provider);
        }

        /// <param name="hasHeaders"><inheritdoc cref="HasHeaders" path="*"/></param>
        /// <inheritdoc cref="GetConnection()"/>
        public OleDbConnection GetDatabaseConnection(bool? hasHeaders)
        {
            return GetConnection(WorkbookPath, hasHeaders, Provider);
        }

        #endregion

        #region < Connection Strings >

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        [Obsolete("Use ExcelWorkbook.GetConnection() instead.", true)]
        public static OleDbConnection GetACEConnection(string workbookPath, bool? hasHeaders = null) => throw new NotImplementedException("Deprecated");
        [Obsolete("Use ExcelWorkbook.GetConnection() instead.", true)]
        public static OleDbConnection GetJETConnection(string workbookPath, bool? hasHeaders = null) => throw new NotImplementedException("Deprecated");
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Generate the <see cref="OleDbConnection"/> to the specified <paramref name="workbookPath"/>
        /// Generate a Connection string via <see cref="GetConnectionString(string, bool?, MSOfficeConnectionProvider?)"/>, and transform it into a new OleDBConnection
        /// </summary>
        /// <returns>A new <see cref="OleDbConnection"/> object</returns>
        /// <inheritdoc cref="GetConnectionString(string, bool?, MSOfficeConnectionProvider?)"/>
        public static OleDbConnection GetConnection(string workbookPath, bool? hasHeaders = true, MSOfficeConnectionProvider? provider = null)
        {
            return new OleDbConnection(GetConnectionString(workbookPath, hasHeaders, provider));
        }

        /// <summary>
        /// Generate a new OLEDB connection string
        /// </summary>
        /// <param name="workbookPath">Path to the workbook - must be fully qualified</param>
        /// <param name="hasHeaders"><inheritdoc cref="HasHeaders" path="*"/></param>
        /// <param name="provider">The selected Connection provider </param>
        /// <returns>A generated connection string</returns>
        /// <exception cref="ArgumentException"/>
        public static string GetConnectionString(string workbookPath, bool? hasHeaders = true, MSOfficeConnectionProvider? provider = null)
        {
            return new ExcelConnectionStringBuilder() { Headers = hasHeaders, Provider = provider ?? DefaultProvider, WorkbookPath = workbookPath }.ToString();
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
