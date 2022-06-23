using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalGuide/Sword1")]
public class BossFinalGuideSword1 : Script
{
	public class SwordBullet : Bullet
	{
		private Vector2 m_origin;

		private float m_sign;

		private bool m_doubleSwing;

		public SwordBullet(Vector2 origin, float sign, bool doubleSwing)
		{
			m_origin = origin;
			m_sign = sign;
			m_doubleSwing = doubleSwing;
		}

		protected override IEnumerator Top()
		{
			yield return Wait(20);
			float angle = (base.Position - m_origin).ToAngle();
			float dist = Vector2.Distance(base.Position, m_origin);
			Speed = 0f;
			yield return Wait(30);
			base.ManualControl = true;
			int swingtime = ((!m_doubleSwing) ? 25 : 100);
			float swingDegrees = ((!m_doubleSwing) ? 180 : 540);
			for (int i = 0; i < swingtime; i++)
			{
				float newAngle = angle - Mathf.SmoothStep(0f, m_sign * swingDegrees, (float)i / (float)swingtime);
				base.Position = m_origin + BraveMathCollege.DegreesToVector(newAngle, dist);
				yield return Wait(1);
			}
			yield return Wait(5);
			Vanish();
		}
	}

	private const int SetupTime = 20;

	private const int HoldTime = 30;

	private const int SwingTime = 25;

	private float m_sign;

	private bool m_doubleSwing;

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		m_sign = BraveUtility.RandomSign();
		m_doubleSwing = BraveUtility.RandomBool();
		Vector2 leftOrigin = new Vector2(m_sign * 2f, -0.2f);
		FireLine(leftOrigin, new Vector2(m_sign * 3.8f, 0.2f), new Vector2(m_sign * 11f, 0.2f), 14);
		FireLine(leftOrigin, new Vector2(m_sign * 11.6f, -0.2f), new Vector2(m_sign * 11.6f, -0.2f), 2);
		FireLine(leftOrigin, new Vector2(m_sign * 3.8f, -0.6f), new Vector2(m_sign * 11f, -0.6f), 14);
		FireLine(leftOrigin, new Vector2(m_sign * 3.1f, -1.2f), new Vector2(m_sign * 3.1f, 0.8f), 4);
		FireLine(leftOrigin, new Vector2(m_sign * 2.2f, -0.2f), new Vector2(m_sign * 2.7f, -0.2f), 2);
		yield return Wait(75);
	}

	private void FireLine(Vector2 spawnPoint, Vector2 start, Vector2 end, int numBullets)
	{
		Vector2 vector = (end - start) / Mathf.Max(1, numBullets - 1);
		float num = 1f / 3f;
		for (int i = 0; i < numBullets; i++)
		{
			Vector2 vector2 = ((numBullets != 1) ? (start + vector * i) : end);
			float speed = Vector2.Distance(vector2, spawnPoint) / num;
			Fire(new Offset(spawnPoint, 0f, string.Empty), new Direction((vector2 - spawnPoint).ToAngle()), new Speed(speed), new SwordBullet(base.Position, m_sign, m_doubleSwing));
		}
	}
}
