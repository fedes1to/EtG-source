using System;
using FullInspector;

[InspectorDropdownName("Bosses/BossFinalGuide/BuffBehavior")]
public class BossFinalGuideBuffBehavior : BuffEnemiesBehavior
{
	public string behaviorName;

	public float behaviorProb;

	public float behaviorCooldown;

	private AttackBehaviorGroup.AttackGroupItem m_behavior;

	private float m_cachedProb;

	private float m_cachedCooldown;

	protected override void BuffEnemy(AIActor enemy)
	{
		if (m_behavior == null)
		{
			m_behavior = FindBehavior(enemy);
		}
		if (m_behavior != null)
		{
			m_cachedProb = m_behavior.Probability;
			m_cachedCooldown = (m_behavior.Behavior as BasicAttackBehavior).Cooldown;
			m_behavior.Probability = behaviorProb;
			(m_behavior.Behavior as BasicAttackBehavior).Cooldown = behaviorCooldown;
		}
		base.BuffEnemy(enemy);
	}

	protected override void UnbuffEnemy(AIActor enemy)
	{
		if (m_behavior != null)
		{
			m_behavior.Probability = m_cachedProb;
			(m_behavior.Behavior as BasicAttackBehavior).Cooldown = m_cachedCooldown;
		}
		base.UnbuffEnemy(enemy);
	}

	private AttackBehaviorGroup.AttackGroupItem FindBehavior(AIActor enemy)
	{
		AttackBehaviorGroup attackBehaviorGroup = enemy.behaviorSpeculator.AttackBehaviors.Find((AttackBehaviorBase b) => b is AttackBehaviorGroup) as AttackBehaviorGroup;
		if (attackBehaviorGroup == null)
		{
			return null;
		}
		return attackBehaviorGroup.AttackBehaviors.Find((AttackBehaviorGroup.AttackGroupItem i) => i.NickName.Equals(behaviorName, StringComparison.OrdinalIgnoreCase));
	}
}
