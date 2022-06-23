using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnGunDamagedModifier : MonoBehaviour, IGunInheritable
{
	[CheckAnimation(null)]
	public string BrokenAnimation;

	public bool DepleteAmmoOnDamage;

	public bool NondepletedGunGrantsFlight;

	public bool DisableHandsOnDepletion;

	public bool PreventDepleteWithSynergy;

	[LongNumericEnum]
	public CustomSynergyType PreventDepleteSynergy;

	private Gun m_gun;

	private PlayerController m_playerOwner;

	private string m_cachedIdleAnimation;

	private string m_cachedEmptyAnimation;

	private string m_cachedChargeAnimation;

	private string m_cachedIntroAnimation;

	private int m_cachedDefaultID = -1;

	private bool m_hasAwoken;

	private bool m_gunBroken;

	private int m_lastFramePlayerHadSynergy = -1;

	public bool Broken
	{
		get
		{
			return m_gunBroken;
		}
		set
		{
			m_gunBroken = value;
		}
	}

	private void Awake()
	{
		m_hasAwoken = true;
		m_gun = GetComponent<Gun>();
		m_cachedIdleAnimation = m_gun.idleAnimation;
		m_cachedEmptyAnimation = m_gun.emptyAnimation;
		m_cachedChargeAnimation = m_gun.chargeAnimation;
		m_cachedIntroAnimation = m_gun.introAnimation;
		m_cachedDefaultID = m_gun.DefaultSpriteID;
		if (m_gunBroken && !string.IsNullOrEmpty(BrokenAnimation))
		{
			SetBrokenAnims();
		}
		Gun gun = m_gun;
		gun.OnInitializedWithOwner = (Action<GameActor>)Delegate.Combine(gun.OnInitializedWithOwner, new Action<GameActor>(OnGunInitialized));
		Gun gun2 = m_gun;
		gun2.OnDropped = (Action)Delegate.Combine(gun2.OnDropped, new Action(OnGunDroppedOrDestroyed));
		Gun gun3 = m_gun;
		gun3.OnAmmoChanged = (Action<PlayerController, Gun>)Delegate.Combine(gun3.OnAmmoChanged, new Action<PlayerController, Gun>(HandleAmmoChanged));
		Gun gun4 = m_gun;
		gun4.OnPostFired = (Action<PlayerController, Gun>)Delegate.Combine(gun4.OnPostFired, new Action<PlayerController, Gun>(HandleAmmoChanged));
		if (m_gun.CurrentOwner != null)
		{
			OnGunInitialized(m_gun.CurrentOwner);
		}
	}

	private void Start()
	{
		m_cachedDefaultID = m_gun.DefaultSpriteID;
	}

	private void Update()
	{
		if ((bool)m_playerOwner && PreventDepleteWithSynergy && m_playerOwner.HasActiveBonusSynergy(PreventDepleteSynergy))
		{
			m_lastFramePlayerHadSynergy = Time.frameCount;
		}
	}

	private void SetBrokenAnims()
	{
		m_gun.CanBeDropped = false;
		m_gun.idleAnimation = BrokenAnimation;
		m_gun.emptyAnimation = BrokenAnimation;
		m_gun.chargeAnimation = string.Empty;
		m_gun.introAnimation = string.Empty;
		tk2dSpriteAnimationClip clipByName = m_gun.spriteAnimator.GetClipByName(BrokenAnimation);
		m_gun.DefaultSpriteID = clipByName.frames[clipByName.frames.Length - 1].spriteId;
	}

	private void HandleAmmoChanged(PlayerController player, Gun ammoGun)
	{
		if (!m_playerOwner)
		{
			return;
		}
		if (ammoGun == m_gun && ammoGun.ammo >= 1 && m_gunBroken)
		{
			m_gunBroken = false;
			if (DisableHandsOnDepletion)
			{
				m_gun.additionalHandState = AdditionalHandState.None;
				player.ToggleHandRenderers(true, string.Empty);
				player.ProcessHandAttachment();
				GameManager.Instance.Dungeon.StartCoroutine(FrameDelayedProcessing(player));
			}
			if (!string.IsNullOrEmpty(BrokenAnimation))
			{
				m_gun.CanBeDropped = true;
				m_gun.idleAnimation = m_cachedIdleAnimation;
				m_gun.emptyAnimation = m_cachedEmptyAnimation;
				m_gun.chargeAnimation = m_cachedChargeAnimation;
				m_gun.introAnimation = m_cachedIntroAnimation;
				m_gun.DefaultSpriteID = m_cachedDefaultID;
				m_gun.PlayIdleAnimation();
			}
		}
		CheckFlightStatus(m_playerOwner.CurrentGun);
	}

	private IEnumerator FrameDelayedProcessing(PlayerController p)
	{
		yield return null;
		if ((bool)p && p.CurrentGun == m_gun && (bool)m_gun)
		{
			p.ToggleHandRenderers(true, string.Empty);
			p.ProcessHandAttachment();
		}
	}

	private void OnGunInitialized(GameActor actor)
	{
		if (m_playerOwner != null)
		{
			OnGunDroppedOrDestroyed();
		}
		if (!(actor == null))
		{
			if (actor is PlayerController)
			{
				m_playerOwner = actor as PlayerController;
				m_playerOwner.OnReceivedDamage += OnReceivedDamage;
				m_playerOwner.GunChanged += HandleGunChanged;
			}
			if ((bool)m_playerOwner)
			{
				CheckFlightStatus(m_playerOwner.CurrentGun);
			}
		}
	}

	private void CheckFlightStatus(Gun currentGun)
	{
		if ((bool)m_gun)
		{
			m_gun.overrideOutOfAmmoHandedness = GunHandedness.NoHanded;
		}
		if (NondepletedGunGrantsFlight && (bool)m_playerOwner && (bool)currentGun)
		{
			OnGunDamagedModifier component = currentGun.GetComponent<OnGunDamagedModifier>();
			if ((bool)component && component.NondepletedGunGrantsFlight)
			{
				m_playerOwner.SetIsFlying(!component.m_gunBroken, "balloon gun", false);
				m_playerOwner.AdditionalCanDodgeRollWhileFlying.SetOverride("balloon gun", true);
			}
			else
			{
				m_playerOwner.SetIsFlying(false, "balloon gun", false);
				m_playerOwner.AdditionalCanDodgeRollWhileFlying.RemoveOverride("balloon gun");
			}
		}
	}

	private void HandleGunChanged(Gun previous, Gun current, bool isNew)
	{
		CheckFlightStatus(current);
	}

	private void OnReceivedDamage(PlayerController player)
	{
		if (!player || !(player.CurrentGun == m_gun) || (PreventDepleteWithSynergy && player.HasActiveBonusSynergy(PreventDepleteSynergy)) || (PreventDepleteWithSynergy && m_lastFramePlayerHadSynergy == Time.frameCount))
		{
			return;
		}
		if (!m_gunBroken)
		{
			m_gunBroken = true;
			if (!string.IsNullOrEmpty(BrokenAnimation))
			{
				SetBrokenAnims();
				m_gun.PlayIdleAnimation();
			}
		}
		if (DepleteAmmoOnDamage && (!PreventDepleteWithSynergy || !player.HasActiveBonusSynergy(PreventDepleteSynergy)))
		{
			m_gun.ammo = 0;
			if (DisableHandsOnDepletion)
			{
				m_gun.additionalHandState = AdditionalHandState.HideBoth;
			}
		}
		CheckFlightStatus(player.CurrentGun);
	}

	private void OnDestroy()
	{
		OnGunDroppedOrDestroyed();
	}

	private void OnGunDroppedOrDestroyed()
	{
		if (m_playerOwner != null)
		{
			m_playerOwner.OnReceivedDamage -= OnReceivedDamage;
			m_playerOwner.GunChanged -= HandleGunChanged;
			m_playerOwner = null;
		}
	}

	public void InheritData(Gun sourceGun)
	{
		if (!sourceGun)
		{
			return;
		}
		if (!m_hasAwoken)
		{
			m_gun = GetComponent<Gun>();
			m_cachedIdleAnimation = m_gun.idleAnimation;
			m_cachedEmptyAnimation = m_gun.emptyAnimation;
			m_cachedChargeAnimation = m_gun.chargeAnimation;
			m_cachedIntroAnimation = m_gun.introAnimation;
			m_cachedDefaultID = m_gun.DefaultSpriteID;
		}
		OnGunDamagedModifier component = sourceGun.GetComponent<OnGunDamagedModifier>();
		if ((bool)component)
		{
			m_gunBroken = component.m_gunBroken;
			if (!string.IsNullOrEmpty(component.m_cachedEmptyAnimation))
			{
				m_cachedEmptyAnimation = component.m_cachedEmptyAnimation;
			}
			if (!string.IsNullOrEmpty(component.m_cachedIdleAnimation))
			{
				m_cachedIdleAnimation = component.m_cachedIdleAnimation;
			}
			if (!string.IsNullOrEmpty(component.m_cachedChargeAnimation))
			{
				m_cachedChargeAnimation = component.m_cachedChargeAnimation;
			}
			if (!string.IsNullOrEmpty(component.m_cachedIntroAnimation))
			{
				m_cachedIntroAnimation = component.m_cachedIntroAnimation;
			}
			if (component.m_cachedDefaultID != -1)
			{
				m_cachedDefaultID = component.m_cachedDefaultID;
			}
			GetComponent<Gun>().idleAnimation = m_cachedIdleAnimation;
			GetComponent<Gun>().emptyAnimation = m_cachedEmptyAnimation;
			GetComponent<Gun>().chargeAnimation = m_cachedChargeAnimation;
			GetComponent<Gun>().introAnimation = m_cachedIntroAnimation;
		}
	}

	public void MidGameSerialize(List<object> data, int dataIndex)
	{
		data.Add(Broken);
	}

	public void MidGameDeserialize(List<object> data, ref int dataIndex)
	{
		Broken = (bool)data[dataIndex];
		if (m_gunBroken && !string.IsNullOrEmpty(BrokenAnimation))
		{
			SetBrokenAnims();
			m_gun.PlayIdleAnimation();
		}
		dataIndex++;
	}
}
