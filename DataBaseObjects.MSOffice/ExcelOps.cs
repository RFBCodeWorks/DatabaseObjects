using SqlKata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Runtime.CompilerServices;
using System.Text;

namespace RFBCodeWorks.DatabaseObjects
{
    /// <summary>
    /// Functions specific to Excel Workbooks
    /// </summary>
    /// *  Connection Strings: https://www.connectionstrings.com/excel/
    public static class ExcelOps
    {
        /// <summary>
        /// Add a new pair to the <see cref="Exception.Data"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void AddVariableData(this Exception e, object key, object value) => e.Data.Add(key, value);

        /// <summary>
        /// Test the <see cref="OleDbConnection"/> to the specified <paramref name="workbookPath"/>
        /// </summary>
        /// <param name="workbookPath">path to the workbook</param>
        /// <returns>TRUE if a connection was successfull, otherwise false</returns>
        public static bool TestConnection(string workbookPath)
        {
            if (!System.IO.File.Exists(workbookPath)) return false;
            return DBOps.TestConnection(GetConnection(workbookPath));
        }

        /// <inheritdoc cref="DatabaseTypes.ExcelWorkBook.GetConnection(string, bool?, DatabaseTypes.MSOfficeConnectionProvider)"/>
        public static OleDbConnection GetConnection(string workbookPath, bool? hasHeaders = null) 
            => DatabaseTypes.ExcelWorkBook.GetConnection(workbookPath, hasHeaders);

        /// <summary>
        /// Opens an <see cref="OleDbCommand"/> to the specified workbook and retrieves a <see cref="DataTable"/> representation of the <paramref name="SheetName"/>
        /// </summary>
        /// <param name="ExcelWorkBookPath">path to the workbook</param>
        /// <param name="SheetName">name of the sheet</param>
        /// <param name="hasHeaders">treat the first row as table headers</param>
        /// <returns>a <see cref="DataTable"/> that represents the specified <paramref name="SheetName"/></returns>
        /// <exception cref="System.IO.FileNotFoundException"/>
        /// <exception cref="ExcelTableNotFoundException"/>
        /// <exception cref="ArgumentException"/>
        // Getting a table from an excel workbook:  https://stackoverflow.com/questions/33994160/read-excel-table-data-using-c-sharp
        public static DataTable GetDataTable(string ExcelWorkBookPath, string SheetName, bool? hasHeaders = true)
        {
            // Get Connection to workbook
            DataTable DT = new();

            if (!System.IO.File.Exists(ExcelWorkBookPath)) throw new System.IO.FileNotFoundException("Excel Workbook Not Found", ExcelWorkBookPath);

            //Grab data
            try
            {
                using (OleDbConnection conn = GetConnection(ExcelWorkBookPath, hasHeaders))
                {
                    conn.Open();
                    DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables_Info, new object[] { null, null, null, "TABLE" });
                    //Loop through schema table until the desired table name is found in the Table_Name column
                    //int TableNameCol = DBOps.GetColNum(schemaTable, "TABLE_NAME");
                    string TblName = ""; bool tblFound = false;
                    string Tbl = SheetName;
                    if (Tbl.Substring(Tbl.Length - 1) != "$") Tbl += "$";
                    foreach (DataRow Row in schemaTable.Rows)
                    {
                        TblName = Row["TABLE_NAME"].ToString();
                        if (TblName.ToUpper() == Tbl.ToUpper()) { tblFound = true; break; }
                    }
                    if (tblFound == false) throw new ExcelTableNotFoundException("Unable to find ' " + SheetName + " ' in workbook: " + ExcelWorkBookPath);

                    using (OleDbDataAdapter daexcel = new OleDbDataAdapter($"SELECT * FROM [{TblName}]", conn))
                    {
                        DT.Locale = System.Globalization.CultureInfo.CurrentCulture; // Don't need I think...
                        daexcel.Fill(DT);
                    }
                    conn.Close();
                }
            }
            catch (Exception E)
            {
                E.AddVariableData(nameof(ExcelWorkBookPath), ExcelWorkBookPath);
                E.AddVariableData(nameof(SheetName), SheetName);
                throw;
            }
            return DT;
        }

        /// <param name="ExcelWorkBookPath">Excel Workbook to look up</param>
        /// <param name="SheetName">Sheet Name to grab from the excel workbook</param>
        /// <inheritdoc cref="DatabaseTypes.AbstractDatabase{TConnectionType, TCommandType}.GetValue(string, string, object, string)"/>
        /// <param name="lookupVal"/><param name="lookupColName"/><param name="returnColName"/>
        public static object GetValue(string ExcelWorkBookPath, string SheetName, string lookupColName, string lookupVal, string returnColName)
        {
            return new RFBCodeWorks.DatabaseObjects.DatabaseTypes.ExcelWorkBook(ExcelWorkBookPath).GetValue(SheetName, lookupColName, lookupVal, returnColName);
        }

        /// <returns><inheritdoc cref="ObjectSanitizing.SanitizeToBool(object)"/></returns>
        /// <inheritdoc cref="GetValue(string, string, string, string, string)"/>
        public static bool? GetValueAsBool(string ExcelWorkBookPath, string SheetName, string lookupColName, string lookupVal, string returnColName) 
            => GetValue(ExcelWorkBookPath, SheetName, lookupColName, lookupVal, returnColName).SanitizeToBool();

        /// <returns><inheritdoc cref="ObjectSanitizing.SanitizeToString(object, IFormatProvider)"/></returns>
        /// <inheritdoc cref="GetValue(string, string, string, string, string)"/>
        [System.Diagnostics.DebuggerHidden]
        public static string GetValueAsString(string ExcelWorkBookPath, string SheetName, string lookupColName, string lookupVal, string returnColName)
            => GetValue(ExcelWorkBookPath, SheetName, lookupColName, lookupVal, returnColName).ToString();

        /// <returns><inheritdoc cref="ObjectSanitizing.SanitizeToInt(object)"/></returns>
        /// <inheritdoc cref="GetValue(string, string, string, string, string)"/>
        [System.Diagnostics.DebuggerHidden]
        public static int? GetValueAsInt(string ExcelWorkBookPath, string SheetName, string lookupColName, string lookupVal, string returnColName)
            => GetValue(ExcelWorkBookPath, SheetName, lookupColName, lookupVal, returnColName).SanitizeToInt();


        ///// <summary>Create Dictionary of all from the excel table. 1st column is Key, 2nd column is Value.</summary>
        ///// <inheritdoc cref="DataTableExtensions.BuildDictionary(DataTable, out Dictionary{string, string}, string, string)"/>
        //[System.Diagnostics.DebuggerHidden]
        //public static void BuildDictionary(out Dictionary<string, string> Dict, string ExcelWorkBookPath, string SheetName, string KeyColumn, string ValueColumn) => ExcelOps.GetDataTable(ExcelWorkBookPath, SheetName).BuildDictionary(out Dict, KeyColumn, ValueColumn);

    }

}
