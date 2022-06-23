using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class BoomerangProjectile : Projectile
{
	public float StunDuration = 5f;

	public float trackingSpeed = 5f;

	public bool stopTrackingIfLeaveRadius;

	public bool UsesMouseAimPoint;

	public int MaximumNumberOfTargets = -1;

	public float MaximumTraversalDistance = -1f;

	private PlayerController throwerPlayer;

	private Queue<AIActor> RemainingEnemiesToHit;

	private float m_targetlessTime;

	private float m_elapsedTargetlessTime;

	public override void Start()
	{
		base.Start();
		if ((bool)PossibleSourceGun && PossibleSourceGun.OwnerHasSynergy(CustomSynergyType.CRAVE_THE_GLAIVE))
		{
			MaximumNumberOfTargets = -1;
			MaximumTraversalDistance = -1f;
		}
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(HandleRigidbodyCollision));
		SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
		speculativeRigidbody2.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Combine(speculativeRigidbody2.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(HandleTileCollision));
		RemainingEnemiesToHit = new Queue<AIActor>();
		GatherTargetEnemies();
		if (RemainingEnemiesToHit.Count > 0)
		{
			AIActor aIActor = RemainingEnemiesToHit.Peek();
			if (aIActor != null)
			{
				Vector2 v = aIActor.CenterPosition - base.transform.position.XY();
				float current = BraveMathCollege.Atan2Degrees(base.specRigidbody.Velocity);
				float target = BraveMathCollege.Atan2Degrees(v);
				float zAngle = Mathf.DeltaAngle(current, target);
				base.transform.Rotate(0f, 0f, zAngle);
			}
		}
		else if ((bool)base.Owner && base.Owner is PlayerController && (bool)base.Owner.CurrentGun)
		{
			float current2 = BraveMathCollege.Atan2Degrees(base.specRigidbody.Velocity);
			float currentAngle = base.Owner.CurrentGun.CurrentAngle;
			float zAngle2 = Mathf.DeltaAngle(current2, currentAngle);
			base.transform.Rotate(0f, 0f, zAngle2);
		}
	}

	private void GatherTargetEnemies()
	{
		List<AIActor> activeEnemies = base.transform.position.GetAbsoluteRoom().GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		Vector2 vector = base.transform.position.XY();
		throwerPlayer = GameManager.Instance.GetActivePlayerClosestToPoint(vector);
		if (UsesMouseAimPoint)
		{
			vector = throwerPlayer.unadjustedAimPoint.XY();
		}
		if (activeEnemies == null || RemainingEnemiesToHit == null)
		{
			return;
		}
		while (RemainingEnemiesToHit.Count < activeEnemies.Count && (MaximumNumberOfTargets <= 0 || RemainingEnemiesToHit.Count < MaximumNumberOfTargets))
		{
			AIActor aIActor = null;
			aIActor = ((!(MaximumTraversalDistance > 0f) || RemainingEnemiesToHit.Count <= 0) ? BraveUtility.GetClosestToPosition(activeEnemies, vector, RemainingEnemiesToHit.ToArray()) : BraveUtility.GetClosestToPosition(activeEnemies, vector, null, MaximumTraversalDistance, RemainingEnemiesToHit.ToArray()));
			if (aIActor == null)
			{
				break;
			}
			RemainingEnemiesToHit.Enqueue(aIActor);
			vector = aIActor.CenterPosition;
		}
	}

	private void HandleTileCollision(CollisionData tileCollision)
	{
		if (RemainingEnemiesToHit.Count == 0)
		{
			if (m_elapsedTargetlessTime >= 5f)
			{
				DieInAir();
			}
			else if (m_targetlessTime <= 0f)
			{
				m_targetlessTime = 0.2f;
			}
		}
		else if (m_elapsedTargetlessTime >= 1f)
		{
			m_elapsedTargetlessTime = 0f;
			m_targetlessTime = 0f;
			RemainingEnemiesToHit.Dequeue();
		}
		else if (m_targetlessTime <= 0f)
		{
			m_targetlessTime = 0.5f;
		}
	}

	private void HandleRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		if ((bool)rigidbodyCollision.OtherRigidbody.aiActor)
		{
			if ((bool)rigidbodyCollision.OtherRigidbody.aiActor.behaviorSpeculator)
			{
				rigidbodyCollision.OtherRigidbody.aiActor.behaviorSpeculator.Stun(StunDuration);
			}
			if (RemainingEnemiesToHit.Count > 0 && RemainingEnemiesToHit.Peek() == rigidbodyCollision.OtherRigidbody.aiActor)
			{
				m_elapsedTargetlessTime = 0f;
				m_targetlessTime = 0f;
				RemainingEnemiesToHit.Dequeue();
			}
		}
	}

	protected override void Move()
	{
		GameActor gameActor = null;
		gameActor = ((RemainingEnemiesToHit.Count <= 0) ? ((GameActor)throwerPlayer) : ((GameActor)RemainingEnemiesToHit.Peek()));
		if (m_targetlessTime <= 0f)
		{
			if (gameActor != null)
			{
				Vector2 v = gameActor.CenterPosition - base.transform.position.XY();
				float current = BraveMathCollege.Atan2Degrees(base.specRigidbody.Velocity);
				float target = BraveMathCollege.Atan2Degrees(v);
				float f = Mathf.DeltaAngle(current, target);
				float zAngle = Mathf.Min(Mathf.Abs(f), trackingSpeed * BraveTime.DeltaTime) * Mathf.Sign(f);
				base.transform.Rotate(0f, 0f, zAngle);
			}
		}
		else
		{
			m_targetlessTime -= BraveTime.DeltaTime;
			m_elapsedTargetlessTime += BraveTime.DeltaTime;
		}
		base.specRigidbody.Velocity = base.transform.right * baseData.speed;
		base.LastVelocity = base.specRigidbody.Velocity;
		if (gameActor == throwerPlayer && Vector2.Distance(gameActor.CenterPosition, base.transform.position) < 1f)
		{
			DieInAir(true);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
