using System;
using UnityEngine;

[Serializable]
public class GameActorHealthEffect : GameActorEffect
{
	public float DamagePerSecondToEnemies = 10f;

	public bool ignitesGoops;

	public override void EffectTick(GameActor actor, RuntimeGameActorEffectData effectData)
	{
		if (AffectsEnemies && actor is AIActor)
		{
			actor.healthHaver.ApplyDamage(DamagePerSecondToEnemies * BraveTime.DeltaTime, Vector2.zero, effectIdentifier, CoreDamageTypes.None, DamageCategory.DamageOverTime);
		}
		if (ignitesGoops)
		{
			DeadlyDeadlyGoopManager.IgniteGoopsCircle(actor.CenterPosition, 0.5f);
		}
	}
}
