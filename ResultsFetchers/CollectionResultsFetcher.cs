using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using SqlBuilder.Reflection;
using System.Reflection;

namespace SqlBuilder
{
	internal class CollectionResultsFetcher<T, C> : ICollectionResultsFetcher<T, C>
		where T : new()
		where C : new()
	{
		private IDictionary<string, string> ProjectionMap;

		public CollectionResultsFetcher(IDictionary<string, string> projectionMap) {
			if (projectionMap == null)
				throw new ArgumentNullException("projectionMap");

			ProjectionMap = projectionMap;
		}

		public void CreateCollectionAndAssignToMember(T obj, SetValue memberSetter, out IList collection)
		{
			collection = new List<C>();

			memberSetter(obj, collection);
		}

		public void AddElementToCollection(IDataReader dr, IList collection)
		{
			C element = new C();

			// Sets the members
			MemberSetter.SetNonCollectionMembersOf(element, ProjectionMap, dr);

			// Add id to the collection
			collection.Add(element);
		}
	}
}
