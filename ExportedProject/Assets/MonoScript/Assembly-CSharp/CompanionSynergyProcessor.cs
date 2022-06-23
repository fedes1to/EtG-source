using System;
using System.Collections;
using UnityEngine;

public class CompanionSynergyProcessor : MonoBehaviour
{
	[LongNumericEnum]
	public CustomSynergyType RequiredSynergy;

	public bool RequiresNoSynergy;

	public bool PersistsOnDisable;

	[EnemyIdentifier]
	public string CompanionGuid;

	[NonSerialized]
	public bool PreventRespawnOnFloorLoad;

	private Gun m_gun;

	private PassiveItem m_item;

	private PlayerItem m_activeItem;

	[NonSerialized]
	public PlayerController ManuallyAssignedPlayer;

	private GameObject m_extantCompanion;

	private bool m_active;

	private PlayerController m_cachedPlayer;

	public GameObject ExtantCompanion
	{
		get
		{
			return m_extantCompanion;
		}
	}

	private void CreateCompanion(PlayerController owner)
	{
		if (!PreventRespawnOnFloorLoad)
		{
			AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(CompanionGuid);
			Vector3 position = owner.transform.position;
			GameObject gameObject = (m_extantCompanion = UnityEngine.Object.Instantiate(orLoadByGuid.gameObject, position, Quaternion.identity));
			CompanionController orAddComponent = m_extantCompanion.GetOrAddComponent<CompanionController>();
			orAddComponent.Initialize(owner);
			if ((bool)orAddComponent.specRigidbody)
			{
				PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(orAddComponent.specRigidbody);
			}
		}
	}

	private void DestroyCompanion()
	{
		if ((bool)m_extantCompanion)
		{
			UnityEngine.Object.Destroy(m_extantCompanion);
			m_extantCompanion = null;
		}
	}

	private void Awake()
	{
		m_gun = GetComponent<Gun>();
		m_item = GetComponent<PassiveItem>();
		m_activeItem = GetComponent<PlayerItem>();
	}

	public void Update()
	{
		PlayerController playerController = ManuallyAssignedPlayer;
		if (!playerController && (bool)m_item)
		{
			playerController = m_item.Owner;
		}
		if (!playerController && (bool)m_activeItem && m_activeItem.PickedUp && (bool)m_activeItem.LastOwner)
		{
			playerController = m_activeItem.LastOwner;
		}
		if (!playerController && (bool)m_gun && m_gun.CurrentOwner is PlayerController)
		{
			playerController = m_gun.CurrentOwner as PlayerController;
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

	private void OnDisable()
	{
		if (m_active && !PersistsOnDisable)
		{
			DeactivateSynergy();
			m_active = false;
		}
		else if (m_active && (bool)m_cachedPlayer)
		{
			m_cachedPlayer.StartCoroutine(HandleDisabledChecks());
		}
	}

	private IEnumerator HandleDisabledChecks()
	{
		yield return null;
		while ((bool)this && (bool)m_cachedPlayer && !base.isActiveAndEnabled && m_active)
		{
			if (!m_cachedPlayer.HasActiveBonusSynergy(RequiredSynergy))
			{
				DeactivateSynergy();
				m_active = false;
				break;
			}
			yield return null;
		}
	}

	private void OnDestroy()
	{
		if (m_active)
		{
			DeactivateSynergy();
			m_active = false;
		}
	}

	public void ActivateSynergy(PlayerController player)
	{
		player.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Combine(player.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloor));
		CreateCompanion(player);
	}

	private void HandleNewFloor(PlayerController obj)
	{
		DestroyCompanion();
		if (!PreventRespawnOnFloorLoad)
		{
			CreateCompanion(obj);
		}
	}

	public void DeactivateSynergy()
	{
		if (m_cachedPlayer != null)
		{
			PlayerController cachedPlayer = m_cachedPlayer;
			cachedPlayer.OnNewFloorLoaded = (Action<PlayerController>)Delegate.Remove(cachedPlayer.OnNewFloorLoaded, new Action<PlayerController>(HandleNewFloor));
			m_cachedPlayer = null;
		}
		DestroyCompanion();
	}
}
