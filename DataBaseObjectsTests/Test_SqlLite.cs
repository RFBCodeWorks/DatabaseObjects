using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RFBCodeWorks.DataBaseObjects.Tests
{
    [TestClass()]
    public class SqlLiteTest
    {
        [TestInitialize]
        public void InitSqlLite()
        {
            DeleteDB();
        }

        [TestMethod()]
        public void RunAllTests()
        {
            var db = PrepareDatabase();
            WriteToDatabase(db);
            ReadDatabase(db);
            DeleteFromDatabase(db);
        }

        [TestCleanup()]
        public void DeleteDB()
        {
            if (System.IO.File.Exists(dbPath))
                System.IO.File.Delete(dbPath);
        }

        private MySqlLiteDB PrepareDatabase()
        {
            DeleteDB();
            var db = new MySqlLiteDB();

            var conn = db.GetDatabaseConnection();
            using (conn)
            {
                var createtbl = conn.CreateCommand();
                createtbl.CommandText = "create table Students (ID INTEGER PRIMARY KEY, FirstName varchar(20), LastName varchar(20))";
                conn.Open();
                createtbl.ExecuteNonQuery();
                conn.Close();
            }
            return db;
        }

        private void WriteToDatabase(MySqlLiteDB db)
        {
            Console.WriteLine("Performing Insertion Tests");
            Assert.AreEqual(1, db.StudentTable.Insert(StudentTable.ValueColumns, new string[] { "Billy", "Bob" })); // PKEY = 1
            Assert.AreEqual(1, db.StudentTable.Insert(StudentTable.ValueColumns, new string[] { "Janey", "Bob" })); // PKEY = 2
            Assert.AreEqual(1, db.StudentTable.Insert(StudentTable.ValueColumns, new string[] { "Bobby", "Bob" })); // PKEY = 3
            Assert.AreEqual(3, db.StudentTable.GetDataTable().Rows.Count);
            Assert.IsNotNull(db.StudentTable.GetDataRow(1));

            Console.WriteLine("Performing Upsertion Tests");
            Assert.AreEqual(1, db.StudentTable.Upsert(db.StudentTable.PrimaryKey, 1, "LastName", "Jane")); 
            Assert.AreEqual(1, db.StudentTable.Upsert(db.StudentTable.PrimaryKey, 4, "LastName", "Jane", true));
            Console.WriteLine("Insert/Upsertion Tests Completed Succesfully");
        }

        private void ReadDatabase(MySqlLiteDB db)
        {
            Console.WriteLine("Performing Read Test - Check if Upsertion updated the expected row");
            var tbl = db.StudentTable.GetDataRow(1);
            Assert.AreEqual("Billy Jane", tbl[1] + " " + tbl[2]);
            Console.WriteLine("Read Test OK");
        }

        private void DeleteFromDatabase(MySqlLiteDB db)
        {
            Console.WriteLine("Performing Deletion Test - Delete 2 of the 4 rows");
            Assert.AreEqual(2, db.StudentTable.DeleteRows("LastName", "Bob"));
            Console.WriteLine("Delete Test OK");
        }

        private const string dbPath = "C:\\TestRFBCodeWorks.sqlite";

        private class MySqlLiteDB : RFBCodeWorks.DataBaseObjects.DataBaseTypes.SqlLiteDataBase
        {
            public MySqlLiteDB() : base()
            {
                var connbuilder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder();
                connbuilder.DataSource = dbPath;
                connbuilder.DefaultTimeout = 5;
                connbuilder.Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWriteCreate;
                connbuilder.Pooling = true;
                ConnectionString = connbuilder.ConnectionString;
                StudentTable = new StudentTable(this);
            }

            public StudentTable StudentTable {get;}

        }

        private class StudentTable : RFBCodeWorks.DataBaseObjects.SimpleKeyDatabaseTable
        {
            public StudentTable(MySqlLiteDB db) : base(db, "Students", "ID")
            {

            }

            public static string[] ValueColumns { get; } = new string[]{ "FirstName", "LastName" };
        }


        
    }
}