using System;
using UnityEngine;

[Serializable]
public class tk2dSpriteDefinition
{
	public enum ColliderType
	{
		Unset,
		None,
		Box,
		Mesh
	}

	public enum PhysicsEngine
	{
		Physics3D,
		Physics2D
	}

	public enum FlipMode
	{
		None,
		Tk2d,
		TPackerCW
	}

	[Serializable]
	public class AttachPoint
	{
		public string name = string.Empty;

		public Vector3 position = Vector3.zero;

		public float angle;

		public void CopyFrom(AttachPoint src)
		{
			name = src.name;
			position = src.position;
			angle = src.angle;
		}

		public bool CompareTo(AttachPoint src)
		{
			return name == src.name && src.position == position && src.angle == angle;
		}
	}

	public string name;

	public Vector3 boundsDataCenter;

	public Vector3 boundsDataExtents;

	public Vector3 untrimmedBoundsDataCenter;

	public Vector3 untrimmedBoundsDataExtents;

	public Vector2 texelSize;

	public Vector3 position0;

	public Vector3 position1;

	public Vector3 position2;

	public Vector3 position3;

	public static Vector3[] defaultNormals;

	public static Vector4[] defaultTangents;

	public Vector2[] uvs;

	private static int[] defaultIndices = new int[6] { 0, 3, 1, 2, 3, 0 };

	public Material material;

	[NonSerialized]
	public Material materialInst;

	public int materialId;

	public bool extractRegion;

	public int regionX;

	public int regionY;

	public int regionW;

	public int regionH;

	public FlipMode flipped;

	public bool complexGeometry;

	public PhysicsEngine physicsEngine;

	public ColliderType colliderType;

	public CollisionLayer collisionLayer;

	[SerializeField]
	public TilesetIndexMetadata metadata;

	public Vector3[] colliderVertices;

	public bool colliderConvex;

	public bool colliderSmoothSphereCollisions;

	private bool? m_cachedIsTileSquare;

	public Vector3[] normals
	{
		get
		{
			if (defaultNormals == null)
			{
				defaultNormals = new Vector3[4]
				{
					new Vector3(0f, 0f, -1f),
					new Vector3(0f, 0f, -1f),
					new Vector3(0f, 0f, -1f),
					new Vector3(0f, 0f, -1f)
				};
			}
			return defaultNormals;
		}
		set
		{
		}
	}

	public Vector4[] tangents
	{
		get
		{
			if (defaultTangents == null)
			{
				defaultTangents = new Vector4[4]
				{
					new Vector4(1f, 0f, 0f, 1f),
					new Vector4(1f, 0f, 0f, 1f),
					new Vector4(1f, 0f, 0f, 1f),
					new Vector4(1f, 0f, 0f, 1f)
				};
			}
			return defaultTangents;
		}
		set
		{
		}
	}

	public int[] indices
	{
		get
		{
			return defaultIndices;
		}
		set
		{
		}
	}

	public bool Valid
	{
		get
		{
			return name.Length != 0;
		}
	}

	public bool IsTileSquare
	{
		get
		{
			if (!m_cachedIsTileSquare.HasValue)
			{
				m_cachedIsTileSquare = CheckIsTileSquare();
			}
			return m_cachedIsTileSquare.Value;
		}
	}

	public Vector3[] ConstructExpensivePositions()
	{
		return new Vector3[4] { position0, position1, position2, position3 };
	}

	public BagelCollider[] GetBagelColliders(tk2dSpriteCollectionData collection, int spriteId)
	{
		return collection.GetBagelColliders(spriteId);
	}

	public AttachPoint[] GetAttachPoints(tk2dSpriteCollectionData collection, int spriteId)
	{
		return collection.GetAttachPoints(spriteId);
	}

	private bool CheckIsTileSquare()
	{
		if (colliderVertices == null)
		{
			return false;
		}
		if (colliderVertices.Length == 2)
		{
			return Mathf.Approximately(colliderVertices[0].x, 0.5f) && Mathf.Approximately(colliderVertices[0].y, 0.5f) && Mathf.Approximately(colliderVertices[1].x, 0.5f) && Mathf.Approximately(colliderVertices[1].y, 0.5f);
		}
		if (colliderVertices.Length == 8)
		{
			for (int i = 0; i < 8; i++)
			{
				if (!Mathf.Approximately(colliderVertices[i].x, 0f) && !Mathf.Approximately(colliderVertices[i].x, 1f))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public Bounds GetBounds()
	{
		return new Bounds(new Vector3(boundsDataCenter.x, boundsDataCenter.y, boundsDataCenter.z), new Vector3(boundsDataExtents.x, boundsDataExtents.y, boundsDataExtents.z));
	}

	public Bounds GetUntrimmedBounds()
	{
		return new Bounds(new Vector3(untrimmedBoundsDataCenter.x, untrimmedBoundsDataCenter.y, untrimmedBoundsDataCenter.z), new Vector3(untrimmedBoundsDataExtents.x, untrimmedBoundsDataExtents.y, untrimmedBoundsDataExtents.z));
	}
}
