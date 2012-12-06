using System;
using SqlBuilder;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;

namespace SqlBuilder.Conditions
{
	public class InCondition : WhereCondition
	{
		public InCondition(SqlFragment columnOrExpr, IList values) : base()
		{
			// Build the fragment
			this.AppendFragment(columnOrExpr).AppendText(" IN (");

			int i;
			for (i = 0; i < values.Count - 1; i++)
			{
				if (values[i] == null)
				{
					this.AppendText("NULL,");
				}
				else
				{
					this.AppendParameter(values[i])
						.AppendText(",");
				}
			}

			this.AppendParameter(values[i]).AppendText(")");
		}

		public InCondition(string columnOrExpr, IList values)
			: this(columnOrExpr.ToSqlFragment(), values)
		{
		}
	}

	public class InCondition<T> : InCondition
	{
		public InCondition(Expression<Func<T, object>> lambdaGetter, IList values)
			: base(ExpressionTreeParser.GetPropOrFieldNameFromLambdaExpr(lambdaGetter), values)
		{
		}
	}
}
