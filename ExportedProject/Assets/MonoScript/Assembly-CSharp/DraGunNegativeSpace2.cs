using System.Collections.Generic;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/NegativeSpace2")]
public class DraGunNegativeSpace2 : ScriptLite
{
	public class WiggleBullet : BulletLite
	{
		private bool m_suppressOffset;

		private Vector2 m_truePosition;

		private Vector2 m_offset;

		private float m_xMagnitude;

		private float m_xPeriod;

		private float m_yMagnitude;

		private float m_yPeriod;

		private Vector2 m_delta;

		public WiggleBullet(bool suppressOffset)
			: base("default_novfx")
		{
			m_suppressOffset = suppressOffset;
		}

		public override void Start()
		{
			base.ManualControl = true;
			m_truePosition = base.Position;
			m_offset = Vector2.zero;
			m_xMagnitude = Random.Range(0f, 0.6f);
			m_xPeriod = Random.Range(1f, 2.5f);
			m_yMagnitude = Random.Range(0f, 0.4f);
			m_yPeriod = Random.Range(1f, 2.5f);
			m_delta = BraveMathCollege.DegreesToVector(Direction, Speed / 60f);
		}

		public override int Update(ref int state)
		{
			if (base.Tick >= 360)
			{
				Vanish();
				return Done();
			}
			if (!m_suppressOffset)
			{
				float t = 0.5f + (float)base.Tick / 60f * m_xPeriod;
				t = Mathf.Repeat(t, 2f);
				float value = 1f - Mathf.Abs(t - 1f);
				value = Mathf.Clamp01(value);
				value = (float)(-2.0 * (double)value * (double)value * (double)value + 3.0 * (double)value * (double)value);
				m_offset.x = (float)((double)m_xMagnitude * (double)value + (double)(0f - m_xMagnitude) * (1.0 - (double)value));
				float t2 = 0.5f + (float)base.Tick / 60f * m_yPeriod;
				t2 = Mathf.Repeat(t2, 2f);
				float value2 = 1f - Mathf.Abs(t2 - 1f);
				value2 = Mathf.Clamp01(value2);
				value2 = (float)(-2.0 * (double)value2 * (double)value2 * (double)value2 + 3.0 * (double)value2 * (double)value2);
				m_offset.y = (float)((double)m_yMagnitude * (double)value2 + (double)(0f - m_yMagnitude) * (1.0 - (double)value2));
			}
			m_truePosition += m_delta;
			base.Position = m_truePosition + m_offset;
			return Wait(1);
		}
	}

	private const int NumPlatforms = 10;

	private const int NumBullets = 19;

	private const int RowDelay = 16;

	private const float HalfRoomWidth = 17f;

	private const int PlatformRadius = 4;

	private static float[] PlatformAngles = new float[5] { 155f, 125f, 90f, 55f, 25f };

	private static float[] PlatformDistances = new float[5] { 1f, 2.5f, 3f, 2.5f, 1f };

	private static List<int> s_validPlatformIndices = new List<int>();

	private float ActivePlatformRadius;

	private List<Vector2> m_platformCenters;

	private List<bool> m_centerBullets = new List<bool>();

	private float m_verticalGap;

	private float m_lastCenterHeight;

	private float m_rowHeight;

	public override void Start()
	{
		ActivePlatformRadius = 4f;
		int num = 8;
		m_platformCenters = new List<Vector2>(10);
		m_platformCenters.Add(new Vector2(Random.Range(-17f, 17f) / 2f, 0f));
		for (int i = 1; i < 10; i++)
		{
			Vector2 vector = m_platformCenters[m_platformCenters.Count - 1];
			s_validPlatformIndices.Clear();
			for (int j = 0; j < PlatformAngles.Length; j++)
			{
				Vector2 a = vector + BraveMathCollege.DegreesToVector(PlatformAngles[j], 2f * ActivePlatformRadius + PlatformDistances[j]);
				if (a.x > -17f && a.x < 17f && Vector2.Distance(a, m_platformCenters[m_platformCenters.Count - 1]) > (float)num && (i < 2 || Vector2.Distance(a, m_platformCenters[m_platformCenters.Count - 2]) > (float)num) && (i < 3 || Vector2.Distance(a, m_platformCenters[m_platformCenters.Count - 3]) > (float)num))
				{
					s_validPlatformIndices.Add(j);
				}
			}
			if (s_validPlatformIndices.Count == 0)
			{
				s_validPlatformIndices.Add(2);
			}
			int num2 = BraveUtility.RandomElement(s_validPlatformIndices);
			m_platformCenters.Add(vector + BraveMathCollege.DegreesToVector(PlatformAngles[num2], 2f * ActivePlatformRadius + PlatformDistances[num2]));
			if (i % 2 != 1)
			{
				continue;
			}
			s_validPlatformIndices.Remove(num2);
			for (int num3 = s_validPlatformIndices.Count - 1; num3 >= 0; num3--)
			{
				int num4 = s_validPlatformIndices[num3];
				Vector2 a2 = vector + BraveMathCollege.DegreesToVector(PlatformAngles[num4], 2f * ActivePlatformRadius + PlatformDistances[num4]);
				if (Vector2.Distance(a2, m_platformCenters[m_platformCenters.Count - 1]) < (float)num || Vector2.Distance(a2, m_platformCenters[m_platformCenters.Count - 2]) < (float)num)
				{
					s_validPlatformIndices.RemoveAt(num3);
				}
			}
			if (s_validPlatformIndices.Count > 0)
			{
				num2 = BraveUtility.RandomElement(s_validPlatformIndices);
				m_platformCenters.Add(vector + BraveMathCollege.DegreesToVector(PlatformAngles[num2], 2f * ActivePlatformRadius + PlatformDistances[num2]));
			}
		}
		m_verticalGap = 1.6f;
		m_lastCenterHeight = m_platformCenters[m_platformCenters.Count - 1].y;
		m_rowHeight = 0f;
		m_centerBullets.Clear();
		for (int k = 0; k < m_platformCenters.Count; k++)
		{
			m_centerBullets.Add(false);
		}
	}

	public override int Update(ref int state)
	{
		if (state == 0)
		{
			if (m_rowHeight < m_lastCenterHeight)
			{
				for (int i = 0; i < 19; i++)
				{
					float num = SubdivideRange(-17f, 17f, 19, i);
					Vector2 a = new Vector2(num, m_rowHeight);
					bool suppressOffset = false;
					for (int j = 0; j < m_platformCenters.Count; j++)
					{
						if (Vector2.Distance(a, m_platformCenters[j]) < ActivePlatformRadius)
						{
							Vector2 i2;
							Vector2 i3;
							int num2 = BraveMathCollege.LineCircleIntersections(m_platformCenters[j], ActivePlatformRadius, new Vector2(-17f, m_rowHeight), new Vector2(17f, m_rowHeight), out i2, out i3);
							num = ((num2 != 1) ? ((!(Mathf.Abs(num - i2.x) < Mathf.Abs(num - i3.x))) ? i3.x : i2.x) : i2.x);
							suppressOffset = true;
						}
					}
					Fire(new Offset(num, 18f, 0f, string.Empty), new Direction(-90f), new Speed(6f), new WiggleBullet(suppressOffset));
				}
				m_rowHeight += m_verticalGap;
				for (int k = 0; k < m_platformCenters.Count; k++)
				{
					if (!m_centerBullets[k] && m_platformCenters[k].y < m_rowHeight - 2f)
					{
						if (BraveUtility.RandomBool())
						{
							Fire(new Offset(m_platformCenters[k].x, 20.5f - m_rowHeight + m_platformCenters[k].y, 0f, string.Empty), new Direction(-90f), new Speed(6f), new WiggleBullet(true));
						}
						m_centerBullets[k] = true;
					}
				}
				return Wait(16);
			}
			state++;
			return Wait(120);
		}
		return Done();
	}
}
