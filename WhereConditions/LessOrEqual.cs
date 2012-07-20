using System;

namespace SqlBuilder.Conditions
{
	public class LessOrEqual : SimpleComparison
	{
		public LessOrEqual(string column, object @value) : base(column, "<=", @value)
		{
		}

		public LessOrEqual(string leftSideColumnOrExpression, string rightSideColumnOrExpression) : base(leftSideColumnOrExpression, "<=", rightSideColumnOrExpression)
		{
		}
	}
}
