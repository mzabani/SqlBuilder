using System;
using System.Text;
using System.Collections.Generic;

namespace SqlBuilder
{
	/// <summary>
	/// A SqlFragment is a collection if SqlNodes. A space separated concatenation of SqlFragments form a Sql query.
	/// </summary>
	public class SqlFragment
	{
		private IList<SqlNode> nodes;
		
		/// <summary>
		/// Appends a node to this Sql fragment.
		/// </summary>
		/// <returns>
		/// This fragment.
		/// </returns>
		/// <param name='node'>
		/// The Sql Node to be appended.
		/// </param>
		private SqlFragment AppendNode(SqlNode node)
		{
			nodes.Add(node);
			
			return this;
		}
		
		/// <summary>
		/// Appends a text node (no parameters here) to this Sql fragment.
		/// </summary>
		/// <returns>
		/// This fragment.
		/// </returns>
		/// <param name='text'>
		/// The text to be appended as a text Sql Node.
		/// </param>
		public SqlFragment AppendText(string text) {
			SqlNode textNode = new SqlNode(text, SqlNodeType.Text);
			nodes.Add(textNode);
			
			return this;
		}

		/// <summary>
		/// Appends a formatted text node (no parameters here) to this Sql fragment.
		/// </summary>
		/// <returns>
		/// This fragment.
		/// </returns>
		/// <param name='text'>
		/// The text to be appended as a text Sql Node.
		/// </param>
		/// <param name='args'>
		/// The arguments passed to String.Format to create the text to be appended to the node.
		/// </param>
		public SqlFragment AppendTextFormatted(string text, params object[] args) {
			SqlNode textNode = new SqlNode(String.Format(text, args), SqlNodeType.Text);
			nodes.Add(textNode);
			
			return this;
		}
		
		/// <summary>
		/// Appends a parameter node to this Sql fragment.
		/// </summary>
		/// <returns>
		/// This fragment.
		/// </returns>
		/// <param name='text'>
		/// The parameter to be appended as a Sql Node.
		/// </param>
		public SqlFragment AppendParameter(object param) {
			SqlNode textNode = new SqlNode(param);
			nodes.Add(textNode);
			
			return this;
		}
		
		/// <summary>
		/// Appends all the virtual nodes in fragment <paramref name="frag"/> to this fragment.
		/// </summary>
		/// <param name='frag'>
		/// The Sql Fragment whose nodes are to be appended to this fragment.
		/// </param>
		public SqlFragment AppendFragment(SqlFragment frag) {
			foreach (SqlNode node in frag.GetNodes())
			{
				this.AppendNode(node);
			}
			
			return this;
		}
		
		/// <summary>
		/// Renders this SQL fragment.
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
		public virtual string ToSqlString(int initialParameterIndex, IDictionary<string, object> parameters) {
			StringBuilder sb = new StringBuilder(10);
			
			int parameterIndex = initialParameterIndex;
			foreach (SqlNode node in nodes)
			{
				object parameterValue = null;
				sb.Append(node.ToSqlString(parameterIndex, out parameterValue)).Append(" ");
				
				// Only increase the parameter index if the node was a parameter node
				// In this case, add it to the parameters dictionary
				if (parameterValue != null)
				{
					parameters.Add("p" + parameterIndex, parameterValue);
					parameterIndex++;
				}
					
			}
			
			return sb.Remove(sb.Length - 1, 1).ToString();
		}
		protected string ToSqlString() {
			IDictionary<string, object> trash = new Dictionary<string, object>();
			return this.ToSqlString(0, trash);
		}
		
		/// <summary>
		/// Retrieves nodes that could form this fragment, in order. The nodes returned here do not have to correspond to the
		/// actual "nodes" private list in the object. They have only to be returned such that they can form a copy of this SqlFragment.
		/// The nodes that form a fragment, whether they exist or not, are to be called virtual nodes.
		/// </summary>
		/// <returns>
		/// The nodes.
		/// </returns>
		internal virtual IEnumerable<SqlNode> GetNodes() {
			if (nodes != null)
			{
				foreach (SqlNode node in nodes)
				{
					yield return node;
				}
			}
		}
		
		public SqlFragment()
		{
			nodes = new List<SqlNode>(1);
		}
		
		public SqlFragment(string textFragment) {
			nodes = new List<SqlNode>(1);
			this.AppendText(textFragment);
		}
	}
}
