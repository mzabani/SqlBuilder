using System;
using SqlBuilder;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using Tests.Mocking;
using Tests.Entities;

namespace Tests
{
	[TestFixture()]
	public class Test
	{
		[Test()]
		public void TestNullComparisonToTheLeftAndToTheRight()
		{
			var rqb1 = new RootQueryBuilder<Store>("table");
			var rqb2 = new RootQueryBuilder<Store>("table");
			rqb1.Where(x => x.name == null).SelectAllColumns();
			rqb2.Where(x => null == x.name).SelectAllColumns();
			Assert.AreEqual(rqb1.ToSqlString(), rqb2.ToSqlString());

			rqb2.Where(x => x.items != null);
			rqb1.Where(x => null != x.items);
			Assert.AreEqual(rqb1.ToSqlString(), rqb2.ToSqlString());
		}
		
		[Test()]
		public void TestMockedIDataReader() {
			var readerBuilder = new ListStoresJoinedToStoreItemsCommand();
			
			// Adds the query results (two lines, one store)
			readerBuilder.AddQueryResult(new Store {
				storeid = 1,
				name = "Açougue do Zabani"
			},
			new StoreItem {
				storeid = 1,
				name = "Frango Assado",
				itemid = 34,
				amount_available = 7
			});
			readerBuilder.AddQueryResult(new Store {
				storeid = 1,
				name = "Açougue do Zabani"
			},
			new StoreItem {
				storeid = 1,
				name = "Costela bovina",
				itemid = 37,
				amount_available = 14
			});

			int records = 0;
			using (var command = readerBuilder.GetCommand())
			{
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						records++;
					}
				}
			}

			Assert.AreEqual(2, records);
		}

		[Test()]
		public void TestResultsPropertyCheck() {
			var readerBuilder = new ListStoresJoinedToStoreItemsCommand();
			
			// Adds the query results (two lines, one store)
			readerBuilder.AddQueryResult(new Store {
				storeid = 1,
				name = "Açougue do Zabani"
			},
			new StoreItem {
				storeid = 1,
				name = "Frango Assado",
				itemid = 34,
				amount_available = 7
			});
			readerBuilder.AddQueryResult(new Store {
				storeid = 1,
				name = "Açougue do Zabani"
			},
			new StoreItem {
				storeid = 1,
				name = "Costela bovina",
				itemid = 37,
				amount_available = 14
			});
			
			var resultsFetcher = new ResultsFetcher<Store>(readerBuilder.StoreProjectionMap);
			resultsFetcher.AddOneToManyCollection(s => s.items, readerBuilder.StoreItemProjectionMap);
			IList<Store> queryResults = resultsFetcher.List(readerBuilder.GetCommand());

			// Checks first row
			foreach (Store currentRow in queryResults)
			{
				Assert.AreEqual(1, currentRow.storeid);
				Assert.AreEqual("Açougue do Zabani", currentRow.name);
				Assert.AreEqual(2, currentRow.items.Count);

				foreach (StoreItem item in currentRow.items)
				{
					Assert.AreEqual(1, item.storeid);
				}
			}

			// Only two results!
			Assert.AreEqual(2, queryResults.Count);
		}

		[Test()]
		public void TestResultsFetcherOneToMany() {
			var readerBuilder = new ListStoresJoinedToStoreItemsCommand();

			// Adds the query results
			readerBuilder.AddQueryResult(new Store {
				storeid = 1,
				name = "Açougue do Zabani"
			},
			new StoreItem {
				storeid = 1,
				name = "Frango Assado",
				itemid = 34,
				amount_available = 7
			});
			readerBuilder.AddQueryResult(new Store {
				storeid = 1,
				name = "Açougue do Zabani"
			},
			new StoreItem {
				storeid = 1,
				name = "Costela bovina",
				itemid = 37,
				amount_available = 14
			});

			readerBuilder.AddQueryResult(new Store {
				storeid = 3,
				name = "Restaurante do Dandy"
			},
			new StoreItem {
				storeid = 3,
				name = "Frango na tigela",
				itemid = 47,
				amount_available = 102
			});

			readerBuilder.AddQueryResult(new Store {
				storeid = 1,
				name = "Açougue do Zabani"
			},
			new StoreItem {
				storeid = 1,
				name = "Joelho de porco",
				itemid = 49,
				amount_available = 3
			});

			var resultsFetcher = new ResultsFetcher<Store>(readerBuilder.StoreProjectionMap);
			resultsFetcher.AddOneToManyCollection(s => s.items, readerBuilder.StoreItemProjectionMap);
			ISet<Store> queryResults = resultsFetcher.AsSet(readerBuilder.GetCommand());

			Assert.AreEqual(2, queryResults.Count);
			Assert.AreEqual(3, queryResults.First().items.Count);
			Assert.AreEqual(1, queryResults.Last().items.Count);
		}
	}
}
