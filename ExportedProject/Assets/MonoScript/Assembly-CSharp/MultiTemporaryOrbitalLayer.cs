using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class MultiTemporaryOrbitalLayer
{
	public int targetNumberOrbitals;

	public float collisionDamage;

	public float circleRadius = 3f;

	public float rotationDegreesPerSecond = 360f;

	public GameObject deathVFX;

	protected GameObject m_orbitalPrefab;

	protected PlayerController m_player;

	protected List<SpeculativeRigidbody> m_orbitals;

	protected float m_elapsed;

	protected float m_traversedDistance;

	protected Vector3 m_currentShieldVelocity = Vector3.zero;

	protected Vector3 m_currentShieldCenterOffset = Vector3.zero;

	private Vector3 m_cachedOffsetBase;

	public void Initialize(PlayerController player, GameObject orbitalPrefab)
	{
		m_player = player;
		m_orbitalPrefab = orbitalPrefab;
		m_orbitals = new List<SpeculativeRigidbody>();
		for (int i = 0; i < targetNumberOrbitals; i++)
		{
			Vector3 position = player.LockedApproximateSpriteCenter + Quaternion.Euler(0f, 0f, 360f / (float)targetNumberOrbitals * (float)i) * Vector3.right * circleRadius;
			GameObject gameObject = UnityEngine.Object.Instantiate(orbitalPrefab, position, Quaternion.identity);
			tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
			component.HeightOffGround = 1.5f;
			tk2dSpriteAnimator component2 = gameObject.GetComponent<tk2dSpriteAnimator>();
			component2.PlayFromFrame(UnityEngine.Random.Range(0, component2.DefaultClip.frames.Length));
			SpeculativeRigidbody component3 = gameObject.GetComponent<SpeculativeRigidbody>();
			component3.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(component3.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
			component3.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(component3.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(HandleCollision));
			component3.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Combine(component3.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(HandleTileCollision));
			m_orbitals.Add(component3);
		}
	}

	public void Disconnect()
	{
		if (m_orbitals == null || m_orbitals.Count == 0)
		{
			return;
		}
		for (int i = 0; i < m_orbitals.Count; i++)
		{
			if ((bool)m_orbitals[i])
			{
				m_orbitals[i].CollideWithTileMap = true;
			}
		}
	}

	private void HandleTileCollision(CollisionData tileCollision)
	{
		DestroyKnife(tileCollision.MyRigidbody);
	}

	protected Vector3 GetTargetPositionForKniveID(Vector3 center, int i, float radiusToUse)
	{
		float num = rotationDegreesPerSecond * m_elapsed % 360f;
		return center + Quaternion.Euler(0f, 0f, num + 360f / (float)m_orbitals.Count * (float)i) * Vector3.right * radiusToUse;
	}

	private void OnPreCollision(SpeculativeRigidbody myRigidbody, PixelCollider myCollider, SpeculativeRigidbody other, PixelCollider otherCollider)
	{
		Projectile component = other.GetComponent<Projectile>();
		if (component != null && component.Owner is PlayerController)
		{
			PhysicsEngine.SkipCollision = true;
		}
		GameActor component2 = other.GetComponent<GameActor>();
		if (component2 is PlayerController)
		{
			PhysicsEngine.SkipCollision = true;
		}
		if (component2 is AIActor && !(component2 as AIActor).IsNormalEnemy)
		{
			PhysicsEngine.SkipCollision = true;
		}
	}

	private void HandleCollision(SpeculativeRigidbody other, SpeculativeRigidbody source, CollisionData collisionData)
	{
		if (other.GetComponent<AIActor>() != null)
		{
			HealthHaver component = other.GetComponent<HealthHaver>();
			component.ApplyDamage(collisionDamage, Vector2.zero, "Orbital Shield");
			DestroyKnife(source);
		}
		else if (other.GetComponent<Projectile>() != null)
		{
			Projectile component2 = other.GetComponent<Projectile>();
			if (!(component2.Owner is PlayerController))
			{
				component2.DieInAir();
				DestroyKnife(source);
			}
		}
	}

	private void DestroyKnife(SpeculativeRigidbody source)
	{
		int num = m_orbitals.IndexOf(source);
		if (num != -1)
		{
			m_orbitals.RemoveAt(num);
		}
		source.sprite.PlayEffectOnSprite(deathVFX, Vector3.zero, false);
		targetNumberOrbitals--;
		UnityEngine.Object.Destroy(source.gameObject);
	}

	public void Update()
	{
		if (GameManager.Instance.IsLoadingLevel || Dungeon.IsGenerating)
		{
			return;
		}
		m_elapsed += BraveTime.DeltaTime;
		Vector3 vector = m_currentShieldVelocity * BraveTime.DeltaTime;
		m_currentShieldCenterOffset += vector;
		m_cachedOffsetBase = m_player.LockedApproximateSpriteCenter;
		Vector3 center = m_cachedOffsetBase + m_currentShieldCenterOffset;
		float radiusToUse = circleRadius;
		while (m_orbitals.Count < targetNumberOrbitals)
		{
			AddOrbital();
		}
		for (int i = 0; i < m_orbitals.Count; i++)
		{
			if (m_orbitals[i] != null && (bool)m_orbitals[i])
			{
				Vector3 targetPositionForKniveID = GetTargetPositionForKniveID(center, i, radiusToUse);
				Vector3 vector2 = targetPositionForKniveID - m_orbitals[i].transform.position;
				Vector2 velocity = vector2.XY() / BraveTime.DeltaTime;
				m_orbitals[i].Velocity = velocity;
				m_orbitals[i].sprite.UpdateZDepth();
			}
		}
	}

	private void AddOrbital()
	{
		Vector3 position = m_player.LockedApproximateSpriteCenter + Quaternion.Euler(0f, 0f, 360f / (float)targetNumberOrbitals * (float)(m_orbitals.Count - 1)) * Vector3.right * circleRadius;
		GameObject gameObject = UnityEngine.Object.Instantiate(m_orbitalPrefab, position, Quaternion.identity);
		tk2dSprite component = gameObject.GetComponent<tk2dSprite>();
		component.HeightOffGround = 1.5f;
		tk2dSpriteAnimator component2 = gameObject.GetComponent<tk2dSpriteAnimator>();
		component2.PlayFromFrame(UnityEngine.Random.Range(0, component2.DefaultClip.frames.Length));
		SpeculativeRigidbody component3 = gameObject.GetComponent<SpeculativeRigidbody>();
		component3.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(component3.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(OnPreCollision));
		component3.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(component3.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(HandleCollision));
		component3.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Combine(component3.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(HandleTileCollision));
		m_orbitals.Add(component3);
	}
}
