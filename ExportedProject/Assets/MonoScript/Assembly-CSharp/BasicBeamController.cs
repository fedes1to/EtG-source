using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dungeonator;
using UnityEngine;

public class BasicBeamController : BeamController
{
	[Serializable]
	public class AngularKnockbackTier
	{
		public float minAngularSpeed;

		public float damageMultiplier;

		public float knockbackMultiplier;

		public float ignoreHitRigidbodyTime;

		public VFXPool hitRigidbodyVFX;

		public int additionalAmmoCost;
	}

	public enum BeamState
	{
		Charging,
		Telegraphing,
		Firing,
		Dissipating,
		Disconnected
	}

	public enum BeamBoneType
	{
		Straight = 0,
		Projectile = 2
	}

	public enum BeamTileType
	{
		GrowAtEnd,
		GrowAtBeginning,
		Flowing
	}

	public enum BeamEndType
	{
		Vanish,
		Persist,
		Dissipate
	}

	public enum BeamCollisionType
	{
		Default,
		Rectangle
	}

	public class BeamBone
	{
		public float PosX;

		public float RotationAngle;

		public Vector2 Position;

		public Vector2 Velocity;

		public int SubtileNum;

		public float HomingRadius;

		public float HomingAngularVelocity;

		public AIActor HomingTarget;

		public bool HomingDampenMotion;

		public BeamBone(float posX, float rotationAngle, int subtileNum)
		{
			PosX = posX;
			RotationAngle = rotationAngle;
			SubtileNum = subtileNum;
		}

		public BeamBone(float posX, Vector2 position, Vector2 velocity)
		{
			PosX = posX;
			Position = position;
			Velocity = velocity;
		}

		public BeamBone(BeamBone other)
		{
			PosX = other.PosX;
			RotationAngle = other.RotationAngle;
			Position = other.Position;
			Velocity = other.Velocity;
			SubtileNum = other.SubtileNum;
			HomingRadius = other.HomingRadius;
			HomingAngularVelocity = other.HomingAngularVelocity;
			HomingDampenMotion = other.HomingDampenMotion;
		}

		public void ApplyHoming(SpeculativeRigidbody ignoreRigidbody = null, float overrideDeltaTime = -1f)
		{
			if (HomingRadius == 0f || HomingAngularVelocity == 0f)
			{
				return;
			}
			IntVector2 pos = Position.ToIntVector2(VectorConversions.Floor);
			if (!GameManager.Instance.Dungeon.CellExists(pos))
			{
				return;
			}
			RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(pos);
			List<AIActor> activeEnemies = absoluteRoomFromPosition.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			if (activeEnemies == null || activeEnemies.Count == 0)
			{
				return;
			}
			float num = float.MaxValue;
			Vector2 vector = Vector2.zero;
			AIActor aIActor = null;
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				if ((bool)activeEnemies[i] && !activeEnemies[i].healthHaver.IsDead && (!ignoreRigidbody || !(activeEnemies[i].specRigidbody == ignoreRigidbody)))
				{
					Vector2 vector2 = activeEnemies[i].CenterPosition - Position;
					float magnitude = vector2.magnitude;
					if (magnitude < num - 0.5f)
					{
						vector = vector2;
						num = magnitude;
						aIActor = activeEnemies[i];
					}
				}
			}
			if (num < HomingRadius && aIActor != null)
			{
				float num2 = 1f - num / HomingRadius;
				float current = Velocity.ToAngle();
				float target = vector.ToAngle();
				float maxDelta = HomingAngularVelocity * num2 * ((!(overrideDeltaTime >= 0f)) ? BraveTime.DeltaTime : overrideDeltaTime);
				float angle = Mathf.MoveTowardsAngle(current, target, maxDelta);
				Velocity = BraveMathCollege.DegreesToVector(angle, Velocity.magnitude);
				if (aIActor != HomingTarget)
				{
					HomingDampenMotion = true;
				}
				HomingTarget = aIActor;
			}
		}
	}

	[Serializable]
	public class TelegraphAnims
	{
		[CheckAnimation(null)]
		public string beamAnimation;

		[CheckAnimation(null)]
		public string beamStartAnimation;

		[CheckAnimation(null)]
		public string beamEndAnimation;
	}

	public bool usesTelegraph;

	[ShowInInspectorIf("usesTelegraph", true)]
	public float telegraphTime;

	[ShowInInspectorIf("usesTelegraph", true)]
	public TelegraphAnims telegraphAnimations;

	[Header("Beam Structure")]
	public BeamBoneType boneType;

	[ShowInInspectorIf("boneType", 1, true)]
	public bool interpolateStretchedBones;

	public int penetration;

	public int reflections;

	public bool PenetratesCover;

	public bool angularKnockback;

	[ShowInInspectorIf("angularKnockback", true)]
	public float angularSpeedAvgWindow = 0.15f;

	public List<AngularKnockbackTier> angularKnockbackTiers;

	public float homingRadius;

	public float homingAngularVelocity;

	public float TimeToStatus = 0.5f;

	[Header("Beam Animations")]
	public BeamTileType TileType;

	[CheckAnimation(null)]
	public string beamAnimation;

	[CheckAnimation(null)]
	public string beamStartAnimation;

	[CheckAnimation(null)]
	public string beamEndAnimation;

	[Header("Beam Overlays")]
	[CheckAnimation(null)]
	public string muzzleAnimation;

	[CheckAnimation(null)]
	public string chargeAnimation;

	[ShowInInspectorIf("chargeAnimation", true)]
	public bool rotateChargeAnimation;

	[CheckAnimation(null)]
	public string impactAnimation;

	[Header("Persistence")]
	public BeamEndType endType;

	[ShowInInspectorIf("endType", 1, true)]
	public float decayNear;

	[ShowInInspectorIf("endType", 1, true)]
	public float decayFar;

	[ShowInInspectorIf("endType", 1, true)]
	public bool collisionSeparation;

	[ShowInInspectorIf("endType", 1, true)]
	public float breakAimAngle;

	[ShowInInspectorIf("endType", 2, true)]
	public float dissipateTime;

	[ShowInInspectorIf("endType", 2, true)]
	public TelegraphAnims dissipateAnimations;

	[Header("Collision")]
	public BeamCollisionType collisionType;

	[ShowInInspectorIf("collisionType", 0, true)]
	public float collisionRadius = 1.5f;

	[ShowInInspectorIf("collisionType", 1, true)]
	public int collisionLength = 320;

	[ShowInInspectorIf("collisionType", 1, true)]
	public int collisionWidth = 64;

	[Header("Particles")]
	public bool UsesDispersalParticles;

	[ShowInInspectorIf("UsesDispersalParticles", true)]
	public float DispersalDensity = 3f;

	[ShowInInspectorIf("UsesDispersalParticles", true)]
	public float DispersalMinCoherency = 0.2f;

	[ShowInInspectorIf("UsesDispersalParticles", true)]
	public float DispersalMaxCoherency = 1f;

	[ShowInInspectorIf("UsesDispersalParticles", true)]
	public GameObject DispersalParticleSystemPrefab;

	[ShowInInspectorIf("UsesDispersalParticles", true)]
	public float DispersalExtraImpactFactor = 1f;

	[Header("Nonsense")]
	public bool doesScreenDistortion;

	[ShowInInspectorIf("doesScreenDistortion", true)]
	public float startDistortionRadius = 0.3f;

	[ShowInInspectorIf("doesScreenDistortion", true)]
	public float endDistortionRadius = 0.2f;

	[ShowInInspectorIf("doesScreenDistortion", true)]
	public float startDistortionPower = 0.7f;

	[ShowInInspectorIf("doesScreenDistortion", true)]
	public float endDistortionPower = 0.5f;

	[ShowInInspectorIf("doesScreenDistortion", true)]
	public float distortionPulseSpeed = 25f;

	[ShowInInspectorIf("doesScreenDistortion", true)]
	public float minDistortionOffset;

	[ShowInInspectorIf("doesScreenDistortion", true)]
	public float distortionOffsetIncrease = 0.02f;

	public int overrideBeamQuadPixelWidth = -1;

	public bool FlipBeamSpriteLocal;

	[Header("Audio Flags")]
	public string startAudioEvent;

	public string endAudioEvent;

	[NonSerialized]
	private Material m_distortionMaterial;

	[NonSerialized]
	public Action<SpeculativeRigidbody, Vector2> OverrideHitChecks;

	[NonSerialized]
	public bool SkipPostProcessing;

	public float IgnoreTilesDistance = -1f;

	private bool m_hasToggledGunOutline;

	private int exceptionTracker;

	private static List<Vector2> s_goopPoints = new List<Vector2>();

	private Vector2? m_lastBeamEnd;

	private int m_currentTintPriority = -1;

	private Vector2 m_cachedRectangleOrigin;

	private Vector2 m_cachedRectangleDirection;

	private tk2dTiledSprite m_beamSprite;

	private Transform m_muzzleTransform;

	private tk2dSprite m_beamMuzzleSprite;

	private tk2dSpriteAnimator m_beamMuzzleAnimator;

	private Transform m_impactTransform;

	private tk2dSprite m_impactSprite;

	private tk2dSpriteAnimator m_impactAnimator;

	private Transform m_impact2Transform;

	private tk2dSprite m_impact2Sprite;

	private tk2dSpriteAnimator m_impact2Animator;

	private GoopModifier m_beamGoopModifier;

	private List<tk2dBaseSprite> m_pierceImpactSprites;

	private float m_chargeTimer;

	private float m_telegraphTimer;

	private float m_dissipateTimer;

	private KnockbackDoer m_enemyKnockback;

	private int m_enemyKnockbackId;

	private Vector3 m_beamSpriteDimensions;

	private int m_beamSpriteSubtileWidth;

	private float m_beamSpriteUnitWidth;

	private float m_uvOffset;

	private float? m_previousAngle;

	private float m_currentBeamDistance;

	private LinkedList<BeamBone> m_bones;

	private BasicBeamController m_reflectedBeam;

	private Vector2 m_lastHitNormal;

	private int m_beamQuadPixelWidth;

	private float m_beamQuadUnitWidth;

	private float m_sqrNewBoneThreshold;

	private float m_projectileScale = 1f;

	private float m_currentLuteScaleModifier = 1f;

	private float averageAngularVelocity;

	private ParticleSystem m_dispersalParticles;

	private const float c_minGoopDistance = 1.75f;

	private static float CurrentBeamHeightOffGround = 0.75f;

	private const float c_defaultBeamHeightOffGround = 0.75f;

	private const int c_defaultBeamQuadPixelWidth = 4;

	private static readonly List<IntVector2> s_pixelCloud = new List<IntVector2>();

	private static readonly List<IntVector2> s_lastPixelCloud = new List<IntVector2>();

	public bool playerStatsModified { get; set; }

	public bool SelfUpdate { get; set; }

	public BeamState State { get; set; }

	public float HeightOffset { get; set; }

	public float RampHeightOffset { get; set; }

	public bool ContinueBeamArtToWall { get; set; }

	public float BoneSpeed
	{
		get
		{
			if (State == BeamState.Telegraphing)
			{
				return -1f;
			}
			return base.projectile.baseData.speed;
		}
	}

	public override bool ShouldUseAmmo
	{
		get
		{
			return State == BeamState.Firing;
		}
	}

	public string CurrentBeamAnimation
	{
		get
		{
			if (State == BeamState.Telegraphing)
			{
				return telegraphAnimations.beamAnimation;
			}
			if (State == BeamState.Dissipating)
			{
				return dissipateAnimations.beamAnimation;
			}
			return beamAnimation;
		}
	}

	public string CurrentBeamStartAnimation
	{
		get
		{
			if (State == BeamState.Telegraphing)
			{
				return telegraphAnimations.beamStartAnimation;
			}
			if (State == BeamState.Dissipating)
			{
				return dissipateAnimations.beamStartAnimation;
			}
			return beamStartAnimation;
		}
	}

	public string CurrentBeamEndAnimation
	{
		get
		{
			if (State == BeamState.Telegraphing)
			{
				return telegraphAnimations.beamEndAnimation;
			}
			if (State == BeamState.Dissipating)
			{
				return dissipateAnimations.beamEndAnimation;
			}
			return beamEndAnimation;
		}
	}

	public bool UsesChargeSprite
	{
		get
		{
			return !string.IsNullOrEmpty(chargeAnimation);
		}
	}

	public bool UsesMuzzleSprite
	{
		get
		{
			return !string.IsNullOrEmpty(muzzleAnimation);
		}
	}

	public bool UsesImpactSprite
	{
		get
		{
			return !string.IsNullOrEmpty(impactAnimation);
		}
	}

	public bool UsesBeamStartAnimation
	{
		get
		{
			return !string.IsNullOrEmpty(CurrentBeamStartAnimation);
		}
	}

	public bool UsesBeamEndAnimation
	{
		get
		{
			return !string.IsNullOrEmpty(CurrentBeamEndAnimation);
		}
	}

	public bool UsesBones
	{
		get
		{
			return boneType == BeamBoneType.Projectile || IsHoming || ProjectileAndBeamMotionModule != null;
		}
	}

	public bool IsConnected
	{
		get
		{
			return State != BeamState.Disconnected;
		}
	}

	public float HomingRadius
	{
		get
		{
			return base.ChanceBasedHomingRadius + homingRadius;
		}
	}

	public float HomingAngularVelocity
	{
		get
		{
			float num = base.ChanceBasedHomingAngularVelocity + homingAngularVelocity;
			if (BoneSpeed < 0f)
			{
				return num;
			}
			return num * (BoneSpeed / 40f);
		}
	}

	public bool IsHoming
	{
		get
		{
			return HomingRadius > 0f && HomingAngularVelocity > 0f;
		}
	}

	public SpeculativeRigidbody ReflectedFromRigidbody { get; set; }

	public bool ShowImpactOnMaxDistanceEnd { get; set; }

	public bool IsBlackBullet { get; set; }

	public ProjectileAndBeamMotionModule ProjectileAndBeamMotionModule { get; set; }

	public float ProjectileScale
	{
		get
		{
			return m_projectileScale;
		}
		set
		{
			m_projectileScale = value;
		}
	}

	public float ApproximateDistance
	{
		get
		{
			return m_currentBeamDistance;
		}
	}

	public void Start()
	{
		if (UsesDispersalParticles && m_dispersalParticles == null)
		{
			m_dispersalParticles = GlobalDispersalParticleManager.GetSystemForPrefab(DispersalParticleSystemPrefab);
		}
		m_beamQuadPixelWidth = ((overrideBeamQuadPixelWidth <= 0) ? 4 : overrideBeamQuadPixelWidth);
		m_beamQuadUnitWidth = (float)m_beamQuadPixelWidth / 16f;
		m_sqrNewBoneThreshold = m_beamQuadUnitWidth * m_beamQuadUnitWidth * 2f * 2f;
		m_beamSprite = base.gameObject.GetComponent<tk2dTiledSprite>();
		m_beamSprite.renderer.sortingLayerName = "Player";
		m_beamSpriteDimensions = m_beamSprite.GetUntrimmedBounds().size;
		m_beamSprite.dimensions = new Vector2(0f, m_beamSpriteDimensions.y * 16f);
		base.spriteAnimator.Play(beamAnimation);
		m_beamSprite.HeightOffGround = CurrentBeamHeightOffGround + HeightOffset;
		m_beamSprite.IsPerpendicular = false;
		m_beamSprite.usesOverrideMaterial = true;
		PlayerController playerController = base.projectile.Owner as PlayerController;
		if ((bool)playerController)
		{
			m_projectileScale = playerController.BulletScaleModifier;
		}
		if (IsConnected)
		{
			m_muzzleTransform = base.transform.Find("beam muzzle flare");
			if ((bool)m_muzzleTransform)
			{
				m_beamMuzzleSprite = m_muzzleTransform.GetComponent<tk2dSprite>();
				m_beamMuzzleAnimator = m_muzzleTransform.GetComponent<tk2dSpriteAnimator>();
			}
			if (UsesChargeSprite || UsesMuzzleSprite)
			{
				if (!m_muzzleTransform)
				{
					GameObject gameObject = new GameObject("beam muzzle flare");
					m_muzzleTransform = gameObject.transform;
					m_muzzleTransform.parent = base.transform;
					m_muzzleTransform.localPosition = new Vector3(0f, 0f, 0.05f);
					m_beamMuzzleSprite = gameObject.AddComponent<tk2dSprite>();
					m_beamMuzzleSprite.SetSprite(m_beamSprite.Collection, m_beamSprite.spriteId);
					m_beamMuzzleAnimator = gameObject.AddComponent<tk2dSpriteAnimator>();
					m_beamMuzzleAnimator.SetSprite(m_beamSprite.Collection, m_beamSprite.spriteId);
					m_beamMuzzleAnimator.Library = base.spriteAnimator.Library;
					m_beamSprite.AttachRenderer(m_beamMuzzleSprite);
					m_beamMuzzleSprite.HeightOffGround = 0.05f;
					m_beamMuzzleSprite.IsPerpendicular = false;
					m_beamMuzzleSprite.usesOverrideMaterial = true;
				}
				m_muzzleTransform.localScale = new Vector3(m_projectileScale, m_projectileScale, 1f);
			}
			if ((bool)m_muzzleTransform)
			{
				m_muzzleTransform.gameObject.SetActive(false);
			}
			if (usesChargeDelay)
			{
				base.renderer.enabled = false;
				State = BeamState.Charging;
				if (UsesChargeSprite)
				{
					m_beamMuzzleAnimator.Play(chargeAnimation);
					m_muzzleTransform.gameObject.SetActive(true);
				}
			}
			else if (usesTelegraph)
			{
				State = BeamState.Telegraphing;
				base.spriteAnimator.Play(CurrentBeamAnimation);
			}
			else
			{
				State = BeamState.Firing;
				if (UsesMuzzleSprite)
				{
					m_beamMuzzleAnimator.Play(muzzleAnimation);
					m_muzzleTransform.gameObject.SetActive(true);
				}
			}
			AIActor aIActor = base.Owner as AIActor;
			if ((bool)aIActor && aIActor.IsBlackPhantom)
			{
				BecomeBlackBullet();
			}
			if (GameManager.AUDIO_ENABLED && !string.IsNullOrEmpty(startAudioEvent))
			{
				AkSoundEngine.PostEvent(startAudioEvent, base.gameObject);
			}
		}
		else
		{
			m_muzzleTransform = base.transform.Find("beam muzzle flare");
			if ((bool)m_muzzleTransform)
			{
				m_muzzleTransform.gameObject.SetActive(false);
			}
		}
		if (UsesImpactSprite)
		{
			m_impactTransform = base.transform.Find("beam impact vfx");
			if ((bool)m_impactTransform)
			{
				m_impactSprite = m_impactTransform.GetComponent<tk2dSprite>();
				m_impactAnimator = m_impactTransform.GetComponent<tk2dSpriteAnimator>();
			}
			else
			{
				GameObject gameObject2 = new GameObject("beam impact vfx");
				m_impactTransform = gameObject2.transform;
				m_impactTransform.parent = base.transform;
				m_impactTransform.localPosition = new Vector3(0f, 0f, 0.05f);
				m_impactSprite = gameObject2.AddComponent<tk2dSprite>();
				m_impactSprite.SetSprite(m_beamSprite.Collection, m_beamSprite.spriteId);
				m_impactAnimator = gameObject2.AddComponent<tk2dSpriteAnimator>();
				m_impactAnimator.SetSprite(m_beamSprite.Collection, m_beamSprite.spriteId);
				m_impactAnimator.Library = base.spriteAnimator.Library;
				m_beamSprite.AttachRenderer(m_impactSprite);
				m_impactSprite.HeightOffGround = 0.05f;
				m_impactSprite.IsPerpendicular = true;
				m_impactSprite.usesOverrideMaterial = true;
			}
			m_impactTransform.localScale = new Vector3(m_projectileScale, m_projectileScale, 1f);
			m_impact2Transform = base.transform.Find("beam impact vfx 2");
			if ((bool)m_impact2Transform)
			{
				m_impact2Sprite = m_impact2Transform.GetComponent<tk2dSprite>();
				m_impact2Animator = m_impact2Transform.GetComponent<tk2dSpriteAnimator>();
			}
			else
			{
				GameObject gameObject3 = new GameObject("beam impact vfx 2");
				m_impact2Transform = gameObject3.transform;
				m_impact2Transform.parent = base.transform;
				m_impact2Transform.localPosition = new Vector3(0f, 0f, 0.05f);
				m_impact2Sprite = gameObject3.AddComponent<tk2dSprite>();
				m_impact2Sprite.SetSprite(m_beamSprite.Collection, m_beamSprite.spriteId);
				m_impact2Animator = gameObject3.AddComponent<tk2dSpriteAnimator>();
				m_impact2Animator.SetSprite(m_beamSprite.Collection, m_beamSprite.spriteId);
				m_impact2Animator.Library = base.spriteAnimator.Library;
				m_beamSprite.AttachRenderer(m_impact2Sprite);
				m_impact2Sprite.HeightOffGround = 0.05f;
				m_impact2Sprite.IsPerpendicular = true;
				m_impact2Sprite.usesOverrideMaterial = true;
			}
			m_impact2Transform.localScale = new Vector3(m_projectileScale, m_projectileScale, 1f);
			if (!m_impactAnimator.IsPlaying(impactAnimation))
			{
				m_impactAnimator.Play(impactAnimation);
			}
			if (!m_impact2Animator.IsPlaying(impactAnimation))
			{
				m_impact2Animator.Play(impactAnimation);
			}
			if ((bool)m_impactTransform)
			{
				m_impactTransform.gameObject.SetActive(false);
			}
			if ((bool)m_impact2Transform)
			{
				m_impact2Transform.gameObject.SetActive(false);
			}
			for (int i = 0; i < base.transform.childCount; i++)
			{
				Transform child = base.transform.GetChild(i);
				if (child.name.StartsWith("beam pierce impact vfx"))
				{
					if (m_pierceImpactSprites == null)
					{
						m_pierceImpactSprites = new List<tk2dBaseSprite>();
					}
					m_pierceImpactSprites.Add(child.GetComponent<tk2dBaseSprite>());
					m_pierceImpactSprites[m_pierceImpactSprites.Count - 1].gameObject.SetActive(false);
				}
			}
		}
		m_beamSprite.OverrideGetTiledSpriteGeomDesc = GetTiledSpriteGeomDesc;
		m_beamSprite.OverrideSetTiledSpriteGeom = SetTiledSpriteGeom;
		tk2dSpriteDefinition currentSpriteDef = m_beamSprite.GetCurrentSpriteDef();
		m_beamSpriteSubtileWidth = Mathf.RoundToInt(currentSpriteDef.untrimmedBoundsDataExtents.x / currentSpriteDef.texelSize.x) / m_beamQuadPixelWidth;
		m_beamSpriteUnitWidth = (float)Mathf.RoundToInt(currentSpriteDef.untrimmedBoundsDataExtents.x / currentSpriteDef.texelSize.x) / 16f;
		if (m_bones == null)
		{
			m_bones = new LinkedList<BeamBone>();
			m_bones.AddFirst(new BeamBone(0f, 0f, m_beamSpriteSubtileWidth - 1));
			m_bones.AddLast(new BeamBone(0f, 0f, -1));
			m_uvOffset = 1f;
			if (boneType == BeamBoneType.Projectile)
			{
				m_bones.First.Value.Position = base.Origin;
				m_bones.First.Value.Velocity = base.Direction.normalized * BoneSpeed;
				m_bones.Last.Value.Position = base.Origin;
				m_bones.Last.Value.Velocity = base.Direction.normalized * BoneSpeed;
			}
		}
		else
		{
			m_beamSprite.ForceBuild();
			m_beamSprite.UpdateZDepth();
		}
		m_beamGoopModifier = base.gameObject.GetComponent<GoopModifier>();
		if (IsConnected && base.Owner is PlayerController && !SkipPostProcessing)
		{
			(base.Owner as PlayerController).DoPostProcessBeam(this);
		}
		if (IsConnected && base.Owner is AIActor && !SkipPostProcessing)
		{
			AIActor aIActor2 = base.Owner as AIActor;
			if ((bool)aIActor2.CompanionOwner)
			{
				aIActor2.CompanionOwner.DoPostProcessBeam(this);
				ProjectileScale *= aIActor2.CompanionOwner.BulletScaleModifier;
			}
		}
		ProjectileAndBeamMotionModule = base.projectile.OverrideMotionModule as ProjectileAndBeamMotionModule;
	}

	public void Update()
	{
		if (State == BeamState.Disconnected || State == BeamState.Dissipating)
		{
			if (boneType != 0 && ((m_bones.Count == 2 && Mathf.Approximately(m_bones.First.Value.PosX, m_bones.Last.Value.PosX)) || m_bones.Count < 2))
			{
				DestroyBeam();
			}
			if (State == BeamState.Dissipating && m_dissipateTimer >= dissipateTime)
			{
				DestroyBeam();
			}
		}
		if (SelfUpdate)
		{
			FrameUpdate();
		}
	}

	protected override void OnDestroy()
	{
		if ((bool)m_enemyKnockback)
		{
			m_enemyKnockback.EndContinuousKnockback(m_enemyKnockbackId);
		}
		m_enemyKnockback = null;
		m_enemyKnockbackId = -1;
		if ((bool)m_reflectedBeam)
		{
			m_reflectedBeam.CeaseAttack();
			m_reflectedBeam = null;
		}
		base.OnDestroy();
	}

	public void ForceChargeTimer(float val)
	{
		m_chargeTimer = val;
	}

	public void FrameUpdate()
	{
		try
		{
			SpeculativeRigidbody[] ignoreRigidbodies = GetIgnoreRigidbodies();
			int num = 0;
			PixelCollider pixelCollider;
			if (State == BeamState.Charging)
			{
				m_chargeTimer += BraveTime.DeltaTime;
				HandleBeamFrame(base.Origin, base.Direction, base.HitsPlayers, base.HitsEnemies, false, out pixelCollider, ignoreRigidbodies);
			}
			else if (State == BeamState.Telegraphing)
			{
				m_telegraphTimer += BraveTime.DeltaTime;
				HandleBeamFrame(base.Origin, base.Direction, base.HitsPlayers, base.HitsEnemies, false, out pixelCollider, ignoreRigidbodies);
			}
			else if (State == BeamState.Dissipating)
			{
				m_dissipateTimer += BraveTime.DeltaTime;
				HandleBeamFrame(base.Origin, base.Direction, base.HitsPlayers, base.HitsEnemies, false, out pixelCollider, ignoreRigidbodies);
			}
			else
			{
				if (State != BeamState.Firing && State != BeamState.Disconnected)
				{
					return;
				}
				List<SpeculativeRigidbody> list = HandleBeamFrame(base.Origin, base.Direction, base.HitsPlayers, base.HitsEnemies, false, out pixelCollider, ignoreRigidbodies);
				if (list != null && list.Count > 0)
				{
					float num2 = base.projectile.baseData.damage + base.DamageModifier;
					PlayerController playerController = base.projectile.Owner as PlayerController;
					if ((bool)playerController)
					{
						num2 *= playerController.stats.GetStatValue(PlayerStats.StatType.RateOfFire);
					}
					if (base.ChanceBasedShadowBullet)
					{
						num2 *= 2f;
					}
					for (int i = 0; i < list.Count; i++)
					{
						SpeculativeRigidbody speculativeRigidbody = list[i];
						if (OverrideHitChecks != null)
						{
							OverrideHitChecks(speculativeRigidbody, base.Direction);
						}
						else
						{
							if (!speculativeRigidbody)
							{
								continue;
							}
							if (base.Owner is AIActor)
							{
								if ((bool)speculativeRigidbody.healthHaver)
								{
									if ((bool)speculativeRigidbody.gameActor && speculativeRigidbody.gameActor is PlayerController)
									{
										bool flag = (bool)base.Owner && (base.Owner as AIActor).IsBlackPhantom;
										bool isAlive = speculativeRigidbody.healthHaver.IsAlive;
										float num3 = ((base.projectile.baseData.damage != 0f) ? 0.5f : 0f);
										speculativeRigidbody.healthHaver.ApplyDamage(num3, base.Direction, (base.Owner as AIActor).GetActorName(), hitPixelCollider: pixelCollider, damageTypes: CoreDamageTypes.None, damageCategory: flag ? DamageCategory.BlackBullet : DamageCategory.Normal);
										bool arg = isAlive && speculativeRigidbody.healthHaver.IsDead;
										if (base.projectile.OnHitEnemy != null)
										{
											base.projectile.OnHitEnemy(base.projectile, speculativeRigidbody, arg);
										}
									}
									else
									{
										num2 = ((!speculativeRigidbody.aiActor) ? base.projectile.baseData.damage : ProjectileData.FixedFallbackDamageToEnemies) + base.DamageModifier;
										bool isAlive2 = speculativeRigidbody.healthHaver.IsAlive;
										HealthHaver obj = speculativeRigidbody.healthHaver;
										float damage2 = num2 * BraveTime.DeltaTime;
										Vector2 direction2 = base.Direction;
										string actorName2 = (base.Owner as AIActor).GetActorName();
										PixelCollider hitPixelCollider2 = pixelCollider;
										obj.ApplyDamage(damage2, direction2, actorName2, CoreDamageTypes.None, DamageCategory.Normal, false, hitPixelCollider2);
										bool arg2 = isAlive2 && speculativeRigidbody.healthHaver.IsDead;
										if (base.projectile.OnHitEnemy != null)
										{
											base.projectile.OnHitEnemy(base.projectile, speculativeRigidbody, arg2);
										}
									}
								}
							}
							else if ((bool)speculativeRigidbody.healthHaver)
							{
								float num4 = num2;
								if (num >= 1)
								{
									int num5 = Mathf.Clamp(num - 1, 0, GameManager.Instance.PierceDamageScaling.Length - 1);
									num4 *= GameManager.Instance.PierceDamageScaling[num5];
								}
								if (speculativeRigidbody.healthHaver.IsBoss && (bool)base.projectile)
								{
									num4 *= base.projectile.BossDamageMultiplier;
								}
								if ((bool)base.projectile && base.projectile.BlackPhantomDamageMultiplier != 1f && (bool)speculativeRigidbody.aiActor && speculativeRigidbody.aiActor.IsBlackPhantom)
								{
									num4 *= base.projectile.BlackPhantomDamageMultiplier;
								}
								bool isAlive3 = speculativeRigidbody.healthHaver.IsAlive;
								string empty = string.Empty;
								empty = ((!base.projectile) ? ((!(base.Owner is AIActor)) ? base.Owner.ActorName : (base.Owner as AIActor).GetActorName()) : base.projectile.OwnerName);
								float num6 = num4 * BraveTime.DeltaTime;
								if (angularKnockback)
								{
									AngularKnockbackTier knockbackTier = GetKnockbackTier();
									if (knockbackTier != null)
									{
										num6 = num2 * knockbackTier.damageMultiplier;
										SpeculativeRigidbody speculativeRigidbody2 = speculativeRigidbody.healthHaver.specRigidbody;
										if (Array.IndexOf(ignoreRigidbodies, speculativeRigidbody2) >= 0)
										{
											num6 = 0f;
										}
										if (num6 > 0f)
										{
											AkSoundEngine.PostEvent("Play_WPN_woodbeam_impact_01", base.gameObject);
											knockbackTier.hitRigidbodyVFX.SpawnAtPosition(speculativeRigidbody.UnitCenter);
											if (knockbackTier.additionalAmmoCost > 0 && (bool)base.Gun)
											{
												base.Gun.LoseAmmo(knockbackTier.additionalAmmoCost);
											}
											if ((bool)speculativeRigidbody2)
											{
												TimedIgnoreRigidbodies.Add(Tuple.Create(speculativeRigidbody2, knockbackTier.ignoreHitRigidbodyTime));
											}
											else
											{
												TimedIgnoreRigidbodies.Add(Tuple.Create(speculativeRigidbody, knockbackTier.ignoreHitRigidbodyTime));
											}
										}
									}
								}
								HealthHaver obj2 = speculativeRigidbody.healthHaver;
								float damage2 = num6;
								Vector2 direction2 = base.Direction;
								string actorName2 = empty;
								CoreDamageTypes damageTypes = base.projectile.damageTypes;
								PixelCollider hitPixelCollider2 = pixelCollider;
								obj2.ApplyDamage(damage2, direction2, actorName2, damageTypes, DamageCategory.Normal, false, hitPixelCollider2);
								bool arg3 = isAlive3 && speculativeRigidbody.healthHaver.IsDead;
								if (base.projectile.OnHitEnemy != null)
								{
									base.projectile.OnHitEnemy(base.projectile, speculativeRigidbody, arg3);
								}
								num++;
							}
							if ((bool)speculativeRigidbody.majorBreakable)
							{
								speculativeRigidbody.majorBreakable.ApplyDamage(num2 * BraveTime.DeltaTime, base.Direction, false);
								Chest component = speculativeRigidbody.GetComponent<Chest>();
								if ((bool)component && BraveUtility.EnumFlagsContains((uint)base.projectile.damageTypes, 32u) > 0 && component.ChestIdentifier == Chest.SpecialChestIdentifier.SECRET_RAINBOW)
								{
									component.RevealSecretRainbow();
								}
							}
							if ((bool)speculativeRigidbody.gameActor)
							{
								GameActor gameActor = speculativeRigidbody.gameActor;
								gameActor.BeamStatusAmount += BraveTime.DeltaTime * 1.5f;
								if (gameActor.BeamStatusAmount > TimeToStatus || base.Owner is AIActor)
								{
									if (base.projectile.AppliesSpeedModifier && UnityEngine.Random.value < BraveMathCollege.SliceProbability(statusEffectChance, BraveTime.DeltaTime))
									{
										gameActor.ApplyEffect(base.projectile.speedEffect);
									}
									if (base.projectile.AppliesPoison && UnityEngine.Random.value < BraveMathCollege.SliceProbability(statusEffectChance, BraveTime.DeltaTime))
									{
										gameActor.ApplyEffect(base.projectile.healthEffect);
									}
									if (base.projectile.AppliesCharm && UnityEngine.Random.value < BraveMathCollege.SliceProbability(statusEffectChance, BraveTime.DeltaTime))
									{
										gameActor.ApplyEffect(base.projectile.charmEffect);
									}
									if (base.projectile.AppliesFire && UnityEngine.Random.value < BraveMathCollege.SliceProbability(statusEffectChance, BraveTime.DeltaTime))
									{
										if (gameActor is PlayerController)
										{
											if (base.projectile.fireEffect.AffectsPlayers)
											{
												(gameActor as PlayerController).IsOnFire = true;
											}
										}
										else
										{
											gameActor.ApplyEffect(base.projectile.fireEffect);
										}
									}
									if (base.projectile.AppliesStun && (bool)gameActor.behaviorSpeculator && UnityEngine.Random.value < BraveMathCollege.SliceProbability(statusEffectChance, BraveTime.DeltaTime))
									{
										gameActor.behaviorSpeculator.Stun(base.projectile.AppliedStunDuration);
									}
								}
								if (base.projectile.AppliesFreeze)
								{
									gameActor.ApplyEffect(base.projectile.freezeEffect, BraveTime.DeltaTime * statusEffectAccumulateMultiplier);
								}
								if (base.projectile.AppliesBleed)
								{
									gameActor.ApplyEffect(base.projectile.bleedEffect, BraveTime.DeltaTime * statusEffectAccumulateMultiplier, base.projectile);
								}
							}
							if ((bool)m_beamGoopModifier)
							{
								if (m_beamGoopModifier.OnlyGoopOnEnemyCollision)
								{
									if (speculativeRigidbody.aiActor != null)
									{
										m_beamGoopModifier.SpawnCollisionGoop(speculativeRigidbody.UnitBottomCenter);
									}
								}
								else
								{
									m_beamGoopModifier.SpawnCollisionGoop(speculativeRigidbody.UnitBottomCenter);
								}
							}
							if (speculativeRigidbody.OnHitByBeam != null)
							{
								speculativeRigidbody.OnHitByBeam(this);
							}
							if (base.Owner is PlayerController)
							{
								(base.Owner as PlayerController).DoPostProcessBeamTick(this, speculativeRigidbody, BraveTime.DeltaTime);
							}
							if (!(base.Owner is AIActor))
							{
								continue;
							}
							AIActor aIActor = base.Owner as AIActor;
							if (!aIActor.CompanionOwner)
							{
								continue;
							}
							aIActor.CompanionOwner.DoPostProcessBeamTick(this, speculativeRigidbody, BraveTime.DeltaTime);
							if ((bool)aIActor.CompanionOwner.CurrentGun && aIActor.CompanionOwner.CurrentGun.LuteCompanionBuffActive)
							{
								if (m_currentLuteScaleModifier != 1.75f)
								{
									m_currentLuteScaleModifier = 1.75f;
									ProjectileScale *= m_currentLuteScaleModifier;
								}
							}
							else if (m_currentLuteScaleModifier != 1f)
							{
								ProjectileScale /= m_currentLuteScaleModifier;
								m_currentLuteScaleModifier = 1f;
							}
						}
					}
				}
				if (angularKnockback)
				{
					if (list == null || list.Count <= 0)
					{
						return;
					}
					AngularKnockbackTier knockbackTier2 = GetKnockbackTier();
					if (knockbackTier2 == null)
					{
						return;
					}
					for (int j = 0; j < list.Count; j++)
					{
						KnockbackDoer knockbackDoer = list[j].knockbackDoer;
						if (!knockbackDoer)
						{
							continue;
						}
						Vector2 vector = knockbackDoer.specRigidbody.UnitCenter - base.Origin;
						if (!(knockbackTier2.minAngularSpeed <= 0f))
						{
							if (averageAngularVelocity > 0f)
							{
								vector = vector.Rotate(90f);
							}
							else if (averageAngularVelocity < 0f)
							{
								vector = vector.Rotate(-90f);
							}
						}
						knockbackDoer.ApplyKnockback(vector, base.projectile.baseData.force * knockbackTier2.knockbackMultiplier);
					}
					return;
				}
				KnockbackDoer knockbackDoer2 = ((list == null || list.Count <= 0) ? null : list[0].knockbackDoer);
				if (knockbackDoer2 != m_enemyKnockback)
				{
					if ((bool)m_enemyKnockback)
					{
						m_enemyKnockback.EndContinuousKnockback(m_enemyKnockbackId);
					}
					if ((bool)knockbackDoer2)
					{
						m_enemyKnockbackId = knockbackDoer2.ApplyContinuousKnockback(knockbackDoer2.specRigidbody.UnitCenter - base.Origin, base.projectile.baseData.force);
					}
					m_enemyKnockback = knockbackDoer2;
				}
				if (m_beamGoopModifier != null)
				{
					HandleGoopFrame(m_beamGoopModifier);
				}
				HandleIgnitionAndFreezing();
			}
		}
		catch (Exception ex)
		{
			throw new Exception(string.Format("Caught BasicBeamController.HandleBeamFrame() exception. i={0}, ex={1}", exceptionTracker, ex.ToString()));
		}
	}

	private AngularKnockbackTier GetKnockbackTier()
	{
		AngularKnockbackTier result = null;
		for (int i = 0; i < angularKnockbackTiers.Count; i++)
		{
			AngularKnockbackTier angularKnockbackTier = angularKnockbackTiers[i];
			if (angularKnockbackTier.minAngularSpeed < Mathf.Abs(averageAngularVelocity))
			{
				result = angularKnockbackTiers[i];
				continue;
			}
			break;
		}
		return result;
	}

	public List<SpeculativeRigidbody> HandleBeamFrame(Vector2 origin, Vector2 direction, bool hitsPlayers, bool hitsEnemies, bool hitsProjectiles, out PixelCollider pixelCollider, params SpeculativeRigidbody[] ignoreRigidbodies)
	{
		exceptionTracker = 0;
		float num = Mathf.Atan2(direction.y, direction.x) * 57.29578f;
		List<SpeculativeRigidbody> list = new List<SpeculativeRigidbody>();
		pixelCollider = null;
		if (!m_beamSprite)
		{
			return list;
		}
		if (ProjectileAndBeamMotionModule is OrbitProjectileMotionModule)
		{
			m_currentBeamDistance = 30f + (float)Math.PI * 2f * (ProjectileAndBeamMotionModule as OrbitProjectileMotionModule).BeamOrbitRadius;
		}
		float num2 = 0f;
		if (angularKnockback)
		{
			float num3 = direction.ToAngle();
			if (num3 <= 155f && num3 >= 25f)
			{
				num2 = -1f;
				if (!m_hasToggledGunOutline && (bool)base.Gun && (bool)base.Gun.GetSprite())
				{
					m_hasToggledGunOutline = true;
					SpriteOutlineManager.RemoveOutlineFromSprite(base.Gun.GetSprite());
				}
			}
			else if (m_hasToggledGunOutline)
			{
				m_hasToggledGunOutline = false;
				SpriteOutlineManager.AddOutlineToSprite(base.Gun.GetSprite(), Color.black);
			}
		}
		m_beamSprite.HeightOffGround = CurrentBeamHeightOffGround + HeightOffset + num2;
		if (IsConnected && base.Owner is PlayerController && HandleChanceTick() && m_chanceTick < -999f)
		{
			m_bones.First.Value.HomingRadius = HomingRadius;
			m_bones.First.Value.HomingAngularVelocity = HomingAngularVelocity;
			m_bones.Last.Value.HomingRadius = HomingRadius;
			m_bones.Last.Value.HomingAngularVelocity = HomingAngularVelocity;
			m_chanceTick = 1f;
		}
		float z = origin.y - CurrentBeamHeightOffGround;
		base.transform.position = Vector3Extensions.WithZ(origin, z).Quantize(0.0625f);
		if (!m_previousAngle.HasValue)
		{
			m_previousAngle = num;
		}
		if (angularKnockback && BraveTime.DeltaTime > 0f)
		{
			float num4 = BraveMathCollege.ClampAngle180(num - m_previousAngle.Value);
			float newSpeed = num4 / BraveTime.DeltaTime;
			averageAngularVelocity = BraveMathCollege.MovingAverageSpeed(averageAngularVelocity, newSpeed, BraveTime.DeltaTime, angularSpeedAvgWindow);
		}
		if (State == BeamState.Charging)
		{
			if (UsesChargeSprite && rotateChargeAnimation)
			{
				m_beamMuzzleAnimator.transform.rotation = Quaternion.Euler(0f, 0f, direction.ToAngle());
			}
			if (m_chargeTimer >= chargeDelay)
			{
				GetComponent<Renderer>().enabled = true;
				if (UsesChargeSprite && rotateChargeAnimation)
				{
					m_beamMuzzleAnimator.transform.rotation = Quaternion.identity;
				}
				if (boneType == BeamBoneType.Projectile)
				{
					m_bones.First.Value.Position = base.Origin;
					m_bones.First.Value.Velocity = base.Direction.normalized * BoneSpeed;
					m_bones.Last.Value.Position = base.Origin;
					m_bones.Last.Value.Velocity = base.Direction.normalized * BoneSpeed;
				}
				if (usesTelegraph)
				{
					State = BeamState.Telegraphing;
					if ((bool)m_beamMuzzleSprite)
					{
						m_muzzleTransform.gameObject.SetActive(false);
					}
				}
				else
				{
					if (UsesMuzzleSprite)
					{
						m_muzzleTransform.gameObject.SetActive(true);
						m_beamMuzzleAnimator.Play(muzzleAnimation);
					}
					else if ((bool)m_beamMuzzleSprite)
					{
						m_muzzleTransform.gameObject.SetActive(false);
					}
					State = BeamState.Firing;
				}
			}
		}
		else
		{
			if (State == BeamState.Telegraphing && m_telegraphTimer > telegraphTime)
			{
				State = BeamState.Firing;
				base.spriteAnimator.Play(CurrentBeamAnimation);
				if (UsesMuzzleSprite)
				{
					m_beamMuzzleAnimator.Play(muzzleAnimation);
					m_muzzleTransform.gameObject.SetActive(true);
				}
			}
			float num5 = 0f;
			int num6 = 0;
			if (boneType == BeamBoneType.Straight)
			{
				if (BoneSpeed > 0f)
				{
					num5 = BraveTime.DeltaTime * BoneSpeed;
					m_currentBeamDistance = Mathf.Min(m_currentBeamDistance + num5, base.projectile.baseData.range);
					for (LinkedListNode<BeamBone> linkedListNode = m_bones.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
					{
						linkedListNode.Value.PosX = Mathf.Min(linkedListNode.Value.PosX + num5, m_currentBeamDistance);
					}
					m_bones.First.Value.PosX = Mathf.Max(0f, m_bones.First.Next.Value.PosX - m_beamQuadUnitWidth);
					while (m_bones.First.Value.PosX != 0f)
					{
						int num7 = m_bones.First.Value.SubtileNum - 1;
						if (num7 < 0)
						{
							num7 = m_beamSpriteSubtileWidth - 1;
						}
						m_bones.AddFirst(new BeamBone(Mathf.Max(0f, m_bones.First.Value.PosX - m_beamQuadUnitWidth), 0f, num7));
						num6++;
					}
					while (m_bones.Count > 2 && m_bones.Last.Previous.Value.PosX == m_currentBeamDistance)
					{
						m_bones.RemoveLast();
					}
					if (TileType == BeamTileType.Flowing)
					{
						m_uvOffset -= num5 / m_beamSpriteUnitWidth;
						while (m_uvOffset < 0f)
						{
							m_uvOffset += 1f;
						}
					}
				}
				else if (BoneSpeed <= 0f)
				{
					m_currentBeamDistance = base.projectile.baseData.range;
				}
			}
			else if (boneType == BeamBoneType.Projectile)
			{
				num5 = BraveTime.DeltaTime * BoneSpeed;
				m_currentBeamDistance = Mathf.Min(m_currentBeamDistance + num5, base.projectile.baseData.range);
				LinkedListNode<BeamBone> linkedListNode2 = m_bones.First;
				bool flag = false;
				while (linkedListNode2 != null)
				{
					linkedListNode2.Value.ApplyHoming(ReflectedFromRigidbody);
					linkedListNode2.Value.PosX = Mathf.Min(linkedListNode2.Value.PosX + num5, m_currentBeamDistance);
					linkedListNode2.Value.Position += linkedListNode2.Value.Velocity * BraveTime.DeltaTime;
					if (linkedListNode2.Value.HomingDampenMotion)
					{
						flag = true;
					}
					linkedListNode2 = linkedListNode2.Next;
				}
				if (flag)
				{
					linkedListNode2 = m_bones.First.Next;
					Vector2 vector = m_bones.First.Value.Position;
					Vector2 vector2 = m_bones.First.Value.Velocity;
					while (linkedListNode2 != null)
					{
						if (linkedListNode2.Next != null)
						{
							Vector2 position = linkedListNode2.Next.Value.Position;
							Vector2 velocity = linkedListNode2.Next.Value.Velocity;
							Vector2 position2 = linkedListNode2.Value.Position;
							Vector2 velocity2 = linkedListNode2.Value.Velocity;
							if (linkedListNode2.Value.HomingDampenMotion)
							{
								linkedListNode2.Value.Position = 0.2f * position2 + 0.4f * position + 0.4f * vector;
								linkedListNode2.Value.Velocity = 0.2f * velocity2 + 0.4f * velocity + 0.4f * vector2;
							}
							vector = position2;
							vector2 = velocity2;
						}
						linkedListNode2 = linkedListNode2.Next;
					}
				}
				if (interpolateStretchedBones && m_bones.Count > 1)
				{
					linkedListNode2 = m_bones.First.Next;
					LinkedListNode<BeamBone> linkedListNode3 = null;
					while (linkedListNode2 != null)
					{
						if (Vector2.SqrMagnitude(linkedListNode2.Value.Position - linkedListNode2.Previous.Value.Position) > m_sqrNewBoneThreshold)
						{
							BeamBone beamBone = new BeamBone((linkedListNode2.Previous.Value.PosX + linkedListNode2.Value.PosX) / 2f, linkedListNode2.Value.RotationAngle, linkedListNode2.Value.SubtileNum);
							if (linkedListNode2.Previous.Previous != null && linkedListNode2.Next != null)
							{
								Vector2 position3 = linkedListNode2.Previous.Previous.Value.Position;
								Vector2 position4 = linkedListNode2.Next.Value.Position;
								Vector2 position5 = linkedListNode2.Previous.Value.Position;
								Vector2 p = position5 + (position5 - position3).normalized * m_beamQuadUnitWidth;
								Vector2 position6 = linkedListNode2.Value.Position;
								Vector2 p2 = position6 + (position6 - position4).normalized * m_beamQuadUnitWidth;
								beamBone.Position = BraveMathCollege.CalculateBezierPoint(0.5f, position5, p, p2, position6);
							}
							else
							{
								beamBone.Position = (linkedListNode2.Previous.Value.Position + linkedListNode2.Value.Position) / 2f;
							}
							beamBone.Velocity = (linkedListNode2.Previous.Value.Velocity + linkedListNode2.Value.Velocity) / 2f;
							linkedListNode3 = m_bones.AddBefore(linkedListNode2, beamBone);
						}
						linkedListNode2 = linkedListNode2.Next;
					}
					if (linkedListNode3 != null)
					{
						for (LinkedListNode<BeamBone> linkedListNode4 = linkedListNode3; linkedListNode4 != null; linkedListNode4 = linkedListNode4.Previous)
						{
							linkedListNode4.Value.SubtileNum = ((linkedListNode4.Next.Value.SubtileNum != 0) ? (linkedListNode4.Next.Value.SubtileNum - 1) : (m_beamSpriteSubtileWidth - 1));
						}
					}
				}
				if (State == BeamState.Telegraphing || State == BeamState.Firing || State == BeamState.Dissipating)
				{
					Vector2 origin2 = base.Origin;
					Vector2 position7 = m_bones.First.Value.Position;
					if (IsHoming)
					{
						m_previousAngle = m_bones.First.Value.Velocity.ToAngle();
					}
					float num8 = Mathf.Max(0f, m_bones.First.Next.Value.PosX - m_beamQuadUnitWidth);
					float t = Mathf.InverseLerp(0f, num5, num8);
					float angle = m_previousAngle.Value + Mathf.Lerp(BraveMathCollege.ClampAngle180(num - m_previousAngle.Value), 0f, t);
					m_bones.First.Value.PosX = num8;
					m_bones.First.Value.Position = Vector2.Lerp(origin2, position7, Mathf.InverseLerp(0f, num5, m_bones.First.Value.PosX));
					m_bones.First.Value.Velocity = BraveMathCollege.DegreesToVector(angle, BoneSpeed);
					m_bones.First.Value.HomingRadius = HomingRadius;
					m_bones.First.Value.HomingAngularVelocity = HomingAngularVelocity;
					while (m_bones.First.Value.PosX != 0f)
					{
						int subtileNum = ((m_bones.First.Value.SubtileNum != 0) ? (m_bones.First.Value.SubtileNum - 1) : (m_beamSpriteSubtileWidth - 1));
						num8 = Mathf.Max(0f, m_bones.First.Value.PosX - m_beamQuadUnitWidth);
						t = Mathf.InverseLerp(0f, num5, num8);
						angle = m_previousAngle.Value + Mathf.Lerp(BraveMathCollege.ClampAngle180(num - m_previousAngle.Value), 0f, t);
						m_bones.AddFirst(new BeamBone(num8, 0f, subtileNum));
						m_bones.First.Value.Position = Vector2.Lerp(origin2, position7, t);
						m_bones.First.Value.Velocity = BraveMathCollege.DegreesToVector(angle, BoneSpeed);
						m_bones.First.Value.HomingRadius = HomingRadius;
						m_bones.First.Value.HomingAngularVelocity = HomingAngularVelocity;
						num6++;
					}
					if (TileType == BeamTileType.Flowing)
					{
						m_uvOffset -= num5 / m_beamSpriteUnitWidth;
						while (m_uvOffset < 0f)
						{
							m_uvOffset += 1f;
						}
					}
				}
				else if (State == BeamState.Disconnected)
				{
					if (decayNear > 0f)
					{
						float num9 = m_bones.First.Value.PosX + decayNear * BraveTime.DeltaTime;
						m_bones.First.Value.PosX = num9;
						LinkedListNode<BeamBone> next = m_bones.First.Next;
						while (next != null && next.Value.PosX < num9)
						{
							next.Value.PosX = num9;
							next = next.Next;
						}
					}
					if (decayFar > 0f)
					{
						m_currentBeamDistance -= decayFar * BraveTime.DeltaTime;
						LinkedListNode<BeamBone> linkedListNode5 = m_bones.Last;
						while (linkedListNode5 != null && linkedListNode5.Value.PosX >= m_currentBeamDistance)
						{
							linkedListNode5.Value.PosX = m_currentBeamDistance;
							linkedListNode5 = linkedListNode5.Previous;
						}
					}
				}
				float posX = m_bones.First.Value.PosX;
				while (m_bones.Count > 2 && m_bones.First.Next.Value.PosX <= posX)
				{
					m_bones.RemoveFirst();
				}
				while (m_bones.Count > 2 && m_bones.Last.Previous.Value.PosX >= m_currentBeamDistance)
				{
					m_bones.RemoveLast();
				}
			}
			if (UsesBones && (State == BeamState.Telegraphing || State == BeamState.Firing))
			{
				if (boneType == BeamBoneType.Straight)
				{
					m_bones.Clear();
					DungeonData data = GameManager.Instance.Dungeon.data;
					float num10 = 0f;
					Vector2 position8 = origin;
					float angle2 = num;
					float num11 = ((!(BoneSpeed < 0f)) ? BoneSpeed : 40f);
					float num12 = m_beamQuadUnitWidth / num11;
					m_bones.AddLast(new BeamBone(num10, position8, BraveMathCollege.DegreesToVector(angle2, num11)));
					while (num10 < m_currentBeamDistance)
					{
						num10 = Mathf.Min(num10 + m_beamQuadUnitWidth, m_currentBeamDistance);
						BeamBone beamBone2 = new BeamBone(num10, position8, BraveMathCollege.DegreesToVector(angle2, num11));
						beamBone2.HomingRadius = HomingRadius;
						beamBone2.HomingAngularVelocity = HomingAngularVelocity;
						m_bones.AddLast(beamBone2);
						float overrideDeltaTime = num12;
						beamBone2.ApplyHoming(null, overrideDeltaTime);
						beamBone2.Position += beamBone2.Velocity * num12;
						position8 = beamBone2.Position;
						if (ProjectileAndBeamMotionModule != null && ProjectileAndBeamMotionModule is OrbitProjectileMotionModule)
						{
							position8 += ProjectileAndBeamMotionModule.GetBoneOffset(beamBone2, this, base.projectile.Inverted);
						}
						angle2 = beamBone2.Velocity.ToAngle();
						if (num10 > IgnoreTilesDistance && data.isWall((int)position8.x, (int)position8.y) && !data.isAnyFaceWall((int)position8.x, (int)position8.y))
						{
							m_currentBeamDistance = num10;
							break;
						}
					}
				}
				for (LinkedListNode<BeamBone> linkedListNode6 = m_bones.First; linkedListNode6 != null; linkedListNode6 = linkedListNode6.Next)
				{
					Vector2 zero = Vector2.zero;
					if (linkedListNode6.Next != null)
					{
						zero += (linkedListNode6.Next.Value.Position - linkedListNode6.Value.Position).normalized;
					}
					if (linkedListNode6.Previous != null)
					{
						zero += (linkedListNode6.Value.Position - linkedListNode6.Previous.Value.Position).normalized;
					}
					linkedListNode6.Value.RotationAngle = ((!(zero != Vector2.zero)) ? 0f : BraveMathCollege.Atan2Degrees(zero));
				}
			}
			int num13 = CollisionLayerMatrix.GetMask(CollisionLayer.Projectile);
			if (!hitsPlayers)
			{
				num13 &= ~CollisionMask.LayerToMask(CollisionLayer.PlayerHitBox, CollisionLayer.EnemyBulletBlocker);
			}
			if (!hitsEnemies)
			{
				num13 &= ~CollisionMask.LayerToMask(CollisionLayer.EnemyHitBox);
			}
			if (hitsProjectiles)
			{
				num13 |= CollisionMask.LayerToMask(CollisionLayer.Projectile);
			}
			num13 |= CollisionMask.LayerToMask(CollisionLayer.BeamBlocker);
			if (m_pierceImpactSprites != null)
			{
				for (int i = 0; i < m_pierceImpactSprites.Count; i++)
				{
					m_pierceImpactSprites[i].gameObject.SetActive(false);
				}
			}
			int num14 = 0;
			int num15 = penetration;
			bool flag2 = false;
			UltraFortunesFavor ultraFortunesFavor = null;
			SpeculativeRigidbody speculativeRigidbody = null;
			List<SpeculativeRigidbody> list2 = new List<SpeculativeRigidbody>(ignoreRigidbodies);
			int num16 = 0;
			bool flag3;
			Vector2 targetPoint;
			Vector2 targetNormal;
			SpeculativeRigidbody hitRigidbody;
			List<PointcastResult> boneCollisions;
			bool flag4;
			do
			{
				flag3 = FindBeamTarget(origin, direction, m_currentBeamDistance, num13, out targetPoint, out targetNormal, out hitRigidbody, out pixelCollider, out boneCollisions, null, list2.ToArray());
				flag2 = flag3;
				flag4 = flag3 && (bool)hitRigidbody;
				if (flag3 && !hitRigidbody && (bool)m_beamGoopModifier && !m_beamGoopModifier.OnlyGoopOnEnemyCollision)
				{
					Vector3 vector3 = targetPoint;
					if (targetNormal.y < 0.8f)
					{
						vector3.y -= 1f;
					}
					m_beamGoopModifier.SpawnCollisionGoop(vector3);
				}
				if (flag3 && (bool)hitRigidbody)
				{
					ultraFortunesFavor = hitRigidbody.ultraFortunesFavor;
					if ((bool)ultraFortunesFavor)
					{
						hitRigidbody = null;
					}
					if ((bool)hitRigidbody && hitRigidbody.ReflectBeams)
					{
						speculativeRigidbody = hitRigidbody;
						hitRigidbody = null;
						pixelCollider = null;
						flag4 = false;
					}
					if ((bool)hitRigidbody && hitRigidbody.BlockBeams)
					{
						hitRigidbody = null;
						pixelCollider = null;
						flag4 = false;
					}
					if ((bool)hitRigidbody && hitRigidbody.PreventPiercing)
					{
						if (!hitRigidbody.healthHaver && !hitRigidbody.majorBreakable)
						{
							hitRigidbody = null;
						}
						flag4 = false;
					}
					if (hitsPlayers && (bool)hitRigidbody)
					{
						PlayerController component = hitRigidbody.GetComponent<PlayerController>();
						if ((bool)component && (component.spriteAnimator.QueryInvulnerabilityFrame() || !component.healthHaver.IsVulnerable || component.IsEthereal))
						{
							list2.Add(hitRigidbody);
							hitRigidbody = null;
							num15++;
							component.HandleDodgedBeam(this);
						}
					}
					if ((bool)hitRigidbody && (bool)hitRigidbody.minorBreakable)
					{
						if (targetNormal != Vector2.zero)
						{
							hitRigidbody.minorBreakable.Break(-targetNormal);
						}
						else
						{
							hitRigidbody.minorBreakable.Break();
						}
						if ((bool)m_beamGoopModifier && !m_beamGoopModifier.OnlyGoopOnEnemyCollision)
						{
							m_beamGoopModifier.SpawnCollisionGoop(hitRigidbody.UnitBottomCenter);
						}
						hitRigidbody = null;
						num15++;
					}
					if ((bool)hitRigidbody && pixelCollider != null && pixelCollider.CollisionLayer == CollisionLayer.BeamBlocker)
					{
						TorchController component2 = hitRigidbody.GetComponent<TorchController>();
						if ((bool)component2)
						{
							list2.Add(hitRigidbody);
							hitRigidbody = null;
							num15++;
						}
					}
					if (PenetratesCover && (bool)hitRigidbody && (bool)hitRigidbody.majorBreakable && (bool)hitRigidbody.transform.parent && (bool)hitRigidbody.transform.parent.GetComponent<FlippableCover>())
					{
						flag4 = true;
						num15++;
					}
					if ((bool)hitRigidbody)
					{
						list.Add(hitRigidbody);
						list2.Add(hitRigidbody);
					}
					num15--;
					if ((bool)hitRigidbody && num15 >= 0 && !string.IsNullOrEmpty(impactAnimation))
					{
						if (m_pierceImpactSprites == null)
						{
							m_pierceImpactSprites = new List<tk2dBaseSprite>();
						}
						if (num14 >= m_pierceImpactSprites.Count)
						{
							m_pierceImpactSprites.Add(CreatePierceImpactEffect());
						}
						tk2dBaseSprite tk2dBaseSprite2 = m_pierceImpactSprites[num14];
						tk2dBaseSprite2.gameObject.SetActive(true);
						float z2 = Mathf.Atan2(targetNormal.y, targetNormal.x) * 57.29578f;
						tk2dBaseSprite2.transform.rotation = Quaternion.Euler(0f, 0f, z2);
						tk2dBaseSprite2.transform.position = targetPoint;
						bool flag6 = (tk2dBaseSprite2.IsPerpendicular = targetNormal.y < -0.5f);
						tk2dBaseSprite2.HeightOffGround = ((!flag6) ? 0.05f : 2f);
						tk2dBaseSprite2.UpdateZDepth();
						num14++;
					}
					if ((bool)hitRigidbody && (bool)hitRigidbody.hitEffectHandler)
					{
						HitEffectHandler hitEffectHandler = hitRigidbody.hitEffectHandler;
						if (hitEffectHandler.additionalHitEffects != null && hitEffectHandler.additionalHitEffects.Length > 0)
						{
							hitEffectHandler.HandleAdditionalHitEffects(BraveMathCollege.DegreesToVector(base.Direction.ToAngle(), 8f), pixelCollider);
						}
					}
					if ((bool)hitRigidbody && hitRigidbody.OnBeamCollision != null)
					{
						hitRigidbody.OnBeamCollision(this);
					}
				}
				num16++;
			}
			while (flag4 && num15 >= 0 && num16 < 100);
			if (num16 >= 100)
			{
				Debug.LogErrorFormat("Infinite loop averted!  TELL RUBEL! {0} {1}", base.Owner, this);
			}
			if (flag3 && (bool)hitRigidbody && (bool)hitRigidbody.gameActor && ContinueBeamArtToWall && !hitRigidbody.BlockBeams)
			{
				int collisionMask = CollisionMask.LayerToMask(CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker);
				Func<SpeculativeRigidbody, bool> rigidbodyExcluder = (SpeculativeRigidbody specRigidbody) => specRigidbody.gameActor;
				SpeculativeRigidbody hitRigidbody2;
				PixelCollider hitPixelCollider;
				flag3 = FindBeamTarget(origin, direction, m_currentBeamDistance, collisionMask, out targetPoint, out targetNormal, out hitRigidbody2, out hitPixelCollider, out boneCollisions, rigidbodyExcluder, list2.ToArray());
			}
			if (flag3)
			{
				bool flag7 = false;
				Vector2 vector4 = new Vector2(-1f, -1f);
				Vector2 vector5 = Vector2.zero;
				HitEffectHandler hitEffectHandler2 = ((!(hitRigidbody != null)) ? null : hitRigidbody.hitEffectHandler);
				LinkedListNode<BeamBone> linkedListNode8;
				if (UsesBones)
				{
					int num17 = 0;
					bool flag8 = false;
					if (boneCollisions[num17].hitDirection == HitDirection.Forward && boneCollisions[num17].boneIndex == 0)
					{
						num17++;
						if (boneCollisions.Count == 1)
						{
							flag8 = true;
						}
					}
					if (flag8 || boneCollisions[num17].hitDirection == HitDirection.Backward)
					{
						Vector2 contact;
						float posX2;
						LinkedListNode<BeamBone> linkedListNode7;
						if (flag8)
						{
							contact = boneCollisions[0].hitResult.Contact;
							posX2 = m_bones.First.Value.PosX;
							linkedListNode7 = m_bones.Last;
						}
						else
						{
							linkedListNode8 = m_bones.First;
							for (int j = 0; j < boneCollisions[num17].boneIndex; j++)
							{
								linkedListNode8 = linkedListNode8.Next;
							}
							contact = boneCollisions[num17].hitResult.Contact;
							posX2 = Mathf.Lerp(linkedListNode8.Value.PosX, linkedListNode8.Previous.Value.PosX, Mathf.Clamp01(Vector2.Distance(linkedListNode8.Value.Position, contact) / Vector2.Distance(linkedListNode8.Value.Position, linkedListNode8.Previous.Value.Position)));
							linkedListNode7 = linkedListNode8.Previous;
							flag7 = true;
							vector4 = contact;
							vector5 = boneCollisions[num17].hitResult.Normal;
						}
						while (linkedListNode7 != null)
						{
							linkedListNode7.Value.PosX = posX2;
							linkedListNode7.Value.Position = contact;
							linkedListNode7 = linkedListNode7.Previous;
						}
						flag2 = false;
						num17++;
					}
					if (num17 < boneCollisions.Count)
					{
						if (boneCollisions[num17].hitDirection != HitDirection.Forward)
						{
							Debug.LogError("WTF?");
						}
						LinkedListNode<BeamBone> linkedListNode9 = m_bones.First;
						for (int k = 0; k < boneCollisions[num17].boneIndex; k++)
						{
							linkedListNode9 = linkedListNode9.Next;
						}
						float t2 = 1f;
						if (linkedListNode9.Next != null)
						{
							t2 = Mathf.Clamp01(Vector2.Distance(linkedListNode9.Value.Position, targetPoint) / Vector2.Distance(linkedListNode9.Value.Position, linkedListNode9.Next.Value.Position));
						}
						if (num17 + 1 < boneCollisions.Count && collisionSeparation && !(ProjectileAndBeamMotionModule is OrbitProjectileMotionModule))
						{
							num17++;
							LinkedListNode<BeamBone> linkedListNode10 = m_bones.First;
							for (int l = 0; l < boneCollisions[num17].boneIndex; l++)
							{
								linkedListNode10 = linkedListNode10.Next;
							}
							Vector2 contact2 = boneCollisions[num17].hitResult.Contact;
							float t3 = Mathf.Clamp01(Vector2.Distance(linkedListNode10.Value.Position, contact2) / Vector2.Distance(linkedListNode10.Value.Position, linkedListNode10.Previous.Value.Position));
							float newPosX = Mathf.Lerp(linkedListNode10.Value.PosX, linkedListNode10.Previous.Value.PosX, t3);
							SeparateBeam(linkedListNode10, contact2, newPosX);
							flag2 = true;
						}
						m_currentBeamDistance = ((linkedListNode9.Next == null) ? linkedListNode9.Value.PosX : Mathf.Lerp(linkedListNode9.Value.PosX, linkedListNode9.Next.Value.PosX, t2));
					}
				}
				else
				{
					m_currentBeamDistance = (targetPoint - origin).magnitude;
					if (m_bones.Count == 2)
					{
						m_bones.First.Value.PosX = 0f;
						m_bones.Last.Value.PosX = m_currentBeamDistance;
					}
				}
				linkedListNode8 = m_bones.Last;
				while (linkedListNode8 != null && !(linkedListNode8.Value.PosX < m_currentBeamDistance))
				{
					linkedListNode8.Value.PosX = Mathf.Min(m_currentBeamDistance, linkedListNode8.Value.PosX);
					linkedListNode8 = linkedListNode8.Previous;
				}
				while (m_bones.Count > 2 && m_bones.Last.Previous.Value.PosX == m_currentBeamDistance)
				{
					m_bones.RemoveLast();
				}
				bool flag9 = !(hitEffectHandler2 != null) || !hitEffectHandler2.SuppressAllHitEffects;
				if (UsesImpactSprite)
				{
					if (State != BeamState.Telegraphing && flag9)
					{
						if (!m_impactTransform.gameObject.activeSelf)
						{
							m_impactTransform.gameObject.SetActive(true);
						}
						float z3 = Mathf.Atan2(targetNormal.y, targetNormal.x) * 57.29578f;
						m_impactTransform.rotation = Quaternion.Euler(0f, 0f, z3);
						m_impactTransform.position = targetPoint;
						bool flag10 = targetNormal.y < -0.5f;
						m_impactSprite.IsPerpendicular = flag10;
						m_impactSprite.HeightOffGround = ((!flag10) ? 0.05f : 2f);
						m_impactSprite.UpdateZDepth();
						if ((bool)m_impact2Transform && flag7)
						{
							if (!m_impact2Transform.gameObject.activeSelf)
							{
								m_impact2Transform.gameObject.SetActive(true);
							}
							float z4 = Mathf.Atan2(vector5.y, vector5.x) * 57.29578f;
							m_impact2Transform.rotation = Quaternion.Euler(0f, 0f, z4);
							m_impact2Transform.position = vector4;
							bool flag11 = vector5.y < -0.5f;
							m_impact2Sprite.IsPerpendicular = flag11;
							m_impact2Sprite.HeightOffGround = ((!flag11) ? 0.05f : 2f);
							m_impact2Sprite.UpdateZDepth();
						}
					}
					else if (UsesImpactSprite)
					{
						if (m_impactTransform.gameObject.activeSelf)
						{
							m_impactTransform.gameObject.SetActive(false);
						}
						if (m_impact2Transform.gameObject.activeSelf)
						{
							m_impact2Transform.gameObject.SetActive(false);
						}
					}
				}
			}
			else if (ShowImpactOnMaxDistanceEnd)
			{
				if (!m_impactTransform.gameObject.activeSelf)
				{
					m_impactTransform.gameObject.SetActive(true);
				}
				Vector2 vector6 = ((m_bones.Count < 2 || !UsesBones) ? (base.Origin + base.Direction.normalized * m_currentBeamDistance) : GetBonePosition(m_bones.Last.Value));
				m_impactTransform.rotation = Quaternion.identity;
				m_impactTransform.position = vector6;
				m_impactSprite.IsPerpendicular = false;
				m_impactSprite.HeightOffGround = 0.05f;
				m_impactSprite.UpdateZDepth();
				if (m_impact2Transform.gameObject.activeSelf)
				{
					m_impact2Transform.gameObject.SetActive(false);
				}
			}
			else if (UsesImpactSprite)
			{
				if (m_impactTransform.gameObject.activeSelf)
				{
					m_impactTransform.gameObject.SetActive(false);
				}
				if (m_impact2Transform.gameObject.activeSelf)
				{
					m_impact2Transform.gameObject.SetActive(false);
				}
			}
			if (UsesDispersalParticles)
			{
				if (boneType == BeamBoneType.Straight)
				{
					Vector2 bonePosition = GetBonePosition(m_bones.First.Value);
					Vector2 vector7 = bonePosition + BraveMathCollege.DegreesToVector(base.transform.eulerAngles.z).normalized * m_currentBeamDistance;
					DoDispersalParticles(bonePosition.ToVector3ZisY(), vector7.ToVector3ZisY(), flag3);
				}
				else
				{
					for (LinkedListNode<BeamBone> linkedListNode11 = m_bones.First; linkedListNode11 != null; linkedListNode11 = linkedListNode11.Next)
					{
						DoDispersalParticles(linkedListNode11, linkedListNode11.Value.SubtileNum, flag3);
					}
				}
			}
			exceptionTracker = 0;
			if ((reflections > 0 || (bool)ultraFortunesFavor || (bool)speculativeRigidbody) && flag2)
			{
				exceptionTracker = 100;
				if (targetNormal.x == 0f && targetNormal.y == 0f)
				{
					targetNormal = m_lastHitNormal;
				}
				else
				{
					m_lastHitNormal = targetNormal;
				}
				exceptionTracker = 101;
				float num18 = BraveMathCollege.ClampAngle360(direction.ToAngle() + 180f);
				float num19 = targetNormal.ToAngle();
				if ((bool)ultraFortunesFavor)
				{
					num19 = ultraFortunesFavor.GetBeamNormal(targetPoint).ToAngle();
					ultraFortunesFavor.HitFromPoint(targetPoint);
				}
				exceptionTracker = 102;
				if ((bool)speculativeRigidbody && speculativeRigidbody.ReflectProjectilesNormalGenerator != null)
				{
					num19 = speculativeRigidbody.ReflectProjectilesNormalGenerator(targetPoint, targetNormal).ToAngle();
				}
				float angle3 = num18 + 2f * BraveMathCollege.ClampAngle180(num19 - num18);
				Vector2 vector8 = targetPoint + targetNormal.normalized * PhysicsEngine.PixelToUnit(2);
				if (!m_reflectedBeam)
				{
					exceptionTracker = 103;
					m_reflectedBeam = CreateReflectedBeam(vector8, BraveMathCollege.DegreesToVector(angle3), !ultraFortunesFavor);
				}
				else
				{
					exceptionTracker = 104;
					m_reflectedBeam.Origin = vector8;
					m_reflectedBeam.Direction = BraveMathCollege.DegreesToVector(angle3);
					m_reflectedBeam.LateUpdatePosition(vector8);
				}
				exceptionTracker = 1041;
				if ((bool)m_reflectedBeam)
				{
					m_reflectedBeam.penetration = num15;
					exceptionTracker = 39;
					if ((bool)speculativeRigidbody)
					{
						m_reflectedBeam.ReflectedFromRigidbody = speculativeRigidbody;
					}
					else
					{
						m_reflectedBeam.ReflectedFromRigidbody = null;
					}
				}
			}
			else
			{
				exceptionTracker = 105;
				if ((bool)m_reflectedBeam)
				{
					exceptionTracker = 106;
					m_reflectedBeam.CeaseAttack();
					m_reflectedBeam = null;
				}
			}
		}
		exceptionTracker = 0;
		if (doesScreenDistortion)
		{
			if (m_distortionMaterial == null)
			{
				m_distortionMaterial = new Material(ShaderCache.Acquire("Brave/Internal/DistortionLine"));
			}
			Pixelator.Instance.RegisterAdditionalRenderPass(m_distortionMaterial);
			Vector2 vector9 = m_currentBeamDistance * direction.normalized + origin;
			Vector3 vector10 = GameManager.Instance.MainCameraController.Camera.WorldToViewportPoint(origin.ToVector3ZUp());
			Vector3 vector11 = GameManager.Instance.MainCameraController.Camera.WorldToViewportPoint(vector9.ToVector3ZUp());
			Vector4 value = new Vector4(vector10.x, vector10.y, startDistortionRadius, startDistortionPower);
			Vector4 value2 = new Vector4(vector11.x, vector11.y, endDistortionRadius, endDistortionPower);
			m_distortionMaterial.SetVector("_WavePoint1", value);
			m_distortionMaterial.SetVector("_WavePoint2", value2);
			m_distortionMaterial.SetFloat("_DistortProgress", (Mathf.Sin(Time.realtimeSinceStartup * distortionPulseSpeed) + 1f) * distortionOffsetIncrease + minDistortionOffset);
		}
		Vector2 vector12 = new Vector2(m_currentBeamDistance * 16f, m_beamSprite.dimensions.y);
		if (vector12 != m_beamSprite.dimensions)
		{
			m_beamSprite.dimensions = vector12;
		}
		else
		{
			m_beamSprite.ForceBuild();
		}
		m_beamSprite.UpdateZDepth();
		m_previousAngle = num;
		for (int num20 = TimedIgnoreRigidbodies.Count - 1; num20 >= 0; num20--)
		{
			TimedIgnoreRigidbodies[num20].Second -= BraveTime.DeltaTime;
			if (TimedIgnoreRigidbodies[num20].Second <= 0f)
			{
				TimedIgnoreRigidbodies.RemoveAt(num20);
			}
		}
		return list;
	}

	private void DoDispersalParticles(Vector3 posStart, Vector3 posEnd, bool didImpact)
	{
		int num = Mathf.Max(Mathf.CeilToInt(Vector2.Distance(posStart.XY(), posEnd.XY()) * DispersalDensity), 1);
		for (int i = 0; i < num; i++)
		{
			float t = (float)i / (float)num;
			Vector3 position = Vector3.Lerp(posStart, posEnd, t);
			float num2 = Mathf.PerlinNoise(position.x / 3f, position.y / 3f);
			Vector3 a = Quaternion.Euler(0f, 0f, num2 * 360f) * Vector3.right;
			Vector3 vector = Vector3.Lerp(a, UnityEngine.Random.insideUnitSphere, UnityEngine.Random.Range(DispersalMinCoherency, DispersalMaxCoherency));
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			emitParams.position = position;
			emitParams.velocity = vector * m_dispersalParticles.startSpeed;
			emitParams.startSize = m_dispersalParticles.startSize;
			emitParams.startLifetime = m_dispersalParticles.startLifetime;
			emitParams.startColor = m_dispersalParticles.startColor;
			ParticleSystem.EmitParams emitParams2 = emitParams;
			m_dispersalParticles.Emit(emitParams2, 1);
		}
	}

	private void DoDispersalParticles(LinkedListNode<BeamBone> boneNode, int subtilesPerTile, bool didImpact)
	{
		if (!UsesDispersalParticles || boneNode.Value == null || boneNode.Next == null || boneNode.Next.Value == null)
		{
			return;
		}
		bool flag = boneNode == m_bones.First;
		Vector2 bonePosition = GetBonePosition(boneNode.Value);
		Vector3 vector = bonePosition.ToVector3ZUp(bonePosition.y);
		LinkedListNode<BeamBone> next = boneNode.Next;
		Vector2 bonePosition2 = GetBonePosition(next.Value);
		Vector3 vector2 = bonePosition2.ToVector3ZUp(bonePosition2.y);
		bool flag2 = next == m_bones.Last && didImpact;
		float num = ((!flag && !flag2) ? 1 : 3);
		int num2 = Mathf.Max(Mathf.CeilToInt(Vector2.Distance(vector.XY(), vector2.XY()) * DispersalDensity * num), 1);
		if (flag2)
		{
			num2 = Mathf.CeilToInt((float)num2 * DispersalExtraImpactFactor);
		}
		for (int i = 0; i < num2; i++)
		{
			float t = (float)i / (float)num2;
			if (flag)
			{
				t = Mathf.Lerp(0f, 0.5f, t);
			}
			if (flag2)
			{
				t = Mathf.Lerp(0.5f, 1f, t);
			}
			Vector3 position = Vector3.Lerp(vector, vector2, t);
			float num3 = Mathf.PerlinNoise(position.x / 3f, position.y / 3f);
			Vector3 a = Quaternion.Euler(0f, 0f, num3 * 360f) * Vector3.right;
			Vector3 vector3 = Vector3.Lerp(a, UnityEngine.Random.insideUnitSphere, UnityEngine.Random.Range(DispersalMinCoherency, DispersalMaxCoherency));
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			emitParams.position = position;
			emitParams.velocity = vector3 * m_dispersalParticles.startSpeed;
			emitParams.startSize = m_dispersalParticles.startSize;
			emitParams.startLifetime = m_dispersalParticles.startLifetime;
			emitParams.startColor = m_dispersalParticles.startColor;
			ParticleSystem.EmitParams emitParams2 = emitParams;
			m_dispersalParticles.Emit(emitParams2, 1);
		}
	}

	private void HandleIgnitionAndFreezing()
	{
		if (!base.projectile)
		{
			return;
		}
		if ((base.projectile.damageTypes | CoreDamageTypes.Ice) == base.projectile.damageTypes)
		{
			if (m_bones.Count > 2)
			{
				Vector3 vector = GetBonePosition(m_bones.First.Value);
				LinkedListNode<BeamBone> next = m_bones.First.Next;
				while (next != null)
				{
					Vector3 vector2 = GetBonePosition(next.Value);
					Vector2 p = vector.XY();
					Vector2 p2 = vector2.XY();
					DeadlyDeadlyGoopManager.FreezeGoopsLine(p, p2, 1f);
					next = next.Next;
					vector = vector2;
				}
			}
			else if (boneType == BeamBoneType.Straight)
			{
				Vector2 bonePosition = GetBonePosition(m_bones.First.Value);
				Vector2 p3 = bonePosition + BraveMathCollege.DegreesToVector(base.transform.eulerAngles.z).normalized * m_currentBeamDistance;
				DeadlyDeadlyGoopManager.FreezeGoopsLine(bonePosition, p3, 1f);
			}
		}
		if ((base.projectile.damageTypes | CoreDamageTypes.Fire) == base.projectile.damageTypes)
		{
			if (m_bones.Count > 2)
			{
				Vector3 vector3 = GetBonePosition(m_bones.First.Value);
				LinkedListNode<BeamBone> next2 = m_bones.First.Next;
				while (next2 != null)
				{
					Vector3 vector4 = GetBonePosition(next2.Value);
					Vector2 p4 = vector3.XY();
					Vector2 p5 = vector4.XY();
					DeadlyDeadlyGoopManager.IgniteGoopsLine(p4, p5, 1f);
					next2 = next2.Next;
					vector3 = vector4;
				}
			}
			else if (m_bones.Count != 2)
			{
			}
		}
		if ((base.projectile.damageTypes | CoreDamageTypes.Electric) != base.projectile.damageTypes)
		{
			return;
		}
		if (m_bones.Count > 2)
		{
			Vector3 vector5 = GetBonePosition(m_bones.First.Value);
			LinkedListNode<BeamBone> next3 = m_bones.First.Next;
			while (next3 != null)
			{
				Vector3 vector6 = GetBonePosition(next3.Value);
				Vector2 p6 = vector5.XY();
				Vector2 p7 = vector6.XY();
				DeadlyDeadlyGoopManager.ElectrifyGoopsLine(p6, p7, 1f);
				next3 = next3.Next;
				vector5 = vector6;
			}
		}
		else if (m_bones.Count != 2)
		{
		}
	}

	public void HandleGoopFrame(GoopModifier gooper)
	{
		if (gooper.IsSynergyContingent && !gooper.SynergyViable)
		{
			return;
		}
		if (gooper.SpawnGoopInFlight && m_bones.Count >= 2)
		{
			s_goopPoints.Clear();
			float num = (0f - collisionRadius) * m_projectileScale * PhysicsEngine.Instance.PixelUnitWidth;
			float num2 = collisionRadius * m_projectileScale * PhysicsEngine.Instance.PixelUnitWidth;
			int num3 = Mathf.Max(2, Mathf.CeilToInt((num2 - num) / 0.25f));
			for (LinkedListNode<BeamBone> linkedListNode = m_bones.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				Vector2 bonePosition = GetBonePosition(linkedListNode.Value);
				Vector2 vector = new Vector2(0f, 1f).Rotate(linkedListNode.Value.RotationAngle);
				for (int i = 0; i < num3; i++)
				{
					float num4 = Mathf.Lerp(num, num2, (float)i / (float)(num3 - 1));
					s_goopPoints.Add(bonePosition + vector * num4 + gooper.spawnOffset);
				}
			}
			gooper.Manager.AddGoopPoints(s_goopPoints, gooper.InFlightSpawnRadius, base.Owner.specRigidbody.UnitCenter, 1.75f);
		}
		if (gooper.SpawnAtBeamEnd)
		{
			Vector2 vector2 = ((m_bones.Count < 2 || !UsesBones) ? (base.Origin + base.Direction.normalized * m_currentBeamDistance) : GetBonePosition(m_bones.Last.Value));
			if (m_lastBeamEnd.HasValue)
			{
				gooper.Manager.AddGoopLine(m_lastBeamEnd.Value, vector2, gooper.BeamEndRadius);
			}
			m_lastBeamEnd = vector2;
		}
	}

	public override void LateUpdatePosition(Vector3 origin)
	{
		if (m_previousAngle.HasValue)
		{
			float z = origin.y - CurrentBeamHeightOffGround;
			base.transform.position = origin.WithZ(z).Quantize(0.0625f);
		}
		if (State == BeamState.Charging || State == BeamState.Telegraphing || State == BeamState.Firing || State == BeamState.Dissipating)
		{
			base.Origin = origin;
			FrameUpdate();
		}
	}

	private void CeaseAdditionalBehavior()
	{
		if (angularKnockback && m_hasToggledGunOutline && (bool)base.Gun && (bool)base.Gun.GetSprite())
		{
			m_hasToggledGunOutline = false;
			SpriteOutlineManager.AddOutlineToSprite(base.Gun.GetSprite(), Color.black);
		}
	}

	public override void CeaseAttack()
	{
		CeaseAdditionalBehavior();
		if (State == BeamState.Charging || State == BeamState.Telegraphing)
		{
			DestroyBeam();
			return;
		}
		if (endType == BeamEndType.Vanish)
		{
			DestroyBeam();
			return;
		}
		if (endType == BeamEndType.Dissipate)
		{
			State = BeamState.Dissipating;
			base.spriteAnimator.Play(CurrentBeamAnimation);
			m_dissipateTimer = 0f;
			SelfUpdate = true;
		}
		else if (endType == BeamEndType.Persist)
		{
			State = BeamState.Disconnected;
			if (ProjectileAndBeamMotionModule is OrbitProjectileMotionModule)
			{
				(ProjectileAndBeamMotionModule as OrbitProjectileMotionModule).BeamDestroyed();
			}
			SelfUpdate = true;
		}
		if (UsesChargeSprite || UsesMuzzleSprite)
		{
			m_muzzleTransform.gameObject.SetActive(false);
		}
	}

	public override void DestroyBeam()
	{
		if ((bool)m_reflectedBeam)
		{
			m_reflectedBeam.CeaseAttack();
			m_reflectedBeam = null;
		}
		if ((bool)m_enemyKnockback && m_enemyKnockbackId >= 0)
		{
			m_enemyKnockback.EndContinuousKnockback(m_enemyKnockbackId);
			m_enemyKnockback = null;
			m_enemyKnockbackId = -1;
		}
		if (doesScreenDistortion && m_distortionMaterial != null)
		{
			Pixelator.Instance.DeregisterAdditionalRenderPass(m_distortionMaterial);
		}
		if (GameManager.AUDIO_ENABLED && !string.IsNullOrEmpty(endAudioEvent))
		{
			AkSoundEngine.PostEvent(endAudioEvent, base.gameObject);
		}
		if (ProjectileAndBeamMotionModule is OrbitProjectileMotionModule)
		{
			(ProjectileAndBeamMotionModule as OrbitProjectileMotionModule).BeamDestroyed();
		}
		UnityEngine.Object.Destroy(base.transform.gameObject);
	}

	public override void AdjustPlayerBeamTint(Color targetTintColor, int priority, float lerpTime = 0f)
	{
		if (base.Owner is PlayerController && priority > m_currentTintPriority)
		{
			m_currentTintPriority = priority;
			ChangeTintColorShader(m_beamSprite, lerpTime, targetTintColor);
			if ((bool)m_beamMuzzleSprite)
			{
				ChangeTintColorShader(m_beamMuzzleSprite, lerpTime, targetTintColor);
			}
			if ((bool)m_impactSprite)
			{
				ChangeTintColorShader(m_impactSprite, lerpTime, targetTintColor);
			}
			if ((bool)m_impact2Sprite)
			{
				ChangeTintColorShader(m_impact2Sprite, lerpTime, targetTintColor);
			}
		}
	}

	private void ChangeTintColorShader(tk2dBaseSprite baseSprite, float time, Color color)
	{
		baseSprite.OverrideMaterialMode = tk2dBaseSprite.SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_SIMPLE;
		Material material = baseSprite.renderer.material;
		bool flag = material.HasProperty("_EmissivePower");
		float value = 0f;
		float value2 = 0f;
		if (flag)
		{
			value = material.GetFloat("_EmissivePower");
			value2 = material.GetFloat("_EmissiveColorPower");
		}
		Shader shader = null;
		shader = (flag ? ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive") : ShaderCache.Acquire("tk2d/CutoutVertexColorTintableTilted"));
		if (baseSprite.renderer.material.shader != shader)
		{
			baseSprite.renderer.material.shader = shader;
			baseSprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
			if (flag)
			{
				baseSprite.renderer.material.SetFloat("_EmissivePower", value);
				baseSprite.renderer.material.SetFloat("_EmissiveColorPower", value2);
			}
		}
		if (time == 0f)
		{
			baseSprite.renderer.sharedMaterial.SetColor("_OverrideColor", color);
		}
		else
		{
			StartCoroutine(ChangeTintColorCR(baseSprite, time, color));
		}
	}

	private IEnumerator ChangeTintColorCR(tk2dBaseSprite baseSprite, float time, Color color)
	{
		Material targetMaterial = baseSprite.renderer.sharedMaterial;
		float timer = 0f;
		while (timer < time)
		{
			targetMaterial.SetColor("_OverrideColor", Color.Lerp(Color.white, color, timer / time));
			timer += BraveTime.DeltaTime;
			yield return null;
		}
		targetMaterial.SetColor("_OverrideColor", color);
	}

	public void BecomeBlackBullet()
	{
		if (!IsBlackBullet && (bool)base.sprite)
		{
			IsBlackBullet = true;
			base.sprite.usesOverrideMaterial = true;
			base.sprite.renderer.material.SetFloat("_BlackBullet", 1f);
			base.sprite.renderer.material.SetFloat("_EmissivePower", -40f);
		}
	}

	public void ReturnFromBlackBullet()
	{
		if (IsBlackBullet)
		{
			IsBlackBullet = false;
			base.sprite.renderer.material.SetFloat("_BlackBullet", 0f);
			base.sprite.usesOverrideMaterial = false;
			base.sprite.ForceUpdateMaterial();
		}
	}

	public void GetTiledSpriteGeomDesc(out int numVertices, out int numIndices, tk2dSpriteDefinition spriteDef, Vector2 dimensions)
	{
		int num = Mathf.CeilToInt(dimensions.x / (float)m_beamQuadPixelWidth);
		if (TileType == BeamTileType.Flowing)
		{
			num = m_bones.Count - 1;
		}
		numVertices = num * 4;
		numIndices = num * 6;
	}

	public void SetTiledSpriteGeom(Vector3[] pos, Vector2[] uv, int offset, out Vector3 boundsCenter, out Vector3 boundsExtents, tk2dSpriteDefinition spriteDef, Vector3 scale, Vector2 dimensions, tk2dBaseSprite.Anchor anchor, float colliderOffsetZ, float colliderExtentZ)
	{
		boundsCenter = Vector3.zero;
		boundsExtents = Vector3.zero;
		int num = Mathf.RoundToInt(spriteDef.untrimmedBoundsDataExtents.x / spriteDef.texelSize.x);
		int num2 = num / m_beamQuadPixelWidth;
		int num3 = Mathf.CeilToInt(dimensions.x / (float)m_beamQuadPixelWidth);
		int num4 = Mathf.CeilToInt((float)num3 / (float)num2);
		if (TileType == BeamTileType.Flowing)
		{
			num3 = m_bones.Count - 1;
			num4 = m_bones.Count((BeamBone b) => b.SubtileNum == 0);
			if (m_bones.First.Value.SubtileNum != 0)
			{
				num4++;
			}
			if (m_bones.Last.Value.SubtileNum == 0)
			{
				num4--;
			}
		}
		Vector2 vector = new Vector2(dimensions.x * spriteDef.texelSize.x * scale.x, dimensions.y * spriteDef.texelSize.y * scale.y);
		Vector2 vector2 = Vector2.Scale(spriteDef.texelSize, scale) * 0.1f;
		int num5 = 0;
		Vector3 a = new Vector3((float)m_beamQuadPixelWidth * spriteDef.texelSize.x, spriteDef.untrimmedBoundsDataExtents.y, spriteDef.untrimmedBoundsDataExtents.z);
		a = Vector3.Scale(a, scale);
		Vector3 zero = Vector3.zero;
		Quaternion quaternion = Quaternion.Euler(0f, 0f, base.Direction.ToAngle());
		LinkedListNode<BeamBone> linkedListNode = m_bones.First;
		for (int i = 0; i < num4; i++)
		{
			int num6 = 0;
			int num7 = num2 - 1;
			if (TileType == BeamTileType.GrowAtBeginning)
			{
				if (i == 0 && num3 % num2 != 0)
				{
					num6 = num2 - num3 % num2;
				}
			}
			else if (TileType == BeamTileType.GrowAtEnd)
			{
				if (i == num4 - 1 && num3 % num2 != 0)
				{
					num7 = num3 % num2 - 1;
				}
			}
			else if (TileType == BeamTileType.Flowing)
			{
				if (i == 0)
				{
					num6 = linkedListNode.Value.SubtileNum;
				}
				if (i == num4 - 1)
				{
					num7 = m_bones.Last.Previous.Value.SubtileNum;
				}
			}
			tk2dSpriteDefinition tk2dSpriteDefinition2 = spriteDef;
			if (UsesBeamStartAnimation && i == 0)
			{
				tk2dSpriteAnimationClip clipByName = base.spriteAnimator.GetClipByName(CurrentBeamStartAnimation);
				tk2dSpriteDefinition2 = m_beamSprite.Collection.spriteDefinitions[clipByName.frames[Mathf.Min(clipByName.frames.Length - 1, base.spriteAnimator.CurrentFrame)].spriteId];
			}
			if (UsesBeamEndAnimation && i == num4 - 1)
			{
				tk2dSpriteAnimationClip clipByName2 = base.spriteAnimator.GetClipByName(CurrentBeamEndAnimation);
				tk2dSpriteDefinition2 = m_beamSprite.Collection.spriteDefinitions[clipByName2.frames[Mathf.Min(clipByName2.frames.Length - 1, base.spriteAnimator.CurrentFrame)].spriteId];
			}
			float num8 = 0f;
			if (i == 0)
			{
				if (TileType == BeamTileType.GrowAtBeginning)
				{
					num8 = 1f - Mathf.Abs(vector.x % (a.x * (float)num2)) / (a.x * (float)num2);
				}
				else if (TileType == BeamTileType.Flowing)
				{
					num8 = m_uvOffset;
				}
			}
			for (int j = num6; j <= num7; j++)
			{
				BeamBone beamBone = null;
				BeamBone beamBone2 = null;
				if (linkedListNode != null)
				{
					beamBone = linkedListNode.Value;
					if (linkedListNode.Next != null)
					{
						beamBone2 = linkedListNode.Next.Value;
					}
				}
				float num9 = 1f;
				if (TileType == BeamTileType.GrowAtBeginning)
				{
					if (i == 0 && j == 0 && (float)num3 * a.x >= Mathf.Abs(vector.x) + vector2.x)
					{
						num9 = Mathf.Abs(vector.x / a.x) - (float)(num3 - 1);
					}
				}
				else if (TileType == BeamTileType.GrowAtEnd)
				{
					if (Mathf.Abs(zero.x + a.x) > Mathf.Abs(vector.x) + vector2.x)
					{
						num9 = vector.x % a.x / a.x;
					}
				}
				else if (TileType == BeamTileType.Flowing)
				{
					if (i == 0 && linkedListNode == m_bones.First)
					{
						num9 = (beamBone2.PosX - beamBone.PosX) / m_beamQuadUnitWidth;
					}
					else if (i == num4 - 1 && linkedListNode.Next.Next == null)
					{
						num9 = (beamBone2.PosX - beamBone.PosX) / m_beamQuadUnitWidth;
					}
				}
				float z = 0f;
				if (RampHeightOffset != 0f && zero.x < 5f)
				{
					z = (1f - zero.x / 5f) * (0f - RampHeightOffset);
				}
				if (UsesBones && beamBone2 != null)
				{
					float rotationAngle = beamBone.RotationAngle;
					float num10 = beamBone2.RotationAngle;
					if (Mathf.Abs(BraveMathCollege.ClampAngle180(num10 - rotationAngle)) > 90f)
					{
						num10 = BraveMathCollege.ClampAngle360(num10 + 180f);
					}
					Vector2 bonePosition = GetBonePosition(beamBone);
					Vector2 bonePosition2 = GetBonePosition(beamBone2);
					int num11 = offset + num5;
					pos[num11++] = Quaternion.Euler(0f, 0f, rotationAngle) * Vector3.Scale(new Vector3(0f, tk2dSpriteDefinition2.position0.y * m_projectileScale, z), scale) + (Vector3)(bonePosition - base.transform.position.XY());
					pos[num11++] = Quaternion.Euler(0f, 0f, num10) * Vector3.Scale(new Vector3(0f, tk2dSpriteDefinition2.position1.y * m_projectileScale, z), scale) + (Vector3)(bonePosition2 - base.transform.position.XY());
					pos[num11++] = Quaternion.Euler(0f, 0f, rotationAngle) * Vector3.Scale(new Vector3(0f, tk2dSpriteDefinition2.position2.y * m_projectileScale, z), scale) + (Vector3)(bonePosition - base.transform.position.XY());
					pos[num11++] = Quaternion.Euler(0f, 0f, num10) * Vector3.Scale(new Vector3(0f, tk2dSpriteDefinition2.position3.y * m_projectileScale, z), scale) + (Vector3)(bonePosition2 - base.transform.position.XY());
				}
				else if (boneType == BeamBoneType.Straight)
				{
					int num12 = offset + num5;
					pos[num12++] = quaternion * (zero + Vector3.Scale(new Vector3(0f, tk2dSpriteDefinition2.position0.y * m_projectileScale, z), scale));
					pos[num12++] = quaternion * (zero + Vector3.Scale(new Vector3(num9 * a.x, tk2dSpriteDefinition2.position1.y * m_projectileScale, z), scale));
					pos[num12++] = quaternion * (zero + Vector3.Scale(new Vector3(0f, tk2dSpriteDefinition2.position2.y * m_projectileScale, z), scale));
					pos[num12++] = quaternion * (zero + Vector3.Scale(new Vector3(num9 * a.x, tk2dSpriteDefinition2.position3.y * m_projectileScale, z), scale));
				}
				Vector2 vector3 = Vector2.Lerp(tk2dSpriteDefinition2.uvs[0], tk2dSpriteDefinition2.uvs[1], num8);
				Vector2 vector4 = Vector2.Lerp(tk2dSpriteDefinition2.uvs[2], tk2dSpriteDefinition2.uvs[3], num8 + num9 / (float)num2);
				if (FlipBeamSpriteLocal && base.Direction.x < 0f)
				{
					float y = vector3.y;
					vector3.y = vector4.y;
					vector4.y = y;
				}
				int num13 = offset + num5;
				if (tk2dSpriteDefinition2.flipped == tk2dSpriteDefinition.FlipMode.Tk2d)
				{
					uv[num13++] = new Vector2(vector3.x, vector3.y);
					uv[num13++] = new Vector2(vector3.x, vector4.y);
					uv[num13++] = new Vector2(vector4.x, vector3.y);
					uv[num13++] = new Vector2(vector4.x, vector4.y);
				}
				else if (tk2dSpriteDefinition2.flipped == tk2dSpriteDefinition.FlipMode.TPackerCW)
				{
					uv[num13++] = new Vector2(vector3.x, vector3.y);
					uv[num13++] = new Vector2(vector4.x, vector3.y);
					uv[num13++] = new Vector2(vector3.x, vector4.y);
					uv[num13++] = new Vector2(vector4.x, vector4.y);
				}
				else
				{
					uv[num13++] = new Vector2(vector3.x, vector3.y);
					uv[num13++] = new Vector2(vector4.x, vector3.y);
					uv[num13++] = new Vector2(vector3.x, vector4.y);
					uv[num13++] = new Vector2(vector4.x, vector4.y);
				}
				num5 += 4;
				zero.x += a.x * num9;
				num8 += num9 / (float)m_beamSpriteSubtileWidth;
				if (linkedListNode != null)
				{
					linkedListNode = linkedListNode.Next;
				}
			}
		}
		Vector3 vector5 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector6 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		for (int k = 0; k < pos.Length; k++)
		{
			vector5 = Vector3.Min(vector5, pos[k]);
			vector6 = Vector3.Max(vector6, pos[k]);
		}
		Vector3 vector7 = (vector6 - vector5) / 2f;
		boundsCenter = vector5 + vector7;
		boundsExtents = vector7;
	}

	private bool FindBeamTarget(Vector2 origin, Vector2 direction, float distance, int collisionMask, out Vector2 targetPoint, out Vector2 targetNormal, out SpeculativeRigidbody hitRigidbody, out PixelCollider hitPixelCollider, out List<PointcastResult> boneCollisions, Func<SpeculativeRigidbody, bool> rigidbodyExcluder = null, params SpeculativeRigidbody[] ignoreRigidbodies)
	{
		bool flag = false;
		targetPoint = new Vector2(-1f, -1f);
		targetNormal = new Vector2(0f, 0f);
		hitRigidbody = null;
		hitPixelCollider = null;
		if (collisionType == BeamCollisionType.Rectangle)
		{
			if (!base.specRigidbody)
			{
				base.specRigidbody = base.gameObject.AddComponent<SpeculativeRigidbody>();
				base.specRigidbody.CollideWithTileMap = false;
				base.specRigidbody.CollideWithOthers = true;
				PixelCollider pixelCollider = new PixelCollider();
				pixelCollider.Enabled = false;
				pixelCollider.CollisionLayer = CollisionLayer.PlayerBlocker;
				pixelCollider.Enabled = true;
				pixelCollider.IsTrigger = true;
				pixelCollider.ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual;
				pixelCollider.ManualOffsetX = 0;
				pixelCollider.ManualOffsetY = collisionWidth / -2;
				pixelCollider.ManualWidth = collisionLength;
				pixelCollider.ManualHeight = collisionWidth;
				base.specRigidbody.PixelColliders = new List<PixelCollider>(1);
				base.specRigidbody.PixelColliders.Add(pixelCollider);
				base.specRigidbody.Initialize();
			}
			if (m_cachedRectangleOrigin != origin || m_cachedRectangleDirection != direction)
			{
				base.specRigidbody.Position = new Position(origin);
				base.specRigidbody.PrimaryPixelCollider.SetRotationAndScale(direction.ToAngle(), Vector2.one);
				base.specRigidbody.UpdateColliderPositions();
				m_cachedRectangleOrigin = origin;
				m_cachedRectangleDirection = direction;
			}
			int num = CollisionMask.LayerToMask(CollisionLayer.PlayerHitBox);
			if ((collisionMask & num) == num)
			{
				base.specRigidbody.PrimaryPixelCollider.CollisionLayerIgnoreOverride &= ~CollisionMask.LayerToMask(CollisionLayer.PlayerHitBox, CollisionLayer.PlayerCollider);
			}
			else
			{
				base.specRigidbody.PrimaryPixelCollider.CollisionLayerIgnoreOverride |= CollisionMask.LayerToMask(CollisionLayer.PlayerHitBox, CollisionLayer.PlayerCollider);
			}
			List<CollisionData> list = new List<CollisionData>();
			base.specRigidbody.PrimaryPixelCollider.Enabled = true;
			flag = PhysicsEngine.Instance.OverlapCast(base.specRigidbody, list, false, true, null, null, false, null, null, ignoreRigidbodies);
			base.specRigidbody.PrimaryPixelCollider.Enabled = false;
			boneCollisions = new List<PointcastResult>();
			if (!flag)
			{
				return false;
			}
			targetNormal = list[0].Normal;
			targetPoint = list[0].Contact;
			hitRigidbody = list[0].OtherRigidbody;
			hitPixelCollider = list[0].OtherPixelCollider;
		}
		else if (UsesBones)
		{
			float num2 = (0f - collisionRadius) * m_projectileScale * PhysicsEngine.Instance.PixelUnitWidth;
			float num3 = collisionRadius * m_projectileScale * PhysicsEngine.Instance.PixelUnitWidth;
			int num4 = Mathf.Max(2, Mathf.CeilToInt((num3 - num2) / 0.25f));
			int ignoreTileBoneCount;
			List<IntVector2> points = GeneratePixelCloud(num2, num3, num4, out ignoreTileBoneCount);
			List<IntVector2> lastFramePoints = GenerateLastPixelCloud(num2, num3, num4);
			if (!PhysicsEngine.Instance.Pointcast(points, lastFramePoints, num4, out boneCollisions, true, true, collisionMask, CollisionLayer.Projectile, false, rigidbodyExcluder, ignoreTileBoneCount, ignoreRigidbodies))
			{
				return false;
			}
			PointcastResult pointcastResult = boneCollisions[0];
			for (int i = 0; i < boneCollisions.Count; i++)
			{
				if (boneCollisions[i].hitDirection == HitDirection.Forward && boneCollisions[i].boneIndex > 0)
				{
					pointcastResult = boneCollisions[i];
					break;
				}
			}
			targetPoint = pointcastResult.hitResult.Contact;
			targetNormal = pointcastResult.hitResult.Normal;
			hitRigidbody = pointcastResult.hitResult.SpeculativeRigidbody;
			hitPixelCollider = pointcastResult.hitResult.OtherPixelCollider;
		}
		else
		{
			float num5 = (0f - collisionRadius) * m_projectileScale * PhysicsEngine.Instance.PixelUnitWidth;
			float num6 = collisionRadius * m_projectileScale * PhysicsEngine.Instance.PixelUnitWidth;
			int num7 = Mathf.Max(2, Mathf.CeilToInt((num6 - num5) / 0.25f));
			RaycastResult obj = null;
			for (int j = 0; j < num7; j++)
			{
				float y = Mathf.Lerp(num5, num6, (float)j / (float)(num7 - 1));
				Vector2 unitOrigin = origin + new Vector2(0f, y).Rotate(direction.ToAngle());
				RaycastResult result;
				if (PhysicsEngine.Instance.RaycastWithIgnores(unitOrigin, direction.normalized, distance, out result, true, true, collisionMask, CollisionLayer.Projectile, false, rigidbodyExcluder, ignoreRigidbodies))
				{
					flag = true;
					if (obj == null || result.Distance < obj.Distance)
					{
						RaycastResult.Pool.Free(ref obj);
						obj = result;
					}
					else
					{
						RaycastResult.Pool.Free(ref result);
					}
				}
			}
			boneCollisions = new List<PointcastResult>();
			if (!flag)
			{
				return false;
			}
			targetNormal = obj.Normal;
			targetPoint = origin + BraveMathCollege.DegreesToVector(direction.ToAngle(), obj.Distance);
			hitRigidbody = obj.SpeculativeRigidbody;
			hitPixelCollider = obj.OtherPixelCollider;
			RaycastResult.Pool.Free(ref obj);
		}
		if (hitRigidbody == null)
		{
			return true;
		}
		if ((bool)hitRigidbody.minorBreakable && !hitRigidbody.minorBreakable.OnlyBrokenByCode)
		{
			hitRigidbody.minorBreakable.Break(direction);
		}
		DebrisObject component = hitRigidbody.GetComponent<DebrisObject>();
		if ((bool)component)
		{
			component.Trigger(direction, 0.5f);
		}
		TorchController component2 = hitRigidbody.GetComponent<TorchController>();
		if ((bool)component2)
		{
			component2.BeamCollision(base.projectile);
		}
		if ((bool)hitRigidbody.projectile && hitRigidbody.projectile.collidesWithProjectiles)
		{
			hitRigidbody.projectile.BeamCollision(base.projectile);
		}
		return true;
	}

	private List<IntVector2> GeneratePixelCloud(float minOffset, float maxOffset, float numOffsets, out int ignoreTileBoneCount)
	{
		ignoreTileBoneCount = -1;
		bool flag = false;
		s_pixelCloud.Clear();
		for (LinkedListNode<BeamBone> linkedListNode = m_bones.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			Vector2 bonePosition = GetBonePosition(linkedListNode.Value);
			Vector2 vector = new Vector2(0f, 1f).Rotate(linkedListNode.Value.RotationAngle);
			if (IgnoreTilesDistance > 0f && !flag && linkedListNode.Value.PosX > IgnoreTilesDistance)
			{
				ignoreTileBoneCount = s_pixelCloud.Count;
				flag = true;
			}
			for (int i = 0; (float)i < numOffsets; i++)
			{
				float num = Mathf.Lerp(minOffset, maxOffset, (float)i / (numOffsets - 1f));
				s_pixelCloud.Add(PhysicsEngine.UnitToPixel(bonePosition + vector * num));
			}
		}
		if (IgnoreTilesDistance > 0f && !flag)
		{
			ignoreTileBoneCount = s_pixelCloud.Count;
			flag = true;
		}
		return s_pixelCloud;
	}

	private List<IntVector2> GenerateLastPixelCloud(float minOffset, float maxOffset, float numOffsets)
	{
		s_lastPixelCloud.Clear();
		for (LinkedListNode<BeamBone> linkedListNode = m_bones.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
		{
			Vector2 vector = GetBonePosition(linkedListNode.Value) - linkedListNode.Value.Velocity * BraveTime.DeltaTime;
			Vector2 vector2 = new Vector2(0f, 1f).Rotate(linkedListNode.Value.RotationAngle);
			for (int i = 0; (float)i < numOffsets; i++)
			{
				float num = Mathf.Lerp(minOffset, maxOffset, (float)i / (numOffsets - 1f));
				s_lastPixelCloud.Add(PhysicsEngine.UnitToPixel(vector + vector2 * num));
			}
		}
		return s_lastPixelCloud;
	}

	private Vector2 GetBonePosition(BeamBone bone)
	{
		if (UsesBones)
		{
			if (ProjectileAndBeamMotionModule != null)
			{
				return bone.Position + ProjectileAndBeamMotionModule.GetBoneOffset(bone, this, base.projectile.Inverted);
			}
			return bone.Position;
		}
		return base.Origin + BraveMathCollege.DegreesToVector(base.Direction.ToAngle(), bone.PosX);
	}

	public Vector2 GetPointOnBeam(float t)
	{
		if (m_bones.Count < 2)
		{
			return base.Origin;
		}
		if (UsesBones)
		{
			return base.Origin + base.Direction.normalized * (m_bones.Last.Value.Position - m_bones.First.Value.Position).magnitude * t;
		}
		return base.Origin + base.Direction.normalized * m_currentBeamDistance * t;
	}

	private void SeparateBeam(LinkedListNode<BeamBone> startNode, Vector2 newOrigin, float newPosX)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(base.gameObject);
		gameObject.name = base.gameObject.name + " (Split)";
		BasicBeamController component = gameObject.GetComponent<BasicBeamController>();
		component.State = BeamState.Disconnected;
		component.m_bones = new LinkedList<BeamBone>();
		component.SelfUpdate = true;
		component.projectile.Owner = base.projectile.Owner;
		component.Owner = base.Owner;
		component.Gun = base.Gun;
		component.HitsPlayers = base.HitsPlayers;
		component.HitsEnemies = base.HitsEnemies;
		component.Origin = base.Origin;
		component.Direction = base.Direction;
		component.DamageModifier = base.DamageModifier;
		component.GetComponent<tk2dTiledSprite>().dimensions = m_beamSprite.dimensions;
		component.m_previousAngle = m_previousAngle;
		component.m_currentBeamDistance = m_currentBeamDistance;
		component.reflections = reflections;
		component.Origin = newOrigin;
		BeamBone beamBone = new BeamBone(startNode.Previous.Value);
		beamBone.Position = newOrigin;
		beamBone.PosX = newPosX;
		component.m_bones.AddFirst(beamBone);
		LinkedListNode<BeamBone> previous = startNode.Previous;
		while (previous.Next != null)
		{
			LinkedListNode<BeamBone> next = previous.Next;
			m_bones.Remove(next);
			component.m_bones.AddLast(next);
		}
	}

	private BasicBeamController CreateReflectedBeam(Vector2 pos, Vector2 dir, bool decrementReflections = true)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(base.gameObject);
		gameObject.name = base.gameObject.name + " (Reflect)";
		BasicBeamController component = gameObject.GetComponent<BasicBeamController>();
		component.State = BeamState.Firing;
		component.IsReflectedBeam = true;
		component.Owner = base.Owner;
		component.Gun = base.Gun;
		component.HitsPlayers = base.HitsPlayers;
		component.HitsEnemies = base.HitsEnemies;
		component.Origin = pos;
		component.Direction = dir;
		component.DamageModifier = base.DamageModifier;
		component.usesChargeDelay = false;
		component.muzzleAnimation = string.Empty;
		component.chargeAnimation = string.Empty;
		component.beamStartAnimation = string.Empty;
		component.IgnoreTilesDistance = -1f;
		component.reflections = reflections;
		if (decrementReflections)
		{
			component.reflections--;
		}
		component.projectile.Owner = base.projectile.Owner;
		component.playerStatsModified = playerStatsModified;
		return component;
	}

	private tk2dBaseSprite CreatePierceImpactEffect()
	{
		GameObject gameObject = new GameObject("beam pierce impact vfx");
		Transform transform = gameObject.transform;
		transform.parent = base.transform;
		transform.localPosition = new Vector3(0f, 0f, 0.05f);
		transform.localScale = new Vector3(m_projectileScale, m_projectileScale, 1f);
		tk2dSprite tk2dSprite2 = gameObject.AddComponent<tk2dSprite>();
		tk2dSprite2.SetSprite(m_beamSprite.Collection, m_beamSprite.spriteId);
		tk2dSpriteAnimator tk2dSpriteAnimator2 = gameObject.AddComponent<tk2dSpriteAnimator>();
		tk2dSpriteAnimator2.SetSprite(m_beamSprite.Collection, m_beamSprite.spriteId);
		tk2dSpriteAnimator2.Library = base.spriteAnimator.Library;
		tk2dSpriteAnimator2.Play(impactAnimation);
		m_beamSprite.AttachRenderer(tk2dSprite2);
		tk2dSprite2.HeightOffGround = 0.05f;
		tk2dSprite2.IsPerpendicular = true;
		tk2dSprite2.usesOverrideMaterial = true;
		return tk2dSprite2;
	}

	public static void SetGlobalBeamHeight(float newDepth)
	{
		CurrentBeamHeightOffGround = newDepth;
	}

	public static void ResetGlobalBeamHeight()
	{
		CurrentBeamHeightOffGround = 0.75f;
	}
}
