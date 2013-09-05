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
	internal class ResultsFetcher<T> : IResultsFetcher<T>
		where T : new()
	{
		private IDictionary<string, string> RootEntityProjectionsMap;
		private IDictionary<string, ICollectionResultsFetcher<T>> FetchManyFetchers;

		public ResultsFetcher(IDictionary<string, string> rootEntityProjectionsMap) {
			RootEntityProjectionsMap = rootEntityProjectionsMap;
		}

		public void AddOneToManyCollection<C>(Expression<Func<T, IList<C>>> fetchManyExpr, IDictionary<string, string> projectionsMap)
			where C : new()
		{
			if (FetchManyFetchers == null)
				FetchManyFetchers = new Dictionary<string, ICollectionResultsFetcher<T>>();

			string memberName = ExpressionTreeHelper.GetPropOrFieldNameFromLambdaExpr(fetchManyExpr);
			ICollectionResultsFetcher<T> collectionFetcher = new CollectionResultsFetcher<T, C>(projectionsMap);
			FetchManyFetchers.Add(memberName, collectionFetcher);
		}

		/// <summary>
		/// Execute the query and returns a set with the results of type <typeparamref name="T"/>. This method DOES avoid duplicates
		/// of the root entity from being returned, no matter what kind of joins are going on, as long as GetHashCode and Equals are property implemented for <typeparamref name="T" />.
		/// </summary>
		/// <param name='com'>
		/// The IDbCommand to be executed.
		/// </param>
		/// <typeparam name='T'>
		/// The type of each result.
		/// </typeparam>
		public HashSet<T> AsSet(IDbCommand com)
		{
			return new HashSet<T>(AsEnumerable(com));
		}

		/// <summary>
		/// Execute the query and enumerates the results of type <typeparamref name="T"/>. This method DOES NOT avoid duplicates
		/// of the root entity from being returned if there is any join to a one-to-many relationed entity. Do not make this public,
		/// since enumerating results one by one will interfere with one-to-many fetching (by not having all elements of the collection
		/// for some yielded results).
		/// </summary>
		/// <param name='com'>
		/// The IDbCommand to be executed.
		/// </param>
		/// <typeparam name='T'>
		/// The type of each result.
		/// </typeparam>
		private IEnumerable<T> AsEnumerable(IDbCommand com)
		{
			// Get the fields and properties of type T and put their setter and getter methods in the cache, if not already there
			IDictionary<string, SetValue> rootEntitySetters = CachedTypeData.FetchSettersOf<T>();

			// To update collections of all returned root entities in one-to-many relations, we keep them here for easy access
			IDictionary<T, IDictionary<string, IList>> rootEntitiesAndAssociatedCollections = null;
			bool hasSomeFetchManyAssociation = FetchManyFetchers != null;
			if (hasSomeFetchManyAssociation)
				rootEntitiesAndAssociatedCollections = new Dictionary<T, IDictionary<string, IList>>();

			using (IDataReader dr = com.ExecuteReader())
			{
				while (dr.Read())
				{
					T temp = new T();

					// Set the fields/properties of the root object accordingly, skipping collections at first
					MemberSetter.SetNonCollectionMembersOf(temp, RootEntityProjectionsMap, dr);

					if (hasSomeFetchManyAssociation)
					{
						if (rootEntitiesAndAssociatedCollections.ContainsKey(temp) == false)
							rootEntitiesAndAssociatedCollections.Add(temp, new Dictionary<string, IList>());

						foreach (var collectionMemberFetcher in FetchManyFetchers)
						{
							string memberName = collectionMemberFetcher.Key;
							SetValue setter = rootEntitySetters[memberName];
							ICollectionResultsFetcher<T> collectionFetcher = collectionMemberFetcher.Value;
							IDictionary<string, IList> collectionsPerMemberName = rootEntitiesAndAssociatedCollections[temp];

							IList collection;
							if (collectionsPerMemberName.ContainsKey(memberName))
							{
								collection = collectionsPerMemberName[memberName];
								setter(temp, collection);
							}
							else
							{
								collectionFetcher.CreateCollectionAndAssignToMember(temp, setter, out collection);
								collectionsPerMemberName.Add(memberName, collection);
							}

							// Now add an element to the collection and set its properties
							collectionFetcher.AddElementToCollection(dr, collection);
						}
					}

					yield return temp;
				}
			}
		}
	
		/// <summary>
		/// Execute the query and enumerates the results of type <typeparamref name="T"/>. This method DOES NOT avoid duplicates
		/// of the root entity from being returned if there is any join to a one-to-many relationed entity.
		/// </summary>
		/// <param name='com'>
		/// The IDbCommand to be executed.
		/// </param>
		/// <typeparam name='T'>
		/// The type of each result.
		/// </typeparam>
		public List<T> List(IDbCommand com) {
			return new List<T>(AsEnumerable(com));
		}
	}
}
