using System;
using SqlBuilder;
using System.Linq.Expressions;

namespace SqlBuilder.Conditions
{
	public class CompositeComparison : WhereCondition
	{
		public CompositeComparison(SqlFragment leftSideWithFirstOperator, string operator2, object value2) 
		{
			this.AppendFragment(leftSideWithFirstOperator)
				.AppendText(operator2)
				.AppendParameter(value2);
		}

		public CompositeComparison(SqlFragment leftSideColumnOrExpression, string operator1, object value1, string operator2, object value2)
			: this(new SqlFragment(leftSideColumnOrExpression).AppendText(operator1).AppendParameter(value1), operator2, value2)
		{
		}

		public CompositeComparison(string leftSideColumnOrExpression, string operator1, object value1, string operator2, object value2)
			: this(new SqlFragment(leftSideColumnOrExpression), operator1, value1, operator2, value2)
		{
		}


	}
}
