using System;

namespace SqlBuilder.Conditions
{
	public class EqualTo : SimpleComparison
	{
		public EqualTo(string columnOrExpression, object @value) : base(columnOrExpression, "=", @value)
		{
		}

		public EqualTo(string leftSideColumnOrExpression, string rightSideColumnOrExpression) : base(leftSideColumnOrExpression, "=", rightSideColumnOrExpression)
		{
		}
	}
}

