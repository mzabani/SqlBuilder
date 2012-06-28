using System;
using System.Collections.Generic;

namespace TestProject
{
	public class Event
	{
		public int eventid;
		
		public IList<EvDate> evdates;

		public override int GetHashCode ()
		{
			return eventid;
		}

		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			if (obj is Event == false)
				return false;

			return ((Event)obj).eventid == eventid;
		}
	}
}
