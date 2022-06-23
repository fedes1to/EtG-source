using System;
using System.Collections.Generic;
using System.Linq;
using Brave.BulletScript;
using FullInspector;
using FullInspector.Internal;
using UnityEngine;

[Serializable]
public class BulletScriptSelector : fiInspectorOnly, tkCustomEditor
{
	public string scriptTypeName;

	private static Type[] _types;

	private static string[] _typeNames;

	private static GUIContent[] _labels;

	public bool IsNull
	{
		get
		{
			return string.IsNullOrEmpty(scriptTypeName) || scriptTypeName == "null";
		}
	}

	public Bullet CreateInstance()
	{
		Type type = Type.GetType(scriptTypeName);
		if (type == null)
		{
			Debug.LogError("Unknown type! " + scriptTypeName);
			return null;
		}
		return (Bullet)Activator.CreateInstance(type);
	}

	public BulletScriptSelector Clone()
	{
		BulletScriptSelector bulletScriptSelector = new BulletScriptSelector();
		bulletScriptSelector.scriptTypeName = scriptTypeName;
		return bulletScriptSelector;
	}

	tkControlEditor tkCustomEditor.GetEditor()
	{
		return new tkControlEditor(new tk<BulletScriptSelector, tkDefaultContext>.Popup(tk<BulletScriptSelector, tkDefaultContext>.Val((BulletScriptSelector o, tkDefaultContext c) => new fiGUIContent(c.Label.text)), tk<BulletScriptSelector, tkDefaultContext>.Val((BulletScriptSelector o) => o.GetLabels()), tk<BulletScriptSelector, tkDefaultContext>.Val((BulletScriptSelector o) => (!string.IsNullOrEmpty(o.scriptTypeName)) ? Math.Max(0, Array.FindIndex(o.GetTypeNames(), (string gc) => gc == o.scriptTypeName)) : 0), delegate(BulletScriptSelector o, tkDefaultContext c, int v)
		{
			o.scriptTypeName = o.GetTypeNames()[v];
			if (o.scriptTypeName == "null")
			{
				o.scriptTypeName = null;
			}
			return o;
		}));
	}

	public GUIContent[] GetLabels()
	{
		if (_types == null)
		{
			InitEditorCache();
		}
		return _labels;
	}

	public string[] GetTypeNames()
	{
		if (_types == null)
		{
			InitEditorCache();
		}
		return _typeNames;
	}

	private void InitEditorCache()
	{
		List<Type> list = new List<Type>();
		list.Add(null);
		list.AddRange(fiRuntimeReflectionUtility.AllSimpleCreatableTypesDerivingFrom(typeof(Script)));
		list.Remove(typeof(Script));
		list.AddRange(fiRuntimeReflectionUtility.AllSimpleCreatableTypesDerivingFrom(typeof(ScriptLite)));
		list.Remove(typeof(ScriptLite));
		_types = list.ToArray();
		_typeNames = _types.Select((Type t) => (t != null) ? t.FullName : "null").ToArray();
		_labels = _types.Select(delegate(Type t)
		{
			if (t == null)
			{
				return new GUIContent("null");
			}
			InspectorDropdownNameAttribute inspectorDropdownNameAttribute = Attribute.GetCustomAttribute(t, typeof(InspectorDropdownNameAttribute)) as InspectorDropdownNameAttribute;
			return (inspectorDropdownNameAttribute != null) ? new GUIContent(inspectorDropdownNameAttribute.DisplayName) : new GUIContent(t.FullName);
		}).ToArray();
	}
}
