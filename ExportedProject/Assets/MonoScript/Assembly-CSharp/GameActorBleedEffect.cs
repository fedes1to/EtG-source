using System;
using UnityEngine;

[Serializable]
public class GameActorBleedEffect : GameActorEffect
{
	public float ChargeAmount = 10f;

	public float ChargeDispelFactor = 10f;

	public GameObject vfxChargingReticle;

	public GameObject vfxExplosion;

	private GameObject m_extantReticle;

	private bool m_isHammerOfDawn;

	public bool ShouldVanishOnDeath(GameActor actor)
	{
		if ((bool)actor.healthHaver && actor.healthHaver.IsBoss)
		{
			return false;
		}
		if (actor is AIActor && (actor as AIActor).IsSignatureEnemy)
		{
			return false;
		}
		return true;
	}

	public override void ApplyTint(GameActor actor)
	{
	}

	public override void OnEffectApplied(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1f)
	{
		if ((bool)actor && (bool)actor.healthHaver && actor.healthHaver.IsDead)
		{
			effectData.accumulator = 0f;
			return;
		}
		m_isHammerOfDawn = vfxExplosion.GetComponent<HammerOfDawnController>() != null;
		effectData.accumulator += ChargeAmount * partialAmount;
	}

	public override void OnDarkSoulsAccumulate(GameActor actor, RuntimeGameActorEffectData effectData, float partialAmount = 1f, Projectile sourceProjectile = null)
	{
		if ((bool)actor && (bool)actor.healthHaver && actor.healthHaver.IsDead)
		{
			effectData.accumulator = 0f;
			return;
		}
		if (m_isHammerOfDawn && HammerOfDawnController.HasExtantHammer(sourceProjectile))
		{
			effectData.accumulator = 0f;
			return;
		}
		effectData.accumulator += ChargeAmount * partialAmount;
		if ((!m_isHammerOfDawn || !HammerOfDawnController.HasExtantHammer(sourceProjectile)) && !m_extantReticle)
		{
			m_extantReticle = UnityEngine.Object.Instantiate(vfxChargingReticle, actor.specRigidbody.HitboxPixelCollider.UnitBottomCenter, Quaternion.identity);
			m_extantReticle.transform.parent = actor.transform;
			RailgunChargeEffectController component = m_extantReticle.GetComponent<RailgunChargeEffectController>();
			if ((bool)component)
			{
				component.IsManuallyControlled = true;
			}
		}
		if (!(effectData.accumulator > 100f) || !actor.healthHaver.IsAlive)
		{
			return;
		}
		effectData.accumulator = 0f;
		if (!m_isHammerOfDawn || !HammerOfDawnController.HasExtantHammer(sourceProjectile))
		{
			GameObject gameObject = null;
			gameObject = ((!m_isHammerOfDawn) ? actor.PlayEffectOnActor(vfxExplosion, Vector3.zero, false) : UnityEngine.Object.Instantiate(vfxExplosion, actor.transform.position, Quaternion.identity));
			tk2dBaseSprite component2 = gameObject.GetComponent<tk2dBaseSprite>();
			if ((bool)actor && (bool)actor.specRigidbody && (bool)component2)
			{
				component2.PlaceAtPositionByAnchor(actor.specRigidbody.HitboxPixelCollider.UnitBottomCenter, tk2dBaseSprite.Anchor.LowerCenter);
			}
			HammerOfDawnController component3 = gameObject.GetComponent<HammerOfDawnController>();
			if ((bool)component3 && (bool)sourceProjectile)
			{
				component3.AssignOwner(sourceProjectile.Owner as PlayerController, sourceProjectile);
			}
		}
		if ((bool)m_extantReticle)
		{
			UnityEngine.Object.Destroy(m_extantReticle.gameObject);
			m_extantReticle = null;
		}
	}

	public override void EffectTick(GameActor actor, RuntimeGameActorEffectData effectData)
	{
		if (effectData.accumulator > 0f)
		{
			effectData.accumulator = Mathf.Max(0f, effectData.accumulator - BraveTime.DeltaTime * ChargeDispelFactor);
		}
		if ((bool)m_extantReticle)
		{
			RailgunChargeEffectController component = m_extantReticle.GetComponent<RailgunChargeEffectController>();
			if ((bool)component)
			{
				component.ManualCompletionPercentage = effectData.accumulator / 100f;
			}
		}
	}

	public override void OnEffectRemoved(GameActor actor, RuntimeGameActorEffectData effectData)
	{
		if ((bool)m_extantReticle)
		{
			UnityEngine.Object.Destroy(m_extantReticle.gameObject);
			m_extantReticle = null;
		}
	}

	public override bool IsFinished(GameActor actor, RuntimeGameActorEffectData effectData, float elapsedTime)
	{
		return effectData.accumulator <= 0f;
	}
}
