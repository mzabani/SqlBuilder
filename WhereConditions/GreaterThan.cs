using System;

namespace SqlBuilder.Conditions
{
	public class GreaterThan : SimpleComparison
	{
		public GreaterThan(string column, object @value) : base(column, ">", @value)
		{
		}

		public GreaterThan(string leftSideColumnOrExpression, string rightSideColumnOrExpression) : base(leftSideColumnOrExpression, ">", rightSideColumnOrExpression)
		{
		}
	}
}

