using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlKata.Compilers;
using System;
using System.Collections.Generic;
using System.Text;
using RFBCodeWorks.SqlKata.MsOfficeCompilers;
using RFBCodeWorks.DatabaseObjects;
using System.Linq;
using System.Threading.Tasks;
using SqlKata;

namespace AccessTests
{
    [TestClass]
    public class Test_MSAccess
    {
        [TestCleanup]
        public void DelayBetweenTests() { Task.WaitAll(Task.Delay(100)); }

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

        private AccessDB TableInit(bool delete = false)
        {
            var db = new AccessDB();
            if (delete) db.RunAction(new SqlKata.Query(db.Students.TableName).AsDelete());  // Delete all rows within the table
            return db;
        }

        [TestMethod]
        public async Task A_InsertRawTest()
        {
            //This test will always fail if run after another test. This test passes if running on its own.
            // This is due to the way this was written it enforces the file to not exist.


            System.IO.File.Delete(Test_AccessDAO.DBPath);
            var db = TableInit(true);

            //Check database table is empty
            Assert.AreEqual(0, db.Students.GetDataTable().Rows.Count);

            async Task WaitForDB() => await Task.Delay(500);

            //Raw statement - First row in table with ID that AutoIncrements will have ID of 1
            using (var cmd = db.GetCommand())
            {
                cmd.CommandText = $"INSERT INTO {db.Students.TableName} (FirstName, LastName) VALUES ('FirstTest', 'FirstTest');";
                cmd.Connection.Open();
                Assert.AreEqual(1, await cmd.ExecuteNonQueryAsync());
                cmd.Connection.Close();
            }
            await WaitForDB();
            Assert.AreEqual("FirstTest", db.Students.GetValue(1, "FirstName")?.ToString());
            Assert.AreEqual("FirstTest", db.Students.GetValue(1, "LastName")?.ToString());

            //Validate wrapping values with quotes
            using (var cmd = db.GetCommand())
            {
                cmd.CommandText = $"INSERT INTO {db.Students.TableName} (FirstName, LastName) VALUES (\"SecondTest\", \"SecondTest\");";
                cmd.Connection.Open();
                Assert.AreEqual(1, await cmd.ExecuteNonQueryAsync());
                cmd.Connection.Close();
            }
            await WaitForDB();
            Assert.AreEqual("SecondTest", db.Students.GetValue(2, "FirstName")?.ToString());
            Assert.AreEqual("SecondTest", db.Students.GetValue(2, "LastName")?.ToString());

            //Validate various wrapping of values
            using (var cmd = db.GetCommand())
            {
                cmd.CommandText = $"INSERT INTO [{db.Students.TableName}] ([FirstName], [LastName]) VALUES ('ThirdTest', \"ThirdTest\");";
                cmd.Connection.Open();
                Assert.AreEqual(1, await cmd.ExecuteNonQueryAsync());
                cmd.Connection.Close();
            }
            await WaitForDB();
            Assert.AreEqual("ThirdTest", db.Students.GetValue(3, "FirstName")?.ToString());
            Assert.AreEqual("ThirdTest", db.Students.GetValue(3, "LastName")?.ToString());

            //Equivalent Command with parameters
            //OLEDB COmmand cannot use named parameters!
            using (var cmd = db.GetCommand())
            {
                cmd.CommandText = $"INSERT INTO [{db.Students.TableName}] ([FirstName], [LastName]) VALUES (?, ?);";
                cmd.Parameters.AddWithValue(null, "FourthTest");
                cmd.Parameters.AddWithValue(null, "FourthTest");

                cmd.Connection.Open();
                Assert.AreEqual(1, await cmd.ExecuteNonQueryAsync());
                cmd.Connection.Close();
            }
            await WaitForDB();
            Assert.AreEqual("FourthTest", db.Students.GetValue(4, "FirstName").ToString());
            Assert.AreEqual("FourthTest", db.Students.GetValue(4, "LastName").ToString());
        }

        [DataRow("Bill", "Bob")]
        [DataRow("Weebles", "Wob")]
        [DataRow("Frankie", "LostHisJob")]
        [DataRow("DJ", "DobbyDob")]
        [TestMethod]
        public void SqlInsert(string first, string last)
        {
            var db = TableInit();
            var dic = new Dictionary<string, object>();
            dic.Add("FirstName", first);
            dic.Add("LastName", last);
            Assert.AreEqual(1, db.Students.Insert(dic));
        }

        [TestMethod]
        public async Task SqlSelect()
        {
            var tbl = TableInit(true).Students;
            //Attempting to retrieve a table that does not exist results in an exception
            Assert.ThrowsException<System.Data.OleDb.OleDbException>(() => tbl.Parent.GetDataTable(new SqlKata.Query("InvalidTable")));
            
            //Attempting to retrieve a table with 0 rows succeeds if the table exists.
            Assert.AreEqual(0, tbl.GetDataTable().Rows.Count);


            //Insert two rows
            SqlInsert("Bill", "Bob");
            await Task.Delay(500);
            SqlInsert("Will", "Frog");
            await Task.Delay(500);
            Assert.AreEqual(2, tbl.GetDataTable().Rows.Count);
            var iD = tbl.Parent.GetValue(tbl.Select("ID").Where("FirstName", "Will"));
            Assert.AreEqual(1, tbl.Parent.GetDataTable(tbl.Select().Where("ID", iD)).Rows.Count);
        }

        [DataRow(2)]
        [DataRow(1)]
        [TestMethod]
        public void SqlSelectTop(int recordCount)
        {
            var tbl = TableInit(false).Students;
            Assert.IsTrue(tbl.Parent.GetDataTable(tbl.Select()).Rows.Count >= 1, $"\nNot enough rows in the table to perform the test! Must have alteast 1 row(s)"); // check some amount of rows exist - expects 2 rows

            string raw = $"Select TOP {recordCount} * from {tbl.TableName}";
            Console.Write("Raw SQL test");
            Assert.AreEqual(recordCount, tbl.Parent.GetDataTable(raw).Rows.Count, $"\nFailed to retrieve TOP {recordCount} rows");
            Console.WriteLine(" -- SUCCESS");


            var qry = tbl.Select().Limit(recordCount);
            PrintQuery(qry);
            Assert.AreEqual(recordCount, tbl.Parent.GetDataTable(qry).Rows.Count, $"\nFailed to retrieve TOP {recordCount} rows");

            raw = $"Select TOP {recordCount} * from {tbl.TableName} ORDER BY [{tbl.PrimaryKey}] DESC";
            Console.Write("\n\nRaw SQL test");
            Assert.AreEqual(recordCount, tbl.Parent.GetDataTable(raw).Rows.Count, $"\nFailed to retrieve TOP {recordCount} rows");
            Console.WriteLine(" -- SUCCESS");

            qry = tbl.Select().Limit(recordCount).OrderByDesc(tbl.PrimaryKey);
            PrintQuery(qry);
            Assert.AreEqual(recordCount, tbl.Parent.GetDataTable(qry).Rows.Count, $"\nFailed to perform OrderByDescending");
        }

        private void PrintQuery(SqlKata.Query qry)
        {
            var compiler = MSAccessCompiler.AccessCompiler;
            var result = compiler.Compile(qry);
            Console.WriteLine($"Raw SQL: " + result.RawSql);
            Console.WriteLine($"Parameters: ");
            if (!result.NamedBindings.Any())
                Console.WriteLine(" - None");
            else
            {
                foreach (var bnd in result.NamedBindings)
                    Console.WriteLine(string.Format("  - {0} | {1}", bnd.Key, bnd.Value));
            }
            Console.WriteLine($"Compiled SQL: " + result.ToString());
        }

        [DataRow("FirstName", "Frankie", "Munez")]
        [DataRow("ID", "?", "Froglodyte")]
        [TestMethod]
        public void SqlUpdate(string searchCol, object searchVal, string newLastName)
        {
            var tbl = TableInit(true).Students;
            //Insert two rows
            SqlInsert("Bill", "Bob"); // ID1
            SqlInsert("Frankie", "Frog"); //ID2
            var iD = tbl.Parent.GetValue(tbl.Select("ID").Where("FirstName", "Frankie"));

            var dic = new Dictionary<string, object>();
            dic.Add("LastName", newLastName);
            var dic2 = new Dictionary<string, object>();
            dic2.Add("LastName", newLastName + "2");

            if (searchCol == "ID")
            {
                Assert.AreEqual(1, tbl.Update(iD, dic.ToArray()));
                Assert.AreEqual(1, tbl.Update(dic2, new RFBCodeWorks.SqlKata.Extensions.WhereColumnValue(searchCol, iD)), "WhereColumnValue object failed");
            }
            else
            {
                Assert.AreEqual(1, tbl.Parent.RunAction(tbl.Select().AsUpdate(dic).Where(searchCol, searchVal)), "SqlKata Query update failed");
                Assert.AreEqual(1, tbl.Update(dic2, new RFBCodeWorks.SqlKata.Extensions.WhereColumnValue(searchCol, searchVal)), "WhereColumnValue object failed");
            }
        }


        [TestMethod]
        public void SqlRemove()
        {
            var tbl = TableInit(true).Students;
            //Insert two rows
            SqlInsert("Bill", "Bob");
            SqlInsert("Will", "Frog");
            var iD = tbl.Parent.GetValue(tbl.Select("ID").Where("FirstName", "Will"));
            Assert.AreEqual(1, tbl.DeleteRows("ID", iD));
        }

        [TestMethod]
        public void GetCommandTest_1()
        {
            var db = TableInit(false);
            Assert.IsInstanceOfType<System.Data.OleDb.OleDbCommand>(db.GetCommand());
        }

        [TestMethod]
        public void GetCommandTest_2()
        {
            var db = TableInit(false);
            using (var cmd = db.GetCommand("Select * from Students Where [LastName] = @LastName AND [FirstName] = @FirstName",
                new KeyValuePair<string, object>("@LastName", "Bob"),
                new KeyValuePair<string, object>("@FirstName", "Will"))
                )
            {
                Assert.IsInstanceOfType<System.Data.OleDb.OleDbCommand>(cmd);
                cmd.Connection.Open();
                using (var rdr = cmd.ExecuteReader())
                    Assert.IsNotNull(rdr);
            }
        }

        [TestMethod]
        public void GetCommandTest_3()
        {
            var db = TableInit(false);
            var dic = new Dictionary<string, object>();
            dic.Add("@LastName", "Bob");
            dic.Add("@FirstName", "Will");
            using (var cmd = db.GetCommand("Select * from Students Where [LastName] = @LastName AND [FirstName] = @FirstName", dic))
            {
                Assert.IsInstanceOfType<System.Data.OleDb.OleDbCommand>(cmd);
                using (cmd.Connection)
                {
                    cmd.Connection.Open();
                    using (var rdr = cmd.ExecuteReader())
                        Assert.IsNotNull(rdr);
                }

            }
        }

        [TestMethod]
        public void GetCommandTest_4()
        {
            var db = TableInit(false);
            using (var cmd = db.GetCommand(db.Students.Select("FirstName").Where("LastName", "Bob").Where("FirstName", "Will")))
            {
                Assert.IsInstanceOfType<System.Data.OleDb.OleDbCommand>(cmd);
                cmd.Connection.Open();
                using (var rdr = cmd.ExecuteReader())
                    Assert.IsNotNull(rdr);
            }
        }

        [TestMethod]
        public void GitIssue3()
        {
            // Test Github Issue #3 - issue introduced on SqlKata 2.4.0, worked ok on 2.3.9 per OP.
            var compiler = new MSAccessCompiler();
            var query = new SqlKata.Query("MainTable").Where("ID", 1);
            SqlResult result = compiler.Compile(query);
        }

        private class AccessDB : RFBCodeWorks.DatabaseObjects.DatabaseTypes.MSAccessDataBase
        {
            public AccessDB() : base(Test_AccessDAO.DBPath, "")
            {
                Students = new PrimaryKeyTable(this, nameof(Students), "ID");
                if (!System.IO.File.Exists(Test_AccessDAO.DBPath))
                {
                    Test_AccessDAO.GetDatabase().Close(); // Creates the database if it does not exist
                    new Test_AccessDAO().CreateAll();
                }
            }

            public PrimaryKeyTable Students { get; }
        }
    }
}
