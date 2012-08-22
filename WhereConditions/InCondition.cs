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
			this.AppendText(column + " IN (");
			
			for (int i = 0; i < ids.Count - 1; i++)
			{
				this.AppendParameter(ids[i])
					.AppendText(",");
			}
			
			this.AppendText(ids.Last() + ")");
		}
	}
}
