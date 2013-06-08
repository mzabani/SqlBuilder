using System;
using System.Collections.Generic;

namespace SqlBuilder
{
	public class ProjectionFragment : SqlFragment
	{
		private string alias;
		
		/// <summary>
		/// Gets the projection's name, which is its alias, if an alias is defined, or the projection's column's name.
		/// </summary>
		/// <returns>
		/// The projection's name.
		/// </returns>
		public string GetName() {
			if (alias != null)
				return alias;
			
			// The projection can be in the format "table.column", so we better check for this
			string textProj = this.ToSqlString();
			if (textProj.Contains("."))
				return textProj.Split('.')[1];
			else
				return textProj;
		}
		
		/// <summary>
		/// Defines an alias for this projection, which renders a "projection AS alias" SQL fragment.
		/// </summary>
		/// <param name='alias'>
		/// The desired alias for this projection.
		/// </param>
		public ProjectionFragment As(string alias) {
			this.alias = alias;
			
			return this;
		}
		
		/// <summary>
		/// Renders this SQL fragment with a possible alias ("AS").
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
		public override string ToSqlString(int initialParameterIndex, IDictionary<string, object> parameters, IDictionary<object, int> parametersIdx) {
			string frag = base.ToSqlString(initialParameterIndex, parameters, parametersIdx);
			if (alias == null)
				return frag;
			
			return frag + " AS " + alias;
		}
		
		internal override IEnumerable<SqlNode> GetNodes()
		{
			foreach (SqlNode node in base.GetNodes())
			{
				yield return node;
			}
			
			// Return a text node for the " AS alias" part, if an alias was specified
			if (alias != null)
				yield return new SqlNode(" AS " + alias, SqlNodeType.Text);
		}


		public ProjectionFragment()
		{

		}

		/// <summary>
		/// Creates a projection with a name. Projections should have the same name of the properties of
		/// the results' class or an expression, in which case you should give it an alias with the name of a valid property.
		/// </summary>
		/// <param name='fragment'>
		/// The textual part of the projection.
		/// </param>
		public ProjectionFragment(SqlFragment fragment)
		{
			this.AppendFragment(fragment);
		}

		/// <summary>
		/// Creates a projection with a name. Projections should have the same name of the properties of
		/// the results' class or an expression, in which case you should give it an alias with the name of a valid property.
		/// </summary>
		/// <param name='textFragment'>
		/// The textual part of the projection.
		/// </param>
		public ProjectionFragment(string textFragment)
		{
			this.AppendText(textFragment);
		}
		
		/// <summary>
		/// Creates a projection with a name. Projections should have the same name of the properties of
		/// the results' class or an expression, in which case you should give it an alias with the name of a valid property.
		/// </summary>
		/// <param name='textFragment'>
		/// The textual part of the projection.
		/// </param>
		/// <param name='alias'>
		/// The alias of the projection (used with "AS alias")
		/// </param>
		public ProjectionFragment(string textFragment, string alias) : this(textFragment)
		{
			this.alias = alias;
		}

		/// <summary>
		/// Creates a projection with a name. Projections should have the same name of the properties of
		/// the results' class or an expression, in which case you should give it an alias with the name of a valid property.
		/// </summary>
		/// <param name='fragment'>
		/// The textual part of the projection.
		/// </param>
		/// <param name='alias'>
		/// The alias of the projection (used with "AS alias")
		/// </param>
		public ProjectionFragment(SqlFragment fragment, string alias) : this(fragment)
		{
			this.alias = alias;
		}
	}
}
