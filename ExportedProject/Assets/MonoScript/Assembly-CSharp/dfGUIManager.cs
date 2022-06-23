using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[RequireComponent(typeof(BoxCollider))]
[ExecuteInEditMode]
[AddComponentMenu("Daikon Forge/User Interface/GUI Manager")]
[RequireComponent(typeof(dfInputManager))]
public class dfGUIManager : MonoBehaviour, IDFControlHost, IComparable<dfGUIManager>
{
	[dfEventCategory("Modal Dialog")]
	public delegate void ModalPoppedCallback(dfControl control);

	[dfEventCategory("Global Callbacks")]
	public delegate void RenderCallback(dfGUIManager manager);

	private struct ModalControlReference
	{
		public dfControl control;

		public ModalPoppedCallback callback;
	}

	[SerializeField]
	public CameraClearFlags overrideClearFlags = CameraClearFlags.Depth;

	[SerializeField]
	protected float uiScale = 1f;

	[SerializeField]
	public Vector2 InputOffsetScreenPercent;

	[SerializeField]
	protected bool uiScaleLegacy = true;

	[SerializeField]
	protected dfInputManager inputManager;

	[SerializeField]
	protected int fixedWidth = -1;

	[SerializeField]
	protected int fixedHeight = 600;

	[SerializeField]
	public bool FixedAspect;

	[SerializeField]
	protected dfAtlas atlas;

	[SerializeField]
	protected dfFontBase defaultFont;

	[SerializeField]
	protected bool mergeMaterials;

	[SerializeField]
	protected bool pixelPerfectMode = true;

	[SerializeField]
	protected Camera renderCamera;

	[SerializeField]
	protected bool generateNormals;

	[SerializeField]
	protected bool consumeMouseEvents;

	[SerializeField]
	protected bool overrideCamera;

	[SerializeField]
	protected int renderQueueBase = 3000;

	[SerializeField]
	public int renderQueueSecondDraw = -1;

	[SerializeField]
	public List<dfDesignGuide> guides = new List<dfDesignGuide>();

	private static List<dfGUIManager> activeInstances = new List<dfGUIManager>();

	private static dfControl activeControl = null;

	private static Stack<ModalControlReference> modalControlStack = new Stack<ModalControlReference>();

	private dfGUICamera guiCamera;

	private Mesh[] renderMesh;

	private MeshFilter renderFilter;

	private MeshRenderer meshRenderer;

	private int activeRenderMesh;

	private int cachedChildCount;

	private bool isDirty;

	private bool abortRendering;

	private Vector2 cachedScreenSize;

	private Vector3[] corners = new Vector3[4];

	private dfList<Rect> occluders = new dfList<Rect>(256);

	private Stack<dfTriangleClippingRegion> clipStack = new Stack<dfTriangleClippingRegion>();

	private static dfRenderData masterBuffer = new dfRenderData(4096);

	private dfList<dfRenderData> drawCallBuffers = new dfList<dfRenderData>();

	private dfList<dfRenderGroup> renderGroups = new dfList<dfRenderGroup>();

	private List<int> submeshes = new List<int>();

	private int drawCallCount;

	private Vector2 uiOffset = Vector2.zero;

	private static Plane[] clippingPlanes;

	private dfList<int> drawCallIndices = new dfList<int>();

	private dfList<dfControl> controlsRendered = new dfList<dfControl>();

	private bool shutdownInProcess;

	private int suspendCount;

	private bool? applyHalfPixelOffset;

	[NonSerialized]
	public bool ResolutionIsChanging;

	public MeshRenderer MeshRenderer
	{
		get
		{
			return meshRenderer;
		}
	}

	public static IEnumerable<dfGUIManager> ActiveManagers
	{
		get
		{
			return activeInstances;
		}
	}

	public int TotalDrawCalls { get; private set; }

	public int TotalTriangles { get; private set; }

	public int NumControlsRendered { get; private set; }

	public int FramesRendered { get; private set; }

	public IList<dfControl> ControlsRendered
	{
		get
		{
			return controlsRendered;
		}
	}

	public IList<int> DrawCallStartIndices
	{
		get
		{
			return drawCallIndices;
		}
	}

	public int RenderQueueBase
	{
		get
		{
			return renderQueueBase;
		}
		set
		{
			if (value != renderQueueBase)
			{
				renderQueueBase = value;
				RefreshAll();
			}
		}
	}

	public static dfControl ActiveControl
	{
		get
		{
			return activeControl;
		}
	}

	public float UIScale
	{
		get
		{
			return uiScale;
		}
		set
		{
			if (!Mathf.Approximately(value, uiScale))
			{
				uiScale = value;
				onResolutionChanged();
			}
		}
	}

	public bool UIScaleLegacyMode
	{
		get
		{
			return uiScaleLegacy;
		}
		set
		{
			if (value != uiScaleLegacy)
			{
				uiScaleLegacy = value;
				onResolutionChanged();
			}
		}
	}

	public Vector2 UIOffset
	{
		get
		{
			return uiOffset;
		}
		set
		{
			if (!object.Equals(uiOffset, value))
			{
				uiOffset = value;
				Invalidate();
			}
		}
	}

	public Camera RenderCamera
	{
		get
		{
			return renderCamera;
		}
		set
		{
			if (!object.ReferenceEquals(renderCamera, value))
			{
				renderCamera = value;
				Invalidate();
				if (value != null && value.gameObject.GetComponent<dfGUICamera>() == null)
				{
					value.gameObject.AddComponent<dfGUICamera>();
				}
				if (inputManager != null)
				{
					inputManager.RenderCamera = value;
				}
			}
		}
	}

	public bool MergeMaterials
	{
		get
		{
			return mergeMaterials;
		}
		set
		{
			if (value != mergeMaterials)
			{
				mergeMaterials = value;
				invalidateAllControls();
			}
		}
	}

	public bool GenerateNormals
	{
		get
		{
			return generateNormals;
		}
		set
		{
			if (value != generateNormals)
			{
				generateNormals = value;
				if (renderMesh != null)
				{
					renderMesh[0].Clear();
					renderMesh[1].Clear();
				}
				dfRenderData.FlushObjectPool();
				invalidateAllControls();
			}
		}
	}

	public bool PixelPerfectMode
	{
		get
		{
			return pixelPerfectMode;
		}
		set
		{
			if (value != pixelPerfectMode)
			{
				pixelPerfectMode = value;
				onResolutionChanged();
				Invalidate();
			}
		}
	}

	public dfAtlas DefaultAtlas
	{
		get
		{
			return atlas;
		}
		set
		{
			if (!dfAtlas.Equals(value, atlas))
			{
				atlas = value;
				invalidateAllControls();
			}
		}
	}

	public dfFontBase DefaultFont
	{
		get
		{
			return defaultFont;
		}
		set
		{
			if (value != defaultFont)
			{
				defaultFont = value;
				invalidateAllControls();
			}
		}
	}

	public int FixedWidth
	{
		get
		{
			return fixedWidth;
		}
		set
		{
			if (value != fixedWidth)
			{
				fixedWidth = value;
				onResolutionChanged();
			}
		}
	}

	public int FixedHeight
	{
		get
		{
			return fixedHeight;
		}
		set
		{
			if (value != fixedHeight)
			{
				int oldSize = fixedHeight;
				fixedHeight = value;
				onResolutionChanged(oldSize, value);
			}
		}
	}

	public bool ConsumeMouseEvents
	{
		get
		{
			return consumeMouseEvents;
		}
		set
		{
			consumeMouseEvents = value;
		}
	}

	public bool OverrideCamera
	{
		get
		{
			return overrideCamera;
		}
		set
		{
			overrideCamera = value;
		}
	}

	private float RenderAspect
	{
		get
		{
			return (!FixedAspect) ? RenderCamera.aspect : 1.77777779f;
		}
	}

	public static event RenderCallback BeforeRender;

	public static event RenderCallback AfterRender;

	public void OnApplicationQuit()
	{
		shutdownInProcess = true;
	}

	public void Awake()
	{
		dfRenderData.FlushObjectPool();
	}

	public void OnEnable()
	{
		Camera[] allCameras = Camera.allCameras;
		for (int i = 0; i < allCameras.Length; i++)
		{
			allCameras[i].eventMask &= ~(1 << base.gameObject.layer);
		}
		if (meshRenderer == null)
		{
			initialize();
		}
		base.useGUILayout = !ConsumeMouseEvents;
		activeInstances.Add(this);
		FramesRendered = 0;
		TotalDrawCalls = 0;
		TotalTriangles = 0;
		if (meshRenderer != null)
		{
			meshRenderer.enabled = true;
		}
		inputManager = GetComponent<dfInputManager>() ?? base.gameObject.AddComponent<dfInputManager>();
		inputManager.RenderCamera = RenderCamera;
		FramesRendered = 0;
		if (meshRenderer != null)
		{
			meshRenderer.enabled = true;
		}
		if (Application.isPlaying)
		{
			onResolutionChanged();
		}
		Invalidate();
	}

	public void OnDisable()
	{
		activeInstances.Remove(this);
		if (meshRenderer != null)
		{
			meshRenderer.enabled = false;
		}
		resetDrawCalls();
	}

	public void OnDestroy()
	{
		if (activeInstances.Count == 0)
		{
			dfMaterialCache.Clear();
		}
		if (renderMesh != null && !(renderFilter == null))
		{
			renderFilter.sharedMesh = null;
			UnityEngine.Object.DestroyImmediate(renderMesh[0]);
			UnityEngine.Object.DestroyImmediate(renderMesh[1]);
			renderMesh = null;
		}
	}

	public void Start()
	{
		Camera[] allCameras = Camera.allCameras;
		for (int i = 0; i < allCameras.Length; i++)
		{
			allCameras[i].eventMask &= ~(1 << base.gameObject.layer);
		}
	}

	public void Update()
	{
		activeInstances.Sort();
		if (renderCamera == null || !base.enabled)
		{
			if (meshRenderer != null)
			{
				meshRenderer.enabled = false;
			}
			return;
		}
		if (renderMesh == null || renderMesh.Length == 0)
		{
			initialize();
			if (Application.isEditor && !Application.isPlaying)
			{
				Render();
			}
		}
		if (cachedChildCount != base.transform.childCount)
		{
			cachedChildCount = base.transform.childCount;
			Invalidate();
		}
		Vector2 screenSize = GetScreenSize();
		if ((screenSize - cachedScreenSize).sqrMagnitude > float.Epsilon)
		{
			onResolutionChanged(cachedScreenSize, screenSize);
			cachedScreenSize = screenSize;
		}
	}

	public void LateUpdate()
	{
		if (renderMesh == null || renderMesh.Length == 0)
		{
			initialize();
		}
		if (!Application.isPlaying)
		{
			BoxCollider boxCollider = GetComponent<Collider>() as BoxCollider;
			if (boxCollider != null)
			{
				Vector2 vector = GetScreenSize() * PixelsToUnits();
				boxCollider.center = Vector3.zero;
				boxCollider.size = vector;
			}
		}
		if (!(activeInstances[0] == this))
		{
			return;
		}
		dfFontManager.RebuildDynamicFonts();
		bool flag = false;
		for (int i = 0; i < activeInstances.Count; i++)
		{
			dfGUIManager dfGUIManager2 = activeInstances[i];
			if (dfGUIManager2.isDirty && dfGUIManager2.suspendCount <= 0)
			{
				flag = true;
				dfGUIManager2.abortRendering = false;
				dfGUIManager2.isDirty = false;
				dfGUIManager2.Render();
			}
		}
		if (flag)
		{
			dfMaterialCache.Reset();
			updateDrawCalls();
			for (int j = 0; j < activeInstances.Count; j++)
			{
				activeInstances[j].updateDrawCalls();
			}
		}
	}

	public void SuspendRendering()
	{
		suspendCount++;
	}

	public void ResumeRendering()
	{
		if (suspendCount != 0 && --suspendCount == 0)
		{
			Invalidate();
		}
	}

	public static dfControl HitTestAll(Vector2 screenPosition)
	{
		dfControl result = null;
		float num = float.MinValue;
		for (int i = 0; i < activeInstances.Count; i++)
		{
			if (!activeInstances[i].GetComponent<dfInputManager>().enabled)
			{
				continue;
			}
			dfGUIManager dfGUIManager2 = activeInstances[i];
			Camera camera = dfGUIManager2.RenderCamera;
			if (!(camera.depth < num))
			{
				dfControl dfControl2 = dfGUIManager2.HitTest(screenPosition);
				if (dfControl2 != null)
				{
					result = dfControl2;
					num = camera.depth;
				}
			}
		}
		return result;
	}

	public dfControl HitTest(Vector2 screenPosition)
	{
		Vector2 vector = screenPosition;
		Ray ray = renderCamera.ScreenPointToRay(vector);
		float maxDistance = renderCamera.farClipPlane - renderCamera.nearClipPlane;
		dfControl modalControl = GetModalControl();
		dfList<dfControl> dfList2 = controlsRendered;
		int count = dfList2.Count;
		dfControl[] items = dfList2.Items;
		if (occluders.Count != count)
		{
			Debug.LogWarning("Occluder count does not match control count");
			return null;
		}
		Vector2 vector2 = vector;
		vector2.y = (float)RenderCamera.pixelHeight / RenderCamera.rect.height - vector.y;
		for (int num = count - 1; num >= 0; num--)
		{
			dfControl dfControl2 = items[num];
			RaycastHit hitInfo;
			if (!(dfControl2 == null) && !(dfControl2.GetComponent<Collider>() == null) && dfControl2.GetComponent<Collider>().Raycast(ray, out hitInfo, maxDistance) && (!(modalControl != null) || dfControl2.transform.IsChildOf(modalControl.transform)) && dfControl2.IsInteractive && dfControl2.IsEnabled && isInsideClippingRegion(hitInfo.point, dfControl2))
			{
				return dfControl2;
			}
		}
		return null;
	}

	public Vector2 WorldPointToGUI(Vector3 worldPoint)
	{
		Camera camera = Camera.main ?? renderCamera;
		return ScreenToGui(camera.WorldToScreenPoint(worldPoint));
	}

	public float PixelsToUnits()
	{
		float num = 2f / (float)FixedHeight;
		return num * UIScale;
	}

	public Plane[] GetClippingPlanes()
	{
		Vector3[] array = GetCorners();
		Vector3 inNormal = base.transform.TransformDirection(Vector3.right);
		Vector3 inNormal2 = base.transform.TransformDirection(Vector3.left);
		Vector3 inNormal3 = base.transform.TransformDirection(Vector3.up);
		Vector3 inNormal4 = base.transform.TransformDirection(Vector3.down);
		if (clippingPlanes == null)
		{
			clippingPlanes = new Plane[4];
		}
		clippingPlanes[0] = new Plane(inNormal, array[0]);
		clippingPlanes[1] = new Plane(inNormal2, array[1]);
		clippingPlanes[2] = new Plane(inNormal3, array[2]);
		clippingPlanes[3] = new Plane(inNormal4, array[0]);
		return clippingPlanes;
	}

	public Vector3[] GetCorners()
	{
		float num = PixelsToUnits();
		Vector2 vector = GetScreenSize() * num;
		float x = vector.x;
		float y = vector.y;
		Vector3 vector2 = new Vector3((0f - x) * 0.5f, y * 0.5f);
		Vector3 vector3 = vector2 + new Vector3(x, 0f);
		Vector3 point = vector2 + new Vector3(0f, 0f - y);
		Vector3 point2 = vector3 + new Vector3(0f, 0f - y);
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		corners[0] = localToWorldMatrix.MultiplyPoint(vector2);
		corners[1] = localToWorldMatrix.MultiplyPoint(vector3);
		corners[2] = localToWorldMatrix.MultiplyPoint(point2);
		corners[3] = localToWorldMatrix.MultiplyPoint(point);
		return corners;
	}

	public Vector2 GetScreenSize()
	{
		Camera camera = RenderCamera;
		bool flag = Application.isPlaying && camera != null;
		Vector2 zero = Vector2.zero;
		if (flag)
		{
			float num = ((!PixelPerfectMode) ? ((float)camera.pixelHeight / (float)fixedHeight) : 1f);
			if (guiCamera == null)
			{
				guiCamera = camera.GetComponent<dfGUICamera>();
			}
			zero = (new Vector2(camera.pixelWidth, camera.pixelHeight) / num).CeilToInt();
			if (!guiCamera.MaintainCameraAspect)
			{
			}
			if (uiScaleLegacy)
			{
				zero *= uiScale;
			}
			else
			{
				zero /= uiScale;
			}
		}
		else
		{
			zero = new Vector2(FixedWidth, FixedHeight);
			if (!uiScaleLegacy)
			{
				zero /= uiScale;
			}
		}
		return zero;
	}

	public T AddControl<T>() where T : dfControl
	{
		return (T)AddControl(typeof(T));
	}

	public dfControl AddControl(Type controlType)
	{
		if (!typeof(dfControl).IsAssignableFrom(controlType))
		{
			throw new InvalidCastException();
		}
		GameObject gameObject = new GameObject(controlType.Name);
		gameObject.transform.parent = base.transform;
		gameObject.layer = base.gameObject.layer;
		dfControl dfControl2 = gameObject.AddComponent(controlType) as dfControl;
		dfControl2.ZOrder = getMaxZOrder() + 1;
		return dfControl2;
	}

	public void AddControl(dfControl child)
	{
		child.transform.parent = base.transform;
	}

	public dfControl AddPrefab(GameObject prefab)
	{
		if (prefab.GetComponent<dfControl>() == null)
		{
			throw new InvalidCastException();
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(prefab);
		gameObject.transform.parent = base.transform;
		gameObject.layer = base.gameObject.layer;
		dfControl component = gameObject.GetComponent<dfControl>();
		component.transform.parent = base.transform;
		component.PerformLayout();
		BringToFront(component);
		return component;
	}

	public dfRenderData GetDrawCallBuffer(int drawCallNumber)
	{
		return drawCallBuffers[drawCallNumber];
	}

	public static dfControl GetModalControl()
	{
		return (modalControlStack.Count <= 0) ? null : modalControlStack.Peek().control;
	}

	public Vector2 ScreenToGui(Vector2 position)
	{
		Vector2 screenSize = GetScreenSize();
		Camera camera = GameManager.Instance.MainCameraController.Camera ?? renderCamera;
		position.x = (float)(camera.pixelWidth / Screen.width) * position.x;
		position.y = (float)(camera.pixelHeight / Screen.height) * position.y;
		position.y = screenSize.y - position.y;
		return position;
	}

	public static void PushModal(dfControl control)
	{
		PushModal(control, null);
	}

	public static void PushModal(dfControl control, ModalPoppedCallback callback)
	{
		if (control == null)
		{
			throw new NullReferenceException("Cannot call PushModal() with a null reference");
		}
		modalControlStack.Push(new ModalControlReference
		{
			control = control,
			callback = callback
		});
	}

	public static void PopModal()
	{
		if (modalControlStack.Count == 0)
		{
			throw new InvalidOperationException("Modal stack is empty");
		}
		ModalControlReference modalControlReference = modalControlStack.Pop();
		if (modalControlReference.callback != null)
		{
			modalControlReference.callback(modalControlReference.control);
		}
	}

	public static bool ModalStackContainsControl(dfControl control)
	{
		ModalControlReference[] array = modalControlStack.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].control == control)
			{
				return true;
			}
		}
		return false;
	}

	public static void PopModalToControl(dfControl control, bool includeControl)
	{
		while (modalControlStack.Count > 0)
		{
			if (modalControlStack.Peek().control == control)
			{
				if (includeControl)
				{
					modalControlStack.Pop();
				}
				break;
			}
			modalControlStack.Pop();
		}
	}

	public static void SetFocus(dfControl control, bool allowScrolling = true)
	{
		if (activeControl == control || (control != null && !control.CanFocus))
		{
			return;
		}
		dfControl dfControl2 = activeControl;
		activeControl = control;
		dfFocusEventArgs args = new dfFocusEventArgs(control, dfControl2, allowScrolling);
		dfList<dfControl> prevFocusChain = dfList<dfControl>.Obtain();
		if (dfControl2 != null)
		{
			dfControl dfControl3 = dfControl2;
			while (dfControl3 != null)
			{
				prevFocusChain.Add(dfControl3);
				dfControl3 = dfControl3.Parent;
			}
		}
		dfList<dfControl> newFocusChain = dfList<dfControl>.Obtain();
		if (control != null)
		{
			dfControl dfControl4 = control;
			while (dfControl4 != null)
			{
				newFocusChain.Add(dfControl4);
				dfControl4 = dfControl4.Parent;
			}
		}
		if (dfControl2 != null)
		{
			prevFocusChain.ForEach(delegate(dfControl c)
			{
				if (!newFocusChain.Contains(c))
				{
					c.OnLeaveFocus(args);
				}
			});
			dfControl2.OnLostFocus(args);
		}
		if (control != null)
		{
			newFocusChain.ForEach(delegate(dfControl c)
			{
				if (!prevFocusChain.Contains(c))
				{
					c.OnEnterFocus(args);
				}
			});
			control.OnGotFocus(args);
		}
		newFocusChain.Release();
		prevFocusChain.Release();
	}

	public static bool HasFocus(dfControl control)
	{
		if (control == null)
		{
			return false;
		}
		return activeControl == control;
	}

	public static bool ContainsFocus(dfControl control)
	{
		if (activeControl == control)
		{
			return true;
		}
		if (activeControl == null || control == null)
		{
			return object.ReferenceEquals(activeControl, control);
		}
		return activeControl.transform.IsChildOf(control.transform);
	}

	public void BringToFront(dfControl control)
	{
		if (control.Parent != null)
		{
			control = control.GetRootContainer();
		}
		using (dfList<dfControl> dfList2 = getTopLevelControls())
		{
			int zOrder = 0;
			for (int i = 0; i < dfList2.Count; i++)
			{
				dfControl dfControl2 = dfList2[i];
				if (dfControl2 != control)
				{
					dfControl2.ZOrder = zOrder++;
				}
			}
			control.ZOrder = zOrder;
			Invalidate();
		}
	}

	public void SendToBack(dfControl control)
	{
		if (control.Parent != null)
		{
			control = control.GetRootContainer();
		}
		using (dfList<dfControl> dfList2 = getTopLevelControls())
		{
			int num = 1;
			for (int i = 0; i < dfList2.Count; i++)
			{
				dfControl dfControl2 = dfList2[i];
				if (dfControl2 != control)
				{
					dfControl2.ZOrder = num++;
				}
			}
			control.ZOrder = 0;
			Invalidate();
		}
	}

	public void Invalidate()
	{
		if (!isDirty)
		{
			isDirty = true;
			updateRenderSettings();
		}
	}

	public static void InvalidateAll()
	{
		for (int i = 0; i < activeInstances.Count; i++)
		{
			activeInstances[i].Invalidate();
		}
	}

	public static void RefreshAll()
	{
		RefreshAll(false);
	}

	public static void RefreshAll(bool force)
	{
		List<dfGUIManager> list = activeInstances;
		for (int i = 0; i < list.Count; i++)
		{
			dfGUIManager dfGUIManager2 = list[i];
			if (dfGUIManager2.renderMesh != null && dfGUIManager2.renderMesh.Length != 0)
			{
				dfGUIManager2.invalidateAllControls();
				if (force || !Application.isPlaying)
				{
					dfGUIManager2.Render();
				}
			}
		}
	}

	internal void AbortRender()
	{
		abortRendering = true;
	}

	public void Render()
	{
		if (meshRenderer == null)
		{
			return;
		}
		meshRenderer.enabled = false;
		FramesRendered++;
		if (dfGUIManager.BeforeRender != null)
		{
			dfGUIManager.BeforeRender(this);
		}
		try
		{
			occluders.Clear();
			occluders.EnsureCapacity(NumControlsRendered);
			NumControlsRendered = 0;
			controlsRendered.Clear();
			drawCallIndices.Clear();
			renderGroups.Clear();
			TotalDrawCalls = 0;
			TotalTriangles = 0;
			if (RenderCamera == null || !base.enabled)
			{
				if (meshRenderer != null)
				{
					meshRenderer.enabled = false;
				}
				return;
			}
			if (meshRenderer != null && !meshRenderer.enabled)
			{
				meshRenderer.enabled = true;
			}
			if (renderMesh == null || renderMesh.Length == 0)
			{
				Debug.LogError("GUI Manager not initialized before Render() called");
				return;
			}
			resetDrawCalls();
			dfRenderData buffer = null;
			clipStack.Clear();
			clipStack.Push(dfTriangleClippingRegion.Obtain());
			uint sTART_VALUE = dfChecksumUtil.START_VALUE;
			using (dfList<dfControl> dfList2 = getTopLevelControls())
			{
				updateRenderOrder(dfList2);
				for (int i = 0; i < dfList2.Count; i++)
				{
					if (abortRendering)
					{
						break;
					}
					dfControl control = dfList2[i];
					renderControl(ref buffer, control, sTART_VALUE, 1f);
				}
			}
			if (abortRendering)
			{
				clipStack.Clear();
				throw new dfAbortRenderingException();
			}
			drawCallBuffers.RemoveAll(isEmptyBuffer);
			drawCallCount = drawCallBuffers.Count;
			TotalDrawCalls = drawCallCount;
			if (drawCallBuffers.Count == 0)
			{
				if (renderFilter.sharedMesh != null)
				{
					renderFilter.sharedMesh.Clear();
				}
				if (clipStack.Count > 0)
				{
					clipStack.Pop().Release();
					clipStack.Clear();
				}
				return;
			}
			dfRenderData dfRenderData2 = compileMasterBuffer();
			TotalTriangles = dfRenderData2.Triangles.Count / 3;
			Mesh mesh = getRenderMesh();
			renderFilter.sharedMesh = mesh;
			Mesh mesh2 = mesh;
			mesh2.Clear(true);
			mesh2.vertices = dfRenderData2.Vertices.Items;
			mesh2.uv = dfRenderData2.UV.Items;
			mesh2.colors32 = dfRenderData2.Colors.Items;
			if (generateNormals && dfRenderData2.Normals.Items.Length == dfRenderData2.Vertices.Items.Length)
			{
				mesh2.normals = dfRenderData2.Normals.Items;
				mesh2.tangents = dfRenderData2.Tangents.Items;
			}
			mesh2.subMeshCount = submeshes.Count;
			for (int j = 0; j < submeshes.Count; j++)
			{
				int num = submeshes[j];
				int length = dfRenderData2.Triangles.Count - num;
				if (j < submeshes.Count - 1)
				{
					length = submeshes[j + 1] - num;
				}
				int[] array = dfTempArray<int>.Obtain(length);
				dfRenderData2.Triangles.CopyTo(num, array, 0, length);
				mesh2.SetTriangles(array, j);
			}
			if (clipStack.Count != 1)
			{
				Debug.LogError("Clip stack not properly maintained");
			}
			clipStack.Pop().Release();
			clipStack.Clear();
			updateRenderSettings();
		}
		catch (dfAbortRenderingException)
		{
			isDirty = true;
			abortRendering = false;
		}
		finally
		{
			meshRenderer.enabled = true;
			if (dfGUIManager.AfterRender != null)
			{
				dfGUIManager.AfterRender(this);
			}
		}
	}

	private void updateDrawCalls()
	{
		if (meshRenderer == null)
		{
			initialize();
		}
		Material[] array = gatherMaterials();
		meshRenderer.sharedMaterials = array;
		int num = renderQueueBase + array.Length;
		dfRenderGroup[] items = renderGroups.Items;
		int count = renderGroups.Count;
		for (int i = 0; i < count; i++)
		{
			items[i].UpdateRenderQueue(ref num);
		}
	}

	private static bool isInsideClippingRegion(Vector3 point, dfControl control)
	{
		while (control != null)
		{
			Plane[] array = ((!control.ClipChildren) ? null : control.GetClippingPlanes());
			if (array != null && array.Length > 0)
			{
				for (int i = 0; i < array.Length; i++)
				{
					if (!array[i].GetSide(point))
					{
						return false;
					}
				}
			}
			control = control.Parent;
		}
		return true;
	}

	private int getMaxZOrder()
	{
		int num = -1;
		using (dfList<dfControl> dfList2 = getTopLevelControls())
		{
			for (int i = 0; i < dfList2.Count; i++)
			{
				num = Mathf.Max(num, dfList2[i].ZOrder);
			}
			return num;
		}
	}

	private bool isEmptyBuffer(dfRenderData buffer)
	{
		return buffer.Vertices.Count == 0;
	}

	private dfList<dfControl> getTopLevelControls()
	{
		int childCount = base.transform.childCount;
		dfList<dfControl> dfList2 = dfList<dfControl>.Obtain(childCount);
		dfControl[] items = dfControl.ActiveInstances.Items;
		int count = dfControl.ActiveInstances.Count;
		for (int i = 0; i < count; i++)
		{
			dfControl dfControl2 = items[i];
			if (dfControl2.IsTopLevelControl(this))
			{
				dfList2.Add(dfControl2);
			}
		}
		dfList2.Sort();
		return dfList2;
	}

	private void updateRenderSettings()
	{
		Camera camera = RenderCamera;
		if (camera == null)
		{
			return;
		}
		if (!overrideCamera)
		{
			updateRenderCamera(camera);
		}
		if (base.transform.hasChanged)
		{
			Vector3 localScale = base.transform.localScale;
			if (localScale.x < float.Epsilon || !Mathf.Approximately(localScale.x, localScale.y) || !Mathf.Approximately(localScale.x, localScale.z))
			{
				localScale.y = (localScale.z = (localScale.x = Mathf.Max(localScale.x, 0.001f)));
				base.transform.localScale = localScale;
			}
		}
		if (!overrideCamera)
		{
			float num = 1f;
			if (Application.isPlaying && PixelPerfectMode)
			{
				float num2 = (float)camera.pixelHeight / (float)fixedHeight;
				camera.orthographicSize = num2 / num;
				camera.fieldOfView = 60f * num2;
			}
			else
			{
				camera.orthographicSize = 1f / num;
				camera.fieldOfView = 60f;
			}
		}
		camera.transparencySortMode = TransparencySortMode.Orthographic;
		if (cachedScreenSize.sqrMagnitude <= float.Epsilon)
		{
			cachedScreenSize = new Vector2(FixedWidth, FixedHeight);
		}
		base.transform.hasChanged = false;
	}

	private void updateRenderCamera(Camera camera)
	{
		if (Application.isPlaying && camera.targetTexture != null)
		{
			camera.clearFlags = CameraClearFlags.Color;
			camera.backgroundColor = Color.clear;
		}
		else
		{
			camera.clearFlags = overrideClearFlags;
		}
		dfGUICamera component = camera.GetComponent<dfGUICamera>();
		Vector3 vector = Vector3.zero;
		if (component != null)
		{
			vector = component.cameraPositionOffset;
		}
		Vector3 vector2 = ((!Application.isPlaying) ? vector : (-(Vector3)uiOffset * PixelsToUnits() + vector));
		if (camera.orthographic)
		{
			camera.nearClipPlane = Mathf.Min(camera.nearClipPlane, -1f);
			camera.farClipPlane = Mathf.Max(camera.farClipPlane, 1f);
		}
		else
		{
			float num = camera.fieldOfView * ((float)Math.PI / 180f);
			Vector3[] array = GetCorners();
			float num2 = ((!uiScaleLegacy) ? uiScale : 1f);
			float num3 = Vector3.Distance(array[3], array[0]) * num2;
			float num4 = num3 / (2f * Mathf.Tan(num / 2f));
			Vector3 vector3 = base.transform.TransformDirection(Vector3.back) * num4;
			camera.farClipPlane = Mathf.Max(num4 * 2f, camera.farClipPlane);
			vector2 += vector3 / uiScale;
		}
		int height = Screen.height;
		float num5 = 2f / (float)height * ((float)height / (float)FixedHeight);
		if (Application.isPlaying && !component.ForceNoHalfPixelOffset && needHalfPixelOffset())
		{
			Vector3 vector4 = new Vector3(num5 * 0.5f, num5 * -0.5f, 0f);
			if (AmmonomiconController.GuiManagerIsPageRenderer(this))
			{
				vector4.x /= 2f;
			}
			vector2 += vector4;
		}
		if (!overrideCamera)
		{
			camera.renderingPath = RenderingPath.Forward;
			if (camera.pixelWidth % 2 != 0)
			{
				vector2.x += num5 * 0.5f;
			}
			if (camera.pixelHeight % 2 != 0)
			{
				vector2.y += num5 * 0.5f;
			}
			if (Vector3.SqrMagnitude(camera.transform.localPosition - vector2) > float.Epsilon)
			{
				camera.transform.localPosition = vector2;
			}
			camera.transform.hasChanged = false;
		}
	}

	private dfRenderData compileMasterBuffer()
	{
		try
		{
			submeshes.Clear();
			masterBuffer.Clear();
			dfRenderData[] items = drawCallBuffers.Items;
			int num = 0;
			for (int i = 0; i < drawCallCount; i++)
			{
				num += items[i].Vertices.Count;
			}
			masterBuffer.EnsureCapacity(num);
			for (int j = 0; j < drawCallCount; j++)
			{
				submeshes.Add(masterBuffer.Triangles.Count);
				dfRenderData dfRenderData2 = items[j];
				if (generateNormals && dfRenderData2.Normals.Count == 0)
				{
					generateNormalsAndTangents(dfRenderData2);
				}
				masterBuffer.Merge(dfRenderData2, false);
			}
			masterBuffer.ApplyTransform(base.transform.worldToLocalMatrix);
			return masterBuffer;
		}
		finally
		{
		}
	}

	private void generateNormalsAndTangents(dfRenderData buffer)
	{
		Vector3 normalized = buffer.Transform.MultiplyVector(Vector3.back).normalized;
		Vector4 item = buffer.Transform.MultiplyVector(Vector3.right).normalized;
		item.w = -1f;
		for (int i = 0; i < buffer.Vertices.Count; i++)
		{
			buffer.Normals.Add(normalized);
			buffer.Tangents.Add(item);
		}
	}

	private bool needHalfPixelOffset()
	{
		if (applyHalfPixelOffset.HasValue)
		{
			return applyHalfPixelOffset.Value;
		}
		RuntimePlatform platform = Application.platform;
		bool flag = pixelPerfectMode && (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor) && SystemInfo.graphicsDeviceVersion.ToLower().StartsWith("direct");
		bool flag2 = SystemInfo.graphicsShaderLevel >= 40;
		applyHalfPixelOffset = (Application.isEditor || flag) && !flag2;
		return flag;
	}

	private Material[] gatherMaterials()
	{
		try
		{
			int materialCount = getMaterialCount();
			int num = 0;
			int num2 = renderQueueBase;
			Material[] array = dfTempArray<Material>.Obtain(materialCount);
			for (int i = 0; i < drawCallBuffers.Count; i++)
			{
				dfRenderData dfRenderData2 = drawCallBuffers[i];
				if (!(dfRenderData2.Material == null))
				{
					Material material = dfMaterialCache.Lookup(dfRenderData2.Material);
					material.mainTexture = dfRenderData2.Material.mainTexture;
					material.shader = dfRenderData2.Shader ?? material.shader;
					if (renderQueueSecondDraw > -1 && material.shader.renderQueue > 6000)
					{
						material.renderQueue = material.shader.renderQueue;
						num2++;
					}
					else
					{
						material.renderQueue = num2++;
					}
					material.mainTextureOffset = Vector2.zero;
					material.mainTextureScale = Vector2.zero;
					array[num++] = material;
				}
			}
			return array;
		}
		finally
		{
		}
	}

	private int getMaterialCount()
	{
		int num = 0;
		for (int i = 0; i < drawCallCount; i++)
		{
			if (drawCallBuffers[i] != null && drawCallBuffers[i].Material != null)
			{
				num++;
			}
		}
		return num;
	}

	private void resetDrawCalls()
	{
		drawCallCount = 0;
		for (int i = 0; i < drawCallBuffers.Count; i++)
		{
			drawCallBuffers[i].Release();
		}
		drawCallBuffers.Clear();
	}

	private dfRenderData getDrawCallBuffer(Material material)
	{
		dfRenderData dfRenderData2 = null;
		if (MergeMaterials && material != null)
		{
			dfRenderData2 = findDrawCallBufferByMaterial(material);
			if (dfRenderData2 != null)
			{
				return dfRenderData2;
			}
		}
		dfRenderData2 = dfRenderData.Obtain();
		dfRenderData2.Material = material;
		drawCallBuffers.Add(dfRenderData2);
		drawCallCount++;
		return dfRenderData2;
	}

	private dfRenderData findDrawCallBufferByMaterial(Material material)
	{
		for (int i = 0; i < drawCallCount; i++)
		{
			if (drawCallBuffers[i].Material == material)
			{
				return drawCallBuffers[i];
			}
		}
		return null;
	}

	private Mesh getRenderMesh()
	{
		activeRenderMesh = ((activeRenderMesh != 1) ? 1 : 0);
		return renderMesh[activeRenderMesh];
	}

	private void renderControl(ref dfRenderData buffer, dfControl control, uint checksum, float opacity)
	{
		if (!control.enabled || !control.gameObject.activeSelf)
		{
			return;
		}
		float num = opacity * control.Opacity;
		dfRenderGroup renderGroupForControl = dfRenderGroup.GetRenderGroupForControl(control, true);
		if (renderGroupForControl != null && renderGroupForControl.enabled)
		{
			renderGroups.Add(renderGroupForControl);
			renderGroupForControl.Render(renderCamera, control, occluders, controlsRendered, checksum, num);
		}
		else
		{
			if (num <= 0.001f || !control.GetIsVisibleRaw())
			{
				return;
			}
			dfTriangleClippingRegion dfTriangleClippingRegion2 = clipStack.Peek();
			checksum = dfChecksumUtil.Calculate(checksum, control.Version);
			Bounds bounds = control.GetBounds();
			bool wasClipped = false;
			if (!(control is IDFMultiRender))
			{
				dfRenderData dfRenderData2 = control.Render();
				if (dfRenderData2 != null)
				{
					processRenderData(ref buffer, dfRenderData2, ref bounds, checksum, dfTriangleClippingRegion2, ref wasClipped);
				}
			}
			else
			{
				dfList<dfRenderData> dfList2 = ((IDFMultiRender)control).RenderMultiple();
				if (dfList2 != null)
				{
					dfRenderData[] items = dfList2.Items;
					int count = dfList2.Count;
					for (int i = 0; i < count; i++)
					{
						dfRenderData dfRenderData3 = items[i];
						if (dfRenderData3 != null)
						{
							processRenderData(ref buffer, dfRenderData3, ref bounds, checksum, dfTriangleClippingRegion2, ref wasClipped);
						}
					}
				}
			}
			control.setClippingState(wasClipped);
			NumControlsRendered++;
			occluders.Add(getControlOccluder(control));
			controlsRendered.Add(control);
			drawCallIndices.Add(drawCallBuffers.Count - 1);
			if (control.ClipChildren)
			{
				dfTriangleClippingRegion2 = dfTriangleClippingRegion.Obtain(dfTriangleClippingRegion2, control);
				clipStack.Push(dfTriangleClippingRegion2);
			}
			dfControl[] items2 = control.Controls.Items;
			int count2 = control.Controls.Count;
			controlsRendered.EnsureCapacity(controlsRendered.Count + count2);
			occluders.EnsureCapacity(occluders.Count + count2);
			for (int j = 0; j < count2; j++)
			{
				renderControl(ref buffer, items2[j], checksum, num);
			}
			if (control.ClipChildren)
			{
				clipStack.Pop().Release();
			}
		}
	}

	private Rect getControlOccluder(dfControl control)
	{
		if (!control.IsInteractive)
		{
			return default(Rect);
		}
		Rect screenRect = control.GetScreenRect();
		Vector2 vector = new Vector2(screenRect.width * control.HotZoneScale.x, screenRect.height * control.HotZoneScale.y);
		Vector2 vector2 = new Vector2(vector.x - screenRect.width, vector.y - screenRect.height) * 0.5f;
		return new Rect(screenRect.x - vector2.x, screenRect.y - vector2.y, vector.x, vector.y);
	}

	private bool processRenderData(ref dfRenderData buffer, dfRenderData controlData, ref Bounds bounds, uint checksum, dfTriangleClippingRegion clipInfo, ref bool wasClipped)
	{
		wasClipped = false;
		if (controlData == null || controlData.Material == null || !controlData.IsValid())
		{
			return false;
		}
		bool flag = false;
		if (buffer == null)
		{
			flag = true;
		}
		else if (!object.Equals(controlData.Material, buffer.Material))
		{
			flag = true;
		}
		else if (!textureEqual(controlData.Material.mainTexture, buffer.Material.mainTexture))
		{
			flag = true;
		}
		else if (!shaderEqual(buffer.Shader, controlData.Shader))
		{
			flag = true;
		}
		if (flag)
		{
			buffer = getDrawCallBuffer(controlData.Material);
			buffer.Material = controlData.Material;
			buffer.Material.mainTexture = controlData.Material.mainTexture;
			buffer.Material.shader = controlData.Shader ?? controlData.Material.shader;
		}
		if (clipInfo.PerformClipping(buffer, ref bounds, checksum, controlData))
		{
			return true;
		}
		wasClipped = true;
		return false;
	}

	private bool textureEqual(Texture lhs, Texture rhs)
	{
		return object.Equals(lhs, rhs);
	}

	private bool shaderEqual(Shader lhs, Shader rhs)
	{
		if (lhs == null || rhs == null)
		{
			return object.ReferenceEquals(lhs, rhs);
		}
		return lhs.name.Equals(rhs.name);
	}

	private void initialize()
	{
		if (Application.isPlaying && renderCamera == null)
		{
			Debug.LogError("No camera is assigned to the GUIManager");
			return;
		}
		meshRenderer = GetComponent<MeshRenderer>();
		if (meshRenderer == null)
		{
			meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
		}
		renderFilter = GetComponent<MeshFilter>();
		if (renderFilter == null)
		{
			renderFilter = base.gameObject.AddComponent<MeshFilter>();
		}
		renderMesh = new Mesh[2]
		{
			new Mesh
			{
				hideFlags = HideFlags.DontSave
			},
			new Mesh
			{
				hideFlags = HideFlags.DontSave
			}
		};
		renderMesh[0].MarkDynamic();
		renderMesh[1].MarkDynamic();
		if (fixedWidth < 0)
		{
			fixedWidth = Mathf.RoundToInt((float)fixedHeight * 1.33333f);
			dfControl[] componentsInChildren = GetComponentsInChildren<dfControl>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].ResetLayout();
			}
		}
	}

	private void onResolutionChanged()
	{
		int currentSize = ((!Application.isPlaying) ? FixedHeight : renderCamera.pixelHeight);
		onResolutionChanged(FixedHeight, currentSize);
	}

	private void onResolutionChanged(int oldSize, int currentSize)
	{
		float renderAspect = RenderAspect;
		float x = (float)oldSize * renderAspect;
		float x2 = (float)currentSize * renderAspect;
		Vector2 oldSize2 = new Vector2(x, oldSize);
		Vector2 currentSize2 = new Vector2(x2, currentSize);
		onResolutionChanged(oldSize2, currentSize2);
	}

	public static void ForceResolutionUpdates()
	{
		for (int i = 0; i < activeInstances.Count; i++)
		{
			activeInstances[i].onResolutionChanged();
		}
	}

	public void ResolutionChanged()
	{
		onResolutionChanged();
	}

	private void onResolutionChanged(Vector2 oldSize, Vector2 currentSize)
	{
		if (shutdownInProcess)
		{
			return;
		}
		cachedScreenSize = currentSize;
		applyHalfPixelOffset = null;
		float renderAspect = RenderAspect;
		float x = oldSize.y * renderAspect;
		float x2 = currentSize.y * renderAspect;
		Vector2 previousResolution = new Vector2(x, oldSize.y);
		Vector2 currentResolution = new Vector2(x2, currentSize.y);
		dfControl[] componentsInChildren = GetComponentsInChildren<dfControl>();
		Array.Sort(componentsInChildren, renderSortFunc);
		ResolutionIsChanging = true;
		for (int num = componentsInChildren.Length - 1; num >= 0; num--)
		{
			if (pixelPerfectMode && componentsInChildren[num].Parent == null)
			{
				componentsInChildren[num].MakePixelPerfect();
			}
			componentsInChildren[num].OnResolutionChanged(previousResolution, currentResolution);
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].PerformLayout();
		}
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			if (!pixelPerfectMode)
			{
				break;
			}
			if (componentsInChildren[j].Parent == null)
			{
				componentsInChildren[j].MakePixelPerfect();
			}
		}
		ResolutionIsChanging = false;
		isDirty = true;
		updateRenderSettings();
	}

	private void invalidateAllControls()
	{
		dfControl[] componentsInChildren = GetComponentsInChildren<dfControl>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].Invalidate();
		}
		updateRenderOrder();
	}

	private int renderSortFunc(dfControl lhs, dfControl rhs)
	{
		return lhs.RenderOrder.CompareTo(rhs.RenderOrder);
	}

	private void updateRenderOrder()
	{
		updateRenderOrder(null);
	}

	private void updateRenderOrder(dfList<dfControl> list)
	{
		dfList<dfControl> dfList2 = list;
		bool flag = false;
		if (list == null)
		{
			dfList2 = getTopLevelControls();
			flag = true;
		}
		else
		{
			dfList2.Sort();
		}
		int order = 0;
		int count = dfList2.Count;
		dfControl[] items = dfList2.Items;
		for (int i = 0; i < count; i++)
		{
			dfControl dfControl2 = items[i];
			if (dfControl2.Parent == null)
			{
				dfControl2.setRenderOrder(ref order);
			}
		}
		if (flag)
		{
			dfList2.Release();
		}
	}

	public int CompareTo(dfGUIManager other)
	{
		int num = renderQueueBase.CompareTo(other.renderQueueBase);
		if (num == 0 && RenderCamera != null && other.RenderCamera != null)
		{
			return RenderCamera.depth.CompareTo(other.RenderCamera.depth);
		}
		return num;
	}
}
