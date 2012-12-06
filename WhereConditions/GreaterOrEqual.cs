using System;
using System.Linq.Expressions;

namespace SqlBuilder.Conditions
{
	public class GreaterOrEqual : SimpleComparison
	{
		public GreaterOrEqual(string column, object @value) : base(column, ">=", @value)
		{
		}

		public GreaterOrEqual(string leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) : base(leftSideColumnOrExpression, ">=", rightSideColumnOrExpression)
		{
		}
	}

	public class GreaterOrEqual<T> : GreaterOrEqual
	{
		public GreaterOrEqual(Expression<Func<T, object>> lambdaGetter, object @value)
			: base(ExpressionTreeParser.GetPropOrFieldNameFromLambdaExpr(lambdaGetter), @value)
		{
		}
	}
}

