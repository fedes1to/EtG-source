using System;
using System.Collections;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Helicopter/Missiles1")]
public class HelicoperMissiles1 : Script
{
	public class ArcBullet : Bullet
	{
		private Vector2 m_target;

		private float m_t;

		public ArcBullet(Vector2 target, float t)
			: base("missile")
		{
			m_target = target;
			m_t = t;
		}

		public override void Initialize()
		{
			tk2dSpriteAnimator spriteAnimator = Projectile.spriteAnimator;
			spriteAnimator.Play();
			spriteAnimator.SetFrame(spriteAnimator.CurrentClip.frames.Length - 1);
			base.Initialize();
		}

		protected override IEnumerator Top()
		{
			Vector2 toTarget = m_target - base.Position;
			float trueDirection = toTarget.ToAngle();
			Vector2 truePosition = base.Position;
			Vector2 lastPosition = base.Position;
			float travelTime = toTarget.magnitude / Speed * 60f - 1f;
			float magnitude = BraveUtility.RandomSign() * (1f - m_t) * 8f;
			Vector2 offset = magnitude * toTarget.Rotate(90f).normalized;
			base.ManualControl = true;
			Direction = trueDirection;
			for (int i = 0; (float)i < travelTime; i++)
			{
				float angleRad = trueDirection * ((float)Math.PI / 180f);
				Velocity.x = Mathf.Cos(angleRad) * Speed;
				Velocity.y = Mathf.Sin(angleRad) * Speed;
				truePosition += Velocity / 60f;
				lastPosition = base.Position;
				base.Position = truePosition + offset * Mathf.Sin((float)base.Tick / travelTime * (float)Math.PI);
				Direction = (base.Position - lastPosition).ToAngle();
				yield return Wait(1);
			}
			Vector2 v = (base.Position - lastPosition) * 60f;
			Speed = v.magnitude;
			Direction = v.ToAngle();
			base.ManualControl = false;
		}
	}

	public string[] s_targets = new string[4] { "shoot point 1", "shoot point 2", "shoot point 3", "shoot point 4" };

	protected override IEnumerator Top()
	{
		for (int i = 0; i < 12; i++)
		{
			float t = UnityEngine.Random.value;
			float speed = Mathf.Lerp(8f, 11f, t);
			Fire(bullet: new ArcBullet((!BraveUtility.RandomBool()) ? GetPredictedTargetPositionExact(1f, speed) : BulletManager.PlayerPosition(), t), offset: new Offset(s_targets[i % 4]), speed: new Speed(speed));
			PostWwiseEvent("Play_BOSS_RatMech_Missile_01");
			yield return Wait(10);
		}
	}
}
