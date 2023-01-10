using Microsoft.VisualStudio.TestTools.UnitTesting;
using RFBCodeWorks.DataBaseObjects;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace SqlLiteTests
{
    [TestClass()]
    public class SqlLiteTest
    {
        [TestInitialize]
        public void InitSqlLite()
        {
            var path = GetDatabaseLocation();
            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        
        [TestMethod()]
        public void ReadWriteDelete()
        {
            var db = PrepareDatabase();
            WriteToDatabase(db);
            Update(db);
            Upsert(db);
            DeleteFromDatabase(db);
        }

        

        private MySqlLiteDB PrepareDatabase()
        {
            var db = new MySqlLiteDB();

            var conn = db.GetConnection();
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
            Assert.AreEqual(1, db.StudentTable.Insert(StudentTable.ValueColumns, new string[] { "Joey", "Bob" })); // PKEY = 4
            Assert.AreEqual(1, db.StudentTable.Insert(StudentTable.ValueColumns, new string[] { "Bruce", "Banner" })); // PKEY = 5
            Assert.AreEqual(5, db.StudentTable.GetDataTable().Rows.Count);
            Assert.AreEqual("Bruce", db.StudentTable.GetValue(5, "FirstName"));
            Assert.ThrowsException<Microsoft.Data.Sqlite.SqliteException>(() => db.GetValue(new SqlKata.Query("MissingTable")), "\n\n Invalid Table Name in query does not throw exception");
            Assert.ThrowsException<Microsoft.Data.Sqlite.SqliteException>(() => db.StudentTable.GetValue(1, "MissingColumnTest"), "\n\n Invalid Column Name in query does not throw exception");
        }

        private void Update(MySqlLiteDB db)
        {
            Console.WriteLine("Performing Update Tests");
            Dictionary<string, string> cols = new Dictionary<string, string>
            {
                { "FirstName", "Update" },
                { "LastName", "Successful" }
            };
            Assert.AreEqual(1, db.StudentTable.Update(cols.Keys, cols.Values, new RFBCodeWorks.SqlKata.Extensions.WhereNumericValue<int>("ID", 3, RFBCodeWorks.SqlKata.Extensions.NumericOperators.EqualTo)));
            var row = db.StudentTable.GetDataRow(3);
            Assert.AreEqual("Update Successful", row[1] + " " + row[2]);
            Console.WriteLine("Update Tests Completed Succesfully");
        }

        private void Upsert(MySqlLiteDB db)
        {
            Console.WriteLine("Performing Upsertion Tests");
            Assert.AreEqual(1, DBOps.Upsert(db.StudentTable, 1, "FirstName", "Upsertion"));
            Assert.AreEqual(1, DBOps.Upsert(db.StudentTable, 1, "LastName", "OK"));
            var row = db.StudentTable.GetDataRow(1);
            Assert.AreEqual("Upsertion OK", row[1] + " " + row[2]);

            Assert.AreEqual(1, DBOps.Upsert(db.StudentTable,6, new string[] { "FirstName", "LastName" }, new string[] { "Upsertion", "OK2" }));
            row = db.StudentTable.GetDataRow(6);
            Assert.AreEqual("Upsertion OK2", row[1] + " " + row[2]);
            Console.WriteLine("Upsertion Tests Completed Succesfully");
        }

        private void DeleteFromDatabase(MySqlLiteDB db)
        {
            Console.WriteLine("Performing Deletion Test - Delete 2 of the 4 rows");
            Assert.AreEqual(2, db.StudentTable.DeleteRows("LastName", "Bob"));
            Console.WriteLine("Delete Test OK");
        }

        private class MySqlLiteDB : RFBCodeWorks.DataBaseObjects.DataBaseTypes.SqliteDataBase
        {
            public MySqlLiteDB() : base()
            {
                var connbuilder = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
                {
                    DataSource = GetDatabaseLocation(),
                    DefaultTimeout = 5,
                    Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWriteCreate,
                    Pooling = true
                };
                ConnectionString = connbuilder.ConnectionString;
                StudentTable = new StudentTable(this);
            }

            public StudentTable StudentTable {get;}


        }

        private static string GetDatabaseLocation() => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestDatabase.sqlite");

        private class StudentTable : RFBCodeWorks.DataBaseObjects.PrimaryKeyTable
        {
            public StudentTable(MySqlLiteDB db) : base(db, "Students", "ID")
            {

            }

            public static string[] ValueColumns { get; } = new string[]{ "FirstName", "LastName" };
        }


        
    }
}