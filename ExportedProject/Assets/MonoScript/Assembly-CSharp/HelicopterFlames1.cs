using System;
using System.Collections;
using System.Collections.Generic;
using Brave.BulletScript;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Helicopter/Flames1")]
public class HelicopterFlames1 : Script
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
			Projectile.spriteAnimator.Play();
			base.ManualControl = true;
			Direction = (m_goalPos - base.Position).ToAngle();
			Speed = Vector2.Distance(m_goalPos, base.Position) / ((float)m_flightTime / 60f);
			Vector2 truePosition = base.Position;
			for (int i = 0; i < m_flightTime; i++)
			{
				truePosition += BraveMathCollege.DegreesToVector(Direction, Speed / 60f);
				base.Position = truePosition + new Vector2(0f, Mathf.Sin((float)i / (float)m_flightTime * (float)Math.PI) * 5f);
				yield return Wait(1);
			}
			yield return Wait(480);
			Vanish();
		}
	}

	private static string[] s_Transforms = new string[4] { "shoot point 1", "shoot point 2", "shoot point 3", "shoot point 4" };

	public const int NumFlamesPerRow = 9;

	protected override IEnumerator Top()
	{
		List<AIActor> spawnedActors = new List<AIActor>();
		Vector2 basePos = base.BulletBank.aiActor.ParentRoom.area.UnitBottomLeft + new Vector2(5f, 22.8f);
		float height = 2.1f;
		float radius = 2f;
		float[] xPos = new float[2]
		{
			UnityEngine.Random.Range(4, 13),
			UnityEngine.Random.Range(21, 30)
		};
		for (int i = 0; i < 2; i++)
		{
			float num = xPos[i];
			float num2 = UnityEngine.Random.Range(0f, 0.8f * height);
			for (int j = 0; j < 9; j++)
			{
				Vector2 vector = basePos + new Vector2(num - radius, (float)j * (0f - height) - num2);
				float direction = (vector - base.Position).ToAngle();
				Fire(new Offset(s_Transforms[i * 2]), new Direction(direction), new FlameBullet(vector, spawnedActors, 60 + 5 * j));
				vector.x += 2f * radius;
				direction = (vector - base.Position).ToAngle();
				Fire(new Offset(s_Transforms[i * 2 + 1]), new Direction(direction), new FlameBullet(vector, spawnedActors, 60 + 5 * j));
			}
		}
		yield return Wait(105);
		GoopDefinition goop = base.BulletBank.GetComponent<GoopDoer>().goopDefinition;
		DeadlyDeadlyGoopManager gooper = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goop);
		for (int k = 0; k < 2; k++)
		{
			Vector2 vector2 = basePos + new Vector2(xPos[k], 0f);
			Vector2 p = vector2 + new Vector2(0f, -18f);
			gooper.TimedAddGoopLine(vector2, p, radius, 1f);
		}
	}
}
