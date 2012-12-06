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
			{
				this.PrependText("(").AppendText(") AND ");
			}

			this.AppendFragment(andCondition);

			hasCondition = true;

			return this;
		}
		public WhereCondition And(SqlFragment andCondition) {
			return And(new WhereCondition(andCondition));
		}
		
		public WhereCondition Or(WhereCondition orCondition) {
			if (hasCondition)
			{
				this.PrependText("(").AppendText(") OR ");
			}

			this.AppendFragment(orCondition);

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
		public override string ToSqlString(int initialParameterIndex, IDictionary<string, object> parameters, IDictionary<object, int> parametersIdx) {
			return base.ToSqlString(initialParameterIndex, parameters, parametersIdx);
		}

		public WhereCondition() : base()
		{
			hasCondition = false;
		}
		
		public WhereCondition(SqlFragment frag) : this() {
			this.AppendFragment(frag);
			hasCondition = true;
		}

		#region Overloaded operators
		public static WhereCondition operator &(WhereCondition a, WhereCondition b) {
			WhereCondition anded = new WhereCondition(a);
			anded.And(b);

			return anded;
		}

		public static WhereCondition operator |(WhereCondition a, WhereCondition b) {
			WhereCondition ored = new WhereCondition(a);
			ored.Or(b);

			return ored;
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
