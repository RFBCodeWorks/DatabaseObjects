using RFBCodeWorks.DataBaseObjects.Exceptions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Text;

namespace RFBCodeWorks.DataBaseObjects
{
    /// <summary>
    /// Functions specific to Excel Workbooks
    /// </summary>
    /// *  Connection Strings: https://www.connectionstrings.com/excel/
    public class ExcelOps
    {
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

        /// <inheritdoc cref="ConnectionStringBuilders.ExcelWorkbooks.GetConnection(string, bool?)"/>
        public static OleDbConnection GetConnection(string workbookPath, bool? hasHeaders = null) 
            => ConnectionStringBuilders.ExcelWorkbooks.GetConnection(workbookPath, hasHeaders);

        /// <summary>
        /// Opens an <see cref="OleDbCommand"/> to the specified workbook and retrieves a <see cref="DataTable"/> representation of the <paramref name="SheetName"/>
        /// </summary>
        /// <param name="ExcelWorkBookPath">path to the workbook</param>
        /// <param name="SheetName">name of the sheet</param>
        /// <param name="hasHeaders">treat the first row as table headers</param>
        /// <returns>a <see cref="DataTable"/> that represents the specified <paramref name="SheetName"/></returns>
        // Getting a table from an excel workbook:  https://stackoverflow.com/questions/33994160/read-excel-table-data-using-c-sharp
        public static DataTable GetDataTable(string ExcelWorkBookPath, string SheetName, bool hasHeaders = true)
        {
            // Get Connection to workbook
            DataTable DT = new DataTable();

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
                    if (Tbl.Substring(Tbl.Length - 1) != "$") Tbl = Tbl + "$";
                    foreach (DataRow Row in schemaTable.Rows)
                    {
                        TblName = Row["TABLE_NAME"].ToString();
                        if (TblName.ToUpper() == Tbl.ToUpper()) { tblFound = true; break; }
                    }
                    if (tblFound == false) throw new ExcelTableNotFoundException("Unable to find ' " + SheetName + " ' in workbook: " + ExcelWorkBookPath);

                    string query = $"SELECT * FROM [{TblName}]";
                    OleDbDataAdapter daexcel = new OleDbDataAdapter(query, conn);
                    DT.Locale = System.Globalization.CultureInfo.CurrentCulture; // Don't need I think...
                    daexcel.Fill(DT);
                    conn.Close();
                }
            }
            catch (Exception E)
            {
                E.AddVariableData(nameof(ExcelWorkBookPath), ExcelWorkBookPath);
                E.AddVariableData(nameof(SheetName), SheetName);
                throw E;
            }
            return DT;
        }

        /// <param name="ExcelWorkBookPath">Excel Workbook to look up</param>
        /// <param name="SheetName">Sheet Name to grab from the excel workbook</param>
        /// <inheritdoc cref="DataTableExtensions.GetValueAsBool(DataTable, int, string, int)"/>
        /// <param name="LookupVal"/><param name="LookupCol"/><param name="ReturnCol"/>
        public static bool GetValueAsBool(string ExcelWorkBookPath, string SheetName, string LookupCol, string LookupVal, string ReturnCol) => ExcelOps.GetDataTable(ExcelWorkBookPath, SheetName).GetValueAsBool(LookupCol, LookupVal, ReturnCol);


        /// <inheritdoc cref="DataTableExtensions.GetValueAsString(DataTable, int, string, int)"/>
        /// <inheritdoc cref="GetValueAsBool(string, string, string, string, string)"/>
        [System.Diagnostics.DebuggerHidden]
        public static string GetValueAsString(string ExcelWorkBookPath, string SheetName, string LookupCol, string LookupVal, string ReturnCol) => ExcelOps.GetDataTable(ExcelWorkBookPath, SheetName).GetValueAsString(LookupCol, LookupVal, ReturnCol);

        /// <inheritdoc cref="DataTableExtensions.GetValueAsInt(DataTable, int, string, int)"/>
        /// <inheritdoc cref="GetValueAsBool(string, string, string, string, string)"/>
        [System.Diagnostics.DebuggerHidden]
        public static int GetValueAsInt(string ExcelWorkBookPath, string SheetName, string LookupCol, string LookupVal, string ReturnCol) => ExcelOps.GetDataTable(ExcelWorkBookPath, SheetName).GetValueAsInt(LookupCol, LookupVal, ReturnCol);


        /// <summary>Create Dictionary of all from the excel table. 1st column is Key, 2nd column is Value.</summary>
        /// <inheritdoc cref="BuildDictionary(out Dictionary{string, string}, DataTable, int, int)"/>
        [System.Diagnostics.DebuggerHidden]
        public static void BuildDictionary(out Dictionary<string, string> Dict, string ExcelWorkBookPath, string SheetName, string KeyColumn, string ValueColumn) => ExcelOps.GetDataTable(ExcelWorkBookPath, SheetName).BuildDictionary(out Dict, KeyColumn, ValueColumn);

    }

}
