using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public class CultistKaliberTentacle1 : Script
{
	public class TentacleBullet : Bullet
	{
		private int m_delay;

		private CultistKaliberTentacle1 m_parentScript;

		private int m_index;

		private float m_offsetScalar;

		private Vector2 m_target;

		public TentacleBullet(int delay, CultistKaliberTentacle1 parentScript, int index, float offsetScalar)
		{
			m_delay = delay;
			m_parentScript = parentScript;
			m_index = index;
			m_offsetScalar = offsetScalar;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			yield return Wait(m_delay);
			Vector2 truePosition = base.Position;
			for (int i = 0; i < 360; i++)
			{
				float offsetMagnitude2 = m_offsetScalar * Mathf.SmoothStep(-0.75f, 0.75f, Mathf.PingPong(0.5f + (float)i / 60f * 3f, 1f));
				offsetMagnitude2 *= Mathf.Lerp(1f, 0.25f, (float)(i - 20) / 40f);
				if (i >= 20 && i <= 60)
				{
					if (i == 20)
					{
						m_target = m_parentScript.GetTargetPosition(m_index, this);
					}
					float num = (m_target - truePosition).ToAngle();
					float value = BraveMathCollege.ClampAngle180(num - Direction);
					Direction += Mathf.Clamp(value, -6f, 6f);
				}
				truePosition += BraveMathCollege.DegreesToVector(Direction, Speed / 60f);
				base.Position = truePosition + BraveMathCollege.DegreesToVector(Direction - 90f, offsetMagnitude2);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int NumTentacles = 4;

	private const int NumBullets = 8;

	private const int BulletSpeed = 10;

	private const float TentacleMagnitude = 0.75f;

	private const float TentaclePeriod = 3f;

	private Vector2?[] m_targetPositions;

	protected override IEnumerator Top()
	{
		m_targetPositions = new Vector2?[4];
		float aimDirection = base.AimDirection;
		for (int i = 0; i < 4; i++)
		{
			float direction = SubdivideArc(aimDirection - 65f, 130f, 4, i) + Random.Range(-6f, 6f);
			for (int j = 0; j < 8; j++)
			{
				Fire(new Direction(direction), new Speed(10f), new TentacleBullet(j * 6, this, i, ((float)i < 2f) ? 1 : (-1)));
			}
		}
		return null;
	}

	public Vector2 GetTargetPosition(int index, Bullet bullet)
	{
		Vector2? vector = m_targetPositions[index];
		if (!vector.HasValue)
		{
			m_targetPositions[index] = bullet.GetPredictedTargetPosition((!((double)Random.value < 0.5)) ? 1 : 0, 10f);
		}
		return m_targetPositions[index].Value;
	}
}
