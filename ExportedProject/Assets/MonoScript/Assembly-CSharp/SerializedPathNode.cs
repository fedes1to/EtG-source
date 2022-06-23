using System;
using UnityEngine;

[Serializable]
public struct SerializedPathNode
{
	public enum SerializedNodePlacement
	{
		Center,
		North,
		NorthEast,
		East,
		SouthEast,
		South,
		SouthWest,
		West,
		NorthWest
	}

	public IntVector2 position;

	public float delayTime;

	public SerializedNodePlacement placement;

	public bool UsesAlternateTarget;

	public int AlternateTargetPathIndex;

	public int AlternateTargetNodeIndex;

	public Vector2 RoomPosition
	{
		get
		{
			IntVector2 normalizedVectorFromPlacement = GetNormalizedVectorFromPlacement();
			return position.ToCenterVector2() + new Vector2(0.5f * (float)normalizedVectorFromPlacement.x, 0.5f * (float)normalizedVectorFromPlacement.y);
		}
	}

	public SerializedPathNode(IntVector2 pos)
	{
		position = pos;
		placement = SerializedNodePlacement.SouthWest;
		delayTime = 0f;
		UsesAlternateTarget = false;
		AlternateTargetNodeIndex = -1;
		AlternateTargetPathIndex = -1;
	}

	public SerializedPathNode(SerializedPathNode sourceNode, IntVector2 positionAdjustment)
	{
		position = sourceNode.position + positionAdjustment;
		placement = sourceNode.placement;
		delayTime = sourceNode.delayTime;
		UsesAlternateTarget = sourceNode.UsesAlternateTarget;
		AlternateTargetNodeIndex = sourceNode.AlternateTargetNodeIndex;
		AlternateTargetPathIndex = sourceNode.AlternateTargetPathIndex;
	}

	public static SerializedPathNode CreateMirror(SerializedPathNode source, IntVector2 roomDimensions)
	{
		SerializedPathNode result = default(SerializedPathNode);
		result.position = source.position;
		result.position.x = roomDimensions.x - (result.position.x + 1);
		result.delayTime = source.delayTime;
		result.placement = source.placement;
		result.UsesAlternateTarget = source.UsesAlternateTarget;
		result.AlternateTargetPathIndex = source.AlternateTargetPathIndex;
		result.AlternateTargetNodeIndex = source.AlternateTargetNodeIndex;
		return result;
	}

	public IntVector2 GetNormalizedVectorFromPlacement()
	{
		switch (placement)
		{
		case SerializedNodePlacement.Center:
			return IntVector2.Zero;
		case SerializedNodePlacement.North:
			return IntVector2.North;
		case SerializedNodePlacement.NorthEast:
			return IntVector2.NorthEast;
		case SerializedNodePlacement.East:
			return IntVector2.East;
		case SerializedNodePlacement.SouthEast:
			return IntVector2.SouthEast;
		case SerializedNodePlacement.South:
			return IntVector2.South;
		case SerializedNodePlacement.SouthWest:
			return IntVector2.SouthWest;
		case SerializedNodePlacement.West:
			return IntVector2.West;
		case SerializedNodePlacement.NorthWest:
			return IntVector2.NorthWest;
		default:
			return IntVector2.Zero;
		}
	}
}
