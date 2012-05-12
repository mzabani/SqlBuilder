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
	class SqlNode
	{
		private SqlNodeType type;
		
		// Either a text fragment...
		private string textFragment;
		
		// or a parameter
		private object parameter;
		
		/// <summary>
		/// Renders the SQL fragment that corresponds to this node.
		/// </summary>
		/// <param name="parameterIndex">
		/// The index of this node's parameter (if any), such that its name is a "p" concatenated with this number.
		/// </param>
		/// <param name="parameter">
		/// The parameter in this node, if any.
		/// </param>
		/// <returns>
		/// The appropriate SQL fragment.
		/// </returns>
		public string ToSqlString(int parameterIndex, out object nodeParameter) {
			if (type == SqlNodeType.Param)
			{
				nodeParameter = parameter;
				return ":p" + parameterIndex;
			}
			
			// No parameter for text nodes
			nodeParameter = null;
			return textFragment;
		}
		
		/// <summary>
		/// Gets the parameter if this node is a parameter node, or null if it is not.
		/// </summary>
		/// <returns>
		/// This node's parameter, or null if it is not a parameter node.
		/// </returns>
		/*public object GetParameter() {
			if (type == SqlNodeType.Param)
			{
				return parameter;
			}
			
			return null;
		}
		*/
		public SqlNode(string textFragment, SqlNodeType type)
		{
			if (type == SqlNodeType.Param)
				throw new Exception("A text node cannot be a parameter node");
			
			this.textFragment = textFragment;
			this.type = type;
		}
		public SqlNode(object val)
		{
			this.type = SqlNodeType.Param;
			this.parameter = val;
		}
	}
}
