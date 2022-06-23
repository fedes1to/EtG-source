using System.Collections;
using Dungeonator;
using UnityEngine;

public class MagazineRack : MonoBehaviour
{
	public float Duration = 10f;

	public float Radius = 5f;

	private bool m_radialIndicatorActive;

	private HeatIndicatorController m_radialIndicator;

	private int m_p1MaxGunAmmoThisFrame = 1000;

	private int m_p1GunIDThisFrame = -1;

	private int m_p2MaxGunAmmoThisFrame = 1000;

	private int m_p2GunIDThisFrame = -1;

	public IEnumerator Start()
	{
		HandleRadialIndicator();
		yield return new WaitForSeconds(Duration);
		Object.Destroy(base.gameObject);
	}

	private void Update()
	{
		if (Dungeon.IsGenerating || GameManager.Instance.IsLoadingLevel)
		{
			return;
		}
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			float num = Radius;
			if (playerController.HasActiveBonusSynergy(CustomSynergyType.MAGAZINE_CLIPS))
			{
				num *= 2f;
			}
			if ((bool)playerController && Vector2.Distance(playerController.CenterPosition, base.transform.position.XY()) < num)
			{
				if (i == 0 && (bool)playerController.CurrentGun)
				{
					m_p1MaxGunAmmoThisFrame = playerController.CurrentGun.CurrentAmmo;
					m_p1GunIDThisFrame = playerController.CurrentGun.PickupObjectId;
				}
				if (i == 1 && (bool)playerController.CurrentGun)
				{
					m_p2MaxGunAmmoThisFrame = playerController.CurrentGun.CurrentAmmo;
					m_p2GunIDThisFrame = playerController.CurrentGun.PickupObjectId;
				}
				playerController.InfiniteAmmo.SetOverride("MagazineRack", true);
				playerController.OnlyFinalProjectiles.SetOverride("MagazineRack", playerController.HasActiveBonusSynergy(CustomSynergyType.JUNK_MAIL));
			}
			else if ((bool)playerController)
			{
				playerController.InfiniteAmmo.SetOverride("MagazineRack", false);
				playerController.OnlyFinalProjectiles.SetOverride("MagazineRack", false);
			}
		}
	}

	private void LateUpdate()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if ((bool)playerController && (bool)playerController.CurrentGun && playerController.InfiniteAmmo.HasOverride("MagazineRack"))
			{
				int num = ((i != 0) ? m_p2MaxGunAmmoThisFrame : m_p1MaxGunAmmoThisFrame);
				int num2 = ((i != 0) ? m_p2GunIDThisFrame : m_p1GunIDThisFrame);
				if (!playerController.CurrentGun.RequiresFundsToShoot && playerController.CurrentGun.CurrentAmmo < num && playerController.CurrentGun.PickupObjectId == num2)
				{
					playerController.CurrentGun.ammo = Mathf.Min(playerController.CurrentGun.AdjustedMaxAmmo, num);
				}
			}
			if (i == 0 && (bool)playerController.CurrentGun)
			{
				m_p1MaxGunAmmoThisFrame = playerController.CurrentGun.CurrentAmmo;
			}
			if (i == 1 && (bool)playerController.CurrentGun)
			{
				m_p2MaxGunAmmoThisFrame = playerController.CurrentGun.CurrentAmmo;
			}
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if ((bool)playerController)
			{
				playerController.InfiniteAmmo.SetOverride("MagazineRack", false);
				playerController.OnlyFinalProjectiles.SetOverride("MagazineRack", false);
			}
		}
		UnhandleRadialIndicator();
	}

	private void HandleRadialIndicator()
	{
		if (!m_radialIndicatorActive)
		{
			m_radialIndicatorActive = true;
			m_radialIndicator = ((GameObject)Object.Instantiate(ResourceCache.Acquire("Global VFX/HeatIndicator"), base.transform.position, Quaternion.identity, base.transform)).GetComponent<HeatIndicatorController>();
			Debug.LogError("setting color and fire");
			m_radialIndicator.CurrentColor = Color.white;
			m_radialIndicator.IsFire = false;
			float num = Radius;
			int count = -1;
			if (PlayerController.AnyoneHasActiveBonusSynergy(CustomSynergyType.MAGAZINE_CLIPS, out count))
			{
				num *= 2f;
			}
			m_radialIndicator.CurrentRadius = num;
		}
	}

	private void UnhandleRadialIndicator()
	{
		if (m_radialIndicatorActive)
		{
			m_radialIndicatorActive = false;
			if ((bool)m_radialIndicator)
			{
				m_radialIndicator.EndEffect();
			}
			m_radialIndicator = null;
		}
	}
}
