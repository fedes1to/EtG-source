using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class PrototypePlacedObjectData
{
	public DungeonPlaceable placeableContents;

	[FormerlySerializedAs("behaviourContents")]
	public DungeonPlaceableBehaviour nonenemyBehaviour;

	public string enemyBehaviourGuid;

	public GameObject unspecifiedContents;

	public Vector2 contentsBasePosition;

	public int layer;

	public float spawnChance = 1f;

	public int xMPxOffset;

	public int yMPxOffset;

	public List<PrototypePlacedObjectFieldData> fieldData;

	public DungeonPrerequisite[] instancePrerequisites;

	public List<int> linkedTriggerAreaIDs;

	public int assignedPathIDx = -1;

	public int assignedPathStartNode;

	public bool IsEnemyForReinforcementLayerCheck()
	{
		if (placeableContents != null && !placeableContents.ContainsEnemy && !placeableContents.ContainsEnemylikeObjectForReinforcement)
		{
			return false;
		}
		if (nonenemyBehaviour != null)
		{
			return false;
		}
		return true;
	}

	public bool GetBoolFieldValueByName(string name)
	{
		if (fieldData == null)
		{
			return false;
		}
		for (int i = 0; i < fieldData.Count; i++)
		{
			if (fieldData[i].fieldName == name)
			{
				return fieldData[i].boolValue;
			}
		}
		return false;
	}

	public float GetFieldValueByName(string name)
	{
		for (int i = 0; i < fieldData.Count; i++)
		{
			if (fieldData[i].fieldName == name)
			{
				return fieldData[i].floatValue;
			}
		}
		return 1f;
	}

	public int GetWidth(bool accountForFieldData = false)
	{
		if (accountForFieldData && fieldData != null)
		{
			for (int i = 0; i < fieldData.Count; i++)
			{
				if (fieldData[i].fieldName == "DwarfConfigurableWidth")
				{
					return Mathf.RoundToInt(GetFieldValueByName("DwarfConfigurableWidth"));
				}
			}
		}
		if (placeableContents != null)
		{
			return placeableContents.width;
		}
		if (nonenemyBehaviour != null)
		{
			return nonenemyBehaviour.placeableWidth;
		}
		if (!string.IsNullOrEmpty(enemyBehaviourGuid))
		{
			return EnemyDatabase.GetEntry(enemyBehaviourGuid).placeableWidth;
		}
		return 1;
	}

	public int GetHeight(bool accountForFieldData = false)
	{
		if (accountForFieldData && fieldData != null)
		{
			for (int i = 0; i < fieldData.Count; i++)
			{
				if (fieldData[i].fieldName == "DwarfConfigurableHeight")
				{
					return Mathf.RoundToInt(GetFieldValueByName("DwarfConfigurableHeight"));
				}
			}
		}
		if (placeableContents != null)
		{
			return placeableContents.height;
		}
		if (nonenemyBehaviour != null)
		{
			return nonenemyBehaviour.placeableHeight;
		}
		if (!string.IsNullOrEmpty(enemyBehaviourGuid))
		{
			return EnemyDatabase.GetEntry(enemyBehaviourGuid).placeableHeight;
		}
		return 1;
	}

	public PrototypePlacedObjectData CreateMirror(IntVector2 roomDimensions)
	{
		PrototypePlacedObjectData prototypePlacedObjectData = new PrototypePlacedObjectData();
		prototypePlacedObjectData.placeableContents = placeableContents;
		prototypePlacedObjectData.nonenemyBehaviour = nonenemyBehaviour;
		prototypePlacedObjectData.enemyBehaviourGuid = enemyBehaviourGuid;
		prototypePlacedObjectData.unspecifiedContents = unspecifiedContents;
		prototypePlacedObjectData.contentsBasePosition = contentsBasePosition;
		prototypePlacedObjectData.contentsBasePosition.x = (float)roomDimensions.x - (prototypePlacedObjectData.contentsBasePosition.x + (float)GetWidth(true));
		prototypePlacedObjectData.layer = layer;
		prototypePlacedObjectData.spawnChance = spawnChance;
		prototypePlacedObjectData.xMPxOffset = xMPxOffset;
		prototypePlacedObjectData.yMPxOffset = yMPxOffset;
		prototypePlacedObjectData.fieldData = new List<PrototypePlacedObjectFieldData>();
		for (int i = 0; i < fieldData.Count; i++)
		{
			PrototypePlacedObjectFieldData prototypePlacedObjectFieldData = new PrototypePlacedObjectFieldData();
			prototypePlacedObjectFieldData.boolValue = fieldData[i].boolValue;
			prototypePlacedObjectFieldData.fieldName = fieldData[i].fieldName;
			prototypePlacedObjectFieldData.fieldType = fieldData[i].fieldType;
			prototypePlacedObjectFieldData.floatValue = fieldData[i].floatValue;
			prototypePlacedObjectData.fieldData.Add(prototypePlacedObjectFieldData);
		}
		prototypePlacedObjectData.instancePrerequisites = new DungeonPrerequisite[instancePrerequisites.Length];
		for (int j = 0; j < instancePrerequisites.Length; j++)
		{
			prototypePlacedObjectData.instancePrerequisites[j] = instancePrerequisites[j];
		}
		prototypePlacedObjectData.assignedPathIDx = assignedPathIDx;
		prototypePlacedObjectData.assignedPathStartNode = assignedPathStartNode;
		prototypePlacedObjectData.linkedTriggerAreaIDs = new List<int>();
		for (int k = 0; k < linkedTriggerAreaIDs.Count; k++)
		{
			prototypePlacedObjectData.linkedTriggerAreaIDs.Add(linkedTriggerAreaIDs[k]);
		}
		return prototypePlacedObjectData;
	}
}
