using System;
using System.Collections.Generic;
using UnityEngine;

public class AppliesGoopDoerModifier : MonoBehaviour
{
	public GoopDefinition goopDefinitionToUse;

	public float goopRadius = 3f;

	public bool IsSynergyContingent;

	public CustomSynergyType SynergyToCheck;

	protected Projectile m_projectile;

	protected HashSet<AIActor> m_processedActors = new HashSet<AIActor>();

	private void Start()
	{
		m_projectile = GetComponent<Projectile>();
		Projectile projectile = m_projectile;
		projectile.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(projectile.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleHitEnemy));
	}

	private void HandleHitEnemy(Projectile p1, SpeculativeRigidbody srb1, bool killedEnemy)
	{
		if ((IsSynergyContingent && (!p1 || !p1.PossibleSourceGun || !(p1.PossibleSourceGun.CurrentOwner is PlayerController) || !(p1.PossibleSourceGun.CurrentOwner as PlayerController).HasActiveBonusSynergy(SynergyToCheck))) || !this || !srb1)
		{
			return;
		}
		AIActor component = srb1.GetComponent<AIActor>();
		if ((bool)component && !m_processedActors.Contains(component))
		{
			m_processedActors.Add(component);
			if (killedEnemy)
			{
				Vector2 unitCenter = srb1.UnitCenter;
				DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopDefinitionToUse).TimedAddGoopCircle(unitCenter, goopRadius, 1f);
				return;
			}
			GoopDoer goopDoer = srb1.gameObject.AddComponent<GoopDoer>();
			goopDoer.updateTiming = GoopDoer.UpdateTiming.TriggerOnly;
			goopDoer.updateOnPreDeath = true;
			goopDoer.goopDefinition = goopDefinitionToUse;
			goopDoer.defaultGoopRadius = goopRadius;
			goopDoer.isTimed = true;
		}
	}
}
