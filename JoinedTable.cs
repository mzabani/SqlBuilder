using System;

namespace SqlBuilder
{
	public enum JoinType {
		InnerJoin, LeftOuterJoin
	};
	
	class JoinedTable
	{
		public string Table;
		public JoinType JoinType;
		
		public string Column1;
		public string Column2;
		
		public override int GetHashCode()
		{
			int hash = 0;
		 	
		    foreach (char c in Table)
		        hash = ((hash << 5) + hash) + c; // hash * 33 + c
		 	
		    return hash;
		}
		
		public override bool Equals (object obj)
		{
			// TODO: Multiple joins to the same table
			JoinedTable b = (JoinedTable) obj;
			return this.Table == b.Table;
		}
		
		public JoinedTable(string table, string column1, string column2, JoinType joinType)
		{
			this.Table = table;
			this.Column1 = column1;
			this.Column2 = column2;
			this.JoinType = joinType;
		}
	}
}

