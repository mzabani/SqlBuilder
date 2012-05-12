using System;
using System.Collections.Generic;

namespace SqlBuilder
{
	public class FromFragment : SqlFragment
	{
		private string alias;
		
		/// <summary>
		/// Defines an alias for this subquery, which renders a "projection AS alias" SQL fragment.
		/// </summary>
		/// <param name='alias'>
		/// The desired alias for this subquery.
		/// </param>
		public FromFragment As(string alias) {
			this.alias = alias;
			
			return this;
		}
		
		/// <summary>
		/// Renders this SQL fragment with a possible alias ("AS").
		/// </summary>
		/// <param name="initialParameterIndex">
		/// The parameters added in the conditions will be the letter "p" concatenated with an index that starts with this parameter.
		/// </param>
		/// <param name="parameters">
		/// An initialized IDictionary. Any parameters in this Sql fragment will be added to it.
		/// </param>
		/// <returns>
		/// The appropriate SQL fragment.
		/// </returns>
		public override string ToSqlString(int initialParameterIndex, IDictionary<string, object> parameters) {
			string frag = base.ToSqlString(initialParameterIndex, parameters);
			if (alias == null)
				return frag;
			
			return "(" + frag + ") AS " + alias;
		}
		
		internal override IEnumerable<SqlNode> GetNodes()
		{
			// Return nodes for parentheses and the aliasing (in case this is a subquery)
			if (alias != null)
				yield return new SqlNode("(", SqlNodeType.Text);
			
			foreach (SqlNode node in base.GetNodes())
			{
				yield return node;
			}
			
			if (alias != null)
				yield return new SqlNode(") AS " + alias, SqlNodeType.Text);
		}
		
		/// <summary>
		/// Creates a fragment that represents a table. This means that ToSqlString will only render the table's name.
		/// </summary>
		/// <param name='table'>
		/// The name of the table.
		/// </param>
		public FromFragment(string table)
		{
			this.AppendText(table);
		}
		
		/// <summary>
		/// Creates a fragment that represents a subquery. This means that ToSqlString will render the subquery inside parentheses and alias it.
		/// </summary>
		/// <param name='sf'>
		/// A valid query sql fragment.
		/// </param>
		/// <param name='alias'>
		/// The alias of the subquery.
		/// </param>
		public FromFragment(SqlFragment sf, string alias)
		{
			this.alias = alias;
			this.AppendFragment(sf);
		}
	}
}
