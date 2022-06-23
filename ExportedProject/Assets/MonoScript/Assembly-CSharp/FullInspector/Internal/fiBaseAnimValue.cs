using System;
using UnityEngine;

namespace FullInspector.Internal
{
	public abstract class fiBaseAnimValue<T>
	{
		private double m_LerpPosition = 1.0;

		public float speed = 2f;

		private T m_Start;

		[SerializeField]
		private T m_Target;

		private double m_LastTime;

		private bool m_Animating;

		public bool isAnimating
		{
			get
			{
				return m_Animating;
			}
		}

		protected float lerpPosition
		{
			get
			{
				double num = 1.0 - m_LerpPosition;
				return (float)(1.0 - num * num * num * num);
			}
		}

		protected T start
		{
			get
			{
				return m_Start;
			}
		}

		public T target
		{
			get
			{
				return m_Target;
			}
			set
			{
				if (!m_Target.Equals(value))
				{
					BeginAnimating(value, this.value);
				}
			}
		}

		public T value
		{
			get
			{
				return GetValue();
			}
			set
			{
				StopAnim(value);
			}
		}

		protected fiBaseAnimValue(T value)
		{
			m_Start = value;
			m_Target = value;
		}

		private static T2 Clamp<T2>(T2 val, T2 min, T2 max) where T2 : IComparable<T2>
		{
			if (val.CompareTo(min) < 0)
			{
				return min;
			}
			if (val.CompareTo(max) > 0)
			{
				return max;
			}
			return val;
		}

		protected void BeginAnimating(T newTarget, T newStart)
		{
			m_Start = newStart;
			m_Target = newTarget;
			fiLateBindings.EditorApplication.AddUpdateFunc(Update);
			m_Animating = true;
			m_LastTime = fiLateBindings.EditorApplication.timeSinceStartup;
			m_LerpPosition = 0.0;
		}

		private void Update()
		{
			if (m_Animating)
			{
				UpdateLerpPosition();
				if (!((double)lerpPosition < 1.0))
				{
					m_Animating = false;
					fiLateBindings.EditorApplication.RemUpdateFunc(Update);
				}
			}
		}

		private void UpdateLerpPosition()
		{
			double timeSinceStartup = fiLateBindings.EditorApplication.timeSinceStartup;
			m_LerpPosition = Clamp(m_LerpPosition + (timeSinceStartup - m_LastTime) * (double)speed, 0.0, 1.0);
			m_LastTime = timeSinceStartup;
		}

		protected void StopAnim(T newValue)
		{
			bool flag = false;
			if (!newValue.Equals(GetValue()) || m_LerpPosition < 1.0)
			{
				flag = true;
			}
			m_Target = newValue;
			m_Start = newValue;
			m_LerpPosition = 1.0;
			m_Animating = false;
			if (flag)
			{
			}
		}

		protected abstract T GetValue();
	}
}
