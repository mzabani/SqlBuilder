using System;
using System.Collections.Generic;

namespace SqlBuilder
{
	public enum OrderBy {
		Asc = 1, Desc
	};
	
	public class OrderByFragment : SqlFragment
	{
		private OrderBy orderBy;
		
		/// <summary>
		/// Renders this SQL fragment with the ordering type (ASC or DESC)
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
			
			if (orderBy == OrderBy.Desc)
				return frag + " DESC";
			else
				return frag;
		}
		
		internal override IEnumerable<SqlNode> GetNodes()
		{
			foreach (SqlNode node in base.GetNodes())
			{
				yield return node;
			}
			
			// Return a text node for the " ASC" or " DESC" part
			if (orderBy == OrderBy.Desc)
				yield return new SqlNode(" DESC", SqlNodeType.Text);
		}

		/// <summary>
		/// Sets or changes the sorting order of this fragment.
		/// </summary>
		/// <param name='orderBy'>
		/// The desired sorting order.
		/// </param>
		public void SetOrder(OrderBy orderBy) {
			this.orderBy = orderBy;
		}

		/// <summary>
		/// Creates a order by ascending fragment.
		/// </summary>
		/// <param name='textFragment'>
		/// The column or expression that will be the ordering criteria.
		/// </param>
		public OrderByFragment(string textFragment)
		{
			orderBy = OrderBy.Asc;
			this.AppendText(textFragment);
		}

		/// <summary>
		/// Creates a order by ascending fragment.
		/// </summary>
		/// <param name='sqlFragment'>
		/// The column or expression that will be the ordering criteria.
		/// </param>
		public OrderByFragment(SqlFragment sqlFragment)
		{
			orderBy = OrderBy.Asc;
			this.AppendFragment(sqlFragment);
		}
		
		/// <summary>
		/// Creates a order by fragment.
		/// </summary>
		/// <param name='textFragment'>
		/// The column or expression that will be the ordering criteria.
		/// </param>
		/// <param name='orderBy'>
		/// Defines if results will be ordered in ascending or descending fashion.
		/// </param>
		public OrderByFragment(string textFragment, OrderBy orderBy)
		{
			this.orderBy = orderBy;
			this.AppendText(textFragment);
		}

		/// <summary>
		/// Creates a order by fragment.
		/// </summary>
		/// <param name='sqlFragment'>
		/// The column or expression that will be the ordering criteria.
		/// </param>
		/// <param name='orderBy'>
		/// Defines if results will be ordered in ascending or descending fashion.
		/// </param>
		public OrderByFragment(SqlFragment sqlFragment, OrderBy orderBy)
		{
			this.orderBy = orderBy;
			this.AppendFragment(sqlFragment);
		}
	}
}
