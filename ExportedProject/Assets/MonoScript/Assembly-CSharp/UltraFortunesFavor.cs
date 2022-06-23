using System;
using System.Collections.Generic;
using UnityEngine;

public class UltraFortunesFavor : BraveBehaviour
{
	private class ProjectileData
	{
		public Projectile projectile;

		public float positionDeg;

		public Vector2 initialVelocity;

		public float degPerSecond;

		public ProjectileData(Projectile projectile, float positionDeg, Vector2 initialVelocity, float degPerSecond)
		{
			this.projectile = projectile;
			this.positionDeg = positionDeg;
			this.initialVelocity = initialVelocity;
			this.degPerSecond = degPerSecond;
		}
	}

	public GameObject sparkOctantVFX;

	public float vfxOffset = 0.625f;

	public float bulletRadius = 2f;

	public float bulletSpeedModifier = 0.8f;

	public float beamRadius = 2f;

	public float goopRadius = 2f;

	private readonly List<ProjectileData> m_caughtBullets = new List<ProjectileData>();

	private GameObject[] m_octantVfx = new GameObject[8];

	private PixelCollider m_bulletBlocker;

	private PixelCollider m_beamReflector;

	private Vector2 m_lastPosition;

	private int m_goopExceptionId = -1;

	private float m_enemyOverlapTimer;

	public Vector2 BulletCircleCenter
	{
		get
		{
			return base.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
	}

	public Vector2 BeamCircleCenter
	{
		get
		{
			return base.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
	}

	public Vector2 GoopCircleCenter
	{
		get
		{
			return base.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
	}

	public void Awake()
	{
		m_enemyOverlapTimer = UnityEngine.Random.Range(2f, 4f);
	}

	public void Start()
	{
		base.specRigidbody.Initialize();
		if (bulletRadius > 0f)
		{
			IntVector2 intVector = PhysicsEngine.UnitToPixel(BulletCircleCenter - base.transform.position.XY());
			int num = PhysicsEngine.UnitToPixel(bulletRadius);
			m_bulletBlocker = new PixelCollider();
			m_bulletBlocker.ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Circle;
			m_bulletBlocker.CollisionLayer = CollisionLayer.BulletBlocker;
			m_bulletBlocker.IsTrigger = true;
			m_bulletBlocker.ManualOffsetX = intVector.x - num;
			m_bulletBlocker.ManualOffsetY = intVector.y - num;
			m_bulletBlocker.ManualDiameter = num * 2;
			m_bulletBlocker.Regenerate(base.transform);
			base.specRigidbody.PixelColliders.Add(m_bulletBlocker);
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnTriggerCollision));
		}
		if (beamRadius > 0f)
		{
			IntVector2 intVector2 = PhysicsEngine.UnitToPixel(BeamCircleCenter - base.transform.position.XY());
			int num2 = PhysicsEngine.UnitToPixel(beamRadius);
			m_beamReflector = new PixelCollider();
			m_beamReflector.ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Circle;
			m_beamReflector.CollisionLayer = CollisionLayer.BeamBlocker;
			m_beamReflector.IsTrigger = false;
			m_beamReflector.ManualOffsetX = intVector2.x - num2;
			m_beamReflector.ManualOffsetY = intVector2.y - num2;
			m_beamReflector.ManualDiameter = num2 * 2;
			m_beamReflector.Regenerate(base.transform);
			base.specRigidbody.PixelColliders.Add(m_beamReflector);
		}
		if (bulletRadius > 0f || beamRadius > 0f)
		{
			PhysicsEngine.UpdatePosition(base.specRigidbody);
		}
		if (goopRadius > 0f)
		{
			m_goopExceptionId = DeadlyDeadlyGoopManager.RegisterUngoopableCircle(GoopCircleCenter, goopRadius);
			m_lastPosition = base.transform.position.XY();
		}
	}

	public void Update()
	{
		for (int num = m_caughtBullets.Count - 1; num >= 0; num--)
		{
			ProjectileData projectileData = m_caughtBullets[num];
			projectileData.positionDeg += BraveTime.DeltaTime * projectileData.degPerSecond;
			Projectile projectile = m_caughtBullets[num].projectile;
			if (!(projectile == null))
			{
				if (!projectile)
				{
					m_caughtBullets[num] = null;
				}
				else
				{
					HitFromPoint(projectile.transform.position.XY());
					Vector2 vector = BulletCircleCenter - projectile.transform.position.XY();
					if (Mathf.Abs(BraveMathCollege.ClampAngle180(vector.ToAngle() - m_caughtBullets[num].initialVelocity.ToAngle())) > 90f)
					{
						Vector2 vector2 = Quaternion.Euler(0f, 0f, -90f * Mathf.Sign(m_caughtBullets[num].degPerSecond)) * (BulletCircleCenter - projectile.transform.position.XY());
						projectile.ManualControl = false;
						projectile.SendInDirection(m_caughtBullets[num].initialVelocity.magnitude * vector2.normalized, true);
						m_caughtBullets[num].projectile = null;
					}
					else
					{
						Vector2 bulletPosition = GetBulletPosition(projectileData.positionDeg);
						projectile.specRigidbody.Velocity = (bulletPosition - (Vector2)projectile.transform.position) / BraveTime.DeltaTime;
						if (projectile.shouldRotate)
						{
							projectile.transform.rotation = Quaternion.Euler(0f, 0f, 180f + (Quaternion.Euler(0f, 0f, 90f) * (BulletCircleCenter - bulletPosition)).XY().ToAngle());
						}
					}
				}
			}
		}
		if (goopRadius > 0f)
		{
			Vector2 vector3 = base.transform.position.XY();
			if (vector3 != m_lastPosition)
			{
				DeadlyDeadlyGoopManager.UpdateUngoopableCircle(m_goopExceptionId, GoopCircleCenter, goopRadius);
				m_lastPosition = vector3;
			}
		}
		m_enemyOverlapTimer -= BraveTime.DeltaTime;
		if (!PhysicsEngine.HasInstance || !(m_enemyOverlapTimer <= 0f))
		{
			return;
		}
		List<SpeculativeRigidbody> overlappingRigidbodies = PhysicsEngine.Instance.GetOverlappingRigidbodies(base.specRigidbody);
		for (int i = 0; i < overlappingRigidbodies.Count; i++)
		{
			SpeculativeRigidbody speculativeRigidbody = overlappingRigidbodies[i];
			if ((bool)speculativeRigidbody && (bool)speculativeRigidbody.aiActor)
			{
				base.specRigidbody.RegisterGhostCollisionException(speculativeRigidbody);
				speculativeRigidbody.RegisterGhostCollisionException(base.specRigidbody);
			}
		}
		m_enemyOverlapTimer = 2f;
	}

	protected override void OnDestroy()
	{
		if (bulletRadius > 0f)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Remove(speculativeRigidbody.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnTriggerCollision));
		}
		DeadlyDeadlyGoopManager.DeregisterUngoopableCircle(m_goopExceptionId);
		base.OnDestroy();
	}

	private void OnTriggerCollision(SpeculativeRigidbody specRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		if (base.enabled && collisionData.MyPixelCollider == m_bulletBlocker && collisionData.OtherRigidbody != null && collisionData.OtherRigidbody.projectile != null)
		{
			Projectile projectile = collisionData.OtherRigidbody.projectile;
			Vector2 vector = BulletCircleCenter - projectile.transform.position.XY();
			float f = BraveMathCollege.ClampAngle180(vector.ToAngle() - projectile.specRigidbody.Velocity.ToAngle());
			float positionDeg = BraveMathCollege.ClampAngle360((collisionData.Contact - BulletCircleCenter).ToAngle());
			float num = Mathf.Sign(f) * projectile.specRigidbody.Velocity.magnitude / ((float)Math.PI * bulletRadius) * 180f;
			m_caughtBullets.Insert(Mathf.Max(0, m_caughtBullets.Count - 1), new ProjectileData(projectile, positionDeg, projectile.specRigidbody.Velocity, num * bulletSpeedModifier));
			projectile.specRigidbody.Velocity = Vector2.zero;
			projectile.ManualControl = true;
			collisionData.OtherRigidbody.RegisterSpecificCollisionException(collisionData.MyRigidbody);
			HitFromDirection(-vector);
			if (!base.talkDoer || !base.talkDoer.IsTalking)
			{
				SendPlaymakerEvent("takePlayerDamage");
			}
		}
	}

	public Vector2 GetBeamNormal(Vector2 targetPoint)
	{
		return (targetPoint - BulletCircleCenter).normalized;
	}

	public void HitFromPoint(Vector2 targetPoint)
	{
		HitFromDirection(targetPoint - BulletCircleCenter);
	}

	private Vector2 GetBulletPosition(float angle)
	{
		return BulletCircleCenter + new Vector2(Mathf.Cos(angle * ((float)Math.PI / 180f)), Mathf.Sin(angle * ((float)Math.PI / 180f))) * bulletRadius;
	}

	private void HitFromDirection(Vector2 dir)
	{
		int num = BraveMathCollege.VectorToOctant(dir);
		if (!m_octantVfx[num])
		{
			Vector3 offset = Quaternion.Euler(0f, 0f, -num * 45 - 90) * new Vector3(0f - vfxOffset, 0f, 0f);
			m_octantVfx[num] = PlayEffectOnActor(sparkOctantVFX, offset, true, true);
			m_octantVfx[num].transform.rotation = Quaternion.Euler(0f, 0f, -45 + -45 * num);
		}
	}

	private GameObject PlayEffectOnActor(GameObject effect, Vector3 offset, bool attached = true, bool alreadyMiddleCenter = false)
	{
		GameObject gameObject = SpawnManager.SpawnVFX(effect, true);
		tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
		if (!alreadyMiddleCenter)
		{
			component.PlaceAtPositionByAnchor(base.sprite.WorldCenter.ToVector3ZUp() + offset, tk2dBaseSprite.Anchor.MiddleCenter);
		}
		else
		{
			component.transform.position = base.sprite.WorldCenter.ToVector3ZUp() + offset;
		}
		if (attached)
		{
			gameObject.transform.parent = base.transform;
			component.HeightOffGround = 0.2f;
			base.sprite.AttachRenderer(component);
		}
		return gameObject;
	}
}
