using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
[AddComponentMenu("2D Toolkit/Sprite/tk2dTiledSprite")]
[RequireComponent(typeof(MeshFilter))]
public class tk2dTiledSprite : tk2dBaseSprite
{
	public delegate void OverrideGetTiledSpriteGeomDescDelegate(out int numVertices, out int numIndices, tk2dSpriteDefinition spriteDef, Vector2 dimensions);

	public delegate void OverrideSetTiledSpriteGeomDelegate(Vector3[] pos, Vector2[] uv, int offset, out Vector3 boundsCenter, out Vector3 boundsExtents, tk2dSpriteDefinition spriteDef, Vector3 scale, Vector2 dimensions, Anchor anchor, float colliderOffsetZ, float colliderExtentZ);

	private Mesh mesh;

	private Vector2[] meshUvs;

	private Vector3[] meshVertices;

	private Color32[] meshColors;

	private Vector3[] meshNormals;

	private Vector4[] meshTangents;

	private int[] meshIndices;

	[SerializeField]
	private Vector2 _dimensions = new Vector2(50f, 50f);

	[SerializeField]
	private Anchor _anchor;

	[SerializeField]
	protected bool _createBoxCollider;

	private Vector3 boundsCenter = Vector3.zero;

	private Vector3 boundsExtents = Vector3.zero;

	public OverrideGetTiledSpriteGeomDescDelegate OverrideGetTiledSpriteGeomDesc;

	public OverrideSetTiledSpriteGeomDelegate OverrideSetTiledSpriteGeom;

	public Vector2 dimensions
	{
		get
		{
			return _dimensions;
		}
		set
		{
			if (value != _dimensions)
			{
				_dimensions = value;
				UpdateVertices();
				UpdateCollider();
			}
		}
	}

	public Anchor anchor
	{
		get
		{
			return _anchor;
		}
		set
		{
			if (value != _anchor)
			{
				_anchor = value;
				UpdateVertices();
				UpdateCollider();
			}
		}
	}

	public bool CreateBoxCollider
	{
		get
		{
			return _createBoxCollider;
		}
		set
		{
			if (_createBoxCollider != value)
			{
				_createBoxCollider = value;
				UpdateCollider();
			}
		}
	}

	private new void Awake()
	{
		base.Awake();
		mesh = new Mesh();
		mesh.hideFlags = HideFlags.DontSave;
		GetComponent<MeshFilter>().mesh = mesh;
		if ((bool)base.Collection)
		{
			if (_spriteId < 0 || _spriteId >= base.Collection.Count)
			{
				_spriteId = 0;
			}
			Build();
			if (boxCollider == null)
			{
				boxCollider = GetComponent<BoxCollider>();
			}
			if (boxCollider2D == null)
			{
				boxCollider2D = GetComponent<BoxCollider2D>();
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)mesh)
		{
			Object.Destroy(mesh);
		}
	}

	protected new void SetColors(Color32[] dest)
	{
		int numVertices;
		int numIndices;
		if (OverrideGetTiledSpriteGeomDesc != null)
		{
			OverrideGetTiledSpriteGeomDesc(out numVertices, out numIndices, base.CurrentSprite, dimensions);
		}
		else
		{
			tk2dSpriteGeomGen.GetTiledSpriteGeomDesc(out numVertices, out numIndices, base.CurrentSprite, dimensions);
		}
		tk2dSpriteGeomGen.SetSpriteColors(dest, 0, numVertices, _color, collectionInst.premultipliedAlpha);
	}

	public override void Build()
	{
		tk2dSpriteDefinition currentSprite = base.CurrentSprite;
		int numVertices;
		int numIndices;
		if (OverrideGetTiledSpriteGeomDesc != null)
		{
			OverrideGetTiledSpriteGeomDesc(out numVertices, out numIndices, currentSprite, dimensions);
		}
		else
		{
			tk2dSpriteGeomGen.GetTiledSpriteGeomDesc(out numVertices, out numIndices, currentSprite, dimensions);
		}
		int num = numVertices;
		if (meshUvs == null || meshUvs.Length < numVertices)
		{
			num = BraveUtility.SmartListResizer((meshUvs != null) ? meshUvs.Length : 0, numVertices);
			meshUvs = new Vector2[num];
			meshVertices = new Vector3[num];
			meshColors = new Color32[num];
		}
		if (meshIndices == null || meshIndices.Length < numIndices)
		{
			int num2 = BraveUtility.SmartListResizer((meshIndices != null) ? meshIndices.Length : 0, numIndices, 100, 3);
			meshIndices = new int[num2];
		}
		int num3 = 0;
		if (currentSprite != null && currentSprite.normals != null && currentSprite.normals.Length > 0)
		{
			num3 = num;
		}
		if (meshNormals == null || meshNormals.Length < num3)
		{
			meshNormals = new Vector3[num3];
		}
		int num4 = 0;
		if (currentSprite != null && currentSprite.tangents != null && currentSprite.tangents.Length > 0)
		{
			num4 = num;
		}
		if (meshTangents == null || meshTangents.Length < num4)
		{
			meshTangents = new Vector4[num4];
		}
		float colliderOffsetZ = ((!(boxCollider != null)) ? 0f : boxCollider.center.z);
		float colliderExtentZ = ((!(boxCollider != null)) ? 0.5f : (boxCollider.size.z * 0.5f));
		if (OverrideSetTiledSpriteGeom != null)
		{
			OverrideSetTiledSpriteGeom(meshVertices, meshUvs, 0, out boundsCenter, out boundsExtents, currentSprite, _scale, dimensions, anchor, colliderOffsetZ, colliderExtentZ);
		}
		else
		{
			tk2dSpriteGeomGen.SetTiledSpriteGeom(meshVertices, meshUvs, 0, out boundsCenter, out boundsExtents, currentSprite, _scale, dimensions, anchor, colliderOffsetZ, colliderExtentZ);
		}
		tk2dSpriteGeomGen.SetTiledSpriteIndices(meshIndices, 0, 0, currentSprite, dimensions, OverrideGetTiledSpriteGeomDesc);
		if (meshNormals.Length > 0 || meshTangents.Length > 0)
		{
			tk2dSpriteGeomGen.SetSpriteVertexNormalsFast(meshVertices, meshNormals, meshTangents);
		}
		SetColors(meshColors);
		if (base.ShouldDoTilt)
		{
			bool isPerpendicular = base.IsPerpendicular;
			for (int i = 0; i < numVertices; i++)
			{
				float y = (m_transform.rotation * Vector3.Scale(meshVertices[i], m_transform.lossyScale)).y;
				if (isPerpendicular)
				{
					meshVertices[i].z -= y;
				}
				else
				{
					meshVertices[i].z += y;
				}
			}
		}
		if (mesh == null)
		{
			mesh = new Mesh();
			mesh.hideFlags = HideFlags.DontSave;
		}
		else
		{
			mesh.Clear();
		}
		mesh.vertices = meshVertices;
		mesh.colors32 = meshColors;
		mesh.uv = meshUvs;
		mesh.normals = meshNormals;
		mesh.tangents = meshTangents;
		mesh.triangles = meshIndices;
		mesh.RecalculateBounds();
		mesh.bounds = tk2dBaseSprite.AdjustedMeshBounds(mesh.bounds, renderLayer);
		GetComponent<MeshFilter>().mesh = mesh;
		UpdateCollider();
		UpdateMaterial();
	}

	protected override void UpdateGeometry()
	{
		UpdateGeometryImpl();
	}

	protected override void UpdateColors()
	{
		UpdateColorsImpl();
	}

	protected override void UpdateVertices()
	{
		UpdateGeometryImpl();
	}

	protected void UpdateColorsImpl()
	{
		if (meshColors == null || meshColors.Length == 0)
		{
			Build();
			return;
		}
		SetColors(meshColors);
		mesh.colors32 = meshColors;
	}

	protected void UpdateGeometryImpl()
	{
		Build();
	}

	protected override void UpdateCollider()
	{
		if (!CreateBoxCollider)
		{
			return;
		}
		if (base.CurrentSprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics3D)
		{
			if (boxCollider != null)
			{
				boxCollider.size = 2f * boundsExtents;
				boxCollider.center = boundsCenter;
			}
		}
		else if (base.CurrentSprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics2D && boxCollider2D != null)
		{
			boxCollider2D.size = 2f * boundsExtents;
			boxCollider2D.offset = boundsCenter;
		}
	}

	protected override void CreateCollider()
	{
		UpdateCollider();
	}

	protected override void UpdateMaterial()
	{
		if (OverrideMaterialMode == SpriteMaterialOverrideMode.NONE && base.renderer.sharedMaterial != collectionInst.spriteDefinitions[base.spriteId].materialInst)
		{
			base.renderer.material = collectionInst.spriteDefinitions[base.spriteId].materialInst;
		}
	}

	protected override int GetCurrentVertexCount()
	{
		return 16;
	}

	public override void ReshapeBounds(Vector3 dMin, Vector3 dMax)
	{
		tk2dSpriteDefinition currentSprite = base.CurrentSprite;
		Vector3 vector = new Vector3(_dimensions.x * currentSprite.texelSize.x * _scale.x, _dimensions.y * currentSprite.texelSize.y * _scale.y);
		Vector3 zero = Vector3.zero;
		switch (_anchor)
		{
		case Anchor.LowerLeft:
			zero.Set(0f, 0f, 0f);
			break;
		case Anchor.LowerCenter:
			zero.Set(0.5f, 0f, 0f);
			break;
		case Anchor.LowerRight:
			zero.Set(1f, 0f, 0f);
			break;
		case Anchor.MiddleLeft:
			zero.Set(0f, 0.5f, 0f);
			break;
		case Anchor.MiddleCenter:
			zero.Set(0.5f, 0.5f, 0f);
			break;
		case Anchor.MiddleRight:
			zero.Set(1f, 0.5f, 0f);
			break;
		case Anchor.UpperLeft:
			zero.Set(0f, 1f, 0f);
			break;
		case Anchor.UpperCenter:
			zero.Set(0.5f, 1f, 0f);
			break;
		case Anchor.UpperRight:
			zero.Set(1f, 1f, 0f);
			break;
		}
		zero = Vector3.Scale(zero, vector) * -1f;
		Vector3 vector2 = vector + dMax - dMin;
		vector2.x /= currentSprite.texelSize.x * _scale.x;
		vector2.y /= currentSprite.texelSize.y * _scale.y;
		Vector3 vector3 = new Vector3((!Mathf.Approximately(_dimensions.x, 0f)) ? (zero.x * vector2.x / _dimensions.x) : 0f, (!Mathf.Approximately(_dimensions.y, 0f)) ? (zero.y * vector2.y / _dimensions.y) : 0f);
		Vector3 position = zero + dMin - vector3;
		position.z = 0f;
		base.transform.position = base.transform.TransformPoint(position);
		dimensions = new Vector2(vector2.x, vector2.y);
	}
}
