using System;
using System.Linq;
using System.Collections.Generic;

using System.Reflection;

using SqlBuilder.Types;

namespace SqlBuilder.Reflection
{
	internal delegate void SetValue(Object obj, Object val);
	internal delegate Object GetValue(Object obj);

	internal class ReflectionHelper
	{
		private delegate void FieldSetValue(Object obj, Object val);
		private delegate void PropSetValue(Object obj, Object val, Object[] index);
		private delegate Object PropGetValue(Object obj, Object[] index);
		private static SetValue FieldValueSetterDelegate(FieldSetValue fieldSetter, Type typeOfField)
		{
			// The property/field setter function automatically converts an object returned by the DB
			// with a possible custom IUserType
			SetValue setValueFunc = (obj, val) => 
				{
					if (val == null)
					{
						fieldSetter(obj, null);
					}
					else
					{
						Type valType = val.GetType();
						fieldSetter(obj, RegisteredCustomTypes.GetSystemObjectAfterCustomTypeConversion(val, typeOfField, valType));
					}
				};
			
			return setValueFunc;
		}
		private static SetValue PropValueSetterDelegate(PropSetValue propSetter, Type typeOfProperty) {
			// The property/field setter function automatically converts an object returned by the DB
			// with a possible custom IUserType
			SetValue setValueFunc = (obj, val) => 
				{
					if (val == null)
					{
						propSetter(obj, null, null);
					}
					else
					{
						Type valType = val.GetType();
						propSetter(obj, RegisteredCustomTypes.GetSystemObjectAfterCustomTypeConversion(val, typeOfProperty, valType), null);
					}
				};
			
			return setValueFunc;
		}
		private static GetValue PropValueGetterDelegate(PropGetValue propGetter) {
			GetValue getValueFunc = (obj) => propGetter(obj, null);
			
			return getValueFunc;
		}
		
		private static IDictionary<Type, IDictionary<string, SetValue>> propOrFieldSetters = new Dictionary<Type, IDictionary<string, SetValue>>(5);
		private static IDictionary<Type, IDictionary<string, GetValue>> propOrFieldGetters = new Dictionary<Type, IDictionary<string, GetValue>>(5);
		
		/// <summary>
		/// Get the publicly settable fields and propertiy setters of type T and put them in a internal cache, if not already there.
		/// Adds a name without a leading underscore for every field or property that possesses one, as long as a publicly settable 
		/// property or field without the leading underscore does not exist. This way, setting value of both "_prop" or "prop"
		/// may actually be setting the value of "_prop", if a field or property "prop" does not exist in type <paramref name="T"/>.
		/// </summary>
		public static IDictionary<string, SetValue> FetchSettersOf<T>() {
			Type typeOfT = typeof(T);

			if (propOrFieldSetters.ContainsKey(typeOfT))
			{
				return propOrFieldSetters[typeOfT];
			}
			else
			{
				IDictionary<string, SetValue> setters = new Dictionary<string, SetValue>(1);
				propOrFieldSetters.Add(typeOfT, setters);
				var fields = typeOfT.GetFields();
				var props = typeOfT.GetProperties();
				
				foreach (FieldInfo field in fields)
				{
					//Console.WriteLine("Found field {0}", field.Name);
					setters.Add(field.Name, FieldValueSetterDelegate(field.SetValue, field.FieldType));
				}
				
				foreach (PropertyInfo prop in props)
				{
					MethodInfo publicSetter = prop.GetSetMethod();
					if (publicSetter != null)
					{
						//Console.WriteLine("Found property {0} with public setter.", prop.Name);
						setters.Add(prop.Name, PropValueSetterDelegate(prop.SetValue, prop.PropertyType));
					}
					else
					{
						//Console.WriteLine("Property {0} found bot not included (no public setter).", prop.Name);
					}						
				}

				// Checks for field or properties with leading underscore without non-underscored analog.
				IDictionary<string, SetValue> analogs = new Dictionary<string, SetValue>();
				foreach (var setter in setters)
				{
					if (setter.Key[0] != '_')
						continue;

					string analogName = setter.Key.Substring(1);
					if (setters.ContainsKey(analogName) == false)
					{
						analogs.Add(analogName, setter.Value);
					}
				}

				foreach (var analogSetter in analogs)
					setters.Add(analogSetter);

				return setters;
			}
		}

		/// <summary>
		/// Get the publicly gettable fields and propertiy getters of type T and put them the cache, if not already there.
		/// </summary>
		public static IDictionary<string, GetValue> FetchGettersOf<T>() {
			Type typeOfT = typeof(T);
			IDictionary<string, GetValue> getters;
			if (propOrFieldGetters.ContainsKey(typeOfT))
			{
				getters = propOrFieldGetters[typeOfT];
			}
			else
			{
				getters = new Dictionary<string, GetValue>(1);
				propOrFieldGetters.Add(typeOfT, getters);
				var fields = typeOfT.GetFields();
				var props = typeOfT.GetProperties();
				
				foreach (var field in fields)
				{
					//Console.WriteLine("Found field {0}", field.Name);
					getters.Add(field.Name, field.GetValue);
				}
				
				foreach (var prop in props)
				{
					MethodInfo publicGetter = prop.GetGetMethod();
					if (publicGetter != null)
					{
						//Console.WriteLine("Found property {0} with public setter.", prop.Name);
						getters.Add(prop.Name, PropValueGetterDelegate(prop.GetValue));
					}
					else
					{
						//Console.WriteLine("Property {0} found bot not included (no public setter).", prop.Name);
					}						
				}
			}
			
			return getters;
		}
	}
}
