using System;
using SqlBuilder;
using System.Linq;
using System.Collections.Generic;

namespace SqlBuilder.Conditions
{
	public class InCondition : WhereCondition
	{
		public InCondition(string column, IList<int> ids) : base()
		{
			// Build the fragment
			SqlFragment frag = new SqlFragment();
			frag.AppendText(column + " IN (");
			
			for (int i = 0; i < ids.Count - 1; i++)
			{
				frag.AppendText(ids[i] + ", ");
			}
			
			frag.AppendText(ids.Last() + ")");
			
			this.SetSqlFragment(frag);
		}
	}
}
