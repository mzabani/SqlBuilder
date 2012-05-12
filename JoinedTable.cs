using System;

namespace SqlBuilder
{
	public enum JoinType {
		InnerJoin, LeftOuterJoin
	};
	
	class JoinedTable
	{
		public string table;
		public JoinType joinType;
		
		public string column1;
		public string column2;
		
		public override int GetHashCode()
		{
			int hash = 0;
		 	
		    foreach (char c in table)
		        hash = ((hash << 5) + hash) + c; /* hash * 33 + c */
		 	
		    return hash;
		}
		
		public override bool Equals (object obj)
		{
			// TODO: Multiple joins to the same table
			JoinedTable b = (JoinedTable) obj;
			return this.table == b.table;
		}
		
		public JoinedTable(string table, string column1, string column2, JoinType joinType)
		{
			this.table = table;
			this.column1 = column1;
			this.column2 = column2;
			this.joinType = joinType;
		}
	}
}

