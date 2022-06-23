using System;
using UnityEngine;
using UnityEngine.Serialization;

public class EncounterTrackable : MonoBehaviour
{
	[HideInInspector]
	public string ProxyEncounterGuid;

	[FormerlySerializedAs("EncounterGuid")]
	[Header("Local Settings")]
	[SerializeField]
	public string m_encounterGuid;

	[FormerlySerializedAs("DoNotificationOnEncounter")]
	[SerializeField]
	public bool m_doNotificationOnEncounter = true;

	public bool SuppressInInventory;

	[Header("Database Settings")]
	[SerializeField]
	[FormerlySerializedAs("journalData")]
	public JournalEntry m_journalData;

	[FormerlySerializedAs("IgnoreDifferentiator")]
	[SerializeField]
	public bool m_ignoreDifferentiator;

	[FormerlySerializedAs("prerequisites")]
	[SerializeField]
	public DungeonPrerequisite[] m_prerequisites;

	[SerializeField]
	[FormerlySerializedAs("UsesPurpleNotifications")]
	public bool m_usesPurpleNotifications;

	[NonSerialized]
	public bool m_hasCheckedForPickup;

	[NonSerialized]
	public bool m_hasCheckedForProxy;

	[NonSerialized]
	public EncounterDatabaseEntry m_proxyEncounterTrackable;

	private PickupObject m_pickup;

	public static bool SuppressNextNotification { get; set; }

	public string EncounterGuid
	{
		get
		{
			if (!m_hasCheckedForProxy)
			{
				GetProxy();
			}
			if (m_proxyEncounterTrackable != null)
			{
				return ProxyEncounterGuid;
			}
			return m_encounterGuid;
		}
		set
		{
			if (!m_hasCheckedForProxy)
			{
				GetProxy();
			}
			if (m_proxyEncounterTrackable != null)
			{
				throw new Exception("Trying to change an EncounterTrackable via a proxy!");
			}
			m_encounterGuid = value;
		}
	}

	public string TrueEncounterGuid
	{
		get
		{
			return m_encounterGuid;
		}
		set
		{
			m_encounterGuid = value;
		}
	}

	public bool DoNotificationOnEncounter
	{
		get
		{
			if (!m_hasCheckedForProxy)
			{
				GetProxy();
			}
			if (m_proxyEncounterTrackable != null)
			{
				return m_proxyEncounterTrackable.doNotificationOnEncounter;
			}
			return m_doNotificationOnEncounter;
		}
		set
		{
			if (!m_hasCheckedForProxy)
			{
				GetProxy();
			}
			if (m_proxyEncounterTrackable != null)
			{
				throw new Exception("Trying to change an EncounterTrackable via a proxy!");
			}
			m_doNotificationOnEncounter = value;
		}
	}

	public JournalEntry journalData
	{
		get
		{
			if (!m_hasCheckedForProxy)
			{
				GetProxy();
			}
			if (m_proxyEncounterTrackable != null)
			{
				return m_proxyEncounterTrackable.journalData;
			}
			return m_journalData;
		}
		set
		{
			if (!m_hasCheckedForProxy)
			{
				GetProxy();
			}
			if (m_proxyEncounterTrackable != null)
			{
				throw new Exception("Trying to change an EncounterTrackable via a proxy!");
			}
			m_journalData = value;
		}
	}

	public bool IgnoreDifferentiator
	{
		get
		{
			if (!m_hasCheckedForProxy)
			{
				GetProxy();
			}
			if (m_proxyEncounterTrackable != null)
			{
				return m_proxyEncounterTrackable.IgnoreDifferentiator;
			}
			return m_ignoreDifferentiator;
		}
		set
		{
			if (!m_hasCheckedForProxy)
			{
				GetProxy();
			}
			if (m_proxyEncounterTrackable != null)
			{
				throw new Exception("Trying to change an EncounterTrackable via a proxy!");
			}
			m_ignoreDifferentiator = value;
		}
	}

	public DungeonPrerequisite[] prerequisites
	{
		get
		{
			if (!m_hasCheckedForProxy)
			{
				GetProxy();
			}
			if (m_proxyEncounterTrackable != null)
			{
				return m_proxyEncounterTrackable.prerequisites;
			}
			return m_prerequisites;
		}
		set
		{
			if (!m_hasCheckedForProxy)
			{
				GetProxy();
			}
			if (m_proxyEncounterTrackable != null)
			{
				throw new Exception("Trying to change an EncounterTrackable via a proxy!");
			}
			m_prerequisites = value;
		}
	}

	public bool UsesPurpleNotifications
	{
		get
		{
			if (!m_hasCheckedForProxy)
			{
				GetProxy();
			}
			if (m_proxyEncounterTrackable != null)
			{
				return m_proxyEncounterTrackable.usesPurpleNotifications;
			}
			return m_usesPurpleNotifications;
		}
		set
		{
			if (!m_hasCheckedForProxy)
			{
				GetProxy();
			}
			if (m_proxyEncounterTrackable != null)
			{
				throw new Exception("Trying to change an EncounterTrackable via a proxy!");
			}
			m_usesPurpleNotifications = value;
		}
	}

	private void GetProxy()
	{
		if (!string.IsNullOrEmpty(ProxyEncounterGuid))
		{
			m_proxyEncounterTrackable = EncounterDatabase.GetEntry(ProxyEncounterGuid);
		}
		m_hasCheckedForProxy = true;
	}

	public void Awake()
	{
		m_pickup = GetComponent<PickupObject>();
		m_hasCheckedForPickup = true;
	}

	public bool PrerequisitesMet()
	{
		if (m_prerequisites == null || m_prerequisites.Length == 0)
		{
			return true;
		}
		if (GameStatsManager.Instance.IsForceUnlocked(EncounterGuid))
		{
			return true;
		}
		for (int i = 0; i < m_prerequisites.Length; i++)
		{
			if (!m_prerequisites[i].CheckConditionsFulfilled())
			{
				return false;
			}
		}
		if (!m_hasCheckedForPickup)
		{
			m_pickup = GetComponent<PickupObject>();
			m_hasCheckedForPickup = true;
		}
		if ((bool)m_pickup && m_pickup.quality == PickupObject.ItemQuality.EXCLUDED)
		{
			return false;
		}
		return true;
	}

	public void HandleEncounter()
	{
		GameStatsManager.Instance.HandleEncounteredObject(this);
		if (m_doNotificationOnEncounter && !SuppressNextNotification && !GameUIRoot.Instance.BossHealthBarVisible)
		{
			GameUIRoot.Instance.DoNotification(this);
		}
		SuppressNextNotification = false;
	}

	public void ForceDoNotification(tk2dBaseSprite overrideSprite = null)
	{
		tk2dBaseSprite tk2dBaseSprite2 = ((!(overrideSprite == null)) ? overrideSprite : GetComponent<tk2dBaseSprite>());
		GameUIRoot.Instance.notificationController.DoCustomNotification(m_journalData.GetPrimaryDisplayName(), m_journalData.GetNotificationPanelDescription(), tk2dBaseSprite2.Collection, tk2dBaseSprite2.spriteId);
	}

	public void HandleEncounter_GeneratedObjects()
	{
		GameStatsManager.Instance.HandleEncounteredObject(this);
	}

	public string GetModifiedDisplayName()
	{
		if (!m_hasCheckedForPickup)
		{
			m_pickup = GetComponent<PickupObject>();
			m_hasCheckedForPickup = true;
		}
		string text = m_journalData.GetPrimaryDisplayName();
		if (m_pickup != null)
		{
			if (m_pickup.GetComponent<CursedItemModifier>() != null)
			{
				text = StringTableManager.GetItemsString("#CURSED_NAMEMOD") + " " + text;
			}
			if (m_pickup is Gun)
			{
				if ((m_pickup as Gun).IsMinusOneGun)
				{
					text += " -1";
				}
				GunderfuryController component = m_pickup.GetComponent<GunderfuryController>();
				if ((bool)component)
				{
					text = text + " Lv" + IntToStringSansGarbage.GetStringForInt(GunderfuryController.GetCurrentLevel());
				}
			}
		}
		return text;
	}
}
