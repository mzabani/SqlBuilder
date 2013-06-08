using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace SqlBuilder
{
	static class ExpressionTreeHelper
	{
		private static string GetPropOrFieldNameFromMemberAccessExpression(MemberExpression expr) {
			return expr.Member.Name;
		}
		
		public static string GetPropOrFieldNameFromLambdaExpr<T>(Expression<Func<T, object>> getterExpr)
		{
			LambdaExpression le = getterExpr as LambdaExpression;
			
			if (le.Body.NodeType == ExpressionType.Convert)
			{
				UnaryExpression ue = le.Body as UnaryExpression;
				
				return GetPropOrFieldNameFromMemberAccessExpression((MemberExpression)ue.Operand);
			}
			
			if (le.Body.NodeType == ExpressionType.MemberAccess)
			{
				return GetPropOrFieldNameFromMemberAccessExpression((MemberExpression)le.Body);
			}
			
			return "";
		}

		public static string GetPropOrFieldNameFromLambdaExpr<T, C>(Expression<Func<T, IList<C>>> getterExpr)
		{
			LambdaExpression le = getterExpr as LambdaExpression;
			
			if (le.Body.NodeType == ExpressionType.Convert)
			{
				UnaryExpression ue = le.Body as UnaryExpression;
				
				return GetPropOrFieldNameFromMemberAccessExpression((MemberExpression)ue.Operand);
			}
			
			if (le.Body.NodeType == ExpressionType.MemberAccess)
			{
				return GetPropOrFieldNameFromMemberAccessExpression((MemberExpression)le.Body);
			}
			
			return "";
		}
	}
}
