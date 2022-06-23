using System;
using UnityEngine;

public class StickyGrenadeBuff : AppliedEffectBase
{
	public bool IsSynergyContingent;

	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public ExplosionData explosionData;

	public GameObject vfx;

	private GameObject instantiatedVFX;

	private PlayerController m_player;

	private Gun m_attachedGun;

	private HealthHaver hh;

	private Vector2 m_cachedSourceVector = Vector2.zero;

	private void InitializeSelf(StickyGrenadeBuff source)
	{
		if (!source)
		{
			return;
		}
		explosionData = source.explosionData;
		hh = GetComponent<HealthHaver>();
		if (hh != null)
		{
			Projectile component = source.GetComponent<Projectile>();
			if (component.PossibleSourceGun != null)
			{
				m_attachedGun = component.PossibleSourceGun;
				m_player = component.PossibleSourceGun.CurrentOwner as PlayerController;
				Gun possibleSourceGun = component.PossibleSourceGun;
				possibleSourceGun.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(possibleSourceGun.OnReloadPressed, new Action<PlayerController, Gun, bool>(ExplodeOnReload));
				if ((bool)m_player)
				{
					m_player.GunChanged += GunChanged;
				}
			}
			else if ((bool)component && (bool)component.Owner && (bool)component.Owner.CurrentGun)
			{
				m_attachedGun = component.Owner.CurrentGun;
				m_player = component.Owner as PlayerController;
				Gun currentGun = component.Owner.CurrentGun;
				currentGun.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(currentGun.OnReloadPressed, new Action<PlayerController, Gun, bool>(ExplodeOnReload));
				if ((bool)m_player)
				{
					m_player.GunChanged += GunChanged;
				}
			}
		}
		else
		{
			UnityEngine.Object.Destroy(this);
		}
	}

	private void Disconnect()
	{
		if ((bool)m_player)
		{
			m_player.GunChanged -= GunChanged;
		}
		if ((bool)m_attachedGun)
		{
			Gun attachedGun = m_attachedGun;
			attachedGun.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Remove(attachedGun.OnReloadPressed, new Action<PlayerController, Gun, bool>(ExplodeOnReload));
		}
	}

	private void GunChanged(Gun arg1, Gun arg2, bool newGun)
	{
		Disconnect();
		DoEffect();
	}

	private void ExplodeOnReload(PlayerController arg1, Gun arg2, bool actual)
	{
		Disconnect();
		DoEffect();
	}

	public override void Initialize(AppliedEffectBase source)
	{
		if (source is StickyGrenadeBuff)
		{
			StickyGrenadeBuff stickyGrenadeBuff = source as StickyGrenadeBuff;
			InitializeSelf(stickyGrenadeBuff);
			if (!(stickyGrenadeBuff.vfx != null))
			{
				return;
			}
			instantiatedVFX = SpawnManager.SpawnVFX(stickyGrenadeBuff.vfx, base.transform.position, Quaternion.identity, true);
			tk2dSprite component = instantiatedVFX.GetComponent<tk2dSprite>();
			tk2dSprite component2 = GetComponent<tk2dSprite>();
			if (component != null && component2 != null)
			{
				component2.AttachRenderer(component);
				component.HeightOffGround = 0.1f;
				component.IsPerpendicular = true;
				component.usesOverrideMaterial = true;
			}
			BuffVFXAnimator component3 = instantiatedVFX.GetComponent<BuffVFXAnimator>();
			if (component3 != null)
			{
				Projectile component4 = source.GetComponent<Projectile>();
				if ((bool)component4 && component4.LastVelocity != Vector2.zero)
				{
					m_cachedSourceVector = component4.LastVelocity;
					component3.InitializePierce(GetComponent<GameActor>(), component4.LastVelocity);
				}
				else
				{
					component3.Initialize(GetComponent<GameActor>());
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
		if (target.GetComponent<HealthHaver>() == null)
		{
			return;
		}
		if (IsSynergyContingent)
		{
			Projectile component = GetComponent<Projectile>();
			if (!component || !(component.Owner is PlayerController) || !(component.Owner as PlayerController).HasActiveBonusSynergy(RequiredSynergy))
			{
				return;
			}
		}
		StickyGrenadeBuff stickyGrenadeBuff = target.AddComponent<StickyGrenadeBuff>();
		stickyGrenadeBuff.Initialize(this);
	}

	private void DoEffect()
	{
		if ((bool)hh)
		{
			float force = explosionData.force / 4f;
			explosionData.force = 0f;
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
