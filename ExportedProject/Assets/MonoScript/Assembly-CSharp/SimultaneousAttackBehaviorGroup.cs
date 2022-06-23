using System.Collections.Generic;
using FullInspector;
using UnityEngine;

[InspectorDropdownName(".Groups/SimultaneousAttackBehaviorGroup")]
public class SimultaneousAttackBehaviorGroup : AttackBehaviorBase, IAttackBehaviorGroup
{
	[InspectorCollectionRotorzFlags(HideRemoveButtons = true)]
	public List<AttackBehaviorBase> AttackBehaviors;

	private bool[] m_finished;

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
		m_finished = new bool[AttackBehaviors.Count];
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
			if (AttackBehaviors[i].OverrideOtherBehaviors())
			{
				return true;
			}
		}
		return false;
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
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			BehaviorResult behaviorResult2 = AttackBehaviors[i].Update();
			if (i > 0 && behaviorResult2 != behaviorResult)
			{
				Debug.LogError("Mismatching result returned from a SimultaneousAttackBehaviorGroup: this is not supported!");
			}
			behaviorResult = behaviorResult2;
			m_finished[i] = false;
		}
		return behaviorResult;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		bool flag = false;
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			if (!m_finished[i])
			{
				if (AttackBehaviors[i].ContinuousUpdate() == ContinuousBehaviorResult.Continue)
				{
					flag = true;
					continue;
				}
				m_finished[i] = true;
				AttackBehaviors[i].EndContinuousUpdate();
			}
		}
		return (!flag) ? ContinuousBehaviorResult.Finished : ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			if (!m_finished[i])
			{
				AttackBehaviors[i].EndContinuousUpdate();
			}
		}
	}

	public override void Destroy()
	{
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			AttackBehaviors[i].Destroy();
		}
		base.Destroy();
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
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			if (AttackBehaviors[i].UpdateEveryFrame())
			{
				return true;
			}
		}
		return false;
	}

	public override bool IsOverridable()
	{
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			if (!AttackBehaviors[i].IsOverridable())
			{
				return false;
			}
		}
		return true;
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
