using System;
using System.Collections.Generic;
using UnityEngine;

namespace FullInspector.Internal
{
	public static class fiSerializationManager
	{
		[NonSerialized]
		public static bool DisableAutomaticSerialization;

		private static readonly List<ISerializedObject> s_pendingDeserializations;

		private static readonly List<ISerializedObject> s_pendingSerializations;

		private static readonly Dictionary<ISerializedObject, fiSerializedObjectSnapshot> s_snapshots;

		public static HashSet<UnityEngine.Object> DirtyForceSerialize;

		private static HashSet<ISerializedObject> s_seen;

		static fiSerializationManager()
		{
			DisableAutomaticSerialization = false;
			s_pendingDeserializations = new List<ISerializedObject>();
			s_pendingSerializations = new List<ISerializedObject>();
			s_snapshots = new Dictionary<ISerializedObject, fiSerializedObjectSnapshot>();
			DirtyForceSerialize = new HashSet<UnityEngine.Object>();
			s_seen = new HashSet<ISerializedObject>();
			if (fiUtility.IsEditor)
			{
				fiLateBindings.EditorApplication.AddUpdateFunc(OnEditorUpdate);
			}
		}

		private static bool SupportsMultithreading<TSerializer>() where TSerializer : BaseSerializer
		{
			int result;
			if (!fiSettings.ForceDisableMultithreadedSerialization && !fiUtility.IsUnity4)
			{
				TSerializer val = fiSingletons.Get<TSerializer>();
				result = (val.SupportsMultithreading ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}

		public static void OnUnityObjectAwake<TSerializer>(ISerializedObject obj) where TSerializer : BaseSerializer
		{
			if (!obj.IsRestored)
			{
				DoDeserialize(obj);
			}
		}

		public static void OnUnityObjectDeserialize<TSerializer>(ISerializedObject obj) where TSerializer : BaseSerializer
		{
			if (SupportsMultithreading<TSerializer>())
			{
				DoDeserialize(obj);
			}
			else if (fiUtility.IsEditor)
			{
				lock (s_pendingDeserializations)
				{
					s_pendingDeserializations.Add(obj);
				}
			}
		}

		public static void OnUnityObjectSerialize<TSerializer>(ISerializedObject obj) where TSerializer : BaseSerializer
		{
			if (SupportsMultithreading<TSerializer>())
			{
				DoSerialize(obj);
			}
			else if (fiUtility.IsEditor)
			{
				lock (s_pendingSerializations)
				{
					s_pendingSerializations.Add(obj);
				}
			}
		}

		private static void OnEditorUpdate()
		{
			if (Application.isPlaying)
			{
				if (s_pendingDeserializations.Count > 0 || s_pendingSerializations.Count > 0 || s_snapshots.Count > 0)
				{
					s_pendingDeserializations.Clear();
					s_pendingSerializations.Clear();
					s_snapshots.Clear();
				}
			}
			else
			{
				if (fiLateBindings.EditorApplication.isPlaying && BraveUtility.isLoadingLevel)
				{
					return;
				}
				while (s_pendingDeserializations.Count > 0)
				{
					ISerializedObject serializedObject;
					lock (s_pendingDeserializations)
					{
						serializedObject = s_pendingDeserializations[s_pendingDeserializations.Count - 1];
						s_pendingDeserializations.RemoveAt(s_pendingDeserializations.Count - 1);
					}
					if (!(serializedObject is UnityEngine.Object) || !((UnityEngine.Object)serializedObject == null))
					{
						DoDeserialize(serializedObject);
					}
				}
				while (s_pendingSerializations.Count > 0)
				{
					ISerializedObject serializedObject2;
					lock (s_pendingSerializations)
					{
						serializedObject2 = s_pendingSerializations[s_pendingSerializations.Count - 1];
						s_pendingSerializations.RemoveAt(s_pendingSerializations.Count - 1);
					}
					if (!(serializedObject2 is UnityEngine.Object) || !((UnityEngine.Object)serializedObject2 == null))
					{
						DoSerialize(serializedObject2);
					}
				}
			}
		}

		private static void DoDeserialize(ISerializedObject obj)
		{
			obj.RestoreState();
		}

		private static void DoSerialize(ISerializedObject obj)
		{
			if (DisableAutomaticSerialization)
			{
				return;
			}
			bool flag = obj is UnityEngine.Object && DirtyForceSerialize.Contains((UnityEngine.Object)obj);
			if (flag)
			{
				DirtyForceSerialize.Remove((UnityEngine.Object)obj);
			}
			if (!flag && obj is UnityEngine.Object && !fiLateBindings.EditorApplication.isCompilingOrChangingToPlayMode)
			{
				UnityEngine.Object @object = (UnityEngine.Object)obj;
				if (@object is Component)
				{
					@object = ((Component)@object).gameObject;
				}
				UnityEngine.Object object2 = fiLateBindings.Selection.activeObject;
				if (object2 is Component)
				{
					object2 = ((Component)object2).gameObject;
				}
				if (object.ReferenceEquals(@object, object2))
				{
					return;
				}
			}
			CheckForReset(obj);
			obj.SaveState();
		}

		private static void CheckForReset(ISerializedObject obj)
		{
		}

		private static bool IsNullOrEmpty<T>(IList<T> list)
		{
			return list == null || list.Count == 0;
		}
	}
}
