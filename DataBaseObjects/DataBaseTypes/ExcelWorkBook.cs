using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseObjects.DataBaseTypes
{
    /// <summary>
    /// Represents a data connection to an excel workbook
    /// </summary>
    public partial class ExcelWorkBook : AbstractDataBase<OleDbConnection>
    {
        
        public ExcelWorkBook(string path, bool hasHeaders = true) : base()
        {
            HasHeaders = hasHeaders;
            WorkbookPath = path;
        }
        
        private string workbookPath;
        private bool hasHeaders;

        /// <remarks> Updates the ConnectionString automatically when this value is modified.</remarks>
        public string WorkbookPath { 
            get => workbookPath; 
            init {
                workbookPath = value;
                base.ConnectionString = GetDatabaseConnection().ConnectionString;
            } 
        }

        /// <summary>
        /// Treat the first row of each worksheet as if it were the headers (column names) of a table
        /// </summary>
        public bool HasHeaders { 
            get => hasHeaders; 
            init {
                hasHeaders = value;
                if (workbookPath.IsNotEmpty()) base.ConnectionString = GetDatabaseConnection().ConnectionString;
            }
        }

        /// <summary>
        /// Check if the workbook exists at the specified <see cref="WorkbookPath"/>
        /// </summary>
        /// <inheritdoc cref="File.Exists(string)"/>
        public bool FileExists => File.Exists(WorkbookPath);

        /// <inheritdoc/>
        public override Compiler Compiler => CompilerSingletons.ExcelWorkbookCompiler;

        /// <inheritdoc/>
        public override OleDbConnection GetDatabaseConnection()
        {
           return ExcelOps.GetConnection(WorkbookPath, HasHeaders);
        }

        /// <param name="hasHeaders"><inheritdoc cref="HasHeaders" path="*"/></param>
        /// <inheritdoc cref="GetDatabaseConnection()"/>
        public OleDbConnection GetDatabaseConnection(bool hasHeaders)
        {
            return ExcelOps.GetConnection(WorkbookPath, hasHeaders);
        }

    }
    
}
