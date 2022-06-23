using System.Collections.Generic;
using FullInspector;

[InspectorDropdownName("Bosses/BossFinalRogue/ShootBehavior")]
public class BossFinalRogueShootBehavior : BasicAttackBehavior
{
	public bool SuppressBaseGuns;

	public List<BossFinalRogueGunController> Guns;

	public bool CheckPlayerInArea;

	[InspectorShowIf("CheckPlayerInArea")]
	public ShootBehavior.FiringAreaStyle playerArea;

	[InspectorShowIf("CheckPlayerInArea")]
	public float playerAreaSetupTime;

	public bool EndIfAnyGunsFinish;

	private BossFinalRogueController m_ship;

	private float m_checkPlayerInAreaTimer;

	public override void Start()
	{
		base.Start();
		m_ship = m_aiActor.GetComponent<BossFinalRogueController>();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		if (CheckPlayerInArea && BasicAttackBehavior.DrawDebugFiringArea && (bool)m_aiActor.TargetRigidbody)
		{
			playerArea.DrawDebugLines(GetOrigin(playerArea.targetAreaOrigin), m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox), m_aiActor);
		}
		if (CheckPlayerInArea)
		{
			DecrementTimer(ref m_checkPlayerInAreaTimer);
		}
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
		for (int i = 0; i < Guns.Count; i++)
		{
			Guns[i].Fire();
		}
		for (int j = 0; j < Guns.Count; j++)
		{
			if (!Guns[j].IsFinished)
			{
				if (SuppressBaseGuns)
				{
					m_ship.SuppressBaseGuns = true;
				}
				if (CheckPlayerInArea)
				{
					m_checkPlayerInAreaTimer = playerAreaSetupTime;
				}
				m_updateEveryFrame = true;
				return BehaviorResult.RunContinuousInClass;
			}
		}
		UpdateCooldowns();
		return BehaviorResult.SkipRemainingClassBehaviors;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (CheckPlayerInArea && m_checkPlayerInAreaTimer <= 0f && !TargetStillInFiringArea())
		{
			return ContinuousBehaviorResult.Finished;
		}
		if (EndIfAnyGunsFinish)
		{
			for (int i = 0; i < Guns.Count; i++)
			{
				if (Guns[i].IsFinished)
				{
					return ContinuousBehaviorResult.Finished;
				}
			}
			return ContinuousBehaviorResult.Continue;
		}
		for (int j = 0; j < Guns.Count; j++)
		{
			if (!Guns[j].IsFinished)
			{
				return ContinuousBehaviorResult.Continue;
			}
		}
		return ContinuousBehaviorResult.Finished;
	}

	public override void EndContinuousUpdate()
	{
		if (SuppressBaseGuns)
		{
			m_ship.SuppressBaseGuns = false;
		}
		for (int i = 0; i < Guns.Count; i++)
		{
			Guns[i].CeaseFire();
		}
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	protected bool TargetStillInFiringArea()
	{
		if (playerArea == null)
		{
			return true;
		}
		if (!m_aiActor.TargetRigidbody)
		{
			return false;
		}
		return playerArea.TargetInFiringArea(GetOrigin(playerArea.targetAreaOrigin), m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox));
	}
}
