using System;

[Serializable]
public class GameActorSpeedEffect : GameActorEffect
{
	public float SpeedMultiplier = 1f;

	public float CooldownMultiplier = 1f;

	public bool OnlyAffectPlayerWhenGrounded;

	public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1f)
	{
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
		if (OnlyAffectPlayerWhenGrounded)
		{
			PlayerController playerController = myRigidbody.gameActor as PlayerController;
			if ((bool)playerController && (!playerController.IsGrounded || playerController.IsSlidingOverSurface))
			{
				return;
			}
		}
		myRigidbody.Velocity *= SpeedMultiplier;
	}
}
