public class BossFinalRogueBulletScriptGun : BossFinalRogueGunController
{
	public ShootBehavior ShootBehavior;

	private bool m_isRunning;

	public override bool IsFinished
	{
		get
		{
			return !m_isRunning;
		}
	}

	public override void Start()
	{
		base.Start();
		ShootBehavior.Init(ship.gameObject, ship.aiActor, ship.aiShooter);
		ShootBehavior.Start();
	}

	public override void Update()
	{
		base.Update();
		ShootBehavior.SetDeltaTime(BraveTime.DeltaTime);
		ShootBehavior.Upkeep();
		if (m_isRunning)
		{
			ContinuousBehaviorResult continuousBehaviorResult = ShootBehavior.ContinuousUpdate();
			if (continuousBehaviorResult == ContinuousBehaviorResult.Finished)
			{
				m_isRunning = false;
				ShootBehavior.EndContinuousUpdate();
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void Fire()
	{
		BehaviorResult behaviorResult = ShootBehavior.Update();
		if (behaviorResult == BehaviorResult.RunContinuous || behaviorResult == BehaviorResult.RunContinuousInClass)
		{
			m_isRunning = true;
		}
	}

	public override void CeaseFire()
	{
		if (m_isRunning)
		{
			ShootBehavior.EndContinuousUpdate();
		}
	}
}
