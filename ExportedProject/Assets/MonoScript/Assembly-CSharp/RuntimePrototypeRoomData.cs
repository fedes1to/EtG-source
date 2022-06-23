using System.Collections.Generic;
using UnityEngine;

public class RuntimePrototypeRoomData
{
	public int RoomId;

	public string GUID;

	public IntVector2 rewardChestSpawnPosition;

	public GameObject associatedMinimapIcon;

	public List<PrototypePlacedObjectData> placedObjects;

	public List<Vector2> placedObjectPositions;

	public List<PrototypeRoomObjectLayer> additionalObjectLayers;

	public List<SerializedPath> paths;

	public List<RoomEventDefinition> roomEvents;

	public bool usesCustomAmbient;

	public Color customAmbient;

	public bool usesDifferentCustomAmbientLowQuality;

	public Color customAmbientLowQuality;

	public bool UsesCustomMusicState;

	public DungeonFloorMusicController.DungeonMusicState CustomMusicState;

	public bool UsesCustomMusic;

	public string CustomMusicEvent;

	public bool UsesCustomSwitch;

	public string CustomMusicSwitch;

	public RuntimePrototypeRoomData(PrototypeDungeonRoom source)
	{
		RoomId = source.RoomId;
		associatedMinimapIcon = source.associatedMinimapIcon;
		placedObjects = source.placedObjects;
		placedObjectPositions = source.placedObjectPositions;
		additionalObjectLayers = source.runtimeAdditionalObjectLayers ?? source.additionalObjectLayers;
		paths = source.paths;
		roomEvents = source.roomEvents;
		rewardChestSpawnPosition = source.rewardChestSpawnPosition;
		usesCustomAmbient = source.usesCustomAmbientLight;
		customAmbient = source.customAmbientLight;
		if (usesCustomAmbient)
		{
			usesDifferentCustomAmbientLowQuality = usesCustomAmbient;
			customAmbientLowQuality = new Color(customAmbient.r + 0.35f, customAmbient.g + 0.35f, customAmbient.b + 0.35f);
		}
		else
		{
			usesDifferentCustomAmbientLowQuality = false;
		}
		UsesCustomMusic = source.UseCustomMusic;
		CustomMusicEvent = source.CustomMusicEvent;
		UsesCustomMusicState = source.UseCustomMusicState;
		CustomMusicState = source.OverrideMusicState;
		UsesCustomSwitch = source.UseCustomMusicSwitch;
		CustomMusicSwitch = source.CustomMusicSwitch;
		GUID = source.GUID;
	}

	public bool DoesUnsealOnClear()
	{
		for (int i = 0; i < roomEvents.Count; i++)
		{
			if (roomEvents[i].condition == RoomEventTriggerCondition.ON_ENEMIES_CLEARED && roomEvents[i].action == RoomEventTriggerAction.UNSEAL_ROOM)
			{
				return true;
			}
		}
		return false;
	}
}
