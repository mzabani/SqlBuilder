using System;
using System.Linq;
using Tests.Entities;
using System.Collections.Generic;
using System.Data;
using Moq;

namespace Tests.Mocking
{
	class ListStoresJoinedToStoreItemsCommand
	{
		private Stack<Tuple<Store, StoreItem>> QueryResults;
		private Tuple<Store, StoreItem> CurrentResult;
		private Mock<IDataReader> ReaderMock;

		public IDictionary<string, string> StoreProjectionMap {
			get
			{
				return new Dictionary<string, string>() {
					{ "Store_storeid", "storeid" },
					{ "Store_name", "name" }
				};
			}
		}

		public IDictionary<string, string> StoreItemProjectionMap {
			get
			{
				return new Dictionary<string, string>() {
					{ "itemid", "itemid" },
					{ "StoreItem_name", "name" },
					{ "amount_available", "amount_available" },
					{ "StoreItem_storeid", "storeid" }
				};
			}
		}

		/// <summary>
		/// Returns the queried projections in the order they would come from the database.
		/// </summary>
		public IEnumerable<string> GetOrderedColumnsNames() {
			yield return "Store_storeid";
			yield return "Store_name";
			yield return "itemid";
			yield return "StoreItem_name";
			yield return "amount_available";
			yield return "StoreItem_storeid";
		}

		private object ArrayStyleAccess(int ordinal) {
			var storeAndItem = CurrentResult;

			Store store = storeAndItem.Item1;
			StoreItem item = storeAndItem.Item2;

			switch (ordinal)
			{
				case 0:
					return store.storeid;

				case 1:
					return store.name;

				case 2:
					return item.itemid;

				case 3:
					return item.name;

				case 4:
					return item.amount_available;

				case 5:
					return item.storeid;
			}

			throw new IndexOutOfRangeException();
		}

		private int SimulateGetOrdinal(string projectionName) {
			return GetOrderedColumnsNames().TakeWhile(c => c != projectionName).Count();
		}

		/// <summary>
		/// (Re)mocks array-style access and IDataReader methods to simulate a different result being returned by the database.
		/// </summary>
		private bool SimulateRead() {
			if (QueryResults.Count == 0)
				return false;

			CurrentResult = QueryResults.Pop();
			return true;
		}

		public IDbCommand GetCommand() {
			var commandMock = new Mock<IDbCommand>();

			commandMock.Setup(c => c.ExecuteReader()).Returns(ReaderMock.Object);

			return commandMock.Object;
		}

		/// <summary>
		/// Adds a query result, making some sanity checks first to avoid future headaches.
		/// </summary>
		public void AddQueryResult(Store store, StoreItem item) {
			if (store == null)
				throw new ArgumentNullException();

			if (store.storeid != item.storeid)
				throw new Exception("storeid must match in both objects (this is some kind of join, right?)");

			QueryResults.Push(new Tuple<Store, StoreItem>(store, item));
		}

		/// <summary>
		/// Only inner joins for now, please, and no null values either.
		/// </summary>
		public ListStoresJoinedToStoreItemsCommand()
		{
			QueryResults = new Stack<Tuple<Store, StoreItem>>();
			ReaderMock = new Mock<IDataReader>();

			// Read() must be mocked!
			ReaderMock.Setup(m => m.Read())
					  .Returns(SimulateRead);

			// Array style access too!
			ReaderMock.Setup(m => m.GetValue(It.IsAny<int>())).Returns((int ordinal) => ArrayStyleAccess(ordinal));
			ReaderMock.Setup(m => m.GetOrdinal(It.IsAny<string>())).Returns((string colname) => SimulateGetOrdinal(colname));

			// And of course IsDBNull.. for now nothing is null..
			ReaderMock.Setup(m => m.IsDBNull(It.IsAny<int>())).Returns(false);
		}
	}
}
