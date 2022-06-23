using UnityEngine;

namespace Dungeonator
{
	public struct CellVisualData
	{
		public enum CellFloorType
		{
			Stone,
			Water,
			Carpet,
			Ice,
			Grass,
			Bone,
			Flesh,
			ThickGoop
		}

		public int roomVisualTypeIndex;

		public bool isDecal;

		public bool isPattern;

		public bool IsChannel;

		public bool IsPhantomCarpet;

		public CellFloorType floorType;

		public bool absorbsDebris;

		public bool facewallGridPreventsWallSpaceStamp;

		public bool containsWallSpaceStamp;

		public bool containsObjectSpaceStamp;

		public DungeonTileStampData.IntermediaryMatchingStyle forcedMatchingStyle;

		public bool precludeAllTileDrawing;

		public bool shouldIgnoreWallDrawing;

		public bool shouldIgnoreBorders;

		public bool hasAlreadyBeenTilemapped;

		public bool hasBeenLit;

		public bool floorTileOverridden;

		public bool preventFloorStamping;

		public int doorFeetOverrideMode;

		public bool containsLight;

		public GameObject lightObject;

		public LightStampData facewallLightStampData;

		public LightStampData sidewallLightStampData;

		public DungeonData.Direction lightDirection;

		public int distanceToNearestLight;

		public int faceWallOverrideIndex;

		public int pitOverrideIndex;

		public int inheritedOverrideIndex;

		public bool inheritedOverrideIndexIsFloor;

		public bool ceilingHasBeenProcessed;

		public bool occlusionHasBeenProcessed;

		public bool hasStampedPath;

		public int pathTilesetGridIndex;

		public bool IsFacewallForInteriorTransition;

		public int InteriorTransitionIndex;

		public bool IsFeatureCell;

		public bool IsFeatureAdditional;

		public bool UsesCustomIndexOverride01;

		public int CustomIndexOverride01Layer;

		public int CustomIndexOverride01;

		public bool RequiresPitBordering;

		public bool HasTriggeredPitVFX;

		public float PitVFXCooldown;

		public float PitParticleCooldown;

		public int RatChunkBorderIndex;

		public void CopyFrom(CellVisualData other)
		{
			roomVisualTypeIndex = other.roomVisualTypeIndex;
			isDecal = other.isDecal;
			isPattern = other.isPattern;
			IsPhantomCarpet = other.IsPhantomCarpet;
			floorType = other.floorType;
			precludeAllTileDrawing = other.precludeAllTileDrawing;
			shouldIgnoreWallDrawing = other.shouldIgnoreWallDrawing;
			shouldIgnoreBorders = other.shouldIgnoreBorders;
			floorTileOverridden = other.floorTileOverridden;
			preventFloorStamping = other.preventFloorStamping;
			faceWallOverrideIndex = other.faceWallOverrideIndex;
			pitOverrideIndex = other.pitOverrideIndex;
			inheritedOverrideIndex = other.inheritedOverrideIndex;
			inheritedOverrideIndexIsFloor = other.inheritedOverrideIndexIsFloor;
			IsFeatureCell = other.IsFeatureCell;
			IsFeatureAdditional = other.IsFeatureAdditional;
			UsesCustomIndexOverride01 = other.UsesCustomIndexOverride01;
			CustomIndexOverride01Layer = other.CustomIndexOverride01Layer;
			CustomIndexOverride01 = other.CustomIndexOverride01;
			RatChunkBorderIndex = other.RatChunkBorderIndex;
		}
	}
}
