using System;
using SqlBuilder;
using System.Linq.Expressions;

namespace SqlBuilder.Conditions
{
	public class SimpleComparison : WhereCondition
	{
		public SimpleComparison(SqlFragment columnOrExpression, string @operator, object @value) {
			this.AppendFragment(columnOrExpression)
				.AppendText(" " + @operator + " ")
				.AppendParameter(@value);
		}

		public SimpleComparison(SqlFragment leftSideColumnOrExpression, string @operator, SqlFragment rightSideColumnOrExpression) {
			this.AppendFragment(leftSideColumnOrExpression)
				.AppendText(" " + @operator + " ")
				.AppendFragment(rightSideColumnOrExpression);
		}

		public SimpleComparison(string leftSideColumnOrExpression, string @operator, SqlFragment rightSideColumnOrExpression)
			: this(new SqlFragment(leftSideColumnOrExpression), @operator, rightSideColumnOrExpression)
		{
		}
		
		public SimpleComparison(string columnOrExpression, string @operator, object @value)
			: this(new SqlFragment(columnOrExpression), @operator, @value)
		{
		}
	}


	public class SimpleComparison<T> : SimpleComparison
	{
		public SimpleComparison(Expression<Func<T, object>> lambdaGetter, string @operator, object @value)
			: base(new SqlFragment(ExpressionTreeHelper.GetPropOrFieldNameFromLambdaExpr<T>(lambdaGetter)),
			       @operator,
			       @value)
		{
		}
	}
}
