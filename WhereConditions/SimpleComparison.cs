using System;
using SqlBuilder;

namespace SqlBuilder.Conditions
{
	public class SimpleComparison : WhereCondition
	{
		public SimpleComparison(string column, string @operator, object @value)
		{
			SqlFragment frag = new SqlFragment();
			frag.AppendText(column + " " + @operator + " ")
				.AppendParameter(@value);
			
			this.SetSqlFragment(frag);
		}
	}
}
