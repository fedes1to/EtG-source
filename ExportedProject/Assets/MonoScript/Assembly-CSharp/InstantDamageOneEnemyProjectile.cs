using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class InstantDamageOneEnemyProjectile : Projectile
{
	public bool DoesWhiteFlash;

	public bool DoesCameraFlash;

	public bool DoesStickyFriction;

	public float StickyFrictionDuration = 0.6f;

	public bool DoesAmbientVFX;

	public float AmbientVFXTime;

	public GameObject AmbientVFX;

	public float minTimeBetweenAmbientVFX = 0.1f;

	public GameObject DamagedEnemyVFX;

	private float m_ambientTimer;

	protected override void Move()
	{
		if (DoesWhiteFlash)
		{
			Pixelator.Instance.FadeToColor(0.1f, Color.white.WithAlpha(0.25f), true, 0.1f);
		}
		if (DoesCameraFlash)
		{
			StickyFrictionManager.Instance.RegisterCustomStickyFriction(0.125f, 0f, false);
			Pixelator.Instance.TimedFreezeFrame(0.25f, 0.125f);
		}
		if (DoesAmbientVFX && AmbientVFXTime > 0f && AmbientVFX != null)
		{
			GameManager.Instance.Dungeon.StartCoroutine(HandleAmbientSpawnTime(base.transform.position, AmbientVFXTime));
		}
		if (DoesStickyFriction)
		{
			StickyFrictionManager.Instance.RegisterCustomStickyFriction(StickyFrictionDuration, 0f, true);
		}
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		if (absoluteRoom != null)
		{
			List<AIActor> activeEnemies = absoluteRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			if (activeEnemies != null)
			{
				AIActor aIActor = null;
				float num = float.MaxValue;
				Vector2 b = base.Owner.CenterPosition;
				if (base.Owner is PlayerController)
				{
					b = (base.Owner as PlayerController).unadjustedAimPoint.XY();
				}
				for (int i = 0; i < activeEnemies.Count; i++)
				{
					if ((bool)activeEnemies[i] && activeEnemies[i].IsNormalEnemy && (bool)activeEnemies[i].healthHaver && activeEnemies[i].isActiveAndEnabled)
					{
						float num2 = Vector2.Distance(activeEnemies[i].CenterPosition, b);
						if (num2 < num)
						{
							num = num2;
							aIActor = activeEnemies[i];
						}
					}
				}
				if ((bool)aIActor)
				{
					ProcessEnemy(aIActor, 0f);
				}
			}
		}
		DieInAir();
	}

	protected void HandleAmbientVFXSpawn(Vector2 centerPoint, float radius)
	{
		if (!(AmbientVFX == null))
		{
			bool flag = false;
			m_ambientTimer -= BraveTime.DeltaTime;
			if (m_ambientTimer <= 0f)
			{
				flag = true;
				m_ambientTimer = minTimeBetweenAmbientVFX;
			}
			if (flag)
			{
				Vector2 vector = centerPoint + Random.insideUnitCircle * radius;
				SpawnManager.SpawnVFX(AmbientVFX, vector, Quaternion.identity);
			}
		}
	}

	protected IEnumerator HandleAmbientSpawnTime(Vector2 centerPoint, float remainingTime)
	{
		float elapsed = 0f;
		while (elapsed < remainingTime)
		{
			elapsed += BraveTime.DeltaTime;
			HandleAmbientVFXSpawn(centerPoint, 10f);
			yield return null;
		}
	}

	public void ProcessEnemy(AIActor a, float b)
	{
		if ((bool)a && a.IsNormalEnemy && (bool)a.healthHaver)
		{
			if ((bool)base.Owner)
			{
				a.healthHaver.ApplyDamage(base.ModifiedDamage, Vector2.zero, base.OwnerName, damageTypes);
				base.LastVelocity = (a.CenterPosition - base.Owner.CenterPosition).normalized;
				HandleKnockback(a.specRigidbody, base.Owner as PlayerController, true);
			}
			else
			{
				a.healthHaver.ApplyDamage(base.ModifiedDamage, Vector2.zero, "projectile", damageTypes);
			}
			if (DamagedEnemyVFX != null)
			{
				a.PlayEffectOnActor(DamagedEnemyVFX, Vector3.zero, false, true);
			}
		}
	}
}
