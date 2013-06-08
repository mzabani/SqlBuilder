using System;
using System.Collections.Generic;

namespace TestProject
{
	class PlaceEditionAndVoters {
		public int editionid;
		public int placeid;
		public DateTime date;

		public IList<int> votersids;

		public override int GetHashCode ()
		{
			return editionid;
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			else if (obj is PlaceEditionAndVoters == false)
				return false;

			return this.editionid == ((PlaceEditionAndVoters)obj).editionid;
		}
	}
}
