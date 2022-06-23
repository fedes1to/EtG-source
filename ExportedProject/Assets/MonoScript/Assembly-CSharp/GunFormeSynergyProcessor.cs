using System;
using System.Collections;
using UnityEngine;

public class GunFormeSynergyProcessor : MonoBehaviour
{
	public GunFormeData[] Formes;

	private Gun m_gun;

	private int CurrentForme;

	[NonSerialized]
	public bool JustActiveReloaded;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		Gun gun = m_gun;
		gun.OnReloadPressed = (Action<PlayerController, Gun, bool>)Delegate.Combine(gun.OnReloadPressed, new Action<PlayerController, Gun, bool>(HandleReloadPressed));
	}

	private void Update()
	{
		if ((bool)m_gun && !m_gun.CurrentOwner && CurrentForme != 0)
		{
			ChangeForme(Formes[0]);
			CurrentForme = 0;
		}
		JustActiveReloaded = false;
	}

	private void HandleReloadPressed(PlayerController ownerPlayer, Gun sourceGun, bool manual)
	{
		if (!JustActiveReloaded && manual && !sourceGun.IsReloading)
		{
			int nextValidForme = GetNextValidForme(ownerPlayer);
			if (nextValidForme != CurrentForme)
			{
				ChangeForme(Formes[nextValidForme]);
				CurrentForme = nextValidForme;
			}
		}
	}

	private int GetNextValidForme(PlayerController ownerPlayer)
	{
		for (int i = 0; i < Formes.Length; i++)
		{
			int num = (i + CurrentForme) % Formes.Length;
			if (num != CurrentForme && Formes[num].IsValid(ownerPlayer))
			{
				return num;
			}
		}
		return CurrentForme;
	}

	private void ChangeForme(GunFormeData targetForme)
	{
		Gun gun = PickupObjectDatabase.GetById(targetForme.FormeID) as Gun;
		m_gun.TransformToTargetGun(gun);
		if ((bool)m_gun.encounterTrackable && (bool)gun.encounterTrackable)
		{
			m_gun.encounterTrackable.journalData.PrimaryDisplayName = gun.encounterTrackable.journalData.PrimaryDisplayName;
			m_gun.encounterTrackable.journalData.ClearCache();
			PlayerController playerController = m_gun.CurrentOwner as PlayerController;
			if ((bool)playerController)
			{
				GameUIRoot.Instance.TemporarilyShowGunName(playerController.IsPrimaryPlayer);
			}
		}
	}

	public static void AssignTemporaryOverrideGun(PlayerController targetPlayer, int gunID, float duration)
	{
		if ((bool)targetPlayer && !targetPlayer.IsGhost)
		{
			targetPlayer.StartCoroutine(HandleTransformationDuration(targetPlayer, gunID, duration));
		}
	}

	public static IEnumerator HandleTransformationDuration(PlayerController targetPlayer, int gunID, float duration)
	{
		float elapsed2 = 0f;
		if ((bool)targetPlayer && targetPlayer.inventory.GunLocked.Value && (bool)targetPlayer.CurrentGun)
		{
			MimicGunController component = targetPlayer.CurrentGun.GetComponent<MimicGunController>();
			if ((bool)component)
			{
				component.ForceClearMimic();
			}
		}
		targetPlayer.inventory.GunChangeForgiveness = true;
		Gun limitGun = PickupObjectDatabase.GetById(gunID) as Gun;
		Gun m_extantGun = targetPlayer.inventory.AddGunToInventory(limitGun, true);
		m_extantGun.CanBeDropped = false;
		m_extantGun.CanBeSold = false;
		targetPlayer.inventory.GunLocked.SetOverride("override gun", true);
		elapsed2 = 0f;
		while (elapsed2 < duration)
		{
			elapsed2 += BraveTime.DeltaTime;
			yield return null;
		}
		ClearTemporaryOverrideGun(targetPlayer, m_extantGun);
	}

	protected static void ClearTemporaryOverrideGun(PlayerController targetPlayer, Gun m_extantGun)
	{
		if ((bool)targetPlayer && (bool)m_extantGun)
		{
			if ((bool)targetPlayer)
			{
				targetPlayer.inventory.GunLocked.RemoveOverride("override gun");
				targetPlayer.inventory.DestroyGun(m_extantGun);
				m_extantGun = null;
			}
			targetPlayer.inventory.GunChangeForgiveness = false;
		}
	}
}
