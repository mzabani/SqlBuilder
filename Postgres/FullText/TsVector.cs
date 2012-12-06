using System;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace SqlBuilder.Postgres
{
	public class TsVector : ProjectionFragment
	{
		public TsVector(string configuration, string textOrColumn, string weight, bool column)
		{
			if (weight != "A" && weight != "B" && weight != "C" && weight != "D" && weight != null)
				throw new ArgumentException("Parameter must be \"A\", \"B\", \"C\", \"D\" or null", "weight");

			if (weight != null)
			{
				this.AppendText("SETWEIGHT(");
			}

			this.AppendText("TO_TSVECTOR(")
				.AppendParameter(configuration)
				.AppendText(",");

			if (column)
				this.AppendText(textOrColumn);
			else
				this.AppendParameter(textOrColumn);
				
			this.AppendText(")");

			if (weight != null)
			{
				this.AppendText(",")
					.AppendParameter(weight)
					.AppendText(")");
			}
		}

		public TsVector(string configuration, string textOrColumn, bool column) : this(configuration, textOrColumn, null, column)
		{
		}
	}

	public class TsVector<T> : TsVector
		where T : class, new()
	{
		public TsVector(string configuration, Expression<Func<T, object>> lambdaGetterExpr) 
			: base(configuration, ExpressionTreeParser.GetPropOrFieldNameFromLambdaExpr(lambdaGetterExpr), true)
		{
		}
	}
}
