using System;
using System.Linq.Expressions;

namespace SqlBuilder.Conditions
{
	public class NotEqualTo : SimpleComparison
	{
		public NotEqualTo(string column, object @value) : base(column, "!=", @value)
		{
		}

		public NotEqualTo(string leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) : base(leftSideColumnOrExpression, "!=", rightSideColumnOrExpression)
		{
		}
	}

	public class NotEqualTo<T> : NotEqualTo
	{
		public NotEqualTo(Expression<Func<T, object>> lambdaGetter, object @value)
			: base(ExpressionTreeParser.GetPropOrFieldNameFromLambdaExpr(lambdaGetter), @value)
		{
		}
	}
}
