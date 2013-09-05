using System;
using System.Reflection;

namespace SqlBuilder.Reflection
{
	internal static class MemberInfoExtensions
	{
		public static Type GetUnderlyingType(this MemberInfo member) {
			if (member is PropertyInfo)
				return ((PropertyInfo)member).PropertyType;

			else if (member is FieldInfo)
				return ((FieldInfo)member).FieldType;

			throw new ArgumentException("This MemberInfo has to be either a PropertyInfo or a FieldInfo");
		}
	}
}

