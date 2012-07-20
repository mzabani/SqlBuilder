using System;
using System.Reflection;
using System.Collections.Generic;

namespace SqlBuilder.Reflection
{
	public delegate void SetValue(Object obj, Object val);
	public delegate Object GetValue(Object obj);

	public class ReflectionHelper
	{
		private delegate void PropSetValue(Object obj, Object val, Object[] index);
		private delegate Object PropGetValue(Object obj, Object[] index);
		private static SetValue PropValueSetterDelegate(PropSetValue propSetter) {
			SetValue setValueFunc = (obj, val) => 
				{
					propSetter(obj, val, null);
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
		/// Get the publicly settable fields and propertiy setters of type T and put them the cache, if not already there.
		/// </summary>
		public static IDictionary<string, SetValue> FetchSettersOf<T>() {
			Type typeOfT = typeof(T);
			IDictionary<string, SetValue> setters;
			if (propOrFieldSetters.ContainsKey(typeOfT))
			{
				setters = propOrFieldSetters[typeOfT];
			}
			else
			{
				setters = new Dictionary<string, SetValue>(1);
				propOrFieldSetters.Add(typeOfT, setters);
				var fields = typeOfT.GetFields();
				var props = typeOfT.GetProperties();
				
				foreach (var field in fields)
				{
					//Console.WriteLine("Found field {0}", field.Name);
					setters.Add(field.Name, field.SetValue);
				}
				
				foreach (var prop in props)
				{
					MethodInfo publicSetter = prop.GetSetMethod();
					if (publicSetter != null)
					{
						//Console.WriteLine("Found property {0} with public setter.", prop.Name);
						setters.Add(prop.Name, PropValueSetterDelegate(prop.SetValue));
					}
					else
					{
						//Console.WriteLine("Property {0} found bot not included (no public setter).", prop.Name);
					}						
				}
			}
			
			return setters;
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

