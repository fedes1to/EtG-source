using System;
using UnityEngine;

public static class GlobalDungeonData
{
	[Flags]
	public enum ValidTilesets
	{
		GUNGEON = 1,
		CASTLEGEON = 2,
		SEWERGEON = 4,
		CATHEDRALGEON = 8,
		MINEGEON = 0x10,
		CATACOMBGEON = 0x20,
		FORGEGEON = 0x40,
		HELLGEON = 0x80,
		SPACEGEON = 0x100,
		PHOBOSGEON = 0x200,
		WESTGEON = 0x400,
		OFFICEGEON = 0x800,
		BELLYGEON = 0x1000,
		JUNGLEGEON = 0x2000,
		FINALGEON = 0x4000,
		RATGEON = 0x8000
	}

	public static int COMMON_BASE_PRICE = 20;

	public static int D_BASE_PRICE = 35;

	public static int C_BASE_PRICE = 45;

	public static int B_BASE_PRICE = 65;

	public static int A_BASE_PRICE = 90;

	public static int S_BASE_PRICE = 120;

	public static int occlusionPartitionIndex = 0;

	public static int pitLayerIndex = 1;

	public static int floorLayerIndex = 2;

	public static int patternLayerIndex = 3;

	public static int decalLayerIndex = 4;

	public static int actorCollisionLayerIndex = 5;

	public static int collisionLayerIndex = 6;

	public static int wallStampLayerIndex = 7;

	public static int objectStampLayerIndex = 8;

	public static int shadowLayerIndex = 9;

	public static int killLayerIndex = 10;

	public static int ceilingLayerIndex = 11;

	public static int borderLayerIndex = 12;

	public static int aboveBorderLayerIndex = 13;

	public static bool GUNGEON_EXPERIMENTAL = false;

	public static readonly string[] TilesetPaths = new string[16]
	{
		"Assets\\Sprites\\Collections\\ENV_Tileset_Gungeon.prefab",
		"Assets\\Sprites\\Collections\\ENV_Tileset_Castle.prefab",
		"Assets\\Sprites\\Collections\\ENV_Tileset_Sewer.prefab",
		"Assets\\Sprites\\Collections\\ENV_Tileset_Cathedral.prefab",
		"Assets\\Sprites\\Collections\\ENV_Tileset_Mines.prefab",
		"Assets\\Sprites\\Collections\\ENV_Tileset_Catacombs.prefab",
		"Assets\\Sprites\\Collections\\ENV_Tileset_Forge.prefab",
		"Assets\\Sprites\\Collections\\ENV_Tileset_BulletHell.prefab",
		string.Empty,
		string.Empty,
		string.Empty,
		"Assets\\Sprites\\Collections\\ENV_Tileset_Nakatomi.prefab",
		string.Empty,
		string.Empty,
		string.Empty,
		"Assets\\Sprites\\Collections\\Dolphin Tilesets\\ENV_Tileset_Rat.prefab"
	};

	public static int GetBasePrice(PickupObject.ItemQuality quality)
	{
		switch (quality)
		{
		case PickupObject.ItemQuality.COMMON:
			return COMMON_BASE_PRICE;
		case PickupObject.ItemQuality.D:
			return D_BASE_PRICE;
		case PickupObject.ItemQuality.C:
			return C_BASE_PRICE;
		case PickupObject.ItemQuality.B:
			return B_BASE_PRICE;
		case PickupObject.ItemQuality.A:
			return A_BASE_PRICE;
		case PickupObject.ItemQuality.S:
			return S_BASE_PRICE;
		default:
			if (Application.isPlaying)
			{
				Debug.LogError(string.Concat("Invalid quality : ", quality, " in GetBasePrice"));
			}
			return S_BASE_PRICE;
		}
	}
}
