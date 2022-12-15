using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Text;
using RFBCodeWorks.SqlKataCompilers;

namespace SqlKata.Compilers.Tests
{
    [TestClass()]
    public class ExcelWorkbookCompilerTests
    {
        static ExcelWorkbookCompiler Compiler => RFBCodeWorks.SqlKataCompilers.ExcelWorkbookCompiler.ExcelCompiler;

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
    }
}