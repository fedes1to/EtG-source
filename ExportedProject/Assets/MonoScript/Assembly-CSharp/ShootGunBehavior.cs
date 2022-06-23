using System.Collections.Generic;
using Dungeonator;
using FullInspector;
using UnityEngine;

public class ShootGunBehavior : BasicAttackBehavior
{
	private enum State
	{
		Idle,
		PreFireLaser,
		PreFire,
		Firing,
		WaitingForNextShot
	}

	[InspectorCategory("Conditions")]
	public float GroupCooldownVariance = 0.2f;

	[InspectorCategory("Conditions")]
	public bool LineOfSight = true;

	public WeaponType WeaponType;

	[InspectorIndent]
	[InspectorShowIf("IsAiShooter")]
	public string OverrideBulletName;

	[InspectorShowIf("IsBulletScript")]
	[InspectorIndent]
	public BulletScriptSelector BulletScript;

	[InspectorShowIf("IsComplexBullet")]
	[InspectorIndent]
	public bool FixTargetDuringAttack;

	public bool StopDuringAttack;

	public float LeadAmount;

	[InspectorShowIf("ShowLeadChance")]
	[InspectorIndent]
	public float LeadChance = 1f;

	public bool RespectReload;

	[InspectorIndent]
	[InspectorShowIf("RespectReload")]
	public float MagazineCapacity = 1f;

	[InspectorShowIf("RespectReload")]
	[InspectorIndent]
	public float ReloadSpeed = 1f;

	[InspectorIndent]
	[InspectorShowIf("RespectReload")]
	public bool EmptiesClip = true;

	[InspectorIndent]
	[InspectorShowIf("RespectReload")]
	public bool SuppressReloadAnim;

	[InspectorIndent]
	[InspectorShowIf("ShowTimeBetweenShots")]
	public float TimeBetweenShots = -1f;

	public bool PreventTargetSwitching;

	[InspectorCategory("Visuals")]
	public string OverrideAnimation;

	[InspectorCategory("Visuals")]
	public string OverrideDirectionalAnimation;

	[InspectorShowIf("IsComplexBullet")]
	[InspectorCategory("Visuals")]
	public bool HideGun;

	[InspectorCategory("Visuals")]
	public bool UseLaserSight;

	[InspectorShowIf("UseLaserSight")]
	[InspectorCategory("Visuals")]
	public bool UseGreenLaser;

	[InspectorShowIf("UseLaserSight")]
	[InspectorCategory("Visuals")]
	public float PreFireLaserTime = -1f;

	[InspectorCategory("Visuals")]
	public bool AimAtFacingDirectionWhenSafe;

	private State m_state;

	private LaserSightController m_laserSight;

	private float m_remainingAmmo;

	private float m_reloadTimer;

	private float m_prefireLaserTimer;

	private float m_nextShotTimer;

	private float m_preFireTime;

	private float m_timeSinceLastShot;

	private bool ShouldPreFire
	{
		get
		{
			if (m_preFireTime < Cooldown)
			{
				return true;
			}
			if (m_timeSinceLastShot > Cooldown * 2f)
			{
				return true;
			}
			return false;
		}
	}

	private bool IsAiShooter()
	{
		return WeaponType == WeaponType.AIShooterProjectile;
	}

	private bool IsBulletScript()
	{
		return WeaponType == WeaponType.BulletScript;
	}

	private bool IsComplexBullet()
	{
		return WeaponType != WeaponType.AIShooterProjectile;
	}

	private bool ShowLeadChance()
	{
		return LeadAmount != 0f;
	}

	private bool ShowTimeBetweenShots()
	{
		return RespectReload && EmptiesClip;
	}

	public override void Start()
	{
		base.Start();
		m_remainingAmmo = MagazineCapacity;
		if (UseLaserSight)
		{
			if (UseGreenLaser)
			{
				m_aiActor.CurrentGun.LaserSightIsGreen = true;
			}
			m_aiActor.CurrentGun.ForceLaserSight = true;
		}
		Gun gun = PickupObjectDatabase.GetById(m_aiShooter.equippedGunId) as Gun;
		if ((bool)gun && !string.IsNullOrEmpty(gun.enemyPreFireAnimation))
		{
			tk2dSpriteAnimationClip clipByName = gun.spriteAnimator.GetClipByName(gun.enemyPreFireAnimation);
			m_preFireTime = clipByName.BaseClipLength;
		}
		if (UseLaserSight)
		{
			PhysicsEngine.Instance.OnPostRigidbodyMovement += OnPostRigidbodyMovement;
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_nextShotTimer);
		DecrementTimer(ref m_reloadTimer);
		DecrementTimer(ref m_prefireLaserTimer);
		m_timeSinceLastShot += m_deltaTime;
		if (UseLaserSight && !m_laserSight && (bool)m_aiActor && (bool)m_aiActor.CurrentGun && (bool)m_aiActor.CurrentGun.LaserSight)
		{
			m_laserSight = m_aiActor.CurrentGun.LaserSight.GetComponent<LaserSightController>();
			if (PreFireLaserTime > 0f && m_state != State.PreFireLaser)
			{
				m_laserSight.renderer.enabled = false;
			}
		}
		if (AimAtFacingDirectionWhenSafe && m_behaviorSpeculator.TargetRigidbody == null)
		{
			m_aiShooter.AimInDirection(BraveMathCollege.DegreesToVector(m_aiAnimator.FacingDirection));
		}
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		if (m_behaviorSpeculator.TargetRigidbody == null)
		{
			return BehaviorResult.Continue;
		}
		bool flag = RespectReload && m_reloadTimer > 0f;
		bool flag2 = EmptiesClip && m_remainingAmmo < MagazineCapacity;
		bool flag3 = Range > 0f && m_aiActor.DistanceToTarget > Range && !flag2;
		bool flag4 = LineOfSight && !m_aiActor.HasLineOfSightToTarget && !flag2;
		if (flag || m_aiActor.TargetRigidbody == null || flag3 || flag4)
		{
			m_aiShooter.CeaseAttack();
			return BehaviorResult.Continue;
		}
		BeginAttack();
		if (PreventTargetSwitching)
		{
			m_aiActor.SuppressTargetSwitch = true;
		}
		m_updateEveryFrame = true;
		return (!StopDuringAttack) ? BehaviorResult.RunContinuousInClass : BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_state == State.Idle)
		{
			return ContinuousBehaviorResult.Finished;
		}
		bool flag = LeadAmount > 0f && LeadChance >= 1f;
		if (m_state == State.PreFireLaser && UseLaserSight && m_prefireLaserTimer > 0f)
		{
			flag = false;
		}
		if (m_aiShooter.CurrentGun != null && (bool)m_aiActor.TargetRigidbody)
		{
			Vector2 vector = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			if (flag)
			{
				Vector2 b = FindPredictedTargetPosition();
				vector = Vector2.Lerp(vector, b, LeadAmount);
			}
			m_aiShooter.OverrideAimPoint = vector;
		}
		if (m_state == State.WaitingForNextShot)
		{
			if (m_nextShotTimer <= 0f)
			{
				BeginAttack();
			}
		}
		else if (m_state == State.PreFireLaser)
		{
			if (UseLaserSight && (bool)m_laserSight && PreFireLaserTime > 0f)
			{
				m_laserSight.renderer.enabled = true;
				m_laserSight.UpdateCountdown(m_prefireLaserTimer, PreFireLaserTime);
			}
			if (m_prefireLaserTimer <= 0f)
			{
				if (UseLaserSight && (bool)m_laserSight && PreFireLaserTime > 0f)
				{
					m_laserSight.ResetCountdown();
				}
				m_state = State.PreFire;
				m_aiShooter.StartPreFireAnim();
			}
		}
		else if (m_state == State.PreFire)
		{
			if (m_aiShooter.IsPreFireComplete)
			{
				Fire();
			}
		}
		else if (m_state == State.Firing && IsBulletSourceEnded())
		{
			if (FixTargetDuringAttack)
			{
				m_aiActor.bulletBank.FixedPlayerPosition = null;
			}
			if (!RespectReload || !EmptiesClip || !(m_reloadTimer <= 0f))
			{
				return ContinuousBehaviorResult.Finished;
			}
			m_state = State.WaitingForNextShot;
			m_nextShotTimer = ((!(TimeBetweenShots > 0f)) ? Cooldown : TimeBetweenShots);
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		m_updateEveryFrame = false;
		m_state = State.Idle;
		if (HideGun)
		{
			m_aiShooter.ToggleGunRenderers(true, "ShootGunBehavior");
		}
		m_aiShooter.OverrideAimPoint = null;
		if (FixTargetDuringAttack)
		{
			m_aiActor.bulletBank.FixedPlayerPosition = null;
		}
		if (PreventTargetSwitching)
		{
			m_aiActor.SuppressTargetSwitch = false;
		}
		if (!string.IsNullOrEmpty(OverrideDirectionalAnimation))
		{
			m_aiAnimator.EndAnimationIf(OverrideDirectionalAnimation);
		}
		else if (!string.IsNullOrEmpty(OverrideAnimation))
		{
			m_aiAnimator.EndAnimationIf(OverrideAnimation);
		}
		if (UseLaserSight && (bool)m_laserSight)
		{
			m_laserSight.ResetCountdown();
		}
		UpdateCooldowns();
	}

	public override bool IsReady()
	{
		if (!base.IsReady())
		{
			return false;
		}
		if (RespectReload && m_reloadTimer > 0f)
		{
			return false;
		}
		return true;
	}

	protected override void UpdateCooldowns()
	{
		base.UpdateCooldowns();
		if (GroupCooldownVariance > 0f)
		{
			List<AIActor> activeEnemies = m_aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
			for (int i = 0; i < activeEnemies.Count; i++)
			{
				if (!(activeEnemies[i] == m_aiActor) && (activeEnemies[i].specRigidbody.UnitCenter - m_aiActor.specRigidbody.UnitCenter).sqrMagnitude < 6.25f)
				{
					m_cooldownTimer += Random.value * GroupCooldownVariance;
					break;
				}
			}
		}
		if (m_preFireTime < Cooldown)
		{
			m_cooldownTimer = Mathf.Max(0f, m_cooldownTimer - m_preFireTime);
		}
	}

	private Vector2 FindPredictedTargetPosition()
	{
		AIBulletBank.Entry bulletEntry = m_aiShooter.GetBulletEntry(OverrideBulletName);
		float num = float.MaxValue;
		num = ((!bulletEntry.OverrideProjectile) ? bulletEntry.BulletObject.GetComponent<Projectile>().baseData.speed : bulletEntry.ProjectileData.speed);
		if (num < 0f)
		{
			num = float.MaxValue;
		}
		Vector2 unitCenter = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
		Vector2 targetVelocity = m_aiActor.TargetVelocity;
		Vector2 unitCenter2 = m_aiActor.specRigidbody.UnitCenter;
		return BraveMathCollege.GetPredictedPosition(unitCenter, targetVelocity, unitCenter2, num);
	}

	private bool IsBulletSourceEnded()
	{
		if (WeaponType == WeaponType.BulletScript)
		{
			return m_aiShooter.BraveBulletSource.IsEnded;
		}
		return true;
	}

	private void BeginAttack()
	{
		if (UseLaserSight && PreFireLaserTime > 0f)
		{
			m_state = State.PreFireLaser;
			m_prefireLaserTimer = PreFireLaserTime;
		}
		else if (ShouldPreFire)
		{
			m_state = State.PreFire;
			m_aiShooter.StartPreFireAnim();
		}
		else
		{
			Fire();
		}
	}

	private void Fire()
	{
		m_timeSinceLastShot = 0f;
		switch (WeaponType)
		{
		case WeaponType.AIShooterProjectile:
			HandleAIShoot();
			break;
		case WeaponType.BulletScript:
			m_aiShooter.ShootBulletScript(BulletScript);
			break;
		}
		if (RespectReload)
		{
			m_remainingAmmo -= 1f;
			if (m_remainingAmmo == 0f)
			{
				m_remainingAmmo = MagazineCapacity;
				m_reloadTimer = ReloadSpeed;
				if (!SuppressReloadAnim)
				{
					m_aiShooter.Reload();
				}
			}
		}
		if (!string.IsNullOrEmpty(OverrideDirectionalAnimation))
		{
			m_aiAnimator.PlayUntilFinished(OverrideDirectionalAnimation, true);
		}
		else if (!string.IsNullOrEmpty(OverrideAnimation))
		{
			m_aiAnimator.PlayUntilFinished(OverrideAnimation);
		}
		if (IsComplexBullet())
		{
			if (StopDuringAttack)
			{
				m_aiActor.ClearPath();
			}
			if (FixTargetDuringAttack && (bool)m_aiActor.TargetRigidbody)
			{
				m_aiActor.bulletBank.FixedPlayerPosition = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			}
			if (HideGun)
			{
				m_aiShooter.ToggleGunRenderers(false, "ShootGunBehavior");
			}
			m_state = State.Firing;
		}
		else if (RespectReload && EmptiesClip && m_reloadTimer <= 0f)
		{
			m_state = State.WaitingForNextShot;
			m_nextShotTimer = ((!(TimeBetweenShots > 0f)) ? Cooldown : TimeBetweenShots);
		}
		else
		{
			m_state = State.Idle;
		}
	}

	private void HandleAIShoot()
	{
		if (!(LeadAmount > 0f) || (!(LeadChance >= 1f) && !(Random.value < LeadChance)))
		{
			m_aiShooter.ShootAtTarget(OverrideBulletName);
		}
		else if ((bool)m_aiActor.TargetRigidbody)
		{
			PixelCollider pixelCollider = m_aiActor.TargetRigidbody.GetPixelCollider(ColliderType.HitBox);
			Vector2 a = ((pixelCollider == null) ? m_aiActor.TargetRigidbody.UnitCenter : pixelCollider.UnitCenter);
			Vector2 b = FindPredictedTargetPosition();
			Vector2 vector = Vector2.Lerp(a, b, LeadAmount);
			if (m_aiShooter.CurrentGun == null)
			{
				m_aiShooter.ShootInDirection(vector - m_aiShooter.specRigidbody.UnitCenter);
				return;
			}
			m_aiShooter.OverrideAimPoint = vector;
			m_aiShooter.AimAtPoint(vector);
			m_aiShooter.Shoot(OverrideBulletName);
			m_aiShooter.OverrideAimPoint = null;
		}
	}

	private void OnPostRigidbodyMovement()
	{
		if (m_state == State.PreFireLaser && UseLaserSight && m_prefireLaserTimer > 0f && m_aiShooter.CurrentGun != null && (bool)m_aiActor.TargetRigidbody)
		{
			m_aiShooter.OverrideAimPoint = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			m_aiShooter.AimAtOverride();
		}
	}

	public override void OnActorPreDeath()
	{
		if (UseLaserSight && PhysicsEngine.HasInstance)
		{
			PhysicsEngine.Instance.OnPostRigidbodyMovement -= OnPostRigidbodyMovement;
		}
		base.OnActorPreDeath();
	}
}
