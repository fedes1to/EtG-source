using System.Collections;
using System.Collections.Generic;
using Brave.BulletScript;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/ResourcefulRat/Daggers1")]
public class ResourcefulRatDaggers1 : Script
{
	private const int NumWaves = 3;

	private const int NumDaggersPerWave = 17;

	private const int AimDelay = 1;

	private const int VanishDelay = 15;

	private const int FireDelay = 25;

	private const float DaggerSpeed = 60f;

	private List<GameObject> m_reticles = new List<GameObject>();

	protected override IEnumerator Top()
	{
		yield return Wait(18);
		float[] angles = new float[17];
		CellArea area = base.BulletBank.aiActor.ParentRoom.area;
		int totalAttackTicks = 56;
		for (int i = 0; i < 3; i++)
		{
			for (int j = 0; j < 17; j++)
			{
				float angle = base.AimDirection;
				float timeUntilFire = (float)(totalAttackTicks - j) / 60f;
				if (j != 16)
				{
					int num = j / 2;
					bool flag = j % 2 == 1;
					Vector2 vector = IntVector2.CardinalsAndOrdinals[num].ToVector2();
					float num2 = ((!flag) ? 7f : 8.15f);
					Vector2 targetOrigin = BulletManager.PlayerPosition();
					Vector2 vector2 = vector.normalized * num2;
					targetOrigin += vector2 * timeUntilFire;
					Vector2 predictedPosition = BraveMathCollege.GetPredictedPosition(targetOrigin, BulletManager.PlayerVelocity(), base.Position, 60f);
					angle = (predictedPosition - base.Position).ToAngle();
				}
				for (int k = 0; k < j; k++)
				{
					if (!float.IsNaN(angles[k]) && BraveMathCollege.AbsAngleBetween(angles[k], angle) < 3f)
					{
						angle = float.NaN;
					}
				}
				angles[j] = angle;
				if (!float.IsNaN(angles[j]))
				{
					ResourcefulRatController component = base.BulletBank.GetComponent<ResourcefulRatController>();
					float num3 = 20f;
					Vector2 result = Vector2.zero;
					if (BraveMathCollege.LineSegmentRectangleIntersection(base.Position, base.Position + BraveMathCollege.DegreesToVector(angle, 60f), area.UnitBottomLeft, area.UnitTopRight - new Vector2(0f, 6f), ref result))
					{
						num3 = (result - base.Position).magnitude;
					}
					GameObject gameObject = SpawnManager.SpawnVFX(component.ReticleQuad);
					tk2dSlicedSprite component2 = gameObject.GetComponent<tk2dSlicedSprite>();
					component2.transform.position = new Vector3(base.Position.x, base.Position.y, base.Position.y) + (Vector3)BraveMathCollege.DegreesToVector(angle, 2f);
					component2.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
					component2.dimensions = new Vector2((num3 - 3f) * 16f, 5f);
					component2.UpdateZDepth();
					m_reticles.Add(gameObject);
				}
				if (j < 16)
				{
					yield return Wait(1);
				}
			}
			yield return Wait(15);
			CleanupReticles();
			yield return Wait(25);
			for (int l = 0; l < 17; l++)
			{
				if (!float.IsNaN(angles[l]))
				{
					Fire(new Offset(new Vector2(0.5f, 0f), angles[l], string.Empty), new Direction(angles[l]), new Speed(60f), new Bullet("dagger", true));
				}
			}
			yield return Wait(12);
			for (int m = 0; m < 17; m++)
			{
				if (!float.IsNaN(angles[m]))
				{
					Fire(new Offset(new Vector2(0.5f, 0f), angles[m], string.Empty), new Direction(angles[m]), new Speed(30f), new Bullet("dagger", true));
				}
			}
			yield return Wait(24);
		}
	}

	public override void OnForceEnded()
	{
		CleanupReticles();
	}

	public Vector2 GetPredictedTargetPosition(float leadAmount, float speed, float fireDelay)
	{
		Vector2 targetOrigin = BulletManager.PlayerPosition();
		Vector2 vector = BulletManager.PlayerVelocity();
		targetOrigin += vector * fireDelay;
		return BraveMathCollege.GetPredictedPosition(targetOrigin, BulletManager.PlayerVelocity(), base.Position, speed);
	}

	private void CleanupReticles()
	{
		for (int i = 0; i < m_reticles.Count; i++)
		{
			SpawnManager.Despawn(m_reticles[i]);
		}
		m_reticles.Clear();
	}
}
