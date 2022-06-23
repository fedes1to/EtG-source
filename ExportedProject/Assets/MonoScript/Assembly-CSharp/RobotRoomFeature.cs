using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public abstract class RobotRoomFeature
{
	public IntVector2 LocalBasePosition;

	public IntVector2 LocalDimensions;

	public virtual void Use()
	{
	}

	protected SerializedPath GenerateVerticalPath(IntVector2 BasePosition, IntVector2 Dimensions)
	{
		SerializedPath serializedPath = new SerializedPath(BasePosition + new IntVector2(0, Dimensions.y - 1));
		serializedPath.AddPosition(BasePosition);
		serializedPath.wrapMode = SerializedPath.SerializedPathWrapMode.PingPong;
		SerializedPathNode value = serializedPath.nodes[0];
		value.placement = SerializedPathNode.SerializedNodePlacement.SouthWest;
		serializedPath.nodes[0] = value;
		value = serializedPath.nodes[1];
		value.placement = SerializedPathNode.SerializedNodePlacement.SouthWest;
		serializedPath.nodes[1] = value;
		return serializedPath;
	}

	protected SerializedPath GenerateHorizontalPath(IntVector2 BasePosition, IntVector2 Dimensions)
	{
		SerializedPath serializedPath = new SerializedPath(BasePosition + new IntVector2(Dimensions.x - 1, 0));
		serializedPath.AddPosition(BasePosition);
		serializedPath.wrapMode = SerializedPath.SerializedPathWrapMode.PingPong;
		SerializedPathNode value = serializedPath.nodes[0];
		value.placement = SerializedPathNode.SerializedNodePlacement.SouthWest;
		serializedPath.nodes[0] = value;
		value = serializedPath.nodes[1];
		value.placement = SerializedPathNode.SerializedNodePlacement.SouthWest;
		serializedPath.nodes[1] = value;
		return serializedPath;
	}

	protected SerializedPath GenerateRectanglePath(IntVector2 BasePosition, IntVector2 Dimensions)
	{
		SerializedPath serializedPath = new SerializedPath(BasePosition);
		serializedPath.AddPosition(BasePosition + Dimensions.WithY(0));
		serializedPath.AddPosition(BasePosition + Dimensions);
		serializedPath.AddPosition(BasePosition + Dimensions.WithX(0));
		serializedPath.wrapMode = SerializedPath.SerializedPathWrapMode.Loop;
		SerializedPathNode value = serializedPath.nodes[0];
		value.placement = SerializedPathNode.SerializedNodePlacement.SouthWest;
		serializedPath.nodes[0] = value;
		value = serializedPath.nodes[1];
		value.placement = SerializedPathNode.SerializedNodePlacement.SouthWest;
		serializedPath.nodes[1] = value;
		value = serializedPath.nodes[2];
		value.placement = SerializedPathNode.SerializedNodePlacement.SouthWest;
		serializedPath.nodes[2] = value;
		value = serializedPath.nodes[3];
		value.placement = SerializedPathNode.SerializedNodePlacement.SouthWest;
		serializedPath.nodes[3] = value;
		return serializedPath;
	}

	protected SerializedPath GenerateRectanglePathInset(IntVector2 BasePosition, IntVector2 Dimensions)
	{
		BasePosition += new IntVector2(-1, 0);
		Dimensions += IntVector2.One;
		SerializedPath serializedPath = new SerializedPath(BasePosition);
		serializedPath.AddPosition(BasePosition + Dimensions.WithY(0));
		serializedPath.AddPosition(BasePosition + Dimensions);
		serializedPath.AddPosition(BasePosition + Dimensions.WithX(0));
		serializedPath.wrapMode = SerializedPath.SerializedPathWrapMode.Loop;
		SerializedPathNode value = serializedPath.nodes[0];
		value.placement = SerializedPathNode.SerializedNodePlacement.NorthEast;
		serializedPath.nodes[0] = value;
		value = serializedPath.nodes[1];
		value.placement = SerializedPathNode.SerializedNodePlacement.NorthWest;
		serializedPath.nodes[1] = value;
		value = serializedPath.nodes[2];
		value.placement = SerializedPathNode.SerializedNodePlacement.SouthWest;
		serializedPath.nodes[2] = value;
		value = serializedPath.nodes[3];
		value.placement = SerializedPathNode.SerializedNodePlacement.SouthEast;
		serializedPath.nodes[3] = value;
		return serializedPath;
	}

	public abstract bool AcceptableInIdea(RobotDaveIdea idea, IntVector2 dim, bool isInternal, int numFeatures);

	public virtual bool CanContainOtherFeature()
	{
		return false;
	}

	public virtual int RequiredInsetForOtherFeature()
	{
		return 0;
	}

	protected PrototypePlacedObjectData PlaceObject(DungeonPlaceable item, PrototypeDungeonRoom room, IntVector2 position, int targetObjectLayer)
	{
		if (room.CheckRegionOccupied(position.x, position.y, item.GetWidth(), item.GetHeight()))
		{
			return null;
		}
		Vector2 vector = position.ToVector2();
		PrototypePlacedObjectData prototypePlacedObjectData = new PrototypePlacedObjectData();
		prototypePlacedObjectData.fieldData = new List<PrototypePlacedObjectFieldData>();
		prototypePlacedObjectData.instancePrerequisites = new DungeonPrerequisite[0];
		prototypePlacedObjectData.placeableContents = item;
		prototypePlacedObjectData.contentsBasePosition = vector;
		int count = room.placedObjects.Count;
		room.placedObjects.Add(prototypePlacedObjectData);
		room.placedObjectPositions.Add(vector);
		for (int i = 0; i < item.GetWidth(); i++)
		{
			for (int j = 0; j < item.GetHeight(); j++)
			{
				PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = room.ForceGetCellDataAtPoint(position.x + i, position.y + j);
				prototypeDungeonRoomCellData.placedObjectRUBELIndex = count;
			}
		}
		return prototypePlacedObjectData;
	}

	protected PrototypePlacedObjectData PlaceObject(DungeonPlaceableBehaviour item, PrototypeDungeonRoom room, IntVector2 position, int targetObjectLayer)
	{
		if (room.CheckRegionOccupied(position.x, position.y, item.GetWidth(), item.GetHeight()))
		{
			return null;
		}
		Vector2 vector = position.ToVector2();
		PrototypePlacedObjectData prototypePlacedObjectData = new PrototypePlacedObjectData();
		prototypePlacedObjectData.fieldData = new List<PrototypePlacedObjectFieldData>();
		prototypePlacedObjectData.instancePrerequisites = new DungeonPrerequisite[0];
		prototypePlacedObjectData.nonenemyBehaviour = item;
		prototypePlacedObjectData.contentsBasePosition = vector;
		if (targetObjectLayer == -1)
		{
			int count = room.placedObjects.Count;
			room.placedObjects.Add(prototypePlacedObjectData);
			room.placedObjectPositions.Add(vector);
			for (int i = 0; i < item.GetWidth(); i++)
			{
				for (int j = 0; j < item.GetHeight(); j++)
				{
					PrototypeDungeonRoomCellData prototypeDungeonRoomCellData = room.ForceGetCellDataAtPoint(position.x + i, position.y + j);
					prototypeDungeonRoomCellData.placedObjectRUBELIndex = count;
				}
			}
		}
		else
		{
			PrototypeRoomObjectLayer prototypeRoomObjectLayer = room.additionalObjectLayers[targetObjectLayer];
			int count2 = prototypeRoomObjectLayer.placedObjects.Count;
			prototypeRoomObjectLayer.placedObjects.Add(prototypePlacedObjectData);
			prototypeRoomObjectLayer.placedObjectBasePositions.Add(vector);
			for (int k = 0; k < item.GetWidth(); k++)
			{
				for (int l = 0; l < item.GetHeight(); l++)
				{
					PrototypeDungeonRoomCellData prototypeDungeonRoomCellData2 = room.ForceGetCellDataAtPoint(position.x + k, position.y + l);
					prototypeDungeonRoomCellData2.additionalPlacedObjectIndices[targetObjectLayer] = count2;
				}
			}
		}
		return prototypePlacedObjectData;
	}

	public abstract void Develop(PrototypeDungeonRoom room, RobotDaveIdea idea, int targetObjectLayer);
}
