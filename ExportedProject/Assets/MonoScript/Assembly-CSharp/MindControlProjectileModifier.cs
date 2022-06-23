using System;
using UnityEngine;

public class MindControlProjectileModifier : MonoBehaviour
{
	private void Start()
	{
		Projectile component = GetComponent<Projectile>();
		if ((bool)component)
		{
			component.OnHitEnemy = (Action<Projectile, SpeculativeRigidbody, bool>)Delegate.Combine(component.OnHitEnemy, new Action<Projectile, SpeculativeRigidbody, bool>(HandleHitEnemy));
		}
	}

	private void HandleHitEnemy(Projectile arg1, SpeculativeRigidbody arg2, bool arg3)
	{
		if ((bool)arg2 && (bool)arg2.aiActor)
		{
			AIActor aiActor = arg2.aiActor;
			if (aiActor.IsNormalEnemy && !aiActor.healthHaver.IsBoss && !aiActor.IsHarmlessEnemy && !aiActor.gameObject.GetComponent<MindControlEffect>())
			{
				MindControlEffect orAddComponent = aiActor.gameObject.GetOrAddComponent<MindControlEffect>();
				orAddComponent.owner = arg1.Owner as PlayerController;
			}
		}
	}
}
