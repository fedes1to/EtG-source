using System;
using Dungeonator;
using UnityEngine;

public class SpawnProjModifier : MonoBehaviour
{
	public enum CollisionSpawnStyle
	{
		RADIAL,
		FLAK_BURST,
		REVERSE_FLAK_BURST
	}

	public bool PostprocessSpawnedProjectiles;

	[Header("Spawn in Flight")]
	public bool spawnProjectilesInFlight;

	public Projectile projectileToSpawnInFlight;

	public float inFlightSpawnCooldown = 1f;

	public float inFlightSpawnAngle = 90f;

	public Transform InFlightSourceTransform;

	public bool usesComplexSpawnInFlight;

	[ShowInInspectorIf("usesComplexSpawnInFlight", false)]
	public int numToSpawnInFlight = 2;

	[ShowInInspectorIf("usesComplexSpawnInFlight", false)]
	public bool fireRandomlyInAngle;

	[ShowInInspectorIf("usesComplexSpawnInFlight", false)]
	public bool inFlightAimAtEnemies;

	public string inFlightSpawnAnimation;

	[Header("Spawn on Collision")]
	public bool spawnProjectilesOnCollision;

	public CollisionSpawnStyle collisionSpawnStyle;

	public bool doOverrideObjectCollisionSpawnStyle;

	public CollisionSpawnStyle overrideObjectSpawnStyle;

	public bool spawnCollisionProjectilesOnBounce;

	public Projectile projectileToSpawnOnCollision;

	public bool UsesMultipleCollisionSpawnProjectiles;

	public Projectile[] collisionSpawnProjectiles;

	public int numberToSpawnOnCollison = 2;

	public int startAngle = 90;

	public bool randomRadialStartAngle;

	public bool spawnOnObjectCollisions = true;

	public bool alignToSurfaceNormal;

	public bool spawnProjecitlesOnDieInAir;

	public bool SpawnedProjectilesInheritData;

	[NonSerialized]
	public bool SpawnedProjectilesInheritAppearance;

	[NonSerialized]
	public float SpawnedProjectileScaleModifier = 1f;

	[Header("Audio")]
	public string spawnAudioEvent = string.Empty;

	private SpeculativeRigidbody m_srb;

	private Projectile p;

	private float elapsed;

	protected bool m_hasCheckedProjectile;

	protected Projectile m_projectile;

	private Vector2 SpawnPos
	{
		get
		{
			if ((bool)m_srb)
			{
				return m_srb.UnitCenter;
			}
			if ((bool)base.transform)
			{
				return base.transform.position.XY();
			}
			if ((bool)m_projectile)
			{
				return m_projectile.LastPosition;
			}
			return GameManager.Instance.BestActivePlayer.CenterPosition;
		}
	}

	private void Update()
	{
		if (p == null)
		{
			p = GetComponent<Projectile>();
		}
		if (m_srb == null)
		{
			m_srb = GetComponent<SpeculativeRigidbody>();
		}
		if (!spawnProjectilesInFlight)
		{
			return;
		}
		elapsed += BraveTime.DeltaTime;
		if (!(elapsed > inFlightSpawnCooldown))
		{
			return;
		}
		if (usesComplexSpawnInFlight)
		{
			elapsed -= inFlightSpawnCooldown;
			if (inFlightAimAtEnemies)
			{
				AIActor aIActor = null;
				RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY());
				for (int i = 0; i < numToSpawnInFlight; i++)
				{
					AIActor randomActiveEnemy = absoluteRoomFromPosition.GetRandomActiveEnemy(false);
					if (randomActiveEnemy != null && randomActiveEnemy != aIActor)
					{
						aIActor = randomActiveEnemy;
						SpawnProjectile(projectileToSpawnInFlight, SpawnPos.ToVector3ZUp(), BraveMathCollege.Atan2Degrees(randomActiveEnemy.CenterPosition - SpawnPos));
						continue;
					}
					break;
				}
			}
			else
			{
				for (int j = 0; j < numToSpawnInFlight; j++)
				{
					float num = inFlightSpawnAngle / (float)(numToSpawnInFlight - 1) * (float)j - inFlightSpawnAngle / 2f;
					if (fireRandomlyInAngle)
					{
						num = UnityEngine.Random.value * inFlightSpawnAngle - inFlightSpawnAngle / 2f;
					}
					SpawnProjectile(projectileToSpawnInFlight, SpawnPos.ToVector3ZUp(), p.transform.eulerAngles.z + num);
				}
			}
		}
		else if ((bool)InFlightSourceTransform)
		{
			elapsed -= inFlightSpawnCooldown;
			SpawnProjectile(projectileToSpawnInFlight, InFlightSourceTransform.position, InFlightSourceTransform.eulerAngles.z);
		}
		else
		{
			elapsed -= inFlightSpawnCooldown;
			SpawnProjectile(projectileToSpawnInFlight, SpawnPos.ToVector3ZUp(), p.transform.eulerAngles.z + inFlightSpawnAngle);
			SpawnProjectile(projectileToSpawnInFlight, SpawnPos.ToVector3ZUp(), p.transform.eulerAngles.z - inFlightSpawnAngle);
		}
		if (!string.IsNullOrEmpty(inFlightSpawnAnimation))
		{
			p.sprite.spriteAnimator.PlayForDuration(inFlightSpawnAnimation, -1f, p.sprite.spriteAnimator.CurrentClip.name, true);
		}
		if (!string.IsNullOrEmpty(spawnAudioEvent))
		{
			AkSoundEngine.PostEvent(spawnAudioEvent, base.gameObject);
		}
	}

	public void SpawnCollisionProjectiles(Vector2 contact, Vector2 normal, SpeculativeRigidbody collidedRigidbody, bool hitObject = false)
	{
		if ((bool)this && (bool)m_srb)
		{
			CollisionSpawnStyle collisionSpawnStyle = this.collisionSpawnStyle;
			if (hitObject && doOverrideObjectCollisionSpawnStyle)
			{
				collisionSpawnStyle = overrideObjectSpawnStyle;
			}
			switch (collisionSpawnStyle)
			{
			case CollisionSpawnStyle.RADIAL:
				HandleSpawnRadial(contact, normal, collidedRigidbody);
				break;
			case CollisionSpawnStyle.FLAK_BURST:
				HandleSpawnFlakBurst(contact, normal, collidedRigidbody);
				break;
			case CollisionSpawnStyle.REVERSE_FLAK_BURST:
				HandleReverseSpawnFlakBurst(contact, normal, collidedRigidbody);
				break;
			}
			if (!string.IsNullOrEmpty(spawnAudioEvent))
			{
				AkSoundEngine.PostEvent(spawnAudioEvent, base.gameObject);
			}
		}
	}

	private void HandleReverseSpawnFlakBurst(Vector2 contact, Vector2 normal, SpeculativeRigidbody collidedRigidbody)
	{
		int num = UnityEngine.Random.Range(0, 20);
		Vector2 unitBottomLeft = m_srb.UnitBottomLeft;
		Vector2 unitTopRight = m_srb.UnitTopRight;
		for (int i = 0; i < numberToSpawnOnCollison; i++)
		{
			Projectile proj = ((!UsesMultipleCollisionSpawnProjectiles) ? projectileToSpawnOnCollision : collisionSpawnProjectiles[UnityEngine.Random.Range(0, collisionSpawnProjectiles.Length)]);
			float num2 = 15f - BraveMathCollege.GetLowDiscrepancyRandom(i + num) * 30f;
			float num3 = BraveMathCollege.Atan2Degrees(normal) + num2;
			if (alignToSurfaceNormal)
			{
				num3 = BraveMathCollege.Atan2Degrees(-1f * normal) + num2;
			}
			Vector2 vector = new Vector2(UnityEngine.Random.Range(unitBottomLeft.x, unitTopRight.x), UnityEngine.Random.Range(unitBottomLeft.y, unitTopRight.y));
			SpawnProjectile(proj, vector.ToVector3ZUp(base.transform.position.z), 180f + num3, collidedRigidbody);
		}
	}

	private void HandleSpawnFlakBurst(Vector2 contact, Vector2 normal, SpeculativeRigidbody collidedRigidbody)
	{
		int num = UnityEngine.Random.Range(0, 20);
		Vector2 unitBottomLeft = m_srb.UnitBottomLeft;
		Vector2 unitTopRight = m_srb.UnitTopRight;
		for (int i = 0; i < numberToSpawnOnCollison; i++)
		{
			Projectile proj = ((!UsesMultipleCollisionSpawnProjectiles) ? projectileToSpawnOnCollision : collisionSpawnProjectiles[UnityEngine.Random.Range(0, collisionSpawnProjectiles.Length)]);
			float num2 = 15f - BraveMathCollege.GetLowDiscrepancyRandom(i + num) * 30f;
			float zRotation = BraveMathCollege.Atan2Degrees(normal) + num2;
			Vector2 vector = new Vector2(UnityEngine.Random.Range(unitBottomLeft.x, unitTopRight.x), UnityEngine.Random.Range(unitBottomLeft.y, unitTopRight.y));
			SpawnProjectile(proj, vector.ToVector3ZUp(base.transform.position.z), zRotation, collidedRigidbody);
		}
	}

	private void HandleSpawnRadial(Vector2 contact, Vector2 normal, SpeculativeRigidbody collidedRigidbody)
	{
		float num = 360f / (float)numberToSpawnOnCollison;
		for (int i = 0; i < numberToSpawnOnCollison; i++)
		{
			Projectile proj = ((!UsesMultipleCollisionSpawnProjectiles) ? projectileToSpawnOnCollision : collisionSpawnProjectiles[UnityEngine.Random.Range(0, collisionSpawnProjectiles.Length)]);
			float num2 = 0.5f;
			if (randomRadialStartAngle)
			{
				num2 = UnityEngine.Random.Range(0, 360);
			}
			float zRotation = ((!alignToSurfaceNormal) ? (p.transform.eulerAngles.z + num2 + (float)startAngle + num * (float)i) : (Mathf.Atan2(normal.y, normal.x) * 57.29578f + num2 + (float)startAngle + num * (float)i));
			SpawnProjectile(proj, (contact + normal * 0.5f).ToVector3ZUp(base.transform.position.z), zRotation, collidedRigidbody);
		}
	}

	private void SpawnProjectile(Projectile proj, Vector3 spawnPosition, float zRotation, SpeculativeRigidbody collidedRigidbody = null)
	{
		GameObject gameObject = SpawnManager.SpawnProjectile(proj.gameObject, spawnPosition, Quaternion.Euler(0f, 0f, zRotation));
		Projectile component = gameObject.GetComponent<Projectile>();
		if ((bool)component)
		{
			component.SpawnedFromOtherPlayerProjectile = true;
			if (component is HelixProjectile)
			{
				component.Inverted = UnityEngine.Random.value < 0.5f;
			}
		}
		if (!m_hasCheckedProjectile)
		{
			m_hasCheckedProjectile = true;
			m_projectile = GetComponent<Projectile>();
		}
		if ((bool)m_projectile && PostprocessSpawnedProjectiles && (bool)m_projectile.Owner && m_projectile.Owner is PlayerController)
		{
			PlayerController playerController = m_projectile.Owner as PlayerController;
			playerController.DoPostProcessProjectile(component);
		}
		if (SpawnedProjectilesInheritAppearance && (bool)component.sprite && (bool)m_projectile.sprite)
		{
			component.shouldRotate = m_projectile.shouldRotate;
			component.shouldFlipHorizontally = m_projectile.shouldFlipHorizontally;
			component.shouldFlipVertically = m_projectile.shouldFlipVertically;
			component.sprite.SetSprite(m_projectile.sprite.Collection, m_projectile.sprite.spriteId);
			Vector2 vector = component.transform.position.XY() - component.sprite.WorldCenter;
			component.transform.position += vector.ToVector3ZUp();
			component.specRigidbody.Reinitialize();
		}
		if (SpawnedProjectileScaleModifier != 1f)
		{
			component.AdditionalScaleMultiplier *= SpawnedProjectileScaleModifier;
		}
		if ((bool)m_projectile && m_projectile.GetCachedBaseDamage > 0f)
		{
			component.baseData.damage = component.baseData.damage * Mathf.Min(m_projectile.baseData.damage / m_projectile.GetCachedBaseDamage, 1f);
		}
		if ((bool)p)
		{
			component.Owner = p.Owner;
			component.Shooter = p.Shooter;
			if (component is RobotechProjectile)
			{
				RobotechProjectile robotechProjectile = component as RobotechProjectile;
				robotechProjectile.initialOverrideTargetPoint = spawnPosition.XY() + (Quaternion.Euler(0f, 0f, zRotation) * Vector2.right * 10f).XY();
			}
			if (SpawnedProjectilesInheritData)
			{
				component.baseData.damage = Mathf.Max(component.baseData.damage, p.baseData.damage / (float)numberToSpawnOnCollison);
				component.baseData.speed = Mathf.Max(component.baseData.speed, p.baseData.speed / ((float)numberToSpawnOnCollison / 2f));
				component.baseData.force = Mathf.Max(component.baseData.force, p.baseData.force / (float)numberToSpawnOnCollison);
			}
		}
		if ((bool)component.specRigidbody)
		{
			if ((bool)collidedRigidbody)
			{
				component.specRigidbody.RegisterTemporaryCollisionException(collidedRigidbody, 0.25f, 0.5f);
			}
			PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(component.specRigidbody);
		}
	}
}
