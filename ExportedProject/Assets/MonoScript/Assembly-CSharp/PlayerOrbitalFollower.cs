using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class PlayerOrbitalFollower : BraveBehaviour
{
	public Projectile shootProjectile;

	public float shootCooldown = 1f;

	public bool shouldRotate;

	public float rotationOffset;

	public float maxRotationDegreesPerSecond = 360f;

	public bool BlanksOnProjectileRadius;

	public float BlankRadius = 4f;

	public float BlankFrequency = 3f;

	[CheckAnimation(null)]
	public string BlankAnimationName;

	[CheckAnimation(null)]
	public string BlankIdleName;

	public string IdleAnimation;

	[Header("Synergies")]
	public PlayerOrbitalSynergyData[] synergies;

	[NonSerialized]
	public bool OverridePosition;

	[NonSerialized]
	public Vector3 OverrideTargetPosition = Vector3.zero;

	public bool PredictsChests;

	private bool m_initialized;

	private PlayerController m_owner;

	private AIActor m_currentTarget;

	private float m_shootTimer;

	private float m_retargetTimer;

	private int m_orbitalIndex;

	private float m_targetAngle;

	private float m_blankCooldown;

	private bool m_hasLuteBuff;

	private GameObject m_luteOverheadVfx;

	private GameObject BlankVFXPrefab;

	private Chest m_lastPredictedChest;

	private Vector2 m_lastTargetMotionVector;

	private Vector2 m_lastOwnerCenter;

	private const float DIST_BETWEEN_AT_REST = 1.25f;

	public void Initialize(PlayerController owner)
	{
		m_initialized = true;
		m_owner = owner;
		m_orbitalIndex = owner.trailOrbitals.Count;
		owner.trailOrbitals.Add(this);
		base.sprite = GetComponentInChildren<tk2dSprite>();
		base.spriteAnimator = GetComponentInChildren<tk2dSpriteAnimator>();
		SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
	}

	private void DoMicroBlank()
	{
		if (BlankVFXPrefab == null)
		{
			BlankVFXPrefab = (GameObject)BraveResources.Load("Global VFX/BlankVFX_Ghost");
		}
		AkSoundEngine.PostEvent("Play_OBJ_silenceblank_small_01", base.gameObject);
		GameObject gameObject = new GameObject("silencer");
		SilencerInstance silencerInstance = gameObject.AddComponent<SilencerInstance>();
		float additionalTimeAtMaxRadius = 0.25f;
		if ((bool)base.sprite && (bool)base.sprite.spriteAnimator && base.sprite.spriteAnimator.GetClipByName(BlankAnimationName) != null)
		{
			base.sprite.spriteAnimator.PlayForDuration(BlankAnimationName, -1f, BlankIdleName);
		}
		silencerInstance.TriggerSilencer(base.specRigidbody.UnitCenter, 20f, BlankRadius, BlankVFXPrefab, 0f, 3f, 3f, 3f, 30f, 3f, additionalTimeAtMaxRadius, m_owner, false);
	}

	public void ToggleRenderer(bool value)
	{
		base.sprite.renderer.enabled = value;
		SpriteOutlineManager.ToggleOutlineRenderers(base.sprite, value);
	}

	private void Update()
	{
		if (!m_initialized)
		{
			return;
		}
		HandleMotion();
		HandleCombat();
		bool flag = false;
		bool flag2 = false;
		if (synergies != null && (bool)m_owner && (bool)base.spriteAnimator)
		{
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
		}
		if (flag && !flag2 && !string.IsNullOrEmpty(IdleAnimation) && (bool)base.spriteAnimator && !base.spriteAnimator.IsPlaying(IdleAnimation))
		{
			base.spriteAnimator.Play(IdleAnimation);
		}
		if (BlanksOnProjectileRadius)
		{
			m_blankCooldown -= BraveTime.DeltaTime;
			if (m_blankCooldown <= 0f)
			{
				HandleBlanks();
				m_blankCooldown = BlankFrequency;
			}
		}
		if (shouldRotate)
		{
			float target = m_targetAngle + rotationOffset;
			float z = base.transform.rotation.eulerAngles.z;
			target = Mathf.MoveTowardsAngle(z, target, maxRotationDegreesPerSecond * BraveTime.DeltaTime);
			if (float.IsNaN(target) || float.IsInfinity(target))
			{
				target = 0f;
			}
			base.transform.rotation = Quaternion.Euler(0f, 0f, target);
		}
		m_retargetTimer -= BraveTime.DeltaTime;
		m_shootTimer -= BraveTime.DeltaTime;
		if (PredictsChests)
		{
			Chest chest = null;
			float num = float.MaxValue;
			for (int j = 0; j < StaticReferenceManager.AllChests.Count; j++)
			{
				Chest chest2 = StaticReferenceManager.AllChests[j];
				if ((bool)chest2 && (bool)chest2.sprite && !chest2.IsOpen && !chest2.IsBroken && !chest2.IsSealed)
				{
					float num2 = Vector2.Distance(m_owner.CenterPosition, chest2.sprite.WorldCenter);
					if (num2 < num)
					{
						chest = chest2;
						num = num2;
					}
				}
			}
			if (num > 5f)
			{
				chest = null;
			}
			if (m_lastPredictedChest != chest)
			{
				if ((bool)m_lastPredictedChest)
				{
					GetComponent<HologramDoer>().HideSprite(base.gameObject);
				}
				if ((bool)chest)
				{
					List<PickupObject> list = chest.PredictContents(m_owner);
					if (list.Count > 0 && (bool)list[0].encounterTrackable)
					{
						tk2dSpriteCollectionData encounterIconCollection = AmmonomiconController.ForceInstance.EncounterIconCollection;
						GetComponent<HologramDoer>().ChangeToSprite(base.gameObject, encounterIconCollection, encounterIconCollection.GetSpriteIdByName(list[0].encounterTrackable.journalData.AmmonomiconSprite));
					}
				}
				m_lastPredictedChest = chest;
			}
		}
		if (!shootProjectile || !base.specRigidbody)
		{
			return;
		}
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

	private void UpdateVFXOnMovement(SpeculativeRigidbody arg1, Vector2 arg2, IntVector2 arg3)
	{
		if (m_hasLuteBuff && (bool)m_luteOverheadVfx)
		{
			m_luteOverheadVfx.transform.position = base.specRigidbody.UnitCenter.ToVector3ZisY().Quantize(0.0625f) + new Vector3(0f, 1f, 0f);
		}
	}

	protected override void OnDestroy()
	{
		for (int i = 0; i < m_owner.trailOrbitals.Count; i++)
		{
			if (m_owner.trailOrbitals[i].m_orbitalIndex > m_orbitalIndex)
			{
				m_owner.trailOrbitals[i].m_orbitalIndex--;
			}
		}
		m_owner.trailOrbitals.Remove(this);
	}

	private void HandleBlanks()
	{
		Vector2 unitCenter = base.specRigidbody.UnitCenter;
		float num = BlankRadius * BlankRadius;
		for (int i = 0; i < StaticReferenceManager.AllProjectiles.Count; i++)
		{
			Projectile projectile = StaticReferenceManager.AllProjectiles[i];
			if ((bool)projectile && projectile.collidesWithPlayer && (bool)projectile.specRigidbody && (!m_owner || !(projectile.Owner is PlayerController)) && (unitCenter - projectile.specRigidbody.UnitCenter).sqrMagnitude < num)
			{
				DoMicroBlank();
				break;
			}
		}
	}

	private void HandleMotion()
	{
		Vector2 vector = m_owner.CenterPosition;
		if (m_orbitalIndex > 0)
		{
			vector = m_owner.trailOrbitals[m_orbitalIndex - 1].specRigidbody.UnitCenter;
		}
		Vector2 vector2 = vector - m_lastOwnerCenter;
		if (vector2.sqrMagnitude > 0f)
		{
			m_lastTargetMotionVector = vector2.normalized;
		}
		Vector2 vector3 = vector + -1f * m_owner.trailOrbitals[0].m_lastTargetMotionVector * 1.25f;
		vector3 = vector3.Quantize(0.0625f);
		if (OverridePosition)
		{
			vector3 = OverrideTargetPosition.XY();
		}
		float magnitude = (vector3 - base.transform.position.XY()).magnitude;
		float a = Mathf.Lerp(0.1f, 15f, magnitude / 4f) * BraveTime.DeltaTime;
		if (OverridePosition)
		{
			a = 15f * BraveTime.DeltaTime;
		}
		float num = Mathf.Min(a, magnitude);
		Vector2 velocity = (vector3 - base.transform.position.XY()).normalized * num / BraveTime.DeltaTime;
		base.specRigidbody.Velocity = velocity;
		if (shouldRotate)
		{
			m_targetAngle = BraveMathCollege.Atan2Degrees((vector - base.transform.position.XY()).normalized);
		}
		m_lastOwnerCenter = vector;
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

	private Vector2 FindPredictedTargetPosition()
	{
		float num = shootProjectile.baseData.speed;
		if (num < 0f)
		{
			num = float.MaxValue;
		}
		Vector2 a = base.transform.position.XY();
		Vector2 unitCenter = m_currentTarget.specRigidbody.HitboxPixelCollider.UnitCenter;
		float num2 = Vector2.Distance(a, unitCenter) / num;
		return unitCenter + m_currentTarget.specRigidbody.Velocity * num2;
	}

	private void Shoot(Vector2 targetPosition)
	{
		Vector2 vector = targetPosition - base.transform.position.XY();
		float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		GameObject prefab = shootProjectile.gameObject;
		GameObject gameObject = SpawnManager.SpawnProjectile(prefab, base.transform.position.XY(), Quaternion.Euler(0f, 0f, z));
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
		if (GameManager.Instance.IsPaused || !m_owner || m_owner.CurrentInputState != 0 || m_owner.IsInputOverridden)
		{
			return;
		}
		bool flag = false;
		for (int i = 0; i < synergies.Length; i++)
		{
			if ((bool)m_owner && m_owner.HasActiveBonusSynergy(synergies[i].SynergyToCheck) && synergies[i].EngagesFiring && (bool)synergies[i].OverrideProjectile)
			{
				flag = true;
				break;
			}
		}
		if (shootProjectile == null || flag)
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
		if (m_currentTarget == null)
		{
			return;
		}
		if (shouldRotate)
		{
			m_targetAngle = BraveMathCollege.Atan2Degrees(m_currentTarget.CenterPosition - base.transform.position.XY());
		}
		if (m_shootTimer <= 0f)
		{
			m_shootTimer = GetModifiedCooldown();
			Vector2 targetPosition = FindPredictedTargetPosition();
			if (!m_owner.IsStealthed)
			{
				Shoot(targetPosition);
			}
		}
	}
}
