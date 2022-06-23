using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExplosionData
{
	public bool useDefaultExplosion;

	public bool doDamage = true;

	public bool forceUseThisRadius;

	[ShowInInspectorIf("doDamage", true)]
	public float damageRadius = 4.5f;

	[ShowInInspectorIf("doDamage", true)]
	public float damageToPlayer = 0.5f;

	[ShowInInspectorIf("doDamage", true)]
	public float damage = 25f;

	public bool breakSecretWalls;

	[ShowInInspectorIf("breakSecretWalls", true)]
	public float secretWallsRadius = 4.5f;

	public bool forcePreventSecretWallDamage;

	public bool doDestroyProjectiles = true;

	public bool doForce = true;

	[ShowInInspectorIf("doForce", true)]
	public float pushRadius = 6f;

	[ShowInInspectorIf("doForce", true)]
	public float force = 100f;

	[ShowInInspectorIf("doForce", true)]
	public float debrisForce = 50f;

	[ShowInInspectorIf("doForce", true)]
	public bool preventPlayerForce;

	public float explosionDelay = 0.1f;

	public bool usesComprehensiveDelay;

	[ShowInInspectorIf("usesComprehensiveDelay", false)]
	public float comprehensiveDelay;

	public GameObject effect;

	public bool doScreenShake = true;

	[ShowInInspectorIf("doScreenShake", true)]
	public ScreenShakeSettings ss;

	public bool doStickyFriction = true;

	public bool doExplosionRing = true;

	public bool isFreezeExplosion;

	[ShowInInspectorIf("isFreezeExplosion", false)]
	public float freezeRadius = 5f;

	public GameActorFreezeEffect freezeEffect;

	public bool playDefaultSFX = true;

	public bool IsChandelierExplosion;

	public bool rotateEffectToNormal;

	[HideInInspector]
	public List<SpeculativeRigidbody> ignoreList;

	[HideInInspector]
	public GameObject overrideRangeIndicatorEffect;

	public void CopyFrom(ExplosionData source)
	{
		doDamage = source.doDamage;
		forceUseThisRadius = source.forceUseThisRadius;
		damageRadius = source.damageRadius;
		damageToPlayer = source.damageToPlayer;
		damage = source.damage;
		breakSecretWalls = source.breakSecretWalls;
		secretWallsRadius = source.secretWallsRadius;
		doDestroyProjectiles = source.doDestroyProjectiles;
		doForce = source.doForce;
		pushRadius = source.pushRadius;
		force = source.force;
		debrisForce = source.debrisForce;
		explosionDelay = source.explosionDelay;
		effect = source.effect;
		doScreenShake = source.doScreenShake;
		ss = source.ss;
		doStickyFriction = source.doStickyFriction;
		doExplosionRing = source.doExplosionRing;
		isFreezeExplosion = source.isFreezeExplosion;
		freezeRadius = source.freezeRadius;
		freezeEffect = source.freezeEffect;
		playDefaultSFX = source.playDefaultSFX;
		IsChandelierExplosion = source.IsChandelierExplosion;
		ignoreList = new List<SpeculativeRigidbody>();
	}

	public float GetDefinedDamageRadius()
	{
		if (forceUseThisRadius)
		{
			return damageRadius;
		}
		if ((bool)effect)
		{
			ExplosionRadiusDefiner component = effect.GetComponent<ExplosionRadiusDefiner>();
			if ((bool)component)
			{
				return component.Radius;
			}
		}
		return damageRadius;
	}
}
