using NUnit.Framework;
using SqlKata;
using SqlKata.Compilers;
using System;
using System.Text.RegularExpressions;

namespace UnitTest_SqlKataPlayground
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void WhereRaw()
        {
            var comp = new SqlServerCompiler();

            Console.WriteLine("WhereRaw statements use '?' as the indicator for the next parameter!");
            Console.WriteLine("WhereRaw statements and parameters need to be properly wrapped to be considered correct");

            Console.WriteLine("\nThis is how the statement should look (compiled without WhereRaw)");
            Console.WriteLine(comp.Compile(new Query("MyTable").Where("IdColumn", 0)));


            Console.WriteLine("\n-----------------------------------------------------------------");
            Console.WriteLine("The following is a properly written WhereRaw statement:");
            
            var raw = "[IdColumn] = ?";
            Console.WriteLine($"\nWhereRaw Statement: \"{raw}\"");
            Console.WriteLine("Parameters: 0");
            Console.WriteLine(comp.Compile(new Query("MyTable").WhereRaw(raw, 0)));

            Console.WriteLine("\n-----------------------------------------------------------------");
            Console.WriteLine("The following statements shows what will likely be interpreted as an invalid sql statement by the database, due to not properly wrapping the IdColumn in the WHERE clause.");

            raw = "IdColumn = ?";
            Console.WriteLine($"\nWhereRaw Statement: \"{raw}\" ");
            Console.WriteLine("Parameters: 0");
            Console.WriteLine(comp.Compile(new Query("MyTable").WhereRaw(raw, 0)));

            raw = "? = ?";
            Console.WriteLine($"\nWhereRaw Statement: \"{raw}\" ");
            Console.WriteLine("Parameters: \"IdColumn\" | 0");
            Console.WriteLine(comp.Compile(new Query("MyTable").WhereRaw(raw, "IdColumn", 0)));
            
            raw = "? = ?";
            Console.WriteLine($"\nWhereRaw Statement: \"{raw}\" ");
            Console.WriteLine("Parameters: \"[IdColumn]\" | 0");
            Console.WriteLine(comp.Compile(new Query("MyTable").WhereRaw(raw, "[IdColumn]", 0)));
            Console.WriteLine("\n-----------------------------------------------------------------");

        }



        [Test]
        public void WhereVsOrWhere()
        {
            //Test that Where and OrWhere as the first statement makes no difference to the final query
            Console.WriteLine("This test confirms that using 'Where()' and 'OrWhere()' in SqlKata queries makes no difference during final compiling");


            var comp = new SqlServerCompiler();
            var tbl = "MyTable";
            var col = "ID";
            var st = "SearchTerm";
            var col2 = "C2";
            var st2 = 5;
            
            var q1 = new Query(tbl).Where(col, st).WhereNot(col2, st2);
            var q2 = new Query(tbl).OrWhere(col, st).WhereNot(col2, st2);
            Assert.AreEqual(comp.Compile(q1).ToString(), comp.Compile(q2).ToString());

            q1 = new Query(tbl).WhereNot(col, st).WhereNot(col2, st2);
            q2 = new Query(tbl).OrWhereNot(col, st).WhereNot(col2, st2);
            Assert.AreEqual(comp.Compile(q1).ToString(), comp.Compile(q2).ToString());
        }
    }
}