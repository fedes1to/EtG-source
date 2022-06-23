using System;
using UnityEngine;

namespace FullInspector.Internal
{
	[Serializable]
	public class fiAnimBool : fiBaseAnimValue<bool>
	{
		[SerializeField]
		private float m_Value;

		public float faded
		{
			get
			{
				GetValue();
				return m_Value;
			}
		}

		public fiAnimBool()
			: base(false)
		{
		}

		public fiAnimBool(bool value)
			: base(value)
		{
		}

		protected override bool GetValue()
		{
			float num = (base.target ? 0f : 1f);
			float b = 1f - num;
			m_Value = Mathf.Lerp(num, b, base.lerpPosition);
			return (double)m_Value > 0.5;
		}

		public float Fade(float from, float to)
		{
			return Mathf.Lerp(from, to, faded);
		}
	}
}
