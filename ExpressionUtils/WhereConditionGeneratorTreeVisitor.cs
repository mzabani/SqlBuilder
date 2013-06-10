using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SqlBuilder
{
	/// <summary>
	/// Class that parses expression trees and generates a WhereCondition. This is part of our LINQ to SQL module.
	/// </summary>
	public class WhereConditionGeneratorTreeVisitor<T> : ExpressionVisitor
	{
		private string RootTable;
		private string GetColumnName(string propOrField)
		{
			string prefix = RootTable == null ? "" : (RootTable + ".");
			string column = propOrField.StartsWith("_") ? propOrField.Substring(1) : propOrField;

			return prefix + propOrField;
		}

		public WhereCondition Fragment { get; private set; }

		public WhereConditionGeneratorTreeVisitor(string rootTableAlias) : base()
		{
			RootTable = rootTableAlias;
			Fragment = new WhereCondition();
		}

		public Expression Visit(Expression<Func<T, bool>> e)
		{
			return Visit((Expression)e);
		}

		public override Expression Visit(Expression e)
		{
			return base.Visit(e);
		}

		private Expression VisitAndOr(BinaryExpression node) {
			var leftCondition = new WhereConditionGeneratorTreeVisitor<T>(RootTable);
			leftCondition.Visit(node.Left);
			var rightCondition = new WhereConditionGeneratorTreeVisitor<T>(RootTable);
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
		}

		private bool IsNullValue(Expression e) {
			return e.NodeType == ExpressionType.Constant && ((ConstantExpression)e).Value == null;
		}

		protected override Expression VisitBinary(BinaryExpression node)
		{
			// Treat ANDs and ORs especially:
			if (node.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.OrElse || node.NodeType == ExpressionType.Or) {
				return VisitAndOr(node);
			}

			WhereCondition curFrag = Fragment;

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
						curFrag.AppendText("=");
					else
						Fragment.AppendText(" IS ");
					break;

				case ExpressionType.NotEqual:
					// Check for == null
					if (IsNullValue(node.Right) == false)
						curFrag.AppendText("!=");
					else
						curFrag.AppendText(" IS NOT ");
					break;

				case ExpressionType.GreaterThan:
					curFrag.AppendText(">");
					break;

				case ExpressionType.GreaterThanOrEqual:
					curFrag.AppendText(">=");
					break;

				case ExpressionType.LessThan:
					curFrag.AppendText("<");
					break;

				case ExpressionType.LessThanOrEqual:
					curFrag.AppendText("<=");
					break;
				
				#region Arithmetic operators
				case ExpressionType.Add:
					curFrag.AppendText("+");
					break;
				case ExpressionType.Subtract:
					curFrag.AppendText("-");
					break;
				case ExpressionType.Multiply:
					curFrag.AppendText("*");
					break;
				case ExpressionType.Divide:
					curFrag.AppendText("/");
					break;
				#endregion
			}

			// Visit the right expression and generate the SQL
			Visit(node.Right);

			return node;
		}

		protected override Expression VisitMember(MemberExpression node)
		{
			Fragment.AppendText(GetColumnName(node.Member.Name));
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

	/// <summary>
	/// Class that parses expression trees and generates a WhereCondition. This is part of our LINQ to SQL module.
	/// </summary>
	public class WhereConditionGeneratorTreeVisitor<T1, T2> : ExpressionVisitor
	{
		private string Table1;
		private Type Type1;
		private string Table2;
		private Type Type2;
		private string GetColumnName(string propOrField, Type T)
		{
			string table_name = null;
			if (T == Type1)
				table_name = Table1; 
			else if (T == Type2)
				table_name = Table2;
			else
				throw new InvalidOperationException("Type T must be either T1 or T2");

			string prefix = table_name == null ? "" : (table_name + ".");
			string column = propOrField.StartsWith("_") ? propOrField.Substring(1) : propOrField;
			
			return prefix + propOrField;
		}
		
		public WhereCondition Fragment { get; private set; }
		
		public WhereConditionGeneratorTreeVisitor(string table1_alias, string table2_alias) : base()
		{
			Table1 = table1_alias;
			Type1 = typeof(T1);
			Table2 = table2_alias;
			Type2 = typeof(T2);
			Fragment = new WhereCondition();
		}
		
		public Expression Visit(Expression<Func<T1, T2, bool>> e)
		{
			return Visit((Expression)e);
		}
		
		public override Expression Visit(Expression e)
		{
			return base.Visit(e);
		}
		
		private Expression VisitAndOr(BinaryExpression node) {
			var leftCondition = new WhereConditionGeneratorTreeVisitor<T1, T2>(Table1, Table2);
			leftCondition.Visit(node.Left);
			var rightCondition = new WhereConditionGeneratorTreeVisitor<T1, T2>(Table1, Table2);
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
		}
		
		private bool IsNullValue(Expression e) {
			return e.NodeType == ExpressionType.Constant && ((ConstantExpression)e).Value == null;
		}
		
		protected override Expression VisitBinary(BinaryExpression node)
		{
			// Treat ANDs and ORs especially:
			if (node.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.OrElse || node.NodeType == ExpressionType.Or) {
				return VisitAndOr(node);
			}
			
			WhereCondition curFrag = Fragment;
			
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
					curFrag.AppendText("=");
				else
					Fragment.AppendText(" IS ");
				break;
				
			case ExpressionType.NotEqual:
				// Check for == null
				if (IsNullValue(node.Right) == false)
					curFrag.AppendText("!=");
				else
					curFrag.AppendText(" IS NOT ");
				break;
				
			case ExpressionType.GreaterThan:
				curFrag.AppendText(">");
				break;
				
			case ExpressionType.GreaterThanOrEqual:
				curFrag.AppendText(">=");
				break;
				
			case ExpressionType.LessThan:
				curFrag.AppendText("<");
				break;
				
			case ExpressionType.LessThanOrEqual:
				curFrag.AppendText("<=");
				break;

				#region Arithmetic operators
			case ExpressionType.Add:
				curFrag.AppendText("+");
				break;
			case ExpressionType.Subtract:
				curFrag.AppendText("-");
				break;
			case ExpressionType.Multiply:
				curFrag.AppendText("*");
				break;
			case ExpressionType.Divide:
				curFrag.AppendText("/");
				break;

				#endregion
			}
			
			// Visit the right expression and generate the SQL
			Visit(node.Right);
			
			return node;
		}
		
		protected override Expression VisitMember(MemberExpression node)
		{
			Fragment.AppendText(GetColumnName(node.Member.Name, node.Member.DeclaringType));
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
