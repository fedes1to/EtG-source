using System;
using UnityEngine;

public class BossFinalMarineLavaController : BraveBehaviour
{
	public GoopDefinition goopDefinition;

	private DimensionFogController m_dimensionFog;

	public void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnTriggerCollision = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnTriggerCollision, new SpeculativeRigidbody.OnTriggerDelegate(OnTriggerCollision));
		m_dimensionFog = UnityEngine.Object.FindObjectOfType<DimensionFogController>();
	}

	private void OnTriggerCollision(SpeculativeRigidbody speculativeRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		PlayerController component = speculativeRigidbody.GetComponent<PlayerController>();
		Vector2 unitCenter = component.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		if (component.spriteAnimator.QueryGroundedFrame() && Vector2.Distance(unitCenter, m_dimensionFog.transform.position) < m_dimensionFog.ApparentRadius)
		{
			component.IncreasePoison(BraveTime.DeltaTime * 1.5f);
			if (component.CurrentPoisonMeterValue >= 1f)
			{
				component.healthHaver.ApplyDamage(0.5f, Vector2.zero, StringTableManager.GetEnemiesString("#GOOP"), CoreDamageTypes.Poison, DamageCategory.Environment, true);
				component.CurrentPoisonMeterValue = 0f;
			}
		}
	}
}
