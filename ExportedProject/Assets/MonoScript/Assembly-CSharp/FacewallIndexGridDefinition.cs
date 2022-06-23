using System;
using UnityEngine;

[Serializable]
public class FacewallIndexGridDefinition
{
	public TileIndexGrid grid;

	public int minWidth = 3;

	public int maxWidth = 10;

	[Header("Intermediary Tiles")]
	public bool hasIntermediaries;

	public int minIntermediaryBuffer = 4;

	public int maxIntermediaryBuffer = 20;

	public int minIntermediaryLength = 1;

	public int maxIntermediaryLength = 1;

	[Header("Options")]
	public bool topsMatchBottoms;

	public bool middleSectionSequential;

	public bool canExistInCorners = true;

	public bool forceEdgesInCorners;

	public bool canAcceptWallDecoration;

	public bool canAcceptFloorDecoration = true;

	public DungeonTileStampData.IntermediaryMatchingStyle forcedStampMatchingStyle;

	public bool canBePlacedInExits;

	public float chanceToPlaceIfPossible = 0.1f;

	public float perTileFailureRate = 0.05f;
}
