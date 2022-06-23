using System;
using System.Collections;
using Dungeonator;
using UnityEngine;

public class ThrownGunPassiveItem : PassiveItem
{
	public bool MakeThrownGunsExplode;

	[ShowInInspectorIf("MakeThrownGunsExplode", false)]
	public ExplosionData ThrownGunExplosionData;

	public bool MakeThrownGunsReturnLikeBoomerangs;

	[Header("Momentum")]
	public bool HasFlagContingentMomentum;

	[LongEnum]
	public GungeonFlags RequiredFlag;

	public GameObject OverheadVFX;

	public float TimeInMotion = 5f;

	public int AdditionalRollDamage = 100;

	public float MomentumKnockback = 100f;

	public GameObject MomentumVFX;

	private GameObject m_instanceVFX;

	private int m_destroyVFXSemaphore;

	private StatModifier m_damageStat;

	private bool m_cachedFlag;

	private float m_motionTimer;

	private bool m_hasUsedMomentum;

	private void Awake()
	{
		m_damageStat = new StatModifier();
		m_damageStat.statToBoost = PlayerStats.StatType.DodgeRollDamage;
		m_damageStat.modifyType = StatModifier.ModifyMethod.ADDITIVE;
		m_damageStat.amount = AdditionalRollDamage;
	}

	public void EnableVFX(PlayerController target)
	{
		if (m_destroyVFXSemaphore == 0)
		{
			Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(target.sprite);
			if (outlineMaterial != null)
			{
				outlineMaterial.SetColor("_OverrideColor", new Color(99f, 0f, 99f));
			}
			if ((bool)OverheadVFX && !m_instanceVFX)
			{
				m_instanceVFX = target.PlayEffectOnActor(OverheadVFX, new Vector3(0f, 1.375f, 0f), true, true);
			}
		}
	}

	public void DisableVFX(PlayerController target)
	{
		if (m_destroyVFXSemaphore == 0)
		{
			Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(target.sprite);
			if (outlineMaterial != null)
			{
				outlineMaterial.SetColor("_OverrideColor", new Color(0f, 0f, 0f));
			}
			if ((bool)m_instanceVFX)
			{
				SpawnManager.Despawn(m_instanceVFX);
				m_instanceVFX = null;
			}
		}
	}

	protected override void Update()
	{
		if (!GameManager.HasInstance || GameManager.Instance.IsLoadingLevel || Dungeon.IsGenerating || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
		{
			m_motionTimer = 0f;
			m_hasUsedMomentum = true;
			return;
		}
		if (m_pickedUp && (bool)base.Owner && HasFlagContingentMomentum && m_cachedFlag)
		{
			base.Owner.ReceivesTouchDamage = false;
			if (m_destroyVFXSemaphore <= 0)
			{
				if (base.Owner.Velocity.magnitude > 0.05f)
				{
					m_motionTimer += BraveTime.DeltaTime;
					if (m_motionTimer > TimeInMotion)
					{
						ForceTrigger(base.Owner);
						m_motionTimer = 0f;
					}
				}
				else
				{
					m_hasUsedMomentum = true;
					m_motionTimer = 0f;
				}
			}
			else
			{
				if (base.Owner.Velocity.magnitude < 0.05f)
				{
					m_hasUsedMomentum = true;
				}
				m_motionTimer = 0f;
			}
		}
		else
		{
			m_motionTimer = 0f;
			m_hasUsedMomentum = true;
		}
		base.Update();
	}

	public void ForceTrigger(PlayerController target)
	{
		target.StartCoroutine(HandleDamageBoost(target));
	}

	private IEnumerator HandleDamageBoost(PlayerController target)
	{
		EnableVFX(target);
		if (m_destroyVFXSemaphore < 0)
		{
			m_destroyVFXSemaphore = 0;
		}
		m_destroyVFXSemaphore++;
		if (m_destroyVFXSemaphore == 1)
		{
			AkSoundEngine.PostEvent("Play_ITM_Macho_Brace_Active_01", base.gameObject);
		}
		m_hasUsedMomentum = false;
		while (target.IsDodgeRolling)
		{
			yield return null;
		}
		float elapsed = 0f;
		target.ownerlessStatModifiers.Add(m_damageStat);
		target.stats.RecalculateStats(target);
		while (!m_hasUsedMomentum)
		{
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		target.ownerlessStatModifiers.Remove(m_damageStat);
		if (m_destroyVFXSemaphore == 1)
		{
			AkSoundEngine.PostEvent("Play_ITM_Macho_Brace_Fade_01", base.gameObject);
		}
		target.stats.RecalculateStats(target);
		m_destroyVFXSemaphore--;
		if (m_hasUsedMomentum)
		{
			m_destroyVFXSemaphore = 0;
		}
		DisableVFX(target);
	}

	public override void Pickup(PlayerController player)
	{
		if (!m_pickedUp)
		{
			base.Pickup(player);
			if (HasFlagContingentMomentum)
			{
				m_cachedFlag = GameStatsManager.Instance.GetFlag(RequiredFlag);
			}
			player.PostProcessThrownGun += PostProcessThrownGun;
			player.OnRollStarted += HandleRollStarted;
			player.OnRolledIntoEnemy += HandleRolledIntoEnemy;
			player.OnReceivedDamage += HandleReceivedDamage;
		}
	}

	public void UpdateCachedFlag()
	{
		if (HasFlagContingentMomentum)
		{
			m_cachedFlag = GameStatsManager.Instance.GetFlag(RequiredFlag);
		}
	}

	private void HandleRolledIntoEnemy(PlayerController arg1, AIActor arg2)
	{
		if (!m_hasUsedMomentum)
		{
			if ((bool)arg2.knockbackDoer)
			{
				arg2.knockbackDoer.ApplyKnockback(arg1.specRigidbody.Velocity.normalized, MomentumKnockback);
			}
			if ((bool)MomentumVFX)
			{
				GameObject gameObject = arg2.PlayEffectOnActor(MomentumVFX, Vector3.zero, false, true);
				tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
				if ((bool)component)
				{
					component.HeightOffGround = 3.5f;
				}
			}
		}
		m_hasUsedMomentum = true;
	}

	private void HandleReceivedDamage(PlayerController obj)
	{
		m_hasUsedMomentum = true;
		m_motionTimer = 0f;
		Material outlineMaterial = SpriteOutlineManager.GetOutlineMaterial(obj.sprite);
		obj.healthHaver.UpdateCachedOutlineColor(outlineMaterial, Color.black);
	}

	private void HandleRollStarted(PlayerController arg1, Vector2 arg2)
	{
		m_motionTimer = 0f;
	}

	private void PostProcessThrownGun(Projectile thrownGunProjectile)
	{
		if (MakeThrownGunsExplode)
		{
			ExplosiveModifier explosiveModifier = thrownGunProjectile.gameObject.AddComponent<ExplosiveModifier>();
			explosiveModifier.doExplosion = true;
			explosiveModifier.explosionData = ThrownGunExplosionData;
			explosiveModifier.explosionData.damageToPlayer = 0f;
			if (ThrownGunExplosionData.useDefaultExplosion)
			{
				explosiveModifier.explosionData = new ExplosionData();
				explosiveModifier.explosionData.CopyFrom(GameManager.Instance.Dungeon.sharedSettingsPrefab.DefaultExplosionData);
				explosiveModifier.explosionData.damageToPlayer = 0f;
			}
		}
		if (MakeThrownGunsReturnLikeBoomerangs)
		{
			thrownGunProjectile.OnBecameDebrisGrounded = (Action<DebrisObject>)Delegate.Combine(thrownGunProjectile.OnBecameDebrisGrounded, new Action<DebrisObject>(HandleReturnLikeBoomerang));
		}
	}

	private void HandleReturnLikeBoomerang(DebrisObject obj)
	{
		obj.OnGrounded = (Action<DebrisObject>)Delegate.Remove(obj.OnGrounded, new Action<DebrisObject>(HandleReturnLikeBoomerang));
		PickupMover pickupMover = obj.gameObject.AddComponent<PickupMover>();
		if ((bool)pickupMover.specRigidbody)
		{
			pickupMover.specRigidbody.CollideWithTileMap = false;
		}
		pickupMover.minRadius = 1f;
		pickupMover.moveIfRoomUnclear = true;
		pickupMover.stopPathingOnContact = true;
	}

	public override DebrisObject Drop(PlayerController player)
	{
		DebrisObject debrisObject = base.Drop(player);
		debrisObject.GetComponent<ThrownGunPassiveItem>().m_pickedUpThisRun = true;
		if ((bool)player)
		{
			player.ReceivesTouchDamage = true;
			player.PostProcessThrownGun -= PostProcessThrownGun;
			player.OnRollStarted -= HandleRollStarted;
			player.OnReceivedDamage -= HandleReceivedDamage;
			player.OnRolledIntoEnemy -= HandleRolledIntoEnemy;
		}
		return debrisObject;
	}

	protected override void OnDestroy()
	{
		BraveTime.ClearMultiplier(base.gameObject);
		if (m_pickedUp && (bool)base.Owner)
		{
			base.Owner.ReceivesTouchDamage = true;
			base.Owner.PostProcessThrownGun -= PostProcessThrownGun;
			base.Owner.OnRollStarted -= HandleRollStarted;
			base.Owner.OnReceivedDamage -= HandleReceivedDamage;
			base.Owner.OnRolledIntoEnemy -= HandleRolledIntoEnemy;
		}
		base.OnDestroy();
	}
}
