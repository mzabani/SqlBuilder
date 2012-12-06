using System;
using System.Collections.Generic;

namespace SqlBuilder
{
	enum SqlNodeType {
		Text = 1, Param
	};
	
	/// <summary>
	/// A SQL Node. Nodes are the fragments that form the SQL Query String when concatenated through spaces.
	/// Nodes can either be a text fragment or a parameter.
	/// </summary>
	internal class SqlNode
	{
		private SqlNodeType type;
		
		// Either a text fragment...
		private string textFragment;
		
		// or a parameter
		private object paramValue;
		
		/// <summary>
		/// Renders the SQL fragment that corresponds to this node.
		/// </summary>
		/// <param name="parameterIndex">
		/// The index of this node's parameter (if any), such that its name is a "p" concatenated with this number.
		/// </param>
		/// <param name="parameter">
		/// An object which will point to the parameter in this node, if any.
		/// </param>
		/// <returns>
		/// The appropriate SQL fragment.
		/// </returns>
		public string ToSqlString(int parameterIndex, out object nodeParameter) {
			if (type == SqlNodeType.Param)
			{
				nodeParameter = paramValue;
				return ":p" + parameterIndex;
			}
			
			// No parameter for text nodes
			nodeParameter = null;
			return textFragment;
		}

		public object GetParameter() {
			return paramValue;
		}
		public string GetText() {
			return textFragment;
		}

		public override int GetHashCode ()
		{
			if (type == SqlNodeType.Param)
				return paramValue.GetHashCode();
			else
				return textFragment.GetHashCode();
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			else if (obj is SqlNode == false)
				return false;

			SqlNode b = (SqlNode)obj;
			return (paramValue == b.paramValue && textFragment == b.textFragment);
		}

		public SqlNode(string textFragment, SqlNodeType type)
		{
			if (type == SqlNodeType.Param)
				throw new ArgumentException("A text node cannot be a parameter node");
			else if (textFragment == null)
				throw new ArgumentNullException("A text fragment cannot be null");

			this.paramValue = null;
			this.textFragment = textFragment;
			this.type = type;
		}
		public SqlNode(object val)
		{
			if (val == null)
				throw new ArgumentNullException("A SqlNode's parameter cannot be null");

			this.textFragment = null;
			this.type = SqlNodeType.Param;
			this.paramValue = val;
		}
	}
}
