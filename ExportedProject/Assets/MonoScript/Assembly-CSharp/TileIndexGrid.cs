using System;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

[Serializable]
public class TileIndexGrid : ScriptableObject
{
	public enum RatChunkResult
	{
		NONE,
		NORMAL,
		BOTTOM,
		CORNER
	}

	public int roomTypeRestriction = -1;

	[TileIndexList]
	public TileIndexList topLeftIndices;

	[TileIndexList]
	public TileIndexList topIndices;

	[TileIndexList]
	public TileIndexList topRightIndices;

	[TileIndexList]
	public TileIndexList leftIndices;

	[TileIndexList]
	public TileIndexList centerIndices;

	[TileIndexList]
	public TileIndexList rightIndices;

	[TileIndexList]
	public TileIndexList bottomLeftIndices;

	[TileIndexList]
	public TileIndexList bottomIndices;

	[TileIndexList]
	public TileIndexList bottomRightIndices;

	[TileIndexList]
	public TileIndexList horizontalIndices;

	[TileIndexList]
	public TileIndexList verticalIndices;

	[TileIndexList]
	public TileIndexList topCapIndices;

	[TileIndexList]
	public TileIndexList rightCapIndices;

	[TileIndexList]
	public TileIndexList bottomCapIndices;

	[TileIndexList]
	public TileIndexList leftCapIndices;

	[TileIndexList]
	public TileIndexList allSidesIndices;

	[TileIndexList]
	public TileIndexList topLeftNubIndices;

	[TileIndexList]
	public TileIndexList topRightNubIndices;

	[TileIndexList]
	public TileIndexList bottomLeftNubIndices;

	[TileIndexList]
	public TileIndexList bottomRightNubIndices;

	public bool extendedSet;

	[TileIndexList]
	[Header("Extended Set")]
	public TileIndexList topCenterLeftIndices;

	[TileIndexList]
	public TileIndexList topCenterIndices;

	[TileIndexList]
	public TileIndexList topCenterRightIndices;

	[TileIndexList]
	public TileIndexList thirdTopRowLeftIndices;

	[TileIndexList]
	public TileIndexList thirdTopRowCenterIndices;

	[TileIndexList]
	public TileIndexList thirdTopRowRightIndices;

	[TileIndexList]
	public TileIndexList internalBottomLeftCenterIndices;

	[TileIndexList]
	public TileIndexList internalBottomCenterIndices;

	[TileIndexList]
	public TileIndexList internalBottomRightCenterIndices;

	[Header("Additional Borders")]
	[TileIndexList]
	public TileIndexList borderTopNubLeftIndices;

	[TileIndexList]
	public TileIndexList borderTopNubRightIndices;

	[TileIndexList]
	public TileIndexList borderTopNubBothIndices;

	[TileIndexList]
	public TileIndexList borderRightNubTopIndices;

	[TileIndexList]
	public TileIndexList borderRightNubBottomIndices;

	[TileIndexList]
	public TileIndexList borderRightNubBothIndices;

	[TileIndexList]
	public TileIndexList borderBottomNubLeftIndices;

	[TileIndexList]
	public TileIndexList borderBottomNubRightIndices;

	[TileIndexList]
	public TileIndexList borderBottomNubBothIndices;

	[TileIndexList]
	public TileIndexList borderLeftNubTopIndices;

	[TileIndexList]
	public TileIndexList borderLeftNubBottomIndices;

	[TileIndexList]
	public TileIndexList borderLeftNubBothIndices;

	[TileIndexList]
	public TileIndexList diagonalNubsTopLeftBottomRight;

	[TileIndexList]
	public TileIndexList diagonalNubsTopRightBottomLeft;

	[TileIndexList]
	public TileIndexList doubleNubsTop;

	[TileIndexList]
	public TileIndexList doubleNubsRight;

	[TileIndexList]
	public TileIndexList doubleNubsBottom;

	[TileIndexList]
	public TileIndexList doubleNubsLeft;

	[TileIndexList]
	public TileIndexList quadNubs;

	[TileIndexList]
	public TileIndexList topRightWithNub;

	[TileIndexList]
	public TileIndexList topLeftWithNub;

	[TileIndexList]
	public TileIndexList bottomRightWithNub;

	[TileIndexList]
	public TileIndexList bottomLeftWithNub;

	[Header("Diagonals--For Borders Only")]
	[TileIndexList]
	public TileIndexList diagonalBorderNE;

	[TileIndexList]
	public TileIndexList diagonalBorderSE;

	[TileIndexList]
	public TileIndexList diagonalBorderSW;

	[TileIndexList]
	public TileIndexList diagonalBorderNW;

	[TileIndexList]
	public TileIndexList diagonalCeilingNE;

	[TileIndexList]
	public TileIndexList diagonalCeilingSE;

	[TileIndexList]
	public TileIndexList diagonalCeilingSW;

	[TileIndexList]
	public TileIndexList diagonalCeilingNW;

	[Header("Carpet Options")]
	public bool CenterCheckerboard;

	public int CheckerboardDimension = 1;

	public bool CenterIndicesAreStrata;

	[Header("Weirdo Options")]
	[Space(5f)]
	public List<TileIndexGrid> PitInternalSquareGrids;

	[Space(5f)]
	public PitSquarePlacementOptions PitInternalSquareOptions;

	[Space(5f)]
	public bool PitBorderIsInternal;

	[Space(5f)]
	public bool PitBorderOverridesFloorTile;

	[Space(5f)]
	public bool CeilingBorderUsesDistancedCenters;

	[Header("For Rat Chunk Borders")]
	[Space(5f)]
	public bool UsesRatChunkBorders;

	[TileIndexList]
	public TileIndexList RatChunkNormalSet;

	[TileIndexList]
	public TileIndexList RatChunkBottomSet;

	[Header("Path Options")]
	[Space(5f)]
	public GameObject PathFacewallStamp;

	public GameObject PathSidewallStamp;

	[Space(5f)]
	[TileIndexList]
	public TileIndexList PathPitPosts;

	[TileIndexList]
	public TileIndexList PathPitPostsBL;

	[TileIndexList]
	public TileIndexList PathPitPostsBR;

	[Space(5f)]
	public GameObject PathStubNorth;

	public GameObject PathStubEast;

	public GameObject PathStubSouth;

	public GameObject PathStubWest;

	protected virtual int ProcessBenubbedTiles(bool isNorthBorder, bool isNortheastBorder, bool isEastBorder, bool isSoutheastBorder, bool isSouthBorder, bool isSouthwestBorder, bool isWestBorder, bool isNorthwestBorder)
	{
		if (!isNorthBorder && !isEastBorder && !isWestBorder && !isSouthBorder)
		{
			if (isNortheastBorder && isNorthwestBorder && isSoutheastBorder && isSouthwestBorder && quadNubs.ContainsValid())
			{
				return quadNubs.GetIndexByWeight();
			}
			if (isNortheastBorder)
			{
				if (isNorthwestBorder && doubleNubsTop.ContainsValid())
				{
					return doubleNubsTop.GetIndexByWeight();
				}
				if (isSoutheastBorder && doubleNubsRight.ContainsValid())
				{
					return doubleNubsRight.GetIndexByWeight();
				}
				if (isSouthwestBorder && diagonalNubsTopRightBottomLeft.ContainsValid())
				{
					return diagonalNubsTopRightBottomLeft.GetIndexByWeight();
				}
				if (topRightNubIndices.ContainsValid())
				{
					return topRightNubIndices.GetIndexByWeight();
				}
			}
			if (isNorthwestBorder)
			{
				if (isSouthwestBorder && doubleNubsLeft.ContainsValid())
				{
					return doubleNubsLeft.GetIndexByWeight();
				}
				if (isSoutheastBorder && diagonalNubsTopLeftBottomRight.ContainsValid())
				{
					return diagonalNubsTopLeftBottomRight.GetIndexByWeight();
				}
				if (topLeftNubIndices.ContainsValid())
				{
					return topLeftNubIndices.GetIndexByWeight();
				}
			}
			if (isSoutheastBorder)
			{
				if (isSouthwestBorder && doubleNubsBottom.ContainsValid())
				{
					return doubleNubsBottom.GetIndexByWeight();
				}
				if (bottomRightNubIndices.ContainsValid())
				{
					return bottomRightNubIndices.GetIndexByWeight();
				}
			}
			if (isSouthwestBorder && bottomLeftNubIndices.ContainsValid())
			{
				return bottomLeftNubIndices.GetIndexByWeight();
			}
		}
		if (isNorthBorder && !isEastBorder && !isSouthBorder && !isWestBorder)
		{
			if (isSoutheastBorder && isSouthwestBorder && borderTopNubBothIndices.ContainsValid())
			{
				return borderTopNubBothIndices.GetIndexByWeight();
			}
			if (isSoutheastBorder && borderTopNubRightIndices.ContainsValid())
			{
				return borderTopNubRightIndices.GetIndexByWeight();
			}
			if (isSouthwestBorder && borderTopNubLeftIndices.ContainsValid())
			{
				return borderTopNubLeftIndices.GetIndexByWeight();
			}
		}
		if (!isNorthBorder && isEastBorder && !isSouthBorder && !isWestBorder)
		{
			if (isNorthwestBorder && isSouthwestBorder && borderRightNubBothIndices.ContainsValid())
			{
				return borderRightNubBothIndices.GetIndexByWeight();
			}
			if (isSouthwestBorder && borderRightNubBottomIndices.ContainsValid())
			{
				return borderRightNubBottomIndices.GetIndexByWeight();
			}
			if (isNorthwestBorder && borderRightNubTopIndices.ContainsValid())
			{
				return borderRightNubTopIndices.GetIndexByWeight();
			}
		}
		if (!isNorthBorder && !isEastBorder && isSouthBorder && !isWestBorder)
		{
			if (isNortheastBorder && isNorthwestBorder && borderBottomNubBothIndices.ContainsValid())
			{
				return borderBottomNubBothIndices.GetIndexByWeight();
			}
			if (isNorthwestBorder && borderBottomNubLeftIndices.ContainsValid())
			{
				return borderBottomNubLeftIndices.GetIndexByWeight();
			}
			if (isNortheastBorder && borderBottomNubRightIndices.ContainsValid())
			{
				return borderBottomNubRightIndices.GetIndexByWeight();
			}
		}
		if (!isNorthBorder && !isEastBorder && !isSouthBorder && isWestBorder)
		{
			if (isNortheastBorder && isSoutheastBorder && borderLeftNubBothIndices.ContainsValid())
			{
				return borderLeftNubBothIndices.GetIndexByWeight();
			}
			if (isNortheastBorder && borderLeftNubTopIndices.ContainsValid())
			{
				return borderLeftNubTopIndices.GetIndexByWeight();
			}
			if (isSoutheastBorder && borderLeftNubBottomIndices.ContainsValid())
			{
				return borderLeftNubBottomIndices.GetIndexByWeight();
			}
		}
		if (isNorthBorder && isEastBorder && !isSouthBorder && !isWestBorder && isSouthwestBorder)
		{
			return topRightWithNub.GetIndexByWeight();
		}
		if (!isNorthBorder && isEastBorder && isSouthBorder && !isWestBorder && isNorthwestBorder)
		{
			return bottomRightWithNub.GetIndexByWeight();
		}
		if (!isNorthBorder && !isEastBorder && isSouthBorder && isWestBorder && isNortheastBorder)
		{
			return bottomLeftWithNub.GetIndexByWeight();
		}
		if (isNorthBorder && !isEastBorder && !isSouthBorder && isWestBorder && isSoutheastBorder)
		{
			return topLeftWithNub.GetIndexByWeight();
		}
		return -1;
	}

	public virtual int GetIndexGivenEightSides(bool[] eightSides)
	{
		return GetIndexGivenSides(eightSides[0], eightSides[1], eightSides[2], eightSides[3], eightSides[4], eightSides[5], eightSides[6], eightSides[7]);
	}

	public virtual int GetStaticIndexGivenSides(bool isNorthBorder, bool isNortheastBorder, bool isEastBorder, bool isSoutheastBorder, bool isSouthBorder, bool isSouthwestBorder, bool isWestBorder, bool isNorthwestBorder)
	{
		UnityEngine.Random.InitState(147);
		int num = ProcessBenubbedTiles(isNorthBorder, isNortheastBorder, isEastBorder, isSoutheastBorder, isSouthBorder, isSouthwestBorder, isWestBorder, isNorthwestBorder);
		if (num != -1)
		{
			return num;
		}
		return GetIndexGivenSides(isNorthBorder, isEastBorder, isSouthBorder, isWestBorder);
	}

	public virtual int GetIndexGivenSides(bool isNorthBorder, bool isNortheastBorder, bool isEastBorder, bool isSoutheastBorder, bool isSouthBorder, bool isSouthwestBorder, bool isWestBorder, bool isNorthwestBorder)
	{
		int num = ProcessBenubbedTiles(isNorthBorder, isNortheastBorder, isEastBorder, isSoutheastBorder, isSouthBorder, isSouthwestBorder, isWestBorder, isNorthwestBorder);
		if (num != -1)
		{
			return num;
		}
		return GetIndexGivenSides(isNorthBorder, isEastBorder, isSouthBorder, isWestBorder);
	}

	public virtual int GetIndexGivenSides(List<CellData> cells, Func<CellData, bool> evalFunc)
	{
		return GetIndexGivenSides(evalFunc(cells[0]), evalFunc(cells[1]), evalFunc(cells[2]), evalFunc(cells[3]), evalFunc(cells[4]), evalFunc(cells[5]), evalFunc(cells[6]), evalFunc(cells[7]));
	}

	public virtual int GetRatChunkIndexGivenSidesStatic(bool isNorthBorder, bool isNortheastBorder, bool isEastBorder, bool isSoutheastBorder, bool isSouthBorder, bool isSouthwestBorder, bool isWestBorder, bool isNorthwestBorder, bool isTwoSouthEmpty, out RatChunkResult result)
	{
		result = RatChunkResult.NONE;
		if (isNorthBorder || isEastBorder || isSouthBorder || isWestBorder)
		{
			if ((isNorthBorder && isTwoSouthEmpty) || (isEastBorder && isTwoSouthEmpty) || (isWestBorder && isTwoSouthEmpty))
			{
				result = RatChunkResult.BOTTOM;
				return RatChunkBottomSet.indices[0];
			}
			result = RatChunkResult.NORMAL;
			return RatChunkNormalSet.indices[0];
		}
		if (isNortheastBorder || isNorthwestBorder || isSoutheastBorder || isSouthwestBorder)
		{
			result = RatChunkResult.CORNER;
			return bottomRightNubIndices.indices[0];
		}
		return centerIndices.indices[0];
	}

	public virtual int GetRatChunkIndexGivenSides(bool isNorthBorder, bool isNortheastBorder, bool isEastBorder, bool isSoutheastBorder, bool isSouthBorder, bool isSouthwestBorder, bool isWestBorder, bool isNorthwestBorder, bool isTwoSouthEmpty, out RatChunkResult result)
	{
		result = RatChunkResult.NONE;
		if (isNorthBorder || isEastBorder || isSouthBorder || isWestBorder)
		{
			if ((isNorthBorder && isTwoSouthEmpty) || (isEastBorder && isTwoSouthEmpty) || (isWestBorder && isTwoSouthEmpty))
			{
				result = RatChunkResult.BOTTOM;
				return RatChunkBottomSet.GetIndexByWeight();
			}
			result = RatChunkResult.NORMAL;
			return RatChunkNormalSet.GetIndexByWeight();
		}
		if (isNortheastBorder || isNorthwestBorder || isSoutheastBorder || isSouthwestBorder)
		{
			result = RatChunkResult.CORNER;
			return bottomRightNubIndices.GetIndexByWeight();
		}
		return centerIndices.GetIndexByWeight();
	}

	public virtual int GetIndexGivenSides(bool isNorthBorder, bool isEastBorder, bool isSouthBorder, bool isWestBorder)
	{
		if (isNorthBorder && isEastBorder && isSouthBorder && isWestBorder)
		{
			return (!allSidesIndices.ContainsValid()) ? centerIndices.GetIndexByWeight() : allSidesIndices.GetIndexByWeight();
		}
		if (isNorthBorder && isEastBorder && isWestBorder)
		{
			return (!topCapIndices.ContainsValid()) ? centerIndices.GetIndexByWeight() : topCapIndices.GetIndexByWeight();
		}
		if (isEastBorder && isNorthBorder && isSouthBorder)
		{
			return (!rightCapIndices.ContainsValid()) ? centerIndices.GetIndexByWeight() : rightCapIndices.GetIndexByWeight();
		}
		if (isSouthBorder && isEastBorder && isWestBorder)
		{
			return (!bottomCapIndices.ContainsValid()) ? centerIndices.GetIndexByWeight() : bottomCapIndices.GetIndexByWeight();
		}
		if (isWestBorder && isSouthBorder && isNorthBorder)
		{
			return (!leftCapIndices.ContainsValid()) ? centerIndices.GetIndexByWeight() : leftCapIndices.GetIndexByWeight();
		}
		if (isNorthBorder && isEastBorder)
		{
			return (!topRightIndices.ContainsValid()) ? centerIndices.GetIndexByWeight() : topRightIndices.GetIndexByWeight();
		}
		if (isEastBorder && isSouthBorder)
		{
			return (!bottomRightIndices.ContainsValid()) ? centerIndices.GetIndexByWeight() : bottomRightIndices.GetIndexByWeight();
		}
		if (isSouthBorder && isWestBorder)
		{
			return (!bottomLeftIndices.ContainsValid()) ? centerIndices.GetIndexByWeight() : bottomLeftIndices.GetIndexByWeight();
		}
		if (isWestBorder && isNorthBorder)
		{
			return (!topLeftIndices.ContainsValid()) ? centerIndices.GetIndexByWeight() : topLeftIndices.GetIndexByWeight();
		}
		if (isNorthBorder && isSouthBorder)
		{
			return (!horizontalIndices.ContainsValid()) ? centerIndices.GetIndexByWeight() : horizontalIndices.GetIndexByWeight();
		}
		if (isEastBorder && isWestBorder)
		{
			return (!verticalIndices.ContainsValid()) ? centerIndices.GetIndexByWeight() : verticalIndices.GetIndexByWeight();
		}
		if (isNorthBorder)
		{
			return (!topIndices.ContainsValid()) ? centerIndices.GetIndexByWeight() : topIndices.GetIndexByWeight();
		}
		if (isEastBorder)
		{
			return (!rightIndices.ContainsValid()) ? centerIndices.GetIndexByWeight() : rightIndices.GetIndexByWeight();
		}
		if (isSouthBorder)
		{
			return (!bottomIndices.ContainsValid()) ? centerIndices.GetIndexByWeight() : bottomIndices.GetIndexByWeight();
		}
		if (isWestBorder)
		{
			return (!leftIndices.ContainsValid()) ? centerIndices.GetIndexByWeight() : leftIndices.GetIndexByWeight();
		}
		return centerIndices.GetIndexByWeight();
	}

	public virtual int GetIndexGivenSides(bool isNorthBorder, bool isSecondNorthBorder, bool isNortheastBorder, bool isEastBorder, bool isSoutheastBorder, bool isSouthBorder, bool isSouthwestBorder, bool isWestBorder, bool isNorthwestBorder)
	{
		if (!isNorthBorder && isSecondNorthBorder && extendedSet)
		{
			if (isEastBorder && topCenterRightIndices.ContainsValid())
			{
				return topCenterRightIndices.GetIndexByWeight();
			}
			if (isWestBorder && topCenterLeftIndices.ContainsValid())
			{
				return topCenterLeftIndices.GetIndexByWeight();
			}
			if (topCenterIndices.ContainsValid())
			{
				return topCenterIndices.GetIndexByWeight();
			}
		}
		return GetIndexGivenSides(isNorthBorder, isNortheastBorder, isEastBorder, isSoutheastBorder, isSouthBorder, isSouthwestBorder, isWestBorder, isNorthwestBorder);
	}

	public virtual int GetIndexGivenSides(bool isNorthBorder, bool isSecondNorthBorder, bool isThirdNorthBorder, bool isNortheastBorder, bool isEastBorder, bool isSoutheastBorder, bool isSouthBorder, bool isSouthwestBorder, bool isWestBorder, bool isNorthwestBorder)
	{
		if (!isNorthBorder && !isSecondNorthBorder && isThirdNorthBorder && extendedSet)
		{
			if (isEastBorder && thirdTopRowRightIndices.ContainsValid())
			{
				return thirdTopRowRightIndices.GetIndexByWeight();
			}
			if (isWestBorder && thirdTopRowLeftIndices.ContainsValid())
			{
				return thirdTopRowLeftIndices.GetIndexByWeight();
			}
			if (thirdTopRowCenterIndices.ContainsValid())
			{
				return thirdTopRowCenterIndices.GetIndexByWeight();
			}
		}
		return GetIndexGivenSides(isNorthBorder, isSecondNorthBorder, isNortheastBorder, isEastBorder, isSoutheastBorder, isSouthBorder, isSouthwestBorder, isWestBorder, isNorthwestBorder);
	}

	public virtual int GetIndexGivenSides(bool isNorthBorder, bool isSecondNorthBorder, bool isEastBorder, bool isSouthBorder, bool isWestBorder)
	{
		if (!isNorthBorder && isSecondNorthBorder && extendedSet)
		{
			if (isEastBorder && topCenterRightIndices.ContainsValid())
			{
				return topCenterRightIndices.GetIndexByWeight();
			}
			if (isWestBorder && topCenterLeftIndices.ContainsValid())
			{
				return topCenterLeftIndices.GetIndexByWeight();
			}
			if (topCenterIndices.ContainsValid())
			{
				return topCenterIndices.GetIndexByWeight();
			}
		}
		return GetIndexGivenSides(isNorthBorder, isEastBorder, isSouthBorder, isWestBorder);
	}

	public virtual int GetIndexGivenSides(bool isNorthBorder, bool isSecondNorthBorder, bool isThirdNorthBorder, bool isEastBorder, bool isSouthBorder, bool isWestBorder)
	{
		if (!isNorthBorder && !isSecondNorthBorder && isThirdNorthBorder && extendedSet)
		{
			if (isEastBorder && thirdTopRowRightIndices.ContainsValid())
			{
				return thirdTopRowRightIndices.GetIndexByWeight();
			}
			if (isWestBorder && thirdTopRowLeftIndices.ContainsValid())
			{
				return thirdTopRowLeftIndices.GetIndexByWeight();
			}
			if (thirdTopRowCenterIndices.ContainsValid())
			{
				return thirdTopRowCenterIndices.GetIndexByWeight();
			}
		}
		return GetIndexGivenSides(isNorthBorder, isSecondNorthBorder, isEastBorder, isSouthBorder, isWestBorder);
	}

	public virtual int GetInternalIndexGivenSides(bool isNorthBorder, bool isEastBorder, bool isSouthBorder, bool isWestBorder)
	{
		if (extendedSet)
		{
			if (!isSouthBorder)
			{
				if (!isEastBorder)
				{
					return internalBottomRightCenterIndices.GetIndexByWeight();
				}
				if (!isWestBorder)
				{
					return internalBottomLeftCenterIndices.GetIndexByWeight();
				}
				return internalBottomCenterIndices.GetIndexByWeight();
			}
			return -1;
		}
		return -1;
	}
}
