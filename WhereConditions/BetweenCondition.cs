using System;
using SqlBuilder;

namespace SqlBuilder.Conditions
{
	public class Between : WhereCondition
	{
		public Between(string columnOrExpression, object min_val, object max_val) : base()
		{
			// Build the fragment
			this.AppendText(columnOrExpression + " BETWEEN ")
				.AppendParameter(min_val)
				.AppendText(" AND ")
				.AppendParameter(max_val);
		}
		
		public Between(SqlFragment frag, object min_val, object max_val) : base() {
			// Build the fragment
			this.AppendFragment(frag)
				.AppendText(" BETWEEN ")
				.AppendParameter(min_val)
				.AppendText(" AND ")
				.AppendParameter(max_val);
		}
	}
}
