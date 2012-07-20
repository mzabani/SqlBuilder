using System;
using SqlBuilder;
using System.Linq.Expressions;

namespace SqlBuilder.Conditions
{
	public class SimpleComparison : WhereCondition
	{
		public SimpleComparison(SqlFragment columnOrExpression, string @operator, object @value) {
			SqlFragment frag = new SqlFragment();
			frag.AppendFragment(columnOrExpression)
				.AppendText(@operator)
				.AppendParameter(@value);
			
			this.SetSqlFragment(frag);
		}

		public SimpleComparison(SqlFragment leftSideColumnOrExpression, string @operator, SqlFragment rightSideColumnOrExpression) {
			SqlFragment frag = new SqlFragment();
			frag.AppendFragment(leftSideColumnOrExpression)
				.AppendText(@operator)
				.AppendFragment(rightSideColumnOrExpression);
			
			this.SetSqlFragment(frag);
		}
		
		public SimpleComparison(string columnOrExpression, string @operator, object @value) : this(new SqlFragment(columnOrExpression), @operator, @value)
		{
		}

		public SimpleComparison(string leftSideColumnOrExpression, string @operator, string rightSideColumnOrExpression) : this(new SqlFragment(leftSideColumnOrExpression), @operator, new SqlFragment(rightSideColumnOrExpression))
		{
		}
	}


	public class SimpleComparison<T> : SimpleComparison
	{
		public SimpleComparison(Expression<Func<T, object>> lambdaGetter, string @operator, object @value)
			: base(new SqlFragment(ExpressionTreeParser.GetPropOrFieldNameFromLambdaExpr<T>(lambdaGetter)),
			       @operator,
			       @value)
		{
		}
	}
}
