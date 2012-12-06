using System;
using System.Linq.Expressions;

namespace SqlBuilder.Conditions
{
	public class LessOrEqual : SimpleComparison
	{
		public LessOrEqual(string column, object @value) : base(column, "<=", @value)
		{
		}

		public LessOrEqual(string leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) : base(leftSideColumnOrExpression, "<=", rightSideColumnOrExpression)
		{
		}
	}

	public class LessOrEqual<T> : LessOrEqual
	{
		public LessOrEqual(Expression<Func<T, object>> lambdaGetter, object @value)
			: base(ExpressionTreeParser.GetPropOrFieldNameFromLambdaExpr(lambdaGetter), @value)
		{
		}
	}
}
