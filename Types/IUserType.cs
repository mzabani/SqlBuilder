using System;

namespace SqlBuilder.Types
{
	public interface IUserType
	{
		/// <summary>
		/// Receives a possibly null object <paramref name="val"/> of type <paramref name="DbType"/> returned by the DB query and returns an object of a different type.
		/// Usually the type returned is a .NET type.
		/// </summary>
		object ReadValueFromDb(object val);

		/// <summary>
		/// Receives a possibly null object <paramref name="val"/> of type <paramref name="SysType"/> and returns an object of a type that can be inserted in the DB, i.e.
		/// a type that the connection driver understands.
		/// </summary>
		object SetValueToAssign(object val);
	}
}
