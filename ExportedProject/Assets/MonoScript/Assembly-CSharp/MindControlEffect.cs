using System;
using System.Collections.Generic;
using UnityEngine;

public class MindControlEffect : MonoBehaviour
{
	[NonSerialized]
	public PlayerController owner;

	private AIActor m_aiActor;

	private BehaviorSpeculator m_behaviorSpeculator;

	private bool m_attackedThisCycle = true;

	private NonActor m_fakeActor;

	private SpeculativeRigidbody m_fakeTargetRigidbody;

	private ArbitraryCableDrawer m_cable;

	private GameObject m_overheadVFX;

	private void Start()
	{
		m_aiActor = GetComponent<AIActor>();
		m_behaviorSpeculator = m_aiActor.behaviorSpeculator;
		GameObject gameObject = new GameObject("fake target");
		m_fakeActor = gameObject.AddComponent<NonActor>();
		m_fakeActor.HasShadow = false;
		m_fakeTargetRigidbody = gameObject.AddComponent<SpeculativeRigidbody>();
		m_fakeTargetRigidbody.PixelColliders = new List<PixelCollider>();
		m_fakeTargetRigidbody.CollideWithTileMap = false;
		m_fakeTargetRigidbody.CollideWithOthers = false;
		m_fakeTargetRigidbody.CanBeCarried = false;
		m_fakeTargetRigidbody.CanBePushed = false;
		m_fakeTargetRigidbody.CanCarry = false;
		PixelCollider pixelCollider = new PixelCollider();
		pixelCollider.ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual;
		pixelCollider.CollisionLayer = CollisionLayer.TileBlocker;
		pixelCollider.ManualWidth = 4;
		pixelCollider.ManualHeight = 4;
		m_fakeTargetRigidbody.PixelColliders.Add(pixelCollider);
		m_cable = m_aiActor.gameObject.AddComponent<ArbitraryCableDrawer>();
		m_cable.Attach1Offset = owner.CenterPosition - owner.transform.position.XY();
		m_cable.Attach2Offset = m_aiActor.CenterPosition - m_aiActor.transform.position.XY();
		m_cable.Initialize(owner.transform, m_aiActor.transform);
		m_overheadVFX = m_aiActor.PlayEffectOnActor((GameObject)ResourceCache.Acquire("Global VFX/VFX_Controller_Status"), new Vector3(0f, m_aiActor.specRigidbody.HitboxPixelCollider.UnitDimensions.y, 0f), true, false, true);
	}

	private Vector2 GetPlayerAimPointController(Vector2 aimBase, Vector2 aimDirection)
	{
		Func<SpeculativeRigidbody, bool> rigidbodyExcluder = (SpeculativeRigidbody otherRigidbody) => (bool)otherRigidbody.minorBreakable && !otherRigidbody.minorBreakable.stopsBullets;
		Vector2 result = aimBase + aimDirection * 10f;
		CollisionLayer layer = CollisionLayer.EnemyHitBox;
		int rayMask = CollisionMask.LayerToMask(CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker, layer, CollisionLayer.BulletBreakable);
		RaycastResult result2;
		if (PhysicsEngine.Instance.Raycast(aimBase, aimDirection, 50f, out result2, true, true, rayMask, null, false, rigidbodyExcluder))
		{
			result = aimBase + aimDirection * result2.Distance;
		}
		RaycastResult.Pool.Free(ref result2);
		return result;
	}

	private void UpdateAimTargetPosition()
	{
		PlayerController playerController = owner;
		BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(playerController.PlayerIDX);
		GungeonActions activeActions = instanceForPlayer.ActiveActions;
		if (instanceForPlayer.IsKeyboardAndMouse())
		{
			m_fakeTargetRigidbody.transform.position = playerController.unadjustedAimPoint.XY();
		}
		else
		{
			m_fakeTargetRigidbody.transform.position = GetPlayerAimPointController(playerController.CenterPosition, activeActions.Aim.Vector);
		}
		m_fakeTargetRigidbody.Reinitialize();
	}

	private void Update()
	{
		m_fakeActor.specRigidbody = m_fakeTargetRigidbody;
		if ((bool)m_aiActor)
		{
			m_aiActor.CanTargetEnemies = true;
			m_aiActor.CanTargetPlayers = false;
			m_aiActor.PlayerTarget = m_fakeActor;
			m_aiActor.OverrideTarget = null;
			UpdateAimTargetPosition();
			if ((bool)m_aiActor.aiShooter)
			{
				m_aiActor.aiShooter.AimAtPoint(m_behaviorSpeculator.PlayerTarget.CenterPosition);
			}
		}
		if (!m_behaviorSpeculator)
		{
			return;
		}
		PlayerController playerController = owner;
		BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(playerController.PlayerIDX);
		GungeonActions activeActions = instanceForPlayer.ActiveActions;
		if (m_behaviorSpeculator.AttackCooldown <= 0f)
		{
			if (!m_attackedThisCycle && m_behaviorSpeculator.ActiveContinuousAttackBehavior != null)
			{
				m_attackedThisCycle = true;
			}
			if (m_attackedThisCycle && m_behaviorSpeculator.ActiveContinuousAttackBehavior == null)
			{
				m_behaviorSpeculator.AttackCooldown = float.MaxValue;
			}
		}
		else if (activeActions.ShootAction.WasPressed)
		{
			m_attackedThisCycle = false;
			m_behaviorSpeculator.AttackCooldown = 0f;
		}
		if (m_behaviorSpeculator.TargetBehaviors != null && m_behaviorSpeculator.TargetBehaviors.Count > 0)
		{
			m_behaviorSpeculator.TargetBehaviors.Clear();
		}
		if (m_behaviorSpeculator.MovementBehaviors != null && m_behaviorSpeculator.MovementBehaviors.Count > 0)
		{
			m_behaviorSpeculator.MovementBehaviors.Clear();
		}
		m_aiActor.ImpartedVelocity += activeActions.Move.Value * m_aiActor.MovementSpeed;
		if (m_behaviorSpeculator.AttackBehaviors != null)
		{
			for (int i = 0; i < m_behaviorSpeculator.AttackBehaviors.Count; i++)
			{
				AttackBehaviorBase attack = m_behaviorSpeculator.AttackBehaviors[i];
				ProcessAttack(attack);
			}
		}
	}

	private void ProcessAttack(AttackBehaviorBase attack)
	{
		if (attack == null)
		{
			return;
		}
		if (attack is BasicAttackBehavior)
		{
			BasicAttackBehavior basicAttackBehavior = attack as BasicAttackBehavior;
			basicAttackBehavior.Cooldown = 0f;
			basicAttackBehavior.RequiresLineOfSight = false;
			basicAttackBehavior.MinRange = -1f;
			basicAttackBehavior.Range = -1f;
			if (attack is TeleportBehavior)
			{
				basicAttackBehavior.RequiresLineOfSight = true;
				basicAttackBehavior.MinRange = 1000f;
				basicAttackBehavior.Range = 0.1f;
			}
			if (basicAttackBehavior is ShootGunBehavior)
			{
				ShootGunBehavior shootGunBehavior = basicAttackBehavior as ShootGunBehavior;
				shootGunBehavior.LineOfSight = false;
				shootGunBehavior.EmptiesClip = false;
				shootGunBehavior.RespectReload = false;
			}
		}
		else if (attack is AttackBehaviorGroup)
		{
			AttackBehaviorGroup attackBehaviorGroup = attack as AttackBehaviorGroup;
			for (int i = 0; i < attackBehaviorGroup.AttackBehaviors.Count; i++)
			{
				ProcessAttack(attackBehaviorGroup.AttackBehaviors[i].Behavior);
			}
		}
	}
}
