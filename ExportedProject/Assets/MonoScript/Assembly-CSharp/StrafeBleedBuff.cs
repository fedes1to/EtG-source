using System;
using System.Collections.Generic;
using UnityEngine;

public class StrafeBleedBuff : AppliedEffectBase
{
	public bool PreventExplosion;

	public ExplosionData explosionData;

	public GameObject vfx;

	public GameObject additionalVFX;

	public float CascadeTime = 3f;

	private GameObject instantiatedVFX;

	private Gun m_attachedGun;

	private HealthHaver hh;

	private bool m_initialized;

	private float m_elapsed;

	private Vector2 m_cachedSourceVector = Vector2.zero;

	private void InitializeSelf(StrafeBleedBuff source)
	{
		if (!source)
		{
			return;
		}
		m_initialized = true;
		explosionData = source.explosionData;
		PreventExplosion = source.PreventExplosion;
		hh = GetComponent<HealthHaver>();
		if (hh != null)
		{
			Projectile component = source.GetComponent<Projectile>();
			if (component.PossibleSourceGun != null)
			{
				m_attachedGun = component.PossibleSourceGun;
				Gun possibleSourceGun = component.PossibleSourceGun;
				possibleSourceGun.OnFinishAttack = (Action<PlayerController, Gun>)Delegate.Combine(possibleSourceGun.OnFinishAttack, new Action<PlayerController, Gun>(HandleCeaseAttack));
			}
			else if ((bool)component && (bool)component.Owner && (bool)component.Owner.CurrentGun)
			{
				m_attachedGun = component.Owner.CurrentGun;
				Gun currentGun = component.Owner.CurrentGun;
				currentGun.OnFinishAttack = (Action<PlayerController, Gun>)Delegate.Combine(currentGun.OnFinishAttack, new Action<PlayerController, Gun>(HandleCeaseAttack));
			}
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	private void Update()
	{
		if (m_initialized)
		{
			m_elapsed += BraveTime.DeltaTime;
			if (m_elapsed > CascadeTime)
			{
				DoEffect();
				Disconnect();
			}
		}
	}

	private void HandleCeaseAttack(PlayerController arg1, Gun arg2)
	{
		DoEffect();
		Disconnect();
	}

	private void Disconnect()
	{
		m_initialized = false;
		if ((bool)m_attachedGun)
		{
			Gun attachedGun = m_attachedGun;
			attachedGun.OnFinishAttack = (Action<PlayerController, Gun>)Delegate.Remove(attachedGun.OnFinishAttack, new Action<PlayerController, Gun>(HandleCeaseAttack));
		}
	}

	public override void Initialize(AppliedEffectBase source)
	{
		if (source is StrafeBleedBuff)
		{
			StrafeBleedBuff strafeBleedBuff = source as StrafeBleedBuff;
			if (GetComponent<StrafeBleedBuff>() == this && strafeBleedBuff.additionalVFX != null && (bool)GetComponent<SpeculativeRigidbody>())
			{
				SpeculativeRigidbody component = GetComponent<SpeculativeRigidbody>();
				GameObject gameObject = SpawnManager.SpawnVFX(strafeBleedBuff.additionalVFX, component.UnitCenter, Quaternion.identity, true);
				gameObject.transform.parent = base.transform;
			}
			InitializeSelf(strafeBleedBuff);
			if (!(strafeBleedBuff.vfx != null))
			{
				return;
			}
			instantiatedVFX = SpawnManager.SpawnVFX(strafeBleedBuff.vfx, base.transform.position, Quaternion.identity, true);
			tk2dSprite component2 = instantiatedVFX.GetComponent<tk2dSprite>();
			tk2dSprite component3 = GetComponent<tk2dSprite>();
			if (component2 != null && component3 != null)
			{
				component3.AttachRenderer(component2);
				component2.HeightOffGround = 0.1f;
				component2.IsPerpendicular = true;
				component2.usesOverrideMaterial = true;
			}
			BuffVFXAnimator component4 = instantiatedVFX.GetComponent<BuffVFXAnimator>();
			if (component4 != null)
			{
				Projectile component5 = source.GetComponent<Projectile>();
				if ((bool)component5 && component5.LastVelocity != Vector2.zero)
				{
					m_cachedSourceVector = component5.LastVelocity;
					component4.InitializePierce(GetComponent<GameActor>(), component5.LastVelocity);
				}
				else
				{
					component4.Initialize(GetComponent<GameActor>());
				}
			}
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	public override void AddSelfToTarget(GameObject target)
	{
		if (!(target.GetComponent<HealthHaver>() == null))
		{
			StrafeBleedBuff strafeBleedBuff = target.AddComponent<StrafeBleedBuff>();
			strafeBleedBuff.Initialize(this);
		}
	}

	private void DoEffect()
	{
		if ((bool)hh && !PreventExplosion)
		{
			float force = explosionData.force / 4f;
			explosionData.force = 0f;
			if ((bool)hh.specRigidbody)
			{
				if (explosionData.ignoreList == null)
				{
					explosionData.ignoreList = new List<SpeculativeRigidbody>();
				}
				explosionData.ignoreList.Add(hh.specRigidbody);
				hh.ApplyDamage(explosionData.damage, m_cachedSourceVector.normalized, "Strafe");
			}
			if (instantiatedVFX != null)
			{
				Exploder.Explode(instantiatedVFX.GetComponent<tk2dBaseSprite>().WorldCenter + m_cachedSourceVector.normalized * -0.5f, explosionData, Vector2.zero, null, true);
			}
			else
			{
				Exploder.Explode(hh.aiActor.CenterPosition, explosionData, Vector2.zero, null, true);
			}
			if ((bool)hh.knockbackDoer && m_cachedSourceVector != Vector2.zero)
			{
				hh.knockbackDoer.ApplyKnockback(m_cachedSourceVector.normalized, force);
			}
		}
		if ((bool)instantiatedVFX)
		{
			UnityEngine.Object.Destroy(instantiatedVFX);
		}
		UnityEngine.Object.Destroy(this);
	}

	private void OnDestroy()
	{
		Disconnect();
	}
}
