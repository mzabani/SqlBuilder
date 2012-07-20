using System;

namespace SqlBuilder.Conditions
{
	public class LessThan : SimpleComparison
	{
		public LessThan(string columnOrExpression, object @value) : base(columnOrExpression, "<", @value)
		{
		}

		public LessThan(string leftSideColumnOrExpression, string rightSideColumnOrExpression) : base(leftSideColumnOrExpression, "<", rightSideColumnOrExpression)
		{
		}
	}
}

