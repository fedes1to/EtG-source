using System;
using System.Collections.Generic;
using BraveDynamicTree;
using UnityEngine;

public class SpeculativeRigidbody : BraveBehaviour, ICollidableObject, ILevelLoadedListener
{
	public delegate void OnPreRigidbodyCollisionDelegate(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider);

	public delegate void OnPreTileCollisionDelegate(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, PhysicsEngine.Tile tile, PixelCollider tilePixelCollider);

	public delegate void OnRigidbodyCollisionDelegate(CollisionData rigidbodyCollision);

	public delegate void OnBeamCollisionDelegate(BeamController beam);

	public delegate void OnTileCollisionDelegate(CollisionData tileCollision);

	public delegate void OnTriggerDelegate(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData);

	public delegate void OnTriggerExitDelegate(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody);

	public delegate void MovementRestrictorDelegate(SpeculativeRigidbody specRigidbody, IntVector2 prevPixelOffset, IntVector2 pixelOffset, ref bool validLocation);

	public enum RegistrationState
	{
		Registered,
		DeregisterScheduled,
		Deregistered,
		Unknown
	}

	[Serializable]
	public class DebugSettings
	{
		public bool ShowPosition;

		public int PositionHistory;

		public bool ShowVelocity;

		public bool ShowSlope;
	}

	public struct PushedRigidbodyData
	{
		public SpeculativeRigidbody SpecRigidbody;

		public bool PushedThisFrame;

		public IntVector2 Direction;

		public bool CollidedX
		{
			get
			{
				return Direction.x != 0;
			}
		}

		public bool CollidedY
		{
			get
			{
				return Direction.y != 0;
			}
		}

		public PushedRigidbodyData(SpeculativeRigidbody specRigidbody)
		{
			SpecRigidbody = specRigidbody;
			PushedThisFrame = false;
			Direction = IntVector2.Zero;
		}

		internal IntVector2 GetPushedPixelsToMove(IntVector2 pixelsToMove)
		{
			return IntVector2.Scale(Direction, pixelsToMove);
		}
	}

	public struct TemporaryException
	{
		public SpeculativeRigidbody SpecRigidbody;

		public float MinTimeRemaining;

		public float? MaxTimeRemaining;

		public TemporaryException(SpeculativeRigidbody specRigidbody, float minTime, float? maxTime)
		{
			SpecRigidbody = specRigidbody;
			MinTimeRemaining = minTime;
			MaxTimeRemaining = maxTime;
		}

		public bool HasEnded(SpeculativeRigidbody myRigidbody)
		{
			if (!SpecRigidbody)
			{
				return true;
			}
			float? maxTimeRemaining = MaxTimeRemaining;
			if (maxTimeRemaining.HasValue)
			{
				float? maxTimeRemaining2 = MaxTimeRemaining;
				MaxTimeRemaining = ((!maxTimeRemaining2.HasValue) ? null : new float?(maxTimeRemaining2.GetValueOrDefault() - BraveTime.DeltaTime));
				if (MaxTimeRemaining.Value <= 0f)
				{
					return true;
				}
			}
			if (MinTimeRemaining > 0f)
			{
				MinTimeRemaining -= BraveTime.DeltaTime;
				return false;
			}
			for (int i = 0; i < myRigidbody.PixelColliders.Count; i++)
			{
				PixelCollider pixelCollider = myRigidbody.PixelColliders[i];
				for (int j = 0; j < SpecRigidbody.PixelColliders.Count; j++)
				{
					PixelCollider otherCollider = SpecRigidbody.PixelColliders[j];
					if (pixelCollider.CanCollideWith(otherCollider, true) && pixelCollider.Overlaps(otherCollider))
					{
						return false;
					}
				}
			}
			return true;
		}
	}

	public bool CollideWithTileMap = true;

	public bool CollideWithOthers = true;

	public Vector2 Velocity = new Vector2(0f, 0f);

	public bool CapVelocity;

	[ShowInInspectorIf("CapVelocity", false)]
	public Vector2 MaxVelocity;

	public bool ForceAlwaysUpdate;

	public bool CanPush;

	public bool CanBePushed;

	[ShowInInspectorIf("CanPush", false)]
	public float PushSpeedModifier = 1f;

	public bool CanCarry;

	public bool CanBeCarried = true;

	[NonSerialized]
	public bool ForceCarriesRigidbodies;

	public bool PreventPiercing;

	public bool SkipEmptyColliders;

	[HideInInspector]
	public tk2dBaseSprite TK2DSprite;

	public Action<SpeculativeRigidbody> OnPreMovement;

	public OnPreRigidbodyCollisionDelegate OnPreRigidbodyCollision;

	public OnPreTileCollisionDelegate OnPreTileCollision;

	public Action<CollisionData> OnCollision;

	public OnRigidbodyCollisionDelegate OnRigidbodyCollision;

	public OnBeamCollisionDelegate OnBeamCollision;

	public OnTileCollisionDelegate OnTileCollision;

	public OnTriggerDelegate OnEnterTrigger;

	public OnTriggerDelegate OnTriggerCollision;

	public OnTriggerExitDelegate OnExitTrigger;

	public Action OnPathTargetReached;

	public Action<SpeculativeRigidbody, Vector2, IntVector2> OnPostRigidbodyMovement;

	public MovementRestrictorDelegate MovementRestrictor;

	public Action<BasicBeamController> OnHitByBeam;

	[NonSerialized]
	public bool RegenerateColliders;

	public bool RecheckTriggers;

	public bool UpdateCollidersOnRotation;

	public bool UpdateCollidersOnScale;

	[HideInInspector]
	public Vector2 AxialScale = Vector2.one;

	public DebugSettings DebugParams = new DebugSettings();

	[HideInInspector]
	public bool IgnorePixelGrid;

	[HideInInspector]
	public List<PixelCollider> PixelColliders;

	[NonSerialized]
	public int SortHash = -1;

	[NonSerialized]
	public int proxyId = -1;

	[NonSerialized]
	public RegistrationState PhysicsRegistration = RegistrationState.Deregistered;

	public Func<Vector2, Vector2, Vector2> ReflectProjectilesNormalGenerator;

	public Func<Vector2, Vector2, Vector2> ReflectBeamsNormalGenerator;

	private bool? m_cachedIsSimpleProjectile;

	[NonSerialized]
	public bool PathMode;

	[NonSerialized]
	public IntVector2 PathTarget;

	[NonSerialized]
	public float PathSpeed;

	[NonSerialized]
	public LinkedList<Vector3> PreviousPositions = new LinkedList<Vector3>();

	[NonSerialized]
	public Vector3 LastVelocity;

	[NonSerialized]
	public float LastRotation;

	[NonSerialized]
	public Vector2 LastScale;

	public Position m_position = new Position(0, 0);

	[NonSerialized]
	private List<SpeculativeRigidbody> m_specificCollisionExceptions;

	[NonSerialized]
	public List<TemporaryException> m_temporaryCollisionExceptions;

	[NonSerialized]
	private List<SpeculativeRigidbody> m_ghostCollisionExceptions;

	[NonSerialized]
	public List<PushedRigidbodyData> m_pushedRigidbodies = new List<PushedRigidbodyData>();

	[NonSerialized]
	private List<SpeculativeRigidbody> m_carriedRigidbodies;

	private bool m_initialized;

	public Position Position
	{
		get
		{
			return m_position;
		}
		set
		{
			m_position = value;
			UpdateColliderPositions();
			PhysicsEngine.UpdatePosition(this);
		}
	}

	public b2AABB b2AABB
	{
		get
		{
			int count = PixelColliders.Count;
			if (count == 1)
			{
				PixelCollider pixelCollider = PixelColliders[0];
				IntVector2 position = pixelCollider.Position;
				IntVector2 dimensions = pixelCollider.Dimensions;
				return new b2AABB((float)position.x * 0.0625f, (float)position.y * 0.0625f, (float)(position.x + dimensions.x - 1) * 0.0625f, (float)(position.y + dimensions.y - 1) * 0.0625f);
			}
			if (count > 1)
			{
				PixelCollider pixelCollider2 = PixelColliders[0];
				IntVector2 position2 = pixelCollider2.Position;
				IntVector2 dimensions2 = pixelCollider2.Dimensions;
				float num = position2.x;
				float num2 = position2.y;
				float num3 = position2.x + dimensions2.x - 1;
				float num4 = position2.y + dimensions2.y - 1;
				int num5 = 1;
				do
				{
					pixelCollider2 = PixelColliders[num5];
					position2 = pixelCollider2.Position;
					dimensions2 = pixelCollider2.Dimensions;
					num = Mathf.Min(num, position2.x);
					num2 = Mathf.Min(num2, position2.y);
					num3 = Mathf.Max(num3, position2.x + dimensions2.x - 1);
					num4 = Mathf.Max(num4, position2.y + dimensions2.y - 1);
					num5++;
				}
				while (num5 < count);
				return new b2AABB(num * 0.0625f, num2 * 0.0625f, num3 * 0.0625f, num4 * 0.0625f);
			}
			Debug.LogError("Trying to access a b2AABB for a SpecRigidbody with NO COLLIDERS.");
			return new b2AABB(Vector2.zero, Vector2.zero);
		}
	}

	public PixelCollider PrimaryPixelCollider
	{
		get
		{
			if (PixelColliders == null || PixelColliders.Count == 0)
			{
				return null;
			}
			return PixelColliders[0];
		}
	}

	public PixelCollider HitboxPixelCollider
	{
		get
		{
			for (int i = 0; i < PixelColliders.Count; i++)
			{
				if (PixelColliders[i].Enabled && (!SkipEmptyColliders || PixelColliders[i].Height != 0 || PixelColliders[i].Width != 0) && (PixelColliders[i].CollisionLayer == CollisionLayer.EnemyHitBox || PixelColliders[i].CollisionLayer == CollisionLayer.PlayerHitBox))
				{
					return PixelColliders[i];
				}
			}
			for (int j = 0; j < PixelColliders.Count; j++)
			{
				if (PixelColliders[j].Enabled && (!SkipEmptyColliders || PixelColliders[j].Height != 0 || PixelColliders[j].Width != 0) && (PixelColliders[j].CollisionLayer == CollisionLayer.BulletBlocker || PixelColliders[j].CollisionLayer == CollisionLayer.BulletBreakable))
				{
					return PixelColliders[j];
				}
			}
			for (int k = 0; k < PixelColliders.Count; k++)
			{
				if (PixelColliders[k].Enabled && (!SkipEmptyColliders || PixelColliders[k].Height != 0 || PixelColliders[k].Width != 0) && PixelColliders[k].CollisionLayer == CollisionLayer.HighObstacle)
				{
					return PixelColliders[k];
				}
			}
			for (int l = 0; l < PixelColliders.Count; l++)
			{
				if (PixelColliders[l].Enabled && (!SkipEmptyColliders || PixelColliders[l].Height != 0 || PixelColliders[l].Width != 0) && PixelColliders[l].CollisionLayer == CollisionLayer.Projectile)
				{
					return PixelColliders[l];
				}
			}
			return PrimaryPixelCollider;
		}
	}

	public PixelCollider GroundPixelCollider
	{
		get
		{
			for (int i = 0; i < PixelColliders.Count; i++)
			{
				if (PixelColliders[i].Enabled && (PixelColliders[i].CollisionLayer == CollisionLayer.EnemyCollider || PixelColliders[i].CollisionLayer == CollisionLayer.EnemyHitBox))
				{
					return PixelColliders[i];
				}
			}
			for (int j = 0; j < PixelColliders.Count; j++)
			{
				if (PixelColliders[j].Enabled && PixelColliders[j].CollisionLayer == CollisionLayer.PlayerCollider)
				{
					return PixelColliders[j];
				}
			}
			return null;
		}
	}

	public PixelCollider this[CollisionLayer layer]
	{
		get
		{
			return PixelColliders.Find((PixelCollider c) => c.CollisionLayer == layer);
		}
	}

	public Vector2 UnitTopLeft
	{
		get
		{
			PixelCollider primaryPixelCollider = PrimaryPixelCollider;
			return new Vector2((float)primaryPixelCollider.Position.x / (float)PhysicsEngine.Instance.PixelsPerUnit, (float)(primaryPixelCollider.Position.y + primaryPixelCollider.Height) / (float)PhysicsEngine.Instance.PixelsPerUnit);
		}
	}

	public Vector2 UnitTopCenter
	{
		get
		{
			PixelCollider primaryPixelCollider = PrimaryPixelCollider;
			return new Vector2(((float)primaryPixelCollider.Position.x + (float)primaryPixelCollider.Width / 2f) / (float)PhysicsEngine.Instance.PixelsPerUnit, (float)(primaryPixelCollider.Position.y + primaryPixelCollider.Height) / (float)PhysicsEngine.Instance.PixelsPerUnit);
		}
	}

	public Vector2 UnitTopRight
	{
		get
		{
			PixelCollider primaryPixelCollider = PrimaryPixelCollider;
			return new Vector2((float)(primaryPixelCollider.Position.x + primaryPixelCollider.Width) / (float)PhysicsEngine.Instance.PixelsPerUnit, (float)(primaryPixelCollider.Position.y + primaryPixelCollider.Height) / (float)PhysicsEngine.Instance.PixelsPerUnit);
		}
	}

	public Vector2 UnitCenterLeft
	{
		get
		{
			PixelCollider primaryPixelCollider = PrimaryPixelCollider;
			return new Vector2((float)primaryPixelCollider.Position.x / (float)PhysicsEngine.Instance.PixelsPerUnit, ((float)primaryPixelCollider.Position.y + (float)primaryPixelCollider.Height / 2f) / (float)PhysicsEngine.Instance.PixelsPerUnit);
		}
	}

	public Vector2 UnitCenter
	{
		get
		{
			PixelCollider primaryPixelCollider = PrimaryPixelCollider;
			return new Vector2(((float)primaryPixelCollider.Position.x + (float)primaryPixelCollider.Width / 2f) / (float)PhysicsEngine.Instance.PixelsPerUnit, ((float)primaryPixelCollider.Position.y + (float)primaryPixelCollider.Height / 2f) / (float)PhysicsEngine.Instance.PixelsPerUnit);
		}
	}

	public Vector2 UnitCenterRight
	{
		get
		{
			PixelCollider primaryPixelCollider = PrimaryPixelCollider;
			return new Vector2((float)(primaryPixelCollider.Position.x + primaryPixelCollider.Width) / (float)PhysicsEngine.Instance.PixelsPerUnit, ((float)primaryPixelCollider.Position.y + (float)primaryPixelCollider.Height / 2f) / (float)PhysicsEngine.Instance.PixelsPerUnit);
		}
	}

	public Vector2 UnitBottomLeft
	{
		get
		{
			PixelCollider primaryPixelCollider = PrimaryPixelCollider;
			return new Vector2((float)primaryPixelCollider.Position.x / (float)PhysicsEngine.Instance.PixelsPerUnit, (float)primaryPixelCollider.Position.y / (float)PhysicsEngine.Instance.PixelsPerUnit);
		}
	}

	public Vector2 UnitBottomCenter
	{
		get
		{
			PixelCollider primaryPixelCollider = PrimaryPixelCollider;
			return new Vector2(((float)primaryPixelCollider.Position.x + (float)primaryPixelCollider.Width / 2f) / (float)PhysicsEngine.Instance.PixelsPerUnit, (float)primaryPixelCollider.Position.y / (float)PhysicsEngine.Instance.PixelsPerUnit);
		}
	}

	public Vector2 UnitBottomRight
	{
		get
		{
			PixelCollider primaryPixelCollider = PrimaryPixelCollider;
			return new Vector2((float)(primaryPixelCollider.Position.x + primaryPixelCollider.Width) / (float)PhysicsEngine.Instance.PixelsPerUnit, (float)primaryPixelCollider.Position.y / (float)PhysicsEngine.Instance.PixelsPerUnit);
		}
	}

	public Vector2 UnitDimensions
	{
		get
		{
			return PrimaryPixelCollider.Dimensions.ToVector2() / PhysicsEngine.Instance.PixelsPerUnit;
		}
	}

	public float UnitLeft
	{
		get
		{
			return (float)PrimaryPixelCollider.MinX / (float)PhysicsEngine.Instance.PixelsPerUnit;
		}
	}

	public float UnitRight
	{
		get
		{
			return (float)(PrimaryPixelCollider.MaxX + 1) / (float)PhysicsEngine.Instance.PixelsPerUnit;
		}
	}

	public float UnitBottom
	{
		get
		{
			return (float)PrimaryPixelCollider.MinY / (float)PhysicsEngine.Instance.PixelsPerUnit;
		}
	}

	public float UnitTop
	{
		get
		{
			return (float)(PrimaryPixelCollider.MaxY + 1) / (float)PhysicsEngine.Instance.PixelsPerUnit;
		}
	}

	public float UnitWidth
	{
		get
		{
			return (float)PrimaryPixelCollider.Dimensions.x / (float)PhysicsEngine.Instance.PixelsPerUnit;
		}
	}

	public float UnitHeight
	{
		get
		{
			return (float)PrimaryPixelCollider.Dimensions.y / (float)PhysicsEngine.Instance.PixelsPerUnit;
		}
	}

	public bool ReflectProjectiles { get; set; }

	public bool ReflectBeams { get; set; }

	public bool BlockBeams { get; set; }

	public bool IsSimpleProjectile
	{
		get
		{
			bool? cachedIsSimpleProjectile = m_cachedIsSimpleProjectile;
			if (!cachedIsSimpleProjectile.HasValue)
			{
				m_cachedIsSimpleProjectile = PixelColliders.Count == 1 && PixelColliders[0].CollisionLayer == CollisionLayer.Projectile;
				if ((bool)base.projectile)
				{
					bool? cachedIsSimpleProjectile2 = m_cachedIsSimpleProjectile;
					m_cachedIsSimpleProjectile = ((!base.projectile.collidesWithProjectiles) ? cachedIsSimpleProjectile2 : new bool?(false));
				}
			}
			return m_cachedIsSimpleProjectile.Value;
		}
	}

	public bool HasTriggerCollisions { get; set; }

	public bool HasFrameSpecificCollisionExceptions { get; set; }

	public bool HasUnresolvedTriggerCollisions
	{
		get
		{
			if (RecheckTriggers)
			{
				return true;
			}
			for (int i = 0; i < PixelColliders.Count; i++)
			{
				for (int j = 0; j < PixelColliders[i].TriggerCollisions.Count; j++)
				{
					if (!PixelColliders[i].TriggerCollisions[j].Notified)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	public float TimeRemaining { get; set; }

	public IntVector2 PixelsToMove { get; set; }

	public IntVector2 ImpartedPixelsToMove { get; set; }

	public bool CollidedX { get; set; }

	public bool CollidedY { get; set; }

	public List<PushedRigidbodyData> PushedRigidbodies
	{
		get
		{
			return m_pushedRigidbodies;
		}
	}

	public List<TemporaryException> TemporaryCollisionExceptions
	{
		get
		{
			return m_temporaryCollisionExceptions;
		}
	}

	public List<SpeculativeRigidbody> GhostCollisionExceptions
	{
		get
		{
			return m_ghostCollisionExceptions;
		}
	}

	public List<SpeculativeRigidbody> CarriedRigidbodies
	{
		get
		{
			return m_carriedRigidbodies;
		}
	}

	public List<PixelCollider> GetPixelColliders()
	{
		return PixelColliders;
	}

	public void ForceRegenerate(bool? allowRotation = null, bool? allowScale = null)
	{
		if (!allowRotation.HasValue)
		{
			allowRotation = UpdateCollidersOnRotation;
		}
		if (!allowScale.HasValue)
		{
			allowScale = UpdateCollidersOnScale;
		}
		for (int i = 0; i < PixelColliders.Count; i++)
		{
			PixelColliders[i].Regenerate(base.transform, allowRotation.Value, allowScale.Value);
		}
		RegenerateColliders = false;
		PhysicsEngine.Instance.Register(this);
		PhysicsEngine.UpdatePosition(this);
	}

	public PixelCollider GetPixelCollider(ColliderType preferredCollider)
	{
		PixelCollider pixelCollider = null;
		switch (preferredCollider)
		{
		case ColliderType.Ground:
			pixelCollider = GroundPixelCollider;
			break;
		case ColliderType.HitBox:
			pixelCollider = HitboxPixelCollider;
			break;
		}
		if (pixelCollider == null)
		{
			pixelCollider = PrimaryPixelCollider;
		}
		return pixelCollider;
	}

	public Vector2 GetUnitCenter(ColliderType preferredCollider)
	{
		return GetPixelCollider(preferredCollider).UnitCenter;
	}

	private void Start()
	{
		Initialize();
	}

	public void Initialize()
	{
		if (!m_initialized)
		{
			if (TK2DSprite == null)
			{
				TK2DSprite = base.sprite;
			}
			m_position.UnitPosition = base.transform.position;
			if (UpdateCollidersOnRotation && base.transform != null)
			{
				LastRotation = base.transform.eulerAngles.z;
			}
			ForceRegenerate();
			if (TK2DSprite != null)
			{
				TK2DSprite.UpdateZDepth();
			}
			if (PhysicsEngine.Instance != null)
			{
				PhysicsEngine.Instance.Register(this);
			}
			m_initialized = true;
		}
	}

	public void Reinitialize()
	{
		if (!m_initialized)
		{
			Initialize();
			return;
		}
		m_position.UnitPosition = base.transform.position;
		for (int i = 0; i < PixelColliders.Count; i++)
		{
			PixelCollider pixelCollider = PixelColliders[i];
			pixelCollider.Position = Position.PixelPosition + pixelCollider.Offset;
		}
		PhysicsEngine.Instance.Register(this);
		PhysicsEngine.UpdatePosition(this);
	}

	protected override void OnDestroy()
	{
		if (PhysicsEngine.HasInstance)
		{
			PhysicsEngine.Instance.Deregister(this);
		}
		base.OnDestroy();
	}

	private void OnDrawGizmos()
	{
		if (DebugParams.ShowPosition)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(Position.UnitPosition, 0.05f);
			if (DebugParams.PositionHistory > 0)
			{
				int num = 0;
				foreach (Vector3 previousPosition in PreviousPositions)
				{
					Gizmos.color = Color.Lerp(Color.green, Color.red, (float)num / (float)DebugParams.PositionHistory);
					Gizmos.DrawSphere(previousPosition, 0.05f);
					num++;
				}
			}
		}
		if (DebugParams.ShowVelocity && PrimaryPixelCollider != null)
		{
			if (Velocity.magnitude > 0f)
			{
				LastVelocity = Velocity;
			}
			if (LastVelocity.magnitude > 0f)
			{
				Gizmos.color = Color.white;
				Vector3 vector = Position.UnitPosition;
				vector += new Vector3(PrimaryPixelCollider.Width, PrimaryPixelCollider.Height, 0f) / (PhysicsEngine.Instance.PixelsPerUnit * 2);
				Gizmos.DrawLine(vector, vector + LastVelocity.normalized);
			}
		}
	}

	public bool ContainsPoint(Vector2 point, int mask = int.MaxValue, bool collideWithTriggers = false)
	{
		return ContainsPixel(PhysicsEngine.UnitToPixel(point), mask, collideWithTriggers);
	}

	public bool ContainsPixel(IntVector2 pixel, int mask = int.MaxValue, bool collideWithTriggers = false)
	{
		for (int i = 0; i < GetPixelColliders().Count; i++)
		{
			PixelCollider pixelCollider = GetPixelColliders()[i];
			if ((collideWithTriggers || !pixelCollider.IsTrigger) && pixelCollider.CanCollideWith(mask) && pixelCollider.ContainsPixel(pixel))
			{
				return true;
			}
		}
		return false;
	}

	public void RegisterSpecificCollisionException(SpeculativeRigidbody specRigidbody)
	{
		if ((bool)specRigidbody)
		{
			if (m_specificCollisionExceptions == null)
			{
				m_specificCollisionExceptions = new List<SpeculativeRigidbody>();
			}
			if (!m_specificCollisionExceptions.Contains(specRigidbody))
			{
				m_specificCollisionExceptions.Add(specRigidbody);
			}
		}
	}

	public bool IsSpecificCollisionException(SpeculativeRigidbody specRigidbody)
	{
		if (m_specificCollisionExceptions == null)
		{
			return false;
		}
		if (m_specificCollisionExceptions.Count == 0)
		{
			return false;
		}
		return m_specificCollisionExceptions.Contains(specRigidbody);
	}

	public void DeregisterSpecificCollisionException(SpeculativeRigidbody specRigidbody)
	{
		if (!(specRigidbody == null) && m_specificCollisionExceptions != null)
		{
			m_specificCollisionExceptions.Remove(specRigidbody);
		}
	}

	public void ClearSpecificCollisionExceptions()
	{
		if (m_specificCollisionExceptions != null)
		{
			m_specificCollisionExceptions.Clear();
		}
	}

	public void ClearFrameSpecificCollisionExceptions()
	{
		for (int i = 0; i < PixelColliders.Count; i++)
		{
			PixelColliders[i].ClearFrameSpecificCollisionExceptions();
		}
		HasFrameSpecificCollisionExceptions = false;
	}

	public void RegisterTemporaryCollisionException(SpeculativeRigidbody specRigidbody, float minTime = 0.01f, float? maxTime = null)
	{
		if (!specRigidbody)
		{
			return;
		}
		if (m_temporaryCollisionExceptions == null)
		{
			m_temporaryCollisionExceptions = new List<TemporaryException>();
		}
		for (int i = 0; i < m_temporaryCollisionExceptions.Count; i++)
		{
			if (!(m_temporaryCollisionExceptions[i].SpecRigidbody == specRigidbody))
			{
				continue;
			}
			TemporaryException value = m_temporaryCollisionExceptions[i];
			value.MinTimeRemaining = Mathf.Max(value.MinTimeRemaining, minTime);
			if (maxTime.HasValue)
			{
				float? maxTimeRemaining = value.MaxTimeRemaining;
				if (!maxTimeRemaining.HasValue)
				{
					value.MaxTimeRemaining = maxTime;
				}
				else
				{
					value.MaxTimeRemaining = Math.Min(value.MaxTimeRemaining.Value, maxTime.Value);
				}
			}
			m_temporaryCollisionExceptions[i] = value;
			return;
		}
		m_temporaryCollisionExceptions.Add(new TemporaryException(specRigidbody, minTime, maxTime));
	}

	public bool IsTemporaryCollisionException(SpeculativeRigidbody specRigidbody)
	{
		if (m_temporaryCollisionExceptions == null)
		{
			return false;
		}
		if (m_temporaryCollisionExceptions.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < m_temporaryCollisionExceptions.Count; i++)
		{
			if (m_temporaryCollisionExceptions[i].SpecRigidbody == specRigidbody)
			{
				return true;
			}
		}
		return false;
	}

	public void DeregisterTemporaryCollisionException(SpeculativeRigidbody specRigidbody)
	{
		if (m_temporaryCollisionExceptions == null)
		{
			return;
		}
		for (int i = 0; i < m_temporaryCollisionExceptions.Count; i++)
		{
			if (m_temporaryCollisionExceptions[i].SpecRigidbody == specRigidbody)
			{
				m_temporaryCollisionExceptions.RemoveAt(i);
				break;
			}
		}
	}

	public void RegisterGhostCollisionException(SpeculativeRigidbody specRigidbody)
	{
		if ((bool)specRigidbody)
		{
			if (m_ghostCollisionExceptions == null)
			{
				m_ghostCollisionExceptions = new List<SpeculativeRigidbody>();
			}
			if (!m_ghostCollisionExceptions.Contains(specRigidbody))
			{
				m_ghostCollisionExceptions.Add(specRigidbody);
			}
		}
	}

	public bool IsGhostCollisionException(SpeculativeRigidbody specRigidbody)
	{
		return specRigidbody != null && m_ghostCollisionExceptions != null && m_ghostCollisionExceptions.Contains(specRigidbody);
	}

	public void DeregisterGhostCollisionException(SpeculativeRigidbody specRigidbody)
	{
		if (m_ghostCollisionExceptions != null)
		{
			m_ghostCollisionExceptions.Remove(specRigidbody);
		}
	}

	public void DeregisterGhostCollisionException(int index)
	{
		if (m_ghostCollisionExceptions != null)
		{
			m_ghostCollisionExceptions.RemoveAt(index);
		}
	}

	public void RegisterCarriedRigidbody(SpeculativeRigidbody specRigidbody)
	{
		if ((bool)specRigidbody)
		{
			if (m_carriedRigidbodies == null)
			{
				m_carriedRigidbodies = new List<SpeculativeRigidbody>();
			}
			if (!m_carriedRigidbodies.Contains(specRigidbody))
			{
				m_carriedRigidbodies.Add(specRigidbody);
			}
		}
	}

	public void DeregisterCarriedRigidbody(SpeculativeRigidbody specRigidbody)
	{
		if (m_carriedRigidbodies != null)
		{
			m_carriedRigidbodies.Remove(specRigidbody);
		}
	}

	public void ResetTriggerCollisionData()
	{
		HasTriggerCollisions = false;
		for (int i = 0; i < PixelColliders.Count; i++)
		{
			PixelColliders[i].ResetTriggerCollisionData();
			if (PixelColliders[i].TriggerCollisions.Count > 0)
			{
				HasTriggerCollisions = true;
			}
		}
	}

	public void FlagCellsOccupied()
	{
		IntVector2 intVector = UnitBottomLeft.ToIntVector2(VectorConversions.Floor);
		PixelCollider primaryPixelCollider = PrimaryPixelCollider;
		Vector2 vector = new Vector2((float)(primaryPixelCollider.Position.x + (primaryPixelCollider.Width - 1)) / (float)PhysicsEngine.Instance.PixelsPerUnit, (float)(primaryPixelCollider.Position.y + (primaryPixelCollider.Height - 1)) / (float)PhysicsEngine.Instance.PixelsPerUnit);
		IntVector2 intVector2 = vector.ToIntVector2(VectorConversions.Floor);
		for (int i = intVector.x; i <= intVector2.x; i++)
		{
			for (int j = intVector.y; j <= intVector2.y; j++)
			{
				GameManager.Instance.Dungeon.data[new IntVector2(i, j)].isOccupied = true;
			}
		}
	}

	public bool CanCollideWith(SpeculativeRigidbody otherRigidbody)
	{
		if (!this)
		{
			return false;
		}
		if (this == otherRigidbody)
		{
			return false;
		}
		if (!base.enabled || !CollideWithOthers)
		{
			return false;
		}
		if (!otherRigidbody || !otherRigidbody.enabled || !otherRigidbody.CollideWithOthers)
		{
			return false;
		}
		if (IsSpecificCollisionException(otherRigidbody) || otherRigidbody.IsSpecificCollisionException(this))
		{
			return false;
		}
		if (IsTemporaryCollisionException(otherRigidbody) || otherRigidbody.IsTemporaryCollisionException(this))
		{
			return false;
		}
		return true;
	}

	public void AddCollisionLayerOverride(int mask)
	{
		for (int i = 0; i < PixelColliders.Count; i++)
		{
			PixelColliders[i].CollisionLayerCollidableOverride |= mask;
		}
	}

	public void RemoveCollisionLayerOverride(int mask)
	{
		for (int i = 0; i < PixelColliders.Count; i++)
		{
			PixelColliders[i].CollisionLayerCollidableOverride &= ~mask;
		}
	}

	public void AddCollisionLayerIgnoreOverride(int mask)
	{
		for (int i = 0; i < PixelColliders.Count; i++)
		{
			PixelColliders[i].CollisionLayerIgnoreOverride |= mask;
		}
	}

	public void RemoveCollisionLayerIgnoreOverride(int mask)
	{
		for (int i = 0; i < PixelColliders.Count; i++)
		{
			PixelColliders[i].CollisionLayerIgnoreOverride &= ~mask;
		}
	}

	public void UpdateColliderPositions()
	{
		for (int i = 0; i < PixelColliders.Count; i++)
		{
			PixelColliders[i].Position = Position.PixelPosition + PixelColliders[i].Offset;
		}
	}

	public void BraveOnLevelWasLoaded()
	{
		PhysicsRegistration = RegistrationState.Unknown;
	}

	public void Cleanup()
	{
		if (m_specificCollisionExceptions != null)
		{
			m_specificCollisionExceptions.Clear();
		}
		if (m_temporaryCollisionExceptions != null)
		{
			m_temporaryCollisionExceptions.Clear();
		}
		if (m_ghostCollisionExceptions != null)
		{
			m_ghostCollisionExceptions.Clear();
		}
		m_pushedRigidbodies.Clear();
		if (m_carriedRigidbodies != null)
		{
			m_carriedRigidbodies.Clear();
		}
	}

	public void AlignWithRigidbodyBottomLeft(SpeculativeRigidbody otherRigidbody)
	{
		Vector2 vector = UnitBottomLeft - base.transform.position.XY();
		Vector2 vector2 = otherRigidbody.UnitBottomLeft - otherRigidbody.transform.position.XY();
		base.transform.position = otherRigidbody.transform.position.XY() - vector + vector2;
		base.specRigidbody.Reinitialize();
	}

	public void AlignWithRigidbodyBottomCenter(SpeculativeRigidbody otherRigidbody, IntVector2? pixelOffset = null)
	{
		Vector2 vector = UnitBottomCenter - base.transform.position.XY();
		Vector2 vector2 = otherRigidbody.UnitBottomCenter - otherRigidbody.transform.position.XY();
		Vector2 vector3 = Vector2.zero;
		if (pixelOffset.HasValue)
		{
			vector3 = PhysicsEngine.PixelToUnit(pixelOffset.Value);
		}
		base.transform.position = otherRigidbody.transform.position.XY() - vector + vector2 + vector3;
		base.specRigidbody.Reinitialize();
	}
}
