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
		private Stack<WhereCondition> ConditionsStack;
		private Stack<ExpressionType> ExpTypeStack;
		private WhereCondition CurrentFragment
		{
			get
			{
				if (ConditionsStack.Count > 0)
					return ConditionsStack.Peek();
				else
					return Fragment;
			}
		}

		public WhereConditionGeneratorTreeVisitor(string rootTableAlias) : base()
		{
			RootTable = rootTableAlias;
			Fragment = new WhereCondition();
			ConditionsStack = new Stack<WhereCondition>();
			ExpTypeStack = new Stack<ExpressionType>();
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
			// Now create this new condition and push it onto the stack.. unless this is the first..
			WhereCondition thisCondition;
			if (ConditionsStack.Count == 0)
				thisCondition = Fragment;
			else
				thisCondition = new WhereCondition();

			ConditionsStack.Push(thisCondition);
			ExpTypeStack.Push(node.NodeType);

			Visit(node.Left);
			switch (node.NodeType)
			{
				case ExpressionType.AndAlso:
					thisCondition.AppendText(" AND ");
					break;

				case ExpressionType.OrElse:
					thisCondition.AppendText(" OR ");
					break;
			}
			Visit(node.Right);

			ConditionsStack.Pop();
			ExpTypeStack.Pop();

			return node;
		}

		protected override Expression VisitBinary(BinaryExpression node)
		{
			// Treat ANDs and ORs especially:
			if (node.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.OrElse || node.NodeType == ExpressionType.Or) {
				return VisitAndOr(node);
			}

			WhereCondition curFrag = CurrentFragment;

			// Visit the left expression, then emit operator related SQL and then visit the right expression
			Visit(node.Left);

			// Now the operator
			switch (node.NodeType) {
				case ExpressionType.Equal:
					curFrag.AppendText("=");
					break;

				case ExpressionType.NotEqual:
					curFrag.AppendText("!=");
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
			}

			// Visit the right expression and generate the SQL
			Visit(node.Right);

			return node;
		}

		protected override Expression VisitMember(MemberExpression node)
		{
			CurrentFragment.AppendText(GetColumnName(node.Member.Name));
			return base.VisitMember(node);
		}

		protected override Expression VisitConditional(ConditionalExpression node)
		{
			return base.Visit(node.Test);
		}

		protected override Expression VisitConstant(ConstantExpression node)
		{
			if (node.Value == null)
				CurrentFragment.AppendText(" NULL");
			else
				CurrentFragment.AppendParameter(node.Value);

			return base.VisitConstant(node);
		}
	}
}
