using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Text;
using RFBCodeWorks.MsAccessDao;
using System.IO;
using RDao = RFBCodeWorks.MsAccessDao;

namespace AccessTests
{
    [TestClass]
    public class Test_AccessDAO
    {
        public static string DBPath { get; } = Path.Combine(
            Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
            "TestDB.mdb");

        private static string Locale => Microsoft.Office.Interop.Access.Dao.LanguageConstants.dbLangGeneral;
        private static Microsoft.Office.Interop.Access.Dao.DatabaseTypeEnum dbVer = Microsoft.Office.Interop.Access.Dao.DatabaseTypeEnum.dbVersion120;
        
        public static Microsoft.Office.Interop.Access.Dao.Database GetDatabase()
        {
            if (File.Exists(DBPath))
                return Management.OpenDatabase(DBPath);
            else
                return Management.CreateDatabase(DBPath, Locale, dbVer);
        }

        [TestMethod]
        public void CreateAll()
        {
            var db = GetDatabase();
            string tblName = "Students";
            var tbl = db.SelectTableDef(tblName);
            if (tbl != null) db.TableDefs.Delete(tblName);
            //Create the table
            tbl = RDao.TableCreation.CreateTableDef(db, tblName);
            tbl.CreateAutoIncrementField();
            RDao.TableCreation.CreateField(tbl, "FirstName", TableCreation.GetDataTypeEnum<string>()); //Explicit call
            tbl.CreateField("LastName", dataType: typeof(string)); // as Extension
            db.TableDefs.Append(tbl);
            Assert.IsNotNull(db.SelectTableDef(tblName));
            db.Close();
        }

        [TestMethod]
        public void CompactDB()
        {
            GetDatabase().Close();
            Assert.IsTrue(File.Exists(DBPath));
            var newPath = DBPath.Replace(Path.GetFileName(DBPath), "NewFile.mdb");
            Management.CompactDB(DBPath, newPath, dbVer);
            Assert.IsTrue(File.Exists(newPath));
            File.Delete(DBPath);
            File.Delete(newPath);
        }

        [TestMethod]
        public void CompactAndReplace()
        {
            GetDatabase().Close();
            Assert.IsTrue(File.Exists(DBPath));
            var newPath = DBPath.Replace(Path.GetFileName(DBPath), "NewFile.mdb");
            Management.CompactAndReplace(DBPath, dbVer);
            Assert.IsTrue(File.Exists(DBPath));
            File.Delete(DBPath);
        }

    }
}
