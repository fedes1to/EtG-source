using System;
using System.Reflection;
using FullInspector.Internal;
using FullSerializer.Internal;
using UnityEngine;

namespace FullInspector
{
	public class InspectedMethod
	{
		public MethodInfo Method { get; private set; }

		public GUIContent DisplayLabel { get; private set; }

		public bool HasArguments { get; private set; }

		public InspectedMethod(MethodInfo method)
		{
			Method = method;
			ParameterInfo[] parameters = method.GetParameters();
			foreach (ParameterInfo parameterInfo in parameters)
			{
				if (!parameterInfo.IsOptional)
				{
					HasArguments = true;
					break;
				}
			}
			DisplayLabel = new GUIContent();
			InspectorNameAttribute attribute = fsPortableReflection.GetAttribute<InspectorNameAttribute>(method);
			if (attribute != null)
			{
				DisplayLabel.text = attribute.DisplayName;
			}
			if (string.IsNullOrEmpty(DisplayLabel.text))
			{
				DisplayLabel.text = fiDisplayNameMapper.Map(method.Name);
			}
			InspectorTooltipAttribute attribute2 = fsPortableReflection.GetAttribute<InspectorTooltipAttribute>(method);
			if (attribute2 != null)
			{
				DisplayLabel.tooltip = attribute2.Tooltip;
			}
		}

		public void Invoke(object instance)
		{
			try
			{
				object[] array = null;
				ParameterInfo[] parameters = Method.GetParameters();
				if (parameters.Length != 0)
				{
					array = new object[parameters.Length];
					for (int i = 0; i < array.Length; i++)
					{
						array[i] = parameters[i].DefaultValue;
					}
				}
				Method.Invoke(instance, array);
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
		}
	}
}
