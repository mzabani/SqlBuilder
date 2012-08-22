using System;
using System.Collections.Generic;

namespace SqlBuilder.Types
{
	public static class RegisteredCustomTypes
	{
		private static IDictionary<Type, IUserType> RegisteredTypes = new Dictionary<Type, IUserType>();

		/// <summary>
		/// Registers a custom type to do conversion before assignment to results.
		/// </summary>
		/// <param name='userType'>
		/// The custom type handler.
		/// </param>
		/// <typeparam name='T'>
		/// The type that will be found in the db, which will be converted.
		/// </typeparam>
		public static void RegisterCustomType<T>(IUserType<T> userType)
		{
			Type typeOfT = typeof(T);
			if (RegisteredTypes.ContainsKey(typeOfT))
			{
				// Already exists, overwrite
				RegisteredTypes[typeOfT] = userType;
			}
			else
			{
				RegisteredTypes.Add(typeOfT, userType);
			}
		}

		/// <summary>
		/// Returns the object after possibly passing it to a custom user type manager for conversion.
		/// </summary>
		/// <returns>
		/// The object after custom type conversion, if a custom type for this DB returned object is registered.
		/// </returns>
		/// <param name='obj'>
		/// The value returned by the DB.
		/// </param>
		public static object GetObjectAfterCustomTypeConversion(object obj) {
			if (obj == null)
				return null;

			Type typeOfObj = obj.GetType();

			// If there is a custom user type
			if (RegisteredTypes.ContainsKey(typeOfObj))
			{
				return RegisteredTypes[typeOfObj].ReadValueFromDb(obj);
			}
			else
			{
				return obj;
			}
		}

		public static object SetObjectAfterCustomTypeConversion(object obj)
		{
			throw new NotImplementedException();
		}
	}
}
