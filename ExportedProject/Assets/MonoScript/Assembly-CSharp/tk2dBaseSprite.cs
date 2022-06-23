using System;
using System.Collections.Generic;
using tk2dRuntime;
using UnityEngine;

[AddComponentMenu("2D Toolkit/Backend/tk2dBaseSprite")]
public abstract class tk2dBaseSprite : PersistentVFXBehaviour, ISpriteCollectionForceBuild
{
	public enum SpriteMaterialOverrideMode
	{
		NONE,
		OVERRIDE_MATERIAL_SIMPLE,
		OVERRIDE_MATERIAL_COMPLEX
	}

	public enum Anchor
	{
		LowerLeft,
		LowerCenter,
		LowerRight,
		MiddleLeft,
		MiddleCenter,
		MiddleRight,
		UpperLeft,
		UpperCenter,
		UpperRight
	}

	public enum PerpendicularState
	{
		UNDEFINED,
		PERPENDICULAR,
		FLAT
	}

	public bool automaticallyManagesDepth = true;

	public bool ignoresTiltworldDepth;

	public bool depthUsesTrimmedBounds;

	public bool allowDefaultLayer;

	public tk2dBaseSprite attachParent;

	public SpriteMaterialOverrideMode OverrideMaterialMode;

	[HideInInspector]
	public bool independentOrientation;

	[Header("Decorator Data")]
	public bool autodetectFootprint = true;

	public IntVector2 customFootprintOrigin;

	public IntVector2 customFootprint;

	protected List<tk2dBaseSprite> attachedRenderers;

	protected MeshRenderer m_renderer;

	private Quaternion m_cachedRotation;

	protected float m_cachedYPosition;

	protected int m_cachedStartingSpriteID;

	public bool hasOffScreenCachedUpdate;

	public tk2dSpriteCollectionData offScreenCachedCollection;

	public int offScreenCachedID = -1;

	[SerializeField]
	private tk2dSpriteCollectionData collection;

	protected tk2dSpriteCollectionData collectionInst;

	[SerializeField]
	protected Color _color = Color.white;

	[SerializeField]
	protected Vector3 _scale = new Vector3(1f, 1f, 1f);

	[SerializeField]
	protected int _spriteId;

	public BoxCollider2D boxCollider2D;

	public BoxCollider boxCollider;

	public MeshCollider meshCollider;

	public Vector3[] meshColliderPositions;

	public Mesh meshColliderMesh;

	private Renderer _cachedRenderer;

	protected tk2dSpriteAnimator m_cachedAnimator;

	protected Transform m_transform;

	protected bool m_forceNoTilt;

	[NonSerialized]
	public float AdditionalFlatForwardPercentage;

	[NonSerialized]
	public float AdditionalPerpForwardPercentage;

	public PerpendicularState CachedPerpState;

	[HideInInspector]
	[SerializeField]
	protected float m_heightOffGround;

	[SerializeField]
	protected int renderLayer;

	[NonSerialized]
	public bool IsOutlineSprite;

	public bool IsBraveOutlineSprite;

	private Vector2 m_cachedScale;

	public bool IsZDepthDirty;

	public bool usesOverrideMaterial
	{
		get
		{
			return OverrideMaterialMode != SpriteMaterialOverrideMode.NONE;
		}
		set
		{
			if (value)
			{
				if (OverrideMaterialMode == SpriteMaterialOverrideMode.NONE)
				{
					OverrideMaterialMode = SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_SIMPLE;
				}
			}
			else
			{
				OverrideMaterialMode = SpriteMaterialOverrideMode.NONE;
			}
		}
	}

	public tk2dSpriteCollectionData Collection
	{
		get
		{
			if (m_cachedAnimator != null)
			{
				m_cachedAnimator.ForceInvisibleSpriteUpdate();
			}
			return collection;
		}
		set
		{
			collection = value;
			collectionInst = collection.inst;
		}
	}

	public Color color
	{
		get
		{
			return _color;
		}
		set
		{
			if (value != _color)
			{
				_color = value;
				InitInstance();
				UpdateColors();
			}
		}
	}

	public Vector3 scale
	{
		get
		{
			return _scale;
		}
		set
		{
			if (value != _scale)
			{
				_scale = value;
				InitInstance();
				UpdateVertices();
				UpdateCollider();
				if (this.SpriteChanged != null)
				{
					this.SpriteChanged(this);
				}
			}
		}
	}

	private Renderer CachedRenderer
	{
		get
		{
			if (_cachedRenderer == null)
			{
				_cachedRenderer = base.renderer;
			}
			return _cachedRenderer;
		}
	}

	public bool ShouldDoTilt
	{
		get
		{
			if (m_forceNoTilt)
			{
				return false;
			}
			if (CachedPerpState != 0)
			{
				return true;
			}
			if (base.renderer != null && base.renderer.sharedMaterial != null)
			{
				return base.renderer.sharedMaterial.HasProperty("_Perpendicular");
			}
			return false;
		}
		set
		{
			m_forceNoTilt = !value;
		}
	}

	public bool IsPerpendicular
	{
		get
		{
			if (base.renderer == null || base.renderer.sharedMaterial == null)
			{
				return false;
			}
			if (CachedPerpState != 0)
			{
				return CachedPerpState == PerpendicularState.PERPENDICULAR;
			}
			if (base.renderer.sharedMaterial.HasProperty("_Perpendicular"))
			{
				if (Application.isPlaying)
				{
					CachedPerpState = ((base.renderer.sharedMaterial.GetFloat("_Perpendicular") == 1f) ? PerpendicularState.PERPENDICULAR : PerpendicularState.FLAT);
					return CachedPerpState == PerpendicularState.PERPENDICULAR;
				}
				return base.renderer.sharedMaterial.GetFloat("_Perpendicular") == 1f;
			}
			Debug.LogWarning(base.name + " <- failed to get perp");
			return true;
		}
		set
		{
			CachedPerpState = (value ? PerpendicularState.PERPENDICULAR : PerpendicularState.FLAT);
			ForceBuild();
		}
	}

	public float HeightOffGround
	{
		get
		{
			return m_heightOffGround;
		}
		set
		{
			m_heightOffGround = value;
		}
	}

	public int SortingOrder
	{
		get
		{
			return CachedRenderer.sortingOrder;
		}
		set
		{
			if (CachedRenderer.sortingOrder != value)
			{
				renderLayer = value;
				CachedRenderer.sortingOrder = value;
			}
		}
	}

	public bool FlipX
	{
		get
		{
			return _scale.x < 0f;
		}
		set
		{
			scale = new Vector3(Mathf.Abs(_scale.x) * (float)((!value) ? 1 : (-1)), _scale.y, _scale.z);
		}
	}

	public bool FlipY
	{
		get
		{
			return _scale.y < 0f;
		}
		set
		{
			scale = new Vector3(_scale.x, Mathf.Abs(_scale.y) * (float)((!value) ? 1 : (-1)), _scale.z);
		}
	}

	public int spriteId
	{
		get
		{
			if (m_cachedAnimator != null)
			{
				m_cachedAnimator.ForceInvisibleSpriteUpdate();
			}
			return _spriteId;
		}
		set
		{
			hasOffScreenCachedUpdate = false;
			offScreenCachedCollection = null;
			offScreenCachedID = -1;
			if (value != _spriteId)
			{
				InitInstance();
				value = Mathf.Clamp(value, 0, collectionInst.spriteDefinitions.Length - 1);
				if (_spriteId < 0 || _spriteId >= collectionInst.spriteDefinitions.Length || GetCurrentVertexCount() != 4 || collectionInst.spriteDefinitions[_spriteId].complexGeometry != collectionInst.spriteDefinitions[value].complexGeometry)
				{
					_spriteId = value;
					UpdateGeometry();
				}
				else
				{
					_spriteId = value;
					UpdateVertices();
				}
				UpdateMaterial();
				UpdateCollider();
				if (this.SpriteChanged != null)
				{
					this.SpriteChanged(this);
				}
			}
		}
	}

	public tk2dSpriteDefinition CurrentSprite
	{
		get
		{
			InitInstance();
			return (!(collectionInst == null)) ? collectionInst.spriteDefinitions[_spriteId] : null;
		}
	}

	public Vector2 WorldCenter
	{
		get
		{
			return base.transform.position.XY() + GetRelativePositionFromAnchor(Anchor.MiddleCenter).Rotate(base.transform.eulerAngles.z);
		}
	}

	public Vector2 WorldTopCenter
	{
		get
		{
			return base.transform.position.XY() + GetRelativePositionFromAnchor(Anchor.UpperCenter).Rotate(base.transform.eulerAngles.z);
		}
	}

	public Vector2 WorldTopLeft
	{
		get
		{
			return base.transform.position.XY() + GetRelativePositionFromAnchor(Anchor.UpperLeft).Rotate(base.transform.eulerAngles.z);
		}
	}

	public Vector2 WorldTopRight
	{
		get
		{
			return base.transform.position.XY() + GetRelativePositionFromAnchor(Anchor.UpperRight).Rotate(base.transform.eulerAngles.z);
		}
	}

	public Vector2 WorldBottomLeft
	{
		get
		{
			return base.transform.position.XY() + GetRelativePositionFromAnchor(Anchor.LowerLeft).Rotate(base.transform.eulerAngles.z);
		}
	}

	public Vector2 WorldBottomCenter
	{
		get
		{
			return base.transform.position.XY() + GetRelativePositionFromAnchor(Anchor.LowerCenter).Rotate(base.transform.eulerAngles.z);
		}
	}

	public Vector2 WorldBottomRight
	{
		get
		{
			return base.transform.position.XY() + GetRelativePositionFromAnchor(Anchor.LowerRight).Rotate(base.transform.eulerAngles.z);
		}
	}

	public event Action<tk2dBaseSprite> SpriteChanged;

	private void InitInstance()
	{
		if (collectionInst == null && collection != null)
		{
			collectionInst = collection.inst;
		}
	}

	public void SetSprite(int newSpriteId)
	{
		spriteId = newSpriteId;
	}

	public bool SetSprite(string spriteName)
	{
		int spriteIdByName = collection.GetSpriteIdByName(spriteName, -1);
		if (spriteIdByName != -1)
		{
			SetSprite(spriteIdByName);
		}
		else
		{
			Debug.LogError("SetSprite - Sprite not found in collection: " + spriteName);
		}
		return spriteIdByName != -1;
	}

	public void SetSprite(tk2dSpriteCollectionData newCollection, int newSpriteId)
	{
		bool flag = false;
		if (Collection != newCollection)
		{
			collection = newCollection;
			collectionInst = collection.inst;
			_spriteId = -1;
			flag = true;
		}
		spriteId = newSpriteId;
		if (flag)
		{
			UpdateMaterial();
		}
	}

	public bool SetSprite(tk2dSpriteCollectionData newCollection, string spriteName)
	{
		int spriteIdByName = newCollection.GetSpriteIdByName(spriteName, -1);
		if (spriteIdByName != -1)
		{
			SetSprite(newCollection, spriteIdByName);
		}
		else
		{
			Debug.LogError("SetSprite - Sprite not found in collection: " + spriteName);
		}
		return spriteIdByName != -1;
	}

	public void MakePixelPerfect()
	{
		float num = 1f;
		tk2dCamera tk2dCamera2 = tk2dCamera.CameraForLayer(base.gameObject.layer);
		if (tk2dCamera2 != null)
		{
			if (Collection.version < 2)
			{
				Debug.LogError("Need to rebuild sprite collection.");
			}
			float distance = base.transform.position.z - tk2dCamera2.transform.position.z;
			float num2 = Collection.invOrthoSize * Collection.halfTargetHeight;
			num = tk2dCamera2.GetSizeAtDistance(distance) * num2;
		}
		else if ((bool)Camera.main)
		{
			if (Camera.main.orthographic)
			{
				num = Camera.main.orthographicSize;
			}
			else
			{
				float zdist = base.transform.position.z - Camera.main.transform.position.z;
				num = tk2dPixelPerfectHelper.CalculateScaleForPerspectiveCamera(Camera.main.fieldOfView, zdist);
			}
			num *= Collection.invOrthoSize;
		}
		else
		{
			Debug.LogError("Main camera not found.");
		}
		scale = new Vector3(Mathf.Sign(scale.x) * num, Mathf.Sign(scale.y) * num, Mathf.Sign(scale.z) * num);
	}

	public void ForceRotationRebuild()
	{
		if ((bool)m_transform && m_transform.rotation != m_cachedRotation)
		{
			Build();
			m_cachedRotation = m_transform.rotation;
		}
		if (attachedRenderers != null)
		{
			for (int i = 0; i < attachedRenderers.Count; i++)
			{
				attachedRenderers[i].ForceRotationRebuild();
			}
		}
	}

	protected abstract void UpdateMaterial();

	protected abstract void UpdateColors();

	protected abstract void UpdateVertices();

	protected abstract void UpdateGeometry();

	protected abstract int GetCurrentVertexCount();

	public void ForceUpdateMaterial()
	{
		if (!(base.renderer == null) && !(collectionInst == null) && base.renderer.sharedMaterial != collectionInst.spriteDefinitions[spriteId].materialInst)
		{
			base.renderer.material = collectionInst.spriteDefinitions[spriteId].materialInst;
		}
	}

	public abstract void Build();

	public int GetSpriteIdByName(string name)
	{
		InitInstance();
		return collectionInst.GetSpriteIdByName(name);
	}

	public static T AddComponent<T>(GameObject go, tk2dSpriteCollectionData spriteCollection, int spriteId) where T : tk2dBaseSprite
	{
		T val = go.AddComponent<T>();
		val._spriteId = -1;
		val.SetSprite(spriteCollection, spriteId);
		val.Build();
		return val;
	}

	public static T AddComponent<T>(GameObject go, tk2dSpriteCollectionData spriteCollection, string spriteName) where T : tk2dBaseSprite
	{
		int spriteIdByName = spriteCollection.GetSpriteIdByName(spriteName, -1);
		if (spriteIdByName == -1)
		{
			Debug.LogError(string.Format("Unable to find sprite named {0} in sprite collection {1}", spriteName, spriteCollection.spriteCollectionName));
			return (T)null;
		}
		return AddComponent<T>(go, spriteCollection, spriteIdByName);
	}

	protected int GetNumVertices()
	{
		InitInstance();
		return 4;
	}

	protected int GetNumIndices()
	{
		InitInstance();
		return collectionInst.spriteDefinitions[spriteId].indices.Length;
	}

	protected void SetPositions(Vector3[] positions, Vector3[] normals, Vector4[] tangents)
	{
		if (m_transform == null)
		{
			m_transform = base.transform;
		}
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[spriteId];
		int numVertices = GetNumVertices();
		positions[0] = Vector3.Scale(tk2dSpriteDefinition2.position0, _scale);
		positions[1] = Vector3.Scale(tk2dSpriteDefinition2.position1, _scale);
		positions[2] = Vector3.Scale(tk2dSpriteDefinition2.position2, _scale);
		positions[3] = Vector3.Scale(tk2dSpriteDefinition2.position3, _scale);
		if (!ShouldDoTilt)
		{
			return;
		}
		float num = 0f;
		for (int i = 0; i < numVertices; i++)
		{
			Vector3 a = positions[i];
			float y = (m_transform.rotation * Vector3.Scale(a, m_transform.lossyScale)).y;
			if (IsPerpendicular)
			{
				positions[i].z -= y;
				if (AdditionalPerpForwardPercentage > 0f)
				{
					positions[i].z -= y * AdditionalPerpForwardPercentage;
				}
				continue;
			}
			positions[i].z += y;
			if (AdditionalFlatForwardPercentage > 0f)
			{
				num = Mathf.Max(y * AdditionalFlatForwardPercentage, num);
				positions[i].z -= y * AdditionalFlatForwardPercentage;
			}
		}
		if (AdditionalFlatForwardPercentage > 0f)
		{
			for (int j = 0; j < numVertices; j++)
			{
				positions[j] += new Vector3(0f, 0f, num);
			}
		}
	}

	protected void SetColors(Color32[] dest)
	{
		Color color = _color;
		if (collectionInst.premultipliedAlpha)
		{
			color.r *= color.a;
			color.g *= color.a;
			color.b *= color.a;
		}
		Color32 color2 = color;
		int numVertices = GetNumVertices();
		for (int i = 0; i < numVertices; i++)
		{
			dest[i] = color2;
		}
	}

	public Bounds GetBounds()
	{
		InitInstance();
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[_spriteId];
		return new Bounds(new Vector3(tk2dSpriteDefinition2.boundsDataCenter.x * _scale.x, tk2dSpriteDefinition2.boundsDataCenter.y * _scale.y, tk2dSpriteDefinition2.boundsDataCenter.z * _scale.z), new Vector3(tk2dSpriteDefinition2.boundsDataExtents.x * Mathf.Abs(_scale.x), tk2dSpriteDefinition2.boundsDataExtents.y * Mathf.Abs(_scale.y), tk2dSpriteDefinition2.boundsDataExtents.z * Mathf.Abs(_scale.z)));
	}

	public Bounds GetUntrimmedBounds()
	{
		InitInstance();
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[_spriteId];
		return new Bounds(new Vector3(tk2dSpriteDefinition2.untrimmedBoundsDataCenter.x * _scale.x, tk2dSpriteDefinition2.untrimmedBoundsDataCenter.y * _scale.y, tk2dSpriteDefinition2.untrimmedBoundsDataCenter.z * _scale.z), new Vector3(tk2dSpriteDefinition2.untrimmedBoundsDataExtents.x * Mathf.Abs(_scale.x), tk2dSpriteDefinition2.untrimmedBoundsDataExtents.y * Mathf.Abs(_scale.y), tk2dSpriteDefinition2.untrimmedBoundsDataExtents.z * Mathf.Abs(_scale.z)));
	}

	public static Bounds AdjustedMeshBounds(Bounds bounds, int renderLayer)
	{
		Vector3 center = bounds.center;
		center.z = (float)(-renderLayer) * 0.01f;
		bounds.center = center;
		return bounds;
	}

	public tk2dSpriteDefinition GetCurrentSpriteDef()
	{
		InitInstance();
		return (!(collectionInst == null)) ? collectionInst.spriteDefinitions[_spriteId] : null;
	}

	public tk2dSpriteDefinition GetTrueCurrentSpriteDef()
	{
		if (hasOffScreenCachedUpdate)
		{
			return offScreenCachedCollection.spriteDefinitions[offScreenCachedID];
		}
		return GetCurrentSpriteDef();
	}

	public virtual void ReshapeBounds(Vector3 dMin, Vector3 dMax)
	{
	}

	protected virtual bool NeedBoxCollider()
	{
		return false;
	}

	protected virtual void UpdateCollider()
	{
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[_spriteId];
		if (tk2dSpriteDefinition2.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics3D || tk2dSpriteDefinition2.physicsEngine != tk2dSpriteDefinition.PhysicsEngine.Physics2D)
		{
			return;
		}
		if (tk2dSpriteDefinition2.colliderType == tk2dSpriteDefinition.ColliderType.Box)
		{
			if (boxCollider2D == null)
			{
				boxCollider2D = base.gameObject.GetComponent<BoxCollider2D>();
				if (boxCollider2D == null)
				{
					boxCollider2D = base.gameObject.AddComponent<BoxCollider2D>();
				}
			}
			if (!boxCollider2D.enabled)
			{
				boxCollider2D.enabled = true;
			}
			boxCollider2D.offset = new Vector2(tk2dSpriteDefinition2.colliderVertices[0].x * _scale.x, tk2dSpriteDefinition2.colliderVertices[0].y * _scale.y);
			boxCollider2D.size = new Vector2(Mathf.Abs(2f * tk2dSpriteDefinition2.colliderVertices[1].x * _scale.x), Mathf.Abs(2f * tk2dSpriteDefinition2.colliderVertices[1].y * _scale.y));
		}
		else if (tk2dSpriteDefinition2.colliderType == tk2dSpriteDefinition.ColliderType.Mesh)
		{
			Debug.LogError("BraveTK2D does not support mesh colliders.");
		}
		else if (tk2dSpriteDefinition2.colliderType == tk2dSpriteDefinition.ColliderType.None && boxCollider2D != null && boxCollider2D.enabled)
		{
			boxCollider2D.enabled = false;
		}
	}

	protected virtual void CreateCollider()
	{
		tk2dSpriteDefinition tk2dSpriteDefinition2 = collectionInst.spriteDefinitions[_spriteId];
		if (tk2dSpriteDefinition2.colliderType != 0 && tk2dSpriteDefinition2.physicsEngine != 0 && tk2dSpriteDefinition2.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics2D)
		{
			UpdateCollider();
		}
	}

	protected void Awake()
	{
		if (collection != null)
		{
			collectionInst = collection.inst;
		}
		CachedRenderer.sortingOrder = renderLayer;
		m_cachedStartingSpriteID = _spriteId;
		m_transform = base.transform;
		m_renderer = GetComponent<MeshRenderer>();
		m_cachedYPosition = m_transform.position.y;
		m_cachedAnimator = GetComponent<tk2dSpriteAnimator>();
		if (attachParent != null)
		{
			automaticallyManagesDepth = false;
			attachParent.AttachRenderer(this);
		}
		bool flag = base.gameObject.layer == 28;
		if (!allowDefaultLayer)
		{
			if (m_renderer.sortingLayerName == "Default" || m_renderer.sortingLayerID == 0)
			{
				renderLayer = 2;
				DepthLookupManager.ProcessRenderer(m_renderer);
			}
			if (base.gameObject.layer < 13 || base.gameObject.layer > 26)
			{
				base.gameObject.layer = 22;
			}
		}
		if (flag || Pixelator.IsValidReflectionObject(this))
		{
			base.gameObject.layer = 28;
		}
		m_cachedScale = scale;
		if (automaticallyManagesDepth)
		{
			UpdateZDepth();
		}
		m_cachedRotation = m_transform.rotation;
	}

	public void OnSpawned()
	{
		m_transform = base.transform;
		m_cachedYPosition = m_transform.position.y;
		if (attachParent != null)
		{
			automaticallyManagesDepth = false;
			attachParent.AttachRenderer(this);
		}
		if (automaticallyManagesDepth)
		{
			UpdateZDepth();
		}
		m_cachedRotation = m_transform.rotation;
	}

	public void OnDespawned()
	{
		scale = m_cachedScale;
	}

	public void CreateSimpleBoxCollider()
	{
		if (CurrentSprite == null)
		{
			return;
		}
		if (CurrentSprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics3D)
		{
			boxCollider2D = GetComponent<BoxCollider2D>();
			if (boxCollider2D != null)
			{
				UnityEngine.Object.DestroyImmediate(boxCollider2D, true);
			}
			boxCollider = GetComponent<BoxCollider>();
			if (boxCollider == null)
			{
				boxCollider = base.gameObject.AddComponent<BoxCollider>();
			}
		}
		else if (CurrentSprite.physicsEngine == tk2dSpriteDefinition.PhysicsEngine.Physics2D)
		{
			boxCollider = GetComponent<BoxCollider>();
			if (boxCollider != null)
			{
				UnityEngine.Object.DestroyImmediate(boxCollider, true);
			}
			boxCollider2D = GetComponent<BoxCollider2D>();
			if (boxCollider2D == null)
			{
				boxCollider2D = base.gameObject.AddComponent<BoxCollider2D>();
			}
		}
	}

	public bool UsesSpriteCollection(tk2dSpriteCollectionData spriteCollection)
	{
		return Collection == spriteCollection;
	}

	public virtual void ForceBuild()
	{
		if ((bool)this && !(collection == null))
		{
			collectionInst = collection.inst;
			if (spriteId < 0 || spriteId >= collectionInst.spriteDefinitions.Length)
			{
				spriteId = 0;
			}
			Build();
			if (this.SpriteChanged != null)
			{
				this.SpriteChanged(this);
			}
		}
	}

	public static GameObject CreateFromTexture<T>(Texture texture, tk2dSpriteCollectionSize size, Rect region, Vector2 anchor) where T : tk2dBaseSprite
	{
		tk2dSpriteCollectionData tk2dSpriteCollectionData2 = SpriteCollectionGenerator.CreateFromTexture(texture, size, region, anchor);
		if (tk2dSpriteCollectionData2 == null)
		{
			return null;
		}
		GameObject gameObject = new GameObject();
		AddComponent<T>(gameObject, tk2dSpriteCollectionData2, 0);
		return gameObject;
	}

	public IntVector2 GetAnchorPixelOffset()
	{
		return -PhysicsEngine.UnitToPixel(GetUntrimmedBounds().min);
	}

	public Vector2 GetRelativePositionFromAnchor(Anchor anchor)
	{
		Bounds bounds = GetBounds();
		Vector2 result = bounds.min;
		switch (anchor)
		{
		case Anchor.LowerCenter:
		case Anchor.MiddleCenter:
		case Anchor.UpperCenter:
			result.x += bounds.extents.x;
			break;
		case Anchor.LowerRight:
		case Anchor.MiddleRight:
		case Anchor.UpperRight:
			result.x += bounds.extents.x * 2f;
			break;
		}
		switch (anchor)
		{
		case Anchor.UpperLeft:
		case Anchor.UpperCenter:
		case Anchor.UpperRight:
			result.y += bounds.extents.y * 2f;
			break;
		case Anchor.MiddleLeft:
		case Anchor.MiddleCenter:
		case Anchor.MiddleRight:
			result.y += bounds.extents.y;
			break;
		}
		return result;
	}

	public void PlayEffectOnSprite(GameObject effect, Vector3 offset, bool attached = true)
	{
		if (!(effect == null))
		{
			GameObject gameObject = SpawnManager.SpawnVFX(effect);
			tk2dBaseSprite component = gameObject.GetComponent<tk2dBaseSprite>();
			component.PlaceAtPositionByAnchor(WorldCenter.ToVector3ZUp() + offset, Anchor.MiddleCenter);
			if (attached)
			{
				gameObject.transform.parent = base.transform;
				component.HeightOffGround = 0.2f;
				AttachRenderer(component);
			}
		}
	}

	public void PlaceAtPositionByAnchor(Vector3 position, Anchor anchor)
	{
		Vector2 relativePositionFromAnchor = GetRelativePositionFromAnchor(anchor);
		m_transform.position = position - relativePositionFromAnchor.ToVector3ZUp();
	}

	public void PlaceAtLocalPositionByAnchor(Vector3 position, Anchor anchor)
	{
		Vector2 relativePositionFromAnchor = GetRelativePositionFromAnchor(anchor);
		m_transform.localPosition = position - relativePositionFromAnchor.ToVector3ZUp();
	}

	public void AttachRenderer(tk2dBaseSprite attachment)
	{
		if (attachedRenderers == null)
		{
			attachedRenderers = new List<tk2dBaseSprite>();
		}
		if (!attachedRenderers.Contains(attachment))
		{
			attachment.attachParent = this;
			if (!attachment.independentOrientation)
			{
				attachment.IsPerpendicular = IsPerpendicular;
			}
			attachedRenderers.Add(attachment);
		}
	}

	public void DetachRenderer(tk2dBaseSprite attachment)
	{
		if (attachedRenderers != null && attachedRenderers.Contains(attachment))
		{
			if (attachment is tk2dSprite)
			{
				(attachment as tk2dSprite).attachParent = null;
			}
			attachedRenderers.Remove(attachment);
		}
	}

	public void ForceBuildWithAttached()
	{
		ForceBuild();
		if (attachedRenderers == null || attachedRenderers.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < attachedRenderers.Count; i++)
		{
			if (attachedRenderers[i] is tk2dSprite)
			{
				(attachedRenderers[i] as tk2dSprite).ForceBuildWithAttached();
			}
			else
			{
				attachedRenderers[i].ForceBuild();
			}
		}
	}

	public void UpdateZDepthAttached(float parentDepth, float parentWorldY, bool parentPerpendicular)
	{
		float num = parentDepth - HeightOffGround;
		float num2 = m_transform.position.y - parentWorldY;
		num = ((!parentPerpendicular) ? (num + num2) : (num - num2));
		UpdateZDepthInternal(num, m_transform.position.y);
	}

	public void StackTraceAttachment()
	{
		if (attachedRenderers == null)
		{
			Debug.Log(base.name + " has no children.");
			return;
		}
		string text = base.name + " parent of: ";
		for (int i = 0; i < attachedRenderers.Count; i++)
		{
			text = text + attachedRenderers[i].name + " ";
		}
		Debug.Log(text);
		for (int j = 0; j < attachedRenderers.Count; j++)
		{
			attachedRenderers[j].StackTraceAttachment();
		}
	}

	public void UpdateZDepthLater()
	{
		IsZDepthDirty = true;
	}

	public void UpdateZDepth()
	{
		IsZDepthDirty = false;
		if (ignoresTiltworldDepth)
		{
			return;
		}
		if (attachParent != null)
		{
			attachParent.UpdateZDepth();
			return;
		}
		if (m_transform == null && (bool)this)
		{
			m_transform = base.transform;
		}
		if (!(collectionInst == null) && collectionInst.spriteDefinitions != null && (bool)m_transform)
		{
			float y = m_transform.position.y;
			float num;
			if (depthUsesTrimmedBounds)
			{
				float y2 = GetBounds().min.y;
				num = y + y2 + ((!IsPerpendicular) ? (0f - y2) : y2);
			}
			else
			{
				num = y;
			}
			float targetZValue = num - HeightOffGround;
			UpdateZDepthInternal(targetZValue, y);
		}
	}

	protected void UpdateZDepthInternal(float targetZValue, float currentYValue)
	{
		IsZDepthDirty = false;
		Vector3 position = m_transform.position;
		if (position.z != targetZValue)
		{
			position.z = targetZValue;
			m_transform.position = position;
		}
		if (attachedRenderers == null || attachedRenderers.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < attachedRenderers.Count; i++)
		{
			tk2dBaseSprite tk2dBaseSprite2 = attachedRenderers[i];
			if (!tk2dBaseSprite2 || tk2dBaseSprite2.attachParent != this)
			{
				attachedRenderers.RemoveAt(i);
				i--;
				continue;
			}
			if ((object)tk2dBaseSprite2 != null)
			{
				tk2dBaseSprite2.UpdateZDepthAttached(targetZValue, currentYValue, IsPerpendicular);
			}
			if (!tk2dBaseSprite2.independentOrientation)
			{
				bool isPerpendicular = IsPerpendicular;
				if (isPerpendicular && !tk2dBaseSprite2.IsPerpendicular)
				{
					tk2dBaseSprite2.IsPerpendicular = true;
				}
				if (!isPerpendicular && tk2dBaseSprite2.IsPerpendicular)
				{
					tk2dBaseSprite2.IsPerpendicular = false;
				}
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
