using System;
using System.Collections;
using Brave.BulletScript;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/ResourcefulRat/MouseTraps1")]
public class ResourcefulRatMouseTraps1 : Script
{
	private class MouseTrapBullet : Bullet
	{
		private Vector2 m_goalPos;

		private GameObject m_mouseTrapPrefab;

		public MouseTrapBullet(Vector2 goalPos, GameObject mouseTrapPrefab)
			: base("mousetrap", true)
		{
			m_goalPos = goalPos;
			m_mouseTrapPrefab = mouseTrapPrefab;
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
			GameObject go = UnityEngine.Object.Instantiate(m_mouseTrapPrefab, Projectile.specRigidbody.UnitCenter + new Vector2(-0.8f, -1.2f), Quaternion.identity);
			go.GetComponent<SpeculativeRigidbody>().Initialize();
			Vanish(true);
		}
	}

	private const int FlightTime = 60;

	private const int NumTraps = 10;

	private static int[] s_xValues;

	private static int[] s_yValues;

	protected override IEnumerator Top()
	{
		yield return Wait(56);
		if (s_xValues == null || s_yValues == null)
		{
			s_xValues = new int[10];
			s_yValues = new int[10];
			for (int j = 0; j < 10; j++)
			{
				s_xValues[j] = j;
				s_yValues[j] = j;
			}
		}
		CellArea area = base.BulletBank.aiActor.ParentRoom.area;
		Vector2 roomLowerLeft = area.UnitBottomLeft;
		Vector2 dimensions = area.dimensions.ToVector2() - new Vector2(0f, 6f);
		Vector2 delta = new Vector2(dimensions.x / 10f, dimensions.y / 10f);
		Vector2 safeZoneLowerLeft = area.UnitBottomLeft + new Vector2(15f, 9f);
		Vector2 safeZoneUpperRight = area.UnitBottomLeft + new Vector2(21f, 15f);
		BraveUtility.RandomizeArray(s_xValues);
		BraveUtility.RandomizeArray(s_yValues);
		for (int i = 0; i < 10; i++)
		{
			int baseX = s_xValues[i];
			int baseY = s_yValues[i];
			Vector2 goalPos = roomLowerLeft + new Vector2(((float)baseX + UnityEngine.Random.value) * delta.x, ((float)baseY + UnityEngine.Random.value) * delta.y);
			if (goalPos.IsWithin(safeZoneLowerLeft, safeZoneUpperRight))
			{
				if (BraveUtility.RandomBool())
				{
					baseX += (int)(BraveUtility.RandomSign() * 3f);
				}
				else
				{
					baseY += (int)(BraveUtility.RandomSign() * 3f);
				}
				goalPos = roomLowerLeft + new Vector2(((float)baseX + UnityEngine.Random.value) * delta.x, ((float)baseY + UnityEngine.Random.value) * delta.y);
			}
			Fire(goalPos);
			yield return Wait(2);
		}
	}

	private void Fire(Vector2 goal)
	{
		float direction = (goal - base.Position).ToAngle();
		GameObject[] mouseTraps = base.BulletBank.GetComponent<ResourcefulRatController>().MouseTraps;
		Fire(new Direction(direction), new MouseTrapBullet(goal, BraveUtility.RandomElement(mouseTraps)));
	}
}
