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
	internal interface IResultsFetcher<T> {
		void AddOneToManyCollection<C>(Expression<Func<T, IList<C>>> fetchManyExpr, IDictionary<string, string> projectionMap)
			where C : new();
	}
}
