using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[Serializable]
public class SerializedPath
{
	public enum SerializedPathWrapMode
	{
		PingPong,
		Loop,
		Once
	}

	public List<SerializedPathNode> nodes;

	public SerializedPathWrapMode wrapMode;

	public float overrideSpeed = -1f;

	public int tilesetPathGrid = -1;

	public SerializedPath(IntVector2 cellPosition)
	{
		nodes = new List<SerializedPathNode>();
		nodes.Add(new SerializedPathNode(cellPosition));
	}

	public SerializedPath(SerializedPath prototypePath, IntVector2 basePositionAdjustment)
	{
		nodes = new List<SerializedPathNode>();
		for (int i = 0; i < prototypePath.nodes.Count; i++)
		{
			nodes.Add(new SerializedPathNode(prototypePath.nodes[i], basePositionAdjustment));
		}
		wrapMode = prototypePath.wrapMode;
	}

	public static SerializedPath CreateMirror(SerializedPath source, IntVector2 roomDimensions, PrototypeDungeonRoom sourceRoom)
	{
		SerializedPath serializedPath = new SerializedPath(IntVector2.Zero);
		serializedPath.nodes.Clear();
		for (int i = 0; i < source.nodes.Count; i++)
		{
			serializedPath.nodes.Add(SerializedPathNode.CreateMirror(source.nodes[i], roomDimensions));
		}
		serializedPath.wrapMode = source.wrapMode;
		serializedPath.overrideSpeed = source.overrideSpeed;
		serializedPath.tilesetPathGrid = source.tilesetPathGrid;
		int num = sourceRoom.paths.IndexOf(source);
		int num2 = 0;
		for (int j = 0; j < sourceRoom.placedObjects.Count; j++)
		{
			if (num >= 0 && sourceRoom.placedObjects[j].assignedPathIDx == num)
			{
				num2 = Mathf.Max(num2, sourceRoom.placedObjects[j].GetWidth(true));
			}
		}
		for (int k = 0; k < sourceRoom.additionalObjectLayers.Count; k++)
		{
			for (int l = 0; l < sourceRoom.additionalObjectLayers[k].placedObjects.Count; l++)
			{
				if (num >= 0 && sourceRoom.additionalObjectLayers[k].placedObjects[l].assignedPathIDx == num)
				{
					num2 = Mathf.Max(num2, sourceRoom.additionalObjectLayers[k].placedObjects[l].GetWidth(true));
				}
			}
		}
		if (num2 > 0)
		{
			for (int m = 0; m < serializedPath.nodes.Count; m++)
			{
				SerializedPathNode value = serializedPath.nodes[m];
				value.position += new IntVector2(-1, 0) * (num2 - 1);
				serializedPath.nodes[m] = value;
			}
		}
		return serializedPath;
	}

	public void StampPathToTilemap(RoomHandler parentRoom)
	{
		if (tilesetPathGrid < 0 || tilesetPathGrid >= GameManager.Instance.Dungeon.pathGridDefinitions.Count)
		{
			return;
		}
		DungeonData data = GameManager.Instance.Dungeon.data;
		for (int i = 1; i < nodes.Count + 1; i++)
		{
			SerializedPathNode serializedPathNode;
			SerializedPathNode serializedPathNode2;
			if (i == nodes.Count)
			{
				if (wrapMode != SerializedPathWrapMode.Loop)
				{
					break;
				}
				serializedPathNode = nodes[i - 1];
				serializedPathNode2 = nodes[0];
			}
			else
			{
				serializedPathNode = nodes[i - 1];
				serializedPathNode2 = nodes[i];
			}
			if (serializedPathNode.position.x != serializedPathNode2.position.x && serializedPathNode.position.y != serializedPathNode2.position.y)
			{
				Debug.LogError("Attempting to stamp a path grid to the tilemap and the path contains diagonals! This cannot be.");
				break;
			}
			IntVector2 intVector = parentRoom.area.basePosition + serializedPathNode.position;
			IntVector2 intVector2 = parentRoom.area.basePosition + serializedPathNode2.position;
			if (serializedPathNode.position.x == serializedPathNode2.position.x)
			{
				TileIndexGrid tileIndexGrid = GameManager.Instance.Dungeon.pathGridDefinitions[tilesetPathGrid];
				if (tileIndexGrid.PathFacewallStamp != null)
				{
					for (int j = Mathf.Min(intVector.y, intVector2.y); j < Mathf.Max(intVector.y, intVector2.y); j++)
					{
						if (data[intVector.x, j].type != CellType.WALL && data[intVector.x, j + 1].type == CellType.WALL)
						{
							GameObject gameObject = UnityEngine.Object.Instantiate(tileIndexGrid.PathFacewallStamp, new Vector3(intVector.x, j + 1, 0f) + tileIndexGrid.PathFacewallStamp.transform.position, Quaternion.identity);
							gameObject.GetComponent<PlacedWallDecorator>().ConfigureOnPlacement(data.GetAbsoluteRoomFromPosition(gameObject.transform.position.IntXY()));
						}
					}
				}
			}
			else
			{
				TileIndexGrid tileIndexGrid2 = GameManager.Instance.Dungeon.pathGridDefinitions[tilesetPathGrid];
				if (tileIndexGrid2.PathSidewallStamp != null)
				{
					for (int k = Mathf.Min(intVector.x, intVector2.x); k < Mathf.Max(intVector.x, intVector2.x); k++)
					{
						if (data[k, intVector.y].type == CellType.FLOOR && data[k + 1, intVector.y].type == CellType.WALL)
						{
							GameObject gameObject2 = UnityEngine.Object.Instantiate(tileIndexGrid2.PathSidewallStamp, new Vector3(k + 1, intVector.y, 0f) + tileIndexGrid2.PathSidewallStamp.transform.position, Quaternion.identity);
							gameObject2.GetComponent<tk2dSprite>().FlipX = true;
						}
						else if (data[k, intVector.y].type == CellType.WALL && data[k + 1, intVector.y].type == CellType.FLOOR)
						{
							UnityEngine.Object.Instantiate(tileIndexGrid2.PathSidewallStamp, new Vector3(k + 1, intVector.y, 0f) + tileIndexGrid2.PathSidewallStamp.transform.position, Quaternion.identity);
						}
					}
				}
			}
			if ((i == nodes.Count - 1 || i == 1) && wrapMode != SerializedPathWrapMode.Loop)
			{
				TileIndexGrid tileIndexGrid3 = GameManager.Instance.Dungeon.pathGridDefinitions[tilesetPathGrid];
				if (i == nodes.Count - 1)
				{
					if (intVector.y == intVector2.y && data[intVector2].type != CellType.WALL && data[intVector2 + BraveUtility.GetIntMajorAxis((intVector2 - intVector).ToVector2())].type != CellType.WALL)
					{
						intVector2 += BraveUtility.GetIntMajorAxis((intVector2 - intVector).ToVector2());
					}
				}
				else if (i == 1 && intVector.y == intVector2.y && data[intVector].type != CellType.WALL && data[intVector + BraveUtility.GetIntMajorAxis((intVector - intVector2).ToVector2())].type != CellType.WALL)
				{
					intVector += BraveUtility.GetIntMajorAxis((intVector - intVector2).ToVector2());
				}
				int num = 1;
				if (nodes.Count - 1 == 1)
				{
					num = 2;
				}
				for (int l = 0; l < num; l++)
				{
					IntVector2 key = ((i != 1 || l == 1) ? intVector2 : intVector);
					IntVector2 intVector3 = ((i != 1 || l == 1) ? BraveUtility.GetIntMajorAxis(intVector2 - intVector) : BraveUtility.GetIntMajorAxis(intVector - intVector2));
					if (i == 1 && l != 1)
					{
						key += intVector3;
					}
					if (data[key] == null || data[key].type != CellType.FLOOR)
					{
						continue;
					}
					switch (DungeonData.GetDirectionFromIntVector2(intVector3))
					{
					case DungeonData.Direction.NORTH:
						if (tileIndexGrid3.PathStubNorth != null)
						{
							UnityEngine.Object.Instantiate(tileIndexGrid3.PathStubNorth, key.ToVector3(), Quaternion.identity);
						}
						break;
					case DungeonData.Direction.EAST:
						if (tileIndexGrid3.PathStubEast != null)
						{
							UnityEngine.Object.Instantiate(tileIndexGrid3.PathStubEast, key.ToVector3(), Quaternion.identity);
						}
						break;
					case DungeonData.Direction.SOUTH:
						if (tileIndexGrid3.PathStubSouth != null)
						{
							UnityEngine.Object.Instantiate(tileIndexGrid3.PathStubSouth, key.ToVector3(), Quaternion.identity);
						}
						break;
					case DungeonData.Direction.WEST:
						if (tileIndexGrid3.PathStubWest != null)
						{
							UnityEngine.Object.Instantiate(tileIndexGrid3.PathStubWest, key.ToVector3(), Quaternion.identity);
						}
						break;
					}
				}
			}
			for (IntVector2 majorAxis = (intVector2 - intVector).MajorAxis; intVector != intVector2; intVector += majorAxis)
			{
				data[intVector].cellVisualData.containsObjectSpaceStamp = true;
				data[intVector].cellVisualData.pathTilesetGridIndex = tilesetPathGrid;
				data[intVector].cellVisualData.hasStampedPath = true;
				BraveUtility.DrawDebugSquare(intVector.ToVector2(), Color.magenta, 1000f);
				data[intVector].fallingPrevented = true;
			}
		}
	}

	public void ChangeNodePlacement(IntVector2 position)
	{
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].position == position)
			{
				int placement = (int)(nodes[i].placement + 1) % Enum.GetValues(typeof(SerializedPathNode.SerializedNodePlacement)).Length;
				SerializedPathNode value = nodes[i];
				value.placement = (SerializedPathNode.SerializedNodePlacement)placement;
				nodes[i] = value;
			}
		}
	}

	public void ChangeNodePlacement(IntVector2 position, SerializedPathNode.SerializedNodePlacement placement)
	{
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].position == position)
			{
				SerializedPathNode value = nodes[i];
				value.placement = placement;
				nodes[i] = value;
			}
		}
	}

	public SerializedPathNode? GetNodeAtPoint(IntVector2 position, out int index)
	{
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].position == position)
			{
				index = i;
				return nodes[i];
			}
		}
		index = -1;
		return null;
	}

	public void AddPosition(IntVector2 position)
	{
		nodes.Add(new SerializedPathNode(position));
	}

	public void AddPosition(IntVector2 position, IntVector2 previousPosition)
	{
		bool flag = false;
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].position == previousPosition)
			{
				flag = true;
				SerializedPathNode item = new SerializedPathNode(position);
				item.placement = nodes[i].placement;
				nodes.Insert(i + 1, item);
				break;
			}
		}
		if (!flag)
		{
			AddPosition(position);
		}
	}

	public bool TranslatePosition(IntVector2 position, IntVector2 translation)
	{
		IntVector2 intVector = position + translation;
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].position == position)
			{
				num = i;
			}
			if (nodes[i].position == intVector)
			{
				num2 = i;
			}
		}
		if (num != -1 && num2 == -1)
		{
			SerializedPathNode value = new SerializedPathNode(intVector);
			value.placement = nodes[num].placement;
			value.delayTime = nodes[num].delayTime;
			nodes[num] = value;
			return true;
		}
		return false;
	}

	public void RemovePosition(IntVector2 position)
	{
		for (int i = 0; i < nodes.Count; i++)
		{
			if (nodes[i].position == position)
			{
				nodes.RemoveAt(i);
				i--;
			}
		}
	}
}
