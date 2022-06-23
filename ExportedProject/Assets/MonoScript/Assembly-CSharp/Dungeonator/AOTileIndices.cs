using System;

namespace Dungeonator
{
	[Serializable]
	public class AOTileIndices
	{
		public int AOFloorTileIndex;

		public int AOBottomWallBaseTileIndex = 1;

		public int AOBottomWallTileRightIndex = 2;

		public int AOBottomWallTileLeftIndex = 3;

		public int AOBottomWallTileBothIndex = 4;

		public int AOTopFacewallRightIndex = -1;

		public int AOTopFacewallLeftIndex = -1;

		public int AOTopFacewallBothIndex = -1;

		public int AOFloorWallLeft = 5;

		public int AOFloorWallRight = 6;

		public int AOFloorWallBoth = 7;

		public int AOFloorPizzaSliceLeft = 8;

		public int AOFloorPizzaSliceRight = 9;

		public int AOFloorPizzaSliceBoth = 10;

		public int AOFloorPizzaSliceLeftWallRight = 11;

		public int AOFloorPizzaSliceRightWallLeft = 12;

		public int AOFloorWallUpAndLeft = 13;

		public int AOFloorWallUpAndRight = 14;

		public int AOFloorWallUpAndBoth = 15;

		public int AOFloorDiagonalWallNortheast = -1;

		public int AOFloorDiagonalWallNortheastLower = -1;

		public int AOFloorDiagonalWallNortheastLowerJoint = -1;

		public int AOFloorDiagonalWallNorthwest = -1;

		public int AOFloorDiagonalWallNorthwestLower = -1;

		public int AOFloorDiagonalWallNorthwestLowerJoint = -1;

		public int AOBottomWallDiagonalNortheast = -1;

		public int AOBottomWallDiagonalNorthwest = -1;
	}
}
