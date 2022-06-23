using System;
using System.Reflection;
using UnityEngine;

public class dfClipboardHelper
{
	private static PropertyInfo m_systemCopyBufferProperty;

	public static string clipBoard
	{
		get
		{
			try
			{
				PropertyInfo systemCopyBufferProperty = GetSystemCopyBufferProperty();
				return (string)systemCopyBufferProperty.GetValue(null, null);
			}
			catch
			{
				return string.Empty;
			}
		}
		set
		{
			try
			{
				PropertyInfo systemCopyBufferProperty = GetSystemCopyBufferProperty();
				systemCopyBufferProperty.SetValue(null, value, null);
			}
			catch
			{
			}
		}
	}

	private static PropertyInfo GetSystemCopyBufferProperty()
	{
		if (m_systemCopyBufferProperty == null)
		{
			Type typeFromHandle = typeof(GUIUtility);
			m_systemCopyBufferProperty = typeFromHandle.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
			if (m_systemCopyBufferProperty == null)
			{
				throw new Exception("Can't access internal member 'GUIUtility.systemCopyBuffer' it may have been removed / renamed");
			}
		}
		return m_systemCopyBufferProperty;
	}
}
