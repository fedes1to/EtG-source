using System;
using System.Linq;
using System.Reflection;

public class dfObservableProperty : IObservableValue
{
	private delegate object ValueGetter();

	private delegate void ValueSetter(object value);

	private static object[] tempArray = new object[1];

	private object lastValue;

	private bool hasChanged;

	private object target;

	private FieldInfo fieldInfo;

	private PropertyInfo propertyInfo;

	private MethodInfo propertyGetter;

	private MethodInfo propertySetter;

	private Type propertyType;

	private bool canWrite;

	public Type PropertyType
	{
		get
		{
			if (fieldInfo != null)
			{
				return fieldInfo.FieldType;
			}
			return propertyInfo.PropertyType;
		}
	}

	public object Value
	{
		get
		{
			return getter();
		}
		set
		{
			lastValue = value;
			setter(value);
			hasChanged = false;
		}
	}

	public bool HasChanged
	{
		get
		{
			if (hasChanged)
			{
				return true;
			}
			object obj = getter();
			if (object.ReferenceEquals(obj, lastValue))
			{
				hasChanged = false;
			}
			else if (obj == null || lastValue == null)
			{
				hasChanged = true;
			}
			else
			{
				hasChanged = !obj.Equals(lastValue);
			}
			return hasChanged;
		}
	}

	internal dfObservableProperty(object target, string memberName)
	{
		MemberInfo memberInfo = target.GetType().GetMember(memberName, BindingFlags.Instance | BindingFlags.Public).FirstOrDefault();
		if (memberInfo == null)
		{
			throw new ArgumentException("Invalid property or field name: " + memberName, "memberName");
		}
		initMember(target, memberInfo);
	}

	internal dfObservableProperty(object target, FieldInfo field)
	{
		initField(target, field);
	}

	internal dfObservableProperty(object target, PropertyInfo property)
	{
		initProperty(target, property);
	}

	internal dfObservableProperty(object target, MemberInfo member)
	{
		initMember(target, member);
	}

	public void ClearChangedFlag()
	{
		hasChanged = false;
		lastValue = getter();
	}

	private void initMember(object target, MemberInfo member)
	{
		if (member is FieldInfo)
		{
			initField(target, (FieldInfo)member);
		}
		else
		{
			initProperty(target, (PropertyInfo)member);
		}
	}

	private void initField(object target, FieldInfo field)
	{
		this.target = target;
		fieldInfo = field;
		Value = getter();
	}

	private void initProperty(object target, PropertyInfo property)
	{
		this.target = target;
		propertyInfo = property;
		propertyGetter = property.GetGetMethod();
		propertySetter = property.GetSetMethod();
		canWrite = propertySetter != null;
		Value = getter();
	}

	private object getter()
	{
		if (propertyInfo != null)
		{
			return getPropertyValue();
		}
		return getFieldValue();
	}

	private void setter(object value)
	{
		if (propertyInfo != null)
		{
			setPropertyValue(value);
		}
		else
		{
			setFieldValue(value);
		}
	}

	private object getPropertyValue()
	{
		return propertyGetter.Invoke(target, null);
	}

	private void setPropertyValue(object value)
	{
		if (canWrite)
		{
			if (propertyType == null)
			{
				propertyType = propertyInfo.PropertyType;
			}
			if (value == null || propertyType.IsAssignableFrom(value.GetType()))
			{
				tempArray[0] = value;
			}
			else
			{
				tempArray[0] = Convert.ChangeType(value, propertyType);
			}
			propertySetter.Invoke(target, tempArray);
		}
	}

	private void setFieldValue(object value)
	{
		if (!fieldInfo.IsLiteral)
		{
			if (propertyType == null)
			{
				propertyType = fieldInfo.FieldType;
			}
			if (value == null || propertyType.IsAssignableFrom(value.GetType()))
			{
				fieldInfo.SetValue(target, value);
				return;
			}
			object value2 = Convert.ChangeType(value, propertyType);
			fieldInfo.SetValue(target, value2);
		}
	}

	private void setFieldValueNOP(object value)
	{
	}

	private object getFieldValue()
	{
		return fieldInfo.GetValue(target);
	}
}
