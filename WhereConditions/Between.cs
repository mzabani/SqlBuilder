using System;
using System.Linq.Expressions;
using SqlBuilder;

namespace SqlBuilder.Conditions
{
	public class Between : CompositeComparison
	{
		public Between(string columnOrExpression, object min_val, object max_val)
			: base(columnOrExpression, "BETWEEN", min_val, "AND", max_val)
		{
		}
		
		public Between(SqlFragment frag, object min_val, object max_val)
			: base(frag, "BETWEEN", min_val, "AND", max_val)
		{
		}
	}


	public class Between<T> : Between
	{
		public Between(Expression<Func<T, object>> lambdaGetter, object min_val, object max_val)
			: base(ExpressionTreeParser.GetPropOrFieldNameFromLambdaExpr(lambdaGetter), min_val, max_val)
		{
		}
	}
}
