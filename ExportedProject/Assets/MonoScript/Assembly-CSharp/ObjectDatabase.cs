using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectDatabase<T> : ScriptableObject where T : UnityEngine.Object
{
	public List<T> Objects;

	public int InternalGetId(T obj)
	{
		return Objects.IndexOf(obj);
	}

	public T InternalGetById(int id)
	{
		if (id < 0 || id >= Objects.Count)
		{
			return (T)null;
		}
		return Objects[id];
	}

	public T InternalGetByName(string name)
	{
		return Objects.Find((T obj) => (UnityEngine.Object)obj != (UnityEngine.Object)null && obj.name.Equals(name, StringComparison.OrdinalIgnoreCase));
	}
}
