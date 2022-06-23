using System.Collections;
using System.Collections.ObjectModel;
using Dungeonator;
using UnityEngine;

public class InstantlyDamageAllProjectile : Projectile
{
	public bool DoesWhiteFlash;

	public bool DoesCameraFlash;

	public bool DoesAmbientVFX;

	public float AmbientVFXTime;

	public GameObject AmbientVFX;

	public float minTimeBetweenAmbientVFX = 0.1f;

	public GameObject DamagedEnemyVFX;

	[Header("Radial Slow Options")]
	public bool DoesRadialSlow;

	[ShowInInspectorIf("DoesRadialSlow", false)]
	public float RadialSlowInTime;

	[ShowInInspectorIf("DoesRadialSlow", false)]
	public float RadialSlowHoldTime = 1f;

	[ShowInInspectorIf("DoesRadialSlow", false)]
	public float RadialSlowOutTime = 0.5f;

	[ShowInInspectorIf("DoesRadialSlow", false)]
	public float RadialSlowTimeModifier = 0.25f;

	private float m_ambientTimer;

	protected override void Move()
	{
		if (DoesWhiteFlash)
		{
			Pixelator.Instance.FadeToColor(0.1f, Color.white, true, 0.1f);
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
		RoomHandler absoluteRoom = base.transform.position.GetAbsoluteRoom();
		absoluteRoom.ApplyActionToNearbyEnemies(base.transform.position.XY(), 100f, ProcessEnemy);
		ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
		for (int num = allProjectiles.Count - 1; num >= 0; num--)
		{
			Projectile projectile = allProjectiles[num];
			if ((bool)projectile && projectile.collidesWithProjectiles && (!projectile.collidesOnlyWithPlayerProjectiles || base.Owner is PlayerController))
			{
				BounceProjModifier component = projectile.GetComponent<BounceProjModifier>();
				if ((bool)component)
				{
					if (component.numberOfBounces <= 0)
					{
						projectile.DieInAir();
					}
					else
					{
						projectile.Direction *= -1f;
						float num2 = projectile.Direction.ToAngle();
						if (shouldRotate)
						{
							base.transform.rotation = Quaternion.Euler(0f, 0f, num2);
						}
						projectile.Speed *= 1f - component.percentVelocityToLoseOnBounce;
						if ((bool)base.braveBulletScript && base.braveBulletScript.bullet != null)
						{
							base.braveBulletScript.bullet.Direction = num2;
							base.braveBulletScript.bullet.Speed *= 1f - component.percentVelocityToLoseOnBounce;
						}
						component.Bounce(this, projectile.specRigidbody.UnitCenter);
					}
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
		if ((bool)a && a.IsNormalEnemy && (bool)a.healthHaver && !a.IsGone)
		{
			if ((bool)base.Owner)
			{
				a.healthHaver.ApplyDamage(base.ModifiedDamage, Vector2.zero, base.OwnerName, damageTypes);
			}
			else
			{
				a.healthHaver.ApplyDamage(base.ModifiedDamage, Vector2.zero, "projectile", damageTypes);
			}
			if (DoesRadialSlow)
			{
				ApplySlowToEnemy(a);
			}
			if (AppliesStun && a.healthHaver.IsAlive && (bool)a.behaviorSpeculator && Random.value < StunApplyChance)
			{
				a.behaviorSpeculator.Stun(AppliedStunDuration);
			}
			if (DamagedEnemyVFX != null)
			{
				a.PlayEffectOnActor(DamagedEnemyVFX, Vector3.zero, false, true);
			}
		}
	}

	protected void ApplySlowToEnemy(AIActor target)
	{
		target.StartCoroutine(ProcessSlow(target));
	}

	private IEnumerator ProcessSlow(AIActor target)
	{
		float elapsed3 = 0f;
		if (RadialSlowInTime > 0f)
		{
			while (elapsed3 < RadialSlowInTime && (bool)target && !target.healthHaver.IsDead)
			{
				elapsed3 += BraveTime.DeltaTime;
				target.LocalTimeScale = Mathf.Lerp(t: elapsed3 / RadialSlowInTime, a: 1f, b: RadialSlowTimeModifier);
				yield return null;
			}
		}
		elapsed3 = 0f;
		if (RadialSlowHoldTime > 0f)
		{
			while (elapsed3 < RadialSlowHoldTime && (bool)target && !target.healthHaver.IsDead)
			{
				elapsed3 += BraveTime.DeltaTime;
				target.LocalTimeScale = RadialSlowTimeModifier;
				yield return null;
			}
		}
		elapsed3 = 0f;
		if (RadialSlowOutTime > 0f)
		{
			while (elapsed3 < RadialSlowOutTime && (bool)target && !target.healthHaver.IsDead)
			{
				elapsed3 += BraveTime.DeltaTime;
				target.LocalTimeScale = Mathf.Lerp(t: elapsed3 / RadialSlowOutTime, a: RadialSlowTimeModifier, b: 1f);
				yield return null;
			}
		}
		if ((bool)target)
		{
			target.LocalTimeScale = 1f;
		}
	}
}
