using System;
using System.Linq.Expressions;

namespace SqlBuilder.Conditions
{
	public class LessThan : SimpleComparison
	{
		public LessThan(string columnOrExpression, object @value) : base(columnOrExpression, "<", @value)
		{
		}

		public LessThan(SqlFragment leftSideFragment, object @value) : base(leftSideFragment, "<", @value)
		{
		}

		public LessThan(string leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) : base(leftSideColumnOrExpression, "<", rightSideColumnOrExpression)
		{
		}
	}

	public class LessThan<T> : LessThan
	{
		public LessThan(Expression<Func<T, object>> lambdaGetter, object @value)
			: base(ExpressionTreeParser.GetPropOrFieldNameFromLambdaExpr(lambdaGetter), @value)
		{
		}
	}
}
