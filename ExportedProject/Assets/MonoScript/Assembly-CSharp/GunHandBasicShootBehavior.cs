using System.Collections.Generic;

public class GunHandBasicShootBehavior : BasicAttackBehavior
{
	public bool LineOfSight = true;

	public bool FireAllGuns;

	public List<GunHandController> GunHands;

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
			for (int i = 0; i < GunHands.Count; i++)
			{
				if ((bool)GunHands[i])
				{
					GunHands[i].CeaseAttack();
				}
			}
			return BehaviorResult.Continue;
		}
		if (FireAllGuns)
		{
			for (int j = 0; j < GunHands.Count; j++)
			{
				if ((bool)GunHands[j])
				{
					GunHands[j].StartFiring();
				}
			}
			UpdateCooldowns();
			return BehaviorResult.SkipRemainingClassBehaviors;
		}
		GunHandController gunHandController = BraveUtility.RandomElement(GunHands);
		if ((bool)gunHandController)
		{
			gunHandController.StartFiring();
		}
		UpdateCooldowns();
		return BehaviorResult.SkipRemainingClassBehaviors;
	}

	public override bool IsReady()
	{
		if (!base.IsReady())
		{
			return false;
		}
		if (FireAllGuns)
		{
			for (int i = 0; i < GunHands.Count; i++)
			{
				if (!GunHands[i].IsReady)
				{
					return false;
				}
			}
			return true;
		}
		for (int j = 0; j < GunHands.Count; j++)
		{
			if (GunHands[j].IsReady)
			{
				return true;
			}
		}
		return false;
	}
}
