using System;
using FullInspector;
using UnityEngine;

public class ShootBehavior : BasicAttackBehavior
{
	public enum StopType
	{
		None,
		Tell,
		Attack,
		Charge,
		TellOnly
	}

	private enum State
	{
		Idle,
		WaitingForCharge,
		WaitingForTell,
		Firing,
		WaitingForPostAnim
	}

	public enum TargetAreaOrigin
	{
		HitboxCenter,
		ShootPoint
	}

	public abstract class FiringAreaStyle
	{
		public TargetAreaOrigin targetAreaOrigin;

		public abstract bool TargetInFiringArea(Vector2 origin, Vector2 targetCenter);

		public abstract void DrawDebugLines(Vector2 origin, Vector2 targetCenter, AIActor actor);
	}

	public class ArcFiringArea : FiringAreaStyle
	{
		public float StartAngle;

		public float SweepAngle;

		public override bool TargetInFiringArea(Vector2 origin, Vector2 targetCenter)
		{
			return BraveMathCollege.IsAngleWithinSweepArea((targetCenter - origin).ToAngle(), StartAngle, SweepAngle);
		}

		public override void DrawDebugLines(Vector2 origin, Vector2 targetCenter, AIActor actor)
		{
			BasicAttackBehavior.m_arcCount++;
		}
	}

	public class RectFiringArea : FiringAreaStyle
	{
		public Vector2 AreaOriginOffset;

		public Vector2 AreaDimensions;

		private Vector2 offset
		{
			get
			{
				Vector2 areaOriginOffset = AreaOriginOffset;
				if (AreaDimensions.x < 0f)
				{
					areaOriginOffset.x += AreaDimensions.x;
				}
				if (AreaDimensions.y < 0f)
				{
					areaOriginOffset.y += AreaDimensions.y;
				}
				return areaOriginOffset;
			}
		}

		private Vector2 dimensions
		{
			get
			{
				return new Vector2(Mathf.Abs(AreaDimensions.x), Mathf.Abs(AreaDimensions.y));
			}
		}

		public override bool TargetInFiringArea(Vector2 origin, Vector2 targetCenter)
		{
			origin += offset;
			return !(targetCenter.x < origin.x) && !(targetCenter.x > origin.x + dimensions.x) && !(targetCenter.y < origin.y) && !(targetCenter.y > origin.y + dimensions.y);
		}

		public override void DrawDebugLines(Vector2 origin, Vector2 targetCenter, AIActor actor)
		{
			origin += offset;
		}
	}

	public GameObject ShootPoint;

	[InspectorShowIf("ShowBulletScript")]
	public BulletScriptSelector BulletScript;

	[InspectorShowIf("ShowBulletName")]
	public string BulletName;

	[InspectorShowIf("IsSingleBullet")]
	public float LeadAmount;

	public StopType StopDuring;

	[InspectorShowIf("ShowImmobileDuringStop")]
	public bool ImmobileDuringStop;

	public float MoveSpeedModifier = 1f;

	public bool LockFacingDirection;

	[InspectorIndent]
	[InspectorShowIf("LockFacingDirection")]
	public bool ContinueAimingDuringTell;

	[InspectorIndent]
	[InspectorShowIf("LockFacingDirection")]
	public bool ReaimOnFire;

	public bool MultipleFireEvents;

	public bool RequiresTarget = true;

	public bool PreventTargetSwitching;

	public bool Uninterruptible;

	public bool ClearGoop;

	[InspectorIndent]
	[InspectorShowIf("ClearGoop")]
	public float ClearGoopRadius = 2f;

	[InspectorShowIf("ShowBulletName")]
	public bool ShouldOverrideFireDirection;

	[InspectorIndent]
	[InspectorShowIf("ShowOverrideFireDirection")]
	public float OverrideFireDirection;

	[InspectorCategory("Visuals")]
	public AIAnimator SpecifyAiAnimator;

	[InspectorCategory("Visuals")]
	public string ChargeAnimation;

	[InspectorCategory("Visuals")]
	[InspectorShowIf("ShowChargeTime")]
	public float ChargeTime;

	[InspectorCategory("Visuals")]
	public string TellAnimation;

	[InspectorCategory("Visuals")]
	public string FireAnimation;

	[InspectorCategory("Visuals")]
	public string PostFireAnimation;

	[InspectorCategory("Visuals")]
	public bool HideGun = true;

	[InspectorCategory("Visuals")]
	public bool OverrideBaseAnims;

	[InspectorShowIf("OverrideBaseAnims")]
	[InspectorIndent]
	[InspectorCategory("Visuals")]
	public string OverrideIdleAnim;

	[InspectorIndent]
	[InspectorCategory("Visuals")]
	[InspectorShowIf("OverrideBaseAnims")]
	public string OverrideMoveAnim;

	[InspectorCategory("Visuals")]
	public bool UseVfx;

	[InspectorCategory("Visuals")]
	[InspectorShowIf("UseVfx")]
	[InspectorIndent]
	public string ChargeVfx;

	[InspectorShowIf("UseVfx")]
	[InspectorCategory("Visuals")]
	[InspectorIndent]
	public string TellVfx;

	[InspectorCategory("Visuals")]
	[InspectorShowIf("UseVfx")]
	[InspectorIndent]
	public string FireVfx;

	[InspectorIndent]
	[InspectorCategory("Visuals")]
	[InspectorShowIf("UseVfx")]
	public string Vfx;

	[InspectorCategory("Visuals")]
	public GameObject[] EnabledDuringAttack;

	private SpeculativeRigidbody m_specRigidbody;

	private AIBulletBank m_bulletBank;

	private BulletScriptSource m_bulletSource;

	private float m_chargeTimer;

	private bool m_beganInactive;

	private bool m_isAimLocked;

	private float m_cachedMovementSpeed;

	private Vector2 m_cachedTargetCenter;

	private int m_goopExceptionId = -1;

	private State m_state;

	public bool IsBulletScript
	{
		get
		{
			return BulletScript != null && !string.IsNullOrEmpty(BulletScript.scriptTypeName);
		}
	}

	public bool IsSingleBullet
	{
		get
		{
			return !string.IsNullOrEmpty(BulletName);
		}
	}

	public bool IsBulletScriptEnded
	{
		get
		{
			if (IsBulletScript)
			{
				return m_bulletSource.IsEnded;
			}
			if (IsSingleBullet)
			{
				return true;
			}
			return true;
		}
	}

	private State state
	{
		get
		{
			return m_state;
		}
		set
		{
			if (m_state != value)
			{
				EndState(m_state);
				m_state = value;
				BeginState(m_state);
			}
		}
	}

	private bool ShowBulletScript()
	{
		return string.IsNullOrEmpty(BulletName);
	}

	private bool ShowBulletName()
	{
		return BulletScript == null || BulletScript.IsNull;
	}

	private bool ShowImmobileDuringStop()
	{
		return StopDuring != StopType.None;
	}

	private bool ShowChargeTime()
	{
		return !string.IsNullOrEmpty(ChargeAnimation);
	}

	private bool ShowOverrideFireDirection()
	{
		return ShowBulletName() && ShouldOverrideFireDirection;
	}

	public override void Start()
	{
		base.Start();
		if ((bool)SpecifyAiAnimator)
		{
			m_aiAnimator = SpecifyAiAnimator;
		}
		if (!string.IsNullOrEmpty(TellAnimation))
		{
			tk2dSpriteAnimator spriteAnimator = m_aiAnimator.spriteAnimator;
			spriteAnimator.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimEventTriggered));
			if ((bool)m_aiAnimator.ChildAnimator)
			{
				tk2dSpriteAnimator spriteAnimator2 = m_aiAnimator.ChildAnimator.spriteAnimator;
				spriteAnimator2.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(spriteAnimator2.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(AnimEventTriggered));
			}
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		if (state == State.WaitingForCharge)
		{
			DecrementTimer(ref m_chargeTimer);
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
		if (RequiresTarget && m_behaviorSpeculator.TargetRigidbody == null)
		{
			return BehaviorResult.Continue;
		}
		if (UseVfx && !string.IsNullOrEmpty(Vfx))
		{
			m_aiAnimator.PlayVfx(Vfx);
		}
		if (!m_gameObject.activeSelf)
		{
			m_gameObject.SetActive(true);
			m_beganInactive = true;
		}
		if ((bool)m_behaviorSpeculator.TargetRigidbody)
		{
			m_cachedTargetCenter = m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
		if (ClearGoop)
		{
			SetGoopClearing(true);
		}
		state = State.Idle;
		if (!string.IsNullOrEmpty(ChargeAnimation))
		{
			m_aiAnimator.PlayUntilFinished(ChargeAnimation, true);
			state = State.WaitingForCharge;
		}
		else if (!string.IsNullOrEmpty(TellAnimation))
		{
			if (!string.IsNullOrEmpty(TellAnimation))
			{
				m_aiAnimator.PlayUntilCancelled(TellAnimation, true);
			}
			else
			{
				m_aiAnimator.PlayUntilFinished(TellAnimation, true);
			}
			state = State.WaitingForTell;
			if (HideGun && (bool)m_aiShooter)
			{
				m_aiShooter.ToggleGunAndHandRenderers(false, "ShootBulletScript");
			}
		}
		else
		{
			Fire();
		}
		if (MoveSpeedModifier != 1f)
		{
			m_cachedMovementSpeed = m_aiActor.MovementSpeed;
			m_aiActor.MovementSpeed *= MoveSpeedModifier;
		}
		if (LockFacingDirection)
		{
			m_aiAnimator.FacingDirection = (m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - m_specRigidbody.GetUnitCenter(ColliderType.HitBox)).ToAngle();
			m_aiAnimator.LockFacingDirection = true;
		}
		if (PreventTargetSwitching && (bool)m_aiActor)
		{
			m_aiActor.SuppressTargetSwitch = true;
		}
		m_updateEveryFrame = true;
		if (OverrideBaseAnims && (bool)m_aiAnimator)
		{
			if (!string.IsNullOrEmpty(OverrideIdleAnim))
			{
				m_aiAnimator.OverrideIdleAnimation = OverrideIdleAnim;
			}
			if (!string.IsNullOrEmpty(OverrideMoveAnim))
			{
				m_aiAnimator.OverrideMoveAnimation = OverrideMoveAnim;
			}
		}
		if (StopDuring == StopType.None || StopDuring == StopType.TellOnly)
		{
			return BehaviorResult.RunContinuousInClass;
		}
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if ((bool)m_behaviorSpeculator.TargetRigidbody)
		{
			m_cachedTargetCenter = m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
		if (state == State.WaitingForCharge)
		{
			if ((ChargeTime > 0f && m_chargeTimer <= 0f) || (ChargeTime <= 0f && !m_aiAnimator.IsPlaying(ChargeAnimation)))
			{
				if (!string.IsNullOrEmpty(TellAnimation))
				{
					m_aiAnimator.PlayUntilFinished(TellAnimation, true);
					state = State.WaitingForTell;
				}
				else
				{
					Fire();
				}
			}
			return ContinuousBehaviorResult.Continue;
		}
		if (state == State.WaitingForTell)
		{
			if (LockFacingDirection && ContinueAimingDuringTell && !m_isAimLocked && (bool)m_behaviorSpeculator.TargetRigidbody)
			{
				m_aiAnimator.FacingDirection = (m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - m_specRigidbody.GetUnitCenter(ColliderType.HitBox)).ToAngle();
			}
			if (!m_aiAnimator.IsPlaying(TellAnimation))
			{
				Fire();
			}
			return ContinuousBehaviorResult.Continue;
		}
		if (state == State.Firing)
		{
			if (!IsBulletScriptEnded)
			{
				return ContinuousBehaviorResult.Continue;
			}
			tk2dSpriteAnimationClip.WrapMode wrapMode;
			if (!string.IsNullOrEmpty(TellAnimation) && m_aiAnimator.IsPlaying(TellAnimation) && m_aiAnimator.GetWrapType(TellAnimation, out wrapMode) && wrapMode == tk2dSpriteAnimationClip.WrapMode.Once)
			{
				return ContinuousBehaviorResult.Continue;
			}
			if (!string.IsNullOrEmpty(FireAnimation) && m_aiAnimator.IsPlaying(FireAnimation) && m_aiAnimator.GetWrapType(FireAnimation, out wrapMode) && wrapMode == tk2dSpriteAnimationClip.WrapMode.Once)
			{
				return ContinuousBehaviorResult.Continue;
			}
			if (!string.IsNullOrEmpty(PostFireAnimation))
			{
				state = State.WaitingForPostAnim;
				m_aiAnimator.PlayUntilFinished(PostFireAnimation);
				return ContinuousBehaviorResult.Continue;
			}
			return ContinuousBehaviorResult.Finished;
		}
		if (state == State.WaitingForPostAnim)
		{
			return (!m_aiAnimator.IsPlaying(PostFireAnimation)) ? ContinuousBehaviorResult.Finished : ContinuousBehaviorResult.Continue;
		}
		return ContinuousBehaviorResult.Finished;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		CeaseFire();
		if (ClearGoop)
		{
			SetGoopClearing(false);
		}
		if (HideGun && (bool)m_aiShooter)
		{
			m_aiShooter.ToggleGunAndHandRenderers(true, "ShootBulletScript");
		}
		if (!string.IsNullOrEmpty(ChargeAnimation))
		{
			m_aiAnimator.EndAnimationIf(ChargeAnimation);
		}
		if (!string.IsNullOrEmpty(TellAnimation))
		{
			m_aiAnimator.EndAnimationIf(TellAnimation);
		}
		if (!string.IsNullOrEmpty(FireAnimation))
		{
			m_aiAnimator.EndAnimationIf(FireAnimation);
		}
		if (UseVfx && !string.IsNullOrEmpty(Vfx))
		{
			m_aiAnimator.StopVfx(Vfx);
		}
		if (UseVfx && !string.IsNullOrEmpty(ChargeVfx))
		{
			m_aiAnimator.StopVfx(ChargeVfx);
		}
		if (UseVfx && !string.IsNullOrEmpty(TellVfx))
		{
			m_aiAnimator.StopVfx(TellVfx);
		}
		if (UseVfx && !string.IsNullOrEmpty(FireVfx))
		{
			m_aiAnimator.StopVfx(FireVfx);
		}
		if (EnabledDuringAttack != null)
		{
			for (int i = 0; i < EnabledDuringAttack.Length; i++)
			{
				EnabledDuringAttack[i].SetActive(false);
			}
		}
		if (m_beganInactive)
		{
			m_aiAnimator.gameObject.SetActive(false);
			m_beganInactive = false;
		}
		if (MoveSpeedModifier != 1f)
		{
			m_aiActor.MovementSpeed = m_cachedMovementSpeed;
		}
		if (StopDuring == StopType.TellOnly)
		{
			m_behaviorSpeculator.PreventMovement = false;
		}
		if ((bool)m_aiActor && StopDuring != 0 && ImmobileDuringStop)
		{
			m_aiActor.knockbackDoer.SetImmobile(false, "ShootBulletScript");
		}
		if (LockFacingDirection)
		{
			m_aiAnimator.LockFacingDirection = false;
		}
		if (PreventTargetSwitching && (bool)m_aiActor)
		{
			m_aiActor.SuppressTargetSwitch = false;
		}
		if (OverrideBaseAnims && (bool)m_aiAnimator)
		{
			if (!string.IsNullOrEmpty(OverrideIdleAnim))
			{
				m_aiAnimator.OverrideIdleAnimation = null;
			}
			if (!string.IsNullOrEmpty(OverrideMoveAnim))
			{
				m_aiAnimator.OverrideMoveAnimation = null;
			}
		}
		m_updateEveryFrame = false;
		state = State.Idle;
		UpdateCooldowns();
	}

	public override void Init(GameObject gameObject, AIActor aiActor, AIShooter aiShooter)
	{
		base.Init(gameObject, aiActor, aiShooter);
		m_specRigidbody = m_behaviorSpeculator.specRigidbody;
		m_bulletBank = m_behaviorSpeculator.bulletBank;
	}

	public override bool IsOverridable()
	{
		return !Uninterruptible;
	}

	private void Fire()
	{
		if (LockFacingDirection && ReaimOnFire && (bool)m_behaviorSpeculator.TargetRigidbody)
		{
			m_aiAnimator.FacingDirection = (m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - m_specRigidbody.GetUnitCenter(ColliderType.HitBox)).ToAngle();
		}
		if (!string.IsNullOrEmpty(FireAnimation))
		{
			m_aiAnimator.EndAnimation();
			m_aiAnimator.PlayUntilFinished(FireAnimation);
		}
		if (UseVfx && !string.IsNullOrEmpty(FireVfx))
		{
			m_aiAnimator.PlayVfx(FireVfx);
		}
		SpawnProjectiles();
		if (EnabledDuringAttack != null)
		{
			for (int i = 0; i < EnabledDuringAttack.Length; i++)
			{
				EnabledDuringAttack[i].SetActive(true);
			}
		}
		if (StopDuring == StopType.TellOnly)
		{
			m_behaviorSpeculator.PreventMovement = false;
			if ((bool)m_aiActor && ImmobileDuringStop)
			{
				m_aiActor.knockbackDoer.SetImmobile(false, "ShootBulletScript");
			}
		}
		else if (StopDuring != 0)
		{
			StopMoving();
		}
		state = State.Firing;
		if (HideGun && (bool)m_aiShooter)
		{
			m_aiShooter.ToggleGunAndHandRenderers(false, "ShootBulletScript");
		}
	}

	private void CeaseFire()
	{
		if (IsBulletScript && (bool)m_bulletSource && !m_bulletSource.IsEnded)
		{
			m_bulletSource.ForceStop();
		}
	}

	private void StopMoving()
	{
		if ((bool)m_aiActor)
		{
			m_aiActor.ClearPath();
			if (StopDuring == StopType.TellOnly)
			{
				m_behaviorSpeculator.PreventMovement = true;
			}
			if (ImmobileDuringStop)
			{
				m_aiActor.knockbackDoer.SetImmobile(true, "ShootBulletScript");
			}
		}
	}

	protected override Vector2 GetOrigin(TargetAreaOrigin origin)
	{
		if (origin == TargetAreaOrigin.ShootPoint)
		{
			return ShootPoint.transform.position.XY();
		}
		return base.GetOrigin(origin);
	}

	private void SpawnProjectiles()
	{
		if (IsBulletScript)
		{
			if (!m_bulletSource)
			{
				m_bulletSource = ShootPoint.GetOrAddComponent<BulletScriptSource>();
			}
			m_bulletSource.BulletManager = m_bulletBank;
			m_bulletSource.BulletScript = BulletScript;
			m_bulletSource.Initialize();
		}
		else
		{
			if (!IsSingleBullet)
			{
				return;
			}
			AIBulletBank.Entry bullet = m_bulletBank.GetBullet(BulletName);
			GameObject bulletObject = bullet.BulletObject;
			Vector2 vector = m_cachedTargetCenter;
			if ((bool)m_behaviorSpeculator.TargetRigidbody)
			{
				vector = m_behaviorSpeculator.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
			}
			float direction;
			if (ShouldOverrideFireDirection)
			{
				direction = OverrideFireDirection;
			}
			else
			{
				if (LeadAmount > 0f)
				{
					Vector2 value = ShootPoint.transform.position;
					float? overrideProjectileSpeed = ((!bullet.OverrideProjectile) ? null : new float?(bullet.ProjectileData.speed));
					Projectile component = bulletObject.GetComponent<Projectile>();
					Vector2 predictedTargetPosition = component.GetPredictedTargetPosition(vector, m_behaviorSpeculator.TargetVelocity, value, overrideProjectileSpeed);
					vector = Vector2.Lerp(vector, predictedTargetPosition, LeadAmount);
				}
				Vector2 vector2 = vector - ShootPoint.transform.position.XY();
				direction = Mathf.Atan2(vector2.y, vector2.x) * 57.29578f;
			}
			GameObject gameObject = m_bulletBank.CreateProjectileFromBank(ShootPoint.transform.position, direction, BulletName);
			if (m_bulletBank.OnProjectileCreatedWithSource != null)
			{
				m_bulletBank.OnProjectileCreatedWithSource(ShootPoint.transform.name, gameObject.GetComponent<Projectile>());
			}
			ArcProjectile component2 = gameObject.GetComponent<ArcProjectile>();
			if ((bool)component2)
			{
				component2.AdjustSpeedToHit(vector);
			}
		}
	}

	private void SetGoopClearing(bool value)
	{
		if (!ClearGoop || !m_aiActor || !m_aiActor.specRigidbody)
		{
			return;
		}
		if (value)
		{
			m_goopExceptionId = DeadlyDeadlyGoopManager.RegisterUngoopableCircle(m_aiActor.specRigidbody.UnitCenter, 2f);
			return;
		}
		if (m_goopExceptionId != -1)
		{
			DeadlyDeadlyGoopManager.DeregisterUngoopableCircle(m_goopExceptionId);
		}
		m_goopExceptionId = -1;
	}

	private void BeginState(State state)
	{
		switch (state)
		{
		case State.WaitingForCharge:
			if (UseVfx && !string.IsNullOrEmpty(ChargeVfx))
			{
				m_aiAnimator.PlayVfx(ChargeVfx);
			}
			if (StopDuring == StopType.Charge)
			{
				StopMoving();
			}
			m_chargeTimer = ChargeTime;
			break;
		case State.WaitingForTell:
			if (UseVfx && !string.IsNullOrEmpty(TellVfx))
			{
				m_aiAnimator.PlayVfx(TellVfx);
			}
			if (StopDuring == StopType.Tell || StopDuring == StopType.TellOnly)
			{
				StopMoving();
			}
			m_isAimLocked = false;
			break;
		}
	}

	private void EndState(State state)
	{
		switch (state)
		{
		case State.WaitingForCharge:
			if (UseVfx && !string.IsNullOrEmpty(ChargeVfx))
			{
				m_aiAnimator.StopVfx(ChargeVfx);
			}
			break;
		case State.WaitingForTell:
			if (UseVfx && !string.IsNullOrEmpty(TellVfx))
			{
				m_aiAnimator.StopVfx(TellVfx);
			}
			if (OverrideBaseAnims)
			{
				if (!string.IsNullOrEmpty(OverrideIdleAnim))
				{
					m_aiAnimator.OverrideIdleAnimation = OverrideIdleAnim;
				}
				if (!string.IsNullOrEmpty(OverrideMoveAnim))
				{
					m_aiAnimator.OverrideMoveAnimation = OverrideMoveAnim;
				}
				if (!string.IsNullOrEmpty(TellAnimation))
				{
					m_aiAnimator.EndAnimationIf(TellAnimation);
				}
			}
			break;
		case State.Firing:
			if (UseVfx && !string.IsNullOrEmpty(FireVfx))
			{
				m_aiAnimator.StopVfx(FireVfx);
			}
			break;
		}
	}

	private void AnimEventTriggered(tk2dSpriteAnimator sprite, tk2dSpriteAnimationClip clip, int frameNum)
	{
		tk2dSpriteAnimationFrame frame = clip.GetFrame(frameNum);
		bool flag = state == State.WaitingForTell;
		if (MultipleFireEvents)
		{
			flag |= state == State.Firing;
		}
		if (flag && frame.eventInfo == "fire")
		{
			Fire();
		}
		if (LockFacingDirection && ContinueAimingDuringTell && frame.eventInfo == "stopAiming")
		{
			m_isAimLocked = true;
		}
	}
}
