using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class PlayerOrbital : BraveBehaviour, IPlayerOrbital
{
	public enum SpecialOrbitalIdentifier
	{
		NONE,
		BABY_DRAGUN
	}

	public enum OrbitalMotionStyle
	{
		ORBIT_PLAYER_ALWAYS,
		ORBIT_TARGET
	}

	public SpecialOrbitalIdentifier SpecialID;

	public OrbitalMotionStyle motionStyle;

	public Projectile shootProjectile;

	public int numToShoot = 1;

	public float shootCooldown = 1f;

	public float orbitRadius = 3f;

	public float orbitDegreesPerSecond = 90f;

	public bool shouldRotate = true;

	public float perfectOrbitalFactor;

	public bool DamagesEnemiesOnShot;

	public float DamageToEnemiesOnShot = 10f;

	public float DamageToEnemiesOnShotCooldown = 3f;

	private float m_damageOnShotCooldown;

	public bool TriggersMachoBraceOnShot;

	public bool PreventOutline;

	public string IdleAnimation;

	[Header("Synergies")]
	public PlayerOrbitalSynergyData[] synergies;

	public bool ExplodesOnTriggerCollision;

	public ExplosionData TriggerExplosionData;

	private bool m_initialized;

	private PlayerController m_owner;

	private AIActor m_currentTarget;

	private float m_currentAngle;

	private float m_shootTimer;

	private float m_retargetTimer;

	private int m_orbitalTier;

	private int m_orbitalTierIndex;

	private Vector2 m_ownerCenterAverage;

	private bool m_hasLuteBuff;

	private GameObject m_luteOverheadVfx;

	[NonSerialized]
	public PlayerOrbitalItem SourceItem;

	private float m_lastExplosionTime;

	public float SinWavelength = 3f;

	public float SinAmplitude = 1f;

	public PlayerController Owner
	{
		get
		{
			return m_owner;
		}
	}

	public static int GetNumberOfOrbitalsInTier(PlayerController owner, int tier)
	{
		int num = 0;
		for (int i = 0; i < owner.orbitals.Count; i++)
		{
			if (owner.orbitals[i].GetOrbitalTier() == tier)
			{
				num++;
			}
		}
		return num;
	}

	public static int CalculateTargetTier(PlayerController owner, IPlayerOrbital orbital)
	{
		float orbitalRadius = orbital.GetOrbitalRadius();
		float orbitalRotationalSpeed = orbital.GetOrbitalRotationalSpeed();
		int num = -1;
		for (int i = 0; i < owner.orbitals.Count; i++)
		{
			if (owner.orbitals[i] != orbital)
			{
				num = Mathf.Max(num, owner.orbitals[i].GetOrbitalTier());
				float orbitalRadius2 = owner.orbitals[i].GetOrbitalRadius();
				float orbitalRotationalSpeed2 = owner.orbitals[i].GetOrbitalRotationalSpeed();
				if (Mathf.Approximately(orbitalRadius2, orbitalRadius) && Mathf.Approximately(orbitalRotationalSpeed2, orbitalRotationalSpeed))
				{
					return owner.orbitals[i].GetOrbitalTier();
				}
			}
		}
		return num + 1;
	}

	public void Initialize(PlayerController owner)
	{
		m_initialized = true;
		m_owner = owner;
		SetOrbitalTier(CalculateTargetTier(owner, this));
		SetOrbitalTierIndex(GetNumberOfOrbitalsInTier(owner, m_orbitalTier));
		Debug.LogError("new orbital tier: " + GetOrbitalTier() + " and index: " + GetOrbitalTierIndex());
		owner.orbitals.Add(this);
		base.sprite = GetComponentInChildren<tk2dSprite>();
		base.spriteAnimator = GetComponentInChildren<tk2dSpriteAnimator>();
		if (!PreventOutline)
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
		}
		m_ownerCenterAverage = m_owner.CenterPosition;
		if ((bool)base.specRigidbody && (DamagesEnemiesOnShot || TriggersMachoBraceOnShot))
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		}
		if ((bool)base.specRigidbody && ExplodesOnTriggerCollision)
		{
			SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
			speculativeRigidbody2.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody2.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(HandleTriggerCollisionExplosion));
		}
	}

	private void HandleTriggerCollisionExplosion(SpeculativeRigidbody otherRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if ((bool)otherRigidbody && (bool)otherRigidbody.aiActor && Time.time - m_lastExplosionTime > 5f)
		{
			m_lastExplosionTime = Time.time;
			Exploder.Explode(base.specRigidbody.UnitCenter, TriggerExplosionData, Vector2.zero);
			Disappear();
		}
	}

	private void Disappear()
	{
		base.specRigidbody.enabled = false;
		SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, false);
		base.sprite.renderer.enabled = false;
	}

	private void Reappear()
	{
		base.specRigidbody.enabled = true;
		base.sprite.renderer.enabled = true;
		SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, true);
		base.specRigidbody.Reinitialize();
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
	}

	public void DecoupleBabyDragun()
	{
		if ((bool)SourceItem)
		{
			SourceItem.DecoupleOrbital();
			m_owner.RemovePassiveItem(SourceItem.PickupObjectId);
		}
		m_owner.orbitals.Remove(this);
		if ((bool)base.specRigidbody)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		}
		UnityEngine.Object.Destroy(this);
	}

	private void HandleRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if (!rigidbodyCollision.OtherRigidbody.projectile)
		{
			return;
		}
		if (DamagesEnemiesOnShot && m_damageOnShotCooldown <= 0f)
		{
			if ((bool)m_owner)
			{
				StartCoroutine(FlashSprite(base.sprite));
				m_owner.CurrentRoom.ApplyActionToNearbyEnemies(m_owner.CenterPosition, 100f, delegate(AIActor enemy, float dist)
				{
					if ((bool)enemy && (bool)enemy.healthHaver)
					{
						enemy.healthHaver.ApplyDamage(DamageToEnemiesOnShot, Vector2.zero, string.Empty);
					}
				});
			}
			m_damageOnShotCooldown = DamageToEnemiesOnShotCooldown;
		}
		if (!TriggersMachoBraceOnShot || !m_owner)
		{
			return;
		}
		for (int i = 0; i < m_owner.passiveItems.Count; i++)
		{
			if (m_owner.passiveItems[i] is MachoBraceItem)
			{
				(m_owner.passiveItems[i] as MachoBraceItem).ForceTrigger(m_owner);
				break;
			}
		}
	}

	private IEnumerator FlashSprite(tk2dBaseSprite targetSprite, float flashTime = 1f)
	{
		Color overrideColor = Color.white;
		overrideColor.a = 1f;
		if ((bool)targetSprite)
		{
			targetSprite.usesOverrideMaterial = true;
		}
		Color startColor = targetSprite.renderer.material.GetColor("_OverrideColor");
		Material targetMaterial = targetSprite.renderer.material;
		for (float elapsed = 0f; elapsed < flashTime; elapsed += BraveTime.DeltaTime)
		{
			float t = 1f - elapsed / flashTime;
			targetMaterial.SetColor("_OverrideColor", Color.Lerp(startColor, overrideColor, t));
			targetMaterial.SetFloat("_SaturationModifier", Mathf.Lerp(1f, 5f, t));
			yield return null;
		}
		targetSprite.renderer.material.SetColor("_OverrideColor", startColor);
		targetMaterial.SetFloat("_SaturationModifier", 1f);
	}

	private void Update()
	{
		if (!m_initialized)
		{
			return;
		}
		if (ExplodesOnTriggerCollision && !base.specRigidbody.enabled && Time.time - m_lastExplosionTime > 5f)
		{
			Reappear();
		}
		HandleMotion();
		HandleCombat();
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < synergies.Length; i++)
		{
			PlayerOrbitalSynergyData playerOrbitalSynergyData = synergies[i];
			flag |= playerOrbitalSynergyData.HasOverrideAnimations;
			if (playerOrbitalSynergyData.HasOverrideAnimations && m_owner.HasActiveBonusSynergy(playerOrbitalSynergyData.SynergyToCheck))
			{
				flag2 = true;
				if (!base.spriteAnimator.IsPlaying(playerOrbitalSynergyData.OverrideIdleAnimation))
				{
					base.spriteAnimator.Play(playerOrbitalSynergyData.OverrideIdleAnimation);
				}
			}
		}
		if (flag && !flag2 && !string.IsNullOrEmpty(IdleAnimation) && !base.spriteAnimator.IsPlaying(IdleAnimation))
		{
			base.spriteAnimator.Play(IdleAnimation);
		}
		if (motionStyle != OrbitalMotionStyle.ORBIT_TARGET)
		{
			m_retargetTimer -= BraveTime.DeltaTime;
		}
		if ((bool)shootProjectile && (bool)base.specRigidbody)
		{
			if (m_hasLuteBuff && (!m_owner || !m_owner.CurrentGun || !m_owner.CurrentGun.LuteCompanionBuffActive))
			{
				if ((bool)m_luteOverheadVfx)
				{
					UnityEngine.Object.Destroy(m_luteOverheadVfx);
					m_luteOverheadVfx = null;
				}
				if ((bool)base.specRigidbody)
				{
					SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
					speculativeRigidbody.OnPostRigidbodyMovement = (Action<SpeculativeRigidbody, Vector2, IntVector2>)Delegate.Remove(speculativeRigidbody.OnPostRigidbodyMovement, new Action<SpeculativeRigidbody, Vector2, IntVector2>(UpdateVFXOnMovement));
				}
				m_hasLuteBuff = false;
			}
			else if (!m_hasLuteBuff && (bool)m_owner && (bool)m_owner.CurrentGun && m_owner.CurrentGun.LuteCompanionBuffActive)
			{
				GameObject prefab = (GameObject)ResourceCache.Acquire("Global VFX/VFX_Buff_Status");
				m_luteOverheadVfx = SpawnManager.SpawnVFX(prefab, base.specRigidbody.UnitCenter.ToVector3ZisY().Quantize(0.0625f) + new Vector3(0f, 1f, 0f), Quaternion.identity);
				if ((bool)base.specRigidbody)
				{
					SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
					speculativeRigidbody2.OnPostRigidbodyMovement = (Action<SpeculativeRigidbody, Vector2, IntVector2>)Delegate.Combine(speculativeRigidbody2.OnPostRigidbodyMovement, new Action<SpeculativeRigidbody, Vector2, IntVector2>(UpdateVFXOnMovement));
				}
				m_hasLuteBuff = true;
			}
		}
		m_damageOnShotCooldown -= BraveTime.DeltaTime;
		m_shootTimer -= BraveTime.DeltaTime;
	}

	private void UpdateVFXOnMovement(SpeculativeRigidbody arg1, Vector2 arg2, IntVector2 arg3)
	{
		if (m_hasLuteBuff && (bool)m_luteOverheadVfx)
		{
			m_luteOverheadVfx.transform.position = base.specRigidbody.UnitCenter.ToVector3ZisY().Quantize(0.0625f) + new Vector3(0f, 1f, 0f);
		}
	}

	protected override void OnDestroy()
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

	public void Reinitialize()
	{
		base.specRigidbody.Reinitialize();
		m_ownerCenterAverage = m_owner.CenterPosition;
	}

	public void ReinitializeWithDelta(Vector2 delta)
	{
		base.specRigidbody.Reinitialize();
		m_ownerCenterAverage += delta;
	}

	private void HandleMotion()
	{
		Vector2 centerPosition = m_owner.CenterPosition;
		if (Vector2.Distance(centerPosition, base.transform.position.XY()) > 20f)
		{
			base.transform.position = centerPosition.ToVector3ZUp();
			base.specRigidbody.Reinitialize();
		}
		if (motionStyle == OrbitalMotionStyle.ORBIT_TARGET && m_currentTarget != null)
		{
			centerPosition = m_currentTarget.CenterPosition;
		}
		Vector2 vector = centerPosition - m_ownerCenterAverage;
		float num = Mathf.Lerp(0.1f, 15f, vector.magnitude / 4f);
		float num2 = Mathf.Min(num * BraveTime.DeltaTime, vector.magnitude);
		float num3 = 360f / (float)GetNumberOfOrbitalsInTier(m_owner, GetOrbitalTier()) * (float)GetOrbitalTierIndex() + BraveTime.ScaledTimeSinceStartup * orbitDegreesPerSecond;
		Vector2 a = m_ownerCenterAverage + (centerPosition - m_ownerCenterAverage).normalized * num2;
		a = Vector2.Lerp(a, centerPosition, perfectOrbitalFactor);
		Vector2 vector2 = a + (Quaternion.Euler(0f, 0f, num3) * Vector3.right * orbitRadius).XY();
		if (SpecialID == SpecialOrbitalIdentifier.BABY_DRAGUN)
		{
			float num4 = Mathf.Sin(Time.time * SinWavelength) * SinAmplitude;
			vector2 += (Quaternion.Euler(0f, 0f, num3) * Vector3.right).XY().normalized * num4;
		}
		m_ownerCenterAverage = a;
		vector2 = vector2.Quantize(0.0625f);
		Vector2 velocity = (vector2 - base.transform.position.XY()) / BraveTime.DeltaTime;
		base.specRigidbody.Velocity = velocity;
		m_currentAngle = num3 % 360f;
		if (shouldRotate)
		{
			base.transform.localRotation = Quaternion.Euler(0f, 0f, m_currentAngle);
		}
	}

	private void AcquireTarget()
	{
		m_retargetTimer = 0.25f;
		m_currentTarget = null;
		if (m_owner == null || m_owner.CurrentRoom == null)
		{
			return;
		}
		List<AIActor> activeEnemies = m_owner.CurrentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (activeEnemies == null || activeEnemies.Count <= 0)
		{
			return;
		}
		AIActor aIActor = null;
		float num = -1f;
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			AIActor aIActor2 = activeEnemies[i];
			if ((bool)aIActor2 && aIActor2.HasBeenEngaged && aIActor2.IsWorthShootingAt)
			{
				float num2 = Vector2.Distance(base.transform.position.XY(), aIActor2.specRigidbody.UnitCenter);
				if (aIActor == null || num2 < num)
				{
					aIActor = aIActor2;
					num = num2;
				}
			}
		}
		if ((bool)aIActor)
		{
			m_currentTarget = aIActor;
		}
	}

	private Projectile GetProjectile()
	{
		Projectile overrideProjectile = shootProjectile;
		for (int i = 0; i < synergies.Length; i++)
		{
			PlayerOrbitalSynergyData playerOrbitalSynergyData = synergies[i];
			if ((bool)playerOrbitalSynergyData.OverrideProjectile && m_owner.HasActiveBonusSynergy(playerOrbitalSynergyData.SynergyToCheck))
			{
				overrideProjectile = playerOrbitalSynergyData.OverrideProjectile;
			}
		}
		return overrideProjectile;
	}

	private Vector2 FindPredictedTargetPosition()
	{
		float num = GetProjectile().baseData.speed;
		if (num < 0f)
		{
			num = float.MaxValue;
		}
		Vector2 a = base.transform.position.XY();
		Vector2 vector = ((m_currentTarget.specRigidbody.HitboxPixelCollider == null) ? m_currentTarget.specRigidbody.UnitCenter : m_currentTarget.specRigidbody.HitboxPixelCollider.UnitCenter);
		float num2 = Vector2.Distance(a, vector) / num;
		return vector + m_currentTarget.specRigidbody.Velocity * num2;
	}

	private void Shoot(Vector2 targetPosition, Vector2 startOffset)
	{
		Vector2 vector = base.transform.position.XY() + startOffset;
		Vector2 vector2 = targetPosition - vector;
		float z = Mathf.Atan2(vector2.y, vector2.x) * 57.29578f;
		GameObject prefab = GetProjectile().gameObject;
		GameObject gameObject = SpawnManager.SpawnProjectile(prefab, vector, Quaternion.Euler(0f, 0f, z));
		Projectile component = gameObject.GetComponent<Projectile>();
		component.collidesWithEnemies = true;
		component.collidesWithPlayer = false;
		component.Owner = m_owner;
		component.Shooter = m_owner.specRigidbody;
		component.TreatedAsNonProjectileForChallenge = true;
		if ((bool)m_owner)
		{
			if (PassiveItem.IsFlagSetForCharacter(m_owner, typeof(BattleStandardItem)))
			{
				component.baseData.damage *= BattleStandardItem.BattleStandardCompanionDamageMultiplier;
			}
			if ((bool)m_owner.CurrentGun && m_owner.CurrentGun.LuteCompanionBuffActive)
			{
				component.baseData.damage *= 2f;
				component.RuntimeUpdateScale(1.75f);
			}
			m_owner.DoPostProcessProjectile(component);
		}
	}

	public void ToggleRenderer(bool value)
	{
		base.sprite.renderer.enabled = value;
		if (!PreventOutline)
		{
			SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, value);
		}
	}

	private int GetNumberToFire()
	{
		int num = numToShoot;
		if (synergies != null && (bool)m_owner)
		{
			for (int i = 0; i < synergies.Length; i++)
			{
				if (m_owner.HasActiveBonusSynergy(synergies[i].SynergyToCheck))
				{
					num += synergies[i].AdditionalShots;
				}
			}
		}
		return num;
	}

	private float GetModifiedCooldown()
	{
		float num = shootCooldown;
		if (synergies != null && (bool)m_owner)
		{
			for (int i = 0; i < synergies.Length; i++)
			{
				if (m_owner.HasActiveBonusSynergy(synergies[i].SynergyToCheck))
				{
					num *= synergies[i].ShootCooldownMultiplier;
				}
			}
		}
		if ((bool)m_owner && (bool)m_owner.CurrentGun && m_owner.CurrentGun.LuteCompanionBuffActive)
		{
			num /= 1.5f;
		}
		return num;
	}

	private void HandleCombat()
	{
		if (GameManager.Instance.IsPaused || !m_owner || m_owner.CurrentInputState != 0 || m_owner.IsInputOverridden || shootProjectile == null)
		{
			return;
		}
		if (m_retargetTimer <= 0f)
		{
			m_currentTarget = null;
		}
		if (m_currentTarget == null || !m_currentTarget || m_currentTarget.healthHaver.IsDead)
		{
			AcquireTarget();
		}
		if (m_currentTarget == null || !m_currentTarget)
		{
			return;
		}
		if (m_shootTimer <= 0f)
		{
			m_shootTimer = GetModifiedCooldown();
			Vector2 vector = FindPredictedTargetPosition();
			if (!m_owner.IsStealthed)
			{
				int numberToFire = GetNumberToFire();
				for (int i = 0; i < numberToFire; i++)
				{
					Vector2 vector2 = Vector2.zero;
					if (i > 0)
					{
						vector2 = UnityEngine.Random.insideUnitCircle.normalized;
					}
					Shoot(vector + vector2, vector2);
				}
			}
		}
		if (shouldRotate)
		{
			float num = BraveMathCollege.Atan2Degrees(m_currentTarget.CenterPosition - base.transform.position.XY());
			base.transform.localRotation = Quaternion.Euler(0f, 0f, num - 90f);
		}
	}

	public Transform GetTransform()
	{
		return base.transform;
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
		return orbitRadius;
	}

	public float GetOrbitalRotationalSpeed()
	{
		return orbitDegreesPerSecond;
	}
}
