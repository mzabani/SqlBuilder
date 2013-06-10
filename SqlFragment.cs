using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

using System.Linq.Expressions;

namespace SqlBuilder
{
	public enum SqlFragmentType {
		Text = 1, Parameter,

		/// <summary>
		/// A list of SqlFragments.
		/// </summary>
		Complex
	};

	/// <summary>
	/// A SqlFragment is either a query parameter, a string or a collection of other SqlFragments. A space separated concatenation of SqlFragments form an SQL query.
	/// </summary>
	public class SqlFragment
	{
		public SqlFragmentType FragmentType { get; private set; }
		public string Text { get; private set; }
		public object Parameter { get; private set; }
		private LinkedList<SqlFragment> Fragments;

		public bool IsEmpty { get; private set; }

		/// <summary>
		/// Appends text (no query parameters here) to this Sql fragment.
		/// </summary>
		/// <returns>
		/// This fragment.
		/// </returns>
		/// <param name='text'>
		/// The text to be appended.
		/// </param>
		public SqlFragment AppendText(string text) {
			return AppendFragment(new SqlFragment(text));
		}

		/// <summary>
		/// Appends formatted text (no query parameters here) to this Sql fragment.
		/// </summary>
		/// <returns>
		/// This fragment.
		/// </returns>
		/// <param name='text'>
		/// The format string and its associated values to be appended.
		/// </param>
		public SqlFragment AppendText(string format, params object[] prms) {
			return AppendText(String.Format(format, prms));
		}

		/// <summary>
		/// Prepends text (no parameters here) to this Sql fragment.
		/// </summary>
		/// <returns>
		/// This fragment.
		/// </returns>
		/// <param name='text'>
		/// The text to be prepended.
		/// </param>
		public SqlFragment PrependText(string text) {
			return PrependFragment(new SqlFragment(text));
		}

		/// <summary>
		/// Prepends formatted text (no query parameters here) to this Sql fragment.
		/// </summary>
		/// <returns>
		/// This fragment.
		/// </returns>
		/// <param name='text'>
		/// The format string and its associated values to be prepended.
		/// </param>
		public SqlFragment PrependText(string format, params object[] prms) {
			return PrependText(String.Format(format, prms));
		}

		/// <summary>
		/// Appends a parameter to this Sql fragment.
		/// </summary>
		/// <returns>
		/// This fragment.
		/// </returns>
		/// <param name='text'>
		/// The parameter to be appended.
		/// </param>
		public SqlFragment AppendParameter(object param) {
			return AppendFragment(new SqlFragment(param, SqlFragmentType.Parameter));
		}

		/// <summary>
		/// Prepends a parameter to this Sql fragment.
		/// </summary>
		/// <returns>
		/// This fragment.
		/// </returns>
		/// <param name='text'>
		/// The parameter to be prepended.
		/// </param>
		public SqlFragment PrependParameter(object param) {
			return PrependFragment(new SqlFragment(param, SqlFragmentType.Parameter));
		}

		/// <summary>
		/// Appends the SqlFragment <paramref name="frag"/> to this fragment. The fragment to be appended is not copied, i.e. we only keep a reference, so
		/// if the appended fragment is changed, this fragment changes too.
		/// </summary>
		/// <param name='frag'>
		/// The SqlFragment to be appended.
		/// </param>
		public SqlFragment AppendFragment(SqlFragment frag) {
			if (frag == null)
				throw new ArgumentNullException("frag");

			IsEmpty = false;
			FragmentType = SqlFragmentType.Complex;
			Fragments.AddLast(frag);
			
			return this;
		}

		/// <summary>
		/// Prepends the SqlFragment <paramref name="frag"/> to this fragment. The fragment to be prepended is not copied, i.e. we only keep a reference, so
		/// if the prepended fragment is changed, this fragment changes too.
		/// </summary>
		/// <param name='frag'>
		/// The Sql Fragment to be prepended.
		/// </param>
		public SqlFragment PrependFragment(SqlFragment frag) {
			if (frag == null)
				throw new ArgumentNullException("frag");

			IsEmpty = false;
			FragmentType = SqlFragmentType.Complex;
			Fragments.AddFirst(frag);
			
			return this;
		}

		protected void BuildParameter(StringBuilder sb, ref int parameterIndex, object parameterValue, IDictionary<string, object> parameters, IDictionary<object, int> parametersIdx) {
			if (parameterValue == null)
				throw new ArgumentNullException("parameterValue");

			if (parametersIdx.ContainsKey(parameterValue))
			{
				// The idx does not change, this parameter has already been added
				int paramIdx = parametersIdx[parameterValue];
				sb.Append(":p" + paramIdx).Append(" ");
			}
			else
			{
				// New parameter, increment index
				string paramName = ":p" + parameterIndex;
				parametersIdx.Add(parameterValue, parameterIndex);
				parameters.Add(paramName, parameterValue);
				sb.Append(paramName).Append(" ");
				++parameterIndex;
			}
		}

		/// <summary>
		/// Renders this SQL fragment.
		/// </summary>
		/// <param name="parameterIndex">
		/// The parameters added in the conditions will be the letter "p" concatenated with an index that starts with this parameter. As parameters are added,
		/// this integer is incremented accordingly.
		/// </param>
		/// <param name="parameters">
		/// An initialized IDictionary. Any parameters in this SqlFragment will be added to it.
		/// </param>
		/// <param name="parametersIdx">
		/// An initialized IDictionary. This dictionary is used to keep a relation between parameters' values and their parameter index. This way
		/// it can avoid adding the same parameter more than once, generating prettier SQL.
		/// </param>
		/// <returns>
		/// The appropriate SQL fragment.
		/// </returns>
		public virtual string ToSqlString(ref int parameterIndex, IDictionary<string, object> parameters, IDictionary<object, int> parametersIdx) {
			StringBuilder sb = new StringBuilder(10);

			// First try Text and Parameter, then the list of fragments
			if (Text != null)
				sb.Append(Text);
			else if (Parameter != null)
				BuildParameter(sb, ref parameterIndex, Parameter, parameters, parametersIdx);

			foreach (SqlFragment frag in Fragments)
			{
				sb.Append(frag.ToSqlString(ref parameterIndex, parameters, parametersIdx));
			}
			
			return sb.ToString();
		}
		public string ToSqlString() {
			IDictionary<string, object> trash = new Dictionary<string, object>();
			IDictionary<object, int> trash2 = new Dictionary<object, int>();
			int count = 0;
			return this.ToSqlString(ref count, trash, trash2);
		}

		#region Constructors
		public SqlFragment()
		{
			IsEmpty = true;
			Fragments = new LinkedList<SqlFragment>();
		}
		
		public SqlFragment(string textFragment) : this() {
			if (textFragment == null)
				throw new ArgumentNullException("A SqlFragment cannot contain null text");

			IsEmpty = false;
			FragmentType = SqlFragmentType.Text;
			Text = textFragment;
		}

		public SqlFragment(object parameter, SqlFragmentType type) : this() {
			if (parameter == null)
				throw new ArgumentNullException("A query parameter cannot be null");
			else if (type != SqlFragmentType.Parameter)
				throw new ArgumentException("This constructor must be used to build a parameter SqlFragment. This parameter must equal SqlFragmentType.Parameter");

			IsEmpty = false;
			Parameter = parameter;
			FragmentType = SqlBuilder.SqlFragmentType.Parameter;
		}

		public SqlFragment(SqlFragment sqlFragment) : this() {
			IsEmpty = false;
			AppendFragment(sqlFragment);
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
			: base(ExpressionTreeHelper.GetPropOrFieldNameFromLambdaExpr(lambdaGetterExpr))
		{
		}

	}
}
