using System;

namespace SqlBuilder.Types
{
	public interface IUserType<UserType, DriverType>
	{
		UserType ReadValue(DriverType val);
		
		DriverType SetValue(UserType val);
	}
}
