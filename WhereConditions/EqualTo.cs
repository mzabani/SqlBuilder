using System;

namespace SqlBuilder.Conditions
{
	public class EqualTo : SimpleComparison
	{
		public EqualTo(string column, object @value) : base(column, "=", @value)
		{
		}
	}
}

