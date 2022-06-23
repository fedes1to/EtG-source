using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class DebrisObject : EphemeralObject
{
	[Serializable]
	public struct DebrisPlacementOptions
	{
		public bool canBeRotated;

		public bool canBeFlippedHorizontally;

		public bool canBeFlippedVertically;
	}

	public enum DebrisFollowupAction
	{
		None,
		FollowupAnimation,
		GroundedAnimation,
		GroundedSprite,
		StopAnimationOnGrounded
	}

	protected class PitFallPoint
	{
		public CellData cellData;

		public Vector3 position;

		public bool inPit;

		public PitFallPoint(CellData cellData, Vector3 position)
		{
			this.cellData = cellData;
			this.position = position;
		}
	}

	public static List<SpeculativeRigidbody> SRB_Walls = new List<SpeculativeRigidbody>();

	public static List<SpeculativeRigidbody> SRB_Pits = new List<SpeculativeRigidbody>();

	private float ACCURATE_DEBRIS_THRESHOLD = 0.25f;

	public string audioEventName;

	[NonSerialized]
	public bool IsCorpse;

	public bool playAnimationOnTrigger;

	public bool usesDirectionalFallAnimations;

	[ShowInInspectorIf("usesDirectionalFallAnimations", false)]
	public DebrisDirectionalAnimationInfo directionalAnimationData;

	public bool breaksOnFall = true;

	[ShowInInspectorIf("breaksOnFall", false)]
	public float breakOnFallChance = 1f;

	public bool changesCollisionLayer;

	public CollisionLayer groundedCollisionLayer = CollisionLayer.LowObstacle;

	public DebrisFollowupAction followupBehavior;

	public string followupIdentifier;

	public bool collisionStopsBullets;

	public bool animatePitFall;

	public bool pitFallSplash;

	public float inertialMass = 1f;

	public float motionMultiplier = 1f;

	public bool canRotate = true;

	public float angularVelocity = 360f;

	public float angularVelocityVariance;

	public int bounceCount = 1;

	public float additionalBounceEnglish;

	public float decayOnBounce = 0.5f;

	public GameObject optionalBounceVFX;

	public tk2dSprite shadowSprite;

	[HideInInspector]
	public bool killTranslationOnBounce;

	public Action<DebrisObject> OnTouchedGround;

	public Action<DebrisObject> OnBounced;

	public Action<DebrisObject> OnGrounded;

	public bool usesLifespan;

	public float lifespanMin = 1f;

	public float lifespanMax = 1f;

	public bool shouldUseSRBMotion;

	public bool removeSRBOnGrounded;

	[NonSerialized]
	public bool PreventFallingInPits;

	public DebrisPlacementOptions placementOptions;

	public bool DoesGoopOnRest;

	[ShowInInspectorIf("DoesGoopOnRest", false)]
	public GoopDefinition AssignedGoop;

	[ShowInInspectorIf("DoesGoopOnRest", false)]
	public float GoopRadius = 1f;

	[HideInInspector]
	public MinorBreakableGroupManager groupManager;

	[HideInInspector]
	public float additionalHeightBoost;

	[HideInInspector]
	public List<ParticleSystem> detachedParticleSystems;

	public Action OnTriggered;

	protected Bounds m_spriteBounds;

	protected float m_currentLifespan;

	protected float m_initialWorldDepth;

	[SerializeField]
	protected float m_finalWorldDepth = -1.5f;

	protected float m_startingHeightOffGround;

	protected bool m_hasBeenTriggered;

	protected bool isStatic = true;

	protected bool doesDecay;

	protected Vector3 m_startPosition;

	protected Vector3 m_velocity;

	protected Vector3 m_frameVelocity;

	protected Vector3 m_currentPosition;

	protected static PitFallPoint[] m_STATIC_PitfallPoints;

	protected Transform m_transform;

	protected Renderer m_renderer;

	protected bool onGround;

	protected bool isFalling;

	protected bool isPitFalling;

	[NonSerialized]
	public bool PreventAbsorption;

	protected bool m_isPickupObject;

	protected bool m_forceUseFinalDepth;

	protected bool accurateDebris;

	protected bool m_recentlyBouncedOffTopwall;

	protected bool m_wasFacewallFixed;

	protected bool m_collisionsInitialized;

	protected bool m_forceCheckGrounded;

	protected bool m_isOnScreen = true;

	protected Dungeon m_dungeonRef;

	public bool ForceUpdateIfDisabled;

	private static int fgNonsenseLayerID = -1;

	private SpeculativeRigidbody m_platform;

	public bool Static
	{
		get
		{
			return isStatic;
		}
	}

	public float GravityOverride { get; set; }

	public bool HasBeenTriggered
	{
		get
		{
			return m_hasBeenTriggered;
		}
	}

	public bool IsPickupObject
	{
		get
		{
			return m_isPickupObject;
		}
	}

	public bool IsAccurateDebris
	{
		get
		{
			return accurateDebris;
		}
		set
		{
			accurateDebris = value;
		}
	}

	public bool DontSetLayer { get; set; }

	public Vector3 UnadjustedDebrisPosition
	{
		get
		{
			return m_currentPosition;
		}
	}

	public static void ClearPerLevelData()
	{
		StaticReferenceManager.AllDebris.Clear();
		m_STATIC_PitfallPoints = null;
		SRB_Pits.Clear();
		SRB_Walls.Clear();
		m_STATIC_PitfallPoints = null;
	}

	public void ForceUpdatePitfall()
	{
		m_forceCheckGrounded = true;
	}

	protected override void Awake()
	{
		base.Awake();
		if (m_STATIC_PitfallPoints == null)
		{
			m_STATIC_PitfallPoints = new PitFallPoint[5];
			for (int i = 0; i < 5; i++)
			{
				m_STATIC_PitfallPoints[i] = new PitFallPoint(null, Vector3.zero);
			}
		}
		m_dungeonRef = GameManager.Instance.Dungeon;
		StaticReferenceManager.AllDebris.Add(this);
	}

	public override void Start()
	{
		base.Start();
		if (fgNonsenseLayerID == -1)
		{
			fgNonsenseLayerID = LayerMask.NameToLayer("FG_Nonsense");
		}
		base.sprite.gameObject.SetLayerRecursively(fgNonsenseLayerID);
		m_spriteBounds = base.sprite.GetBounds();
		if (!m_isPickupObject)
		{
			m_isPickupObject = GetComponent<PickupObject>() != null;
		}
		if (m_isPickupObject || m_spriteBounds.size.x > ACCURATE_DEBRIS_THRESHOLD || m_spriteBounds.size.y > ACCURATE_DEBRIS_THRESHOLD)
		{
			accurateDebris = true;
		}
		if (base.sprite == null)
		{
			base.sprite = GetComponentInChildren<tk2dSprite>();
		}
		if (base.sprite != null)
		{
			DepthLookupManager.AssignRendererToSortingLayer(base.sprite.renderer, DepthLookupManager.GungeonSortingLayer.PLAYFIELD);
		}
		if (base.specRigidbody != null && GetComponent<MinorBreakable>() == null)
		{
			InitializeForCollisions();
		}
		if (!shouldUseSRBMotion && !DontSetLayer)
		{
			base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("FG_Nonsense"));
		}
	}

	public override void OnDespawned()
	{
		m_hasBeenTriggered = false;
		base.OnDespawned();
	}

	protected override void OnDestroy()
	{
		StaticReferenceManager.AllDebris.Remove(this);
		if ((bool)base.specRigidbody)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
		}
		base.OnDestroy();
	}

	public void FlagAsPickup()
	{
		m_isPickupObject = true;
	}

	public void AssignFinalWorldDepth(float depth)
	{
		m_finalWorldDepth = depth;
		m_forceUseFinalDepth = true;
	}

	public void InitializeForCollisions()
	{
		if (!m_collisionsInitialized)
		{
			m_collisionsInitialized = true;
			if (base.specRigidbody != null && GetComponent<MinorBreakable>() == null)
			{
				SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
				speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
			}
		}
	}

	public void OnPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherCollider)
	{
		if (!m_hasBeenTriggered)
		{
			shouldUseSRBMotion = true;
			Vector2 normalized = otherRigidbody.Velocity.normalized;
			float magnitude = otherRigidbody.Velocity.magnitude;
			magnitude = Mathf.Min(magnitude, 5f);
			Vector2 vector = normalized * magnitude;
			float z = Mathf.Lerp(-30f, 30f, UnityEngine.Random.value);
			Vector3 startingForce = Quaternion.Euler(0f, 0f, z) * (vector.normalized * UnityEngine.Random.Range(magnitude * 0.75f, magnitude * 1.25f)).ToVector3ZUp(1f);
			Trigger(startingForce, 0.5f);
			if (!collisionStopsBullets)
			{
				PhysicsEngine.SkipCollision = true;
			}
		}
		else
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
		}
	}

	public void OnAnimationCompleted(tk2dSpriteAnimator a, tk2dSpriteAnimationClip c)
	{
		if (followupBehavior == DebrisFollowupAction.FollowupAnimation)
		{
			base.spriteAnimator.Play(followupIdentifier);
		}
		base.spriteAnimator.AnimationCompleted = null;
	}

	public void ForceReinitializePosition()
	{
		Vector2 vector = m_transform.position.XY();
		m_startPosition = new Vector3(vector.x, vector.y - m_startingHeightOffGround, m_startingHeightOffGround);
		m_currentPosition = m_startPosition;
	}

	public void Trigger(Vector3 startingForce, float startingHeight, float angularVelocityModifier = 1f)
	{
		if (m_hasBeenTriggered)
		{
			return;
		}
		if (base.specRigidbody != null && base.specRigidbody.enabled)
		{
			shouldUseSRBMotion = true;
			if (base.specRigidbody.PrimaryPixelCollider.CollisionLayer == CollisionLayer.BulletBlocker || base.specRigidbody.PrimaryPixelCollider.CollisionLayer == CollisionLayer.BulletBreakable)
			{
				base.specRigidbody.CollideWithOthers = false;
			}
		}
		else if (base.specRigidbody == null)
		{
			shouldUseSRBMotion = false;
		}
		if (groupManager != null)
		{
			groupManager.DeregisterDebris(this);
		}
		m_transform = base.transform;
		m_renderer = base.renderer;
		if (base.sprite == null)
		{
			base.sprite = GetComponentInChildren<tk2dSprite>();
		}
		m_initialWorldDepth = base.sprite.HeightOffGround;
		m_startingHeightOffGround = startingHeight;
		Vector2 vector = m_transform.position.XY();
		m_startPosition = new Vector3(vector.x, vector.y - startingHeight, startingHeight);
		m_currentPosition = m_startPosition;
		m_velocity = startingForce / inertialMass;
		if (usesLifespan)
		{
			m_currentLifespan = UnityEngine.Random.Range(lifespanMin, lifespanMax);
		}
		angularVelocity = (canRotate ? (angularVelocity + UnityEngine.Random.Range(0f - angularVelocityVariance, angularVelocityVariance)) : 0f);
		angularVelocity *= angularVelocityModifier;
		m_hasBeenTriggered = true;
		isStatic = false;
		if (followupBehavior == DebrisFollowupAction.FollowupAnimation && !string.IsNullOrEmpty(followupIdentifier))
		{
			tk2dSpriteAnimator obj = base.spriteAnimator;
			obj.AnimationCompleted = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>)Delegate.Combine(obj.AnimationCompleted, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip>(OnAnimationCompleted));
			base.spriteAnimator.Play();
		}
		else if (playAnimationOnTrigger)
		{
			if (usesDirectionalFallAnimations)
			{
				base.spriteAnimator.Play(directionalAnimationData.GetAnimationForVector(startingForce.XY()));
			}
			else
			{
				base.spriteAnimator.Play();
			}
		}
		if (OnTriggered != null)
		{
			OnTriggered();
		}
	}

	public void ClearVelocity()
	{
		m_velocity = Vector3.zero;
		m_frameVelocity = Vector3.zero;
	}

	public void ApplyFrameVelocity(Vector2 vel)
	{
		if (base.enabled && m_hasBeenTriggered && !(m_currentPosition.z > 0f))
		{
			doesDecay = true;
			isStatic = false;
			m_frameVelocity += new Vector3(vel.x, vel.y, 0f) / inertialMass;
		}
	}

	public void ApplyVelocity(Vector2 vel)
	{
		if (base.enabled && m_hasBeenTriggered && !(m_currentPosition.z > 0f))
		{
			doesDecay = true;
			isStatic = false;
			angularVelocity = 0f;
			if (canRotate)
			{
				angularVelocity = UnityEngine.Random.Range(30, 90);
			}
			m_velocity += new Vector3(vel.x, vel.y, 0f) / inertialMass;
		}
	}

	protected CellData GetCellFromPosition(Vector3 p)
	{
		if (m_dungeonRef == null)
		{
			m_dungeonRef = GameManager.Instance.Dungeon;
		}
		IntVector2 intVector = p.IntXY(VectorConversions.Floor);
		if (!m_dungeonRef.data.CheckInBounds(intVector))
		{
			return null;
		}
		return m_dungeonRef.data[intVector];
	}

	protected bool CheckPositionFacewall(Vector3 position)
	{
		CellData cellFromPosition = GetCellFromPosition(position);
		if (cellFromPosition != null && cellFromPosition.IsAnyFaceWall())
		{
			return true;
		}
		for (int i = 0; i < SRB_Walls.Count; i++)
		{
			if (SRB_Walls[i].ContainsPoint(position))
			{
				return true;
			}
		}
		return false;
	}

	protected Tuple<CellData, Vector3> GetCellPositionTupleFromPosition(Vector3 p)
	{
		return Tuple.Create(GetCellFromPosition(p), p);
	}

	protected bool CheckCurrentCellsFacewall(Vector3 currentPosition)
	{
		Quaternion rotation = m_transform.rotation;
		currentPosition += rotation * m_spriteBounds.min;
		if (CheckPositionFacewall(currentPosition + rotation * (0.5f * m_spriteBounds.size)))
		{
			return true;
		}
		if (accurateDebris)
		{
			if (CheckPositionFacewall(currentPosition))
			{
				return true;
			}
			if (CheckPositionFacewall(currentPosition + rotation * new Vector3(m_spriteBounds.size.x, 0f, 0f)))
			{
				return true;
			}
			if (CheckPositionFacewall(currentPosition + rotation * m_spriteBounds.size))
			{
				return true;
			}
			if (CheckPositionFacewall(currentPosition + rotation * new Vector3(0f, m_spriteBounds.size.y, 0f)))
			{
				return true;
			}
		}
		return false;
	}

	private void RecalculateStaticTargetCells_ProcessPosition(Vector3 position, int index, PitFallPoint[] targetArray)
	{
		PitFallPoint pitFallPoint = targetArray[index];
		CellData cellData = (pitFallPoint.cellData = GetCellFromPosition(position));
		pitFallPoint.position = position;
		pitFallPoint.inPit = false;
	}

	protected void RecalculateStaticTargetCells(Vector3 newPosition, Quaternion newRotation)
	{
		if (m_STATIC_PitfallPoints != null)
		{
			newPosition.z = 0f;
			newPosition += newRotation * m_spriteBounds.min;
			RecalculateStaticTargetCells_ProcessPosition(newPosition + newRotation * (0.5f * m_spriteBounds.size), 0, m_STATIC_PitfallPoints);
			if (accurateDebris)
			{
				RecalculateStaticTargetCells_ProcessPosition(newPosition, 1, m_STATIC_PitfallPoints);
				RecalculateStaticTargetCells_ProcessPosition(newPosition + newRotation * new Vector3(m_spriteBounds.size.x, 0f, 0f), 2, m_STATIC_PitfallPoints);
				RecalculateStaticTargetCells_ProcessPosition(newPosition + newRotation * m_spriteBounds.size, 3, m_STATIC_PitfallPoints);
				RecalculateStaticTargetCells_ProcessPosition(newPosition + newRotation * new Vector3(0f, m_spriteBounds.size.y, 0f), 4, m_STATIC_PitfallPoints);
			}
		}
	}

	protected void HandleRotation(float adjustedDeltaTime)
	{
		if (canRotate)
		{
			int num = ((!(m_velocity.x > 0f)) ? 1 : (-1));
			m_transform.RotateAround(base.sprite.WorldCenter, Vector3.forward, angularVelocity * adjustedDeltaTime * (float)num);
			if (IsPickupObject)
			{
				base.sprite.ForceRotationRebuild();
			}
		}
	}

	protected virtual void UpdateVelocity(float adjustedDeltaTime)
	{
		if (m_currentPosition.z > 0f)
		{
			m_velocity += new Vector3(0f, 0f, -1f) * ((GravityOverride == 0f) ? 10f : GravityOverride) * adjustedDeltaTime;
		}
	}

	protected void HandleWallOrPitDeflection(IntVector2 currentGridCell, CellData nextCell, float adjustedDeltaTime)
	{
		if (base.name.Contains("Bomb"))
		{
			Debug.Log("deflecto detecto");
		}
		if (nextCell.IsAnyFaceWall() && !m_recentlyBouncedOffTopwall)
		{
			if (nextCell.position.x != currentGridCell.x)
			{
				m_velocity.x = (0f - m_velocity.x) * (1f - decayOnBounce);
			}
			m_velocity.y = (0f - Mathf.Abs(m_velocity.y)) * (1f - decayOnBounce);
			m_frameVelocity = Vector3.zero;
			return;
		}
		if (nextCell.position.x != currentGridCell.x)
		{
			m_velocity.x = (0f - m_velocity.x) * (1f - decayOnBounce);
			m_frameVelocity = Vector3.zero;
		}
		if (nextCell.position.y != currentGridCell.y)
		{
			m_velocity.y = (0f - m_velocity.y) * (1f - decayOnBounce);
			m_frameVelocity = Vector3.zero;
		}
	}

	public void IncrementZHeight(float amount)
	{
		if (HasBeenTriggered)
		{
			isStatic = false;
			m_currentPosition.z += amount;
		}
	}

	protected void ConvertYToZHeight(float amount)
	{
		m_currentPosition.y -= amount;
		m_currentPosition.z += amount;
	}

	protected bool CheckPitfallPointsForPit(ref PitFallPoint[] p, ref SpeculativeRigidbody newPlatform)
	{
		if (!m_transform || m_STATIC_PitfallPoints == null)
		{
			return false;
		}
		RecalculateStaticTargetCells(m_transform.position, m_transform.rotation);
		p = m_STATIC_PitfallPoints;
		int num = ((!accurateDebris) ? 1 : 5);
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < num; i++)
		{
			CellData cellData = p[i].cellData;
			if (cellData == null)
			{
				continue;
			}
			if (cellData.type == CellType.PIT && !PreventFallingInPits)
			{
				SpeculativeRigidbody platform = null;
				if (!GameManager.Instance.Dungeon.IsPixelOnPlatform(p[i].position, out platform))
				{
					num2++;
					p[i].inPit = true;
					if (cellData.fallingPrevented)
					{
						num3++;
					}
					continue;
				}
				newPlatform = platform;
			}
			if (SRB_Pits == null || PreventFallingInPits)
			{
				continue;
			}
			for (int j = 0; j < SRB_Pits.Count; j++)
			{
				if (SRB_Pits[j].ContainsPoint(p[i].position, int.MaxValue, true))
				{
					p[i].inPit = true;
					num2++;
					break;
				}
			}
		}
		if (num2 > Mathf.FloorToInt((float)num / 2f))
		{
			if (num2 - num3 > Mathf.FloorToInt((float)num / 2f))
			{
				return true;
			}
			m_forceCheckGrounded = true;
			return false;
		}
		return false;
	}

	protected void ForceCheckForPitfall()
	{
		m_forceCheckGrounded = false;
		PitFallPoint[] p = null;
		SpeculativeRigidbody newPlatform = null;
		if (!PreventFallingInPits && CheckPitfallPointsForPit(ref p, ref newPlatform))
		{
			FallIntoPit(p);
		}
	}

	protected void EnsurePickupsAreNicelyDistant(float realDeltaTime)
	{
		if (!IsPickupObject || !onGround)
		{
			return;
		}
		PickupObject component = GetComponent<PickupObject>();
		if (component != null && component is CurrencyPickup)
		{
			return;
		}
		for (int i = 0; i < StaticReferenceManager.AllDebris.Count; i++)
		{
			DebrisObject debrisObject = StaticReferenceManager.AllDebris[i];
			if ((bool)debrisObject && debrisObject.IsPickupObject && debrisObject != this && debrisObject.onGround)
			{
				MovePickupAwayFromObject(debrisObject, 1.5f, 0.5f);
			}
		}
	}

	private void MovePickupAwayFromObject(BraveBehaviour otherObject, float minDist, float power)
	{
		Vector2 vector = ((!(otherObject.sprite != null)) ? (otherObject.transform.position.XY() + new Vector2(0.5f, 0.5f)) : otherObject.sprite.WorldCenter);
		Vector2 vector2 = ((!(base.sprite != null)) ? (base.transform.position.XY() + new Vector2(0.5f, 0.5f)) : base.sprite.WorldCenter);
		Vector2 vector3 = vector - vector2;
		float magnitude = vector3.magnitude;
		if (magnitude < minDist)
		{
			if (otherObject is DebrisObject)
			{
				((DebrisObject)otherObject).ApplyFrameVelocity(power * vector3.normalized);
			}
			ApplyFrameVelocity(power * vector3.normalized * -1f);
		}
	}

	protected override void InvariantUpdate(float realDeltaTime)
	{
		if ((!base.enabled && !ForceUpdateIfDisabled) || !m_hasBeenTriggered || isPitFalling)
		{
			return;
		}
		if (IsPickupObject && (bool)base.sprite && !isFalling)
		{
			base.sprite.HeightOffGround = Mathf.Max(base.sprite.HeightOffGround, -1f);
		}
		if (motionMultiplier <= 0f)
		{
			m_currentPosition.z = 0f;
			m_velocity = Vector3.zero;
			m_frameVelocity = Vector3.zero;
			isStatic = true;
			OnBecameGrounded();
		}
		SpeculativeRigidbody platform = m_platform;
		m_platform = null;
		if (usesLifespan)
		{
			m_currentLifespan -= realDeltaTime;
			if (m_currentLifespan <= 0f)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
		}
		if (m_forceCheckGrounded && onGround)
		{
			ForceCheckForPitfall();
		}
		if (onGround)
		{
			EnsurePickupsAreNicelyDistant(realDeltaTime);
		}
		if (isStatic)
		{
			m_platform = platform;
			return;
		}
		m_forceCheckGrounded = false;
		IntVector2 intVector = new IntVector2(Mathf.FloorToInt(m_transform.position.x), Mathf.FloorToInt(m_transform.position.y));
		if (IsCorpse && (bool)base.sprite)
		{
			intVector = base.sprite.WorldCenter.ToIntVector2(VectorConversions.Floor);
		}
		if (GameManager.Instance.Dungeon != null && !GameManager.Instance.Dungeon.data.CheckInBounds(intVector))
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		CellData cellData = ((!(GameManager.Instance.Dungeon != null)) ? null : GameManager.Instance.Dungeon.data.cellData[intVector.x][intVector.y]);
		if (cellData == null)
		{
			if (accurateDebris)
			{
				Debug.LogError("Destroying large debris for being outside valid cell ranges! " + base.name);
			}
			MaybeRespawnIfImportant();
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		float value = realDeltaTime * motionMultiplier;
		value = Mathf.Clamp(value, 0f, 0.1f);
		bool flag = CheckCurrentCellsFacewall(m_transform.position);
		if (m_currentPosition.z > 0f || m_velocity.z > 0f || doesDecay || isFalling)
		{
			if (m_currentPosition.z <= 0f && !isFalling)
			{
				m_velocity.z = 0f;
				m_currentPosition.z = 0f;
			}
			HandleRotation(value);
			Vector3 newPosition = m_currentPosition + m_velocity * value + m_frameVelocity * value;
			if (GameManager.Instance.Dungeon != null)
			{
				RecalculateStaticTargetCells(newPosition, m_transform.rotation);
				PitFallPoint[] sTATIC_PitfallPoints = m_STATIC_PitfallPoints;
				int num = 0;
				int num2 = 0;
				int num3 = ((!accurateDebris) ? 1 : 5);
				for (int i = 0; i < num3; i++)
				{
					if (sTATIC_PitfallPoints == null || sTATIC_PitfallPoints.Length <= i || sTATIC_PitfallPoints[i] == null)
					{
						continue;
					}
					CellData cellData2 = sTATIC_PitfallPoints[i].cellData;
					if (cellData2 == null)
					{
						continue;
					}
					bool flag2 = ((!(m_currentPosition.z > 0f)) ? (cellData2.type == CellType.WALL) : (cellData2.type == CellType.WALL && !cellData2.IsLowerFaceWall()));
					bool flag3 = m_isPickupObject && GameManager.Instance.Dungeon.data.isTopWall(cellData2.position.x, cellData2.position.y);
					if (m_isPickupObject)
					{
						bool flag4 = cellData2.parentArea != null && cellData2.parentArea.PrototypeRoomSpecialSubcategory == PrototypeDungeonRoom.RoomSpecialSubCategory.WEIRD_SHOP;
						flag2 = flag2 || (!flag4 && cellData2.type == CellType.PIT);
					}
					if (flag3)
					{
						m_recentlyBouncedOffTopwall = true;
						m_velocity.y = Mathf.Abs(m_velocity.z) + 1f;
						flag2 = true;
					}
					if (flag2)
					{
						CellData cellFromPosition = GetCellFromPosition(m_currentPosition + m_transform.rotation * (0.5f * m_spriteBounds.size));
						IntVector2 currentGridCell = ((cellFromPosition == null) ? intVector : cellFromPosition.position);
						HandleWallOrPitDeflection(currentGridCell, cellData2, value);
						break;
					}
					bool flag5 = cellData2.type == CellType.PIT;
					if (!flag5)
					{
						for (int j = 0; j < SRB_Pits.Count; j++)
						{
							if (SRB_Pits[j].ContainsPoint(sTATIC_PitfallPoints[i].position, int.MaxValue, true))
							{
								flag5 = true;
								break;
							}
						}
					}
					if (!(m_currentPosition.z <= 0f) || !flag5 || PreventFallingInPits)
					{
						continue;
					}
					SpeculativeRigidbody platform2;
					if (GameManager.Instance.Dungeon.IsPixelOnPlatform(sTATIC_PitfallPoints[i].position, out platform2))
					{
						m_platform = platform2;
						continue;
					}
					num++;
					sTATIC_PitfallPoints[i].inPit = true;
					if (cellData2.fallingPrevented)
					{
						num2++;
					}
				}
				if (num > Mathf.FloorToInt((float)num3 / 2f) && !PreventFallingInPits)
				{
					if (!PreventFallingInPits && num - num2 > Mathf.FloorToInt((float)num3 / 2f))
					{
						FallIntoPit(sTATIC_PitfallPoints);
					}
					else
					{
						m_forceCheckGrounded = true;
					}
				}
				if (m_isPickupObject && GameManager.Instance.Dungeon.data.isTopWall(cellData.position.x, cellData.position.y))
				{
					m_recentlyBouncedOffTopwall = true;
					m_velocity.y = Mathf.Abs(m_velocity.z) + 1f;
				}
			}
			newPosition = m_currentPosition + m_velocity * value + m_frameVelocity * value;
			m_frameVelocity = Vector3.zero;
			if (shouldUseSRBMotion)
			{
				m_transform.position = new Vector3(newPosition.x, newPosition.y + newPosition.z, m_transform.position.z);
				if (IsPickupObject)
				{
					m_transform.position = m_transform.position.Quantize(0.0625f);
				}
				if ((bool)base.sprite && (bool)shadowSprite)
				{
					m_transform.position = m_transform.position.Quantize(0.0625f);
					shadowSprite.PlaceAtPositionByAnchor(base.sprite.WorldBottomCenter.WithY(m_transform.position.y + 0.0625f), tk2dBaseSprite.Anchor.MiddleCenter);
					shadowSprite.transform.position = (shadowSprite.transform.position + new Vector3(0f, 0f - newPosition.z, 0f)).Quantize(0.0625f);
				}
				base.specRigidbody.Reinitialize();
			}
			else
			{
				m_transform.position = new Vector3(newPosition.x, newPosition.y + newPosition.z, m_transform.position.z);
				if ((bool)base.sprite && (bool)shadowSprite)
				{
					m_transform.position = m_transform.position.Quantize(0.0625f);
					shadowSprite.PlaceAtPositionByAnchor(base.sprite.WorldBottomCenter.WithY(m_transform.position.y + 0.0625f), tk2dBaseSprite.Anchor.MiddleCenter);
					shadowSprite.transform.position = (shadowSprite.transform.position + new Vector3(0f, 0f - newPosition.z, 0f)).Quantize(0.0625f);
				}
			}
			UpdateVelocity(value);
			m_currentPosition = newPosition;
			if (!onGround && !isFalling)
			{
				base.sprite.HeightOffGround = m_currentPosition.z + additionalHeightBoost;
			}
			if (doesDecay)
			{
				m_velocity *= 0.97f;
				if (m_velocity.magnitude < 0.5f)
				{
					doesDecay = false;
					m_velocity = Vector3.zero;
				}
			}
		}
		else
		{
			SpeculativeRigidbody newPlatform = null;
			if (OnTouchedGround != null)
			{
				OnTouchedGround(this);
			}
			PitFallPoint[] p = null;
			if (GameManager.Instance.Dungeon == null)
			{
				UnityEngine.Object.Destroy(base.gameObject, 1f);
				isStatic = true;
			}
			else if (flag && !m_recentlyBouncedOffTopwall && !m_wasFacewallFixed)
			{
				while (m_currentPosition.z < 0f)
				{
					ConvertYToZHeight(0.5f);
				}
				isStatic = false;
				m_wasFacewallFixed = true;
			}
			else if (m_isPickupObject && cellData.IsTopWall())
			{
				m_recentlyBouncedOffTopwall = true;
				ConvertYToZHeight(0.5f);
				m_velocity.y = Mathf.Max(1f, Mathf.Abs(m_velocity.y));
			}
			else if (cellData.type == CellType.WALL && !m_isPickupObject && !IsAccurateDebris)
			{
				MaybeRespawnIfImportant();
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else if (!PreventFallingInPits && CheckPitfallPointsForPit(ref p, ref newPlatform))
			{
				FallIntoPit(p);
			}
			else if (!m_isPickupObject && !PreventAbsorption && cellData.cellVisualData.floorType == CellVisualData.CellFloorType.Water && cellData.cellVisualData.absorbsDebris)
			{
				if ((bool)base.sprite)
				{
					GameManager.Instance.Dungeon.DoSplashDustupAtPosition(base.sprite.WorldCenter);
				}
				else
				{
					GameManager.Instance.Dungeon.DoSplashDustupAtPosition(base.transform.position.XY());
				}
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else if (bounceCount > 0 && GetComponent<MinorBreakable>() == null)
			{
				if (!string.IsNullOrEmpty(audioEventName))
				{
					AkSoundEngine.PostEvent(audioEventName, base.gameObject);
				}
				m_velocity = m_velocity.WithZ(Mathf.Min(5f, m_velocity.z * -1f)) * (1f - decayOnBounce);
				if (killTranslationOnBounce)
				{
					m_velocity = Vector3.zero.WithZ(m_velocity.z);
				}
				if (canRotate && additionalBounceEnglish > 0f)
				{
					angularVelocity += Mathf.Sign(angularVelocity) * additionalBounceEnglish;
				}
				if (optionalBounceVFX != null)
				{
					GameObject gameObject = SpawnManager.SpawnVFX(optionalBounceVFX);
					tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
					component.PlaceAtPositionByAnchor(base.sprite.WorldCenter, tk2dBaseSprite.Anchor.MiddleCenter);
				}
				if (DoesGoopOnRest)
				{
					DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(AssignedGoop).AddGoopCircle(base.sprite.WorldCenter, GoopRadius);
				}
				m_currentPosition = m_currentPosition.WithZ(0.05f);
				if (OnBounced != null)
				{
					OnBounced(this);
				}
				bounceCount--;
			}
			else if (m_isPickupObject && cellData.isOccupied && IsVitalPickup())
			{
				m_velocity = new Vector3(0f, -3f, 1f);
				ConvertYToZHeight(2f);
			}
			else
			{
				if (newPlatform != null)
				{
					m_platform = newPlatform;
				}
				OnBecameGrounded();
			}
		}
		if (platform != null && m_platform == null)
		{
			base.transform.parent = ((!SpawnManager.HasInstance) ? null : SpawnManager.Instance.VFX);
		}
		if ((bool)base.sprite)
		{
			base.sprite.UpdateZDepth();
		}
	}

	protected void OnBecameGrounded()
	{
		isStatic = true;
		if (detachedParticleSystems != null && detachedParticleSystems.Count > 0)
		{
			for (int i = 0; i < detachedParticleSystems.Count; i++)
			{
				if ((bool)detachedParticleSystems[i])
				{
					BraveUtility.EnableEmission(detachedParticleSystems[i], false);
				}
			}
		}
		if (GetComponent<BlackHoleDoer>() == null)
		{
			GunParticleSystemController gunParticleSystemController = null;
			if (IsPickupObject)
			{
				gunParticleSystemController = GetComponentInChildren<GunParticleSystemController>();
			}
			ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>();
			if (componentsInChildren != null && componentsInChildren.Length > 0)
			{
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					if (!gunParticleSystemController || !(gunParticleSystemController.TargetSystem == componentsInChildren[j]))
					{
						componentsInChildren[j].Stop();
						UnityEngine.Object.Destroy(componentsInChildren[j]);
					}
				}
			}
		}
		if (!onGround && !string.IsNullOrEmpty(audioEventName))
		{
			AkSoundEngine.PostEvent(audioEventName, base.gameObject);
		}
		onGround = true;
		if (shouldUseSRBMotion)
		{
			base.specRigidbody.Velocity = Vector2.zero;
			PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(base.specRigidbody);
		}
		base.sprite.attachParent = null;
		if (!m_isPickupObject)
		{
			base.sprite.IsPerpendicular = false;
			base.sprite.HeightOffGround = m_finalWorldDepth;
			base.sprite.SortingOrder = 0;
		}
		else if (m_forceUseFinalDepth)
		{
			base.sprite.HeightOffGround = m_finalWorldDepth;
		}
		base.sprite.UpdateZDepth();
		if (changesCollisionLayer && base.specRigidbody != null)
		{
			base.specRigidbody.PrimaryPixelCollider.CollisionLayer = groundedCollisionLayer;
			base.specRigidbody.ForceRegenerate();
		}
		if (breaksOnFall && UnityEngine.Random.value < breakOnFallChance)
		{
			MinorBreakable component = GetComponent<MinorBreakable>();
			if (component != null)
			{
				component.heightOffGround = 0.05f;
				component.Break(m_velocity.XY().normalized * 1.5f);
			}
		}
		if (DoesGoopOnRest)
		{
			DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(AssignedGoop).AddGoopCircle(base.sprite.WorldCenter, GoopRadius);
		}
		if (removeSRBOnGrounded)
		{
			shouldUseSRBMotion = false;
			if (base.specRigidbody != null)
			{
				base.specRigidbody.enabled = false;
				UnityEngine.Object.Destroy(base.specRigidbody);
			}
		}
		if (m_platform != null)
		{
			base.transform.parent = m_platform.transform;
		}
		if (OnGrounded != null)
		{
			OnGrounded(this);
		}
		switch (followupBehavior)
		{
		case DebrisFollowupAction.GroundedAnimation:
			if (!string.IsNullOrEmpty(followupIdentifier))
			{
				base.spriteAnimator.Play(followupIdentifier);
			}
			break;
		case DebrisFollowupAction.GroundedSprite:
			if (!string.IsNullOrEmpty(followupIdentifier))
			{
				base.sprite.SetSprite(followupIdentifier);
			}
			break;
		case DebrisFollowupAction.StopAnimationOnGrounded:
			base.spriteAnimator.Stop();
			break;
		}
		if (m_isPickupObject)
		{
			RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Floor));
			GameObject gameObject = GameObject.FindGameObjectWithTag("SellCellController");
			SellCellController sellCellController = null;
			if (gameObject != null)
			{
				sellCellController = gameObject.GetComponent<SellCellController>();
			}
			PickupObject componentInChildren = GetComponentInChildren<PickupObject>();
			if (sellCellController != null)
			{
				RoomHandler absoluteRoomFromPosition2 = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(sellCellController.transform.position.IntXY());
				if (absoluteRoomFromPosition == absoluteRoomFromPosition2)
				{
					sellCellController.AttemptSellItem(componentInChildren);
				}
			}
		}
		else if (Priority > EphemeralPriority.Middling && GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES)
		{
			base.sprite.HeightOffGround = base.sprite.HeightOffGround - UnityEngine.Random.Range(0f, 20f);
		}
	}

	public void FadeToOverrideColor(Color targetColor, float duration, float startAlpha = 0f)
	{
		StartCoroutine(HandleOverrideColorFade(targetColor, duration, startAlpha));
	}

	private IEnumerator HandleOverrideColorFade(Color targetColor, float duration, float startAlpha = 0f)
	{
		if ((bool)m_renderer)
		{
			Color startColor = new Color(targetColor.r, targetColor.g, targetColor.b, startAlpha);
			float elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += BraveTime.DeltaTime;
				Color current = Color.Lerp(t: Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration)), a: startColor, b: targetColor);
				m_renderer.material.SetColor("_OverrideColor", current);
				yield return null;
			}
			m_renderer.material.SetColor("_OverrideColor", targetColor);
		}
	}

	protected void FallIntoPit(PitFallPoint[] nextCells = null)
	{
		if (isFalling)
		{
			return;
		}
		isFalling = true;
		if (animatePitFall)
		{
			StartAnimatedPitFall(nextCells, m_velocity);
			return;
		}
		if ((bool)m_renderer)
		{
			DepthLookupManager.AssignRendererToSortingLayer(m_renderer, DepthLookupManager.GungeonSortingLayer.BACKGROUND);
			m_renderer.sortingOrder = 0;
			m_renderer.material.renderQueue = 2450;
		}
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("BG_Critical"));
		additionalHeightBoost = GameManager.PIT_DEPTH;
		if ((bool)base.sprite)
		{
			base.sprite.HeightOffGround = GameManager.PIT_DEPTH;
			base.sprite.usesOverrideMaterial = true;
			base.sprite.IsPerpendicular = false;
			base.sprite.UpdateZDepth();
		}
		if ((bool)m_renderer)
		{
			m_renderer.material.shader = ShaderCache.Acquire("Brave/DebrisPitfallShader");
		}
		float num = 0.25f;
		if (GameManager.Instance.IsFoyer)
		{
			num = 2f;
		}
		else if (GameManager.Instance.Dungeon.IsEndTimes)
		{
			num = 5f;
		}
		if ((bool)base.sprite && GameManager.Instance.Dungeon.tileIndices.PitAtPositionIsWater(base.sprite.WorldCenter))
		{
			StartCoroutine(HandleSplashDeath());
			return;
		}
		FadeToOverrideColor(Color.black, num, 0.5f);
		if ((bool)GetComponent<NPCCellKeyItem>())
		{
			GetComponent<NPCCellKeyItem>().IsBeingDestroyed = true;
		}
		MaybeRespawnIfImportant();
		UnityEngine.Object.Destroy(base.gameObject, num + 0.1f);
	}

	private IEnumerator HandleSplashDeath()
	{
		float fadeTime = 0.2f;
		FadeToOverrideColor(Color.black, fadeTime, 0.5f);
		MaybeRespawnIfImportant();
		yield return new WaitForSeconds(fadeTime);
		if ((bool)base.sprite && (IsCorpse || UnityEngine.Random.value < 0.25f))
		{
			GameManager.Instance.Dungeon.tileIndices.DoSplashAtPosition(base.sprite.WorldCenter);
		}
		if ((bool)this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void StartAnimatedPitFall(PitFallPoint[] nextCells, Vector2 velocity)
	{
		List<IntVector2> list = new List<IntVector2>();
		if (nextCells != null && accurateDebris)
		{
			if (nextCells[0].inPit && nextCells[1].inPit && nextCells[4].inPit)
			{
				list.Add(IntVector2.Left);
			}
			else if (nextCells[0].inPit && nextCells[2].inPit && nextCells[3].inPit)
			{
				list.Add(IntVector2.Right);
			}
			else if (nextCells[0].inPit && nextCells[3].inPit && nextCells[4].inPit)
			{
				list.Add(IntVector2.Up);
			}
			else if (nextCells[0].inPit && nextCells[1].inPit && nextCells[2].inPit)
			{
				list.Add(IntVector2.Down);
			}
		}
		if (list.Count == 0)
		{
			list.Add(BraveUtility.GetIntMajorAxis(velocity));
		}
		IntVector2 dir = ((!list.Contains(BraveUtility.GetIntMajorAxis(velocity))) ? list[0] : BraveUtility.GetIntMajorAxis(velocity));
		StartCoroutine(StartFallAnimation(dir, velocity));
	}

	private IEnumerator StartFallAnimation(IntVector2 dir, Vector2 debrisVelocity)
	{
		isPitFalling = false;
		if ((bool)base.sprite)
		{
			base.sprite.HeightOffGround = GameManager.PIT_DEPTH;
			base.sprite.UpdateZDepth();
		}
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("BG_Critical"));
		float duration = 0.5f;
		float initialRotation = base.transform.eulerAngles.z;
		float rotation = ((dir.x == 0) ? 0f : ((0f - Mathf.Sign(dir.x)) * 135f));
		Vector3 fallVelocity = dir.ToVector3() * 1.25f / duration;
		Vector3 acceleration = new Vector3(0f, -10f);
		Vector3 velocity = debrisVelocity;
		if (Mathf.Sign(fallVelocity.x) != Mathf.Sign(debrisVelocity.x) || Mathf.Abs(fallVelocity.x) > Mathf.Abs(debrisVelocity.x))
		{
			velocity.x = fallVelocity.x;
		}
		if (Mathf.Sign(fallVelocity.y) != Mathf.Sign(debrisVelocity.y) || Mathf.Abs(fallVelocity.y) > Mathf.Abs(debrisVelocity.y))
		{
			velocity.y = fallVelocity.y;
		}
		velocity.z = 0f;
		Vector3 cachedVector = base.sprite.transform.position;
		base.transform.position = base.sprite.WorldCenter;
		base.sprite.transform.position = cachedVector;
		float timer = 0f;
		while (timer < duration)
		{
			base.transform.position += velocity * BraveTime.DeltaTime;
			base.transform.eulerAngles = base.transform.eulerAngles.WithZ(initialRotation + Mathf.Lerp(0f, rotation, timer / duration));
			base.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0f, timer / duration);
			base.sprite.UpdateZDepth();
			yield return null;
			base.sprite.UpdateZDepth();
			timer += BraveTime.DeltaTime;
			velocity += acceleration * BraveTime.DeltaTime;
		}
		if (pitFallSplash)
		{
			GameManager.Instance.Dungeon.tileIndices.DoSplashAtPosition(base.transform.position);
		}
		MaybeRespawnIfImportant();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private bool IsVitalPickup()
	{
		if ((bool)this && IsPickupObject)
		{
			PickupObject componentInChildren = GetComponentInChildren<PickupObject>();
			if ((bool)componentInChildren)
			{
				if (componentInChildren is CurrencyPickup && (componentInChildren as CurrencyPickup).IsMetaCurrency)
				{
					return true;
				}
				if (componentInChildren is NPCCellKeyItem)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void ForceDestroyAndMaybeRespawn()
	{
		MaybeRespawnIfImportant();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void MaybeRespawnIfImportant()
	{
		if (!this || !IsPickupObject)
		{
			return;
		}
		PickupObject componentInChildren = GetComponentInChildren<PickupObject>();
		if (!componentInChildren || !componentInChildren.RespawnsIfPitfall)
		{
			return;
		}
		bool flag = false;
		if (componentInChildren is CurrencyPickup)
		{
			(componentInChildren as CurrencyPickup).ForceSetPickedUp();
			List<RewardPedestal> componentsAbsoluteInRoom = base.transform.position.GetAbsoluteRoom().GetComponentsAbsoluteInRoom<RewardPedestal>();
			if ((componentInChildren as CurrencyPickup).IsMetaCurrency && componentsAbsoluteInRoom != null && componentsAbsoluteInRoom.Count > 0)
			{
				flag = true;
				LootEngine.SpawnItem(PickupObjectDatabase.GetById(componentInChildren.PickupObjectId).gameObject, (componentsAbsoluteInRoom[0].specRigidbody.UnitCenter + UnityEngine.Random.insideUnitCircle.normalized * 3f).ToVector3ZisY(), Vector2.zero, 0f, true, true);
			}
		}
		if (!flag)
		{
			PlayerController bestActivePlayer = GameManager.Instance.BestActivePlayer;
			if ((bool)bestActivePlayer)
			{
				LootEngine.SpawnItem(PickupObjectDatabase.GetById(componentInChildren.PickupObjectId).gameObject, bestActivePlayer.CenterPosition.ToVector3ZUp(), Vector2.zero, 0f, true, !(componentInChildren is CurrencyPickup));
			}
			else
			{
				LootEngine.SpawnItem(PickupObjectDatabase.GetById(componentInChildren.PickupObjectId).gameObject, base.transform.position.GetAbsoluteRoom().GetCenteredVisibleClearSpot(2, 2).ToVector3(), Vector2.zero, 0f, true, !(componentInChildren is CurrencyPickup));
			}
		}
	}
}
