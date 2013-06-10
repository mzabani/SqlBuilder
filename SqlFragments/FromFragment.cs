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
		public override string ToSqlString(ref int initialParameterIndex, IDictionary<string, object> parameters, IDictionary<object, int> parametersIdx) {
			string frag = base.ToSqlString(ref initialParameterIndex, parameters, parametersIdx);
			if (alias == null)
				return frag;
			
			return "(" + frag + ") AS " + alias;
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
