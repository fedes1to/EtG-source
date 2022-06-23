using System;
using System.Linq;
using UnityEngine;

public static class GameObjectExtensions
{
	public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
	{
		T val = gameObject.GetComponent<T>();
		if ((UnityEngine.Object)val == (UnityEngine.Object)null)
		{
			val = gameObject.AddComponent<T>();
		}
		return val;
	}

	public static void SetLayerRecursively(this GameObject gameObject, int layer)
	{
		gameObject.layer = layer;
		Transform transform = gameObject.transform;
		if (transform.childCount > 0)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				transform.GetChild(i).gameObject.SetLayerRecursively(layer);
			}
		}
	}

	public static void SetComponentEnabledRecursively<T>(this GameObject gameObject, bool enabled) where T : MonoBehaviour
	{
		T[] componentsInChildren = gameObject.GetComponentsInChildren<T>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = enabled;
		}
	}

	public static T[] GetInterfaces<T>(this GameObject gObj)
	{
		if (!typeof(T).IsInterface)
		{
			throw new SystemException("Specified type is not an interface!");
		}
		MonoBehaviour[] components = gObj.GetComponents<MonoBehaviour>();
		return (from a in components
			where a.GetType().GetInterfaces().Any((Type k) => k == typeof(T))
			select (T)(object)a).ToArray();
	}

	public static T GetInterface<T>(this GameObject gObj)
	{
		if (!typeof(T).IsInterface)
		{
			throw new SystemException("Specified type is not an interface!");
		}
		return gObj.GetInterfaces<T>().FirstOrDefault();
	}

	public static T GetInterfaceInChildren<T>(this GameObject gObj)
	{
		if (!typeof(T).IsInterface)
		{
			throw new Exception("Specified type is not an interface!");
		}
		return gObj.GetInterfacesInChildren<T>().FirstOrDefault();
	}

	public static T[] GetInterfacesInChildren<T>(this GameObject gObj)
	{
		if (!typeof(T).IsInterface)
		{
			throw new Exception("Specified type is not an interface!");
		}
		MonoBehaviour[] componentsInChildren = gObj.GetComponentsInChildren<MonoBehaviour>();
		return (from a in componentsInChildren
			where a.GetType().GetInterfaces().Any((Type k) => k == typeof(T))
			select (T)(object)a).ToArray();
	}

	public static int GetPhysicsCollisionMask(this GameObject gameObject, int layer = -1)
	{
		if (layer == -1)
		{
			layer = gameObject.layer;
		}
		int num = 0;
		for (int i = 0; i < 32; i++)
		{
			num |= ((!Physics.GetIgnoreLayerCollision(layer, i)) ? 1 : 0) << i;
		}
		return num;
	}
}
