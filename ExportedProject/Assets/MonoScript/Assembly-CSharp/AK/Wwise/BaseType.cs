using System;
using UnityEngine;

namespace AK.Wwise
{
	[Serializable]
	public class BaseType
	{
		public int ID;

		protected uint GetID()
		{
			return (uint)ID;
		}

		public virtual bool IsValid()
		{
			return (long)ID != 0;
		}

		public bool Validate()
		{
			if (IsValid())
			{
				return true;
			}
			Debug.LogWarning("Wwise ID has not been resolved. Consider picking a new " + GetType().Name + ".");
			return false;
		}

		protected void Verify(AKRESULT result)
		{
		}
	}
}
