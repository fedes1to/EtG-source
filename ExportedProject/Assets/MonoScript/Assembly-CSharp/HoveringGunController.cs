using System.Collections;
using Dungeonator;
using UnityEngine;

public class HoveringGunController : BraveBehaviour, IPlayerOrbital
{
	public enum HoverPosition
	{
		OVERHEAD,
		CIRCULATE
	}

	public enum FireType
	{
		ON_RELOAD,
		ON_COOLDOWN,
		ON_DODGED_BULLET,
		ON_FIRED_GUN
	}

	public enum AimType
	{
		NEAREST_ENEMY,
		PLAYER_AIM
	}

	public HoverPosition Position;

	public FireType Trigger;

	public AimType Aim;

	public float AimRotationAngularSpeed = 360f;

	public float ShootDuration = 2f;

	public float CooldownTime = 1f;

	public bool OnlyOnEmptyReload;

	public bool ConsumesTargetGunAmmo;

	public float ChanceToConsumeTargetGunAmmo = 1f;

	public string ShootAudioEvent;

	public string OnEveryShotAudioEvent;

	public string FinishedShootingAudioEvent;

	private bool m_initialized;

	private Transform m_parentTransform;

	private Transform m_shootPointTransform;

	private Gun m_targetGun;

	private PlayerController m_owner;

	private float m_currentAimTarget;

	private bool m_hasEnemyTarget;

	private float m_fireCooldown;

	private Vector2 m_ownerCenterAverage;

	private float m_orbitalAngle;

	private int m_orbitalTier;

	private int m_orbitalTierIndex;

	private Vector2 ShootPoint
	{
		get
		{
			return m_shootPointTransform.position.XY();
		}
	}

	public void Initialize(Gun targetGun, PlayerController owner)
	{
		m_targetGun = targetGun;
		m_owner = owner;
		m_parentTransform = new GameObject("hover rotator").transform;
		m_parentTransform.parent = base.transform.parent;
		base.transform.parent = m_parentTransform;
		base.sprite.PlaceAtLocalPositionByAnchor(Vector3.zero, tk2dBaseSprite.Anchor.MiddleCenter);
		base.sprite.SetSprite(targetGun.sprite.Collection, targetGun.sprite.spriteId);
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
		m_shootPointTransform = new GameObject("shoot point").transform;
		m_shootPointTransform.parent = base.transform;
		m_shootPointTransform.localPosition = targetGun.barrelOffset.localPosition;
		if (Position == HoverPosition.CIRCULATE)
		{
			SetOrbitalTier(PlayerOrbital.CalculateTargetTier(m_owner, this));
			SetOrbitalTierIndex(PlayerOrbital.GetNumberOfOrbitalsInTier(m_owner, GetOrbitalTier()));
			m_owner.orbitals.Add(this);
			m_ownerCenterAverage = m_owner.CenterPosition;
		}
		if (Trigger == FireType.ON_DODGED_BULLET)
		{
			m_owner.OnDodgedProjectile += HandleDodgedProjectileFire;
		}
		if (Trigger == FireType.ON_FIRED_GUN)
		{
			m_owner.PostProcessProjectile += HandleFiredGun;
		}
		if (Aim == AimType.NEAREST_ENEMY)
		{
			m_fireCooldown = 0.25f;
		}
		UpdatePosition();
		LootEngine.DoDefaultSynergyPoof(base.sprite.WorldCenter);
		m_initialized = true;
	}

	private void HandleFiredGun(Projectile arg1, float arg2)
	{
		if (m_fireCooldown <= 0f)
		{
			Fire();
		}
	}

	private void HandleDodgedProjectileFire(Projectile sourceProjectile)
	{
		if (m_fireCooldown <= 0f && sourceProjectile.collidesWithPlayer)
		{
			Fire();
		}
	}

	public void LateUpdate()
	{
		if (m_initialized && !Dungeon.IsGenerating && !GameManager.Instance.IsLoadingLevel)
		{
			UpdatePosition();
			UpdateFiring();
		}
	}

	private void AimAt(Vector2 point, bool instant = false)
	{
		Vector2 v = point - base.sprite.WorldCenter;
		float num = (m_currentAimTarget = BraveMathCollege.Atan2Degrees(v));
		if (instant)
		{
			m_parentTransform.localRotation = Quaternion.Euler(0f, 0f, m_currentAimTarget);
		}
	}

	private void UpdatePosition()
	{
		switch (Aim)
		{
		case AimType.NEAREST_ENEMY:
		{
			bool flag = false;
			if ((bool)m_owner && m_owner.CurrentRoom != null)
			{
				float nearestDistance = -1f;
				AIActor nearestEnemy = m_owner.CurrentRoom.GetNearestEnemy(m_owner.CenterPosition, out nearestDistance);
				if ((bool)nearestEnemy)
				{
					m_hasEnemyTarget = true;
					AimAt(nearestEnemy.CenterPosition);
					flag = true;
				}
			}
			if (!flag)
			{
				m_hasEnemyTarget = false;
				AimAt(m_owner.unadjustedAimPoint.XY());
			}
			break;
		}
		case AimType.PLAYER_AIM:
			AimAt(m_owner.unadjustedAimPoint.XY());
			break;
		}
		m_parentTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.MoveTowardsAngle(m_parentTransform.localRotation.eulerAngles.z, m_currentAimTarget, AimRotationAngularSpeed * BraveTime.DeltaTime));
		bool flag2 = m_parentTransform.localRotation.eulerAngles.z > 90f && m_parentTransform.localRotation.eulerAngles.z < 270f;
		if (flag2 && !base.sprite.FlipY)
		{
			base.transform.localPosition += new Vector3(0f, base.sprite.GetUntrimmedBounds().extents.y, 0f);
			m_shootPointTransform.localPosition = m_shootPointTransform.localPosition.WithY(0f - m_shootPointTransform.localPosition.y);
			base.sprite.FlipY = true;
		}
		else if (!flag2 && base.sprite.FlipY)
		{
			base.sprite.FlipY = false;
			base.transform.localPosition -= new Vector3(0f, base.sprite.GetUntrimmedBounds().extents.y, 0f);
			m_shootPointTransform.localPosition = m_shootPointTransform.localPosition.WithY(0f - m_shootPointTransform.localPosition.y);
		}
		switch (Position)
		{
		case HoverPosition.OVERHEAD:
			m_parentTransform.position = (m_owner.CenterPosition + new Vector2(0f, 1.5f)).ToVector3ZisY();
			base.sprite.HeightOffGround = 2f;
			base.sprite.UpdateZDepth();
			break;
		case HoverPosition.CIRCULATE:
			HandleOrbitalMotion();
			break;
		}
	}

	private void HandleOrbitalMotion()
	{
		Vector2 centerPosition = m_owner.CenterPosition;
		if (Vector2.Distance(centerPosition, m_parentTransform.position.XY()) > 20f)
		{
			m_parentTransform.position = centerPosition.ToVector3ZUp();
			m_ownerCenterAverage = centerPosition;
			if ((bool)base.specRigidbody)
			{
				base.specRigidbody.Reinitialize();
			}
		}
		Vector2 vector = centerPosition - m_ownerCenterAverage;
		float num = Mathf.Lerp(0.1f, 15f, vector.magnitude / 4f);
		float num2 = Mathf.Min(num * BraveTime.DeltaTime, vector.magnitude);
		float num3 = 360f / (float)PlayerOrbital.GetNumberOfOrbitalsInTier(m_owner, GetOrbitalTier()) * (float)GetOrbitalTierIndex() + BraveTime.ScaledTimeSinceStartup * GetOrbitalRotationalSpeed();
		Vector2 vector2 = m_ownerCenterAverage + (centerPosition - m_ownerCenterAverage).normalized * num2;
		Vector2 vector3 = vector2 + (Quaternion.Euler(0f, 0f, num3) * Vector3.right * GetOrbitalRadius()).XY();
		m_ownerCenterAverage = vector2;
		vector3 = vector3.Quantize(0.0625f);
		Vector2 velocity = (vector3 - m_parentTransform.position.XY()) / BraveTime.DeltaTime;
		if ((bool)base.specRigidbody)
		{
			base.specRigidbody.Velocity = velocity;
		}
		else
		{
			m_parentTransform.position = vector3.ToVector3ZisY();
			base.sprite.HeightOffGround = 0.5f;
			base.sprite.UpdateZDepth();
		}
		m_orbitalAngle = num3 % 360f;
	}

	private void UpdateFiring()
	{
		if (m_fireCooldown <= 0f)
		{
			switch (Trigger)
			{
			case FireType.ON_RELOAD:
				if ((bool)m_owner && (bool)m_owner.CurrentGun && m_owner.CurrentGun.IsReloading && (!OnlyOnEmptyReload || m_owner.CurrentGun.ClipShotsRemaining <= 0))
				{
					Fire();
				}
				break;
			case FireType.ON_COOLDOWN:
				if (Aim != 0 || m_hasEnemyTarget)
				{
					Fire();
				}
				break;
			}
		}
		else
		{
			m_fireCooldown = (m_fireCooldown -= BraveTime.DeltaTime);
		}
	}

	private void Fire()
	{
		m_fireCooldown = CooldownTime;
		Projectile currentProjectile = m_targetGun.DefaultModule.GetCurrentProjectile();
		bool flag = currentProjectile.GetComponent<BeamController>() != null;
		if (!string.IsNullOrEmpty(ShootAudioEvent))
		{
			AkSoundEngine.PostEvent(ShootAudioEvent, base.gameObject);
		}
		if (flag)
		{
			m_owner.StartCoroutine(HandleFireShortBeam(currentProjectile, m_owner, ShootDuration));
			m_fireCooldown = Mathf.Max(m_fireCooldown, ShootDuration);
			return;
		}
		if (m_targetGun.Volley != null)
		{
			if (ShootDuration > 0f)
			{
				StartCoroutine(FireVolleyForDuration(m_targetGun.Volley, m_owner, ShootDuration));
			}
			else
			{
				FireVolley(m_targetGun.Volley, m_owner, m_parentTransform.eulerAngles.z, ShootPoint);
			}
			return;
		}
		ProjectileModule defaultModule = m_targetGun.DefaultModule;
		Projectile currentProjectile2 = defaultModule.GetCurrentProjectile();
		if ((bool)currentProjectile2)
		{
			float angleForShot = defaultModule.GetAngleForShot();
			if (!flag)
			{
				DoSingleProjectile(currentProjectile2, m_owner, m_parentTransform.eulerAngles.z + angleForShot, ShootPoint, true);
			}
		}
	}

	private IEnumerator HandleFireShortBeam(Projectile projectileToSpawn, PlayerController source, float duration)
	{
		float elapsed = 0f;
		BeamController beam = BeginFiringBeam(projectileToSpawn, source, m_parentTransform.eulerAngles.z, ShootPoint);
		yield return null;
		while (elapsed < duration && (bool)m_shootPointTransform && (bool)this && (bool)m_parentTransform)
		{
			elapsed += BraveTime.DeltaTime;
			ContinueFiringBeam(beam, source, m_parentTransform.eulerAngles.z, ShootPoint);
			yield return null;
		}
		CeaseBeam(beam);
		if (!string.IsNullOrEmpty(FinishedShootingAudioEvent) && (bool)this)
		{
			AkSoundEngine.PostEvent(FinishedShootingAudioEvent, base.gameObject);
		}
	}

	private IEnumerator FireVolleyForDuration(ProjectileVolleyData volley, PlayerController source, float duration)
	{
		float elapsed = 0f;
		float cooldown2 = 0f;
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			cooldown2 -= BraveTime.DeltaTime;
			if (cooldown2 <= 0f)
			{
				FireVolley(volley, source, m_parentTransform.eulerAngles.z, ShootPoint);
				cooldown2 = m_targetGun.DefaultModule.cooldownTime;
				for (int i = 0; i < volley.projectiles.Count; i++)
				{
					if (volley.projectiles[i].shootStyle == ProjectileModule.ShootStyle.Charged)
					{
						cooldown2 = Mathf.Max(cooldown2, volley.projectiles[i].maxChargeTime);
						cooldown2 = Mathf.Max(cooldown2, 0.5f);
					}
				}
			}
			yield return null;
		}
		m_fireCooldown = CooldownTime;
		if (!string.IsNullOrEmpty(FinishedShootingAudioEvent))
		{
			AkSoundEngine.PostEvent(FinishedShootingAudioEvent, base.gameObject);
		}
	}

	private void FireVolley(ProjectileVolleyData volley, PlayerController source, float targetAngle, Vector2? overrideSpawnPoint)
	{
		if (!string.IsNullOrEmpty(OnEveryShotAudioEvent))
		{
			AkSoundEngine.PostEvent(OnEveryShotAudioEvent, base.gameObject);
		}
		for (int i = 0; i < volley.projectiles.Count; i++)
		{
			ProjectileModule projectileModule = volley.projectiles[i];
			Projectile currentProjectile = projectileModule.GetCurrentProjectile();
			if ((bool)currentProjectile)
			{
				float angleForShot = projectileModule.GetAngleForShot();
				if (!(currentProjectile.GetComponent<BeamController>() != null))
				{
					DoSingleProjectile(currentProjectile, source, targetAngle + angleForShot, overrideSpawnPoint);
				}
			}
		}
	}

	private void DoSingleProjectile(Projectile projectileToSpawn, PlayerController source, float targetAngle, Vector2? overrideSpawnPoint, bool doAudio = false)
	{
		if (doAudio && !string.IsNullOrEmpty(OnEveryShotAudioEvent))
		{
			AkSoundEngine.PostEvent(OnEveryShotAudioEvent, base.gameObject);
		}
		if (ConsumesTargetGunAmmo && (bool)m_targetGun && m_owner.inventory.AllGuns.Contains(m_targetGun))
		{
			if (m_targetGun.ammo == 0)
			{
				return;
			}
			if (Random.value < ChanceToConsumeTargetGunAmmo)
			{
				m_targetGun.LoseAmmo(1);
			}
		}
		Vector2 vector = ((!overrideSpawnPoint.HasValue) ? source.specRigidbody.UnitCenter : overrideSpawnPoint.Value);
		GameObject gameObject = SpawnManager.SpawnProjectile(projectileToSpawn.gameObject, vector, Quaternion.Euler(0f, 0f, targetAngle));
		Projectile component = gameObject.GetComponent<Projectile>();
		component.Owner = source;
		component.Shooter = source.specRigidbody;
		source.DoPostProcessProjectile(component);
		BounceProjModifier component2 = component.GetComponent<BounceProjModifier>();
		if ((bool)component2)
		{
			component2.numberOfBounces = Mathf.Min(3, component2.numberOfBounces);
		}
	}

	private BeamController BeginFiringBeam(Projectile projectileToSpawn, PlayerController source, float targetAngle, Vector2? overrideSpawnPoint)
	{
		Vector2 vector = ((!overrideSpawnPoint.HasValue) ? source.CenterPosition : overrideSpawnPoint.Value);
		GameObject gameObject = SpawnManager.SpawnProjectile(projectileToSpawn.gameObject, vector, Quaternion.identity);
		Projectile component = gameObject.GetComponent<Projectile>();
		component.Owner = source;
		BeamController component2 = gameObject.GetComponent<BeamController>();
		component2.Owner = source;
		component2.HitsPlayers = false;
		component2.HitsEnemies = true;
		Vector3 vector2 = BraveMathCollege.DegreesToVector(targetAngle);
		component2.Direction = vector2;
		component2.Origin = vector;
		return component2;
	}

	private void ContinueFiringBeam(BeamController beam, PlayerController source, float angle, Vector2? overrideSpawnPoint)
	{
		Vector2 vector = ((!overrideSpawnPoint.HasValue) ? source.CenterPosition : overrideSpawnPoint.Value);
		beam.Direction = BraveMathCollege.DegreesToVector(angle);
		beam.Origin = vector;
		beam.LateUpdatePosition(vector);
	}

	private void CeaseBeam(BeamController beam)
	{
		beam.CeaseAttack();
	}

	protected override void OnDestroy()
	{
		if ((bool)m_owner)
		{
			m_owner.OnDodgedProjectile -= HandleDodgedProjectileFire;
		}
		if ((bool)m_owner)
		{
			m_owner.PostProcessProjectile -= HandleFiredGun;
		}
		if (!string.IsNullOrEmpty(FinishedShootingAudioEvent))
		{
			AkSoundEngine.PostEvent(FinishedShootingAudioEvent, base.gameObject);
		}
		if (Position == HoverPosition.CIRCULATE)
		{
			for (int i = 0; i < m_owner.orbitals.Count; i++)
			{
				if (m_owner.orbitals[i].GetOrbitalTier() == GetOrbitalTier() && m_owner.orbitals[i].GetOrbitalTierIndex() > GetOrbitalTierIndex())
				{
					m_owner.orbitals[i].SetOrbitalTierIndex(m_owner.orbitals[i].GetOrbitalTierIndex() - 1);
				}
			}
			m_owner.orbitals.Remove(this);
		}
		LootEngine.DoDefaultSynergyPoof(base.sprite.WorldCenter);
	}

	public void Reinitialize()
	{
		if ((bool)base.specRigidbody)
		{
			base.specRigidbody.Reinitialize();
		}
		m_ownerCenterAverage = m_owner.CenterPosition;
	}

	public Transform GetTransform()
	{
		return m_parentTransform;
	}

	public void ToggleRenderer(bool visible)
	{
		base.sprite.renderer.enabled = visible;
	}

	public int GetOrbitalTier()
	{
		return m_orbitalTier;
	}

	public void SetOrbitalTier(int tier)
	{
		m_orbitalTier = tier;
	}

	public int GetOrbitalTierIndex()
	{
		return m_orbitalTierIndex;
	}

	public void SetOrbitalTierIndex(int tierIndex)
	{
		m_orbitalTierIndex = tierIndex;
	}

	public float GetOrbitalRadius()
	{
		return 2.5f;
	}

	public float GetOrbitalRotationalSpeed()
	{
		return 120f;
	}
}
