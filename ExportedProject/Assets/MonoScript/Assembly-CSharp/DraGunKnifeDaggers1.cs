using System.Collections;
using System.Collections.Generic;
using Brave.BulletScript;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/DraGun/KnifeDaggers1")]
public class DraGunKnifeDaggers1 : Script
{
	private const int NumWaves = 1;

	private const int NumDaggersPerWave = 7;

	private const int AttackDelay = 42;

	private const float DaggerSpeed = 60f;

	private List<LineReticleController> m_reticles = new List<LineReticleController>();

	protected override IEnumerator Top()
	{
		float[] angles = new float[7];
		CellArea area = base.BulletBank.aiActor.ParentRoom.area;
		Vector2 roomLowerLeft = area.UnitBottomLeft + new Vector2(0f, 19f);
		Vector2 roomUpperRight = roomLowerLeft + new Vector2(36f, 14f);
		DraGunKnifeController knifeController = base.BulletBank.GetComponent<DraGunKnifeController>();
		for (int i = 0; i < 1; i++)
		{
			for (int j = 0; j < 7; j++)
			{
				float num = base.AimDirection;
				float num2 = 0.7f;
				if (j != 6)
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
					float num5 = 20f;
					Vector2 result = Vector2.zero;
					if (BraveMathCollege.LineSegmentRectangleIntersection(base.Position, base.Position + BraveMathCollege.DegreesToVector(num, 60f), roomLowerLeft, roomUpperRight, ref result))
					{
						num5 = (result - base.Position).magnitude;
					}
					GameObject gameObject = SpawnManager.SpawnVFX(knifeController.ReticleQuad);
					LineReticleController component = gameObject.GetComponent<LineReticleController>();
					component.Init(new Vector3(base.Position.x, base.Position.y, base.Position.y) + (Vector3)BraveMathCollege.DegreesToVector(num, 2f), Quaternion.Euler(0f, 0f, num), num5 - 3f);
					m_reticles.Add(component);
				}
			}
			yield return Wait(37);
			CleanupReticles();
			yield return Wait(5);
			for (int l = 0; l < 7; l++)
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
			m_reticles[i].Cleanup();
		}
		m_reticles.Clear();
	}
}
