using System;
using System.Collections.Generic;
using UnityEngine;

public class GoopDefinition : ScriptableObject
{
	[Serializable]
	public class GoopDamageTypeInteraction
	{
		public enum GoopIgnitionMode
		{
			NONE,
			IGNITE,
			DOUSE
		}

		[EnumFlags]
		public CoreDamageTypes damageType;

		public bool electrifiesGoop;

		public bool freezesGoop;

		public GoopIgnitionMode ignitionMode;
	}

	public Texture2D goopTexture;

	public Texture2D worldTexture;

	public bool usesWorldTextureByDefault;

	public Color32 baseColor32 = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	public bool usesLifespan = true;

	[ShowInInspectorIf("usesLifespan", false)]
	public float lifespan = 10f;

	[ShowInInspectorIf("usesLifespan", false)]
	public float fadePeriod = 2f;

	[ShowInInspectorIf("usesLifespan", false)]
	public Color32 fadeColor32 = new Color32(128, 128, 128, 0);

	[ShowInInspectorIf("usesLifespan", false)]
	public float lifespanRadialReduction = 3f;

	public bool damagesPlayers;

	[ShowInInspectorIf("damagesPlayers", false)]
	public float damageToPlayers = 0.5f;

	[ShowInInspectorIf("damagesPlayers", false)]
	public float delayBeforeDamageToPlayers = 0.5f;

	public CoreDamageTypes damageTypes;

	public bool damagesEnemies;

	[ShowInInspectorIf("damagesEnemies", false)]
	public float damagePerSecondtoEnemies = 10f;

	public List<GoopDamageTypeInteraction> goopDamageTypeInteractions;

	public bool usesAmbientGoopFX;

	[ShowInInspectorIf("usesAmbientGoopFX", false)]
	public float ambientGoopFXChance = 0.01f;

	[ShowInInspectorIf("usesAmbientGoopFX", false)]
	public VFXPool ambientGoopFX;

	public bool usesAcidAudio;

	public bool isOily;

	public bool usesWaterVfx = true;

	public bool eternal;

	public bool usesOverrideOpaqueness;

	public float overrideOpaqueness = 0.5f;

	[Header("On Fire Settings")]
	public bool CanBeIgnited;

	public float igniteSpreadTime = 0.1f;

	public bool SelfIgnites;

	public float selfIgniteDelay = 0.5f;

	public bool ignitionChangesLifetime;

	[ShowInInspectorIf("ignitionChangesLifetime", false)]
	public float ignitedLifetime = 5f;

	public bool playerStepsChangeLifetime;

	[ShowInInspectorIf("playerStepsChangeLifetime", false)]
	public float playerStepsLifetime = 2f;

	public float fireDamageToPlayer = 0.5f;

	public float fireDamagePerSecondToEnemies = 10f;

	[ShowInInspectorIf("CanBeIgnited", false)]
	public bool fireBurnsEnemies = true;

	[ShowInInspectorIf("CanBeIgnited", false)]
	public GameActorFireEffect fireEffect;

	public Color32 igniteColor32 = new Color32(byte.MaxValue, 128, 128, byte.MaxValue);

	public Color32 fireColor32 = new Color32(byte.MaxValue, 0, 0, byte.MaxValue);

	[ShowInInspectorIf("CanBeIgnited", false)]
	public bool UsesGreenFire;

	[Header("On Electrified Settings")]
	public bool CanBeElectrified;

	public float electrifiedDamageToPlayer = 0.5f;

	public float electrifiedDamagePerSecondToEnemies = 10f;

	public float electrifiedTime = 2f;

	[Header("On Frozen Settings")]
	public bool CanBeFrozen;

	public float freezeLifespan = 10f;

	public float freezeSpreadTime = 0.1f;

	public Color32 prefreezeColor32 = new Color32(238, 240, byte.MaxValue, byte.MaxValue);

	public Color32 frozenColor32 = new Color32(238, 240, byte.MaxValue, byte.MaxValue);

	[Header("Status Effects")]
	public bool AppliesSpeedModifier;

	public bool AppliesSpeedModifierContinuously;

	public GameActorSpeedEffect SpeedModifierEffect;

	public bool AppliesDamageOverTime;

	public GameActorHealthEffect HealthModifierEffect;

	public bool DrainsAmmo;

	public float PercentAmmoDrainPerSecond = 0.1f;

	public bool AppliesCharm;

	public GameActorCharmEffect CharmModifierEffect;

	public bool AppliesCheese;

	public GameActorCheeseEffect CheeseModifierEffect;

	public float GetLifespan(float radialFraction)
	{
		float result = float.MaxValue;
		if (usesLifespan)
		{
			result = Mathf.Lerp(lifespan, lifespan - lifespanRadialReduction, radialFraction);
		}
		return result;
	}
}
