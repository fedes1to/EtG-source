using System;
using UnityEngine;

[Serializable]
public abstract class GameActorEffect
{
	public enum EffectStackingMode
	{
		Refresh,
		Stack,
		Ignore,
		DarkSoulsAccumulate
	}

	public bool AffectsPlayers = true;

	public bool AffectsEnemies = true;

	public string effectIdentifier = "effect";

	public EffectResistanceType resistanceType;

	public EffectStackingMode stackMode;

	public float duration = 10f;

	[ShowInInspectorIf("stackMode", 1, false)]
	public float maxStackedDuration = -1f;

	public bool AppliesTint;

	[ShowInInspectorIf("AppliesTint", false)]
	public Color TintColor = new Color(1f, 1f, 1f, 0.5f);

	public bool AppliesDeathTint;

	[ShowInInspectorIf("AppliesDeathTint", false)]
	public Color DeathTintColor = new Color(0.388f, 0.388f, 0.388f, 1f);

	public bool AppliesOutlineTint;

	[ColorUsage(true, true, 0f, 1000f, 0.125f, 3f)]
	public Color OutlineTintColor = new Color(0f, 0f, 0f, 1f);

	public GameObject OverheadVFX;

	public bool PlaysVFXOnActor;

	public virtual bool ResistanceAffectsDuration
	{
		get
		{
			return false;
		}
	}

	public virtual void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1f)
	{
	}

	public virtual void OnDarkSoulsAccumulate(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1f, Projectile sourceProjectile = null)
	{
	}

	public virtual void EffectTick(GameActor actor, RuntimeGameActorEffectData effectData)
	{
	}

	public virtual void OnEffectRemoved(GameActor actor, RuntimeGameActorEffectData effectData)
	{
	}

	public virtual void ApplyTint(GameActor actor)
	{
		if (AppliesTint)
		{
			actor.RegisterOverrideColor(TintColor, effectIdentifier);
		}
		if (AppliesOutlineTint && actor is AIActor)
		{
			AIActor aIActor = actor as AIActor;
			aIActor.SetOverrideOutlineColor(OutlineTintColor);
		}
	}

	public virtual bool IsFinished(GameActor actor, RuntimeGameActorEffectData effectData, float elapsedTime)
	{
		float num = duration;
		if (this is GameActorFireEffect && (bool)actor.healthHaver && actor.healthHaver.IsBoss)
		{
			num = Mathf.Min(num, 8f);
		}
		if (ResistanceAffectsDuration)
		{
			float resistanceForEffectType = actor.GetResistanceForEffectType(resistanceType);
			num *= Mathf.Clamp01(1f - resistanceForEffectType);
		}
		return elapsedTime >= num;
	}
}
