using System;
using SqlBuilder;
using NUnit.Framework;

namespace Tests
{
	[TestFixture()]
	public class Test
	{


		[Test()]
		public void TestSimpleEquality()
		{
			var t = new WhereConditionGeneratorTreeVisitor<SampleType>("test_table");
			t.Visit(x => x.prop1 == "abc");

			Assert.AreEqual("test_table.prop1=:p0", t.Fragment.ToSqlString());
		}
	}
}
			    