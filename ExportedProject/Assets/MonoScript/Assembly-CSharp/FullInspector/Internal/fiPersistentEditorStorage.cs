using System;
using System.Collections.Generic;
using System.Linq;
using FullSerializer;
using UnityEngine;

namespace FullInspector.Internal
{
	public class fiPersistentEditorStorage
	{
		private static Dictionary<Type, Type> _cachedRealComponentTypes = new Dictionary<Type, Type>();

		private const string SceneStorageName = "fiPersistentEditorStorage";

		private static GameObject _cachedSceneStorage;

		public static GameObject SceneStorage
		{
			get
			{
				if (_cachedSceneStorage == null)
				{
					_cachedSceneStorage = GameObject.Find("fiPersistentEditorStorage");
					if (_cachedSceneStorage == null)
					{
						_cachedSceneStorage = fiLateBindings.EditorUtility.CreateGameObjectWithHideFlags("fiPersistentEditorStorage", HideFlags.HideInHierarchy);
					}
				}
				return _cachedSceneStorage;
			}
		}

		public static void Reset<T>(UnityEngine.Object key_)
		{
			fiUnityObjectReference fiUnityObjectReference2 = new fiUnityObjectReference(key_);
			fiBaseStorageComponent<T> fiBaseStorageComponent = ((!fiLateBindings.EditorUtility.IsPersistent(fiUnityObjectReference2.Target)) ? GetStorageDictionary<T>(SceneStorage) : GetStorageDictionary<T>(SceneStorage));
			fiBaseStorageComponent.Data.Remove(fiUnityObjectReference2.Target);
			fiLateBindings.EditorUtility.SetDirty(fiBaseStorageComponent);
		}

		public static T Read<T>(UnityEngine.Object key_) where T : new()
		{
			fiUnityObjectReference fiUnityObjectReference2 = new fiUnityObjectReference(key_);
			fiBaseStorageComponent<T> fiBaseStorageComponent = ((!fiLateBindings.EditorUtility.IsPersistent(fiUnityObjectReference2.Target)) ? GetStorageDictionary<T>(SceneStorage) : GetStorageDictionary<T>(SceneStorage));
			if (fiBaseStorageComponent.Data.ContainsKey(fiUnityObjectReference2.Target))
			{
				return fiBaseStorageComponent.Data[fiUnityObjectReference2.Target];
			}
			T val = new T();
			fiBaseStorageComponent.Data[fiUnityObjectReference2.Target] = val;
			T result = val;
			fiLateBindings.EditorUtility.SetDirty(fiBaseStorageComponent);
			return result;
		}

		private static fiBaseStorageComponent<T> GetStorageDictionary<T>(GameObject container)
		{
			Type value;
			if (!_cachedRealComponentTypes.TryGetValue(typeof(fiBaseStorageComponent<T>), out value))
			{
				value = fiRuntimeReflectionUtility.AllSimpleTypesDerivingFrom(typeof(fiBaseStorageComponent<T>)).FirstOrDefault();
				_cachedRealComponentTypes[typeof(fiBaseStorageComponent<T>)] = value;
			}
			if (value == null)
			{
				throw new InvalidOperationException("Unable to find derived component type for " + typeof(fiBaseStorageComponent<T>).CSharpName());
			}
			Component component = container.GetComponent(value);
			if (component == null)
			{
				component = container.AddComponent(value);
			}
			return (fiBaseStorageComponent<T>)component;
		}
	}
}
