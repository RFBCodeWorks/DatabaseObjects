using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Text;
using RFBCodeWorks.SqlKata.MsOfficeCompilers;

namespace AccessTests
{
    [TestClass]
    public class Test_Access
    {
        // First condition is the is the ANSI-89 which needs to convert to ANSI-92, second condition is the equivalent ANSI-92 search term
        [DataRow("Search*Term[*]", "Search%Term*")]
        [DataRow("Search*Term%", "Search%Term[%]")]
        [DataRow("*#*", "%[0-9]%")]
        [DataRow("H??p", "H__p")]
        [DataRow("H?[?]p", "H_?p")]
        [DataRow("[!3-9]", "[^3-9]")]
        [TestMethod]
        public void SanitzeAnsi89ToAnsi92(string value, string expected)
        {
            Assert.AreEqual(expected, MSAccessCompiler.SanitizeWildCards(value));
        }
    }
}
