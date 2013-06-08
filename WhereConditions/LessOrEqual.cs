using System;
using System.Linq.Expressions;

namespace SqlBuilder.Conditions
{
	public class LessOrEqual : SimpleComparison
	{
		public LessOrEqual(string column, object @value) : base(column, "<=", @value)
		{
		}

		public LessOrEqual(SqlFragment leftSideColumnOrExpression, object @value) : base(leftSideColumnOrExpression, "<=", value)
		{
		}

		public LessOrEqual(SqlFragment leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) : base(leftSideColumnOrExpression, "<=", rightSideColumnOrExpression)
		{
		}

		public LessOrEqual(string leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) : base(leftSideColumnOrExpression, "<=", rightSideColumnOrExpression)
		{
		}
	}

	public class LessOrEqual<T> : LessOrEqual
	{
		public LessOrEqual(Expression<Func<T, object>> lambdaGetter, object @value)
			: base(ExpressionTreeHelper.GetPropOrFieldNameFromLambdaExpr(lambdaGetter), @value)
		{
		}
	}
}
