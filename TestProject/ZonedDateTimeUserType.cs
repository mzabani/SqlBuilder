using System;
using SqlBuilder;
using NodaTime;
using Npgsql;

namespace TestProject
{
	public class ZonedDateTimeUserType : SqlBuilder.Types.IUserType
	{
		public object ReadValueFromDb (object val)
		{
			if (val == null)
				return null;

			DateTime db_date = (DateTime)val;

			LocalDateTime local_time = new LocalDateTime(db_date.Year, db_date.Month, db_date.Day, db_date.Hour, db_date.Minute, db_date.Second, db_date.Millisecond);

			throw new NotImplementedException();

			/*var offset = Offset.FromHoursAndMinutes(db_date.

			var dtz = DateTimeZone.ForOffset(offset);
			ZonedDateTime globalDate = new ZonedDateTime(local_time, dtz, offset);

			return globalDate;*/
		}

		public object SetValueToAssign (object val)
		{
			throw new NotImplementedException ();
		}
	}
}
