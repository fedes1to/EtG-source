using System.Collections;
using System.Collections.Generic;
using Brave.BulletScript;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/ResourcefulRat/QuickDaggers1")]
public class ResourcefulRatQuickDaggers1 : Script
{
	private const int NumWaves = 1;

	private const int NumDaggersPerWave = 4;

	private const int AttackDelay = 26;

	private const float DaggerSpeed = 60f;

	private List<GameObject> m_reticles = new List<GameObject>();

	protected override IEnumerator Top()
	{
		float[] angles = new float[4];
		CellArea area = base.BulletBank.aiActor.ParentRoom.area;
		for (int i = 0; i < 1; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				float num = base.AimDirection;
				float num2 = 13f / 30f;
				if (j != 3)
				{
					int num3 = j / 2;
					bool flag = j % 2 == 1;
					Vector2 vector = IntVector2.CardinalsAndOrdinals[num3].ToVector2();
					float num4 = ((!flag) ? 7f : 8.15f);
					Vector2 targetOrigin = BulletManager.PlayerPosition();
					Vector2 vector2 = vector.normalized * num4;
					targetOrigin += vector2 * num2;
					Vector2 predictedPosition = BraveMathCollege.GetPredictedPosition(targetOrigin, BulletManager.PlayerVelocity(), base.Position, 60f);
					num = (predictedPosition - base.Position).ToAngle();
				}
				for (int k = 0; k < j; k++)
				{
					if (!float.IsNaN(angles[k]) && BraveMathCollege.AbsAngleBetween(angles[k], num) < 3f)
					{
						num = float.NaN;
					}
				}
				angles[j] = num;
				if (!float.IsNaN(angles[j]))
				{
					ResourcefulRatController component = base.BulletBank.GetComponent<ResourcefulRatController>();
					float num5 = 20f;
					Vector2 result = Vector2.zero;
					if (BraveMathCollege.LineSegmentRectangleIntersection(base.Position, base.Position + BraveMathCollege.DegreesToVector(num, 60f), area.UnitBottomLeft, area.UnitTopRight - new Vector2(0f, 6f), ref result))
					{
						num5 = (result - base.Position).magnitude;
					}
					GameObject gameObject = SpawnManager.SpawnVFX(component.ReticleQuad);
					tk2dSlicedSprite component2 = gameObject.GetComponent<tk2dSlicedSprite>();
					component2.transform.position = new Vector3(base.Position.x, base.Position.y, base.Position.y) + (Vector3)BraveMathCollege.DegreesToVector(num, 2f);
					component2.transform.localRotation = Quaternion.Euler(0f, 0f, num);
					component2.dimensions = new Vector2((num5 - 3f) * 16f, 5f);
					component2.UpdateZDepth();
					m_reticles.Add(gameObject);
				}
			}
			yield return Wait(26);
			CleanupReticles();
			for (int l = 0; l < 4; l++)
			{
				if (!float.IsNaN(angles[l]))
				{
					Fire(new Offset(new Vector2(0.5f, 0f), angles[l], string.Empty), new Direction(angles[l]), new Speed(60f), new Bullet("dagger", true));
				}
			}
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
