using System;

namespace SqlBuilder.Types
{
	public interface IUserType
	{
		object ReadValueFromDb(object val);

		object SetValueToAssign(object val);
	}

	public interface IUserType<DbType> : IUserType
	{

	}
}
