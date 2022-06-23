using System;
using Dungeonator;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class JournalEntry
{
	public enum CustomJournalEntryType
	{
		NONE = 0,
		RAT_NOTE_01 = 101,
		RAT_NOTE_02 = 102,
		RAT_NOTE_03 = 103,
		RAT_NOTE_04 = 104,
		RAT_NOTE_05 = 105,
		RAT_NOTE_06 = 106,
		RESOURCEFUL_RAT = 107
	}

	public bool SuppressInAmmonomicon;

	public bool SuppressKnownState;

	public bool DisplayOnLoadingScreen;

	public bool RequiresLightBackgroundInLoadingScreen;

	[StringTableString("items")]
	public string PrimaryDisplayName;

	[StringTableString("items")]
	public string NotificationPanelDescription;

	[StringTableString("items")]
	public string AmmonomiconFullEntry;

	[FormerlySerializedAs("AlternateAmmonomiconButtonSpriteName")]
	public string AmmonomiconSprite = string.Empty;

	public bool IsEnemy;

	public Texture2D enemyPortraitSprite;

	public CustomJournalEntryType SpecialIdentifier;

	[NonSerialized]
	private string m_cachedPrimaryDisplayName;

	[NonSerialized]
	private string m_cachedNotificationPanelDescription;

	[NonSerialized]
	private string m_cachedAmmonomiconFullEntry;

	[NonSerialized]
	private int PrivateSemaphoreValue;

	public static int ReloadDataSemaphore { get; set; }

	private void CheckSemaphore()
	{
		if (PrivateSemaphoreValue < ReloadDataSemaphore)
		{
			m_cachedPrimaryDisplayName = null;
			m_cachedNotificationPanelDescription = null;
			m_cachedAmmonomiconFullEntry = null;
			PrivateSemaphoreValue = ReloadDataSemaphore;
		}
	}

	public string GetAmmonomiconFullEntry(bool isInfiniteAmmoGun, bool doesntDamageSecretWalls)
	{
		CheckSemaphore();
		if (string.IsNullOrEmpty(m_cachedAmmonomiconFullEntry))
		{
			if (SpecialIdentifier == CustomJournalEntryType.RESOURCEFUL_RAT)
			{
				string key = "#RESOURCEFULRAT_AGD_LONGDESC_PREKILL";
				if (Application.isPlaying && GameStatsManager.Instance.GetFlag(GungeonFlags.BOSSKILLED_RESOURCEFULRAT))
				{
					key = "#RESOURCEFULRAT_AGD_LONGDESC_POSTKILL";
				}
				string text = HandleLongDescSuffix();
				return StringTableManager.GetEnemiesLongDescription(key) + text;
			}
			if (string.IsNullOrEmpty(AmmonomiconFullEntry))
			{
				return string.Empty;
			}
			if (IsEnemy)
			{
				m_cachedAmmonomiconFullEntry = StringTableManager.GetEnemiesLongDescription(AmmonomiconFullEntry);
			}
			else
			{
				string key2 = AmmonomiconFullEntry;
				if (AmmonomiconFullEntry == "#PIGITEM1_LONGDESC" && Application.isPlaying && GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_HERO_PIG))
				{
					key2 = "#PIGITEM2_LONGDESC";
				}
				if (AmmonomiconFullEntry == "#BRACELETRED_LONGDESC" && Application.isPlaying && GameStatsManager.Instance.GetFlag(GungeonFlags.BLACKSMITH_RECEIVED_RUBY_BRACELET))
				{
					key2 = "#BRACELETRED_LONGDESC_V2";
				}
				string text2 = string.Empty;
				if (isInfiniteAmmoGun)
				{
					text2 = text2 + StringTableManager.GetItemsString("#INFINITEAMMO_TEXT") + " ";
				}
				if (doesntDamageSecretWalls)
				{
					text2 = text2 + StringTableManager.GetItemsString("#NOSECRETS_TEXT") + " ";
				}
				string text3 = HandleLongDescSuffix();
				m_cachedAmmonomiconFullEntry = text2 + StringTableManager.GetItemsLongDescription(key2) + text3;
			}
		}
		return m_cachedAmmonomiconFullEntry;
	}

	private string HandleLongDescSuffix()
	{
		string result = string.Empty;
		if (SpecialIdentifier != 0 && Application.isPlaying)
		{
			DungeonData.Direction[] resourcefulRatSolution = GameManager.GetResourcefulRatSolution();
			if (SpecialIdentifier == CustomJournalEntryType.RAT_NOTE_01 && Application.isPlaying)
			{
				result = GetRatSpriteFromDirection(resourcefulRatSolution[0]);
			}
			else if (SpecialIdentifier == CustomJournalEntryType.RAT_NOTE_02 && Application.isPlaying)
			{
				result = GetRatSpriteFromDirection(resourcefulRatSolution[1]);
			}
			else if (SpecialIdentifier == CustomJournalEntryType.RAT_NOTE_03 && Application.isPlaying)
			{
				result = GetRatSpriteFromDirection(resourcefulRatSolution[2]);
			}
			else if (SpecialIdentifier == CustomJournalEntryType.RAT_NOTE_04 && Application.isPlaying)
			{
				result = GetRatSpriteFromDirection(resourcefulRatSolution[3]);
			}
			else if (SpecialIdentifier == CustomJournalEntryType.RAT_NOTE_05 && Application.isPlaying)
			{
				result = GetRatSpriteFromDirection(resourcefulRatSolution[4]);
			}
			else if (SpecialIdentifier == CustomJournalEntryType.RAT_NOTE_06 && Application.isPlaying)
			{
				result = GetRatSpriteFromDirection(resourcefulRatSolution[5]);
			}
		}
		return result;
	}

	private string GetRatSpriteFromDirection(DungeonData.Direction dir)
	{
		switch (dir)
		{
		case DungeonData.Direction.NORTH:
			return "[sprite \"resourcefulrat_text_note_001\"]";
		case DungeonData.Direction.EAST:
			return "[sprite \"resourcefulrat_text_note_002\"]";
		case DungeonData.Direction.SOUTH:
			return "[sprite \"resourcefulrat_text_note_003\"]";
		case DungeonData.Direction.WEST:
			return "[sprite \"resourcefulrat_text_note_004\"]";
		default:
			return string.Empty;
		}
	}

	public string GetPrimaryDisplayName(bool duringStartup = false)
	{
		CheckSemaphore();
		if (string.IsNullOrEmpty(m_cachedPrimaryDisplayName))
		{
			if (IsEnemy)
			{
				m_cachedPrimaryDisplayName = StringTableManager.GetEnemiesString(PrimaryDisplayName, 0);
			}
			else
			{
				string key = PrimaryDisplayName;
				if (Application.isPlaying && !duringStartup && PrimaryDisplayName == "#PIGITEM1_ENCNAME" && GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_HERO_PIG))
				{
					key = "#PIGITEM2_ENCNAME";
				}
				m_cachedPrimaryDisplayName = StringTableManager.GetItemsString(key, 0);
			}
		}
		return m_cachedPrimaryDisplayName;
	}

	public string GetNotificationPanelDescription()
	{
		CheckSemaphore();
		if (string.IsNullOrEmpty(m_cachedNotificationPanelDescription))
		{
			if (IsEnemy)
			{
				m_cachedNotificationPanelDescription = StringTableManager.GetEnemiesString(NotificationPanelDescription, 0);
			}
			else
			{
				string key = NotificationPanelDescription;
				if (NotificationPanelDescription == "#PIGITEM1_SHORTDESC" && Application.isPlaying && GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_HERO_PIG))
				{
					key = "#PIGITEM2_SHORTDESC";
				}
				if (NotificationPanelDescription == "#BRACELETRED_SHORTDESC" && Application.isPlaying && GameStatsManager.Instance.GetFlag(GungeonFlags.BLACKSMITH_RECEIVED_RUBY_BRACELET))
				{
					key = "#BRACELETRED_SHORTDESC_V2";
				}
				if (NotificationPanelDescription == "#JUNK_SHORTDESC" && SackKnightController.HasJunkan())
				{
					key = "#JUNKSHRINE_SHORTDESC";
				}
				m_cachedNotificationPanelDescription = StringTableManager.GetItemsString(key, 0);
			}
		}
		return m_cachedNotificationPanelDescription;
	}

	public string GetCustomNotificationPanelDescription(int index)
	{
		CheckSemaphore();
		if (IsEnemy)
		{
			return StringTableManager.GetEnemiesString(NotificationPanelDescription, index);
		}
		return StringTableManager.GetItemsString(NotificationPanelDescription, index);
	}

	protected bool Equals(JournalEntry other)
	{
		return SuppressInAmmonomicon.Equals(other.SuppressInAmmonomicon) && DisplayOnLoadingScreen.Equals(other.DisplayOnLoadingScreen) && RequiresLightBackgroundInLoadingScreen.Equals(other.RequiresLightBackgroundInLoadingScreen) && string.Equals(PrimaryDisplayName, other.PrimaryDisplayName) && string.Equals(NotificationPanelDescription, other.NotificationPanelDescription) && string.Equals(AmmonomiconFullEntry, other.AmmonomiconFullEntry) && string.Equals(AmmonomiconSprite, other.AmmonomiconSprite) && IsEnemy.Equals(other.IsEnemy) && SuppressKnownState.Equals(other.SuppressKnownState) && object.Equals(enemyPortraitSprite, other.enemyPortraitSprite);
	}

	public override bool Equals(object obj)
	{
		if (object.ReferenceEquals(null, obj))
		{
			return false;
		}
		if (object.ReferenceEquals(this, obj))
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((JournalEntry)obj);
	}

	public override int GetHashCode()
	{
		int hashCode = SuppressInAmmonomicon.GetHashCode();
		hashCode = (hashCode * 397) ^ DisplayOnLoadingScreen.GetHashCode();
		hashCode = (hashCode * 397) ^ RequiresLightBackgroundInLoadingScreen.GetHashCode();
		hashCode = (hashCode * 397) ^ ((PrimaryDisplayName != null) ? PrimaryDisplayName.GetHashCode() : 0);
		hashCode = (hashCode * 397) ^ ((NotificationPanelDescription != null) ? NotificationPanelDescription.GetHashCode() : 0);
		hashCode = (hashCode * 397) ^ ((AmmonomiconFullEntry != null) ? AmmonomiconFullEntry.GetHashCode() : 0);
		hashCode = (hashCode * 397) ^ ((AmmonomiconSprite != null) ? AmmonomiconSprite.GetHashCode() : 0);
		hashCode = (hashCode * 397) ^ IsEnemy.GetHashCode();
		return (hashCode * 397) ^ ((enemyPortraitSprite != null) ? enemyPortraitSprite.GetHashCode() : 0);
	}

	public JournalEntry Clone()
	{
		JournalEntry journalEntry = new JournalEntry();
		journalEntry.SuppressInAmmonomicon = SuppressInAmmonomicon;
		journalEntry.DisplayOnLoadingScreen = DisplayOnLoadingScreen;
		journalEntry.RequiresLightBackgroundInLoadingScreen = RequiresLightBackgroundInLoadingScreen;
		journalEntry.PrimaryDisplayName = PrimaryDisplayName;
		journalEntry.NotificationPanelDescription = NotificationPanelDescription;
		journalEntry.AmmonomiconFullEntry = AmmonomiconFullEntry;
		journalEntry.AmmonomiconSprite = AmmonomiconSprite;
		journalEntry.IsEnemy = IsEnemy;
		journalEntry.enemyPortraitSprite = enemyPortraitSprite;
		journalEntry.SuppressKnownState = SuppressKnownState;
		journalEntry.SpecialIdentifier = SpecialIdentifier;
		return journalEntry;
	}

	public void ClearCache()
	{
		m_cachedPrimaryDisplayName = null;
		m_cachedNotificationPanelDescription = null;
		m_cachedAmmonomiconFullEntry = null;
	}
}
