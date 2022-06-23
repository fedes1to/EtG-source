using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class BulletKingToadieController : BraveBehaviour
{
	public bool ShouldCry;

	public bool CanReturnAngry;

	public float AutoCrazeTime = -1f;

	public float PostCrazeHealth = 1f;

	public tk2dSpriteAnimator scepterAnimator;

	public Transform hatPoint;

	public GameObject hatVfx;

	private bool m_isCrazed;

	private bool m_isCrying;

	private float m_timer;

	public AIActor MyKing { get; set; }

	public void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(PreRigidbodyCollision));
		base.healthHaver.OnDamaged += OnDamaged;
		if (ShouldCry)
		{
			base.aiActor.PreventAutoKillOnBossDeath = true;
		}
		if (CanReturnAngry)
		{
			base.healthHaver.OnDeath += OnDeath;
		}
		List<AIActor> activeEnemies = base.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if ((bool)activeEnemies[i] && (bool)activeEnemies[i].healthHaver && activeEnemies[i].healthHaver.IsBoss)
			{
				MyKing = activeEnemies[i];
				break;
			}
		}
	}

	public void Update()
	{
		if ((bool)this && (bool)base.healthHaver && base.healthHaver.IsDead)
		{
			return;
		}
		if (ShouldCry && (bool)MyKing && MyKing.healthHaver.IsDead)
		{
			if (!m_isCrazed && (bool)scepterAnimator)
			{
				scepterAnimator.Play("scepter_drop");
				scepterAnimator.transform.parent = SpawnManager.Instance.VFX;
			}
			base.aiAnimator.LockFacingDirection = true;
			base.aiAnimator.FacingDirection = (MyKing.CenterPosition - base.aiActor.CenterPosition).ToAngle();
			base.aiAnimator.PlayUntilCancelled("cry");
			base.aiActor.DiesOnCollison = true;
			base.aiActor.CollisionDamage = 0f;
			base.aiActor.healthHaver.ForceSetCurrentHealth(PostCrazeHealth);
			base.aiActor.ClearPath();
			base.aiActor.BehaviorVelocity = Vector2.zero;
			base.behaviorSpeculator.InterruptAndDisable();
			if (CanReturnAngry && GameManager.HasInstance && (bool)this && (bool)base.healthHaver && base.healthHaver.IsAlive)
			{
				GameManager.Instance.RunData.SpawnAngryToadie = true;
			}
			MyKing = null;
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(PreRigidbodyCollision));
			m_isCrying = true;
		}
		if (!m_isCrazed && !m_isCrying && AutoCrazeTime > 0f)
		{
			m_timer += BraveTime.DeltaTime;
			if (m_timer > AutoCrazeTime)
			{
				LoseHat();
				base.aiAnimator.SetBaseAnim("crazed");
				base.behaviorSpeculator.enabled = true;
				MakeVulnerable();
				m_isCrazed = true;
				SpeculativeRigidbody speculativeRigidbody2 = base.specRigidbody;
				speculativeRigidbody2.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody2.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(PreRigidbodyCollision));
				base.healthHaver.OnDamaged -= OnDamaged;
			}
		}
	}

	protected override void OnDestroy()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(PreRigidbodyCollision));
		base.healthHaver.OnDamaged -= OnDamaged;
		if (CanReturnAngry)
		{
			base.healthHaver.OnDeath -= OnDeath;
		}
		base.OnDestroy();
	}

	private void PreRigidbodyCollision(SpeculativeRigidbody myRigidbody, PixelCollider myPixelCollider, SpeculativeRigidbody otherRigidbody, PixelCollider otherPixelCollider)
	{
		Projectile projectile = otherRigidbody.projectile;
		if ((bool)projectile && projectile.Owner is PlayerController)
		{
			WasHit(otherRigidbody.Velocity);
		}
		else if ((bool)otherRigidbody.gameActor)
		{
			PhysicsEngine.SkipCollision = true;
		}
	}

	private void OnDamaged(float resultvalue, float maxvalue, CoreDamageTypes damagetypes, DamageCategory damagecategory, Vector2 damagedirection)
	{
		WasHit(damagedirection);
	}

	private void OnDeath(Vector2 vector2)
	{
		if (CanReturnAngry && GameManager.HasInstance)
		{
			GameManager.Instance.RunData.SpawnAngryToadie = false;
		}
	}

	private void WasHit(Vector2 hatDirection)
	{
		if (!m_isCrazed)
		{
			DropScepter();
			LoseHat(hatDirection);
			base.aiAnimator.SetBaseAnim("crazed");
			base.behaviorSpeculator.enabled = true;
			m_isCrazed = true;
			Invoke("MakeVulnerable", 1f);
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnPreRigidbodyCollision = (SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate)Delegate.Remove(speculativeRigidbody.OnPreRigidbodyCollision, new SpeculativeRigidbody.OnPreRigidbodyCollisionDelegate(PreRigidbodyCollision));
			base.healthHaver.OnDamaged -= OnDamaged;
		}
	}

	private void DropScepter()
	{
		if ((bool)scepterAnimator)
		{
			scepterAnimator.Play("scepter_drop");
			scepterAnimator.transform.parent = SpawnManager.Instance.VFX;
		}
	}

	private void LoseHat(Vector2? hatDirection = null)
	{
		if ((bool)hatPoint && (bool)hatVfx)
		{
			if (!hatDirection.HasValue)
			{
				hatDirection = Vector2.one;
			}
			GameObject gameObject = SpawnManager.SpawnVFX(hatVfx, hatPoint.position, Quaternion.identity);
			gameObject.transform.parent = SpawnManager.Instance.VFX;
			DebrisObject orAddComponent = gameObject.GetOrAddComponent<DebrisObject>();
			orAddComponent.angularVelocity = 270f;
			orAddComponent.angularVelocityVariance = 0f;
			orAddComponent.decayOnBounce = 0.5f;
			orAddComponent.bounceCount = 3;
			orAddComponent.canRotate = true;
			orAddComponent.Trigger(hatDirection.Value.normalized * 10f, 1f);
		}
	}

	private void MakeVulnerable()
	{
		base.healthHaver.IsVulnerable = true;
		base.healthHaver.ForceSetCurrentHealth(PostCrazeHealth);
	}
}
