using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using System.Linq.Expressions;

namespace SqlBuilder
{
	/// <summary>
	/// A SqlFragment is a collection if SqlNodes. A space separated concatenation of SqlFragments form a Sql query.
	/// </summary>
	public class SqlFragment
	{
		private LinkedList<SqlNode> nodes;
		
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
			nodes.AddLast(node);
			
			return this;
		}

		/// <summary>
		/// Prepends a node to this Sql fragment.
		/// </summary>
		/// <returns>
		/// This fragment.
		/// </returns>
		/// <param name='node'>
		/// The Sql Node to be prepended.
		/// </param>
		private SqlFragment PrependNode(SqlNode node)
		{
			nodes.AddFirst(node);
			
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
			return AppendNode(new SqlNode(text, SqlNodeType.Text));
		}

		/// <summary>
		/// Prepends a text node (no parameters here) to this Sql fragment.
		/// </summary>
		/// <returns>
		/// This fragment.
		/// </returns>
		/// <param name='text'>
		/// The text to be prepende as a text Sql Node.
		/// </param>
		public SqlFragment PrependText(string text) {
			return PrependNode(new SqlNode(text, SqlNodeType.Text));
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
		public SqlFragment AppendText(string text, params object[] args) {
			return AppendNode(new SqlNode(String.Format(text, args), SqlNodeType.Text));
		}

		/// <summary>
		/// Prepends a formatted text node (no parameters here) to this Sql fragment.
		/// </summary>
		/// <returns>
		/// This fragment.
		/// </returns>
		/// <param name='text'>
		/// The text to be prepended as a text Sql Node.
		/// </param>
		/// <param name='args'>
		/// The arguments passed to String.Format.
		/// </param>
		public SqlFragment PrependText(string text, params object[] args) {
			return PrependNode(new SqlNode(String.Format(text, args), SqlNodeType.Text));
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
			return AppendNode(new SqlNode(param));
		}

		/// <summary>
		/// Prepends a parameter node to this Sql fragment.
		/// </summary>
		/// <returns>
		/// This fragment.
		/// </returns>
		/// <param name='text'>
		/// The parameter to be prepended as a Sql Node.
		/// </param>
		public SqlFragment PrependParameter(object param) {
			return PrependNode(new SqlNode(param));
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
		/// Prepends all the virtual nodes in fragment <paramref name="frag"/> to this fragment.
		/// </summary>
		/// <param name='frag'>
		/// The Sql Fragment whose nodes are to be prepended to this fragment.
		/// </param>
		public SqlFragment PrependFragment(SqlFragment frag) {
			foreach (SqlNode node in frag.GetNodes())
			{
				this.PrependNode(node);
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
		public virtual string ToSqlString(int initialParameterIndex, IDictionary<string, object> parameters, IDictionary<object, int> parametersIdx) {
			StringBuilder sb = new StringBuilder(10);
			
			int parameterIndex = initialParameterIndex;
			foreach (SqlNode node in nodes)
			{
				object paramValue = node.GetParameter();

				// If this is a parameter node, check that it has not been added to the parameters dictionary already
				if (paramValue != null)
				{
					if (parametersIdx.ContainsKey(paramValue))
					{
						int paramIdx = parametersIdx[paramValue];
						sb.Append(":p" + paramIdx).Append(" ");
					}
					else
					{
						string paramName = ":p" + parameterIndex;
						parametersIdx.Add(paramValue, parameterIndex);
						parameters.Add(paramName, paramValue);
						sb.Append(paramName).Append(" ");
						parameterIndex++;
					}
				}
				else
				{
					sb.Append(node.GetText());
				}
			}
			
			return sb.ToString();
		}
		protected string ToSqlString() {
			IDictionary<string, object> trash = new Dictionary<string, object>();
			IDictionary<object, int> trash2 = new Dictionary<object, int>();
			return this.ToSqlString(0, trash, trash2);
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

		#region Constructors
		public SqlFragment()
		{
			nodes = new LinkedList<SqlNode>();
		}
		
		public SqlFragment(string textFragment) : this() {
			this.AppendText(textFragment);
		}

		public SqlFragment(SqlFragment sqlFragment) : this() {
			this.AppendFragment(sqlFragment);
		}
		#endregion
	}

	public class SqlFragment<T> : SqlFragment
		where T : new()
	{
		/// <summary>
		/// Creates a SqlFragment with the type's field/property's name.
		/// </summary>
		/// <param name='lambdaGetterExpr'>
		/// A lambda expression that returns the desired property or field.
		/// </param>
		public SqlFragment(Expression<Func<T, object>> lambdaGetterExpr)
			: base(ExpressionTreeParser.GetPropOrFieldNameFromLambdaExpr(lambdaGetterExpr))
		{
		}

	}
}
