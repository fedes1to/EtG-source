using System;

[Serializable]
public class GameActorCharmEffect : GameActorEffect
{
	public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1f)
	{
		if (actor is AIActor)
		{
			AkSoundEngine.PostEvent("Play_OBJ_enemy_charmed_01", GameManager.Instance.gameObject);
			AIActor aIActor = actor as AIActor;
			aIActor.CanTargetEnemies = true;
			aIActor.CanTargetPlayers = false;
		}
	}

	public override void OnEffectRemoved(GameActor actor, RuntimeGameActorEffectData effectData)
	{
		if (actor is AIActor)
		{
			AIActor aIActor = actor as AIActor;
			aIActor.CanTargetEnemies = false;
			aIActor.CanTargetPlayers = true;
		}
	}
}
