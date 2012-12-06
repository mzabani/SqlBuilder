using System;

namespace SqlBuilder
{
	public static class ExtensionMethods
	{
		public static SqlFragment ToSqlFragment(this string str) {
			return new SqlFragment(str);
		}
	}
}

