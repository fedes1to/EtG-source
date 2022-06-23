using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class tk2dSpriteCollectionDefinition
{
	public enum Anchor
	{
		UpperLeft,
		UpperCenter,
		UpperRight,
		MiddleLeft,
		MiddleCenter,
		MiddleRight,
		LowerLeft,
		LowerCenter,
		LowerRight,
		Custom
	}

	public enum Pad
	{
		Default,
		BlackZeroAlpha,
		Extend,
		TileXY
	}

	public enum ColliderType
	{
		UserDefined,
		ForceNone,
		BoxTrimmed,
		BoxCustom,
		Polygon
	}

	public enum PolygonColliderCap
	{
		None,
		FrontAndBack,
		Front,
		Back
	}

	public enum ColliderColor
	{
		Default,
		Red,
		White,
		Black
	}

	public enum Source
	{
		Sprite,
		SpriteSheet,
		Font
	}

	public enum DiceFilter
	{
		Complete,
		SolidOnly,
		TransparentOnly
	}

	public string name = string.Empty;

	public bool disableTrimming;

	public bool additive;

	public Vector3 scale = new Vector3(1f, 1f, 1f);

	public Texture2D texture;

	[NonSerialized]
	public Texture2D thumbnailTexture;

	public int materialId;

	public Anchor anchor = Anchor.MiddleCenter;

	public float anchorX;

	public float anchorY;

	public UnityEngine.Object overrideMesh;

	public bool doubleSidedSprite;

	public bool customSpriteGeometry;

	public tk2dSpriteColliderIsland[] geometryIslands = new tk2dSpriteColliderIsland[0];

	public bool dice;

	public int diceUnitX = 64;

	public int diceUnitY = 64;

	public DiceFilter diceFilter;

	public Pad pad;

	public int extraPadding;

	public Source source;

	public bool fromSpriteSheet;

	public bool hasSpriteSheetId;

	public int spriteSheetId;

	public int spriteSheetX;

	public int spriteSheetY;

	public bool extractRegion;

	public int regionX;

	public int regionY;

	public int regionW;

	public int regionH;

	public int regionId;

	public ColliderType colliderType;

	public CollisionLayer collisionLayer = CollisionLayer.HighObstacle;

	public BagelCollider[] bagelColliders;

	public TilesetIndexMetadata metadata;

	public Vector2 boxColliderMin;

	public Vector2 boxColliderMax;

	public tk2dSpriteColliderIsland[] polyColliderIslands;

	public PolygonColliderCap polyColliderCap = PolygonColliderCap.FrontAndBack;

	public bool colliderConvex;

	public bool colliderSmoothSphereCollisions;

	public ColliderColor colliderColor;

	public List<tk2dSpriteDefinition.AttachPoint> attachPoints = new List<tk2dSpriteDefinition.AttachPoint>();

	public void CopyFrom(tk2dSpriteCollectionDefinition src)
	{
		name = src.name;
		disableTrimming = src.disableTrimming;
		additive = src.additive;
		scale = src.scale;
		texture = src.texture;
		materialId = src.materialId;
		anchor = src.anchor;
		anchorX = src.anchorX;
		anchorY = src.anchorY;
		overrideMesh = src.overrideMesh;
		doubleSidedSprite = src.doubleSidedSprite;
		customSpriteGeometry = src.customSpriteGeometry;
		geometryIslands = src.geometryIslands;
		dice = src.dice;
		diceUnitX = src.diceUnitX;
		diceUnitY = src.diceUnitY;
		diceFilter = src.diceFilter;
		pad = src.pad;
		source = src.source;
		fromSpriteSheet = src.fromSpriteSheet;
		hasSpriteSheetId = src.hasSpriteSheetId;
		spriteSheetX = src.spriteSheetX;
		spriteSheetY = src.spriteSheetY;
		spriteSheetId = src.spriteSheetId;
		extractRegion = src.extractRegion;
		regionX = src.regionX;
		regionY = src.regionY;
		regionW = src.regionW;
		regionH = src.regionH;
		regionId = src.regionId;
		colliderType = src.colliderType;
		collisionLayer = src.collisionLayer;
		if (src.bagelColliders != null)
		{
			bagelColliders = new BagelCollider[src.bagelColliders.Length];
			for (int i = 0; i < src.bagelColliders.Length; i++)
			{
				bagelColliders[i] = new BagelCollider(src.bagelColliders[i]);
			}
		}
		if (src.metadata == null)
		{
			metadata = new TilesetIndexMetadata();
		}
		else
		{
			if (metadata == null)
			{
				metadata = new TilesetIndexMetadata();
			}
			metadata.CopyFrom(src.metadata);
		}
		boxColliderMin = src.boxColliderMin;
		boxColliderMax = src.boxColliderMax;
		polyColliderCap = src.polyColliderCap;
		colliderColor = src.colliderColor;
		colliderConvex = src.colliderConvex;
		colliderSmoothSphereCollisions = src.colliderSmoothSphereCollisions;
		extraPadding = src.extraPadding;
		if (src.polyColliderIslands != null)
		{
			polyColliderIslands = new tk2dSpriteColliderIsland[src.polyColliderIslands.Length];
			for (int j = 0; j < polyColliderIslands.Length; j++)
			{
				polyColliderIslands[j] = new tk2dSpriteColliderIsland();
				polyColliderIslands[j].CopyFrom(src.polyColliderIslands[j]);
			}
		}
		else
		{
			polyColliderIslands = new tk2dSpriteColliderIsland[0];
		}
		if (src.geometryIslands != null)
		{
			geometryIslands = new tk2dSpriteColliderIsland[src.geometryIslands.Length];
			for (int k = 0; k < geometryIslands.Length; k++)
			{
				geometryIslands[k] = new tk2dSpriteColliderIsland();
				geometryIslands[k].CopyFrom(src.geometryIslands[k]);
			}
		}
		else
		{
			geometryIslands = new tk2dSpriteColliderIsland[0];
		}
		attachPoints = new List<tk2dSpriteDefinition.AttachPoint>(src.attachPoints.Count);
		foreach (tk2dSpriteDefinition.AttachPoint attachPoint2 in src.attachPoints)
		{
			tk2dSpriteDefinition.AttachPoint attachPoint = new tk2dSpriteDefinition.AttachPoint();
			attachPoint.CopyFrom(attachPoint2);
			attachPoints.Add(attachPoint);
		}
	}

	public void Clear()
	{
		tk2dSpriteCollectionDefinition src = new tk2dSpriteCollectionDefinition();
		CopyFrom(src);
	}

	public bool CompareTo(tk2dSpriteCollectionDefinition src)
	{
		if (name != src.name)
		{
			return false;
		}
		if (additive != src.additive)
		{
			return false;
		}
		if (scale != src.scale)
		{
			return false;
		}
		if (texture != src.texture)
		{
			return false;
		}
		if (materialId != src.materialId)
		{
			return false;
		}
		if (anchor != src.anchor)
		{
			return false;
		}
		if (anchorX != src.anchorX)
		{
			return false;
		}
		if (anchorY != src.anchorY)
		{
			return false;
		}
		if (overrideMesh != src.overrideMesh)
		{
			return false;
		}
		if (dice != src.dice)
		{
			return false;
		}
		if (diceUnitX != src.diceUnitX)
		{
			return false;
		}
		if (diceUnitY != src.diceUnitY)
		{
			return false;
		}
		if (diceFilter != src.diceFilter)
		{
			return false;
		}
		if (pad != src.pad)
		{
			return false;
		}
		if (extraPadding != src.extraPadding)
		{
			return false;
		}
		if (doubleSidedSprite != src.doubleSidedSprite)
		{
			return false;
		}
		if (customSpriteGeometry != src.customSpriteGeometry)
		{
			return false;
		}
		if (geometryIslands != src.geometryIslands)
		{
			return false;
		}
		if (geometryIslands != null && src.geometryIslands != null)
		{
			if (geometryIslands.Length != src.geometryIslands.Length)
			{
				return false;
			}
			for (int i = 0; i < geometryIslands.Length; i++)
			{
				if (!geometryIslands[i].CompareTo(src.geometryIslands[i]))
				{
					return false;
				}
			}
		}
		if (source != src.source)
		{
			return false;
		}
		if (fromSpriteSheet != src.fromSpriteSheet)
		{
			return false;
		}
		if (hasSpriteSheetId != src.hasSpriteSheetId)
		{
			return false;
		}
		if (spriteSheetId != src.spriteSheetId)
		{
			return false;
		}
		if (spriteSheetX != src.spriteSheetX)
		{
			return false;
		}
		if (spriteSheetY != src.spriteSheetY)
		{
			return false;
		}
		if (extractRegion != src.extractRegion)
		{
			return false;
		}
		if (regionX != src.regionX)
		{
			return false;
		}
		if (regionY != src.regionY)
		{
			return false;
		}
		if (regionW != src.regionW)
		{
			return false;
		}
		if (regionH != src.regionH)
		{
			return false;
		}
		if (regionId != src.regionId)
		{
			return false;
		}
		if (colliderType != src.colliderType)
		{
			return false;
		}
		if (collisionLayer != src.collisionLayer)
		{
			return false;
		}
		if (bagelColliders != src.bagelColliders)
		{
			return false;
		}
		if (metadata != src.metadata)
		{
			return false;
		}
		if (boxColliderMin != src.boxColliderMin)
		{
			return false;
		}
		if (boxColliderMax != src.boxColliderMax)
		{
			return false;
		}
		if (polyColliderIslands != src.polyColliderIslands)
		{
			return false;
		}
		if (polyColliderIslands != null && src.polyColliderIslands != null)
		{
			if (polyColliderIslands.Length != src.polyColliderIslands.Length)
			{
				return false;
			}
			for (int j = 0; j < polyColliderIslands.Length; j++)
			{
				if (!polyColliderIslands[j].CompareTo(src.polyColliderIslands[j]))
				{
					return false;
				}
			}
		}
		if (polyColliderCap != src.polyColliderCap)
		{
			return false;
		}
		if (colliderColor != src.colliderColor)
		{
			return false;
		}
		if (colliderSmoothSphereCollisions != src.colliderSmoothSphereCollisions)
		{
			return false;
		}
		if (colliderConvex != src.colliderConvex)
		{
			return false;
		}
		if (attachPoints.Count != src.attachPoints.Count)
		{
			return false;
		}
		for (int k = 0; k < attachPoints.Count; k++)
		{
			if (!attachPoints[k].CompareTo(src.attachPoints[k]))
			{
				return false;
			}
		}
		return true;
	}
}
