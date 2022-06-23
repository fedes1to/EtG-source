using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public abstract class GameActor : DungeonPlaceableBehaviour, IAutoAimTarget
{
	public delegate void MovementModifier(ref Vector2 volundaryVel, ref Vector2 involuntaryVel);

	[Header("Actor Shared Properties")]
	public string ActorName;

	public string OverrideDisplayName;

	[EnumFlags]
	public CoreActorTypes actorTypes;

	[Space(3f)]
	public bool HasShadow = true;

	[ShowInInspectorIf("HasShadow", true)]
	public GameObject ShadowPrefab;

	[ShowInInspectorIf("HasShadow", true)]
	public GameObject ShadowObject;

	[ShowInInspectorIf("HasShadow", true)]
	public float ShadowHeightOffGround;

	[ShowInInspectorIf("HasShadow", true)]
	public Transform ShadowParent;

	[ShowInInspectorIf("HasShadow", true)]
	public Vector3 ActorShadowOffset;

	[Space(3f)]
	public bool DoDustUps;

	[ShowInInspectorIf("DoDustUps", true)]
	public float DustUpInterval;

	[ShowInInspectorIf("DoDustUps", true)]
	public GameObject OverrideDustUp;

	[Space(3f)]
	public float FreezeDispelFactor = 20f;

	public bool ImmuneToAllEffects;

	public ActorEffectResistance[] EffectResistances;

	public const float OUTLINE_DEPTH = 0.1f;

	public const float GUN_DEPTH = 0.075f;

	public const float ACTOR_VFX_DEPTH = 0.2f;

	public const float BACKFACING_ANGLE_MAX = 155f;

	public const float BACKFACING_ANGLE_MIN = 25f;

	public const float BACKWARDS_ANGLE_MAX = 120f;

	public const float BACKWARDS_ANGLE_MIN = 60f;

	public const float FORWARDS_ANGLE_MAX = -60f;

	public const float FORWARDS_ANGLE_MIN = -120f;

	public const float FLIP_LEFT_THRESHOLD_FRONT = 105f;

	public const float FLIP_RIGHT_THRESHOLD_FRONT = 75f;

	public const float FLIP_LEFT_THRESHOLD_BACK = 105f;

	public const float FLIP_RIGHT_THRESHOLD_BACK = 75f;

	[NonSerialized]
	public bool FallingProhibited;

	private GameObject m_stealthVfx;

	[NonSerialized]
	public float actorReflectionAdditionalOffset;

	protected GoopDefinition m_currentGoop;

	protected bool m_currentGoopFrozen;

	[NonSerialized]
	public Vector2 ImpartedVelocity;

	protected int m_overrideColorID;

	protected int m_overrideFlatColorID;

	protected int m_specialFlagsID;

	protected int m_stencilID;

	[NonSerialized]
	public bool IsOverPitAtAll;

	public Func<bool, bool> OnAboutToFall;

	public bool OverrideColorOverridden;

	private List<string> m_overrideColorSources = new List<string>();

	private List<Color> m_overrideColorStack = new List<Color>();

	protected Material m_colorOverridenMaterial;

	protected Shader m_colorOverridenShader;

	[NonSerialized]
	public float BeamStatusAmount;

	protected Vector2 m_cachedPosition;

	protected bool m_isFalling;

	protected OverridableBool m_isFlying = new OverridableBool(false);

	protected OverridableBool m_isStealthed = new OverridableBool(false);

	protected float m_dustUpTimer;

	protected List<MovingPlatform> m_supportingPlatforms = new List<MovingPlatform>();

	protected List<GameActorEffect> m_activeEffects = new List<GameActorEffect>();

	protected List<RuntimeGameActorEffectData> m_activeEffectData = new List<RuntimeGameActorEffectData>();

	public abstract Gun CurrentGun { get; }

	public abstract Transform GunPivot { get; }

	public virtual Transform SecondaryGunPivot
	{
		get
		{
			return GunPivot;
		}
	}

	public abstract bool SpriteFlipped { get; }

	public abstract Vector3 SpriteDimensions { get; }

	public List<MovingPlatform> SupportingPlatforms
	{
		get
		{
			return m_supportingPlatforms;
		}
	}

	public Vector2 CenterPosition
	{
		get
		{
			if (!base.specRigidbody || base.specRigidbody.HitboxPixelCollider == null)
			{
				if ((bool)base.sprite)
				{
					return base.sprite.WorldCenter;
				}
				return base.transform.position.XY();
			}
			return base.specRigidbody.HitboxPixelCollider.UnitCenter;
		}
	}

	public bool IsFalling
	{
		get
		{
			return m_isFalling;
		}
	}

	public virtual bool IsFlying
	{
		get
		{
			return m_isFlying.Value;
		}
	}

	public bool IsGrounded
	{
		get
		{
			return base.spriteAnimator.QueryGroundedFrame() && !IsFlying && !FallingProhibited;
		}
	}

	public bool IsStealthed
	{
		get
		{
			return m_isStealthed.Value;
		}
	}

	protected virtual float DustUpMultiplier
	{
		get
		{
			return 1f;
		}
	}

	public GoopDefinition CurrentGoop
	{
		get
		{
			return m_currentGoop;
		}
	}

	public float FreezeAmount { get; set; }

	public float CheeseAmount { get; set; }

	public bool StealthDeath { get; set; }

	public bool IsFrozen { get; set; }

	public bool IsCheezen { get; set; }

	public bool IsGone { get; set; }

	public bool SuppressEffectUpdates { get; set; }

	public float FacingDirection
	{
		get
		{
			if ((bool)base.aiAnimator)
			{
				return base.aiAnimator.FacingDirection;
			}
			if (base.gameActor is PlayerController)
			{
				PlayerController playerController = base.gameActor as PlayerController;
				return (playerController.unadjustedAimPoint.XY() - playerController.specRigidbody.GetUnitCenter(ColliderType.HitBox)).ToAngle();
			}
			return -90f;
		}
	}

	public bool PreventAutoAimVelocity { get; set; }

	public bool IsValid
	{
		get
		{
			if (!this || base.healthHaver.IsDead || IsFalling || IsGone)
			{
				return false;
			}
			if (!base.specRigidbody.enabled || base.specRigidbody.GetPixelCollider(ColliderType.HitBox) == null)
			{
				return false;
			}
			AIActor aIActor = this as AIActor;
			if ((bool)aIActor)
			{
				return aIActor.IsWorthShootingAt;
			}
			return true;
		}
	}

	public Vector2 AimCenter
	{
		get
		{
			return base.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
	}

	public Vector2 Velocity
	{
		get
		{
			if (PreventAutoAimVelocity)
			{
				return Vector2.zero;
			}
			return base.specRigidbody.Velocity;
		}
	}

	public bool IgnoreForSuperDuperAutoAim
	{
		get
		{
			return false;
		}
	}

	public float MinDistForSuperDuperAutoAim
	{
		get
		{
			return 0f;
		}
	}

	public Color CurrentOverrideColor
	{
		get
		{
			if (m_overrideColorStack.Count == 0)
			{
				RegisterOverrideColor(new Color(1f, 1f, 1f, 0f), "base");
			}
			return m_overrideColorStack[m_overrideColorStack.Count - 1];
		}
	}

	public event MovementModifier MovementModifiers;

	public float GetResistanceForEffectType(EffectResistanceType resistType)
	{
		if (resistType == EffectResistanceType.None)
		{
			return 0f;
		}
		for (int i = 0; i < EffectResistances.Length; i++)
		{
			if (EffectResistances[i].resistType == resistType)
			{
				return EffectResistances[i].resistAmount;
			}
		}
		return 0f;
	}

	public void SetIsStealthed(bool value, string reason)
	{
		bool isStealthed = IsStealthed;
		m_isStealthed.SetOverride(reason, value);
		if (IsStealthed != isStealthed)
		{
			if (IsStealthed)
			{
				m_stealthVfx = PlayEffectOnActor(BraveResources.Load<GameObject>("Global VFX/VFX_Stealthed"), new Vector3(0f, 1.375f, 0f), true, true);
			}
			else if ((bool)m_stealthVfx)
			{
				UnityEngine.Object.Destroy(m_stealthVfx);
			}
		}
	}

	public void SetIsFlying(bool value, string reason, bool adjustShadow = true, bool modifyPathing = false)
	{
		m_isFlying.SetOverride(reason, value);
		if (adjustShadow && HasShadow && (bool)ShadowObject)
		{
			if (value)
			{
				ShadowObject.transform.position = ShadowObject.transform.position + new Vector3(0f, -0.3f, 0f);
			}
			else
			{
				ShadowObject.transform.position = ShadowObject.transform.position + new Vector3(0f, 0.3f, 0f);
			}
		}
		base.specRigidbody.CanBeCarried = !m_isFlying.Value;
		AIActor aIActor = this as AIActor;
		if (modifyPathing && (bool)aIActor)
		{
			if (value)
			{
				aIActor.PathableTiles |= CellTypes.PIT;
			}
			else
			{
				aIActor.PathableTiles &= ~CellTypes.PIT;
			}
		}
	}

	public virtual void Awake()
	{
		m_overrideColorID = Shader.PropertyToID("_OverrideColor");
		RegisterOverrideColor(new Color(1f, 1f, 1f, 0f), "base");
	}

	public virtual void Start()
	{
		if ((bool)base.specRigidbody)
		{
			base.specRigidbody.Initialize();
		}
	}

	public virtual void Update()
	{
		BeamStatusAmount = Mathf.Max(0f, BeamStatusAmount - BraveTime.DeltaTime / 2f);
		if (!SuppressEffectUpdates)
		{
			for (int i = 0; i < m_activeEffects.Count; i++)
			{
				GameActorEffect gameActorEffect = m_activeEffects[i];
				if (gameActorEffect == null || m_activeEffectData == null || i >= m_activeEffectData.Count)
				{
					continue;
				}
				RuntimeGameActorEffectData runtimeGameActorEffectData = m_activeEffectData[i];
				if (runtimeGameActorEffectData == null)
				{
					continue;
				}
				gameActorEffect.EffectTick(this, runtimeGameActorEffectData);
				if (runtimeGameActorEffectData.instanceOverheadVFX != null)
				{
					if ((bool)base.healthHaver && base.healthHaver.IsAlive && (bool)runtimeGameActorEffectData.instanceOverheadVFX)
					{
						Vector2 vector = base.transform.position.XY();
						if (gameActorEffect.PlaysVFXOnActor)
						{
							if ((bool)base.specRigidbody && base.specRigidbody.HitboxPixelCollider != null)
							{
								vector = base.specRigidbody.HitboxPixelCollider.UnitBottomCenter.Quantize(0.0625f);
							}
							runtimeGameActorEffectData.instanceOverheadVFX.transform.position = vector;
						}
						else
						{
							if ((bool)base.specRigidbody && base.specRigidbody.HitboxPixelCollider != null)
							{
								vector = base.specRigidbody.HitboxPixelCollider.UnitTopCenter.Quantize(0.0625f);
							}
							runtimeGameActorEffectData.instanceOverheadVFX.transform.position = vector;
						}
						runtimeGameActorEffectData.instanceOverheadVFX.renderer.enabled = !IsGone;
					}
					else if ((bool)runtimeGameActorEffectData.instanceOverheadVFX)
					{
						UnityEngine.Object.Destroy(runtimeGameActorEffectData.instanceOverheadVFX.gameObject);
					}
				}
				float num = 1f;
				if (gameActorEffect is GameActorCharmEffect && PassiveItem.IsFlagSetAtAll(typeof(BattleStandardItem)))
				{
					num /= BattleStandardItem.BattleStandardCharmDurationMultiplier;
				}
				runtimeGameActorEffectData.elapsed += BraveTime.DeltaTime * num;
				runtimeGameActorEffectData.tickCounter += BraveTime.DeltaTime;
				if (gameActorEffect.IsFinished(this, runtimeGameActorEffectData, runtimeGameActorEffectData.elapsed))
				{
					RemoveEffect(gameActorEffect);
				}
			}
		}
		if (DoDustUps && !GameManager.Instance.IsLoadingLevel && (bool)base.specRigidbody)
		{
			bool flag = base.specRigidbody.Velocity.magnitude > 0f && !m_isFalling && !IsFlying;
			bool flag2 = false;
			Vector2 unitBottomCenter = base.specRigidbody.PrimaryPixelCollider.UnitBottomCenter;
			IntVector2 intVector = unitBottomCenter.ToIntVector2(VectorConversions.Floor);
			if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(intVector))
			{
				CellData cellData = GameManager.Instance.Dungeon.data[intVector];
				CellVisualData.CellFloorType cellFloorType = cellData.cellVisualData.floorType;
				if (flag && this is PlayerController)
				{
					flag &= base.spriteAnimator.QueryGroundedFrame() || IsFlying;
					flag &= !GameManager.Instance.Dungeon.CellIsPit(base.specRigidbody.UnitCenter);
					flag &= ((PlayerController)this).IsVisible;
					flag = flag && cellFloorType != CellVisualData.CellFloorType.Ice;
				}
				PlayerController playerController = this as PlayerController;
				if ((bool)playerController && playerController.IsGhost)
				{
					flag = true;
					flag2 = true;
				}
				else if ((bool)playerController && playerController.IsSlidingOverSurface)
				{
					flag = false;
				}
				if (flag)
				{
					m_dustUpTimer += BraveTime.DeltaTime;
					if (m_dustUpTimer >= DustUpInterval / DustUpMultiplier)
					{
						if ((bool)OverrideDustUp)
						{
							SpawnManager.SpawnVFX(OverrideDustUp, unitBottomCenter, Quaternion.identity);
							m_dustUpTimer = 0f;
						}
						else if (flag2)
						{
							SpawnManager.SpawnVFX(ResourceCache.Acquire("Global VFX/GhostDustUp") as GameObject, unitBottomCenter, Quaternion.identity);
							m_dustUpTimer = 0f;
						}
						else
						{
							SharedDungeonSettings sharedSettingsPrefab = GameManager.Instance.Dungeon.sharedSettingsPrefab;
							DustUpVFX dungeonDustups = GameManager.Instance.Dungeon.dungeonDustups;
							Color value = Color.clear;
							bool flag3 = false;
							bool flag4 = false;
							if (m_currentGoop != null)
							{
								if (m_currentGoopFrozen)
								{
									cellFloorType = CellVisualData.CellFloorType.Ice;
								}
								else
								{
									cellFloorType = (m_currentGoop.usesWaterVfx ? CellVisualData.CellFloorType.Water : CellVisualData.CellFloorType.ThickGoop);
									flag3 = m_currentGoop.AppliesCheese;
									flag4 = m_currentGoop.AppliesSpeedModifierContinuously && m_currentGoop.playerStepsChangeLifetime && m_currentGoop.SpeedModifierEffect.effectIdentifier.StartsWith("phase web", StringComparison.Ordinal);
								}
								value = m_currentGoop.baseColor32;
							}
							if (cellFloorType == CellVisualData.CellFloorType.Water && dungeonDustups.waterDustup != null)
							{
								GameObject gameObject = SpawnManager.SpawnVFX(dungeonDustups.waterDustup, unitBottomCenter, Quaternion.identity);
								if ((bool)gameObject)
								{
									Renderer component = gameObject.GetComponent<Renderer>();
									if ((bool)component)
									{
										gameObject.GetComponent<tk2dBaseSprite>().OverrideMaterialMode = tk2dBaseSprite.SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_SIMPLE;
										component.material.SetColor(m_overrideColorID, value);
									}
								}
								if (dungeonDustups.additionalWaterDustup != null)
								{
									SpawnManager.SpawnVFX(dungeonDustups.additionalWaterDustup, unitBottomCenter, Quaternion.identity, true);
								}
							}
							else
							{
								switch (cellFloorType)
								{
								case CellVisualData.CellFloorType.ThickGoop:
									if (flag3)
									{
										if (sharedSettingsPrefab.additionalCheeseDustup != null)
										{
											SpawnManager.SpawnVFX(sharedSettingsPrefab.additionalCheeseDustup, unitBottomCenter + new Vector2(0.0625f, -0.25f), Quaternion.identity, true);
										}
									}
									else if (flag4 && sharedSettingsPrefab.additionalWebDustup != null)
									{
										SpawnManager.SpawnVFX(sharedSettingsPrefab.additionalWebDustup, unitBottomCenter + new Vector2(0.0625f, -0.25f), Quaternion.identity, true);
									}
									break;
								default:
									SpawnManager.SpawnVFX(dungeonDustups.runDustup, unitBottomCenter, Quaternion.identity);
									break;
								case CellVisualData.CellFloorType.Ice:
									break;
								}
							}
							m_dustUpTimer = 0f;
							if (flag4)
							{
								m_dustUpTimer = (0f - DustUpInterval) / DustUpMultiplier;
							}
						}
					}
				}
				else if ((bool)playerController && playerController.IsSlidingOverSurface)
				{
					m_dustUpTimer += BraveTime.DeltaTime;
					if (m_dustUpTimer >= DustUpInterval / DustUpMultiplier)
					{
						DustUpVFX dungeonDustups2 = GameManager.Instance.Dungeon.dungeonDustups;
						GameObject gameObject2 = SpawnManager.SpawnVFX(GameManager.Instance.Dungeon.sharedSettingsPrefab.additionalTableDustup, unitBottomCenter + new Vector2(0.0625f, 0.25f), Quaternion.identity);
						if ((bool)gameObject2)
						{
							tk2dBaseSprite component2 = gameObject2.GetComponent<tk2dBaseSprite>();
							if ((bool)component2)
							{
								component2.HeightOffGround = 0f;
								component2.UpdateZDepth();
							}
						}
						m_dustUpTimer = 0f;
					}
				}
			}
		}
		if (!GameManager.Instance.IsLoadingLevel)
		{
			HandleGoopChecks();
		}
	}

	public void ApplyEffect(GameActorEffect effect, float sourcePartialAmount = 1f, Projectile sourceProjectile = null)
	{
		if (ImmuneToAllEffects || (!effect.AffectsPlayers && this is PlayerController) || (!effect.AffectsEnemies && this is AIActor))
		{
			return;
		}
		float num = sourcePartialAmount;
		EffectResistanceType effectResistanceType = effect.resistanceType;
		if (effectResistanceType == EffectResistanceType.None)
		{
			if (effect.effectIdentifier == "poison")
			{
				effectResistanceType = EffectResistanceType.Poison;
			}
			if (effect.effectIdentifier == "fire")
			{
				effectResistanceType = EffectResistanceType.Fire;
			}
			if (effect.effectIdentifier == "freeze")
			{
				effectResistanceType = EffectResistanceType.Freeze;
			}
			if (effect.effectIdentifier == "charm")
			{
				effectResistanceType = EffectResistanceType.Charm;
			}
		}
		num *= 1f - GetResistanceForEffectType(effectResistanceType);
		if (num == 0f || (effect is GameActorCharmEffect && base.healthHaver != null && base.healthHaver.IsBoss))
		{
			return;
		}
		for (int i = 0; i < m_activeEffects.Count; i++)
		{
			if (!(m_activeEffects[i].effectIdentifier == effect.effectIdentifier))
			{
				continue;
			}
			switch (effect.stackMode)
			{
			case GameActorEffect.EffectStackingMode.Stack:
				m_activeEffectData[i].elapsed -= effect.duration;
				if (effect.maxStackedDuration > 0f)
				{
					m_activeEffectData[i].elapsed = Mathf.Max(effect.duration - effect.maxStackedDuration, m_activeEffectData[i].elapsed);
				}
				break;
			case GameActorEffect.EffectStackingMode.Refresh:
				m_activeEffectData[i].elapsed = 0f;
				break;
			case GameActorEffect.EffectStackingMode.Ignore:
				break;
			case GameActorEffect.EffectStackingMode.DarkSoulsAccumulate:
				effect.OnDarkSoulsAccumulate(this, m_activeEffectData[i], num, sourceProjectile);
				break;
			}
			return;
		}
		RuntimeGameActorEffectData runtimeGameActorEffectData = new RuntimeGameActorEffectData();
		runtimeGameActorEffectData.actor = this;
		effect.ApplyTint(this);
		if (effect.OverheadVFX != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(effect.OverheadVFX);
			tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
			gameObject.transform.parent = base.transform;
			if (base.healthHaver.IsBoss)
			{
				gameObject.transform.position = base.specRigidbody.HitboxPixelCollider.UnitTopCenter;
			}
			else
			{
				Bounds bounds = base.sprite.GetBounds();
				Vector3 vector = base.transform.position + new Vector3((bounds.max.x + bounds.min.x) / 2f, bounds.max.y, 0f).Quantize(0.0625f);
				if (effect.PlaysVFXOnActor)
				{
					vector.y = base.transform.position.y + bounds.min.y;
				}
				gameObject.transform.position = base.sprite.WorldCenter.ToVector3ZUp().WithY(vector.y);
			}
			component.HeightOffGround = 0.5f;
			base.sprite.AttachRenderer(component);
			runtimeGameActorEffectData.instanceOverheadVFX = gameObject.GetComponent<tk2dBaseSprite>();
			if (IsGone)
			{
				runtimeGameActorEffectData.instanceOverheadVFX.renderer.enabled = false;
			}
		}
		m_activeEffects.Add(effect);
		m_activeEffectData.Add(runtimeGameActorEffectData);
		effect.OnEffectApplied(this, m_activeEffectData[m_activeEffectData.Count - 1], num);
	}

	public GameActorEffect GetEffect(string effectIdentifier)
	{
		for (int i = 0; i < m_activeEffects.Count; i++)
		{
			if (m_activeEffects[i].effectIdentifier == effectIdentifier)
			{
				return m_activeEffects[i];
			}
		}
		return null;
	}

	public GameActorEffect GetEffect(EffectResistanceType resistanceType)
	{
		for (int i = 0; i < m_activeEffects.Count; i++)
		{
			if (m_activeEffects[i].resistanceType == resistanceType)
			{
				return m_activeEffects[i];
			}
		}
		return null;
	}

	public void RemoveEffect(string effectIdentifier)
	{
		for (int num = m_activeEffects.Count - 1; num >= 0; num--)
		{
			if (m_activeEffects[num].effectIdentifier == effectIdentifier)
			{
				RemoveEffect(num);
			}
		}
	}

	public void RemoveEffect(GameActorEffect effect)
	{
		for (int i = 0; i < m_activeEffects.Count; i++)
		{
			if (m_activeEffects[i].effectIdentifier == effect.effectIdentifier)
			{
				RemoveEffect(i);
				break;
			}
		}
	}

	public void RemoveAllEffects(bool ignoreDeathCheck = false)
	{
		for (int num = m_activeEffects.Count - 1; num >= 0; num--)
		{
			RemoveEffect(num, ignoreDeathCheck);
		}
	}

	private void RemoveEffect(int index, bool ignoreDeathCheck = false)
	{
		if (ignoreDeathCheck || !base.healthHaver || !base.healthHaver.IsDead)
		{
			GameActorEffect gameActorEffect = m_activeEffects[index];
			gameActorEffect.OnEffectRemoved(this, m_activeEffectData[index]);
			if (gameActorEffect.AppliesTint)
			{
				DeregisterOverrideColor(gameActorEffect.effectIdentifier);
			}
			if (gameActorEffect.AppliesOutlineTint && this is AIActor)
			{
				(this as AIActor).ClearOverrideOutlineColor();
			}
			m_activeEffects.RemoveAt(index);
			if ((bool)m_activeEffectData[index].instanceOverheadVFX)
			{
				UnityEngine.Object.Destroy(m_activeEffectData[index].instanceOverheadVFX.gameObject);
			}
			m_activeEffectData.RemoveAt(index);
		}
	}

	protected Vector2 ApplyMovementModifiers(Vector2 voluntaryVel, Vector2 involuntaryVel)
	{
		if (this.MovementModifiers != null)
		{
			this.MovementModifiers(ref voluntaryVel, ref involuntaryVel);
		}
		return voluntaryVel + involuntaryVel;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void SetResistance(EffectResistanceType resistType, float resistAmount)
	{
		bool flag = false;
		for (int i = 0; i < base.aiActor.EffectResistances.Length; i++)
		{
			if (base.aiActor.EffectResistances[i].resistType == resistType)
			{
				base.aiActor.EffectResistances[i].resistAmount = resistAmount;
				flag = true;
			}
		}
		if (!flag)
		{
			ActorEffectResistance actorEffectResistance = default(ActorEffectResistance);
			actorEffectResistance.resistType = resistType;
			actorEffectResistance.resistAmount = resistAmount;
			ActorEffectResistance newElement = actorEffectResistance;
			base.aiActor.EffectResistances = BraveUtility.AppendArray(base.aiActor.EffectResistances, newElement);
		}
	}

	protected void HandleGoopChecks()
	{
		m_currentGoop = null;
		m_currentGoopFrozen = false;
		if (GameManager.Instance.Dungeon == null)
		{
			return;
		}
		RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.specRigidbody.UnitCenter.ToIntVector2());
		List<DeadlyDeadlyGoopManager> roomGoops = absoluteRoomFromPosition.RoomGoops;
		if (roomGoops == null)
		{
			return;
		}
		for (int i = 0; i < roomGoops.Count; i++)
		{
			if (roomGoops[i].ProcessGameActor(this))
			{
				m_currentGoop = roomGoops[i].goopDefinition;
				m_currentGoopFrozen = roomGoops[i].IsPositionFrozen(base.specRigidbody.UnitCenter);
			}
		}
	}

	public void ForceFall()
	{
		Fall();
	}

	protected virtual void Fall()
	{
		if (base.healthHaver != null)
		{
			base.healthHaver.EndFlashEffects();
		}
		m_isFalling = true;
		if (HasShadow && (bool)ShadowObject)
		{
			ShadowObject.GetComponent<Renderer>().enabled = false;
		}
	}

	public virtual void RecoverFromFall()
	{
		m_isFalling = false;
		if (HasShadow && (bool)ShadowObject)
		{
			ShadowObject.GetComponent<Renderer>().enabled = true;
		}
	}

	protected virtual bool QueryGroundedFrame()
	{
		if (!base.spriteAnimator)
		{
			return true;
		}
		return base.spriteAnimator.QueryGroundedFrame();
	}

	protected void HandlePitChecks()
	{
		if (GameManager.Instance.Dungeon == null || GameManager.Instance.Dungeon.data == null)
		{
			return;
		}
		bool flag = QueryGroundedFrame() && !IsFlying && !FallingProhibited;
		PlayerController playerController = this as PlayerController;
		if ((bool)playerController && playerController.CurrentRoom != null && playerController.CurrentRoom.RoomFallValidForMaintenance())
		{
			flag = true;
		}
		if (m_isFalling)
		{
			return;
		}
		Rect source = default(Rect);
		source.min = PhysicsEngine.PixelToUnitMidpoint(base.specRigidbody.PrimaryPixelCollider.LowerLeft);
		source.max = PhysicsEngine.PixelToUnitMidpoint(base.specRigidbody.PrimaryPixelCollider.UpperRight);
		Rect rect = new Rect(source);
		ModifyPitVectors(ref rect);
		Dungeon dungeon = GameManager.Instance.Dungeon;
		bool flag2 = dungeon.ShouldReallyFall(rect.min);
		bool flag3 = dungeon.ShouldReallyFall(new Vector3(rect.xMax, rect.yMin));
		bool flag4 = dungeon.ShouldReallyFall(new Vector3(rect.xMin, rect.yMax));
		bool flag5 = dungeon.ShouldReallyFall(rect.max);
		bool flag6 = dungeon.ShouldReallyFall(rect.center);
		IsOverPitAtAll = flag2 || flag3 || flag4 || flag5 || flag6;
		if (IsOverPitAtAll)
		{
			flag2 |= dungeon.data.isWall((int)rect.xMin, (int)rect.yMin);
			flag3 |= dungeon.data.isWall((int)rect.xMax, (int)rect.yMin);
			flag4 |= dungeon.data.isWall((int)rect.xMin, (int)rect.yMax);
			flag5 |= dungeon.data.isWall((int)rect.xMax, (int)rect.yMax);
			flag6 |= dungeon.data.isWall((int)rect.center.x, (int)rect.center.y);
			bool flag7 = flag2 && flag3 && flag4 && flag5 && flag6;
			bool flag8 = OnAboutToFall == null || OnAboutToFall(!flag7);
			if (flag7 && flag && flag8)
			{
				Fall();
				return;
			}
		}
		bool flag9 = true;
		for (int i = 0; i < SupportingPlatforms.Count; i++)
		{
			if (!SupportingPlatforms[i].StaticForPitfall)
			{
				flag9 = false;
				break;
			}
		}
		if (!flag9)
		{
			return;
		}
		if (SupportingPlatforms.Count > 0)
		{
			m_cachedPosition = SupportingPlatforms[0].specRigidbody.UnitCenter;
		}
		else if (Vector3.Distance(m_cachedPosition, base.specRigidbody.Position.GetPixelVector2()) > 3f)
		{
			bool flag10 = dungeon.CellSupportsFalling(source.min) || dungeon.PositionInCustomPitSRB(source.min);
			bool flag11 = dungeon.CellSupportsFalling(new Vector3(source.xMax, source.yMin)) || dungeon.PositionInCustomPitSRB(new Vector3(source.xMax, source.yMin));
			bool flag12 = dungeon.CellSupportsFalling(new Vector3(source.xMin, source.yMax)) || dungeon.PositionInCustomPitSRB(new Vector3(source.xMin, source.yMax));
			bool flag13 = dungeon.CellSupportsFalling(source.max) || dungeon.PositionInCustomPitSRB(source.max);
			bool flag14 = dungeon.CellSupportsFalling(source.center) || dungeon.PositionInCustomPitSRB(source.center);
			IntVector2 intVector = source.min.ToIntVector2(VectorConversions.Floor);
			bool flag15 = dungeon.data.CheckInBoundsAndValid(intVector) && dungeon.data[intVector].type == CellType.FLOOR;
			if (!flag10 && !flag11 && !flag12 && !flag13 && !flag14 && flag15)
			{
				m_cachedPosition = base.specRigidbody.Position.GetPixelVector2();
			}
		}
		else
		{
			bool flag16 = dungeon.CellIsNearPit(source.min) || dungeon.PositionInCustomPitSRB(source.min);
			bool flag17 = dungeon.CellIsNearPit(new Vector3(source.xMax, source.yMin)) || dungeon.PositionInCustomPitSRB(new Vector3(source.xMax, source.yMin));
			bool flag18 = dungeon.CellIsNearPit(new Vector3(source.xMin, source.yMax)) || dungeon.PositionInCustomPitSRB(new Vector3(source.xMin, source.yMax));
			bool flag19 = dungeon.CellIsNearPit(source.max) || dungeon.PositionInCustomPitSRB(source.max);
			bool flag20 = dungeon.CellIsNearPit(source.center) || dungeon.PositionInCustomPitSRB(source.center);
			IntVector2 intVector2 = source.min.ToIntVector2(VectorConversions.Floor);
			bool flag21 = dungeon.data.CheckInBoundsAndValid(intVector2) && dungeon.data[intVector2].type == CellType.FLOOR;
			if (!flag16 && !flag17 && !flag18 && !flag19 && !flag20 && flag21)
			{
				m_cachedPosition = base.specRigidbody.Position.GetPixelVector2();
			}
		}
	}

	public void PlaySmallExplosionsStyleEffect(GameObject vfxPrefab, int count, float midDelay)
	{
		if ((bool)base.sprite)
		{
			StartCoroutine(HandleSmallExplosionsStyleEffect(vfxPrefab, count, midDelay));
		}
	}

	private IEnumerator HandleSmallExplosionsStyleEffect(GameObject vfxPrefab, int explosionCount, float explosionMidDelay)
	{
		for (int i = 0; i < explosionCount; i++)
		{
			if (!base.sprite)
			{
				break;
			}
			Vector2 minPos = base.sprite.WorldBottomLeft;
			Vector2 maxPos = base.sprite.WorldTopRight;
			Vector2 pos = BraveUtility.RandomVector2(minPos, maxPos, new Vector2((maxPos.x - minPos.x) * 0.1f, (maxPos.y - minPos.y) * 0.1f));
			SpawnManager.SpawnVFX(vfxPrefab, pos, Quaternion.identity);
			yield return new WaitForSeconds(explosionMidDelay);
		}
	}

	protected virtual void ModifyPitVectors(ref Rect rect)
	{
	}

	public GameObject PlayEffectOnActor(GameObject effect, Vector3 offset, bool attached = true, bool alreadyMiddleCenter = false, bool useHitbox = false)
	{
		GameObject gameObject = SpawnManager.SpawnVFX(effect);
		tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
		Vector3 vector = ((!useHitbox || !base.specRigidbody || base.specRigidbody.HitboxPixelCollider == null) ? base.sprite.WorldCenter.ToVector3ZUp() : base.specRigidbody.HitboxPixelCollider.UnitCenter.ToVector3ZUp());
		if (!alreadyMiddleCenter)
		{
			component.PlaceAtPositionByAnchor(vector + offset, tk2dBaseSprite.Anchor.MiddleCenter);
		}
		else
		{
			component.transform.position = vector + offset;
		}
		if (attached)
		{
			gameObject.transform.parent = base.transform;
			component.HeightOffGround = 0.2f;
			base.sprite.AttachRenderer(component);
			if (this is PlayerController)
			{
				SmartOverheadVFXController component2 = gameObject.GetComponent<SmartOverheadVFXController>();
				if (component2 != null)
				{
					component2.Initialize(this as PlayerController, offset);
				}
			}
		}
		if (!alreadyMiddleCenter)
		{
			gameObject.transform.localPosition = gameObject.transform.localPosition.QuantizeFloor(0.0625f);
		}
		return gameObject;
	}

	public GameObject PlayFairyEffectOnActor(GameObject effect, Vector3 offset, float duration, bool alreadyMiddleCenter = false)
	{
		if (base.sprite.FlipX)
		{
			offset += new Vector3(base.sprite.GetBounds().extents.x * 2f, 0f, 0f);
		}
		GameObject gameObject = SpawnManager.SpawnVFX(effect, true);
		tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
		gameObject.transform.parent = base.transform;
		component.HeightOffGround = 0.2f;
		base.sprite.AttachRenderer(component);
		StartCoroutine(HandleFairyFlyEffect(component, offset, duration, alreadyMiddleCenter));
		return gameObject;
	}

	protected IEnumerator HandleFairyFlyEffect(tk2dBaseSprite instantiated, Vector3 offset, float duration, bool alreadyMiddleCenter)
	{
		float ela = 0f;
		float centerX = base.sprite.WorldTopCenter.x - base.sprite.transform.position.x;
		float startY = base.sprite.WorldTopCenter.y - base.sprite.transform.position.y;
		float currentRotationAngle2 = 0f;
		float radius = 1f;
		Transform target = instantiated.transform;
		while (ela < duration)
		{
			ela += BraveTime.DeltaTime;
			float t = ela / duration;
			currentRotationAngle2 = Mathf.Lerp(0f, 720f, t);
			float curX = radius * Mathf.Sin(currentRotationAngle2 * ((float)Math.PI / 180f));
			float curY = Mathf.Lerp(startY, 0f, t);
			Vector2 constructedPos = new Vector2(centerX + curX, curY);
			target.localPosition = constructedPos.ToVector3ZUp() + offset;
			if (currentRotationAngle2 % 360f > 90f && currentRotationAngle2 % 360f < 270f)
			{
				instantiated.HeightOffGround = -0.2f;
			}
			else
			{
				instantiated.HeightOffGround = 0.2f;
			}
			instantiated.UpdateZDepth();
			yield return null;
		}
		SpawnManager.Despawn(instantiated.gameObject);
	}

	public bool HasSourcedOverrideColor(string source)
	{
		return m_overrideColorSources.Contains(source);
	}

	public bool HasOverrideColor()
	{
		if (m_overrideColorSources.Count == 0)
		{
			return false;
		}
		if (m_overrideColorSources.Count == 1 && m_overrideColorSources[0] == "base")
		{
			return false;
		}
		return true;
	}

	public void RegisterOverrideColor(Color overrideColor, string source)
	{
		int num = m_overrideColorSources.IndexOf(source);
		if (num >= 0)
		{
			m_overrideColorStack[num] = overrideColor;
		}
		else
		{
			m_overrideColorSources.Add(source);
			m_overrideColorStack.Add(overrideColor);
		}
		OnOverrideColorsChanged();
	}

	public void DeregisterOverrideColor(string source)
	{
		int num = m_overrideColorSources.IndexOf(source);
		if (num >= 0)
		{
			m_overrideColorStack.RemoveAt(num);
			m_overrideColorSources.RemoveAt(num);
		}
		OnOverrideColorsChanged();
	}

	public void OnOverrideColorsChanged()
	{
		if (OverrideColorOverridden)
		{
			return;
		}
		for (int i = 0; i < base.healthHaver.bodySprites.Count; i++)
		{
			if ((bool)base.healthHaver.bodySprites[i])
			{
				base.healthHaver.bodySprites[i].usesOverrideMaterial = true;
				base.healthHaver.bodySprites[i].renderer.material.SetColor(m_overrideColorID, CurrentOverrideColor);
			}
		}
		if ((bool)base.renderer && (bool)base.renderer.material)
		{
			m_colorOverridenMaterial = base.renderer.material;
			m_colorOverridenShader = m_colorOverridenMaterial.shader;
		}
	}

	public void ToggleShadowVisiblity(bool value)
	{
		if ((bool)ShadowObject)
		{
			ShadowObject.GetComponent<Renderer>().enabled = value;
		}
	}

	protected GameObject GenerateDefaultBlobShadow(float heightOffset = 0f)
	{
		if ((bool)ShadowObject)
		{
			BraveUtility.Log("We are trying to generate a GameActor shadow when we already have one!", Color.red, BraveUtility.LogVerbosity.IMPORTANT);
			return ShadowObject;
		}
		Transform transform = base.transform;
		SpeculativeRigidbody componentInChildren = base.gameObject.GetComponentInChildren<SpeculativeRigidbody>();
		if ((bool)componentInChildren)
		{
			componentInChildren.Reinitialize();
		}
		if ((bool)ShadowPrefab)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(ShadowPrefab);
			gameObject.transform.parent = transform;
			if ((bool)base.specRigidbody)
			{
				gameObject.transform.localPosition = base.specRigidbody.UnitCenter.ToVector3ZUp() - base.transform.position.WithZ(0f);
			}
			else
			{
				gameObject.transform.localPosition = Vector3.zero;
			}
			DepthLookupManager.ProcessRenderer(gameObject.GetComponent<Renderer>(), DepthLookupManager.GungeonSortingLayer.BACKGROUND);
			if (base.aiActor != null && base.aiActor.ActorName == "Gatling Gull")
			{
				tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
				component.HeightOffGround = 6f;
			}
			else if (ShadowHeightOffGround != 0f)
			{
				tk2dBaseSprite component2 = gameObject.GetComponent<tk2dBaseSprite>();
				component2.HeightOffGround = ShadowHeightOffGround;
			}
			ShadowObject = gameObject;
			gameObject.transform.position = gameObject.transform.position.Quantize(0.0625f);
			return gameObject;
		}
		if (transform.Find("PlayerSprite") != null)
		{
			if (transform.Find("PlayerShadow") != null)
			{
				return transform.Find("PlayerShadow").gameObject;
			}
			PlayerController component3 = transform.GetComponent<PlayerController>();
			GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("DefaultShadowSprite"));
			gameObject2.transform.parent = transform;
			gameObject2.transform.localPosition = Vector3.zero;
			gameObject2.transform.localPosition = new Vector3(component3.SpriteBottomCenter.x - transform.position.x, 0f, 0.1f);
			gameObject2.GetComponent<tk2dSprite>().HeightOffGround = -0.1f;
			DepthLookupManager.ProcessRenderer(gameObject2.GetComponent<Renderer>(), DepthLookupManager.GungeonSortingLayer.PLAYFIELD);
			ShadowObject = gameObject2;
			gameObject2.transform.position = gameObject2.transform.position.Quantize(0.0625f);
			return gameObject2;
		}
		if (componentInChildren != null)
		{
			GameObject gameObject3 = (GameObject)UnityEngine.Object.Instantiate(ResourceCache.Acquire("DefaultShadowSprite"));
			gameObject3.transform.parent = transform;
			float y = componentInChildren.UnitBottomLeft.y - transform.position.y + heightOffset;
			Vector3 localPosition = new Vector3(componentInChildren.UnitCenter.x - transform.position.x, y, 0.1f);
			gameObject3.transform.localPosition = localPosition;
			gameObject3.GetComponent<tk2dSprite>().HeightOffGround = (0f - heightOffset) * 2f + ShadowHeightOffGround;
			DepthLookupManager.ProcessRenderer(gameObject3.GetComponent<Renderer>(), DepthLookupManager.GungeonSortingLayer.PLAYFIELD);
			ShadowObject = gameObject3;
			gameObject3.transform.position = gameObject3.transform.position.Quantize(0.0625f);
			ShadowObject.transform.localPosition += ActorShadowOffset;
			return gameObject3;
		}
		return null;
	}
}
