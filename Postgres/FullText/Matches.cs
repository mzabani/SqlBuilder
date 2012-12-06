using System;
using System.Linq.Expressions;

namespace SqlBuilder.Postgres
{
	public static class FullText
	{
		public static WhereCondition Match(TsVector tsVector, TsQuery tsQuery)
		{
			return new SqlBuilder.Conditions.SimpleComparison(tsVector, "@@", tsQuery);
		}

		public static WhereCondition Match(TsVector tsVector, string configuration, string queryString, bool isColumn)
		{
			return Match(tsVector, new TsQuery(configuration, queryString, !isColumn));
		}
	}
}
