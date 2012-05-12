using System;
using SqlBuilder;

namespace SqlBuilder.Conditions
{
	public class Between : WhereCondition
	{
		public Between(string column, object min_val, object max_val) : base()
		{
			// Build the fragment
			SqlFragment frag = new SqlFragment();
			frag.AppendText(column + " BETWEEN ")
				.AppendParameter(min_val)
				.AppendText(" AND ")
				.AppendParameter(max_val);
			
			this.SetSqlFragment(frag);
		}
		
		public Between(SqlFragment frag, object min_val, object max_val) : base() {
			// Build the fragment
			frag.AppendText(" BETWEEN ")
				.AppendParameter(min_val)
				.AppendText(" AND ")
				.AppendParameter(max_val);
			
			this.SetSqlFragment(frag);
		}
	}
}
