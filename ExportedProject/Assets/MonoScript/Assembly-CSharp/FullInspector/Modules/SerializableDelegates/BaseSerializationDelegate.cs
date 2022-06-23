using System;
using System.Reflection;
using FullSerializer.Internal;
using UnityEngine;

namespace FullInspector.Modules.SerializableDelegates
{
	public class BaseSerializationDelegate
	{
		public UnityEngine.Object MethodContainer;

		public string MethodName;

		public bool CanInvoke
		{
			get
			{
				return MethodContainer != null && !string.IsNullOrEmpty(MethodName) && MethodContainer.GetType().GetFlattenedMethod(MethodName) != null;
			}
		}

		public BaseSerializationDelegate()
			: this(null, string.Empty)
		{
		}

		public BaseSerializationDelegate(UnityEngine.Object methodContainer, string methodName)
		{
			MethodContainer = methodContainer;
			MethodName = methodName;
		}

		protected object DoInvoke(params object[] parameters)
		{
			if (MethodContainer == null)
			{
				throw new InvalidOperationException("Attempt to invoke delegate without a method container");
			}
			if (string.IsNullOrEmpty(MethodName))
			{
				throw new InvalidOperationException("Attempt to invoke delegate without a selected method");
			}
			MethodInfo flattenedMethod = MethodContainer.GetType().GetFlattenedMethod(MethodName);
			if (flattenedMethod == null)
			{
				throw new InvalidOperationException("Unable to locate method " + MethodName + " in container " + MethodContainer);
			}
			return flattenedMethod.Invoke(MethodContainer, parameters);
		}
	}
}
