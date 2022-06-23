using System.Collections;
using System.Collections.Generic;
using Brave.BulletScript;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Infinilich/CarnageWalls1")]
public class InfinilichCarnageWalls1 : Script
{
	public class TipBullet : Bullet
	{
		private InfinilichCarnageWalls1 m_parentScript;

		public TipBullet(InfinilichCarnageWalls1 parentScript)
			: base("carnageTip")
		{
			m_parentScript = parentScript;
		}

		protected override IEnumerator Top()
		{
			int i = 0;
			while (true)
			{
				Fire(new Direction(0f, DirectionType.Relative), new Speed(), new ChainBullet(m_parentScript, this, i, Speed));
				yield return Wait(2);
				i++;
			}
		}
	}

	public class ChainBullet : Bullet
	{
		private const float WiggleMagnitude = 0.75f;

		private const float WigglePeriodMultiplier = 0.333f;

		private InfinilichCarnageWalls1 m_parentScript;

		private TipBullet m_tip;

		private int m_spawnDelay;

		private float m_tipSpeed;

		public ChainBullet(InfinilichCarnageWalls1 parentScript, TipBullet tip, int spawnDelay, float tipSpeed)
		{
			m_parentScript = parentScript;
			m_tip = tip;
			m_spawnDelay = spawnDelay;
			m_tipSpeed = tipSpeed;
		}

		protected override IEnumerator Top()
		{
			base.ManualControl = true;
			Vector2 truePosition = base.Position;
			float wigglePeriod = 0.333f * m_tipSpeed;
			float currentOffset = 0f;
			int j = 0;
			while (!m_tip.Destroyed)
			{
				float magnitude3 = 0.75f;
				magnitude3 = Mathf.Min(magnitude3, Mathf.Lerp(0f, 0.75f, (float)j / 20f));
				magnitude3 = Mathf.Min(magnitude3, Mathf.Lerp(0f, 0.75f, (float)m_spawnDelay / 10f));
				currentOffset = Mathf.SmoothStep(0f - magnitude3, magnitude3, Mathf.PingPong(0.5f + (float)j / 60f * wigglePeriod, 1f));
				base.Position = truePosition + BraveMathCollege.DegreesToVector(Direction - 90f, currentOffset);
				yield return Wait(1);
				j++;
			}
			float lastOffset = currentOffset;
			for (j = 0; j < 3; j++)
			{
				base.Position = truePosition + BraveMathCollege.DegreesToVector(magnitude: Mathf.Lerp(lastOffset, 0f, (float)j / 2f), angle: Direction - 90f);
				yield return Wait(1);
			}
			int holdTime = Random.Range(0, 240);
			int timer = 0;
			while (!m_parentScript.Done || timer < holdTime)
			{
				if (m_parentScript.Done)
				{
					timer++;
				}
				if (Random.value < 0.0001f)
				{
					Fire(new Direction(0f, DirectionType.Aim), new Speed(9f));
				}
				yield return Wait(1);
			}
			Vanish();
		}
	}

	public bool Done;

	private const float c_wallBuffer = 5f;

	private static List<Vector2> s_wallPoints = new List<Vector2>(4);

	protected override IEnumerator Top()
	{
		base.EndOnBlank = true;
		Fire(new Offset("limb 1"), new Direction(45f), new Speed(36f), new TipBullet(this));
		Fire(new Offset("limb 2"), new Direction(-45f), new Speed(36f), new TipBullet(this));
		Fire(new Offset("limb 3"), new Direction(-135f), new Speed(36f), new TipBullet(this));
		Fire(new Offset("limb 4"), new Direction(135f), new Speed(36f), new TipBullet(this));
		yield return Wait(60);
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				Vector2 startPoint;
				Vector2 endPoint;
				RandomWallPoints(out startPoint, out endPoint);
				Fire(Offset.OverridePosition(startPoint), new Direction((endPoint - startPoint).ToAngle()), new Speed(24f), new TipBullet(this));
			}
			yield return Wait(180);
		}
		Done = true;
		yield return Wait(60);
	}

	private void RandomWallPoints(out Vector2 startPoint, out Vector2 endPoint)
	{
		CellArea area = base.BulletBank.aiActor.ParentRoom.area;
		Vector2 vector = area.basePosition.ToVector2() + new Vector2(0.75f, 2f);
		Vector2 vector2 = (area.basePosition + area.dimensions).ToVector2() - new Vector2(0.75f, 0.5f);
		s_wallPoints.Clear();
		s_wallPoints.Add(new Vector2(Random.Range(vector.x + 5f, vector2.x - 5f), vector2.y));
		s_wallPoints.Add(new Vector2(Random.Range(vector.x + 5f, vector2.x - 5f), vector.y));
		s_wallPoints.Add(new Vector2(vector.x, Random.Range(vector.y + 5f, vector2.y - 5f)));
		s_wallPoints.Add(new Vector2(vector2.x, Random.Range(vector.y + 5f, vector2.y - 5f)));
		Vector2 vector3 = BraveUtility.RandomElement(s_wallPoints);
		s_wallPoints.Remove(vector3);
		startPoint = vector3;
		float num = 0f;
		int index = 0;
		for (int i = 0; i < s_wallPoints.Count; i++)
		{
			float num2 = Vector2Extensions.SqrDistance(s_wallPoints[i], startPoint);
			if (i == 0 || num2 < num)
			{
				num = num2;
				index = i;
			}
		}
		s_wallPoints.RemoveAt(index);
		endPoint = BraveUtility.RandomElement(s_wallPoints);
	}
}
