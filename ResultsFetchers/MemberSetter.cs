using System;
using System.Reflection;
using SqlBuilder.Reflection;
using System.Collections.Generic;
using System.Data;

namespace SqlBuilder
{
	internal class MemberSetter
	{
		/// <summary>
		/// Sets the non collection members of <paramref name="obj"/> as long as they are present in <paramref name="columnsNames"/>.
		/// </summary>
		/// <param name="obj">The object whose fields and properties will be set.</param>
		/// <param name="initialColumnIdx">The zero-starting index of the first column in <paramref name="columnsNames"/>.</param>
		/// <param name="columnsNames">A mapping of the queried projections to <paramref name="obj"/>'s members' names.</param>
		/// <param name="dr">The IDataReader from which columns' values will be retrieved.</param>
		public static void SetNonCollectionMembersOf<T>(T obj, IDictionary<string, string> columnsNames, IDataReader dr) {
			var setters = CachedTypeData.FetchSettersOf<T>();
			
			foreach (KeyValuePair<string, string> projectionAndMember in columnsNames)
			{
				string memberName = projectionAndMember.Value;
				string projectionName = projectionAndMember.Key;
				SetValue setter;
				try
				{
					setter = setters[memberName];
				}
				catch (KeyNotFoundException)
				{
					// Just skip for now.. Assume this column is not in this object but may be in another
					continue;
					//throw new InvalidOperationException("Object of class " + typeof(T2) + " does not possess a publicly settable member named \"" + memberName + "\"");
				}
				
				// Special case of collection. Skip!
				MemberInfo memberInfo = CachedTypeData.GetMemberInfo<T>(memberName);
				if (TypeUtils.IsGenericList(memberInfo.GetUnderlyingType()))
					continue;
				
				// Check for nulls
				int columnIdx = dr.GetOrdinal(projectionName);
				
				if (dr.IsDBNull(columnIdx))
				{
					setter(obj, null);
				}
				else
				{
					setter(obj, dr.GetValue(columnIdx));
				}
			}
		}
	}
}

