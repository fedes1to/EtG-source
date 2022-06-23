using System.Collections.Generic;
using FullInspector;
using UnityEngine;

[InspectorDropdownName(".Groups/SequentialAttackBehaviorGroup")]
public class SequentialAttackBehaviorGroup : AttackBehaviorBase
{
	private enum State
	{
		Idle,
		Update,
		ContinuousUpdate,
		Cooldown
	}

	public bool RunInClass;

	[InspectorCollectionRotorzFlags(HideRemoveButtons = true)]
	public List<AttackBehaviorBase> AttackBehaviors;

	public List<float> OverrideCooldowns;

	private int m_currentIndex = -1;

	private float m_overrideCooldownTimer;

	private State m_state;

	private AttackBehaviorBase currentBehavior
	{
		get
		{
			return AttackBehaviors[m_currentIndex];
		}
	}

	public int Count
	{
		get
		{
			return AttackBehaviors.Count;
		}
	}

	public override void Start()
	{
		base.Start();
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			AttackBehaviors[i].Start();
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			AttackBehaviors[i].Upkeep();
		}
	}

	public override bool OverrideOtherBehaviors()
	{
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			if (!AttackBehaviors[i].OverrideOtherBehaviors())
			{
				return false;
			}
		}
		return true;
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
		m_currentIndex = 0;
		m_state = State.Update;
		if (StepBehaviors())
		{
			return (!RunInClass) ? BehaviorResult.RunContinuous : BehaviorResult.RunContinuousInClass;
		}
		return BehaviorResult.SkipAllRemainingBehaviors;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		return (!StepBehaviors()) ? ContinuousBehaviorResult.Finished : ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (m_currentIndex < AttackBehaviors.Count)
		{
			AttackBehaviors[m_currentIndex].EndContinuousUpdate();
		}
		m_currentIndex = -1;
	}

	public override void Destroy()
	{
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			AttackBehaviors[i].Destroy();
		}
		base.Destroy();
	}

	private bool StepBehaviors()
	{
		if (m_state == State.Cooldown)
		{
			m_overrideCooldownTimer += m_deltaTime;
			if (m_currentIndex == AttackBehaviors.Count - 1)
			{
				return false;
			}
			bool flag = false;
			if (OverrideCooldowns != null && OverrideCooldowns.Count > 0)
			{
				flag = m_overrideCooldownTimer >= OverrideCooldowns[m_currentIndex];
			}
			else if (currentBehavior.IsReady())
			{
				flag = true;
			}
			if (flag)
			{
				m_currentIndex++;
				m_state = State.Update;
				return StepBehaviors();
			}
			return true;
		}
		if (m_state == State.Update)
		{
			BehaviorResult behaviorResult = currentBehavior.Update();
			switch (behaviorResult)
			{
			case BehaviorResult.Continue:
			case BehaviorResult.SkipRemainingClassBehaviors:
			case BehaviorResult.SkipAllRemainingBehaviors:
				m_state = State.Cooldown;
				m_overrideCooldownTimer = 0f;
				return StepBehaviors();
			case BehaviorResult.RunContinuousInClass:
			case BehaviorResult.RunContinuous:
				m_state = State.ContinuousUpdate;
				return true;
			default:
				Debug.LogError("Unrecognized BehaviorResult " + behaviorResult);
				return false;
			}
		}
		if (m_state == State.ContinuousUpdate)
		{
			ContinuousBehaviorResult continuousBehaviorResult = currentBehavior.ContinuousUpdate();
			switch (continuousBehaviorResult)
			{
			case ContinuousBehaviorResult.Finished:
				currentBehavior.EndContinuousUpdate();
				m_state = State.Cooldown;
				m_overrideCooldownTimer = 0f;
				return StepBehaviors();
			case ContinuousBehaviorResult.Continue:
				return true;
			default:
				Debug.LogError("Unrecognized BehaviorResult " + continuousBehaviorResult);
				return false;
			}
		}
		Debug.LogError("Unrecognized State " + m_state);
		return false;
	}

	public override void Init(GameObject gameObject, AIActor aiActor, AIShooter aiShooter)
	{
		base.Init(gameObject, aiActor, aiShooter);
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			AttackBehaviors[i].Init(gameObject, aiActor, aiShooter);
		}
	}

	public override void SetDeltaTime(float deltaTime)
	{
		base.SetDeltaTime(deltaTime);
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			AttackBehaviors[i].SetDeltaTime(deltaTime);
		}
	}

	public override bool IsReady()
	{
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			if (!AttackBehaviors[i].IsReady())
			{
				return false;
			}
		}
		return true;
	}

	public override float GetMinReadyRange()
	{
		if (!IsReady())
		{
			return -1f;
		}
		float num = float.MaxValue;
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			num = Mathf.Min(num, AttackBehaviors[i].GetMinReadyRange());
		}
		return num;
	}

	public override float GetMaxRange()
	{
		float num = float.MinValue;
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			num = Mathf.Max(num, AttackBehaviors[i].GetMaxRange());
		}
		return num;
	}

	public override bool UpdateEveryFrame()
	{
		if (m_currentIndex < 0)
		{
			return false;
		}
		return currentBehavior.UpdateEveryFrame();
	}

	public override bool IsOverridable()
	{
		if (m_currentIndex < 0)
		{
			return true;
		}
		return currentBehavior.IsOverridable();
	}

	public override void OnActorPreDeath()
	{
		base.OnActorPreDeath();
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			AttackBehaviors[i].OnActorPreDeath();
		}
	}

	public AttackBehaviorBase GetAttackBehavior(int index)
	{
		return AttackBehaviors[index];
	}
}
