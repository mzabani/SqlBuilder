using System;
using SqlBuilder;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;

namespace SqlBuilder.Conditions
{
	public class InCondition : WhereCondition
	{
		//TODO: Make this class ParameterListFragment
		private bool Finalized;
		private bool HasAtLeastOneParameter;

		public InCondition AddParameter(object val) {
			if (Finalized)
				throw new InvalidOperationException("This IN condition is already finished. You either called Finish() or used one of the constructors that receive a list.");

			if (HasAtLeastOneParameter)
				AppendText(", ");

			HasAtLeastOneParameter = true;
			AppendParameter(val);
			return this;
		}

		public InCondition Finish() {
			AppendText(")");
			Finalized = true;
			return this;
		}

		public InCondition(SqlFragment columnOrExpr) : base() {
			// Build the fragment
			AppendFragment(columnOrExpr).AppendText(" IN (");
			Finalized = false;
			HasAtLeastOneParameter = false;
		}

		public InCondition(SqlFragment columnOrExpr, IList values) : this(columnOrExpr)
		{
			int i;
			for (i = 0; i < values.Count - 1; ++i)
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
			HasAtLeastOneParameter = true;
			Finalized = true;
		}

		public InCondition(string columnOrExpr, IList values)
			: this(columnOrExpr.ToSqlFragment(), values)
		{
		}
	}

	public class InCondition<T> : InCondition
	{
		public InCondition(Expression<Func<T, object>> lambdaGetter, IList values)
			: base(ExpressionTreeHelper.GetPropOrFieldNameFromLambdaExpr(lambdaGetter), values)
		{
		}
	}
}
