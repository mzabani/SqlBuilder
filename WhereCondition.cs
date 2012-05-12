using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace SqlBuilder
{
	enum WhereConditionType {
		And = 1, Or
	};
	
	/// <summary>
	/// Useful class to represent a SQL WHERE clause, with methods to AND and OR other conditions along and to set parameters.
	/// It is basically a class with useful methods to generate a useful Sql Fragment for a WHERE clause.
	/// </summary>
	public class WhereCondition
	{
		// A WhereCondition is either:
		// 1. A SQL fragment of a SQL condition, along with the values of any parameters
		private SqlFragment condition;
		
		// 2. An ordered list of WhereConditions attached by And's and Or's, which are also in ordered fashion
		private IList<WhereCondition> innerConditions;
		private IList<WhereConditionType> innerConditionsLinks;
		
		/// <summary>
		/// Renders a SQL fragment that represents the conditions expressed in this WhereCondition.
		/// </summary>
		/// <returns>
		/// The corresponding SqlFragment, without the "WHERE " part.
		/// </returns>
		public SqlFragment ToSqlFragment() {
			if (condition != null)
			{
				return condition;
			}
			
			else if (innerConditions != null && innerConditions.Count > 0)
			{
				SqlFragment ret = new SqlFragment();
				
				ret.AppendFragment(innerConditions[0].ToSqlFragment());
				
				for (int i = 0; i < innerConditionsLinks.Count; i++)
				{
					ret.AppendText(innerConditionsLinks[i] == WhereConditionType.And ? " AND " : " OR ");
					ret.AppendFragment(innerConditions[i + 1].ToSqlFragment());
				}
				
				return ret;
			}
			
			else
			{
				throw new Exception("Empty WhereCondition detected!");
			}
		}
		
		/// <summary>
		/// If this WhereCondition is a simple SQL fragment, transform it into an ordered list of WhereConditions
		/// with the purpose of adding further constraints through AND and/or OR.
		/// </summary>
		private void TransformStrConditionToInnerCondition() {
			WhereCondition firstWhereCondition = new WhereCondition(condition);
			condition = null;
			innerConditions = new List<WhereCondition>(1) { firstWhereCondition };
			innerConditionsLinks = new List<WhereConditionType>();
		}
		
		#region AND'ing and OR'ing
		public WhereCondition And(WhereCondition andCondition) {
			if (condition != null)
				TransformStrConditionToInnerCondition();

			innerConditions.Add(andCondition);
			innerConditionsLinks.Add(WhereConditionType.And);
			
			return this;
		}
		
		public WhereCondition Or(WhereCondition orCondition) {
			if (condition != null)
				TransformStrConditionToInnerCondition();
			
			innerConditions.Add(orCondition);
			innerConditionsLinks.Add(WhereConditionType.Or);
			
			return this;
		}
		#endregion
		
		/// <summary>
		/// Renders the appropriate SQL WHERE clause, without the "WHERE" string.
		/// </summary>
		/// <param name="initialParameterIndex">
		/// The parameters added in the conditions will be the letter "p" concatenated with an index, that starts with this parameter.
		/// </param>
		/// <param name="parameters">
		/// An initialized IDictionary. Any parameters in this Sql fragment will be added to it.
		/// </param>
		/// <returns>
		/// The appropriate SQL WHERE clause, without the "WHERE" string.
		/// </returns>
		public string ToSqlString(int initialParameterIndex, IDictionary<string, object> parameters) {
			if (condition != null)
				return condition.ToSqlString(initialParameterIndex, parameters);
			
			int parameterIndex = initialParameterIndex;
			
			StringBuilder sb = new StringBuilder();
			int num_params = parameters.Count;
			sb.AppendFormat("({0})", innerConditions[0].ToSqlString(parameterIndex, parameters));
			
			for (int i = 0; i < innerConditionsLinks.Count; i++)
			{
				// Increase the parameterIndex appropriately
				parameterIndex += parameters.Count - num_params;
				num_params = parameters.Count;
				
				sb.AppendFormat(" {0} ({1})", innerConditionsLinks[i] == WhereConditionType.And ? "AND" : "OR",
				                			  innerConditions[i + 1].ToSqlString(parameterIndex, parameters));
			}
			
			return sb.ToString();
		}
		
		public void Reset() {
			this.condition = null;
			this.innerConditions = null;
			this.innerConditionsLinks = null;
		}
		
		public void SetSqlFragment(SqlFragment frag) {
			if (innerConditions != null)
				throw new Exception("This Sql fragment contains many conditions in it. Use SqlFragment.Reset() first if you really want to do this");
			
			this.condition = frag;
		}
		
		internal WhereCondition() { }
		
		public WhereCondition(SqlFragment frag) {
			SetSqlFragment(frag);
		}
	}
}