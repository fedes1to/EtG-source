using System.Collections;
using Brave.BulletScript;
using Dungeonator;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalGuide/Clap1")]
public class BossFinalGuideClap1 : Script
{
	private class LightningBullet : Bullet
	{
		public LightningBullet()
			: base("lightning")
		{
		}

		protected override IEnumerator Top()
		{
			Projectile.specRigidbody.CollideWithTileMap = false;
			Projectile.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.HighObstacle));
			yield return Wait(65f / Speed * 60f);
			Vanish();
		}

		public override void OnBulletDestruction(DestroyType destroyType, SpeculativeRigidbody hitRigidbody, bool preventSpawningProjectiles)
		{
			if ((bool)Projectile && (bool)Projectile.specRigidbody)
			{
				Projectile.specRigidbody.RemoveCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.HighObstacle));
			}
		}
	}

	private const int NumBolts = 25;

	private const int BoltSpeed = 20;

	private Vector2 m_roomMin;

	private Vector2 m_roomMax;

	private int[] m_quarters = new int[4] { 0, 1, 2, 3 };

	private int m_quarterIndex = 4;

	protected override IEnumerator Top()
	{
		AIActor aiActor = base.BulletBank.aiActor;
		RoomHandler parentRoom = aiActor.ParentRoom;
		CellArea area = parentRoom.area;
		m_roomMin = area.basePosition.ToVector2();
		m_roomMax = (area.basePosition + area.dimensions).ToVector2();
		m_roomMin.x += 8f;
		m_roomMax.x -= 8f;
		m_roomMax.y -= 9f;
		for (int i = 0; i < 25; i++)
		{
			StartTask(FireBolt());
			yield return Wait(15);
		}
		yield return Wait(60);
	}

	private IEnumerator FireBolt()
	{
		float width = m_roomMax.x - m_roomMin.x;
		float quarterWidth = width / 4f;
		if (m_quarterIndex >= 4)
		{
			m_quarterIndex = 0;
			BraveUtility.RandomizeArray(m_quarters);
		}
		int quarter = m_quarters[m_quarterIndex];
		Vector2 firePos = new Vector2(Random.Range(m_roomMin.x + (float)quarter * quarterWidth, m_roomMin.x + (float)(quarter + 1) * quarterWidth), m_roomMax.y + 10f);
		m_quarterIndex++;
		for (int i = 0; i < 11; i++)
		{
			if (i < 6)
			{
				Fire(Offset.OverridePosition(firePos + new Vector2((float)i * 0.2f, 0f)), new Direction(-90f), new Speed(20f), new LightningBullet());
			}
			if (i == 5)
			{
				Fire(Offset.OverridePosition(firePos + new Vector2(0.5f, -0.1f)), new Direction(-90f), new Speed(20f), new LightningBullet());
			}
			if (i >= 5)
			{
				Fire(Offset.OverridePosition(firePos + new Vector2((float)(i - 5) * 0.2f, -0.2f)), new Direction(-90f), new Speed(20f), new LightningBullet());
			}
			yield return Wait(2);
		}
	}
}
