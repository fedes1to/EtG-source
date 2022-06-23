using System;
using UnityEngine;

namespace FullInspector.Internal
{
	[Serializable]
	public class fiAnimFloat : fiBaseAnimValue<float>
	{
		[SerializeField]
		private float m_Value;

		public fiAnimFloat(float value)
			: base(value)
		{
		}

		protected override float GetValue()
		{
			m_Value = Mathf.Lerp(base.start, base.target, base.lerpPosition);
			return m_Value;
		}
	}
}
