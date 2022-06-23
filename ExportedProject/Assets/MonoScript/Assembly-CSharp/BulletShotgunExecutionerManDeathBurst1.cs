using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("BulletShotgunMan/ExecutionerDeathBurst1")]
public class BulletShotgunExecutionerManDeathBurst1 : Script
{
	public class RingBullet : Bullet
	{
		private float m_angle;

		private BulletShotgunExecutionerManDeathBurst1 m_parentScript;

		public RingBullet(float angle, BulletShotgunExecutionerManDeathBurst1 parentScript)
			: base("chain")
		{
			m_angle = angle;
			m_parentScript = parentScript;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Projectile.specRigidbody.CollideWithTileMap = false;
			Vector2 center = m_parentScript.Position;
			for (int i = 0; i < 60; i++)
			{
				m_angle += 9f;
				float shownAngle = m_angle;
				if (i >= 50)
				{
					shownAngle = Mathf.LerpAngle(m_angle, m_parentScript.RetargetAngle, (float)(i - 49) / 10f);
				}
				base.Position = center + BraveMathCollege.DegreesToVector(shownAngle);
				yield return Wait(1);
			}
			Projectile.specRigidbody.CollideWithTileMap = true;
			Direction = m_parentScript.RetargetAngle;
			Speed = 12f;
			base.ManualControl = false;
		}
	}

	private const int NumInitialBullets = 6;

	private const int NumRingBullets = 12;

	private const float SpinSpeed = 540f;

	private const float FireRadius = 1f;

	private SpeculativeRigidbody m_targetRigidbody;

	private float? m_cachedRetargetAngle;

	private float RetargetAngle
	{
		get
		{
			if ((bool)m_targetRigidbody)
			{
				return (m_targetRigidbody.HitboxPixelCollider.UnitCenter - base.Position).ToAngle();
			}
			float? cachedRetargetAngle = m_cachedRetargetAngle;
			if (!cachedRetargetAngle.HasValue)
			{
				m_cachedRetargetAngle = Random.Range(0f, 360f);
			}
			return m_cachedRetargetAngle.Value;
		}
	}

	protected override IEnumerator Top()
	{
		if ((bool)base.BulletBank && (bool)base.BulletBank.aiActor && (bool)base.BulletBank.aiActor.TargetRigidbody)
		{
			m_targetRigidbody = base.BulletBank.aiActor.TargetRigidbody;
		}
		else if ((bool)GameManager.Instance.BestActivePlayer)
		{
			m_targetRigidbody = GameManager.Instance.BestActivePlayer.specRigidbody;
		}
		float deltaAngle = 60f;
		for (int j = 0; j <= 6; j++)
		{
			Fire(new Direction((float)j * deltaAngle), new Speed(6.5f), new Bullet("flashybullet"));
		}
		float angle = Random.Range(0f, 360f);
		for (int i = 0; i < 12; i++)
		{
			Fire(new Offset(new Vector2(1f, 0f), angle, string.Empty), new Direction(angle), new RingBullet(angle, this));
			yield return Wait(5);
		}
	}
}
