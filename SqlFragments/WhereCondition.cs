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
	public class WhereCondition : SqlFragment
	{
		private bool hasCondition;

		#region AND'ing and OR'ing
		public WhereCondition And(WhereCondition andCondition) {
			if (hasCondition)
				this.AppendText("AND ");

			this.AppendText("(")
				.AppendFragment(andCondition)
				.AppendText(")");

			hasCondition = true;

			/*if (condition != null)
				TransformStrConditionToInnerCondition();

			innerConditions.Add(andCondition);
			innerConditionsLinks.Add(WhereConditionType.And);*/
			
			return this;
		}
		public WhereCondition And(SqlFragment andCondition) {
			return And(new WhereCondition(andCondition));
		}
		
		public WhereCondition Or(WhereCondition orCondition) {
			if (hasCondition)
				this.AppendText("OR  ");

			this.AppendText("(")
				.AppendFragment(orCondition)
				.AppendText(")");

			hasCondition = true;

			return this;
		}
		public WhereCondition Or(SqlFragment orCondition) {
			return Or(new WhereCondition(orCondition));
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
		public override string ToSqlString(int initialParameterIndex, IDictionary<string, object> parameters) {
			return "(" + base.ToSqlString(initialParameterIndex, parameters) + ")";

			/*StringBuilder sb = new StringBuilder();
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
			
			return sb.ToString();*/
		}

		public WhereCondition() : base()
		{
			hasCondition = false;
		}
		
		public WhereCondition(SqlFragment frag) : this() {
			this.AppendFragment(frag);
			hasCondition = true;
		}
	}
}
