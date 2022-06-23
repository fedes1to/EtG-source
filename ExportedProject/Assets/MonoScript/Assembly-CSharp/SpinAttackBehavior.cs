using FullInspector;
using UnityEngine;

public class SpinAttackBehavior : BasicAttackBehavior
{
	private enum SpinState
	{
		None,
		Start,
		Spin,
		End
	}

	public static bool ConcurrentAttackHappening;

	public GameObject ShootPoint;

	public BulletScriptSelector BulletScript;

	public bool PreventConcurrentAttacks;

	[InspectorCategory("Visuals")]
	public string spinBeginAnim = "spin_begin";

	[InspectorCategory("Visuals")]
	public string spinAnim = "spin";

	[InspectorCategory("Visuals")]
	public string spinEndAnim = "spin_end";

	[InspectorCategory("Visuals")]
	public bool SpawnMuzzleVfx;

	[InspectorShowIf("SpawnMuzzleVfx")]
	[InspectorCategory("Visuals")]
	public float VfxMidDelay = 0.1f;

	[InspectorCategory("Visuals")]
	public string spinVfx;

	private SpinState m_state;

	private float m_vfxTimer;

	private BulletScriptSource m_bulletSource;

	public override void Start()
	{
		base.Start();
		if (PreventConcurrentAttacks)
		{
			ConcurrentAttackHappening = false;
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
		m_state = SpinState.Start;
		m_aiAnimator.PlayUntilFinished(spinBeginAnim, true);
		if (PreventConcurrentAttacks)
		{
			ConcurrentAttackHappening = true;
		}
		m_vfxTimer = VfxMidDelay;
		m_aiActor.ClearPath();
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_state == SpinState.Start)
		{
			if (!m_aiAnimator.IsPlaying(spinBeginAnim))
			{
				m_state = SpinState.Spin;
				m_aiAnimator.PlayUntilFinished(spinAnim, true);
				if (!string.IsNullOrEmpty(spinVfx))
				{
					m_aiAnimator.PlayVfx(spinVfx);
				}
				ShootBulletScript();
			}
		}
		else if (m_state == SpinState.Spin)
		{
			if (SpawnMuzzleVfx)
			{
				m_vfxTimer -= m_deltaTime;
				if (VfxMidDelay > 0f && m_vfxTimer <= 0f)
				{
					VFXPool muzzleFlashEffects = m_aiActor.bulletBank.GetBullet().MuzzleFlashEffects;
					PixelCollider hitboxPixelCollider = m_aiActor.specRigidbody.HitboxPixelCollider;
					Vector2 unitBottomLeft = hitboxPixelCollider.UnitBottomLeft;
					Vector2 unitCenter = hitboxPixelCollider.UnitCenter;
					Vector2 unitTopRight = hitboxPixelCollider.UnitTopRight;
					while (m_vfxTimer <= 0f)
					{
						Vector2 vector = BraveUtility.RandomVector2(unitBottomLeft, unitTopRight, new Vector2(-1.5f, -1.5f));
						float zRotation = (vector - unitCenter).ToAngle();
						muzzleFlashEffects.SpawnAtLocalPosition(vector - ShootPoint.transform.position.XY(), zRotation, ShootPoint.transform);
						m_vfxTimer += VfxMidDelay;
					}
				}
			}
			if (m_bulletSource.IsEnded)
			{
				m_state = SpinState.End;
				if (!string.IsNullOrEmpty(spinEndAnim))
				{
					m_aiAnimator.PlayUntilFinished(spinEndAnim, true);
				}
				else
				{
					m_aiAnimator.EndAnimationIf(spinAnim);
				}
				if (!string.IsNullOrEmpty(spinVfx))
				{
					m_aiAnimator.StopVfx(spinVfx);
				}
			}
		}
		else if (m_state == SpinState.End && !m_aiAnimator.IsPlaying(spinEndAnim))
		{
			m_state = SpinState.None;
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_state = SpinState.None;
		if ((bool)m_bulletSource && !m_bulletSource.IsEnded)
		{
			m_bulletSource.ForceStop();
		}
		m_aiAnimator.EndAnimationIf(spinBeginAnim);
		m_aiAnimator.EndAnimationIf(spinAnim);
		m_aiAnimator.EndAnimationIf(spinEndAnim);
		m_aiAnimator.StopVfx(spinVfx);
		if (PreventConcurrentAttacks)
		{
			ConcurrentAttackHappening = false;
		}
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override void OnActorPreDeath()
	{
		base.OnActorPreDeath();
		if (PreventConcurrentAttacks)
		{
			ConcurrentAttackHappening = false;
		}
	}

	public override bool IsReady()
	{
		if (PreventConcurrentAttacks && ConcurrentAttackHappening)
		{
			return false;
		}
		return base.IsReady();
	}

	private void ShootBulletScript()
	{
		if (!m_bulletSource)
		{
			m_bulletSource = ShootPoint.GetOrAddComponent<BulletScriptSource>();
		}
		m_bulletSource.BulletManager = m_aiActor.bulletBank;
		m_bulletSource.BulletScript = BulletScript;
		m_bulletSource.Initialize();
	}
}
