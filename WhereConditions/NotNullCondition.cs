using System;
using System.Linq.Expressions;

namespace SqlBuilder.Conditions
{
	public class NullCondition : WhereCondition
	{
		/// <summary>
		/// Builds a IS NULL or a IS NOT NULL condition, according to <paramref name="isNotNull"/>
		/// </summary>
		/// <param name='column'>
		/// The name of the column.
		/// </param>
		/// <param name='isNotNull'>
		/// If set to true, creates a IS NOT NULL condition, otherwise it creates a IS NULL condition.
		/// </param>
		public NullCondition(SqlFragment columnOrExpr, bool isNotNull) : base()
		{
			// Build the fragment
			if (isNotNull)
			{
				this.AppendFragment(columnOrExpr).AppendText(" IS NOT NULL");
			}
			else
			{
				this.AppendFragment(columnOrExpr).AppendText(" IS NULL");
			}
		}

		public NullCondition(string columnOrExpr, bool isNotNull)
			: this(columnOrExpr.ToSqlFragment(), isNotNull)
		{
		}
	}

	public class NullCondition<T> : NullCondition
	{
		public NullCondition(Expression<Func<T, object>> lambdaGetter, bool isNotNull)
			: base(ExpressionTreeHelper.GetPropOrFieldNameFromLambdaExpr(lambdaGetter), isNotNull)
		{
		}
	}
}
