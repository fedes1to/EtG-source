using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Minibosses/PhantomAgunim/Reflect1")]
public class PhantomAgunimNonReflect1 : Script
{
	public class RingBullet : Bullet
	{
		private const int TicksBeforeStrighteningOut = 35;

		private const int TicksToStraightenOut = 30;

		private float m_angle;

		private float m_rotationSpeed;

		public RingBullet(float angle, float rotationSpeed)
			: base("ring")
		{
			m_angle = angle;
			m_rotationSpeed = rotationSpeed;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Projectile.IgnoreTileCollisionsFor(0.6f);
			Vector2 centerPosition = base.Position;
			for (int j = 0; j < 20; j++)
			{
				m_angle += 7.5f;
				base.Position = centerPosition + BraveMathCollege.DegreesToVector(m_angle, 0.55f);
				yield return Wait(1);
			}
			for (int i = 0; i < 300; i++)
			{
				Direction += m_rotationSpeed / 60f * Mathf.Lerp(1f, 0f, (i - 35) / 30);
				UpdateVelocity();
				centerPosition += Velocity / 60f;
				m_angle += 7.5f;
				base.Position = centerPosition + BraveMathCollege.DegreesToVector(m_angle, 0.55f);
				yield return Wait(1);
			}
			Vanish();
		}
	}

	private const int NumRings = 6;

	private const int NumBulletsPerRing = 5;

	private const float RingRadius = 0.55f;

	private const float RingSpinSpeed = 450f;

	private const int RingDelay = 20;

	private const float DeltaStartAim = 10f;

	private const float StartSpeed = 10f;

	private const float SpeedIncrease = 2f;

	private const float RotationSpeed = 45f;

	private const float RotationSpeedIncrease = 10f;

	protected override IEnumerator Top()
	{
		yield return Wait(48);
		if ((bool)base.BulletBank)
		{
			base.BulletBank.aiAnimator.PlayVfx("hover_charge_loop");
		}
		float sign = ((!BraveUtility.RandomBool()) ? 1 : (-1));
		for (int i = 0; i < 6; i++)
		{
			float startDirection = base.AimDirection + sign * (float)(i + 1) * 10f;
			float rotationSpeed = (0f - sign) * (45f + (float)(i + 1) * 10f);
			for (int j = 0; j < 5; j++)
			{
				float angle = SubdivideCircle(0f, 5, j);
				RingBullet ringBullet = new RingBullet(angle, rotationSpeed);
				Fire(new Direction(startDirection), new Speed(10f + (float)i * 2f), ringBullet);
				ringBullet.Projectile.IgnoreTileCollisionsFor(1f);
			}
			sign *= -1f;
			yield return Wait(20);
			AkSoundEngine.PostEvent("Play_BOSS_agunim_orb_01", base.BulletBank.gameObject);
		}
		yield return Wait(20);
		if ((bool)base.BulletBank)
		{
			base.BulletBank.aiAnimator.StopVfx("hover_charge_loop");
			base.BulletBank.aiAnimator.PlayVfx("hover_charge_end");
		}
		yield return Wait(40);
	}

	public override void OnForceEnded()
	{
		if ((bool)base.BulletBank && (bool)base.BulletBank.aiAnimator)
		{
			base.BulletBank.aiAnimator.StopVfx("hover_charge_loop");
		}
	}
}
