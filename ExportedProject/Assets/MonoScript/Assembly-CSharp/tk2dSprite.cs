using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
[AddComponentMenu("2D Toolkit/Sprite/tk2dSprite")]
[RequireComponent(typeof(MeshRenderer))]
public class tk2dSprite : tk2dBaseSprite
{
	private Mesh mesh;

	private Vector3[] meshVertices;

	private Vector3[] meshNormals;

	private Vector4[] meshTangents;

	private Color32[] meshColors;

	private MeshFilter m_filter;

	public bool ApplyEmissivePropertyBlock;

	public bool GenerateUV2;

	public bool LockUV2OnFrameOne;

	public bool StaticPositions;

	[NonSerialized]
	private bool m_hasGeneratedLockedUV2;

	private static Vector3[] m_defaultNormalArray = new Vector3[4]
	{
		new Vector3(-1f, -1f, -1f),
		new Vector3(1f, -1f, -1f),
		new Vector3(-1f, 1f, -1f),
		new Vector3(1f, 1f, -1f)
	};

	private static Vector4[] m_defaultTangentArray = new Vector4[4]
	{
		new Vector4(1f, 0f, 0f, 1f),
		new Vector4(1f, 0f, 0f, 1f),
		new Vector4(1f, 0f, 0f, 1f),
		new Vector4(1f, 0f, 0f, 1f)
	};

	private bool hasSetPositions;

	private static Vector2[] m_defaultUvs = new Vector2[4]
	{
		Vector2.zero,
		Vector2.right,
		Vector2.up,
		Vector2.one
	};

	private static int m_shaderEmissivePowerID = -1;

	private static int m_shaderEmissiveColorPowerID = -1;

	private static int m_shaderEmissiveColorID = -1;

	private static int m_shaderThresholdID = -1;

	private new void Awake()
	{
		base.Awake();
		mesh = new Mesh();
		mesh.MarkDynamic();
		mesh.hideFlags = HideFlags.DontSave;
		m_filter = GetComponent<MeshFilter>();
		m_filter.mesh = mesh;
		if ((bool)base.Collection)
		{
			if (_spriteId < 0 || _spriteId >= base.Collection.Count)
			{
				_spriteId = 0;
			}
			Build();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if ((bool)mesh)
		{
			UnityEngine.Object.Destroy(mesh);
		}
		if ((bool)meshColliderMesh)
		{
			UnityEngine.Object.Destroy(meshColliderMesh);
		}
	}

	public override void Build()
	{
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[base.spriteId];
		if (meshVertices == null || meshVertices.Length != 4 || meshColors == null || meshColors.Length != 4)
		{
			meshVertices = new Vector3[4];
			meshColors = new Color32[4];
		}
		meshNormals = m_defaultNormalArray;
		meshTangents = m_defaultTangentArray;
		SetPositions(meshVertices, meshNormals, meshTangents);
		SetColors(meshColors);
		if (mesh == null)
		{
			mesh = new Mesh();
			mesh.MarkDynamic();
			mesh.hideFlags = HideFlags.DontSave;
			GetComponent<MeshFilter>().mesh = mesh;
		}
		mesh.Clear();
		mesh.vertices = meshVertices;
		mesh.normals = meshNormals;
		mesh.tangents = meshTangents;
		mesh.colors32 = meshColors;
		mesh.uv = tk2dSpriteDefinition2.uvs;
		if (GenerateUV2)
		{
			if (LockUV2OnFrameOne)
			{
				if (!m_hasGeneratedLockedUV2)
				{
					m_hasGeneratedLockedUV2 = true;
					mesh.uv2 = tk2dSpriteDefinition2.uvs;
				}
			}
			else if (base.spriteAnimator != null && base.spriteAnimator.IsFrameBlendedAnimation)
			{
				mesh.uv2 = base.spriteAnimator.GetNextFrameUVs();
			}
			else
			{
				mesh.uv2 = m_defaultUvs;
			}
		}
		mesh.triangles = tk2dSpriteDefinition2.indices;
		mesh.bounds = tk2dBaseSprite.AdjustedMeshBounds(GetBounds(), renderLayer);
		UpdateMaterial();
		CreateCollider();
	}

	public static tk2dSprite AddComponent(GameObject go, tk2dSpriteCollectionData spriteCollection, int spriteId)
	{
		return tk2dBaseSprite.AddComponent<tk2dSprite>(go, spriteCollection, spriteId);
	}

	public static tk2dSprite AddComponent(GameObject go, tk2dSpriteCollectionData spriteCollection, string spriteName)
	{
		return tk2dBaseSprite.AddComponent<tk2dSprite>(go, spriteCollection, spriteName);
	}

	public static GameObject CreateFromTexture(Texture texture, tk2dSpriteCollectionSize size, Rect region, Vector2 anchor)
	{
		return tk2dBaseSprite.CreateFromTexture<tk2dSprite>(texture, size, region, anchor);
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
		UpdateVerticesImpl();
	}

	protected void UpdateColorsImpl()
	{
		if (!(mesh == null) && meshColors != null && meshColors.Length != 0)
		{
			SetColors(meshColors);
			mesh.colors32 = meshColors;
		}
	}

	protected void UpdateVerticesImpl()
	{
		if (mesh == null || meshVertices == null || meshVertices.Length == 0 || !collectionInst || collectionInst.spriteDefinitions == null)
		{
			return;
		}
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[base.spriteId];
		meshNormals = m_defaultNormalArray;
		meshTangents = m_defaultTangentArray;
		if (!StaticPositions || !hasSetPositions)
		{
			SetPositions(meshVertices, meshNormals, meshTangents);
			hasSetPositions = true;
		}
		mesh.vertices = meshVertices;
		mesh.normals = meshNormals;
		mesh.tangents = meshTangents;
		mesh.uv = tk2dSpriteDefinition2.uvs;
		if (GenerateUV2)
		{
			if (LockUV2OnFrameOne)
			{
				if (!m_hasGeneratedLockedUV2)
				{
					m_hasGeneratedLockedUV2 = true;
					mesh.uv2 = tk2dSpriteDefinition2.uvs;
				}
			}
			else if ((bool)base.spriteAnimator && base.spriteAnimator.IsFrameBlendedAnimation)
			{
				mesh.uv2 = base.spriteAnimator.GetNextFrameUVs();
			}
			else
			{
				mesh.uv2 = m_defaultUvs;
			}
		}
		mesh.bounds = tk2dBaseSprite.AdjustedMeshBounds(GetBounds(), renderLayer);
	}

	protected void UpdateGeometryImpl()
	{
		if (mesh == null)
		{
			return;
		}
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[base.spriteId];
		if (meshVertices == null || meshVertices.Length != 4)
		{
			meshVertices = new Vector3[4];
			meshNormals = m_defaultNormalArray;
			meshTangents = m_defaultTangentArray;
			meshColors = new Color32[4];
		}
		SetPositions(meshVertices, meshNormals, meshTangents);
		SetColors(meshColors);
		mesh.Clear();
		mesh.vertices = meshVertices;
		mesh.normals = meshNormals;
		mesh.tangents = meshTangents;
		mesh.colors32 = meshColors;
		mesh.uv = tk2dSpriteDefinition2.uvs;
		if (GenerateUV2)
		{
			if (LockUV2OnFrameOne)
			{
				if (!m_hasGeneratedLockedUV2)
				{
					m_hasGeneratedLockedUV2 = true;
					mesh.uv2 = tk2dSpriteDefinition2.uvs;
				}
			}
			else if (base.spriteAnimator.IsFrameBlendedAnimation)
			{
				mesh.uv2 = base.spriteAnimator.GetNextFrameUVs();
			}
			else
			{
				mesh.uv2 = m_defaultUvs;
			}
		}
		mesh.bounds = tk2dBaseSprite.AdjustedMeshBounds(GetBounds(), renderLayer);
		mesh.triangles = tk2dSpriteDefinition2.indices;
	}

	protected void CopyPropertyBlock(Material source, Material dest)
	{
		if (dest.HasProperty(m_shaderEmissivePowerID) && source.HasProperty(m_shaderEmissivePowerID))
		{
			dest.SetFloat(m_shaderEmissivePowerID, source.GetFloat(m_shaderEmissivePowerID));
		}
		if (dest.HasProperty(m_shaderEmissiveColorPowerID) && source.HasProperty(m_shaderEmissiveColorPowerID))
		{
			dest.SetFloat(m_shaderEmissiveColorPowerID, source.GetFloat(m_shaderEmissiveColorPowerID));
		}
		if (dest.HasProperty(m_shaderEmissiveColorID) && source.HasProperty(m_shaderEmissiveColorID))
		{
			dest.SetColor(m_shaderEmissiveColorID, source.GetColor(m_shaderEmissiveColorID));
		}
		if (dest.HasProperty(m_shaderThresholdID) && source.HasProperty(m_shaderThresholdID))
		{
			dest.SetFloat(m_shaderThresholdID, source.GetFloat(m_shaderThresholdID));
		}
	}

	protected override void UpdateMaterial()
	{
		if (!base.renderer)
		{
			return;
		}
		if (m_shaderEmissiveColorID == -1)
		{
			m_shaderEmissivePowerID = Shader.PropertyToID("_EmissivePower");
			m_shaderEmissiveColorPowerID = Shader.PropertyToID("_EmissiveColorPower");
			m_shaderEmissiveColorID = Shader.PropertyToID("_EmissiveColor");
			m_shaderThresholdID = Shader.PropertyToID("_EmissiveThresholdSensitivity");
		}
		if (OverrideMaterialMode != 0 && base.renderer.sharedMaterial != null)
		{
			if (OverrideMaterialMode == SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_SIMPLE)
			{
				Material materialInst = collectionInst.spriteDefinitions[base.spriteId].materialInst;
				Material sharedMaterial = base.renderer.sharedMaterial;
				if (sharedMaterial != materialInst)
				{
					sharedMaterial.mainTexture = materialInst.mainTexture;
					if (ApplyEmissivePropertyBlock)
					{
						CopyPropertyBlock(materialInst, sharedMaterial);
					}
				}
				return;
			}
			if (OverrideMaterialMode == SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_COMPLEX)
			{
				return;
			}
		}
		if (base.renderer.sharedMaterial != collectionInst.spriteDefinitions[base.spriteId].materialInst)
		{
			base.renderer.material = collectionInst.spriteDefinitions[base.spriteId].materialInst;
		}
	}

	protected override int GetCurrentVertexCount()
	{
		if (meshVertices == null)
		{
			return 0;
		}
		return meshVertices.Length;
	}

	public override void ForceBuild()
	{
		if ((bool)this)
		{
			base.ForceBuild();
			GetComponent<MeshFilter>().mesh = mesh;
		}
	}

	public override void ReshapeBounds(Vector3 dMin, Vector3 dMax)
	{
		tk2dSpriteDefinition currentSprite = base.CurrentSprite;
		Vector3 vector = Vector3.Scale(currentSprite.untrimmedBoundsDataCenter - 0.5f * currentSprite.untrimmedBoundsDataExtents, _scale);
		Vector3 vector2 = Vector3.Scale(currentSprite.untrimmedBoundsDataExtents, _scale);
		Vector3 vector3 = vector2 + dMax - dMin;
		vector3.x /= currentSprite.untrimmedBoundsDataExtents.x;
		vector3.y /= currentSprite.untrimmedBoundsDataExtents.y;
		Vector3 vector4 = new Vector3((!Mathf.Approximately(_scale.x, 0f)) ? (vector.x * vector3.x / _scale.x) : 0f, (!Mathf.Approximately(_scale.y, 0f)) ? (vector.y * vector3.y / _scale.y) : 0f);
		Vector3 position = vector + dMin - vector4;
		position.z = 0f;
		base.transform.position = base.transform.TransformPoint(position);
		base.scale = new Vector3(vector3.x, vector3.y, _scale.z);
	}
}
