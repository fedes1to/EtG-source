using System;

[Serializable]
public class GameActorMaxHealthEffect : GameActorEffect
{
	public float HealthMultiplier = 1f;

	public bool KeepHealthPercentage = true;

	public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1f)
	{
		float currentHealthPercentage = actor.healthHaver.GetCurrentHealthPercentage();
		float num = actor.healthHaver.GetMaxHealth() * HealthMultiplier;
		actor.healthHaver.SetHealthMaximum(num);
		if (KeepHealthPercentage)
		{
			actor.healthHaver.ForceSetCurrentHealth(currentHealthPercentage * num);
		}
	}

	public override void OnEffectRemoved(GameActor actor, RuntimeGameActorEffectData effectData)
	{
		float currentHealthPercentage = actor.healthHaver.GetCurrentHealthPercentage();
		float num = actor.healthHaver.GetMaxHealth() / HealthMultiplier;
		actor.healthHaver.SetHealthMaximum(num);
		if (KeepHealthPercentage)
		{
			actor.healthHaver.ForceSetCurrentHealth(currentHealthPercentage * num);
		}
	}
}
