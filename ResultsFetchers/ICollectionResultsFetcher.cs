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
	// T is root entity type
	internal interface ICollectionResultsFetcher<T> {
		void CreateCollectionAndAssignToMember(T obj, SetValue memberSetter, out IList collection);
		void AddElementToCollection(IDataReader dr, IList collection);
	}

	// C as in IList<C> and T is root entity type
	internal interface ICollectionResultsFetcher<T, C> : ICollectionResultsFetcher<T> {
	}
}
