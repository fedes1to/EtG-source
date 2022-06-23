using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossFinalConvict/ShootBehavior")]
public class BossFinalConvictShootBehavior : BasicAttackBehavior
{
	public GameObject shootPoint;

	public BulletScriptSelector bulletScript;

	public float maxMoveSpeed = 5f;

	public float moveAcceleration = 10f;

	[InspectorCategory("Visuals")]
	public string anim;

	private BulletScriptSource m_bulletSource;

	private float m_verticalVelocity;

	public override void Start()
	{
		base.Start();
	}

	public override void Upkeep()
	{
		base.Upkeep();
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
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		m_aiActor.ClearPath();
		m_aiActor.BehaviorOverridesVelocity = true;
		m_aiActor.BehaviorVelocity = Vector2.zero;
		m_verticalVelocity = 0f;
		if (!string.IsNullOrEmpty(anim))
		{
			m_aiAnimator.PlayUntilCancelled(anim);
		}
		Fire();
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (!m_aiActor.TargetRigidbody || m_bulletSource.IsEnded)
		{
			return ContinuousBehaviorResult.Finished;
		}
		if (IsTargetUnreachable())
		{
			return ContinuousBehaviorResult.Finished;
		}
		Vector2 vector = m_aiActor.TargetRigidbody.specRigidbody.GetUnitCenter(ColliderType.HitBox) - m_aiActor.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		float num = ((!(Mathf.Abs(vector.y) > 5f)) ? maxMoveSpeed : (1.5f * maxMoveSpeed));
		m_verticalVelocity = Mathf.Clamp(m_verticalVelocity + Mathf.Sign(vector.y) * moveAcceleration * m_deltaTime, 0f - num, num);
		m_aiActor.BehaviorVelocity = new Vector2(0f, m_verticalVelocity);
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_aiActor.BehaviorOverridesVelocity = false;
		if (!string.IsNullOrEmpty(anim))
		{
			m_aiAnimator.EndAnimationIf(anim);
		}
		if ((bool)m_bulletSource)
		{
			m_bulletSource.ForceStop();
		}
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsOverridable()
	{
		return false;
	}

	public override bool IsReady()
	{
		if (!base.IsReady())
		{
			return false;
		}
		if (IsTargetUnreachable())
		{
			return false;
		}
		return true;
	}

	private void Fire()
	{
		if (!m_bulletSource)
		{
			m_bulletSource = shootPoint.GetOrAddComponent<BulletScriptSource>();
		}
		m_bulletSource.BulletManager = m_aiActor.bulletBank;
		m_bulletSource.BulletScript = bulletScript;
		m_bulletSource.Initialize();
	}

	private bool IsTargetUnreachable(float maxDist = float.MaxValue)
	{
		if (!m_aiActor.TargetRigidbody)
		{
			return true;
		}
		Vector2 vector = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - m_aiActor.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		int value = CollisionMask.LayerToMask(CollisionLayer.LowObstacle, CollisionLayer.HighObstacle);
		float y = Mathf.Min(vector.y, maxDist);
		CollisionData result;
		bool result2 = PhysicsEngine.Instance.RigidbodyCastWithIgnores(m_aiActor.specRigidbody, PhysicsEngine.UnitToPixel(new Vector2(0f, y)), out result, true, true, value, false, m_aiActor.specRigidbody);
		CollisionData.Pool.Free(ref result);
		return result2;
	}
}
