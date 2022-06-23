using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TilesetIndexMetadata
{
	[Flags]
	public enum TilesetFlagType
	{
		FACEWALL_UPPER = 1,
		FACEWALL_LOWER = 2,
		FLOOR_TILE = 4,
		CHEST_HIGH_WALL = 8,
		DECAL_TILE = 0x10,
		PATTERN_TILE = 0x20,
		DOOR_FEET_NS = 0x40,
		DOOR_FEET_EW = 0x80,
		DIAGONAL_FACEWALL_UPPER_NE = 0x100,
		DIAGONAL_FACEWALL_UPPER_NW = 0x200,
		DIAGONAL_FACEWALL_LOWER_NE = 0x400,
		DIAGONAL_FACEWALL_LOWER_NW = 0x800,
		DIAGONAL_FACEWALL_TOP_NE = 0x1000,
		DIAGONAL_FACEWALL_TOP_NW = 0x2000,
		FACEWALL_LOWER_LEFTCORNER = 0x4000,
		FACEWALL_LOWER_RIGHTCORNER = 0x8000,
		FACEWALL_UPPER_LEFTCORNER = 0x10000,
		FACEWALL_UPPER_RIGHTCORNER = 0x20000,
		FACEWALL_LOWER_LEFTEDGE = 0x40000,
		FACEWALL_LOWER_RIGHTEDGE = 0x80000,
		FACEWALL_UPPER_LEFTEDGE = 0x100000,
		FACEWALL_UPPER_RIGHTEDGE = 0x200000
	}

	public enum VFXPlaystyle
	{
		CONTINUOUS,
		TIMED_REPEAT,
		ON_ANIMATION_FRAME
	}

	public TilesetFlagType type;

	public float weight = 0.1f;

	public int dungeonRoomSubType;

	public int secondRoomSubType = -1;

	public int thirdRoomSubType = -1;

	public bool preventWallStamping;

	public bool usesAnimSequence;

	public bool usesNeighborDependencies;

	public bool usesPerTileVFX;

	public VFXPlaystyle tileVFXPlaystyle;

	public float tileVFXChance = 1f;

	public GameObject tileVFXPrefab;

	public Vector2 tileVFXOffset;

	public float tileVFXDelayTime;

	public float tileVFXDelayVariance;

	public int tileVFXAnimFrame;

	public void CopyFrom(TilesetIndexMetadata src)
	{
		type = src.type;
		weight = src.weight;
		dungeonRoomSubType = src.dungeonRoomSubType;
		secondRoomSubType = src.secondRoomSubType;
		thirdRoomSubType = src.thirdRoomSubType;
		usesAnimSequence = src.usesAnimSequence;
		usesNeighborDependencies = src.usesNeighborDependencies;
		preventWallStamping = src.preventWallStamping;
		usesPerTileVFX = src.usesPerTileVFX;
		tileVFXPlaystyle = src.tileVFXPlaystyle;
		tileVFXChance = src.tileVFXChance;
		tileVFXPrefab = src.tileVFXPrefab;
		tileVFXOffset = src.tileVFXOffset;
		tileVFXDelayTime = src.tileVFXDelayTime;
		tileVFXDelayVariance = src.tileVFXDelayVariance;
		tileVFXAnimFrame = src.tileVFXAnimFrame;
	}

	public SimpleTilesetAnimationSequence GetAnimSequence(tk2dSpriteCollectionData collection, int spriteId)
	{
		return collection.GetAnimationSequence(spriteId);
	}

	public List<IndexNeighborDependency> GetNeighborDependencies(tk2dSpriteCollectionData collection, int spriteId)
	{
		return collection.GetDependencies(spriteId);
	}
}
