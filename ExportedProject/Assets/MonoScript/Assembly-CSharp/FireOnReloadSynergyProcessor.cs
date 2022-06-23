using System;
using UnityEngine;

public class FireOnReloadSynergyProcessor : MonoBehaviour
{
	public bool RequiresNoSynergy;

	[LongNumericEnum]
	public CustomSynergyType SynergyToCheck;

	public bool OnlyOnEmptyClip;

	public bool DoesRadialBurst = true;

	public RadialBurstInterface RadialBurstSettings;

	public bool DoesDirectedBurst;

	public DirectedBurstInterface DirectedBurstSettings;

	public string SwitchGroup;

	public string SFX;

	private Gun m_gun;

	private PassiveItem m_item;

	private void Awake()
	{
		Gun component = GetComponent<Gun>();
		if (component != null)
		{
			component.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(component.OnReloadPressed, new Action<PlayerController, Gun, bool>(HandleReloaded));
			return;
		}
		m_item = GetComponent<PassiveItem>();
		if (m_item != null)
		{
			PassiveItem item = m_item;
			item.OnPickedUp = (Action<PlayerController>)Delegate.Combine(item.OnPickedUp, new Action<PlayerController>(Hookup));
		}
	}

	private void Hookup(PlayerController acquiringPlayer)
	{
		acquiringPlayer.OnReloadPressed = (Action<PlayerController, Gun>)Delegate.Combine(acquiringPlayer.OnReloadPressed, new Action<PlayerController, Gun>(HandleReloadedPlayer));
	}

	private void HandleReloadedPlayer(PlayerController usingPlayer, Gun usedGun)
	{
		if (!m_item || !m_item.Owner)
		{
			usingPlayer.OnReloadPressed = (Action<PlayerController, Gun>)Delegate.Remove(usingPlayer.OnReloadPressed, new Action<PlayerController, Gun>(HandleReloadedPlayer));
		}
		else
		{
			HandleReloaded(usingPlayer, usedGun, false);
		}
	}

	private void HandleReloaded(PlayerController usingPlayer, Gun usedGun, bool manual)
	{
		if ((!OnlyOnEmptyClip || usedGun.ClipShotsRemaining <= 0) && usedGun.IsReloading && (bool)usingPlayer && (RequiresNoSynergy || usingPlayer.HasActiveBonusSynergy(SynergyToCheck)) && (!usedGun || !usedGun.HasFiredReloadSynergy))
		{
			usedGun.HasFiredReloadSynergy = true;
			if (DoesRadialBurst)
			{
				AkSoundEngine.SetSwitch("WPN_Guns", SwitchGroup, base.gameObject);
				AkSoundEngine.PostEvent(SFX, base.gameObject);
				RadialBurstSettings.DoBurst(usingPlayer);
				AkSoundEngine.SetSwitch("WPN_Guns", usedGun.gunSwitchGroup, base.gameObject);
			}
			if (DoesDirectedBurst)
			{
				AkSoundEngine.SetSwitch("WPN_Guns", SwitchGroup, base.gameObject);
				AkSoundEngine.PostEvent(SFX, base.gameObject);
				DirectedBurstSettings.DoBurst(usingPlayer, usedGun.CurrentAngle);
				AkSoundEngine.SetSwitch("WPN_Guns", usedGun.gunSwitchGroup, base.gameObject);
			}
		}
	}
}
