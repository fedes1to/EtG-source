using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalBullet/AgunimLightning1")]
public class BossFinalBulletAgunimLightning1 : Script
{
	private class LightningBullet : Bullet
	{
		private float m_direction;

		private float m_sign;

		private int m_maxRemainingBullets;

		private int m_timeSinceLastTurn;

		private Vector2? m_truePosition;

		public LightningBullet(float direction, float sign, int maxRemainingBullets, int timeSinceLastTurn, Vector2? truePosition = null)
		{
			m_direction = direction;
			m_sign = sign;
			m_maxRemainingBullets = maxRemainingBullets;
			m_timeSinceLastTurn = timeSinceLastTurn;
			m_truePosition = truePosition;
		}

		protected override IEnumerator Top()
		{
			yield return Wait(2);
			Vector2? truePosition = m_truePosition;
			if (!truePosition.HasValue)
			{
				m_truePosition = base.Position;
			}
			if (m_maxRemainingBullets > 0)
			{
				if (m_timeSinceLastTurn > 0 && m_timeSinceLastTurn != 2 && m_timeSinceLastTurn != 3 && Random.value < 0.2f)
				{
					m_sign *= -1f;
					m_timeSinceLastTurn = 0;
				}
				float num = m_direction + m_sign * 30f;
				Vector2 vector = m_truePosition.Value + BraveMathCollege.DegreesToVector(num, 0.8f);
				Vector2 vector2 = vector + BraveMathCollege.DegreesToVector(num + 90f, Random.Range(-0.3f, 0.3f));
				if (!IsPointInTile(vector2))
				{
					LightningBullet lightningBullet = new LightningBullet(m_direction, m_sign, m_maxRemainingBullets - 1, m_timeSinceLastTurn + 1, vector);
					Fire(Offset.OverridePosition(vector2), lightningBullet);
					if ((bool)lightningBullet.Projectile && (bool)lightningBullet.Projectile.specRigidbody && PhysicsEngine.Instance.OverlapCast(lightningBullet.Projectile.specRigidbody, null, true, false, null, null, false, null, null))
					{
						lightningBullet.Projectile.DieInAir();
					}
				}
			}
			yield return Wait(30);
			Vanish(true);
		}
	}

	public const float Dist = 0.8f;

	public const int MaxBulletDepth = 30;

	public const float RandomOffset = 0.3f;

	public const float TurnChance = 0.2f;

	public const float TurnAngle = 30f;

	protected override IEnumerator Top()
	{
		float direction = BraveMathCollege.QuantizeFloat(base.AimDirection, 45f);
		Fire(new Offset("lightning left shoot point"), new LightningBullet(direction, -1f, 30, -4));
		Fire(new Offset("lightning left shoot point"), new LightningBullet(direction, -1f, 30, 4));
		if (BraveUtility.RandomBool())
		{
			Fire(new Offset("lightning left shoot point"), new LightningBullet(direction, -1f, 30, 4));
		}
		else
		{
			Fire(new Offset("lightning right shoot point"), new LightningBullet(direction, 1f, 30, 4));
		}
		Fire(new Offset("lightning right shoot point"), new LightningBullet(direction, 1f, 30, 4));
		Fire(new Offset("lightning right shoot point"), new LightningBullet(direction, 1f, 30, -4));
		return null;
	}
}
