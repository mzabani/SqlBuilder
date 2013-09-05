using System;
using System.Collections.Generic;

namespace SqlBuilder.Reflection
{
	internal class TypeUtils
	{
		public static bool IsGenericList(Type type)
		{
			// Taken from SO: http://stackoverflow.com/questions/951536/how-do-i-tell-whether-a-type-implements-ilist
			if (type == null) {
				throw new ArgumentNullException("type");
			}
			foreach (Type @interface in type.GetInterfaces()) {
				if (@interface.IsGenericType) {
					if (@interface.GetGenericTypeDefinition() == typeof(ICollection<>)) {
						// if needed, you can also return the type used as generic argument
						return true;
					}
				}
			}
			return false;
		}
	}
}
