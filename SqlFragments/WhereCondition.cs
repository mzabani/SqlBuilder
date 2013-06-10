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
		#region AND'ing and OR'ing
		public WhereCondition And(WhereCondition andCondition) {
			if (!this.IsEmpty)
			{
				PrependText("(").AppendText(") AND ");
			}

			AppendFragment(andCondition);

			return this;
		}
		public WhereCondition And(SqlFragment andCondition) {
			return And(new WhereCondition(andCondition));
		}
		
		public WhereCondition Or(WhereCondition orCondition) {
			if (!this.IsEmpty)
			{
				PrependText("(").AppendText(") OR ");
			}

			AppendFragment(orCondition);

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
		public override string ToSqlString(ref int initialParameterIndex, IDictionary<string, object> parameters, IDictionary<object, int> parametersIdx) {
			return base.ToSqlString(ref initialParameterIndex, parameters, parametersIdx);
		}

		#region Constructors
		public WhereCondition() : base()
		{
		}
		
		public WhereCondition(SqlFragment frag) : this() {
			base.AppendFragment(frag);
		}
		#endregion

		#region Overloaded operators
		public static WhereCondition operator &(WhereCondition a, WhereCondition b) {
			return a.And(b);
		}

		public static WhereCondition operator |(WhereCondition a, WhereCondition b) {
			return a.Or(b);
		}

		public static bool operator true(WhereCondition a) {
			return false;
		}

		public static bool operator false(WhereCondition a) {
			return false;
		}
		#endregion
	}
}
