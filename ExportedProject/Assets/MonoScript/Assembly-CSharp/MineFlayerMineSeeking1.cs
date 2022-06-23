using System;
using System.Collections;
using System.Collections.Generic;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/MineFlayer/MineSeeking1")]
public class MineFlayerMineSeeking1 : Script
{
	private class MineBullet : Bullet
	{
		private Vector2 m_goalPos;

		private List<AIActor> m_spawnedActors;

		private int m_index;

		public MineBullet(Vector2 goalPos, List<AIActor> spawnedActors, int index)
			: base("mine")
		{
			m_goalPos = goalPos;
			m_spawnedActors = spawnedActors;
			m_index = index;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Direction = (m_goalPos - base.Position).ToAngle();
			Speed = Vector2.Distance(m_goalPos, base.Position) / 1f;
			Vector2 truePosition = base.Position;
			for (int i = 0; i < 60; i++)
			{
				truePosition += BraveMathCollege.DegreesToVector(Direction, Speed / 60f);
				base.Position = truePosition + new Vector2(0f, Mathf.Sin((float)i / 60f * (float)Math.PI) * 3.5f);
				yield return Wait(1);
			}
			AIActor spawnedEnemy = AIActor.Spawn(position: Projectile.specRigidbody.UnitBottomLeft, prefabActor: EnemyDatabase.GetOrLoadByGuid("566ecca5f3b04945ac6ce1f26dedbf4f"), source: base.BulletBank.aiActor.ParentRoom, correctForWalls: true, awakenAnimType: AIActor.AwakenAnimationType.Awaken);
			m_spawnedActors.Add(spawnedEnemy);
			WaitThenChargeBehavior waitThenChargeBehavior = new WaitThenChargeBehavior();
			spawnedEnemy.behaviorSpeculator.MovementBehaviors[0] = waitThenChargeBehavior;
			waitThenChargeBehavior.Delay = (float)(9 - m_index) * 0.35f + 0.7f * (float)m_index;
			Vanish();
		}
	}

	private const int FlightTime = 60;

	private const string EnemyGuid = "566ecca5f3b04945ac6ce1f26dedbf4f";

	private const float EnemyAngularSpeed = 30f;

	private const float EnemyAngularSpeedDelta = 5f;

	private const int BulletsPerSpray = 5;

	private static readonly float CircleRadius = 14f;

	protected override IEnumerator Top()
	{
		int numMines = ((!((double)base.BulletBank.healthHaver.GetCurrentHealthPercentage() > 0.5)) ? 12 : 9);
		int goopExceptionId = DeadlyDeadlyGoopManager.RegisterUngoopableCircle(base.BulletBank.specRigidbody.UnitCenter, 2f);
		yield return Wait(72);
		List<AIActor> spawnedActors = new List<AIActor>();
		Vector2 roomCenter = base.BulletBank.aiActor.ParentRoom.area.UnitCenter;
		for (int k = 0; k < numMines; k++)
		{
			float angle = UnityEngine.Random.Range(-60f, 60f) + (float)((k % 2 != 0) ? 180 : 0);
			Fire(bullet: new MineBullet(roomCenter + BraveMathCollege.DegreesToVector(angle, CircleRadius), spawnedActors, k), direction: new Direction(angle));
			yield return Wait(21);
		}
		yield return Wait(63);
		for (int j = 0; j < 19; j++)
		{
			float facingAngle = ((j % 2 != 0) ? 180 : 0);
			float targetAngle = (BulletManager.PlayerPosition() - base.Position).ToAngle();
			if (BraveMathCollege.AbsAngleBetween(facingAngle, targetAngle) < 90f && BraveUtility.RandomBool())
			{
				for (int l = 0; l < 5; l++)
				{
					float direction = SubdivideArc(targetAngle - 25f, 50f, 5, l);
					Fire(new Direction(direction), new Speed(12f));
				}
			}
			yield return Wait(21);
		}
		for (int i = 0; i < spawnedActors.Count; i++)
		{
			AIActor actor = spawnedActors[i];
			if ((bool)actor && actor.healthHaver.IsAlive)
			{
				ExplodeOnDeath explodeOnDeath = actor.GetComponent<ExplodeOnDeath>();
				if ((bool)explodeOnDeath)
				{
					UnityEngine.Object.Destroy(explodeOnDeath);
				}
				actor.healthHaver.ApplyDamage(1E+10f, Vector2.zero, "Claymore Death", CoreDamageTypes.None, DamageCategory.Unstoppable);
				yield return Wait(3);
			}
		}
		DeadlyDeadlyGoopManager.DeregisterUngoopableCircle(goopExceptionId);
	}
}
