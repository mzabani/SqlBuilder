using System;
using System.Linq.Expressions;

namespace SqlBuilder.Conditions
{
	public class EqualTo : SimpleComparison
	{
		public EqualTo(string columnOrExpression, object @value) : base(columnOrExpression, "=", @value)
		{
		}

		public EqualTo(SqlFragment leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) : base(leftSideColumnOrExpression, "=", rightSideColumnOrExpression)
		{
		}

		public EqualTo(string leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) : base(leftSideColumnOrExpression, "=", rightSideColumnOrExpression)
		{
		}
	}

	public class EqualTo<T> : EqualTo
	{
		public EqualTo(Expression<Func<T, object>> lambdaGetter, object @value)
			: base(ExpressionTreeParser.GetPropOrFieldNameFromLambdaExpr(lambdaGetter), value)
		{
		}
	}
}
