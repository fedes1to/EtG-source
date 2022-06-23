using FullInspector;

[InspectorDropdownName("Bosses/Beholster/ShootGunBehavior")]
public class BeholsterShootGunBehavior : BasicAttackBehavior
{
	public bool LineOfSight = true;

	public BeholsterTentacleController[] Tentacles;

	private BeholsterController m_beholster;

	public override void Start()
	{
		base.Start();
		m_beholster = m_aiActor.GetComponent<BeholsterController>();
	}

	public override void Upkeep()
	{
		base.Upkeep();
	}

	public override BehaviorResult Update()
	{
		base.Update();
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		bool flag = LineOfSight && !m_aiActor.HasLineOfSightToTarget;
		if (m_aiActor.TargetRigidbody == null || flag)
		{
			m_beholster.StopFiringTentacles(Tentacles);
			return BehaviorResult.Continue;
		}
		m_beholster.StartFiringTentacles(Tentacles);
		UpdateCooldowns();
		return BehaviorResult.SkipRemainingClassBehaviors;
	}

	public override bool IsReady()
	{
		if (!base.IsReady())
		{
			return false;
		}
		for (int i = 0; i < Tentacles.Length; i++)
		{
			if (Tentacles[i].IsReady)
			{
				return true;
			}
		}
		return false;
	}
}
