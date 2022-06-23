using System;
using System.Collections;
using System.Collections.Generic;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Fusebomb/Bots1")]
public class FusebombBots1 : Script
{
	private class BotBullet : Bullet
	{
		private Vector2 m_goalPos;

		private int m_flightTime;

		public BotBullet(Vector2 goalPos, List<AIActor> spawnedActors, int flightTime)
			: base("bot")
		{
			m_goalPos = goalPos;
			m_flightTime = flightTime;
		}

		protected override IEnumerator Top()
		{
			Projectile.IgnoreTileCollisionsFor((float)(m_flightTime - 5) / 60f);
			base.ManualControl = true;
			Direction = (m_goalPos - base.Position).ToAngle();
			Speed = Vector2.Distance(m_goalPos, base.Position) / ((float)m_flightTime / 60f);
			Vector2 truePosition = base.Position;
			for (int i = 0; i < m_flightTime; i++)
			{
				truePosition += BraveMathCollege.DegreesToVector(Direction, Speed / 60f);
				base.Position = truePosition + new Vector2(0f, Mathf.Sin((float)i / (float)m_flightTime * (float)Math.PI) * 5f);
				if (m_flightTime - i == 60 && (bool)base.BulletBank && (bool)base.BulletBank.aiAnimator)
				{
					AIAnimator aiAnimator = base.BulletBank.aiAnimator;
					string name = "remote_spawn";
					Vector2? position = m_goalPos;
					aiAnimator.PlayVfx(name, null, null, position);
				}
				yield return Wait(1);
			}
			AIActor.Spawn(position: Projectile.specRigidbody.UnitBottomCenter + new Vector2(0f, 0.125f), prefabActor: EnemyDatabase.GetOrLoadByGuid("4538456236f64ea79f483784370bc62f"), source: base.BulletBank.aiActor.ParentRoom, correctForWalls: true, awakenAnimType: AIActor.AwakenAnimationType.Awaken);
			Vanish(true);
		}
	}

	private const string EnemyGuid = "4538456236f64ea79f483784370bc62f";

	protected override IEnumerator Top()
	{
		int num = 4;
		List<AIActor> spawnedActors = new List<AIActor>();
		Vector2 vector = base.BulletBank.aiActor.ParentRoom.area.UnitBottomLeft + new Vector2(1f, 5.5f);
		Vector2 max = new Vector2(base.BulletBank.aiActor.ParentRoom.area.dimensions.x - 2, 4.75f);
		float num2 = max.x / (float)num;
		for (int i = 0; i < num; i++)
		{
			Vector2 vector2 = BraveUtility.RandomVector2(Vector2.zero, max);
			vector2.x = vector2.x % num2 + num2 * (float)i;
			Vector2 vector3 = vector + vector2;
			float direction = (vector3 - base.Position).ToAngle();
			Fire(new Direction(direction), new BotBullet(vector3, spawnedActors, 60 + 10 * i));
		}
		return null;
	}
}
