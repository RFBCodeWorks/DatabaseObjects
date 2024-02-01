using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RFBCodeWorks.DatabaseObjects.DatabaseTypes
{
    /// <summary>
    /// Class used to generate the Connection Strings for MS Excel Files
    /// </summary>
    /// <remarks><see href="https://www.connectionstrings.com/excel/"/></remarks>
    public class ExcelConnectionStringBuilder
    {

        /// <inheritdoc cref="ExcelWorkBook.WorkbookPath"/>
        
        public string WorkbookPath { get; set; }

        /// <inheritdoc cref="ExcelWorkBook.Provider"/>
        public MSOfficeConnectionProvider Provider { get; set; } = ExcelWorkBook.DefaultProvider;
        
        /// <inheritdoc cref="ExcelWorkBook.HasHeaders"/>
        public bool? Headers { get; set; }

        /// <summary>
        /// Generates the connection string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(WorkbookPath)) throw new ArgumentException("Path has no value");
            if (!System.IO.Path.IsPathRooted(WorkbookPath)) throw new ArgumentException("Path is not rooted!");
            if (!System.IO.Path.HasExtension(WorkbookPath)) throw new ArgumentException("Path does not have an extension!");

            string ext = Path.GetExtension(WorkbookPath).ToLowerInvariant();
            StringBuilder connectionBuilder = new();

#if _WIN32
            // If set to Default and is 32-bit, evaluate the file extension
            Provider = Provider != MSOfficeConnectionProvider.Default ? Provider : ext == ".xls" ? MSOfficeConnectionProvider.Jet4 : MSOfficeConnectionProvider.Ace12;
#endif

#pragma warning disable CS0618 // Type or member is obsolete
            connectionBuilder.Append(Provider switch
            {
                MSOfficeConnectionProvider.Jet4 => IntPtr.Size == 4 ? "Provider=Microsoft.Jet.OLEDB.4.0;" : throw new InvalidOperationException("Jet4.0 is only compatible with 32-Bit assemblies."),
                MSOfficeConnectionProvider.Default or
                MSOfficeConnectionProvider.Ace12 => "Provider=Microsoft.ACE.OLEDB.12.0;",
                MSOfficeConnectionProvider.Ace16 => "Provider=Microsoft.ACE.OLEDB.16.0;",
                _ => throw new NotImplementedException($"Provider {Provider} not implemented")
            });
#pragma warning restore CS0618 // Type or member is obsolete

            connectionBuilder.Append($"Data Source={WorkbookPath};");

            // Extended properties
            string hdr = !Headers.HasValue ? string.Empty : Headers.Value ? "HDR=YES;" : "HDR=NO;";
            connectionBuilder.Append(true switch
            {
                true when ext == ".xls" => $"Extended Properties=\"Excel 8.0;{hdr}IMEX=1\"",
                true when ext == ".xlsb" => $"Extended Properties=\"Excel 12.0;{hdr}IMEX=1\"",
                true when ext == ".xlsx" => $"Extended Properties=\"Excel 12.0 Xml;{hdr}IMEX=1\"",
                true when ext == ".xlsm" => $"Extended Properties=\"Excel 12.0 Macro;{hdr}IMEX=1\"",
                _ => throw new NotImplementedException($"Unexpected file extension : {ext}")
            });

            return connectionBuilder.ToString();
        }

    }

    /* // CData ADO.NET Provider for Excel
    public enum ExcelEmptyTextMode
    {
        /// <summary>
        /// Not included in the connection string
        /// </summary>
        Omitted,

        /// <summary>
        /// Empty cells will report their value as null
        /// </summary>
        /// <remarks>
        /// Empty Text Mode=EmptyAsNull;
        /// </remarks>
        EmptyAsNull,

        /// <summary>
        /// Empty cells will report their value as string.empty
        /// </summary>
        /// <remarks>
        /// Empty Text Mode=NullAsEmpty;
        /// </remarks>
        NullAsEmpty
    }
    */

    /*

        /// <summary>
        /// Do not treat values starting with equals (=) as formulas during inserts and updates.
        /// <br/> Default = False (grab values, not formulas)
        /// </summary>
        public bool AllowFormulas { get; set; }

        /// <summary>
        /// Determine how Empty cells are treated
        /// </summary>
        public ExcelEmptyTextMode EmptyTextMode { get; set; }

        /// <summary>
        /// Suppress formula calculation errors
        /// </summary>
        public bool IgnoreCalculationErrors { get; set; }

        /// <summary>
        /// Treats Columns as Rows and Rows as Columns ( default = false )
        /// </summary>
        public bool TiltedSheets { get; set; }
        
        const string s_EmptyTextModeAsNull = "Empty Text Mode=EmptyAsNull;";
        const string s_EmptyTextModeAsEmpty = "Empty Text Mode=NullAsEmpty;";
        const string s_IgnoreCalcErrors = "Ignore Calc Error={0};"; // true/false
        const string s_TilededSheets = "Orientation=Horizontal;"; // rows are headers and columns are rows
        */
}
