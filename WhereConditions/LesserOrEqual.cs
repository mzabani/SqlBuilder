using System;

namespace SqlBuilder.Conditions
{
	public class LesserOrEqual : SimpleComparison
	{
		public LesserOrEqual(string column, object @value) : base(column, "<=", @value)
		{
		}
	}
}

