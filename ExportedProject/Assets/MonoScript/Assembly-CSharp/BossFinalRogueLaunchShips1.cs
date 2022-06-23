using System.Collections;
using Brave.BulletScript;
using UnityEngine;

public abstract class BossFinalRogueLaunchShips1 : Script
{
	public class Ship : Bullet
	{
		private float m_desiredXOffset;

		private float m_desiredYOffset;

		private int m_spawnTime;

		public Ship(float xOffset, float yOffset, int spawnDelay)
			: base("anActualSpaceship")
		{
			m_desiredXOffset = xOffset;
			m_desiredYOffset = yOffset;
			m_spawnTime = spawnDelay;
		}

		protected override IEnumerator Top()
		{
			SpeculativeRigidbody ownerRigidbody = Projectile.Owner.specRigidbody;
			GameObject exhaust = Projectile.transform.Find("Sprite/trail").gameObject;
			Vector2 ownerCenter4 = ownerRigidbody.UnitCenter;
			Vector2 startingOffset = base.Position - ownerCenter4;
			Projectile.ImmuneToBlanks = true;
			Speed = 4f;
			yield return Wait(10);
			ChangeDirection(new Direction((!(startingOffset.x > 0f)) ? 180 : 0), 10);
			yield return Wait(10);
			exhaust.SetActive(true);
			ownerCenter4 = ownerRigidbody.UnitCenter;
			Vector2 lerpStartOffset = base.Position - ownerCenter4;
			Vector2 desiredOffset = startingOffset + new Vector2(m_desiredXOffset, m_desiredYOffset);
			base.ManualControl = true;
			for (int k = 0; k < 60; k++)
			{
				ownerCenter4 = ownerRigidbody.UnitCenter;
				base.Position = ownerCenter4 + Vector2.Lerp(lerpStartOffset, desiredOffset, (float)k / 60f);
				yield return Wait(1);
			}
			ChangeDirection(new Direction(-90f), 10);
			for (int j = 0; j < 10; j++)
			{
				ownerCenter4 = ownerRigidbody.UnitCenter;
				base.Position = ownerCenter4 + desiredOffset;
				yield return Wait(1);
			}
			Direction = -90f;
			ownerRigidbody.CanCarry = true;
			Projectile.specRigidbody.CanBeCarried = true;
			ownerRigidbody.RegisterCarriedRigidbody(Projectile.specRigidbody);
			base.DisableMotion = true;
			int shootInterval = Random.Range(120, 241);
			for (int i = m_spawnTime; i < 900; i++)
			{
				base.Position = Projectile.specRigidbody.Position.UnitPosition;
				shootInterval--;
				if (shootInterval <= 0)
				{
					float aimDirection = GetAimDirection(Random.value, 12f);
					Fire(new Offset(-0.25f, 0.5f, 0f, string.Empty), new Direction(aimDirection), new Speed(12f), new Bullet("laserBlast"));
					ChangeDirection(new Direction(aimDirection), 5);
					shootInterval = Random.Range(120, 241);
				}
				yield return Wait(1);
			}
			ChangeDirection(new Direction(-90f), 5);
			yield return Wait(5);
			ownerRigidbody.DeregisterCarriedRigidbody(Projectile.specRigidbody);
			base.ManualControl = false;
			base.DisableMotion = false;
			Direction = -90f;
			Speed = 0f;
			ChangeSpeed(new Speed(10f), 30);
			yield return Wait(180);
			Vanish();
		}
	}

	private const int NumShipColumns = 4;

	private const int NumShipRows = 3;

	private const int LifeTime = 900;

	private const int MinShootInterval = 120;

	private const int MaxShootInterval = 240;

	private Vector2 ShipPosMin = new Vector2(6f, -2f);

	private Vector2 ShipPosMax = new Vector2(16f, 2f);

	protected override IEnumerator Top()
	{
		for (int y = 0; y < 3; y++)
		{
			for (int x = 0; x < 4; x++)
			{
				float dx = Mathf.Lerp(ShipPosMax.x, ShipPosMin.x, (float)x / 3f);
				float dy = Mathf.Lerp(ShipPosMin.y, ShipPosMax.y, (float)y / 2f);
				if (y % 2 == 1)
				{
					dx += 0.5f * (ShipPosMax.x - ShipPosMin.x) / 3f;
				}
				if (this is BossFinalRogueLaunchShipsLeft1)
				{
					dx *= -1f;
				}
				Fire(new Direction(-90f), new Speed(4f), new Ship(dx, dy, (y * 4 + x) * 15));
				yield return Wait(15);
			}
		}
		yield return Wait(60);
	}
}
