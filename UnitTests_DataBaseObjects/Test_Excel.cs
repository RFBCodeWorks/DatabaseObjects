using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Text;
using RFBCodeWorks.SqlKata.MsOfficeCompilers;
using RFBCodeWorks.SqlKata.Extensions;
using Excel = RFBCodeWorks.DataBaseObjects.DataBaseTypes.ExcelWorkBook;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using SqlKata;

namespace ExcelTests
{
    [TestClass()]
    public class Test_Excel
    {
        static ExcelWorkbookCompiler Compiler => RFBCodeWorks.SqlKata.MsOfficeCompilers.ExcelWorkbookCompiler.ExcelCompiler;

        [TestMethod()]
        public void CompileTableExpressionTest()
        {
            string exp = "SELECT * FROM [SheetName$]";
            Query qry = new Query("SheetName").Select();
            Assert.AreEqual(exp, Compiler.Compile(qry).ToString());

            qry = new Query("SheetName$").Select();
            Assert.AreEqual(exp, Compiler.Compile(qry).ToString());

            exp = "SELECT * FROM [[SheetName$]]]";
            Assert.AreEqual(exp, Compiler.Compile(new Query("[SheetName$]")).ToString());
            Assert.AreEqual(exp, Compiler.Compile(new Query("[SheetName]")).ToString());
        }

        [TestMethod()]
        public void CompileWhereStatementTest()
        {
            string exp = "SELECT * FROM [SheetName$]";
            Query qry = new Query("SheetName").Select();
            Assert.AreEqual(exp, Compiler.Compile(qry).ToString());

            qry = new Query("SheetName$").Select();
            Assert.AreEqual(exp, Compiler.Compile(qry).ToString());

            exp = "SELECT * FROM [[SheetName$]]]";
            Assert.AreEqual(exp, Compiler.Compile(new Query("[SheetName$]")).ToString());
            Assert.AreEqual(exp, Compiler.Compile(new Query("[SheetName]")).ToString());
        }


        [TestMethod()]
        public void TestWorkbookConnection()
        {
            //Test out the Select Statement Builder Here:
            var wk = new TestWorkbook();
            Assert.IsTrue(wk.TestDatabaseConnection());
        }

        [TestMethod()]
        public void TestWorkbookRead()
        {
            var wk = new TestWorkbook();


            Assert.IsNotNull(wk.DataSheet.GetValue(1, "ID"));
            Assert.IsNotNull(wk.DataSheet.GetValue("City", "Melbourne", "ID"));



            ////var qry = RFBCodeWorks.SqlKataExtensions.Extensions.Where(wk.DataSheet.Select(), new WhereValueBetween<int>("ID", 2, 5)); //Should result in 3 rows
            //var tbl = wk.GetDataTable(wk.DataSheet.Select().WhereBetween("ID", 2,5));
            //Assert.AreEqual(3, tbl.Rows.Count);
        }

       [TestMethod()]
        public async Task TestColumnMissing()
        {
            var wk = new TestWorkbook();
            void func() => wk.DataSheet.GetValue(1, "InvalidColumnName");
            Assert.ThrowsException<System.Data.OleDb.OleDbException>(func);
        }

        [TestMethod()]
        public async Task TestWorksheetMissing()
        {
            var wk = new TestWorkbook();
            Assert.IsNotNull(wk.DataSheet.GetDataTable());
            Console.WriteLine("Successfully retrieved data from the existing worksheet.");
            Console.WriteLine("Attempting to retrieve data from a worksheet that is missing");
            var incorrectExTxt = "\n\n !!------------- Incorrect Exception Type thrown -------------!!\n\n";

            //DataTable
            Action func = () => wk.MissingSheet.GetDataTable();
            Assert.ThrowsException<RFBCodeWorks.DataBaseObjects.ExcelTableNotFoundException>(func, incorrectExTxt);
            func = () => wk.MissingSheet.GetDataTable(new string[] { }, whereRaw: "[ID] = ?", 1 );
            Assert.ThrowsException<RFBCodeWorks.DataBaseObjects.ExcelTableNotFoundException>(func, incorrectExTxt);
            func = () => wk.GetDataTable($"Select * FROM [{wk.MissingSheet.TableName}$]");
            Assert.ThrowsException<RFBCodeWorks.DataBaseObjects.ExcelTableNotFoundException>(func, incorrectExTxt);

            //Async DataTable
            Func<Task> taskFunc = () => wk.MissingSheet.GetDataTableAsync();
            await Assert.ThrowsExceptionAsync<RFBCodeWorks.DataBaseObjects.ExcelTableNotFoundException>(taskFunc, incorrectExTxt);
            taskFunc = () => wk.MissingSheet.GetDataTableAsync(new string[] { }, whereRaw: "[ID] = ?", new object[] { 1 });
            await Assert.ThrowsExceptionAsync<RFBCodeWorks.DataBaseObjects.ExcelTableNotFoundException>(taskFunc, incorrectExTxt);
            taskFunc = () => wk.GetDataTableAsync($"Select * FROM [{wk.MissingSheet.TableName}$]");
            await Assert.ThrowsExceptionAsync<RFBCodeWorks.DataBaseObjects.ExcelTableNotFoundException>(taskFunc, incorrectExTxt);

            //GetValue
            func = () => wk.MissingSheet.GetValue("ID", 1, "ID");
            Assert.ThrowsException<RFBCodeWorks.DataBaseObjects.ExcelTableNotFoundException>(func, incorrectExTxt);
            func = () => wk.GetValue(wk.MissingSheet.TableName, "ID", 1, "ID");
            Assert.ThrowsException<RFBCodeWorks.DataBaseObjects.ExcelTableNotFoundException>(func, incorrectExTxt);
            
            //GetValueAsync
            taskFunc = () => wk.MissingSheet.GetValueAsync("ID", 1, "ID");
            await Assert.ThrowsExceptionAsync<RFBCodeWorks.DataBaseObjects.ExcelTableNotFoundException>(taskFunc, incorrectExTxt);
            taskFunc = () => wk.GetValueAsync(wk.MissingSheet.TableName,"ID", 1, "ID");
            await Assert.ThrowsExceptionAsync<RFBCodeWorks.DataBaseObjects.ExcelTableNotFoundException>(taskFunc, incorrectExTxt);

            //GetDataRow
            func = () => wk.GetDataRow(wk.MissingSheet.SelectWhere(new string[] { }, "ID", 1));
            Assert.ThrowsException<RFBCodeWorks.DataBaseObjects.ExcelTableNotFoundException>(func, incorrectExTxt);

            //GetDataRowAsync
            taskFunc = () => wk.GetDataRowAsync(wk.MissingSheet.SelectWhere(new string[] { }, "ID", 1));
            await Assert.ThrowsExceptionAsync<RFBCodeWorks.DataBaseObjects.ExcelTableNotFoundException>(taskFunc, incorrectExTxt);

            Console.WriteLine("All Tests threw 'ExcelTableNotFound' as expected");
        }


        private class TestWorkbook : RFBCodeWorks.DataBaseObjects.DataBaseTypes.ExcelWorkBook
        {
            public TestWorkbook() : base(GetWorkbookLocation())
            {
                DataSheet = new SimpleKeyWorkSheet(this, "DataSheet", "ID", true);
                MissingSheet = new WorkSheet(this, "ThisSheetDoesNotExist");
            }

            private static string GetWorkbookLocation() => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestWorkbook.xlsx");

            public Excel.SimpleKeyWorkSheet DataSheet { get; }

            public Excel.WorkSheet MissingSheet { get; }
        }
    }
}