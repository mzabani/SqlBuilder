using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;

using SqlBuilder.Reflection;
using SqlBuilder.Types;

namespace SqlBuilder
{	
	public class QueryBuilder {
		private FromFragment tableOrSubquery;
		
		#region Adding columns and expressions to the SELECT clause
		private IList<ProjectionFragment> selectedProjections;
		
		/// <summary>
		/// Adds one or more comma separated columns to the SELECT clause.
		/// </summary>
		/// <param name='columns'>
		/// One ore more comma separated columns.
		/// </param>
		public QueryBuilder Select(string columns) {
			// If we detect an " AS " substring, the variables below will be helpful
			string[] aliasSeparator = new string[1] { " AS " };
			string[] splitParts = new string[2];
			
			// Iterate
			foreach (string col in columns.Split(','))
			{
				// We should detect " AS " automatically
				if (col.ToLower().Contains(" as "))
				{
					splitParts = col.Split(aliasSeparator, StringSplitOptions.None);
					selectedProjections.Add(new ProjectionFragment(splitParts[0], splitParts[1]));
				}
				else
				{
					selectedProjections.Add(new ProjectionFragment(col.Trim()));
				}
			}
			
			return this;
		}
		
		public QueryBuilder Select(ProjectionFragment selectFrag) {
			selectedProjections.Add(selectFrag);
			
			return this;
		}
		
		/// <summary>
		/// Goes through the list of projections in the SELECT clause and removes the projections whose
		/// names are not similar to any of the names of T's public fields and properties.
		/// It also aliases similar names correctly, putting an underscore if the property/field starts with an underscore.
		/// </summary>
		/// <typeparam name='T'>
		/// The type whose properties are to be aliased and fetched.
		/// </typeparam>
		public QueryBuilder RemoveSelectionsNotOf<T>() {			
			IDictionary<string, SetValue> setters = ReflectionHelper.FetchSettersOf<T>();
			
			IList<ProjectionFragment> newSelects = new List<ProjectionFragment>();
			foreach (ProjectionFragment proj in selectedProjections)
			{
				string fqn = proj.GetName();
				
				// If the projection's name is not a property name, then..
				if (!setters.ContainsKey(fqn))
				{
					// 1. The property might begin with an underscore, check for that and alias the projection correctly
					if (setters.ContainsKey("_" + fqn))
					{
						proj.As("_" + fqn);
						
						newSelects.Add(proj);
						//Console.WriteLine("- Found {0}", proj.GetName());
					}
					
					// 2. The property does not begin with an underscore, don't add it to the new list (do nothing)
					else
					{
						//Console.WriteLine("- Couldn't find a property or field named {0} or _{0} with a public setter", fqn);
					}
				}
				
				// If it is a property name, it must be included in the new select list!
				else
				{
					newSelects.Add(proj);
					//Console.WriteLine("Found {0}", proj.GetName());
				}
			}
			
			// Renews the select list
			selectedProjections = newSelects;
			
			return this;
		}
		
		/// <summary>
		/// Adds every publicly-settable field or property to the select list, removing any beginning underscore from the name of the selected column.
		/// </summary>
		/// <returns>
		/// This QueryBuilder object.
		/// </returns>
		/// <param name='table_alias'>
		/// The table alias or name, if any. Useful in case of a query with joins.
		/// </param>
		/// <typeparam name='T'>
		/// The class with the properties to be selected.
		/// </typeparam>
		public QueryBuilder AddColumnsOf<T>(string table_alias, IEnumerable<Expression<Func<T, object>>> getterExprs) {
			// 1. Cache all setters for type T.
			IDictionary<string, SetValue> setters = ReflectionHelper.FetchSettersOf<T>();
			
			// 2. Add "table_alias"."propOrFieldName" to the select list or just "propOrFieldName" if table_alias is null for every
			//    field and property returned in getterExprs
			foreach (Expression<Func<T, object>> getterExpr in getterExprs)
			{
				string propOrFieldName = ExpressionTreeParser.GetPropOrFieldNameFromLambdaExpr<T>(getterExpr);
				
				// Only if there is a public setter for this property or if the field is public
				if (setters.ContainsKey(propOrFieldName) == false)
					continue;

				// Remove a possible underscore before adding it to selection
				if (propOrFieldName[0] == '_')
					propOrFieldName = propOrFieldName.Substring(1);

				string select_projection = String.IsNullOrWhiteSpace(table_alias) ? propOrFieldName : table_alias + "." + propOrFieldName;
				Select(select_projection);
			}
			
			
			return this;
		}

		/// <summary>
		/// Adds every publicly-settable field or property in <paramref name="getterExprs"/> to the select list, removing any leading underscore from the name of the selected column.
		/// </summary>
		/// <returns>
		/// This QueryBuilder object.
		/// </returns>
		/// <param name='table_alias'>
		/// The table alias or name, if any. Useful in case of a query with joins.
		/// </param>
		/// <typeparam name='T'>
		/// The class with the properties to be selected.
		/// </typeparam>
		public QueryBuilder AddColumnsOf<T>(string table_alias, params Expression<Func<T, object>>[] getterExprs) {
			return AddColumnsOf<T>(table_alias, getterExprs.AsEnumerable());
		}

		/// <summary>
		/// Adds every publicly-settable field or property in <paramref name="getterExprs"/> to the select list, removing any leading underscore from the name of the selected column.
		/// </summary>
		/// <returns>
		/// This QueryBuilder object.
		/// </returns>
		/// <typeparam name='T'>
		/// The class with the properties to be selected.
		/// </typeparam>
		public QueryBuilder AddColumnsOf<T>(IEnumerable<Expression<Func<T, object>>> getterExprs) {
			return AddColumnsOf<T>(null, getterExprs);
		}

		/// <summary>
		/// Adds every publicly-settable field or property in <paramref name="getterExprs"/> to the select list, removing any leading underscore from the name of the selected column.
		/// </summary>
		/// <returns>
		/// This QueryBuilder object.
		/// </returns>
		/// <typeparam name='T'>
		/// The class with the properties to be selected.
		/// </typeparam>
		public QueryBuilder AddColumnsOf<T>(params Expression<Func<T, object>>[] getterExprs) {
			return AddColumnsOf<T>(null, getterExprs.AsEnumerable());
		}

		/// <summary>
		/// Adds every publicly-settable field or property in type <typeref name="T"/> to the select list, removing any leading underscore from the name of the selected column.
		/// </summary>
		/// <returns>
		/// This QueryBuilder object.
		/// </returns>
		/// <typeparam name='T'>
		/// The class with the properties to be selected.
		/// </typeparam>
		public QueryBuilder AddColumnsOf<T>() {
			// 1. Go after all the public settable properties and public fields in type T
			IDictionary<string, SetValue> setters = ReflectionHelper.FetchSettersOf<T>();
			
			// 2. Add all of them to the select list!
			foreach (string propOrField in setters.Select(x => x.Key))
			{
				string selectableName = propOrField;
				if (selectableName[0] == '_')
					selectableName = propOrField.Substring(1);

				Select(selectableName);
			}
			
			return this;
		}
		
		public QueryBuilder ResetSelects() {
			this.selectedProjections = new List<ProjectionFragment>();
			
			return this;
		}
		#endregion
		
		#region Defining joins
		private IList<JoinedTable> joinedTables;
		
		public QueryBuilder Join(string table, string column1, string column2, JoinType joinType)
		{
			JoinedTable join = new JoinedTable(table, column1, column2, joinType);
			
			if (joinedTables.Any(x => x.Equals(join)))
				return this;
			
			joinedTables.Add(join);
			
			return this;
		}
		#endregion
		
		#region Adding columns to GROUP BY
		private IList<SqlFragment> groupedColumns;
		
		/// <summary>
		/// Adds one or more comma separated columns to the GROUP BY clause.
		/// </summary>
		/// <param name='columns'>
		/// One ore more comma separated columns.
		/// </param>
		public QueryBuilder GroupBy(string columns) {
			// If we detect an " AS " substring, the variables below will be helpful
			string[] aliasSeparator = new string[1] { " AS " };
			string[] splitParts = new string[2];
			
			// Iterate
			foreach (string col in columns.Split(','))
			{
				// We should detect " AS " automatically, and group by alias in this case
				if (col.Contains(" AS "))
				{
					splitParts = col.Split(aliasSeparator, StringSplitOptions.None);
					groupedColumns.Add(new SqlFragment(splitParts[0]));
				}
				else
				{
					groupedColumns.Add(new SqlFragment(col.Trim()));
				}
			}
			
			return this;
		}
		public QueryBuilder GroupBy(SqlFragment frag) {
			groupedColumns.Add(frag);
			
			return this;
		}
		public QueryBuilder ResetGroups() {
			groupedColumns = new List<SqlFragment>();
			
			return this;
		}
		#endregion
		
		#region Defining WHERE conditions
		private WhereCondition whereCondition;

		/// <summary>
		/// AND's the specified <paramref name="andCondition"/> to the QueryBuilder.
		/// </summary>
		/// <param name='andCondition'>
		/// The condition to be ANDed.
		/// </param>
		public QueryBuilder Where(SqlFragment andCondition) {
			if (whereCondition == null)
			{
				whereCondition = new WhereCondition(andCondition);
			}
			else
			{
				whereCondition.And(andCondition);
			}
			
			return this;
		}

		/// <summary>
		/// OR's the specified <paramref name="orCondition"/> to the QueryBuilder.
		/// </summary>
		/// <param name='orCondition'>
		/// The condition to be ORed.
		/// </param>
		public QueryBuilder Or(SqlFragment orCondition) {
			if (whereCondition == null)
			{
				return Where(orCondition);
			}
			else
			{
				whereCondition.Or(orCondition);
				
				return this;
			}
		}
		#endregion
		
		#region Defining ORDER BY columns
		private IList<SqlFragment> orderByColumns;
		
		/// <summary>
		/// Adds one or more comma separated columns to the ORDER BY clause.
		/// </summary>
		/// <param name='columns'>
		/// One ore more comma separated columns.
		/// </param>
		public QueryBuilder OrderBy(string columns) {
			foreach (string col in columns.Split(','))
				orderByColumns.Add(new SqlFragment(col.Trim()));
			
			return this;
		}
		public QueryBuilder OrderBy(SqlFragment frag) {
			orderByColumns.Add(frag);
			
			return this;
		}
		public QueryBuilder ResetOrderBy() {
			orderByColumns = new List<SqlFragment>();
			
			return this;
		}
		#endregion
		
		#region LIMIT and OFFSET
		private int offset;
		private int limit;
		
		public QueryBuilder Skip(int amount) {
			offset = amount;
			
			return this;
		}
		public QueryBuilder Take(int amount) {
			limit = amount;
			
			return this;
		}
		#endregion
		
		#region Building the query
		public SqlFragment ToSqlFragment() {
			if (selectedProjections.Count == 0)
				throw new Exception("No SELECT columns specified");
			
			SqlFragment sf = new SqlFragment();
			
			// The SELECT clause
			sf.AppendText("SELECT ");
			for (int i = 0; i < selectedProjections.Count; i++)
			{
				sf.AppendFragment(selectedProjections[i]);
				
				if (i < selectedProjections.Count - 1)
					sf.AppendText(", ");
			}
			
			// The FROM clause (it IS optional)
			if (tableOrSubquery != null)
			{
				sf.AppendText(" FROM ")
				  .AppendFragment(tableOrSubquery);
			}
			
			// The joins, if any
			foreach (JoinedTable join in joinedTables)
			{
				if (join.joinType == JoinType.InnerJoin)
				{
					sf.AppendText(" INNER JOIN ");
				}
				else if (join.joinType == JoinType.LeftOuterJoin)
				{
					sf.AppendText(" LEFT OUTER JOIN ");
				}
				
				sf.AppendText("{0} ON {1}={2}", join.table, join.column1, join.column2);
			}
			
			// The WHERE clause, if any
			if (whereCondition != null)
			{
				//SqlFragment whereFrag = whereCondition.ToSqlFragment();
				if (whereCondition != null)
				{
					sf.AppendText(" WHERE ")
					  .AppendFragment(whereCondition);
				}
			}
			
			// The GROUP BY clause, if any
			if (groupedColumns.Count > 0)
			{
				sf.AppendText(" GROUP BY ");
				
				for (int i = 0; i < groupedColumns.Count; i++)
				{
					sf.AppendFragment(groupedColumns[i]);
					
					if (i < groupedColumns.Count - 1)
					{
						sf.AppendText(", ");
					}
				}
			}
			
			// The ORDER BY clause, if any
			if (orderByColumns.Count > 0)
			{
				sf.AppendText(" ORDER BY ");
				
				for (int i = 0; i < orderByColumns.Count; i++)
				{
					sf.AppendFragment(orderByColumns[i]);
					
					if (i < orderByColumns.Count - 1)
					{
						sf.AppendText(", ");
					}
				}
			}
			
			// Offset and limit
			if (offset > 0)
			{
				sf.AppendText(" OFFSET {0}", offset);
			}
			if (limit > 0)
			{
				sf.AppendText(" LIMIT {0}", limit);
			}
			
			return sf;
		}
		
		/// <summary>
		/// Creates the SQL command string.
		/// </summary>
		/// <returns>
		/// The sql string itself.
		/// </returns>
		/// <param name='parameters'>
		/// An initialized IDictionary. Any parameters in this query will be added to it.
		/// </param>
		private string ToSqlString(IDictionary<string, object> parameters, IDictionary<object, int> parametersIdx) {
			if (selectedProjections.Count == 0)
				throw new Exception("No SELECT columns specified");
			
			StringBuilder sb = new StringBuilder(150);

			// The SELECT clause
			sb.Append("SELECT ");
			foreach (SqlFragment frag in selectedProjections)
			{
				sb.AppendFormat("{0}, ", frag.ToSqlString(parameters.Count, parameters, parametersIdx));
			}
			sb.Remove(sb.Length - 2, 2);
			
			// The FROM clause
			sb.AppendFormat(" FROM {0}", tableOrSubquery.ToSqlString(parameters.Count, parameters, parametersIdx));
			
			// The joins, if any
			foreach (JoinedTable join in joinedTables)
			{
				if (join.joinType == JoinType.InnerJoin)
				{
					sb.Append(" INNER JOIN ");
				}
				else if (join.joinType == JoinType.LeftOuterJoin)
				{
					sb.Append(" LEFT OUTER JOIN ");
				}
				
				sb.AppendFormat("{0} ON {1}={2}", join.table, join.column1, join.column2);
			}
			
			// The WHERE clause, if any
			if (whereCondition != null)
			{
				// Append the SQL fragment and add to the parameters dictionary
				sb.AppendFormat(" WHERE {0}", whereCondition.ToSqlString(parameters.Count, parameters, parametersIdx));
			}
			
			// The GROUP BY clause, if any
			if (groupedColumns.Count > 0)
			{
				sb.Append(" GROUP BY ");
				
				foreach (SqlFragment proj in groupedColumns)
				{
					sb.AppendFormat("{0}, ", proj.ToSqlString(parameters.Count, parameters, parametersIdx));
				}
				sb.Remove(sb.Length - 2, 2);
			}
			
			// The ORDER BY clause, if any
			if (orderByColumns.Count > 0)
			{
				sb.AppendFormat(" ORDER BY ");
				
				foreach (SqlFragment frag in orderByColumns)
				{
					sb.AppendFormat(frag.ToSqlString(parameters.Count, parameters, parametersIdx))
					  .Append(", ");
				}
				sb.Remove(sb.Length - 2, 2);
			}
			
			// Offset and limit
			if (offset > 0)
			{
				sb.AppendFormat(" OFFSET {0}", offset);
			}
			if (limit > 0)
			{
				sb.AppendFormat(" LIMIT {0}", limit);
			}

			return sb.ToString();
		}
		
		/// <summary>
		/// Return a string with both the SQL query and the parameters' values. Useful only for debugging
		/// </summary>
		/// <returns>
		/// The sql string.
		/// </returns>
		public string ToSqlString() {
			IDictionary<string, object> prms = new Dictionary<string, object>();
			IDictionary<object, int> prmsIdx = new Dictionary<object, int>();
			string ret = ToSqlString(prms, prmsIdx);
			
			ret += "\n";
			foreach (var prm in prms)
			{
				ret += String.Format("{0}={1}, ", prm.Key, prm.Value);
			}
			
			return ret;
		}
		
		private IDbCommand ToSqlCommand(IDbConnection con) {
			IDictionary<string, object> parameters = new Dictionary<string, object>(10);
			IDictionary<object, int> prmsIdx = new Dictionary<object, int>(10);
			string command = this.ToSqlString(parameters, prmsIdx);
			
			IDbCommand com = con.CreateCommand();
			com.CommandText = command;
			
			// Add the query's parameters
			foreach (var param in parameters)
			{
				IDbDataParameter com_param = com.CreateParameter();
				com_param.ParameterName = param.Key;
				com_param.Value = param.Value;
				com.Parameters.Add(com_param);
			}
			
			return com;
		}
		#endregion
		
		#region Returning the results
		/// <summary>
		/// Execute the query and return the results in a list of type <paramref name="T"/>.
		/// </summary>
		/// <param name='con'>
		/// The IDbConnection on which the query will be executed.
		/// </param>
		/// <typeparam name='T'>
		/// The type of each result.
		/// </typeparam>
		public List<T> List<T>(IDbConnection con) where T : new() {
			// Get the fields and properties of type T and put their setter methods in the cache, if not already there
			IDictionary<string, SetValue> setters = ReflectionHelper.FetchSettersOf<T>();

			// Execute the SQL command and create the list
			List<T> results = new List<T>(10);
			using (IDbCommand com = this.ToSqlCommand(con))
			{
				using (IDataReader dr = com.ExecuteReader())
				{
					while (dr.Read())
					{
						T temp = new T();
						
						// Get the setters in the same order of the columns in selectColumns to set the fields/properties accordingly
						int i = 0;
						foreach (ProjectionFragment proj in selectedProjections)
						{
							SetValue setter;
							
							// GetName() should return the correct property name, no matter if it is an aliased expression or a column
							string propertyName = proj.GetName();
							
							try
							{
								setter = setters[propertyName];
							}
							catch (KeyNotFoundException)
							{
								//Console.WriteLine("Property named {0} not defined in {1}", propertyName, typeof(T).ToString());
								i++;
								continue;
							}
							catch (IndexOutOfRangeException)
							{
								throw new Exception("No field or property in the result type named " + propertyName + " exists.");
							}
							
							try
							{
								// Check for nulls
								//Console.WriteLine("SETTING value of {0} to {1}", propertyName, dr[i]);
								if (dr.IsDBNull(i))
								{
									setter(temp, null);
								}
								else
								{
									setter(temp, dr[i]);
								}
								
								i++;
							}
							catch (IndexOutOfRangeException)
							{
								throw new Exception("One of the selected columns does not exist in the database. Maybe there is a typo in the selected columns?");
							}
						}
						
						results.Add(temp);
					}
				}
			}

			return results;
		}

		public List<T> List<T, C>(IDbConnection con, Expression<Func<T, IList<C>>> fetchManyExpr)
			where T : class, new()
			where C : new()
		{
			// Get ready to create instances of T only when we hit a different hashcode, while
			// we fetch every element whose columns are in the object type "referenced" by fetchManyExpr.
			// Before that, cache the setters of this object type
			string collectionPropOrFieldName = ExpressionTreeParser.GetPropOrFieldNameFromLambdaExpr<T, C>(fetchManyExpr);

			// Get the fields and properties of type T and put their setter methods in the cache, if not already there
			IDictionary<string, SetValue> setters = ReflectionHelper.FetchSettersOf<T>();
			//PropertyInfo collectionProp = typeof(T).GetProperty(collectionPropOrFieldName, BindingFlags.Public);
			//FieldInfo collectionField = typeof(T).GetField(collectionPropOrFieldName);
			if (setters.ContainsKey(collectionPropOrFieldName) == false)
			{
				throw new Exception("Can't set the value of " + collectionPropOrFieldName + ". Make sure it is either a public field or a property with a public setter");
			}

			// Now the setters of the collection type members
			IDictionary<string, SetValue> collectionMembersSetters = null;
			if (typeof(C).IsClass)
			{
				collectionMembersSetters = ReflectionHelper.FetchSettersOf<C>();
			}
			
			// Execute the SQL command and create the list
			List<T> results = new List<T>(10);

			using (IDbCommand com = this.ToSqlCommand(con))
			{
				using (IDataReader dr = com.ExecuteReader())
				{
					List<C> collection = null;
					int? lastHashCode = null;
					while (dr.Read())
					{
						T temp = new T();

						// Set the fields/properties of the root object accordingly
						foreach (ProjectionFragment proj in selectedProjections)
						{
							SetValue setter;
							
							// GetName() should return the correct property or field name, no matter if it is an aliased expression or a column
							string propertyName = proj.GetName();

							// We shouldn't do anything if this is the collection property/field
							if (propertyName == collectionPropOrFieldName)
								continue;
							
							try
							{
								setter = setters[propertyName];
							}
							catch (KeyNotFoundException)
							{
								//Console.WriteLine("Property named {0} not defined in {1}", propertyName, typeof(T).ToString());
								continue;
							}
							catch (IndexOutOfRangeException)
							{
								throw new Exception("No field or property in the result type named " + propertyName + " exists.");
							}
							
							try
							{
								// Check for nulls
								int ordinal = dr.GetOrdinal(propertyName);
								//Console.WriteLine("SETTING value of {0} to {1} to form basic object", propertyName, dr[ordinal]);

								if (dr.IsDBNull(ordinal))
								{
									setter(temp, null);
								}
								else
								{
									setter(temp, dr[ordinal]);
								}
							}
							catch (IndexOutOfRangeException)
							{
								throw new Exception("One of the selected columns does not exist in the database. Maybe there is a typo in the selected columns?");
							}
						}

						// After building temp, check if GetHashCode has changed, in which case we create
						// a new List<C> to this new object, adding it to the results (we don't add it to results otherwise).
						// Whether the list is new or not, we add one object of type C to this lastTempRef
						if (temp.GetHashCode() != lastHashCode)
						{
							results.Add(temp);
							lastHashCode = temp.GetHashCode();

							collection = new List<C>();
							setters[collectionPropOrFieldName](temp, collection);
						}

						C collectionElement = new C();
						bool add_collectionElement = false;
						foreach (ProjectionFragment proj in selectedProjections)
						{
							SetValue setter = null;
							
							// GetName() should return the correct property or field name, no matter if it is an aliased expression or a column
							string propertyName = proj.GetName();

							// Don't re-set values in case they have already been set
							if (setters.ContainsKey(propertyName) == true)
							{
								if (collectionMembersSetters != null)
									continue;
								else if (propertyName != collectionPropOrFieldName)
									continue;
							}

							try
							{
								if (collectionMembersSetters != null)
								{
									setter = collectionMembersSetters[propertyName];
								}
							}
							catch (KeyNotFoundException)
							{
								//Console.WriteLine("Property named {0} not defined in {1}", propertyName, typeof(C).ToString());
								continue;
							}
							catch (IndexOutOfRangeException)
							{
								throw new Exception("No field or property in the collection type named " + propertyName + " exists.");
							}
							
							try
							{
								// Check for nulls.
								// Beware of C being a value type, in which case collectionMembersSetters == null.
								int ordinal = dr.GetOrdinal(propertyName);

								if (dr.IsDBNull(ordinal))
								{
									if (collectionMembersSetters != null)
									{
										setter(collectionElement, null);
										add_collectionElement = true;
									}
									else if (collectionPropOrFieldName == propertyName)
									{
										//add_collectionElement = false;
									}
								}
								else
								{
									if (collectionMembersSetters != null)
									{
										setter(collectionElement, dr[ordinal]);
										add_collectionElement = true;
									}
									else if (collectionPropOrFieldName == propertyName)
									{
										collectionElement = (C) dr[ordinal];
										add_collectionElement = true;
									}
								}
							}
							catch (IndexOutOfRangeException)
							{
								throw new Exception("One of the selected columns does not exist in the database. Maybe there is a typo in the selected columns?");
							}
						}

						// Add this element to the collection (or not)
						if (add_collectionElement == true)
							collection.Add(collectionElement);
					}
				}
			}
			
			return results;
		}

		public R ScalarResult<R>(IDbConnection con) {
			IDbCommand command = this.ToSqlCommand(con);
			return (R)command.ExecuteScalar();
		}
		#endregion

		#region Constructors
		private QueryBuilder()
		{
			selectedProjections = new List<ProjectionFragment>();
			joinedTables = new List<JoinedTable>();
			groupedColumns = new List<SqlFragment>();
			orderByColumns = new List<SqlFragment>();
		}

		public QueryBuilder(string table) : this()
		{
			this.tableOrSubquery = new FromFragment(table);
		}
		
		/// <summary>
		/// When querying with a subquery, be careful to include in the supplied fragment any parentheses and aliases that may be
		/// required by the underlying database. See <see cref="SqlBuilder.SubqueryFragment" /> for a class that provides facilities
		/// to achieve this.
		/// </summary>
		/// <param name='subquery'>
		/// The Sql Fragment that represents the subquery (everything that goes in the FROM clause)
		/// </param>
		public QueryBuilder(FromFragment subquery) : this() {
			tableOrSubquery = subquery;
		}
		#endregion
	}
}
