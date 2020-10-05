using System;
using System.Reflection;

namespace Common.Reflection
{
	//foreign code
	[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
	public class InvokeMethodParameter
	{
		// Fields
		private Type[] _types;

		// Methods
		public InvokeMethodParameter(string name, params object[] parameters)
		{
			this.Name = name;
			this.Parameters = parameters;
			this.Flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
		}

		private void InitTypesFromParameters()
		{
			this._types = new Type[this.Parameters.Length];
			for (int i = 0; i < this.Parameters.Length; i++)
			{
				this._types[i] = this.Parameters[i].GetType();
			}
		}

		// Properties
		public BindingFlags Flags { get; set; }

		public string Name { get; private set; }

		public object[] Parameters { get; private set; }

		public Type[] Types
		{
			get
			{
				if (this._types == null)
				{
					this.InitTypesFromParameters();
				}
				return this._types;
			}
			set
			{
				this._types = value;
			}
		}
	}
}