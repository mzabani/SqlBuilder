using System;
using SqlBuilder;
using System.Linq;
using System.Collections.Generic;

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
		public NullCondition(string column, bool isNotNull) : base()
		{
			// Build the fragment
			SqlFragment frag = new SqlFragment();
			if (isNotNull)
			{
				frag.AppendText(column + " IS NOT NULL");
			}
			else
			{
				frag.AppendText(column + " IS NULL");
			}
			
			this.SetSqlFragment(frag);
		}
	}
}
