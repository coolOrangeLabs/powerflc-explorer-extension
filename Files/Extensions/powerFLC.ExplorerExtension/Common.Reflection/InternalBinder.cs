using System;
using System.Reflection;

namespace Common.Reflection
{
	//foreign code
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class InternalBinder
	{
		// Fields
		private object _object;
		private Type _objectType;

		// Methods
		public InternalBinder(object o)
		{
			this.Initialize(o);
		}

		public InternalBinder(Assembly a, string typeName, params object[] ctorParameters)
		{
			object obj2 = GetCtor(a, typeName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, ctorParameters).Invoke(ctorParameters);
			this.Initialize(obj2);
		}

		public InternalBinder(Assembly a, string typeName, BindingFlags ctorFlags, params object[] ctorParameters)
		{
			object obj2 = GetCtor(a, typeName, ctorFlags, ctorParameters).Invoke(ctorParameters);
			this.Initialize(obj2);
		}

		private static ConstructorInfo GetCtor(Assembly a, string typeName, BindingFlags flags, params object[] ctorParameters)
		{
			Type type = a.GetType(typeName);
			Type[] types = new Type[ctorParameters.Length];
			for (int i = 0; i < ctorParameters.Length; i++)
			{
				types[i] = ctorParameters[i].GetType();
			}
			return type.GetConstructor(flags, null, types, null);
		}

		public object GetField(string name)
		{
			return this.GetFieldInfo(name).GetValue(this._object);
		}

		private FieldInfo GetFieldInfo(string name)
		{
			FieldInfo fieldInfo = null;
			for (Type type = this._objectType; (type != null) && (fieldInfo == null); type = type.BaseType)
			{
				fieldInfo = this.GetFieldInfo(type, name);
			}
			return fieldInfo;
		}

		private FieldInfo GetFieldInfo(Type t, string name)
		{
			return t.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		}

		private static MethodInfo GetMethodInfo(Type objectType, InvokeMethodParameter param)
		{
			MethodInfo info = null;
			for (Type type = objectType; (type != null) && (info == null); type = type.BaseType)
			{
				info = GetMethodInfo_(type, param);
			}
			return info;
		}

		private static MethodInfo GetMethodInfo_(Type objectType, InvokeMethodParameter param)
		{
			return objectType.GetMethod(param.Name, param.Flags, null, param.Types, new ParameterModifier[0]);
		}

        public MethodInfo GetMethodInfo(string name)
        {
            return this._objectType.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        }

        public object GetProperty(string name)
		{
			PropertyInfo propertyInfo = this.GetPropertyInfo(name);
			propertyInfo.GetGetMethod();
			try
			{
				return propertyInfo.GetValue(this._object, null);
			}
			catch (Exception)
			{
				return propertyInfo.GetValue(this._object, null);
			}
		}

		private PropertyInfo GetPropertyInfo(string name)
		{
			PropertyInfo propertyInfo = null;
			for (Type type = this._objectType; (type != null) && (propertyInfo == null); type = type.BaseType)
			{
				propertyInfo = this.GetPropertyInfo(type, name);
			}
			return propertyInfo;
		}

		private PropertyInfo GetPropertyInfo(Type t, string name)
		{
			return t.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
		}

		private void Initialize(object Object)
		{
			this._object = Object;
			this._objectType = this._object.GetType();
		}



		public object InvokeMethod(InvokeMethodParameter param)
		{
			return GetMethodInfo(this._objectType, param).Invoke(this._object, param.Parameters);
		}

		public static object InvokeStaticMethod(Type type, InvokeMethodParameter param)
		{
			return GetMethodInfo(type, param).Invoke(null, param.Parameters);
		}

		public static bool IsCtorExist(Assembly a, string typeName, params object[] ctorParameters)
		{
			return (GetCtor(a, typeName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, ctorParameters) != null);
		}

		public static bool IsCtorExist(Assembly a, string typeName, BindingFlags ctorFlags, params object[] ctorParameters)
		{
			return (GetCtor(a, typeName, ctorFlags, ctorParameters) != null);
		}

		public bool IsFieldExist(string name)
		{
			return (this.GetFieldInfo(name) != null);
		}

		public bool IsMethodExist(InvokeMethodParameter param)
		{
			return IsMethodExist(this._objectType, param);
		}

		public static bool IsMethodExist(Type type, InvokeMethodParameter param)
		{
			return (GetMethodInfo(type, param) != null);
		}

		public bool IsPropertyExist(string name)
		{
			return (this.GetPropertyInfo(name) != null);
		}

		public void SetEnumProperty(string name, string val)
		{
			PropertyInfo propertyInfo = this.GetPropertyInfo(name);
			propertyInfo.SetValue(this._object, Enum.Parse(propertyInfo.PropertyType, val), null);
		}

		public void SetField(string name, object val)
		{
			this.GetFieldInfo(name).SetValue(this._object, val);
		}

		public void SetProperty(string name, object val)
		{
			this.GetPropertyInfo(name).SetValue(this._object, val, null);
		}

		// Properties
		public object Object
		{
			get
			{
				return this._object;
			}
		}
	}
}
