using System;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using SqlBuilder.Reflection;

namespace SqlBuilder
{
	public class RootQueryBuilder<T>
	{
		private string RootTable;
		private Type RootEntityType;
		private QueryBuilder Qb;

		#region Queried tables and references to them
		private int QueriedTableIdx;
		/// <summary>
		/// All queried tables, including the root table and joined entities. We need to maintain this to reference columns from all these tables
		/// in the methods in this property.
		/// </summary>
		private IDictionary<string, Type> QueriedTables;

		/// <summary>
		/// Gets a name for a queried table that has not been used for sure.
		/// </summary>
		/// <param name="table_name">The name of the table.</param>
		private string GetNewNameForQueriedTable(string table_name) {
			// Try to create a pretty name (the table's name itself) for it first
			if (QueriedTables.ContainsKey(table_name) == false)
				return table_name;

			// Otherwise, append an index number
			return table_name + ++QueriedTableIdx;
		}

		/// <summary>
		/// Adds a table as a referenced table.
		/// </summary>
		/// <param name="table">The alias or name of the table in the SQL query.</param>
		/// <param name="type">The type of the associated table.</param>
		private void AddQueriedTable(string tableNameOrAlias, Type type) {
			//TODO: Check if the name has already been added. Maybe the GetNewName..'s method should be in here all the time

			QueriedTables.Add(tableNameOrAlias, type);
		}
		#endregion

		/// <summary>
		/// Adds every publicly-settable field or property of the root entity to the select list, removing any beginning underscore from the name of the selected column.
		/// </summary>
		/// <returns>
		/// This RootQueryBuilder object.
		/// </returns>
		public RootQueryBuilder<T> Select(IEnumerable<Expression<Func<T, object>>> getterExprs) {
			// 1. Cache all setters for type T.
			IDictionary<string, SetValue> setters = CachedTypeData.FetchSettersOf<T>();
			
			// 2. Add "RootTable"."propOrFieldName" to the select list or just "propOrFieldName" if table_alias is null for every
			//    field and property returned in getterExprs
			foreach (Expression<Func<T, object>> getterExpr in getterExprs)
			{
				string propOrFieldName = ExpressionTreeHelper.GetPropOrFieldNameFromLambdaExpr<T>(getterExpr);
				
				// Only if there is a public setter for this property or if the field is public
				if (setters.ContainsKey(propOrFieldName) == false)
					continue;
				
				// Remove a possible underscore before adding it to selection
				if (propOrFieldName[0] == '_')
					propOrFieldName = propOrFieldName.Substring(1);
				
				string select_projection = RootTable + "." + propOrFieldName;
				Qb.Select(select_projection);
			}

			return this;
		}
		
		/// <summary>
		/// Adds every publicly-settable field or property in <paramref name="getterExprs"/> to the select list, removing any leading underscore from the name of the selected column.
		/// </summary>
		/// <returns>
		/// This RootQueryBuilder object.
		/// </returns>
		public RootQueryBuilder<T> Select(params Expression<Func<T, object>>[] getterExprs) {
			return Select(getterExprs.AsEnumerable());
		}

		/// <summary>
		/// Adds every publicly-settable field or property in type <typeref name="T"/> to the select list, removing any leading underscore from the name of the selected column.
		/// </summary>
		/// <returns>
		/// This RootQueryBuilder object.
		/// </returns>
		public RootQueryBuilder<T> SelectAllColumns() {
			// 1. Go after all the public settable properties and public fields in type T
			IDictionary<string, SetValue> setters = CachedTypeData.FetchSettersOf<T>();
			
			// 2. Add all of them to the select list!
			foreach (string propOrField in setters.Select(x => x.Key))
			{
				string selectableName = propOrField;
				if (selectableName[0] == '_')
					selectableName = propOrField.Substring(1);

				// Just to have pretty SQL when there is only one table queried
				if (QueriedTables.Count == 1)
					Qb.Select(selectableName);
				else
					Qb.Select(RootTable + "." + selectableName);
			}
			
			return this;
		}

		public RootQueryBuilder<T> Where(Expression<Func<T, bool>> whereExp) {
			var condBuilder = new WhereConditionGeneratorTreeVisitor();
			condBuilder.AddType(RootTable, RootEntityType);
			condBuilder.Visit(whereExp);
			WhereCondition condition = condBuilder.Fragment;
			Qb.Where(condition);
			return this;
		}

		/// <summary>
		/// ANDs a condition to the query. The caller must watch out for names of tables and properties.
		/// </summary>
		/// <param name="cond">A fragment to be inserted in the WHERE clause of the query.</param>
		public RootQueryBuilder<T> Where(SqlFragment cond) {
			Qb.Where(cond);
			return this;
		}

		#region Joining tables
		/// <summary>
		/// Joins a table that has already been queried or joined, of alias <paramref name="already_queried_table_alias"/> and represented by type <typeparamref name="JT1" />, to the table of name <paramref name="joined_table_name"/>, represented by type <typeparamref name="JT2" />.
		/// The join condition is the equality of columns associated to the properties expressed in <paramref name="alreadyQueriedTableColumnGetterExpr"/> and <paramref name="newlyJoinedTableColumnGetterExpr"/>.
		/// This method allows joining a table represented by a type more than once (through their different aliases).
		/// </summary>
		/// <param name="already_queried_table_alias">The alias of a table that has already been queried/joined.</param>
		/// <param name="joined_table_name">The real name of the table to be joined, as it is in the database.</param>
		/// <param name="joinType">The type of join (inner, outer etc.)</param>
		/// <param name="joinedTableAlias">An alias that has been defined for the joined table. Keep this in case you want to join this table to another (instead of joining the root table to something),</param>
		/// <typeparam name="JT1">The type that represents the table that has already been joined or queried.</typeparam>
		/// <typeparam name="JT2">The type of the newly joined table.</typeparam>
		public RootQueryBuilder<T> Join<JT1, JT2>(string already_queried_table_alias, string joined_table_name, Expression<Func<JT1, object>> alreadyQueriedTableColumnGetterExpr, Expression<Func<JT2, object>> newlyJoinedTableColumnGetterExpr, JoinType joinType, out string joined_table_alias)
		{
			if (QueriedTables.ContainsKey(already_queried_table_alias) == false)
				throw new InvalidOperationException("The table alias \"" + already_queried_table_alias + "\" has not been queried prior to this method's invocation.");
			else if (QueriedTables[already_queried_table_alias] != typeof(JT1))
				throw new InvalidOperationException("The type \"" + typeof(JT1) + "\" does not match the type of the table whose alias is \"" + already_queried_table_alias + "\" (" + QueriedTables[already_queried_table_alias] + ").");
			
			// Now that we know who is new, join'em!
			joined_table_alias = GetNewNameForQueriedTable(joined_table_name);
			AddQueriedTable(joined_table_alias, typeof(JT2));
			string column1 = already_queried_table_alias + "." + ExpressionTreeHelper.GetPropOrFieldNameFromLambdaExpr(alreadyQueriedTableColumnGetterExpr);
			string column2 = joined_table_alias + "." + ExpressionTreeHelper.GetPropOrFieldNameFromLambdaExpr(newlyJoinedTableColumnGetterExpr);
			
			Qb.Join(joined_table_name, column1, column2, joinType);
			
			return this;
		}

		/// <summary>
		/// Joins a table that has already been queried or joined, represented by type <typeparamref name="JT1" />, to the table of name <paramref name="joined_table_name"/>, represented by type <typeparamref name="JT2" />.
		/// The join condition is the equality of columns associated to the properties expressed in <paramref name="alreadyQueriedTableColumnGetterExpr"/> and <paramref name="newlyJoinedTableColumnGetterExpr"/>.
		/// The table represented by type <typeparamref name="JT1" /> can't have been queried/joined more than once through this method. If you want to do that, look for <see cref="???"/>.
		/// </summary>
		/// <param name="joined_table_name">The real name of the table to be joined, as it is in the database.</param>
		/// <param name="joinType">The type of join (inner, outer etc.)</param>
		/// <param name="joinedTableAlias">An alias that has been defined for the joined table. Keep this in case you want to join this table to another (instead of joining the root table to something),</param>
		/// <typeparam name="JT1">The type that represents the table that has already been joined or queried.</typeparam>
		/// <typeparam name="JT2">The type of the newly joined table.</typeparam>
		public RootQueryBuilder<T> Join<JT1, JT2>(string joined_table_name, Expression<Func<JT1, object>> alreadyQueriedTableColumnGetterExpr, Expression<Func<JT2, object>> newlyJoinedTableColumnGetterExpr, JoinType joinType, out string joined_table_alias)
		{
			// Get the alias of the already queried table
			string already_queried_table_alias = null;
			bool table_found = false;
			foreach (var queried_table in QueriedTables)
			{
				if (queried_table.Value == typeof(JT1))
				{
					table_found = true;
					already_queried_table_alias = queried_table.Key;
					break;
				}
			}
			
			if (!table_found)
				throw new InvalidOperationException("Please check that the table represented by type JT1 has already been queried or joined prior to calling this method.");

			// Now that we the table's alias, join'em!
			return Join<JT1, JT2>(already_queried_table_alias, alreadyQueriedTableColumnGetterExpr, newlyJoinedTableColumnGetterExpr, joinType, out joined_table_alias);
		}

		/// <summary>
		/// Joins a table that has already been queried or joined, represented by type <typeparamref name="JT1" />, to the table of name <paramref name="joined_table_name"/>, represented by type <typeparamref name="JT2" />.
		/// The join condition is the equality of columns associated to the properties expressed in <paramref name="alreadyQueriedTableColumnGetterExpr"/> and <paramref name="newlyJoinedTableColumnGetterExpr"/>.
		/// The table represented by type <typeparamref name="JT1" /> can't have been queried/joined more than once through this method. If you want to do that, look for <see cref="???"/>.
		/// </summary>
		/// <param name="joined_table_name">The real name of the table to be joined, as it is in the database.</param>
		/// <param name="joinType">The type of join (inner, outer etc.)</param>
		/// <typeparam name="JT1">The type that represents the table that has already been joined or queried.</typeparam>
		/// <typeparam name="JT2">The type of the newly joined table.</typeparam>
		public RootQueryBuilder<T> Join<JT1, JT2>(string joined_table_name, Expression<Func<JT1, object>> alreadyQueriedTableColumnGetterExpr, Expression<Func<JT2, object>> newlyJoinedTableColumnGetterExpr, JoinType joinType)
		{
			string trash;
			return Join<JT1, JT2>(joined_table_name, alreadyQueriedTableColumnGetterExpr, newlyJoinedTableColumnGetterExpr, joinType, out trash);
		}

		/// <summary>
		/// Joins the table of name <paramref name="joined_table_name"/>, represented by type <typeparamref name="JT" /> to the root entity, the join condition
		/// being the equality of columns associated to the properties expressed in <paramref name="rootEntityColumnGetterExpr"/> and <paramref name="joinedEntityColumnGetterExpr"/>.
		/// The joined table will receive an alias, which is set in <paramref name="joined_table_alias"/>, and will be equal to the table's name, unless this table has already been queried or joined before.
		/// </summary>
		/// <param name="joined_table_name">The real name of the table to be joined, as it is in the database.</param>
		/// <param name="joinType">The type of join (inner, outer etc.)</param>
		/// <param name="joinedTableAlias">An alias that has been defined for the joined table. Keep this in case you want to join this table to another (instead of joining the root table to something),</param>
		/// <typeparam name="JT">The type that represents the newly joined table.</typeparam>
		public RootQueryBuilder<T> Join<JT>(string joined_table_name, Expression<Func<T, object>> rootEntityColumnGetterExpr, Expression<Func<JT, object>> joinedEntityColumnGetterExpr, JoinType joinType, out string joined_table_alias)
		{
			return Join<T, JT>(joined_table_name, rootEntityColumnGetterExpr, joinedEntityColumnGetterExpr, joinType, out joined_table_alias);
		}

		/// <summary>
		/// Joins the table of name <paramref name="joined_table_name"/>, represented by type <typeparamref name="JT" /> to the root entity, the join condition
		/// being the equality of columns associated to the properties expressed in <paramref name="rootEntityColumnGetterExpr"/> and <paramref name="joinedEntityColumnGetterExpr"/>.
		/// </summary>
		/// <param name="joined_table_name">The real name of the table to be joined, as it is in the database.</param>
		/// <param name="joinType">The type of join (inner, outer etc.)</param>
		/// <typeparam name="JT">The type that represents the newly joined table.</typeparam>
		public RootQueryBuilder<T> Join<JT>(string joined_table_name, Expression<Func<T, object>> rootEntityColumnGetterExpr, Expression<Func<JT, object>> joinedEntityColumnGetterExpr, JoinType joinType)
		{
			string trash;
			return Join<T, JT>(joined_table_name, rootEntityColumnGetterExpr, joinedEntityColumnGetterExpr, joinType, out trash);
		}

		/// <summary>
		/// Joins the table of name <paramref name="joined_table_name"/>, represented by type <typeparamref name="JT" /> to the root entity, the join condition
		/// being expressed in <paramref name="joinCondition"/>.
		/// </summary>
		/// <param name="joined_table_name">The real name of the table to be joined, as it is in the database.</param>
		/// <param name="joinType">The type of join (inner, outer etc.)</param>
		/// <typeparam name="JT">The type that represents the newly joined table.</typeparam>
		public RootQueryBuilder<T> Join<JT>(string joined_table_name, Expression<Func<T, JT, bool>> joinCondition, JoinType joinType)
		{
			if (QueriedTables.ContainsKey(joined_table_name))
				throw new InvalidOperationException("This table has already been queried/joined. Please use a method that allows you to join to the same table more than once.");

			var conditionBuilder = new WhereConditionGeneratorTreeVisitor();
			conditionBuilder.AddType(RootTable, RootEntityType);
			conditionBuilder.AddType(joined_table_name, typeof(JT));
			conditionBuilder.Visit(joinCondition);
			WhereCondition condFragment = conditionBuilder.Fragment;
			Qb.Join(joined_table_name, condFragment, joinType);

			return this;
		}
		#endregion

		#region LIMIT and OFFSET
		public RootQueryBuilder<T> Skip(int amount) {
			Qb.Skip(amount);
			return this;
		}
		public RootQueryBuilder<T> Take(int amount) {
			Qb.Take(amount);
			
			return this;
		}
		#endregion
		
		#region Building the query
		public SqlFragment ToSqlFragment() {
			return Qb.ToSqlFragment();
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
		internal string ToSqlString(IDictionary<string, object> parameters, IDictionary<object, int> parametersIdx) {
			return Qb.ToSqlString(parameters, parametersIdx);
		}
		
		/// <summary>
		/// Return a string with both the SQL query and the parameters' values. Useful only for debugging
		/// </summary>
		/// <returns>
		/// The sql string.
		/// </returns>
		public string ToSqlString() {
			return Qb.ToSqlString();
		}
		
		internal IDbCommand ToSqlCommand(IDbConnection con) {
			return Qb.ToSqlCommand(con);
		}
		#endregion

		private RootQueryBuilder() {
			QueriedTables = new Dictionary<string, Type>();
			QueriedTableIdx = 1;
		}

		/// <summary>
		/// Helps build queries agains entity <typeparamref="T" /> in strongly-typed fashion, i.e. via Expressions.
		/// </summary>
		/// <param name="table">The table to be queried.</param>
		public RootQueryBuilder(string table) : this()
		{
			RootEntityType = typeof(T);
			RootTable = table;
			Qb = new QueryBuilder(table);
			AddQueriedTable(table, RootEntityType);
		}
	}
}
