using System;
using UnityEngine;

namespace FullInspector.Internal
{
	[Serializable]
	public class fiUnityObjectReference
	{
		[SerializeField]
		private UnityEngine.Object _target;

		public bool IsValid
		{
			get
			{
				return Target != null;
			}
		}

		public UnityEngine.Object Target
		{
			get
			{
				if (_target == null)
				{
					TryRestoreFromInstanceId();
				}
				return _target;
			}
			set
			{
				_target = value;
			}
		}

		public fiUnityObjectReference()
		{
		}

		public fiUnityObjectReference(UnityEngine.Object target)
		{
			Target = target;
		}

		private void TryRestoreFromInstanceId()
		{
			if (!object.ReferenceEquals(_target, null))
			{
				int instanceID = _target.GetInstanceID();
				_target = fiLateBindings.EditorUtility.InstanceIDToObject(instanceID);
			}
		}

		public override int GetHashCode()
		{
			if (!IsValid)
			{
				return 0;
			}
			return Target.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			fiUnityObjectReference fiUnityObjectReference2 = obj as fiUnityObjectReference;
			return fiUnityObjectReference2 != null && fiUnityObjectReference2.Target == Target;
		}
	}
}
