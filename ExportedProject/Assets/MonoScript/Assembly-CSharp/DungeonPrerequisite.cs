using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DungeonPrerequisite
{
	public enum PrerequisiteType
	{
		ENCOUNTER,
		COMPARISON,
		CHARACTER,
		TILESET,
		FLAG,
		DEMO_MODE,
		MAXIMUM_COMPARISON,
		ENCOUNTER_OR_FLAG,
		NUMBER_PASTS_COMPLETED
	}

	public enum PrerequisiteOperation
	{
		LESS_THAN,
		EQUAL_TO,
		GREATER_THAN
	}

	public PrerequisiteType prerequisiteType;

	[ShowInInspectorIf("UsesOperation", false)]
	public PrerequisiteOperation prerequisiteOperation;

	[ShowInInspectorIf("IsStatComparison", false)]
	public TrackedStats statToCheck;

	[ShowInInspectorIf("IsMaxComparison", false)]
	public TrackedMaximums maxToCheck;

	[ShowInInspectorIf("IsComparison", false)]
	public float comparisonValue;

	[ShowInInspectorIf("IsComparison", false)]
	public bool useSessionStatValue;

	[ShowInInspectorIf("IsEncounter", false)]
	[EncounterIdentifier]
	public string encounteredObjectGuid;

	[ShowInInspectorIf("IsEncounter", false)]
	public PrototypeDungeonRoom encounteredRoom;

	[ShowInInspectorIf("IsEncounter", false)]
	public int requiredNumberOfEncounters = 1;

	[ShowInInspectorIf("IsCharacter", false)]
	public PlayableCharacters requiredCharacter;

	[ShowInInspectorIf("IsCharacter", false)]
	public bool requireCharacter = true;

	[ShowInInspectorIf("IsTileset", false)]
	public GlobalDungeonData.ValidTilesets requiredTileset;

	[ShowInInspectorIf("IsTileset", false)]
	public bool requireTileset = true;

	[LongEnum]
	public GungeonFlags saveFlagToCheck;

	[ShowInInspectorIf("IsFlag", false)]
	public bool requireFlag = true;

	[ShowInInspectorIf("IsDemoMode", false)]
	public bool requireDemoMode;

	private bool UsesOperation()
	{
		return prerequisiteType == PrerequisiteType.COMPARISON || prerequisiteType == PrerequisiteType.MAXIMUM_COMPARISON || prerequisiteType == PrerequisiteType.NUMBER_PASTS_COMPLETED || prerequisiteType == PrerequisiteType.ENCOUNTER || prerequisiteType == PrerequisiteType.ENCOUNTER;
	}

	private bool IsComparison()
	{
		return prerequisiteType == PrerequisiteType.COMPARISON || prerequisiteType == PrerequisiteType.MAXIMUM_COMPARISON || prerequisiteType == PrerequisiteType.NUMBER_PASTS_COMPLETED;
	}

	private bool IsStatComparison()
	{
		return prerequisiteType == PrerequisiteType.COMPARISON;
	}

	private bool IsMaxComparison()
	{
		return prerequisiteType == PrerequisiteType.MAXIMUM_COMPARISON;
	}

	private bool IsEncounter()
	{
		return prerequisiteType == PrerequisiteType.ENCOUNTER || prerequisiteType == PrerequisiteType.ENCOUNTER_OR_FLAG;
	}

	private bool IsCharacter()
	{
		return prerequisiteType == PrerequisiteType.CHARACTER;
	}

	private bool IsTileset()
	{
		return prerequisiteType == PrerequisiteType.TILESET;
	}

	private bool IsFlag()
	{
		return prerequisiteType == PrerequisiteType.FLAG || prerequisiteType == PrerequisiteType.ENCOUNTER_OR_FLAG;
	}

	private bool IsDemoMode()
	{
		return prerequisiteType == PrerequisiteType.DEMO_MODE;
	}

	public bool CheckConditionsFulfilled()
	{
		EncounterDatabaseEntry encounterDatabaseEntry = null;
		if (!string.IsNullOrEmpty(encounteredObjectGuid))
		{
			encounterDatabaseEntry = EncounterDatabase.GetEntry(encounteredObjectGuid);
		}
		switch (prerequisiteType)
		{
		case PrerequisiteType.ENCOUNTER:
			if (encounterDatabaseEntry == null && encounteredRoom == null)
			{
				return true;
			}
			if (encounterDatabaseEntry != null)
			{
				int num3 = GameStatsManager.Instance.QueryEncounterable(encounterDatabaseEntry);
				switch (prerequisiteOperation)
				{
				case PrerequisiteOperation.LESS_THAN:
					return num3 < requiredNumberOfEncounters;
				case PrerequisiteOperation.EQUAL_TO:
					return num3 == requiredNumberOfEncounters;
				case PrerequisiteOperation.GREATER_THAN:
					return num3 > requiredNumberOfEncounters;
				}
				Debug.LogError("Switching on invalid stat comparison operation!");
			}
			else if (encounteredRoom != null)
			{
				int num4 = GameStatsManager.Instance.QueryRoomEncountered(encounteredRoom.GUID);
				switch (prerequisiteOperation)
				{
				case PrerequisiteOperation.LESS_THAN:
					return num4 < requiredNumberOfEncounters;
				case PrerequisiteOperation.EQUAL_TO:
					return num4 == requiredNumberOfEncounters;
				case PrerequisiteOperation.GREATER_THAN:
					return num4 > requiredNumberOfEncounters;
				}
				Debug.LogError("Switching on invalid stat comparison operation!");
			}
			return false;
		case PrerequisiteType.ENCOUNTER_OR_FLAG:
			if (GameStatsManager.Instance.GetFlag(saveFlagToCheck) == requireFlag)
			{
				return true;
			}
			if (encounterDatabaseEntry != null)
			{
				int num = GameStatsManager.Instance.QueryEncounterable(encounterDatabaseEntry);
				switch (prerequisiteOperation)
				{
				case PrerequisiteOperation.LESS_THAN:
					return num < requiredNumberOfEncounters;
				case PrerequisiteOperation.EQUAL_TO:
					return num == requiredNumberOfEncounters;
				case PrerequisiteOperation.GREATER_THAN:
					return num > requiredNumberOfEncounters;
				}
				Debug.LogError("Switching on invalid stat comparison operation!");
			}
			else if (encounteredRoom != null)
			{
				int num2 = GameStatsManager.Instance.QueryRoomEncountered(encounteredRoom.GUID);
				switch (prerequisiteOperation)
				{
				case PrerequisiteOperation.LESS_THAN:
					return num2 < requiredNumberOfEncounters;
				case PrerequisiteOperation.EQUAL_TO:
					return num2 == requiredNumberOfEncounters;
				case PrerequisiteOperation.GREATER_THAN:
					return num2 > requiredNumberOfEncounters;
				}
				Debug.LogError("Switching on invalid stat comparison operation!");
			}
			return false;
		case PrerequisiteType.COMPARISON:
		{
			float playerStatValue = GameStatsManager.Instance.GetPlayerStatValue(statToCheck);
			switch (prerequisiteOperation)
			{
			case PrerequisiteOperation.LESS_THAN:
				return playerStatValue < comparisonValue;
			case PrerequisiteOperation.EQUAL_TO:
				return playerStatValue == comparisonValue;
			case PrerequisiteOperation.GREATER_THAN:
				return playerStatValue > comparisonValue;
			}
			Debug.LogError("Switching on invalid stat comparison operation!");
			break;
		}
		case PrerequisiteType.MAXIMUM_COMPARISON:
		{
			float playerMaximum = GameStatsManager.Instance.GetPlayerMaximum(maxToCheck);
			switch (prerequisiteOperation)
			{
			case PrerequisiteOperation.LESS_THAN:
				return playerMaximum < comparisonValue;
			case PrerequisiteOperation.EQUAL_TO:
				return playerMaximum == comparisonValue;
			case PrerequisiteOperation.GREATER_THAN:
				return playerMaximum > comparisonValue;
			}
			Debug.LogError("Switching on invalid stat comparison operation!");
			break;
		}
		case PrerequisiteType.CHARACTER:
		{
			PlayableCharacters playableCharacters = (PlayableCharacters)(-1);
			if (!BraveRandom.IgnoreGenerationDifferentiator)
			{
				if (GameManager.Instance.PrimaryPlayer != null)
				{
					playableCharacters = GameManager.Instance.PrimaryPlayer.characterIdentity;
				}
				else if (GameManager.PlayerPrefabForNewGame != null)
				{
					playableCharacters = GameManager.PlayerPrefabForNewGame.GetComponent<PlayerController>().characterIdentity;
				}
				else if (GameManager.Instance.BestGenerationDungeonPrefab != null)
				{
					playableCharacters = GameManager.Instance.BestGenerationDungeonPrefab.defaultPlayerPrefab.GetComponent<PlayerController>().characterIdentity;
				}
			}
			return requireCharacter == (playableCharacters == requiredCharacter);
		}
		case PrerequisiteType.TILESET:
			if (GameManager.Instance.BestGenerationDungeonPrefab != null)
			{
				return requireTileset == (GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId == requiredTileset);
			}
			return requireTileset == (GameManager.Instance.Dungeon.tileIndices.tilesetId == requiredTileset);
		case PrerequisiteType.FLAG:
			return GameStatsManager.Instance.GetFlag(saveFlagToCheck) == requireFlag;
		case PrerequisiteType.DEMO_MODE:
			return !requireDemoMode;
		case PrerequisiteType.NUMBER_PASTS_COMPLETED:
			return (float)GameStatsManager.Instance.GetNumberPastsBeaten() >= comparisonValue;
		default:
			Debug.LogError("Switching on invalid prerequisite type!!!");
			break;
		}
		return false;
	}

	protected bool Equals(DungeonPrerequisite other)
	{
		return prerequisiteType == other.prerequisiteType && prerequisiteOperation == other.prerequisiteOperation && statToCheck == other.statToCheck && maxToCheck == other.maxToCheck && comparisonValue.Equals(other.comparisonValue) && useSessionStatValue.Equals(other.useSessionStatValue) && object.Equals(encounteredRoom, other.encounteredRoom) && object.Equals(encounteredObjectGuid, other.encounteredObjectGuid) && requiredNumberOfEncounters == other.requiredNumberOfEncounters && requiredCharacter == other.requiredCharacter && requireCharacter.Equals(other.requireCharacter) && requiredTileset == other.requiredTileset && requireTileset.Equals(other.requireTileset) && saveFlagToCheck == other.saveFlagToCheck && requireFlag.Equals(other.requireFlag) && requireDemoMode.Equals(other.requireDemoMode);
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
		return Equals((DungeonPrerequisite)obj);
	}

	public override int GetHashCode()
	{
		int num = (int)prerequisiteType;
		num = (num * 397) ^ (int)prerequisiteOperation;
		num = (num * 397) ^ (int)statToCheck;
		num = (num * 397) ^ comparisonValue.GetHashCode();
		num = (num * 397) ^ useSessionStatValue.GetHashCode();
		num = (num * 397) ^ ((encounteredRoom != null) ? encounteredRoom.GetHashCode() : 0);
		num = (num * 397) ^ ((encounteredObjectGuid != null) ? encounteredObjectGuid.GetHashCode() : 0);
		num = (num * 397) ^ requiredNumberOfEncounters;
		num = (num * 397) ^ (int)requiredCharacter;
		num = (num * 397) ^ requireCharacter.GetHashCode();
		num = (num * 397) ^ (int)requiredTileset;
		num = (num * 397) ^ requireTileset.GetHashCode();
		num = (num * 397) ^ (int)saveFlagToCheck;
		num = (num * 397) ^ requireFlag.GetHashCode();
		return (num * 397) ^ requireDemoMode.GetHashCode();
	}

	public static bool CheckConditionsFulfilled(DungeonPrerequisite[] prereqs)
	{
		if (prereqs == null)
		{
			return true;
		}
		for (int i = 0; i < prereqs.Length; i++)
		{
			if (prereqs[i] != null && !prereqs[i].CheckConditionsFulfilled())
			{
				return false;
			}
		}
		return true;
	}

	public static bool CheckConditionsFulfilled(List<DungeonPrerequisite> prereqs)
	{
		if (prereqs == null)
		{
			return true;
		}
		for (int i = 0; i < prereqs.Count; i++)
		{
			if (prereqs[i] != null && !prereqs[i].CheckConditionsFulfilled())
			{
				return false;
			}
		}
		return true;
	}
}
