using System;

[Serializable]
public class AIActorDebuffEffect : GameActorEffect
{
	public float SpeedMultiplier = 1f;

	public float CooldownMultiplier = 1f;

	public float HealthMultiplier = 1f;

	public bool KeepHealthPercentage = true;

	public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1f)
	{
		HealthHaver healthHaver = actor.healthHaver;
		float targetValue = actor.healthHaver.GetMaxHealth() * HealthMultiplier;
		bool keepHealthPercentage = KeepHealthPercentage;
		healthHaver.SetHealthMaximum(targetValue, null, keepHealthPercentage);
		if (SpeedMultiplier != 1f)
		{
			SpeculativeRigidbody specRigidbody = actor.specRigidbody;
			specRigidbody.OnPreMovement = (Action<SpeculativeRigidbody>)Delegate.Combine(specRigidbody.OnPreMovement, new Action<SpeculativeRigidbody>(ModifyVelocity));
		}
		if (CooldownMultiplier != 1f && (bool)actor.behaviorSpeculator)
		{
			actor.behaviorSpeculator.CooldownScale /= CooldownMultiplier;
		}
	}

	public override void OnEffectRemoved(GameActor actor, RuntimeGameActorEffectData effectData)
	{
		HealthHaver healthHaver = actor.healthHaver;
		float targetValue = actor.healthHaver.GetMaxHealth() / HealthMultiplier;
		bool keepHealthPercentage = KeepHealthPercentage;
		healthHaver.SetHealthMaximum(targetValue, null, keepHealthPercentage);
		if (SpeedMultiplier != 1f)
		{
			SpeculativeRigidbody specRigidbody = actor.specRigidbody;
			specRigidbody.OnPreMovement = (Action<SpeculativeRigidbody>)Delegate.Remove(specRigidbody.OnPreMovement, new Action<SpeculativeRigidbody>(ModifyVelocity));
		}
		if (CooldownMultiplier != 1f && (bool)actor.behaviorSpeculator)
		{
			actor.behaviorSpeculator.CooldownScale *= CooldownMultiplier;
		}
	}

	public void ModifyVelocity(SpeculativeRigidbody myRigidbody)
	{
		myRigidbody.Velocity *= SpeedMultiplier;
	}
}
