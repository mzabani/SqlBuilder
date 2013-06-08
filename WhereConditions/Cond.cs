using System;
using System.Collections;
using System.Linq.Expressions;
using SqlBuilder.Conditions;

namespace SqlBuilder
{
	/// <summary>
	/// A quicker way to instantiate typical WhereConditions via static methods.
	/// </summary>
	public static class Cond
	{
		#region Equality and range comparisons
		public static EqualTo EqualTo(string columnOrExpression, object @value) {
			return new EqualTo(columnOrExpression, @value);
		}

		public static EqualTo EqualTo(string leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) {
			return new EqualTo(leftSideColumnOrExpression, rightSideColumnOrExpression);
		}

		public static EqualTo EqualTo(SqlFragment leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) {
			return new EqualTo(leftSideColumnOrExpression, rightSideColumnOrExpression);
		}

		public static EqualTo<T> EqualTo<T>(Expression<Func<T, object>> lambdaGetter, object @value) {
			return new EqualTo<T>(lambdaGetter, value);
		}

		public static NotEqualTo NotEqualTo(string leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) {
			return new NotEqualTo(leftSideColumnOrExpression, rightSideColumnOrExpression);
		}

		public static NotEqualTo NotEqualTo(string leftSideColumnOrExpression, object @value) {
			return new NotEqualTo(leftSideColumnOrExpression, value);
		}
		public static NotEqualTo<T> NotEqualTo<T>(Expression<Func<T, object>> lambdaGetter, object @value) {
			return new NotEqualTo<T>(lambdaGetter, value);
		}

		public static LessThan LessThan(string leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) {
			return new LessThan(leftSideColumnOrExpression, rightSideColumnOrExpression);
		}

		public static LessThan LessThan(string leftSideColumnOrExpression, object @value) {
			return new LessThan(leftSideColumnOrExpression, value);
		}

		public static LessThan<T> LessThan<T>(Expression<Func<T, object>> lambdaGetter, object @value) {
			return new LessThan<T>(lambdaGetter, value);
		}

		public static LessOrEqual LessOrEqual(SqlFragment leftSideColumnOrExpression, object @value) {
			return new LessOrEqual(leftSideColumnOrExpression, value);
		}

		public static LessOrEqual LessOrEqual(SqlFragment leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) {
			return new LessOrEqual(leftSideColumnOrExpression, rightSideColumnOrExpression);
		}

		public static LessOrEqual LessOrEqual(string leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) {
			return new LessOrEqual(leftSideColumnOrExpression, rightSideColumnOrExpression);
		}

		public static LessOrEqual LessOrEqual(string leftSideColumnOrExpression, object @value) {
			return new LessOrEqual(leftSideColumnOrExpression, value);
		}

		public static LessOrEqual<T> LessOrEqual<T>(Expression<Func<T, object>> lambdaGetter, object @value) {
			return new LessOrEqual<T>(lambdaGetter, value);
		}

		public static GreaterThan GreaterThan(string leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) {
			return new GreaterThan(leftSideColumnOrExpression, rightSideColumnOrExpression);
		}

		public static GreaterThan GreaterThan(string leftSideColumnOrExpression, object @value) {
			return new GreaterThan(leftSideColumnOrExpression, value);
		}

		public static GreaterThan<T> GreaterThan<T>(Expression<Func<T, object>> lambdaGetter, object @value) {
			return new GreaterThan<T>(lambdaGetter, value);
		}

		public static GreaterOrEqual GreaterOrEqual(string leftSideColumnOrExpression, SqlFragment rightSideColumnOrExpression) {
			return new GreaterOrEqual(leftSideColumnOrExpression, rightSideColumnOrExpression);
		}

		public static GreaterOrEqual GreaterOrEqual(string leftSideColumnOrExpression, object @value) {
			return new GreaterOrEqual(leftSideColumnOrExpression, value);
		}

		public static GreaterOrEqual<T> GreaterOrEqual<T>(Expression<Func<T, object>> lambdaGetter, object @value) {
			return new GreaterOrEqual<T>(lambdaGetter, value);
		}

		public static Between Between(string leftSideColumnOrExpression, object min_val, object max_val) {
			return new Between(leftSideColumnOrExpression, min_val, max_val);
		}

		public static Between<T> Between<T>(Expression<Func<T, object>> lambdaGetter, object min_val, object max_val) {
			return new Between<T>(lambdaGetter, min_val, max_val);
		}
		#endregion

		public static InCondition In(string leftSideColumnOrExpression, IList values) {
			return new InCondition(leftSideColumnOrExpression, values);
		}

		public static InCondition In(SqlFragment leftSideColumnOrExpression, IList values) {
			return new InCondition(leftSideColumnOrExpression, values);
		}

		public static InCondition<T> In<T>(Expression<Func<T, object>> lambdaGetter, IList values) {
			return new InCondition<T>(lambdaGetter, values);
		}

		public static LikeCondition Like(string leftSideColumnOrExpression, string match) {
			return new LikeCondition(leftSideColumnOrExpression, match);
		}

		public static LikeCondition Like(SqlFragment leftSideColumnOrExpression, string match) {
			return new LikeCondition(leftSideColumnOrExpression, match);
		}

		public static LikeCondition<T> Like<T>(Expression<Func<T, object>> lambdaGetter, string match) {
			return new LikeCondition<T>(lambdaGetter, match);
		}

		#region IS NULL and IS NOT NULL conditions
		public static NullCondition IsNull(string leftSideColumnOrExpression) {
			return new NullCondition(leftSideColumnOrExpression, false);
		}

		public static NullCondition IsNull(SqlFragment leftSideColumnOrExpression) {
			return new NullCondition(leftSideColumnOrExpression, false);
		}

		public static NullCondition<T> IsNull<T>(Expression<Func<T, object>> lambdaGetter) {
			return new NullCondition<T>(lambdaGetter, false);
		}

		public static NullCondition IsNotNull(string leftSideColumnOrExpression) {
			return new NullCondition(leftSideColumnOrExpression, true);
		}

		public static NullCondition IsNotNull(SqlFragment leftSideColumnOrExpression) {
			return new NullCondition(leftSideColumnOrExpression, true);
		}

		public static NullCondition<T> IsNotNull<T>(Expression<Func<T, object>> lambdaGetter) {
			return new NullCondition<T>(lambdaGetter, true);
		}
		#endregion
	}
}
