using System.Collections.Generic;
using FullInspector;
using UnityEngine;

[InspectorDropdownName(".Groups/AttackBehaviorGroup")]
public class AttackBehaviorGroup : AttackBehaviorBase, IAttackBehaviorGroup
{
	public class AttackGroupItem
	{
		[InspectorName("Nickname")]
		public string NickName;

		public float Probability = 1f;

		public AttackBehaviorBase Behavior;
	}

	public bool ShareCooldowns;

	[InspectorCollectionRotorzFlags(HideRemoveButtons = true)]
	public List<AttackGroupItem> AttackBehaviors;

	private AttackBehaviorBase m_currentBehavior;

	public AttackBehaviorBase CurrentBehavior
	{
		get
		{
			return m_currentBehavior;
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
			if (AttackBehaviors[i].Behavior != null)
			{
				AttackBehaviors[i].Behavior.Start();
			}
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			if (AttackBehaviors[i].Behavior != null)
			{
				AttackBehaviors[i].Behavior.Upkeep();
			}
		}
	}

	public override bool OverrideOtherBehaviors()
	{
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			if (AttackBehaviors[i].Behavior != null && AttackBehaviors[i].Behavior.OverrideOtherBehaviors())
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
		float num = 0f;
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			if (AttackBehaviors[i].Probability > 0f && AttackBehaviors[i].Behavior.IsReady())
			{
				num += AttackBehaviors[i].Probability;
			}
		}
		if (num == 0f)
		{
			return BehaviorResult.Continue;
		}
		float num2 = Random.Range(0f, num);
		for (int j = 0; j < AttackBehaviors.Count; j++)
		{
			if (AttackBehaviors[j].Probability > 0f && AttackBehaviors[j].Behavior.IsReady())
			{
				m_currentBehavior = AttackBehaviors[j].Behavior;
				if (num2 < AttackBehaviors[j].Probability)
				{
					break;
				}
				num2 -= AttackBehaviors[j].Probability;
			}
		}
		return m_currentBehavior.Update();
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		return m_currentBehavior.ContinuousUpdate();
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (m_currentBehavior != null)
		{
			m_currentBehavior.EndContinuousUpdate();
			m_currentBehavior = null;
		}
	}

	public override void Destroy()
	{
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			if (AttackBehaviors[i].Behavior != null)
			{
				AttackBehaviors[i].Behavior.Destroy();
			}
		}
		base.Destroy();
	}

	public override void Init(GameObject gameObject, AIActor aiActor, AIShooter aiShooter)
	{
		base.Init(gameObject, aiActor, aiShooter);
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			if (AttackBehaviors[i].Behavior != null)
			{
				AttackBehaviors[i].Behavior.Init(gameObject, aiActor, aiShooter);
			}
		}
	}

	public override void SetDeltaTime(float deltaTime)
	{
		base.SetDeltaTime(deltaTime);
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			if (AttackBehaviors[i].Behavior != null)
			{
				AttackBehaviors[i].Behavior.SetDeltaTime(deltaTime);
			}
		}
	}

	public override bool IsReady()
	{
		if (ShareCooldowns)
		{
			for (int i = 0; i < AttackBehaviors.Count; i++)
			{
				if (AttackBehaviors[i].Behavior != null && !AttackBehaviors[i].Behavior.IsReady())
				{
					return false;
				}
			}
			return true;
		}
		for (int j = 0; j < AttackBehaviors.Count; j++)
		{
			if (AttackBehaviors[j].Behavior != null && AttackBehaviors[j].Behavior.IsReady())
			{
				return true;
			}
		}
		return false;
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
			if (AttackBehaviors[i].Behavior != null)
			{
				num = Mathf.Min(num, AttackBehaviors[i].Behavior.GetMinReadyRange());
			}
		}
		return num;
	}

	public override float GetMaxRange()
	{
		float num = float.MinValue;
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			if (AttackBehaviors[i].Behavior != null)
			{
				num = Mathf.Max(num, AttackBehaviors[i].Behavior.GetMaxRange());
			}
		}
		return num;
	}

	public override bool UpdateEveryFrame()
	{
		if (m_currentBehavior == null)
		{
			return false;
		}
		return m_currentBehavior.UpdateEveryFrame();
	}

	public override bool IsOverridable()
	{
		return (m_currentBehavior == null) ? base.IsOverridable() : m_currentBehavior.IsOverridable();
	}

	public override void OnActorPreDeath()
	{
		base.OnActorPreDeath();
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			if (AttackBehaviors[i].Behavior != null)
			{
				AttackBehaviors[i].Behavior.OnActorPreDeath();
			}
		}
	}

	public AttackBehaviorBase GetAttackBehavior(int index)
	{
		return AttackBehaviors[index].Behavior;
	}
}
