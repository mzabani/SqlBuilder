using System;
using System.Linq.Expressions;

namespace SqlBuilder.Conditions
{
	public class GreaterThan : SimpleComparison
	{
		public GreaterThan(string column, object @value) : base(column, ">", @value)
		{
		}

		public GreaterThan(string leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) : base(leftSideColumnOrExpression, ">", rightSideColumnOrExpression)
		{
		}
	}

	public class GreaterThan<T> : GreaterThan
	{
		public GreaterThan(Expression<Func<T, object>> lambdaGetter, object @value)
			: base(ExpressionTreeHelper.GetPropOrFieldNameFromLambdaExpr(lambdaGetter), @value)
		{
		}
	}
}

