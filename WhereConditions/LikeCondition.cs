using System;
using System.Linq.Expressions;

namespace SqlBuilder.Conditions
{
	public class LikeCondition : SimpleComparison
	{
		public LikeCondition(string columnOrExpression, object @value) : base(columnOrExpression, "LIKE", @value)
		{
		}

		public LikeCondition(SqlFragment leftSideColumnOrExpression, object @value) : base(leftSideColumnOrExpression, "LIKE", @value)
		{
		}

		public LikeCondition(SqlFragment leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) : base(leftSideColumnOrExpression, "LIKE", rightSideColumnOrExpression)
		{
		}

		public LikeCondition(string leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) : base(leftSideColumnOrExpression, "LIKE", rightSideColumnOrExpression)
		{
		}
	}

	public class LikeCondition<T> : LikeCondition
	{
		public LikeCondition(Expression<Func<T, object>> lambdaGetter, object @value)
			: base(ExpressionTreeHelper.GetPropOrFieldNameFromLambdaExpr(lambdaGetter), value)
		{
		}
	}
}
