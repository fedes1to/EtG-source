using System;
using UnityEngine;

public class OrbitalSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public bool RequiresNoSynergy;

	public PlayerOrbital OrbitalPrefab;

	public PlayerOrbitalFollower OrbitalFollowerPrefab;

	private Gun m_gun;

	private PassiveItem m_item;

	protected GameObject m_extantOrbital;

	private bool m_active;

	private PlayerController m_cachedPlayer;

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		m_item = GetComponent<PassiveItem>();
	}

	private void OnDisable()
	{
		DeactivateSynergy();
		m_active = false;
	}

	private void CreateOrbital(PlayerController owner)
	{
		GameObject gameObject = (m_extantOrbital = UnityEngine.Object.Instantiate((!(OrbitalPrefab != null)) ? OrbitalFollowerPrefab.gameObject : OrbitalPrefab.gameObject, owner.transform.position, Quaternion.identity));
		if (OrbitalPrefab != null)
		{
			m_extantOrbital.GetComponent<PlayerOrbital>().Initialize(owner);
		}
		else if (OrbitalFollowerPrefab != null)
		{
			m_extantOrbital.GetComponent<PlayerOrbitalFollower>().Initialize(owner);
		}
	}

	private void DestroyOrbital()
	{
		if ((bool)m_extantOrbital)
		{
			UnityEngine.Object.Destroy(m_extantOrbital.gameObject);
			m_extantOrbital = null;
		}
	}

	private void HandleNewFloor(PlayerController obj)
	{
		DestroyOrbital();
		CreateOrbital(obj);
	}

	public void ActivateSynergy(PlayerController player)
	{
		player.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Combine(player.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloor));
		CreateOrbital(player);
	}

	public void DeactivateSynergy()
	{
		if (m_cachedPlayer != null)
		{
			PlayerController cachedPlayer = m_cachedPlayer;
			cachedPlayer.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(cachedPlayer.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloor));
			m_cachedPlayer = null;
		}
		DestroyOrbital();
	}

	public void Update()
	{
		PlayerController playerController = null;
		if ((bool)m_gun)
		{
			playerController = m_gun.CurrentOwner as PlayerController;
		}
		else if ((bool)m_item)
		{
			playerController = m_item.Owner;
		}
		if ((bool)playerController && (RequiresNoSynergy || playerController.HasActiveBonusSynergy(RequiredSynergy)) && !m_active)
		{
			m_active = true;
			m_cachedPlayer = playerController;
			ActivateSynergy(playerController);
		}
		else if (!playerController || (!RequiresNoSynergy && !playerController.HasActiveBonusSynergy(RequiredSynergy) && m_active))
		{
			DeactivateSynergy();
			m_active = false;
		}
	}
}
