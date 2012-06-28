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
		
		public SimpleComparison(string column, string @operator, object @value) : this(new SqlFragment(column), @operator, @value)
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
