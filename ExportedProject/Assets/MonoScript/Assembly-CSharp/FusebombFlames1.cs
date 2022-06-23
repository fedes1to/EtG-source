using System;
using System.Collections;
using System.Collections.Generic;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Fusebomb/Flames1")]
public class FusebombFlames1 : Script
{
	private class FlameBullet : Bullet
	{
		private Vector2 m_goalPos;

		private int m_flightTime;

		public FlameBullet(Vector2 goalPos, List<AIActor> spawnedActors, int flightTime)
			: base("flame")
		{
			m_goalPos = goalPos;
			m_flightTime = flightTime;
		}

		protected override IEnumerator Top()
		{
			Projectile.IgnoreTileCollisionsFor((float)(m_flightTime - 5) / 60f);
			float dir = (m_goalPos - base.Position).ToAngle();
			tk2dSpriteAnimationClip clip = ((!(BraveMathCollege.AbsAngleBetween(0f, dir) <= 90f)) ? Projectile.spriteAnimator.GetClipByName("fusebomb_fire_projectile_left") : Projectile.spriteAnimator.GetClipByName("fusebomb_fire_projectile_right"));
			Projectile.spriteAnimator.Play(clip, UnityEngine.Random.Range(0f, clip.BaseClipLength - 0.1f), clip.fps);
			base.ManualControl = true;
			Direction = dir;
			Speed = Vector2.Distance(m_goalPos, base.Position) / ((float)m_flightTime / 60f);
			Vector2 truePosition = base.Position;
			for (int i = 0; i < m_flightTime; i++)
			{
				truePosition += BraveMathCollege.DegreesToVector(Direction, Speed / 60f);
				base.Position = truePosition + new Vector2(0f, Mathf.Sin((float)i / (float)m_flightTime * (float)Math.PI) * 5f);
				yield return Wait(1);
			}
			yield return Wait((4 + UnityEngine.Random.Range(0, 6)) * 60);
			Vanish();
		}
	}

	public const int NumFlameRows = 4;

	public const int NumFlamesPerRow = 6;

	protected override IEnumerator Top()
	{
		List<AIActor> spawnedActors = new List<AIActor>();
		Vector2 vector = base.BulletBank.aiActor.ParentRoom.area.UnitBottomLeft + new Vector2(1f, 4.8f);
		float num = base.BulletBank.aiActor.ParentRoom.area.dimensions.x - 2;
		float num2 = num / 4f;
		for (int i = 0; i < 4; i++)
		{
			float num3 = UnityEngine.Random.Range(0f, num);
			num3 = num3 % num2 + num2 * (float)i;
			float num4 = UnityEngine.Random.Range(0f, 0.8f);
			for (int j = 0; j < 6; j++)
			{
				Vector2 vector2 = vector + new Vector2(num3, (float)j + num4);
				float direction = (vector2 - base.Position).ToAngle();
				Fire(new Direction(direction), new FlameBullet(vector2, spawnedActors, 60 + 10 * i + 10 * j));
			}
		}
		return null;
	}
}
