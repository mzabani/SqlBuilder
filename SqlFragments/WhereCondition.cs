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
		/// <summary>
		/// Creates a copy of this condition in case you don't want to modify this.
		/// </summary>
		private WhereCondition CopyThisCondition() {
			return new WhereCondition(this);
		}

		#region AND'ing and OR'ing
		public WhereCondition And(WhereCondition andCondition) {
			WhereCondition copy = CopyThisCondition();

			if (!copy.IsEmpty)
			{
				copy.PrependText("(").AppendText(") AND ");
			}

			copy.AppendFragment(andCondition);

			return copy;
		}
		public WhereCondition And(SqlFragment andCondition) {
			return And(new WhereCondition(andCondition));
		}
		
		public WhereCondition Or(WhereCondition orCondition) {
			WhereCondition copy = CopyThisCondition();

			if (!copy.IsEmpty)
			{
				copy.PrependText("(").AppendText(") OR ");
			}

			copy.AppendFragment(orCondition);

			return copy;
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
