using System;
using System.Collections.Generic;

namespace SqlBuilder.Types
{
	public static class RegisteredCustomTypes
	{
		#region Type key class to serve as key for the dictionary
		class TypeKey {
			public Type SysType;
			public Type DbType;

			public TypeKey(Type sysType, Type dbType) {
				SysType = sysType;
				DbType = dbType;
			}

			public override int GetHashCode ()
			{
				return SysType.GetHashCode() * 31 + DbType.GetHashCode();
			}

			public override bool Equals (object obj)
			{
				if (obj == null)
					return false;
				if (obj is TypeKey == false)
					return false;

				TypeKey b = (TypeKey)obj;

				return b.DbType.Equals(this.DbType) && b.SysType.Equals(this.SysType);
			}
		}
		#endregion

		private static IDictionary<TypeKey, IUserType> RegisteredTypes = new Dictionary<TypeKey, IUserType>();

		/// <summary>
		/// Registers a custom type to do conversion before assignment to results.
		/// </summary>
		/// <param name='userType'>
		/// The custom type handler.
		/// </param>
		/// <typeparam name='T'>
		/// The type that will be found in the db, which will be converted.
		/// </typeparam>
		public static void RegisterCustomType<SysType, DbType>(IUserType userType)
		{
			Type typeOfSysType = typeof(SysType);
			Type typeOfDbType = typeof(DbType);

			TypeKey typeKey = new TypeKey(typeOfSysType, typeOfDbType);

			if (RegisteredTypes.ContainsKey(typeKey))
			{
				// Already exists, overwrite
				RegisteredTypes[typeKey] = userType;
			}
			else
			{
				RegisteredTypes.Add(typeKey, userType);
			}
		}

		/// <summary>
		/// Returns the object after possibly passing it to a custom user type manager for conversion.
		/// </summary>
		/// <returns>
		/// The object after custom type conversion, if a custom type for this DB returned object is registered.
		/// </returns>
		/// <param name='obj'>
		/// The value returned by the DB. This CANNOT be null.
		/// </param>
		internal static object GetSystemObjectAfterCustomTypeConversion(object obj, Type typeOfSysType, Type typeOfDbType) {
			TypeKey typeKey = new TypeKey(typeOfSysType, typeOfDbType);

			// If there is a custom user type
			if (RegisteredTypes.ContainsKey(typeKey))
			{
				IUserType userType = RegisteredTypes[typeKey];
				return userType.ReadValueFromDb(obj);
			}
			else
			{
				return obj;
			}
		}

		internal static object SetDbObjectAfterCustomTypeConversion(object obj)
		{
			throw new NotImplementedException();
		}
	}
}
