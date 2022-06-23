using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/Data Binding/Proxy Data Object")]
public class dfDataObjectProxy : MonoBehaviour, IDataBindingComponent
{
	[dfEventCategory("Data Changed")]
	public delegate void DataObjectChangedHandler(object data);

	[SerializeField]
	protected string typeName;

	private object data;

	public bool IsBound
	{
		get
		{
			return data != null;
		}
	}

	public string TypeName
	{
		get
		{
			return typeName;
		}
		set
		{
			if (typeName != value)
			{
				typeName = value;
				Data = null;
			}
		}
	}

	public Type DataType
	{
		get
		{
			return getTypeFromName(typeName);
		}
	}

	public object Data
	{
		get
		{
			return data;
		}
		set
		{
			if (!object.ReferenceEquals(value, data))
			{
				data = value;
				if (value != null)
				{
					typeName = value.GetType().Name;
				}
				if (this.DataChanged != null)
				{
					this.DataChanged(value);
				}
			}
		}
	}

	public event DataObjectChangedHandler DataChanged;

	public void Start()
	{
		Type dataType = DataType;
		if (dataType == null)
		{
			Debug.LogError("Unable to retrieve System.Type reference for type: " + TypeName);
		}
	}

	public Type GetPropertyType(string propertyName)
	{
		Type dataType = DataType;
		if (dataType == null)
		{
			return null;
		}
		MemberInfo memberInfo = dataType.GetMember(propertyName, BindingFlags.Instance | BindingFlags.Public).FirstOrDefault();
		if (memberInfo is FieldInfo)
		{
			return ((FieldInfo)memberInfo).FieldType;
		}
		if (memberInfo is PropertyInfo)
		{
			return ((PropertyInfo)memberInfo).PropertyType;
		}
		return null;
	}

	public dfObservableProperty GetProperty(string PropertyName)
	{
		if (data == null)
		{
			return null;
		}
		return new dfObservableProperty(data, PropertyName);
	}

	private Type getTypeFromName(string nameOfType)
	{
		if (nameOfType == null)
		{
			throw new ArgumentNullException("nameOfType");
		}
		Type[] types = GetType().GetAssembly().GetTypes();
		return types.FirstOrDefault((Type t) => t.Name == nameOfType);
	}

	private static Type getTypeFromQualifiedName(string typeName)
	{
		Type type = Type.GetType(typeName);
		if (type != null)
		{
			return type;
		}
		if (typeName.IndexOf('.') == -1)
		{
			return null;
		}
		string assemblyName = typeName.Substring(0, typeName.IndexOf('.'));
		Assembly assembly = Assembly.Load(new AssemblyName(assemblyName));
		if (assembly == null)
		{
			return null;
		}
		return assembly.GetType(typeName);
	}

	public void Bind()
	{
	}

	public void Unbind()
	{
	}
}
