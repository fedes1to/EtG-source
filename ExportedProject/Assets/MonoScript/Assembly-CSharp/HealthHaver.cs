using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;
using UnityEngine.Serialization;

public class HealthHaver : BraveBehaviour
{
	public class ModifyDamageEventArgs : EventArgs
	{
		public float InitialDamage;

		public float ModifiedDamage;
	}

	public class ModifyHealingEventArgs : EventArgs
	{
		public float InitialHealing;

		public float ModifiedHealing;
	}

	public enum BossBarType
	{
		None,
		MainBar,
		SecondaryBar,
		CombinedBar,
		SecretBar,
		VerticalBar,
		SubbossBar
	}

	public delegate void OnDamagedEvent(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection);

	public delegate void OnHealthChangedEvent(float resultValue, float maxValue);

	public enum BulletScriptType
	{
		OnPreDeath,
		OnDeath,
		OnAnimEvent
	}

	protected const float c_flashTime = 0.04f;

	protected const float c_flashDowntime = 0.2f;

	protected const float c_incorporealityFlashOnTime = 0.12f;

	protected const float c_incorporealityFlashOffTime = 0.12f;

	protected const float c_bossDpsCapWindow = 3f;

	public Action<HealthHaver, ModifyDamageEventArgs> ModifyDamage;

	public Action<HealthHaver, ModifyHealingEventArgs> ModifyHealing;

	[TogglesProperty("quantizedIncrement", null)]
	public bool quantizeHealth;

	[HideInInspector]
	public float quantizedIncrement = 0.5f;

	public bool flashesOnDamage = true;

	[TogglesProperty("incorporealityTime", "Incorporeality Period")]
	public bool incorporealityOnDamage;

	[HideInInspector]
	public float incorporealityTime = 1f;

	public bool PreventAllDamage;

	[HideInInspector]
	public bool persistsOnDeath;

	[NonSerialized]
	protected float m_curseHealthMaximum = float.MaxValue;

	[NonSerialized]
	public bool HasCrest;

	[NonSerialized]
	public bool HasRatchetHealthBar;

	[SerializeField]
	protected float maximumHealth = 10f;

	[HideInInspector]
	[SerializeField]
	protected float currentHealth = 10f;

	[SerializeField]
	protected float currentArmor;

	[SerializeField]
	[TogglesProperty("invulnerabilityPeriod", "Invulnerability Period")]
	protected bool usesInvulnerabilityPeriod;

	[HideInInspector]
	[SerializeField]
	protected float invulnerabilityPeriod = 0.5f;

	[ShowInInspectorIf("usesInvulnerabilityPeriod", true)]
	public bool useFortunesFavorInvulnerability;

	public GameObject deathEffect;

	public string damagedAudioEvent = string.Empty;

	public string overrideDeathAudioEvent = string.Empty;

	public string overrideDeathAnimation = string.Empty;

	[Space(5f)]
	public bool shakesCameraOnDamage;

	[ShowInInspectorIf("shakesCameraOnDamage", false)]
	public ScreenShakeSettings cameraShakeOnDamage;

	[Header("Damage Overrides")]
	public List<DamageTypeModifier> damageTypeModifiers;

	public bool healthIsNumberOfHits;

	public bool OnlyAllowSpecialBossDamage;

	[Header("BulletScript")]
	[FormerlySerializedAs("spawnsBulletMl")]
	public bool spawnBulletScript;

	[FormerlySerializedAs("chanceToSpawnBulletMl")]
	[ShowInInspectorIf("spawnBulletScript", true)]
	public float chanceToSpawnBulletScript;

	[FormerlySerializedAs("overrideDeathAnimBulletMl")]
	[ShowInInspectorIf("spawnBulletScript", true)]
	public string overrideDeathAnimBulletScript;

	[ShowInInspectorIf("spawnBulletScript", true)]
	[FormerlySerializedAs("noCorpseWhenBulletMlDeath")]
	public bool noCorpseWhenBulletScriptDeath;

	[FormerlySerializedAs("bulletMlType")]
	[ShowInInspectorIf("spawnBulletScript", true)]
	public BulletScriptType bulletScriptType;

	public BulletScriptSelector bulletScript;

	[Header("For Bosses")]
	public BossBarType bossHealthBar;

	public string overrideBossName;

	public bool forcePreventVictoryMusic;

	[NonSerialized]
	public string lastIncurredDamageSource;

	[NonSerialized]
	public Vector2 lastIncurredDamageDirection;

	[NonSerialized]
	public bool NextShotKills;

	protected List<Material> materialsToFlash;

	protected List<Material> outlineMaterialsToFlash;

	protected List<Material> materialsToEnableBrightnessClampOn;

	protected List<Color> sourceColors;

	protected bool isPlayerCharacter;

	private bool m_isFlashing;

	private bool m_isIncorporeal;

	private float m_damageCap = -1f;

	private float m_bossDpsCap = -1f;

	private float m_recentBossDps;

	private PlayerController m_player;

	[NonSerialized]
	public float minimumHealth;

	[NonSerialized]
	public List<tk2dBaseSprite> bodySprites = new List<tk2dBaseSprite>();

	[NonSerialized]
	public List<SpeculativeRigidbody> bodyRigidbodies;

	[NonSerialized]
	public float AllDamageMultiplier = 1f;

	[NonSerialized]
	private Dictionary<PixelCollider, tk2dBaseSprite> m_independentDamageFlashers;

	protected bool vulnerable = true;

	public float GlobalPixelColliderDamageMultiplier = 1f;

	[NonSerialized]
	public bool NextDamageIgnoresArmor;

	private bool isFirstFrame = true;

	private static int m_hitBarkLimiter;

	private Coroutine m_flashOnHitCoroutine;

	public float CursedMaximum
	{
		get
		{
			return m_curseHealthMaximum;
		}
		set
		{
			m_curseHealthMaximum = value;
			currentHealth = Mathf.Min(currentHealth, AdjustedMaxHealth);
			if (this.OnHealthChanged != null)
			{
				this.OnHealthChanged(GetCurrentHealth(), GetMaxHealth());
			}
		}
	}

	protected float AdjustedMaxHealth
	{
		get
		{
			return GetMaxHealth();
		}
		set
		{
			maximumHealth = value;
		}
	}

	public float Armor
	{
		get
		{
			return currentArmor;
		}
		set
		{
			if (!m_player || m_player.ForceZeroHealthState || !IsDead)
			{
				currentArmor = value;
				if (this.OnHealthChanged != null)
				{
					this.OnHealthChanged(currentHealth, AdjustedMaxHealth);
				}
			}
		}
	}

	public bool IsVulnerable
	{
		get
		{
			if (isPlayerCharacter && m_player.rollStats.additionalInvulnerabilityFrames > 0)
			{
				for (int i = 1; i <= m_player.rollStats.additionalInvulnerabilityFrames; i++)
				{
					if (base.spriteAnimator.QueryPreviousInvulnerabilityFrame(i))
					{
						return false;
					}
				}
			}
			return vulnerable && !base.spriteAnimator.QueryInvulnerabilityFrame();
		}
		set
		{
			vulnerable = value;
		}
	}

	public bool IsAlive
	{
		get
		{
			return GetCurrentHealth() > 0f || Armor > 0f;
		}
	}

	public bool IsDead
	{
		get
		{
			return GetCurrentHealth() <= 0f && Armor <= 0f;
		}
	}

	public bool ManualDeathHandling { get; set; }

	public bool DisableStickyFriction { get; set; }

	public bool IsBoss
	{
		get
		{
			return bossHealthBar != BossBarType.None;
		}
	}

	public bool IsSubboss
	{
		get
		{
			return bossHealthBar == BossBarType.SubbossBar;
		}
	}

	public bool UsesSecondaryBossBar
	{
		get
		{
			return bossHealthBar == BossBarType.SecondaryBar;
		}
	}

	public bool UsesVerticalBossBar
	{
		get
		{
			return bossHealthBar == BossBarType.VerticalBar;
		}
	}

	public bool HasHealthBar
	{
		get
		{
			return bossHealthBar != 0 && bossHealthBar != BossBarType.SecretBar && bossHealthBar != BossBarType.SubbossBar;
		}
	}

	public Vector2? OverrideKillCamPos { get; set; }

	public float? OverrideKillCamTime { get; set; }

	public bool TrackDuringDeath { get; set; }

	public bool SuppressContinuousKillCamBulletDestruction { get; set; }

	public bool SuppressDeathSounds { get; set; }

	public bool CanCurrentlyBeKilled
	{
		get
		{
			return IsVulnerable && !PreventAllDamage && minimumHealth <= 0f;
		}
	}

	public bool PreventCooldownGainFromDamage { get; set; }

	public bool TrackPixelColliderDamage { get; private set; }

	public Dictionary<PixelCollider, float> PixelColliderDamage { get; private set; }

	public List<PixelCollider> DamageableColliders { get; set; }

	public int NumBodyRigidbodies
	{
		get
		{
			if (bodyRigidbodies != null)
			{
				return bodyRigidbodies.Count;
			}
			if ((bool)base.specRigidbody)
			{
				return 1;
			}
			return 0;
		}
	}

	public event Action<Vector2> OnPreDeath;

	public event Action<Vector2> OnDeath;

	public event OnDamagedEvent OnDamaged;

	public event OnHealthChangedEvent OnHealthChanged;

	public float GetMaxHealth()
	{
		return Mathf.Min(CursedMaximum, maximumHealth);
	}

	public float GetCurrentHealth()
	{
		return currentHealth;
	}

	public void ForceSetCurrentHealth(float h)
	{
		currentHealth = h;
		currentHealth = Mathf.Min(currentHealth, GetMaxHealth());
		if (this.OnHealthChanged != null)
		{
			this.OnHealthChanged(currentHealth, GetMaxHealth());
		}
	}

	public float GetCurrentHealthPercentage()
	{
		return currentHealth / AdjustedMaxHealth;
	}

	public void AddTrackedDamagePixelCollider(PixelCollider pixelCollider)
	{
		TrackPixelColliderDamage = true;
		if (PixelColliderDamage == null)
		{
			PixelColliderDamage = new Dictionary<PixelCollider, float>();
		}
		PixelColliderDamage.Add(pixelCollider, 0f);
	}

	public void Awake()
	{
		StaticReferenceManager.AllHealthHavers.Add(this);
		if (GameManager.Instance.InTutorial)
		{
			if (base.name.StartsWith("BulletMan"))
			{
				maximumHealth = 10f;
			}
			if (base.name.StartsWith("BulletShotgunMan"))
			{
				maximumHealth = 15f;
			}
		}
		currentHealth = AdjustedMaxHealth;
		RegisterBodySprite(base.sprite);
		if (IsBoss)
		{
			base.aiActor.SetResistance(EffectResistanceType.Freeze, Mathf.Max(base.aiActor.GetResistanceForEffectType(EffectResistanceType.Freeze), 0.6f));
			base.aiActor.SetResistance(EffectResistanceType.Fire, Mathf.Max(base.aiActor.GetResistanceForEffectType(EffectResistanceType.Fire), 0.25f));
			if ((bool)base.knockbackDoer)
			{
				base.knockbackDoer.SetImmobile(true, "Like-a-boss");
			}
		}
	}

	private void Start()
	{
		if (base.spriteAnimator == null)
		{
			base.spriteAnimator = GetComponentInChildren<tk2dSpriteAnimator>();
		}
		m_player = GetComponent<PlayerController>();
		if (m_player != null)
		{
			isPlayerCharacter = true;
		}
		GameLevelDefinition lastLoadedLevelDefinition = GameManager.Instance.GetLastLoadedLevelDefinition();
		if (lastLoadedLevelDefinition == null)
		{
			return;
		}
		m_damageCap = lastLoadedLevelDefinition.damageCap;
		if (IsBoss && !IsSubboss && lastLoadedLevelDefinition.bossDpsCap > 0f)
		{
			float num = 1f;
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				num = (GameManager.Instance.COOP_ENEMY_HEALTH_MULTIPLIER + 2f) / 2f;
			}
			m_bossDpsCap = lastLoadedLevelDefinition.bossDpsCap * num;
		}
	}

	public void Update()
	{
		isFirstFrame = false;
		if (m_bossDpsCap > 0f && m_recentBossDps > 0f)
		{
			m_recentBossDps = Mathf.Max(0f, m_recentBossDps - m_bossDpsCap * BraveTime.DeltaTime);
		}
	}

	protected override void OnDestroy()
	{
		StaticReferenceManager.AllHealthHavers.Remove(this);
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Remove(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(BulletScriptEventTriggered));
		base.OnDestroy();
	}

	public void RegisterBodySprite(tk2dBaseSprite sprite, bool flashIndependentlyOnDamage = false, int flashPixelCollider = 0)
	{
		if (!bodySprites.Contains(sprite))
		{
			bodySprites.Add(sprite);
		}
		if (flashIndependentlyOnDamage)
		{
			if (m_independentDamageFlashers == null)
			{
				m_independentDamageFlashers = new Dictionary<PixelCollider, tk2dBaseSprite>();
			}
			m_independentDamageFlashers.Add(base.specRigidbody.PixelColliders[flashPixelCollider], sprite);
		}
	}

	public void ApplyHealing(float healing)
	{
		if (!isPlayerCharacter || !m_player.IsGhost)
		{
			if (ModifyHealing != null)
			{
				ModifyHealingEventArgs modifyHealingEventArgs = new ModifyHealingEventArgs();
				modifyHealingEventArgs.InitialHealing = healing;
				modifyHealingEventArgs.ModifiedHealing = healing;
				ModifyHealingEventArgs modifyHealingEventArgs2 = modifyHealingEventArgs;
				ModifyHealing(this, modifyHealingEventArgs2);
				healing = modifyHealingEventArgs2.ModifiedHealing;
			}
			currentHealth += healing;
			if (quantizeHealth)
			{
				currentHealth = BraveMathCollege.QuantizeFloat(currentHealth, quantizedIncrement);
			}
			if (currentHealth > AdjustedMaxHealth)
			{
				currentHealth = AdjustedMaxHealth;
			}
			if (this.OnHealthChanged != null)
			{
				this.OnHealthChanged(currentHealth, AdjustedMaxHealth);
			}
		}
	}

	public void FullHeal()
	{
		currentHealth = AdjustedMaxHealth;
		if (this.OnHealthChanged != null)
		{
			this.OnHealthChanged(currentHealth, AdjustedMaxHealth);
		}
	}

	public void SetHealthMaximum(float targetValue, float? amountOfHealthToGain = null, bool keepHealthPercentage = false)
	{
		if (targetValue == maximumHealth)
		{
			return;
		}
		float currentHealthPercentage = GetCurrentHealthPercentage();
		if (!keepHealthPercentage)
		{
			if (amountOfHealthToGain.HasValue)
			{
				currentHealth += amountOfHealthToGain.Value;
			}
			else if (targetValue > maximumHealth)
			{
				currentHealth += targetValue - maximumHealth;
			}
		}
		maximumHealth = targetValue;
		if (keepHealthPercentage)
		{
			currentHealth = currentHealthPercentage * AdjustedMaxHealth;
			if (amountOfHealthToGain.HasValue)
			{
				currentHealth += amountOfHealthToGain.Value;
			}
		}
		currentHealth = Mathf.Min(currentHealth, AdjustedMaxHealth);
		if (quantizeHealth)
		{
			currentHealth = BraveMathCollege.QuantizeFloat(currentHealth, quantizedIncrement);
			maximumHealth = BraveMathCollege.QuantizeFloat(maximumHealth, quantizedIncrement);
		}
		if (this.OnHealthChanged != null)
		{
			this.OnHealthChanged(currentHealth, AdjustedMaxHealth);
		}
	}

	public void ApplyDamage(float damage, Vector2 direction, string sourceName, CoreDamageTypes damageTypes = CoreDamageTypes.None, DamageCategory damageCategory = DamageCategory.Normal, bool ignoreInvulnerabilityFrames = false, PixelCollider hitPixelCollider = null, bool ignoreDamageCaps = false)
	{
		ApplyDamageDirectional(damage, direction, sourceName, damageTypes, damageCategory, ignoreInvulnerabilityFrames, hitPixelCollider, ignoreDamageCaps);
	}

	public float GetDamageModifierForType(CoreDamageTypes damageTypes)
	{
		float num = 1f;
		for (int i = 0; i < damageTypeModifiers.Count; i++)
		{
			if ((damageTypes & damageTypeModifiers[i].damageType) == damageTypeModifiers[i].damageType)
			{
				num *= damageTypeModifiers[i].damageMultiplier;
			}
		}
		if (isPlayerCharacter && (bool)m_player && (bool)m_player.CurrentGun && m_player.CurrentGun.currentGunDamageTypeModifiers != null)
		{
			for (int j = 0; j < m_player.CurrentGun.currentGunDamageTypeModifiers.Length; j++)
			{
				if ((damageTypes & m_player.CurrentGun.currentGunDamageTypeModifiers[j].damageType) == m_player.CurrentGun.currentGunDamageTypeModifiers[j].damageType)
				{
					num *= m_player.CurrentGun.currentGunDamageTypeModifiers[j].damageMultiplier;
				}
			}
		}
		return num;
	}

	private bool BossHealthSanityCheck(float rawDamage)
	{
		if (GameManager.Instance.PrimaryPlayer.healthHaver.IsDead)
		{
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				if ((!GameManager.Instance.SecondaryPlayer || GameManager.Instance.SecondaryPlayer.healthHaver.IsDead) && GetCurrentHealth() <= rawDamage)
				{
					return false;
				}
			}
			else if (GetCurrentHealth() <= rawDamage)
			{
				return false;
			}
		}
		return true;
	}

	protected void ApplyDamageDirectional(float damage, Vector2 direction, string damageSource, CoreDamageTypes damageTypes, DamageCategory damageCategory = DamageCategory.Normal, bool ignoreInvulnerabilityFrames = false, PixelCollider hitPixelCollider = null, bool ignoreDamageCaps = false)
	{
		if (GetCurrentHealth() > GetMaxHealth())
		{
			Debug.Log("Something went wrong in HealthHaver, but we caught it! " + currentHealth);
			currentHealth = GetMaxHealth();
		}
		if (PreventAllDamage && damageCategory == DamageCategory.Unstoppable)
		{
			PreventAllDamage = false;
		}
		if (PreventAllDamage || ((bool)m_player && m_player.IsGhost) || (hitPixelCollider != null && DamageableColliders != null && !DamageableColliders.Contains(hitPixelCollider)) || (IsBoss && !BossHealthSanityCheck(damage)) || isFirstFrame)
		{
			return;
		}
		if (ignoreInvulnerabilityFrames)
		{
			if (!vulnerable)
			{
				return;
			}
		}
		else if (!IsVulnerable)
		{
			return;
		}
		if (damage <= 0f)
		{
			return;
		}
		damage *= GetDamageModifierForType(damageTypes);
		damage *= AllDamageMultiplier;
		if (OnlyAllowSpecialBossDamage && (damageTypes & CoreDamageTypes.SpecialBossDamage) != CoreDamageTypes.SpecialBossDamage)
		{
			damage = 0f;
		}
		if (IsBoss && !string.IsNullOrEmpty(damageSource))
		{
			if (damageSource == "primaryplayer")
			{
				damage *= GameManager.Instance.PrimaryPlayer.stats.GetStatValue(PlayerStats.StatType.DamageToBosses);
			}
			else if (damageSource == "secondaryplayer")
			{
				damage *= GameManager.Instance.SecondaryPlayer.stats.GetStatValue(PlayerStats.StatType.DamageToBosses);
			}
		}
		if ((bool)m_player && !ignoreInvulnerabilityFrames)
		{
			damage = Mathf.Min(damage, 0.5f);
		}
		if ((bool)m_player && damageCategory == DamageCategory.BlackBullet)
		{
			damage = 1f;
		}
		if (ModifyDamage != null)
		{
			ModifyDamageEventArgs modifyDamageEventArgs = new ModifyDamageEventArgs();
			modifyDamageEventArgs.InitialDamage = damage;
			modifyDamageEventArgs.ModifiedDamage = damage;
			ModifyDamageEventArgs modifyDamageEventArgs2 = modifyDamageEventArgs;
			ModifyDamage(this, modifyDamageEventArgs2);
			damage = modifyDamageEventArgs2.ModifiedDamage;
		}
		if (!m_player && !ignoreInvulnerabilityFrames && damage <= 999f && !ignoreDamageCaps)
		{
			if (m_damageCap > 0f)
			{
				damage = Mathf.Min(m_damageCap, damage);
			}
			if (m_bossDpsCap > 0f)
			{
				damage = Mathf.Min(damage, m_bossDpsCap * 3f - m_recentBossDps);
				m_recentBossDps += damage;
			}
		}
		if (damage <= 0f)
		{
			return;
		}
		if (NextShotKills)
		{
			damage = 100000f;
		}
		if (damage > 0f && HasCrest)
		{
			HasCrest = false;
		}
		if (healthIsNumberOfHits)
		{
			damage = 1f;
		}
		if (!NextDamageIgnoresArmor && !NextShotKills && Armor > 0f)
		{
			Armor -= 1f;
			damage = 0f;
			if (isPlayerCharacter)
			{
				m_player.OnLostArmor();
			}
		}
		NextDamageIgnoresArmor = false;
		float num = damage;
		if (num > 999f)
		{
			num = 0f;
		}
		num = Mathf.Min(currentHealth, num);
		if (TrackPixelColliderDamage)
		{
			if (hitPixelCollider != null)
			{
				float value;
				if (PixelColliderDamage.TryGetValue(hitPixelCollider, out value))
				{
					PixelColliderDamage[hitPixelCollider] = value + damage;
				}
			}
			else if (damage <= 999f)
			{
				float num2 = damage * GlobalPixelColliderDamageMultiplier;
				List<PixelCollider> list = new List<PixelCollider>(PixelColliderDamage.Keys);
				for (int i = 0; i < list.Count; i++)
				{
					PixelCollider key = list[i];
					PixelColliderDamage[key] += num2;
				}
			}
		}
		currentHealth -= damage;
		if (isPlayerCharacter)
		{
			Debug.Log(currentHealth + "||" + damage);
		}
		if (quantizeHealth)
		{
			currentHealth = BraveMathCollege.QuantizeFloat(currentHealth, quantizedIncrement);
		}
		currentHealth = Mathf.Clamp(currentHealth, minimumHealth, AdjustedMaxHealth);
		if (!isPlayerCharacter)
		{
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				GameManager.Instance.AllPlayers[j].OnAnyEnemyTookAnyDamage(num, currentHealth <= 0f && Armor <= 0f, this);
			}
			if (!string.IsNullOrEmpty(damageSource))
			{
				switch (damageSource)
				{
				case "primaryplayer":
				case "Player ID 0":
					GameManager.Instance.PrimaryPlayer.OnDidDamage(damage, currentHealth <= 0f && Armor <= 0f, this);
					break;
				case "secondaryplayer":
				case "Player ID 1":
					GameManager.Instance.SecondaryPlayer.OnDidDamage(damage, currentHealth <= 0f && Armor <= 0f, this);
					break;
				}
			}
		}
		if (flashesOnDamage && base.spriteAnimator != null && !m_isFlashing)
		{
			if (m_flashOnHitCoroutine != null)
			{
				StopCoroutine(m_flashOnHitCoroutine);
			}
			m_flashOnHitCoroutine = null;
			if (materialsToFlash == null)
			{
				materialsToFlash = new List<Material>();
				outlineMaterialsToFlash = new List<Material>();
				sourceColors = new List<Color>();
			}
			if ((bool)base.gameActor)
			{
				for (int k = 0; k < materialsToFlash.Count; k++)
				{
					materialsToFlash[k].SetColor("_OverrideColor", base.gameActor.CurrentOverrideColor);
				}
			}
			if (outlineMaterialsToFlash != null)
			{
				for (int l = 0; l < outlineMaterialsToFlash.Count; l++)
				{
					if (l >= sourceColors.Count)
					{
						Debug.LogError("NOT ENOUGH SOURCE COLORS");
						break;
					}
					outlineMaterialsToFlash[l].SetColor("_OverrideColor", sourceColors[l]);
				}
			}
			m_flashOnHitCoroutine = StartCoroutine(FlashOnHit(damageCategory, hitPixelCollider));
		}
		if (incorporealityOnDamage && !m_isIncorporeal)
		{
			StartCoroutine("IncorporealityOnHit");
		}
		lastIncurredDamageSource = damageSource;
		lastIncurredDamageDirection = direction;
		if (shakesCameraOnDamage)
		{
			GameManager.Instance.MainCameraController.DoScreenShake(cameraShakeOnDamage, base.specRigidbody.UnitCenter);
		}
		if (NextShotKills)
		{
			Armor = 0f;
		}
		if (this.OnDamaged != null)
		{
			this.OnDamaged(currentHealth, AdjustedMaxHealth, damageTypes, damageCategory, direction);
		}
		if (this.OnHealthChanged != null)
		{
			this.OnHealthChanged(currentHealth, AdjustedMaxHealth);
		}
		if (currentHealth == 0f && Armor == 0f)
		{
			NextShotKills = false;
			if (!SuppressDeathSounds)
			{
				AkSoundEngine.PostEvent("Play_ENM_death", base.gameObject);
				AkSoundEngine.PostEvent(string.IsNullOrEmpty(overrideDeathAudioEvent) ? "Play_CHR_general_death_01" : overrideDeathAudioEvent, base.gameObject);
			}
			Die(direction);
		}
		else if (usesInvulnerabilityPeriod)
		{
			StartCoroutine(HandleInvulnerablePeriod());
		}
		if (damageCategory != 0 && damageCategory != DamageCategory.Collision)
		{
			return;
		}
		if (currentHealth <= 0f && Armor <= 0f)
		{
			if (!DisableStickyFriction)
			{
				StickyFrictionManager.Instance.RegisterDeathStickyFriction();
			}
		}
		else if (isPlayerCharacter)
		{
			StickyFrictionManager.Instance.RegisterPlayerDamageStickyFriction(damage);
		}
		else
		{
			StickyFrictionManager.Instance.RegisterOtherDamageStickyFriction(damage);
		}
	}

	public void Die(Vector2 finalDamageDirection)
	{
		EndFlashEffects();
		bool flag = false;
		if (spawnBulletScript && (!base.gameActor || !base.gameActor.IsFalling) && (chanceToSpawnBulletScript >= 1f || UnityEngine.Random.value < chanceToSpawnBulletScript))
		{
			flag = true;
			if (noCorpseWhenBulletScriptDeath)
			{
				base.aiActor.CorpseObject = null;
			}
			if (bulletScriptType == BulletScriptType.OnAnimEvent)
			{
				tk2dSpriteAnimator obj = base.spriteAnimator;
				obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(BulletScriptEventTriggered));
			}
		}
		if (this.OnPreDeath != null)
		{
			this.OnPreDeath(finalDamageDirection);
		}
		if (flag && bulletScriptType == BulletScriptType.OnPreDeath)
		{
			SpawnManager.SpawnBulletScript(base.aiActor, bulletScript);
		}
		if (GetCurrentHealth() > 0f || Armor > 0f)
		{
			return;
		}
		IsVulnerable = false;
		if (deathEffect != null)
		{
			SpawnManager.SpawnVFX(deathEffect, base.transform.position, Quaternion.identity);
		}
		if (IsBoss)
		{
			EndBossState(true);
		}
		if (ManualDeathHandling)
		{
			return;
		}
		if (base.spriteAnimator != null)
		{
			string value = ((!flag || string.IsNullOrEmpty(overrideDeathAnimBulletScript)) ? overrideDeathAnimation : overrideDeathAnimBulletScript);
			if (!string.IsNullOrEmpty(value))
			{
				tk2dSpriteAnimationClip tk2dSpriteAnimationClip2;
				if (base.aiAnimator != null)
				{
					base.aiAnimator.PlayUntilCancelled(value);
					tk2dSpriteAnimationClip2 = base.spriteAnimator.CurrentClip;
				}
				else
				{
					tk2dSpriteAnimationClip2 = base.spriteAnimator.GetClipByName(overrideDeathAnimation);
					if (tk2dSpriteAnimationClip2 != null)
					{
						base.spriteAnimator.Play(tk2dSpriteAnimationClip2);
					}
				}
				if (tk2dSpriteAnimationClip2 != null && !isPlayerCharacter && (!base.gameActor || !base.gameActor.IsFalling))
				{
					tk2dSpriteAnimator obj2 = base.spriteAnimator;
					obj2.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj2.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(DeathEventTriggered));
					tk2dSpriteAnimator obj3 = base.spriteAnimator;
					obj3.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj3.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(DeathAnimationComplete));
					return;
				}
			}
			else
			{
				if (base.aiAnimator != null)
				{
					base.aiAnimator.enabled = false;
				}
				float num = finalDamageDirection.ToAngle();
				tk2dSpriteAnimationClip tk2dSpriteAnimationClip3 = null;
				if (base.aiAnimator != null && base.aiAnimator.HasDirectionalAnimation("death"))
				{
					if (!base.aiAnimator.LockFacingDirection)
					{
						base.aiAnimator.LockFacingDirection = true;
						base.aiAnimator.FacingDirection = (num + 180f) % 360f;
					}
					base.aiAnimator.PlayUntilCancelled("death");
					tk2dSpriteAnimationClip3 = base.spriteAnimator.CurrentClip;
				}
				else if ((bool)base.gameActor && base.gameActor is PlayerSpaceshipController)
				{
					Exploder.DoDefaultExplosion(base.gameActor.CenterPosition, Vector2.zero);
					tk2dSpriteAnimationClip3 = null;
				}
				else
				{
					tk2dSpriteAnimationClip3 = GetDeathClip(BraveMathCollege.ClampAngle360(num + 22.5f));
					if (tk2dSpriteAnimationClip3 != null)
					{
						base.spriteAnimator.Play(tk2dSpriteAnimationClip3);
					}
				}
				if (tk2dSpriteAnimationClip3 != null && !isPlayerCharacter && (!base.gameActor || !base.gameActor.IsFalling))
				{
					tk2dSpriteAnimator obj4 = base.spriteAnimator;
					obj4.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj4.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(DeathEventTriggered));
					tk2dSpriteAnimator obj5 = base.spriteAnimator;
					obj5.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj5.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(DeathAnimationComplete));
					return;
				}
			}
		}
		if (spawnBulletScript && bulletScriptType == BulletScriptType.OnDeath && (!base.gameActor || !base.gameActor.IsFalling))
		{
			SpawnManager.SpawnBulletScript(base.aiActor, bulletScript);
		}
		FinalizeDeath();
	}

	public void EndBossState(bool triggerKillCam)
	{
		bool flag = false;
		List<AIActor> activeEnemies = base.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			HealthHaver healthHaver = activeEnemies[i].healthHaver;
			if ((bool)healthHaver && healthHaver.IsBoss && healthHaver.IsAlive && activeEnemies[i] != base.aiActor)
			{
				flag = true;
				break;
			}
		}
		if (HasHealthBar)
		{
			GameUIBossHealthController gameUIBossHealthController = (base.healthHaver.UsesVerticalBossBar ? GameUIRoot.Instance.bossControllerSide : (base.healthHaver.UsesSecondaryBossBar ? GameUIRoot.Instance.bossController2 : GameUIRoot.Instance.bossController));
			gameUIBossHealthController.DeregisterBossHealthHaver(this);
		}
		if (flag)
		{
			return;
		}
		if (triggerKillCam)
		{
			GameUIRoot.Instance.TriggerBossKillCam(null, base.specRigidbody);
		}
		if (HasHealthBar)
		{
			GameUIRoot.Instance.bossController.DisableBossHealth();
			GameUIRoot.Instance.bossController2.DisableBossHealth();
			GameUIRoot.Instance.bossControllerSide.DisableBossHealth();
		}
		if (triggerKillCam)
		{
			if (!forcePreventVictoryMusic)
			{
				GameManager.Instance.DungeonMusicController.EndBossMusic();
			}
		}
		else
		{
			GameManager.Instance.DungeonMusicController.EndBossMusicNoVictory();
		}
	}

	public tk2dSpriteAnimationClip GetDeathClip(float damageAngle)
	{
		if (!base.spriteAnimator)
		{
			return null;
		}
		int a = Mathf.FloorToInt(BraveMathCollege.ClampAngle360(damageAngle) / 45f);
		a = Mathf.Max(0, Mathf.Min(a, 7));
		string[] array = new string[8] { "die_right", "die_back_right", "die_back", "die_back_left", "die_left", "die_front_left", "die_front", "die_front_right" };
		string text = array[a];
		tk2dSpriteAnimationClip tk2dSpriteAnimationClip2 = base.spriteAnimator.GetClipByName(text);
		if (tk2dSpriteAnimationClip2 == null)
		{
			tk2dSpriteAnimationClip2 = ((a != 7 && a != 0 && a != 1 && a != 2) ? base.spriteAnimator.GetClipByName("die_left") : base.spriteAnimator.GetClipByName("die_right"));
		}
		if (tk2dSpriteAnimationClip2 == null)
		{
			tk2dSpriteAnimationClip2 = base.spriteAnimator.GetClipByName("death");
		}
		if (tk2dSpriteAnimationClip2 == null)
		{
			tk2dSpriteAnimationClip2 = base.spriteAnimator.GetClipByName("die");
		}
		if (isPlayerCharacter && m_player.hasArmorlessAnimations && m_player.healthHaver.Armor == 0f)
		{
			tk2dSpriteAnimationClip2 = base.spriteAnimator.GetClipByName("death_armorless");
		}
		return tk2dSpriteAnimationClip2;
	}

	public void EndFlashEffects()
	{
		if (m_flashOnHitCoroutine != null)
		{
			StopCoroutine(m_flashOnHitCoroutine);
		}
		m_flashOnHitCoroutine = null;
		EndFlashOnHit();
		StopCoroutine("IncorporealityOnHit");
		EndIncorporealityOnHit();
	}

	private void BulletScriptEventTriggered(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip, int frameNum)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNum);
		if (frame.eventInfo == "fire")
		{
			SpawnManager.SpawnBulletScript(base.aiActor, bulletScript);
		}
	}

	public void UpdateCachedOutlineColor(Material m, Color c)
	{
		if (outlineMaterialsToFlash != null && outlineMaterialsToFlash.Contains(m))
		{
			int num = outlineMaterialsToFlash.IndexOf(m);
			if (sourceColors != null && sourceColors.Count > num && num >= 0)
			{
				sourceColors[num] = c;
			}
		}
	}

	private IEnumerator FlashOnHit(DamageCategory sourceDamageCategory, PixelCollider hitPixelCollider)
	{
		if (currentHealth <= 0f && Armor <= 0f)
		{
			m_flashOnHitCoroutine = null;
			yield break;
		}
		m_isFlashing = true;
		if (isPlayerCharacter || sourceDamageCategory != DamageCategory.DamageOverTime)
		{
			AkSoundEngine.PostEvent("Play_CHR_general_hurt_01", base.gameObject);
		}
		else if (m_hitBarkLimiter % 2 == 0)
		{
			AkSoundEngine.PostEvent("Play_CHR_general_hurt_01", base.gameObject);
		}
		m_hitBarkLimiter++;
		if (isPlayerCharacter || sourceDamageCategory != DamageCategory.DamageOverTime)
		{
			AkSoundEngine.PostEvent("Play_ENM_hurt", base.gameObject);
		}
		if ((bool)base.gameActor)
		{
			base.gameActor.OverrideColorOverridden = true;
		}
		Color overrideColor = Color.white;
		overrideColor.a = 1f;
		if ((bool)base.sprite)
		{
			base.sprite.usesOverrideMaterial = true;
		}
		if (materialsToEnableBrightnessClampOn == null)
		{
			materialsToEnableBrightnessClampOn = new List<Material>();
		}
		else
		{
			materialsToEnableBrightnessClampOn.Clear();
		}
		List<tk2dBaseSprite> spritesToFlash = bodySprites;
		tk2dBaseSprite value;
		if (m_independentDamageFlashers != null && hitPixelCollider != null && m_independentDamageFlashers.TryGetValue(hitPixelCollider, out value))
		{
			spritesToFlash = new List<tk2dBaseSprite>(1) { value };
		}
		materialsToFlash.Clear();
		outlineMaterialsToFlash.Clear();
		for (int i = 0; i < spritesToFlash.Count; i++)
		{
			Material material = spritesToFlash[i].renderer.material;
			materialsToFlash.Add(material);
			for (int j = 0; j < material.shaderKeywords.Length; j++)
			{
				if (material.shaderKeywords[j] == "BRIGHTNESS_CLAMP_ON")
				{
					material.DisableKeyword("BRIGHTNESS_CLAMP_ON");
					material.EnableKeyword("BRIGHTNESS_CLAMP_OFF");
					materialsToEnableBrightnessClampOn.Add(material);
					break;
				}
			}
			tk2dSprite[] outlineSprites = SpriteOutlineManager.GetOutlineSprites(spritesToFlash[i]);
			for (int k = 0; k < outlineSprites.Length; k++)
			{
				if ((bool)outlineSprites[k] && (bool)outlineSprites[k].renderer && (bool)outlineSprites[k].renderer.material)
				{
					outlineMaterialsToFlash.Add(outlineSprites[k].renderer.material);
				}
			}
		}
		sourceColors.Clear();
		for (int l = 0; l < materialsToFlash.Count; l++)
		{
			materialsToFlash[l].SetColor("_OverrideColor", overrideColor);
		}
		for (int m = 0; m < outlineMaterialsToFlash.Count; m++)
		{
			sourceColors.Add(outlineMaterialsToFlash[m].GetColor("_OverrideColor"));
			outlineMaterialsToFlash[m].SetColor("_OverrideColor", overrideColor);
		}
		for (float elapsed = 0f; elapsed < 0.04f; elapsed += BraveTime.DeltaTime)
		{
			if (!(currentHealth > 0f) && !(Armor > 0f))
			{
				break;
			}
			float t = 1f - elapsed / 0.04f;
			if (currentHealth > 0f || Armor > 0f)
			{
				if ((bool)base.gameActor)
				{
					for (int n = 0; n < materialsToFlash.Count; n++)
					{
						materialsToFlash[n].SetColor("_OverrideColor", Color.Lerp(base.gameActor.CurrentOverrideColor, overrideColor, t));
					}
				}
				for (int num = 0; num < outlineMaterialsToFlash.Count; num++)
				{
					outlineMaterialsToFlash[num].SetColor("_OverrideColor", Color.Lerp(sourceColors[num], overrideColor, t));
				}
			}
			yield return null;
		}
		if ((bool)base.gameActor)
		{
			base.gameActor.OverrideColorOverridden = false;
			for (int num2 = 0; num2 < materialsToFlash.Count; num2++)
			{
				materialsToFlash[num2].SetColor("_OverrideColor", base.gameActor.CurrentOverrideColor);
			}
		}
		for (int num3 = 0; num3 < outlineMaterialsToFlash.Count; num3++)
		{
			outlineMaterialsToFlash[num3].SetColor("_OverrideColor", sourceColors[num3]);
		}
		for (int num4 = 0; num4 < materialsToEnableBrightnessClampOn.Count; num4++)
		{
			materialsToEnableBrightnessClampOn[num4].DisableKeyword("BRIGHTNESS_CLAMP_OFF");
			materialsToEnableBrightnessClampOn[num4].EnableKeyword("BRIGHTNESS_CLAMP_ON");
		}
		yield return new WaitForSeconds(0.2f);
		m_flashOnHitCoroutine = null;
		m_isFlashing = false;
	}

	private void EndFlashOnHit()
	{
		if (m_flashOnHitCoroutine != null)
		{
			StopCoroutine(m_flashOnHitCoroutine);
		}
		m_flashOnHitCoroutine = null;
		if ((bool)base.gameActor)
		{
			base.gameActor.OverrideColorOverridden = false;
		}
		if (materialsToFlash != null && materialsToFlash.Count > 0 && (bool)base.gameActor)
		{
			for (int i = 0; i < materialsToFlash.Count; i++)
			{
				materialsToFlash[i].SetColor("_OverrideColor", base.gameActor.CurrentOverrideColor);
			}
		}
		if (outlineMaterialsToFlash != null && outlineMaterialsToFlash.Count > 0)
		{
			for (int j = 0; j < outlineMaterialsToFlash.Count; j++)
			{
				outlineMaterialsToFlash[j].SetColor("_OverrideColor", sourceColors[j]);
			}
		}
		if (materialsToEnableBrightnessClampOn != null && materialsToEnableBrightnessClampOn.Count > 0)
		{
			for (int k = 0; k < materialsToEnableBrightnessClampOn.Count; k++)
			{
				materialsToEnableBrightnessClampOn[k].DisableKeyword("BRIGHTNESS_CLAMP_OFF");
				materialsToEnableBrightnessClampOn[k].EnableKeyword("BRIGHTNESS_CLAMP_ON");
			}
		}
		m_isFlashing = false;
	}

	private IEnumerator IncorporealityOnHit()
	{
		m_isIncorporeal = true;
		if (!isPlayerCharacter)
		{
			Debug.LogError("Incorporeality is currently only supported on the player.", this);
		}
		PlayerController player = GetComponent<PlayerController>();
		if (player == null)
		{
			Debug.LogError("Failed to incorporeal something...");
			yield break;
		}
		int enemyMask = CollisionMask.LayerToMask(CollisionLayer.EnemyCollider, CollisionLayer.EnemyHitBox, CollisionLayer.Projectile);
		player.specRigidbody.AddCollisionLayerIgnoreOverride(enemyMask);
		yield return null;
		float timer = 0f;
		float subtimer = 0f;
		while (timer < incorporealityTime)
		{
			while (timer < incorporealityTime)
			{
				timer += BraveTime.DeltaTime;
				subtimer += BraveTime.DeltaTime;
				if (subtimer > 0.12f)
				{
					player.IsVisible = false;
					subtimer -= 0.12f;
					break;
				}
				yield return null;
			}
			while (timer < incorporealityTime)
			{
				timer += BraveTime.DeltaTime;
				subtimer += BraveTime.DeltaTime;
				if (subtimer > 0.12f)
				{
					player.IsVisible = true;
					subtimer -= 0.12f;
					break;
				}
				yield return null;
			}
		}
		EndIncorporealityOnHit();
	}

	private void EndIncorporealityOnHit()
	{
		PlayerController component = GetComponent<PlayerController>();
		if (!(component == null))
		{
			int mask = CollisionMask.LayerToMask(CollisionLayer.EnemyCollider, CollisionLayer.EnemyHitBox, CollisionLayer.Projectile);
			component.IsVisible = true;
			component.specRigidbody.RemoveCollisionLayerIgnoreOverride(mask);
			m_isIncorporeal = false;
		}
	}

	private void DeathEventTriggered(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNo)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNo);
		if (frame.eventInfo == "disableColliders")
		{
			for (int i = 0; i < base.specRigidbody.PixelColliders.Count; i++)
			{
				base.specRigidbody.PixelColliders[i].Enabled = false;
			}
		}
	}

	public void DeathAnimationComplete(tk2dSpriteAnimator spriteAnimator, tk2dSpriteAnimationClip clip)
	{
		FinalizeDeath();
	}

	private void FinalizeDeath()
	{
		if (this.OnDeath != null)
		{
			this.OnDeath(lastIncurredDamageDirection);
		}
		if ((bool)base.aiActor && base.aiActor.IsFalling && !base.aiActor.HasSplashed)
		{
			GameManager.Instance.Dungeon.tileIndices.DoSplashAtPosition(base.sprite.WorldCenter);
			base.aiActor.HasSplashed = true;
		}
		if (GameManager.Instance.InTutorial && !isPlayerCharacter)
		{
			GameManager.BroadcastRoomTalkDoerFsmEvent("enemyKilled");
		}
		if (!persistsOnDeath)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void TriggerInvulnerabilityPeriod(float overrideTime = -1f)
	{
		if (usesInvulnerabilityPeriod)
		{
			StartCoroutine(HandleInvulnerablePeriod(overrideTime));
		}
	}

	protected IEnumerator HandleInvulnerablePeriod(float overrideTime = -1f)
	{
		vulnerable = false;
		if (useFortunesFavorInvulnerability && (bool)base.ultraFortunesFavor)
		{
			base.ultraFortunesFavor.enabled = true;
		}
		if (overrideTime <= 0f)
		{
			yield return new WaitForSeconds(invulnerabilityPeriod);
		}
		else
		{
			yield return new WaitForSeconds(overrideTime);
		}
		vulnerable = true;
		if (useFortunesFavorInvulnerability && (bool)base.ultraFortunesFavor)
		{
			base.ultraFortunesFavor.enabled = false;
		}
	}

	public void ApplyDamageModifiers(List<DamageTypeModifier> newDamageTypeModifiers)
	{
		for (int i = 0; i < newDamageTypeModifiers.Count; i++)
		{
			DamageTypeModifier damageTypeModifier = newDamageTypeModifiers[i];
			bool flag = false;
			for (int j = 0; j < damageTypeModifiers.Count; j++)
			{
				DamageTypeModifier damageTypeModifier2 = damageTypeModifiers[j];
				if (damageTypeModifier.damageType == damageTypeModifier2.damageType)
				{
					damageTypeModifier2.damageMultiplier = damageTypeModifier.damageMultiplier;
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				damageTypeModifiers.Add(new DamageTypeModifier(damageTypeModifier));
			}
		}
	}

	public SpeculativeRigidbody GetBodyRigidbody(int index)
	{
		if (bodyRigidbodies != null)
		{
			return bodyRigidbodies[index];
		}
		return base.specRigidbody;
	}
}
