using System;
using System.Collections.Generic;

namespace Tests.Entities
{
	public class Store
	{
		public int storeid;
		public string name;
		public IList<StoreItem> items;

		public override int GetHashCode ()
		{
			return storeid;
		}
		public override bool Equals (object obj)
		{
			return storeid == ((Store)obj).storeid;
		}
	}
}
