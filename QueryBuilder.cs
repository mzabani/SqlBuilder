using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

namespace SqlBuilder
{	
	public class QueryBuilder<CommType>
			where CommType : IDbCommand, new()
	{
		private FromFragment tableOrSubquery;
		
		#region Internal Reflection Helper Methods
		private delegate void SetValue(Object obj, Object val);
		private delegate void PropSetValue(Object obj, Object val, Object[] index);
		private delegate object InvokeSetter(Object obj, Object[] parameters);
		private SetValue PropValueSetterDelegate(PropSetValue propSetter) {
			SetValue setValueFunc = (obj, val) => 
				{
					propSetter(obj, val, null);
				};
			
			return setValueFunc;
		}
		
		private static IDictionary<Type, IDictionary<string, SetValue>> propSetters = new Dictionary<Type, IDictionary<string, SetValue>>(1);
		
		/// <summary>
		/// Get the field and public property setters of type T and put them the cache, if not already there.
		/// </summary>
		private IDictionary<string, SetValue> FetchSettersOf<T>() {
			Type typeOfT = typeof(T);
			IDictionary<string, SetValue> setters;
			if (propSetters.ContainsKey(typeOfT))
			{
				setters = propSetters[typeOfT];
			}
			else
			{
				setters = new Dictionary<string, SetValue>(1);
				propSetters.Add(typeOfT, setters);
				var fields = typeOfT.GetFields();
				var props = typeOfT.GetProperties();
				
				foreach (var field in fields)
				{
					//Console.WriteLine("Found field {0}", field.Name);
					setters.Add(field.Name, field.SetValue);
				}
				
				foreach (var prop in props)
				{
					MethodInfo publicSetter = prop.GetSetMethod();
					if (publicSetter != null)
					{
						//Console.WriteLine("Found property {0} with public setter.", prop.Name);
						setters.Add(prop.Name, PropValueSetterDelegate(prop.SetValue));
					}
					else
					{
						//Console.WriteLine("Property {0} found bot not included (no public setter).", prop.Name);
					}						
				}
			}
			
			return setters;
		}
		#endregion
		
		#region Adding columns to the SELECT clause
		private IList<ProjectionFragment> selectedProjections;
		
		/// <summary>
		/// Adds one or more comma separated columns to the SELECT clause.
		/// </summary>
		/// <param name='columns'>
		/// One ore more comma separated columns.
		/// </param>
		public QueryBuilder<CommType> AddSelectColumn(string columns) {
			// If we detect an " AS " substring, the variables below will be helpful
			string[] aliasSeparator = new string[1] { " AS " };
			string[] splitParts = new string[2];
			
			// Iterate
			foreach (string col in columns.Split(','))
			{
				// We should detect " AS " automatically
				if (col.Contains(" AS "))
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
		
		public QueryBuilder<CommType> AddSelectColumn(ProjectionFragment selectFrag) {
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
		public QueryBuilder<CommType> RetrieveColumnsOf<T>() {			
			IDictionary<string, SetValue> setters = FetchSettersOf<T>();
			
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
		
		public QueryBuilder<CommType> ResetSelects() {
			this.selectedProjections = new List<ProjectionFragment>();
			
			return this;
		}
		#endregion
		
		#region Defining joins
		private IList<JoinedTable> joinedTables;
		
		public QueryBuilder<CommType> AddJoin(string table, string column1, string column2, JoinType joinType)
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
		public QueryBuilder<CommType> AddGroupedColumn(string columns) {
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
		public QueryBuilder<CommType> AddGroupedColumn(SqlFragment frag) {
			groupedColumns.Add(frag);
			
			return this;
		}
		public QueryBuilder<CommType> ResetGroups() {
			groupedColumns = new List<SqlFragment>();
			
			return this;
		}
		#endregion
		
		#region Defining WHERE conditions
		private WhereCondition whereCondition;
		
		public QueryBuilder<CommType> SetWhereCondition(WhereCondition whereCondition) {
			this.whereCondition = whereCondition;
			
			return this;
		}
		public QueryBuilder<CommType> AndWhereCondition(WhereCondition andCondition) {
			whereCondition = whereCondition.And(andCondition);
			
			return this;
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
		public QueryBuilder<CommType> AddOrderByColumn(string columns) {
			foreach (string col in columns.Split(','))
				orderByColumns.Add(new SqlFragment(col.Trim()));
			
			return this;
		}
		public QueryBuilder<CommType> AddOrderByColumn(SqlFragment frag) {
			orderByColumns.Add(frag);
			
			return this;
		}
		public QueryBuilder<CommType> ResetOrderBy() {
			orderByColumns = new List<SqlFragment>();
			
			return this;
		}
		#endregion
		
		#region LIMIT and OFFSET
		private int offset;
		private int limit;
		
		public QueryBuilder<CommType> Skip(int amount) {
			offset = amount;
			
			return this;
		}
		public QueryBuilder<CommType> Take(int amount) {
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
				
				sf.AppendTextFormatted("{0} ON {1}={2}", join.table, join.column1, join.column2);
			}
			
			// The WHERE clause, if any
			if (whereCondition != null)
			{
				SqlFragment whereFrag = whereCondition.ToSqlFragment();
				if (whereFrag != null)
				{
					sf.AppendText(" WHERE ")
					  .AppendFragment(whereFrag);
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
				sf.AppendTextFormatted(" OFFSET {0}", offset);
			}
			if (limit > 0)
			{
				sf.AppendTextFormatted(" LIMIT {0}", limit);
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
		/// An initialized IDictionary. Any parameters in this Sql fragment will be added to it.
		/// </param>
		public string ToSqlString(IDictionary<string, object> parameters) {
			if (selectedProjections.Count == 0)
				throw new Exception("No SELECT columns specified");
			
			StringBuilder sb = new StringBuilder(150);
			
			// The SELECT clause
			sb.Append("SELECT ");
			foreach (SqlFragment frag in selectedProjections)
			{
				sb.AppendFormat("{0}, ", frag.ToSqlString(parameters.Count, parameters));
			}
			sb.Remove(sb.Length - 2, 2);
			
			// The FROM clause
			sb.AppendFormat(" FROM {0}", tableOrSubquery.ToSqlString(parameters.Count, parameters));
			
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
				sb.AppendFormat(" WHERE {0}", whereCondition.ToSqlString(parameters.Count, parameters));
			}
			
			// The GROUP BY clause, if any
			if (groupedColumns.Count > 0)
			{
				sb.Append(" GROUP BY ");
				
				foreach (SqlFragment proj in groupedColumns)
				{
					sb.AppendFormat("{0}, ", proj.ToSqlString(parameters.Count, parameters));
				}
				sb.Remove(sb.Length - 2, 2);
			}
			
			// The ORDER BY clause, if any
			if (orderByColumns.Count > 0)
			{
				sb.AppendFormat(" ORDER BY ");
				
				foreach (SqlFragment frag in orderByColumns)
				{
					sb.AppendFormat(frag.ToSqlString(parameters.Count, parameters))
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
		public string ToSqlString() {
			IDictionary<string, object> trash = new Dictionary<string, object>();
			return ToSqlString(trash);
		}
		
		private IDbCommand ToSqlCommand(IDbConnection con) {
			IDictionary<string, object> parameters = new Dictionary<string, object>(10);
			string command = this.ToSqlString(parameters);
			
			IDbCommand com = new CommType();
			com.Connection = con;
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
		public IList<T> List<T>(IDbConnection con) where T : new() {
			// Get the fields and properties of type T and put their setter methods in the cache, if not already there
			IDictionary<string, SetValue> setters = FetchSettersOf<T>();
			
			// Execute the SQL command and create the list
			List<T> results = new List<T>(10);
			IDbCommand com = this.ToSqlCommand(con);
			
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
							continue;
						}
						catch (IndexOutOfRangeException)
						{
							throw new Exception("No field or property in the result type named " + propertyName + " exists.");
						}
						
						try
						{
							// Check for nulls
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
			
			return results;
		}
		
		public R ScalarResult<R>(IDbConnection con) {
			IDbCommand command = this.ToSqlCommand(con);
			return (R)command.ExecuteScalar();
		}
		#endregion
		
		private void InstantiateLists() {
			selectedProjections = new List<ProjectionFragment>();
			joinedTables = new List<JoinedTable>();
			groupedColumns = new List<SqlFragment>();
			orderByColumns = new List<SqlFragment>();
		}
		public QueryBuilder(string table)
		{
			this.tableOrSubquery = new FromFragment(table);
			
			InstantiateLists();
		}
		
		/// <summary>
		/// When querying with a subquery, be careful to include in the supplied fragment any parentheses and aliases that may be
		/// required by the underlying database. See <see cref="SqlBuilder.SubqueryFragment" /> for a class that provides facilities
		/// to achieve this.
		/// </summary>
		/// <param name='subquery'>
		/// The Sql Fragment that represents the subquery (everything that goes in the FROM clause)
		/// </param>
		public QueryBuilder(FromFragment subquery) {
			tableOrSubquery = subquery;
			
			InstantiateLists();
		}
	}
}
