using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SqlBuilder
{
	/// <summary>
	/// Class that parses expression trees and generates a SqlFragment. This is part of our LINQ to SQL module.
	/// </summary>
	internal class SqlFragmentGeneratorTreeVisitor : ExpressionVisitor
	{
		private IDictionary<Type, string> TableEntities;

		/// <summary>
		/// The SqlFragment generated after visiting an expression.
		/// </summary>
		public SqlFragment Fragment { get; private set; }

		/// <summary>
		/// A collection of properties and fields visited, grouped per root entity type.
		/// </summary>
		public IDictionary<Type, IList<MemberInfo>> VisitedTypeProperties;

		public SqlFragmentGeneratorTreeVisitor() : base() {
			Fragment = new WhereCondition();
			TableEntities = new Dictionary<Type, string>(2);
			VisitedTypeProperties = new Dictionary<Type, IList<MemberInfo>>();
		}

		protected SqlFragmentGeneratorTreeVisitor(IDictionary<Type, string> table_entities) : this() {
			TableEntities = table_entities;
		}

		public void AddType(string table_alias, Type entity_type) {
			TableEntities.Add(entity_type, table_alias);
		}

		protected object GetMemberExpValue(MemberExpression node)
		{
			var objectMember = Expression.Convert(node, typeof(object));
			var getterLambda = Expression.Lambda<Func<object>>(objectMember);
			var getter = getterLambda.Compile();
			
			return getter();
		}

		/// <summary>
		/// Gets a column's name with the table name and a dot (.) prepended to it. 
		/// </summary>
		/// <param name="t">This parameter must have been added to this object previously, associated with a table alias.</param>
		protected virtual string GetColumnName(string propOrField, Type t)
		{
			if (TableEntities.ContainsKey(t) == false)
				throw new ArgumentException("Parameter t must have been added before.");

			string table_alias = TableEntities[t];

			string prefix = table_alias == null ? "" : (table_alias + ".");
			string column = propOrField.StartsWith("_") ? propOrField.Substring(1) : propOrField;
			
			return prefix + column;
		}

		/*
		public Expression Visit<T1, T2>(Expression e)
		{
			return Visit((Expression)e);
		}

		public Expression Visit<T>(Expression<Func<T, bool>> e)
		{
			return Visit((Expression)e);
		}

		public Expression Visit<T1, T2>(Expression<Func<T1, T2, bool>> e)
		{
			return Visit((Expression)e);
		}*/

		public override Expression Visit(Expression e)
		{
			return base.Visit(e);
		}

		/*
		protected virtual Expression VisitAndOr(BinaryExpression node) {
			var leftCondition = new WhereConditionGeneratorTreeVisitor(TableEntities);
			leftCondition.Visit(node.Left);
			var rightCondition = new WhereConditionGeneratorTreeVisitor(TableEntities);
			rightCondition.Visit(node.Right);

			switch (node.NodeType)
			{
				case ExpressionType.AndAlso:
					Fragment.And(leftCondition.Fragment).And(rightCondition.Fragment);
					break;
				case ExpressionType.OrElse:
					Fragment.Or(leftCondition.Fragment).Or(rightCondition.Fragment);
					break;
			}

			return node;
		}*/

		protected bool IsNullValue(Expression e) {
			return e.NodeType == ExpressionType.Constant && ((ConstantExpression)e).Value == null;
		}

		protected override Expression VisitBinary(BinaryExpression node)
		{
			// Treat ANDs and ORs especially:
			/*
			if (node.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.OrElse || node.NodeType == ExpressionType.Or) {
				return VisitAndOr(node);
			}*/

			// Before visiting, switch expressions if the left side is Constant and null (e.g. for null == identifier)
			if (IsNullValue(node.Left))
			{
				node = Expression.MakeBinary(node.NodeType, node.Right, node.Left);
			}

			// Visit the left expression, then emit operator related SQL and then visit the right expression
			Visit(node.Left);

			// Now the operator
			switch (node.NodeType) {
				case ExpressionType.Equal:
					// Check for == null
					if (IsNullValue(node.Right) == false)
						Fragment.AppendText("=");
					else
						Fragment.AppendText(" IS ");
					break;

				case ExpressionType.NotEqual:
					// Check for == null
					if (IsNullValue(node.Right) == false)
						Fragment.AppendText("!=");
					else
						Fragment.AppendText(" IS NOT ");
					break;

				case ExpressionType.GreaterThan:
					Fragment.AppendText(">");
					break;

				case ExpressionType.GreaterThanOrEqual:
					Fragment.AppendText(">=");
					break;

				case ExpressionType.LessThan:
					Fragment.AppendText("<");
					break;

				case ExpressionType.LessThanOrEqual:
					Fragment.AppendText("<=");
					break;
				
				#region Arithmetic operators
				case ExpressionType.Add:
					Fragment.AppendText("+");
					break;
				case ExpressionType.Subtract:
					Fragment.AppendText("-");
					break;
				case ExpressionType.Multiply:
					Fragment.AppendText("*");
					break;
				case ExpressionType.Divide:
					Fragment.AppendText("/");
					break;
				#endregion
			}

			// Visit the right expression and generate the SQL
			Visit(node.Right);

			return node;
		}

		protected Expression VisitContains(MethodCallExpression node)
		{
			//TODO: Check for 3 argument version of Contains and throw in that case..

			// We may have a few cases here..
			// 1. A multi valued column (such as an array) containing a parameter (is this operation contemplated in standard SQL? I don't think so)
			// 2. A parameter list containing a column
			// 3. The results of a subquery containing a parameter

			// Visit the should-be-contained expression
			Visit(node.Arguments[1]);
			Fragment.AppendText(" IN ");

			// In case of 2, we shouldn't visit this normally.. this is not a column but a collection allocated in memory..
			if (node.Arguments[0] is MemberExpression)
			{
				var rightExp = (MemberExpression) node.Arguments[0];

				// Case 1
				if (TableEntities.ContainsKey(rightExp.Member.DeclaringType))
				{
					throw new NotImplementedException();
					//Visit(rightExp);
				}

				// Case 2
				else
				{
					// Emit a parameter list!
					ParameterListFragment paramsFragment = new ParameterListFragment((System.Collections.IEnumerable)GetMemberExpValue(rightExp));
					Fragment.AppendFragment(paramsFragment);
				}
			}
			else
			{
				// Case 3
				throw new NotImplementedException();
			}

			return node;
		}

		protected override Expression VisitMethodCall (MethodCallExpression node)
		{
			// Check for Contains
			if (node.Method.Name == "Contains" && node.Method.DeclaringType == typeof(Enumerable))
			{
				return VisitContains(node);
			}

			return base.VisitMethodCall(node);
		}

		protected override Expression VisitMember(MemberExpression node)
		{
			Fragment.AppendText(GetColumnName(node.Member.Name, node.Member.DeclaringType));

			// Maintain the list of visited properties
			if (VisitedTypeProperties.ContainsKey(node.Member.DeclaringType) == false)
				VisitedTypeProperties.Add (node.Member.DeclaringType, new List<MemberInfo>());

			VisitedTypeProperties[node.Member.DeclaringType].Add(node.Member);


			return base.VisitMember(node);
		}

		protected override Expression VisitConditional(ConditionalExpression node)
		{
			return base.Visit(node.Test);
		}

		protected override Expression VisitConstant(ConstantExpression node)
		{
			if (node.Value == null)
				Fragment.AppendText("NULL");
			else
				Fragment.AppendParameter(node.Value);

			return base.VisitConstant(node);
		}
	}
}
