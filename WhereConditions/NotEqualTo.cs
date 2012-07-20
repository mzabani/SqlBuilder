using System;

namespace SqlBuilder.Conditions
{
	public class NotEqualTo : SimpleComparison
	{
		public NotEqualTo(string column, object @value) : base(column, "!=", @value)
		{
		}

		public NotEqualTo(string leftSideColumnOrExpression, string rightSideColumnOrExpression) : base(leftSideColumnOrExpression, "!=", rightSideColumnOrExpression)
		{
		}
	}
}

