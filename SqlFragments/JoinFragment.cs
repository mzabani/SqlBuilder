using System;
using System.Collections.Generic;

namespace SqlBuilder
{
	public enum JoinType {
		InnerJoin,
		LeftOuterJoin
	}

	public class JoinFragment : SqlFragment
	{
		/// <summary>
		/// Creates a fragment that represents a join. This fragment should render to "INNER JOIN table ON condition" or similar.
		/// </summary>
		public JoinFragment(string table, SqlFragment joinCondition, JoinType joinType)
		{
			if (joinType == JoinType.InnerJoin)
				AppendText("INNER JOIN ");
			else
				AppendText("LEFT OUTER JOIN ");

			AppendText(table + " ON ");
			AppendFragment(joinCondition);
		}

		public JoinFragment(string table, string column1, string column2, JoinType joinType)
			: this(table, new SqlFragment(column1 + "=" + column2), joinType)
		{
		}
	}
}
