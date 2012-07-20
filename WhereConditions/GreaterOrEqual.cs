using System;

namespace SqlBuilder.Conditions
{
	public class GreaterOrEqual : SimpleComparison
	{
		public GreaterOrEqual(string column, object @value) : base(column, ">=", @value)
		{
		}

		public GreaterOrEqual(string leftSideColumnOrExpression, string rightSideColumnOrExpression) : base(leftSideColumnOrExpression, ">=", rightSideColumnOrExpression)
		{
		}
	}
}

