using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[ExecuteInEditMode]
[AddComponentMenu("Daikon Forge/User Interface/Render Group")]
internal class dfRenderGroup : MonoBehaviour
{
	private struct ClipRegionInfo
	{
		public Vector2 Offset;

		public Vector2 Size;

		public bool IsEmpty
		{
			get
			{
				return Offset == Vector2.zero && Size == Vector2.zero;
			}
		}
	}

	private static List<dfRenderGroup> activeInstances = new List<dfRenderGroup>();

	[SerializeField]
	protected dfClippingMethod clipType;

	private Mesh renderMesh;

	private MeshFilter renderFilter;

	private MeshRenderer meshRenderer;

	private Camera renderCamera;

	private dfControl attachedControl;

	private static dfRenderData masterBuffer = new dfRenderData(4096);

	private dfList<dfRenderData> drawCallBuffers = new dfList<dfRenderData>();

	private List<int> submeshes = new List<int>();

	private Stack<dfTriangleClippingRegion> clipStack = new Stack<dfTriangleClippingRegion>();

	private dfList<Rect> groupOccluders = new dfList<Rect>();

	private dfList<dfControl> groupControls = new dfList<dfControl>();

	private dfList<dfRenderGroup> renderGroups = new dfList<dfRenderGroup>();

	private ClipRegionInfo clipInfo = default(ClipRegionInfo);

	private Rect clipRect = default(Rect);

	private Rect containerRect = default(Rect);

	private int drawCallCount;

	private bool isDirty;

	public dfClippingMethod ClipType
	{
		get
		{
			return clipType;
		}
		set
		{
			if (value != clipType)
			{
				clipType = value;
				if (attachedControl != null)
				{
					attachedControl.Invalidate();
				}
			}
		}
	}

	public void OnEnable()
	{
		activeInstances.Add(this);
		isDirty = true;
		if (meshRenderer == null)
		{
			initialize();
		}
		meshRenderer.enabled = true;
		if (attachedControl != null)
		{
			attachedControl.Invalidate();
		}
		else
		{
			dfGUIManager.InvalidateAll();
		}
		attachedControl = GetComponent<dfControl>();
	}

	public void OnDisable()
	{
		activeInstances.Remove(this);
		if (meshRenderer != null)
		{
			meshRenderer.enabled = false;
		}
		if (attachedControl != null)
		{
			attachedControl.Invalidate();
		}
	}

	public void OnDestroy()
	{
		if (renderFilter != null)
		{
			renderFilter.sharedMesh = null;
		}
		renderFilter = null;
		meshRenderer = null;
		if (renderMesh != null)
		{
			renderMesh.Clear();
			UnityEngine.Object.DestroyImmediate(renderMesh);
			renderMesh = null;
		}
		dfGUIManager.InvalidateAll();
	}

	internal static dfRenderGroup GetRenderGroupForControl(dfControl control)
	{
		return GetRenderGroupForControl(control, false);
	}

	internal static dfRenderGroup GetRenderGroupForControl(dfControl control, bool directlyAttachedOnly)
	{
		Transform transform = control.transform;
		for (int i = 0; i < activeInstances.Count; i++)
		{
			dfRenderGroup dfRenderGroup2 = activeInstances[i];
			if (dfRenderGroup2.attachedControl == control)
			{
				return dfRenderGroup2;
			}
			if (!directlyAttachedOnly && transform.IsChildOf(dfRenderGroup2.transform))
			{
				return dfRenderGroup2;
			}
		}
		return null;
	}

	internal static void InvalidateGroupForControl(dfControl control)
	{
		Transform transform = control.transform;
		for (int i = 0; i < activeInstances.Count; i++)
		{
			dfRenderGroup dfRenderGroup2 = activeInstances[i];
			if (transform.IsChildOf(dfRenderGroup2.transform))
			{
				dfRenderGroup2.isDirty = true;
			}
		}
	}

	internal void Render(Camera renderCamera, dfControl control, dfList<Rect> occluders, dfList<dfControl> controlsRendered, uint checksum, float opacity)
	{
		if (meshRenderer == null)
		{
			initialize();
		}
		this.renderCamera = renderCamera;
		attachedControl = control;
		if (!isDirty)
		{
			occluders.AddRange(groupOccluders);
			controlsRendered.AddRange(groupControls);
			return;
		}
		groupOccluders.Clear();
		groupControls.Clear();
		renderGroups.Clear();
		resetDrawCalls();
		clipInfo = default(ClipRegionInfo);
		clipRect = default(Rect);
		dfRenderData buffer = null;
		using (dfTriangleClippingRegion t = dfTriangleClippingRegion.Obtain())
		{
			clipStack.Clear();
			clipStack.Push(t);
			renderControl(ref buffer, control, checksum, opacity);
			clipStack.Pop();
		}
		drawCallBuffers.RemoveAll(isEmptyBuffer);
		drawCallCount = drawCallBuffers.Count;
		if (drawCallBuffers.Count == 0)
		{
			meshRenderer.enabled = false;
			return;
		}
		meshRenderer.enabled = true;
		dfRenderData dfRenderData2 = compileMasterBuffer();
		Mesh mesh = renderMesh;
		mesh.Clear(true);
		mesh.vertices = dfRenderData2.Vertices.Items;
		mesh.uv = dfRenderData2.UV.Items;
		mesh.colors32 = dfRenderData2.Colors.Items;
		mesh.subMeshCount = submeshes.Count;
		for (int i = 0; i < submeshes.Count; i++)
		{
			int num = submeshes[i];
			int length = dfRenderData2.Triangles.Count - num;
			if (i < submeshes.Count - 1)
			{
				length = submeshes[i + 1] - num;
			}
			int[] array = dfTempArray<int>.Obtain(length);
			dfRenderData2.Triangles.CopyTo(num, array, 0, length);
			mesh.SetTriangles(array, i);
		}
		isDirty = false;
		occluders.AddRange(groupOccluders);
		controlsRendered.AddRange(groupControls);
	}

	internal void UpdateRenderQueue(ref int renderQueueBase)
	{
		int materialCount = getMaterialCount();
		int num = 0;
		Material[] array = dfTempArray<Material>.Obtain(materialCount);
		for (int i = 0; i < drawCallBuffers.Count; i++)
		{
			if (!(drawCallBuffers[i].Material == null))
			{
				Material material = dfMaterialCache.Lookup(drawCallBuffers[i].Material);
				material.mainTexture = drawCallBuffers[i].Material.mainTexture;
				material.shader = drawCallBuffers[i].Shader ?? material.shader;
				material.mainTextureScale = Vector2.zero;
				material.mainTextureOffset = Vector2.zero;
				material.renderQueue = ++renderQueueBase;
				if (Application.isPlaying && clipType == dfClippingMethod.Shader && !clipInfo.IsEmpty && i > 0)
				{
					Vector3 vector = attachedControl.Pivot.TransformToCenter(attachedControl.Size);
					float num2 = vector.x + clipInfo.Offset.x;
					float num3 = vector.y + clipInfo.Offset.y;
					float num4 = attachedControl.PixelsToUnits();
					material.mainTextureScale = new Vector2(1f / ((0f - clipInfo.Size.x) * 0.5f * num4), 1f / ((0f - clipInfo.Size.y) * 0.5f * num4));
					material.mainTextureOffset = new Vector2(num2 / clipInfo.Size.x * 2f, num3 / clipInfo.Size.y * 2f);
				}
				array[num++] = material;
			}
		}
		meshRenderer.sharedMaterials = array;
		dfRenderGroup[] items = renderGroups.Items;
		int count = renderGroups.Count;
		for (int j = 0; j < count; j++)
		{
			items[j].UpdateRenderQueue(ref renderQueueBase);
		}
	}

	private void renderControl(ref dfRenderData buffer, dfControl control, uint checksum, float opacity)
	{
		if (!control.enabled || !control.gameObject.activeSelf)
		{
			return;
		}
		float num = opacity * control.Opacity;
		if (num <= 0.001f)
		{
			return;
		}
		dfRenderGroup renderGroupForControl = GetRenderGroupForControl(control, true);
		if (renderGroupForControl != null && renderGroupForControl != this && renderGroupForControl.enabled)
		{
			renderGroups.Add(renderGroupForControl);
			renderGroupForControl.Render(renderCamera, control, groupOccluders, groupControls, checksum, num);
		}
		else
		{
			if (!control.GetIsVisibleRaw())
			{
				return;
			}
			dfTriangleClippingRegion parent = clipStack.Peek();
			checksum = dfChecksumUtil.Calculate(checksum, control.Version);
			Bounds bounds = control.GetBounds();
			Rect screenRect = control.GetScreenRect();
			Rect controlOccluder = getControlOccluder(ref screenRect, control);
			bool wasClipped = false;
			if (!(control is IDFMultiRender))
			{
				dfRenderData dfRenderData2 = control.Render();
				if (dfRenderData2 != null)
				{
					processRenderData(ref buffer, dfRenderData2, ref bounds, ref screenRect, checksum, parent, ref wasClipped);
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
							processRenderData(ref buffer, dfRenderData3, ref bounds, ref screenRect, checksum, parent, ref wasClipped);
						}
					}
				}
			}
			control.setClippingState(wasClipped);
			groupOccluders.Add(controlOccluder);
			groupControls.Add(control);
			if (control.ClipChildren)
			{
				if (!Application.isPlaying || clipType == dfClippingMethod.Software)
				{
					parent = dfTriangleClippingRegion.Obtain(parent, control);
					clipStack.Push(parent);
				}
				else if (clipInfo.IsEmpty)
				{
					setClipRegion(control, ref screenRect);
				}
			}
			dfControl[] items2 = control.Controls.Items;
			int count2 = control.Controls.Count;
			groupControls.EnsureCapacity(groupControls.Count + count2);
			groupOccluders.EnsureCapacity(groupOccluders.Count + count2);
			for (int j = 0; j < count2; j++)
			{
				renderControl(ref buffer, items2[j], checksum, num);
			}
			if (control.ClipChildren && (!Application.isPlaying || clipType == dfClippingMethod.Software))
			{
				clipStack.Pop().Release();
			}
		}
	}

	private void setClipRegion(dfControl control, ref Rect screenRect)
	{
		Vector2 size = control.Size;
		RectOffset clipPadding = control.GetClipPadding();
		float num = Mathf.Min(Mathf.Max(0f, Mathf.Min(size.x, clipPadding.horizontal)), size.x);
		float num2 = Mathf.Min(Mathf.Max(0f, Mathf.Min(size.y, clipPadding.vertical)), size.y);
		clipInfo = default(ClipRegionInfo);
		clipInfo.Size = Vector2.Max(new Vector2(size.x - num, size.y - num2), Vector3.zero);
		clipInfo.Offset = new Vector3(clipPadding.left - clipPadding.right, -(clipPadding.top - clipPadding.bottom)) * 0.5f;
		clipRect = ((!containerRect.IsEmpty()) ? containerRect.Intersection(screenRect) : screenRect);
	}

	private bool processRenderData(ref dfRenderData buffer, dfRenderData controlData, ref Bounds bounds, ref Rect screenRect, uint checksum, dfTriangleClippingRegion clipInfo, ref bool wasClipped)
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
		else if (!this.clipInfo.IsEmpty && drawCallBuffers.Count == 1)
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
		if (!Application.isPlaying || clipType == dfClippingMethod.Software)
		{
			if (clipInfo.PerformClipping(buffer, ref bounds, checksum, controlData))
			{
				return true;
			}
			wasClipped = true;
		}
		else if (clipRect.IsEmpty() || screenRect.Intersects(clipRect))
		{
			buffer.Merge(controlData);
		}
		else
		{
			wasClipped = true;
		}
		return false;
	}

	private dfRenderData compileMasterBuffer()
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
			dfRenderData buffer = items[j];
			masterBuffer.Merge(buffer, false);
		}
		masterBuffer.ApplyTransform(base.transform.worldToLocalMatrix);
		return masterBuffer;
	}

	private bool isEmptyBuffer(dfRenderData buffer)
	{
		return buffer.Vertices.Count == 0;
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
		dfRenderData dfRenderData2 = dfRenderData.Obtain();
		dfRenderData2.Material = material;
		drawCallBuffers.Add(dfRenderData2);
		drawCallCount++;
		return dfRenderData2;
	}

	private Rect getControlOccluder(ref Rect screenRect, dfControl control)
	{
		if (!control.IsInteractive)
		{
			return default(Rect);
		}
		Vector2 vector = new Vector2(screenRect.width * control.HotZoneScale.x, screenRect.height * control.HotZoneScale.y);
		Vector2 vector2 = new Vector2(vector.x - screenRect.width, vector.y - screenRect.height) * 0.5f;
		return new Rect(screenRect.x - vector2.x, screenRect.y - vector2.y, vector.x, vector.y);
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
		meshRenderer = GetComponent<MeshRenderer>();
		if (meshRenderer == null)
		{
			meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
		}
		meshRenderer.hideFlags = HideFlags.HideInInspector;
		renderFilter = GetComponent<MeshFilter>();
		if (renderFilter == null)
		{
			renderFilter = base.gameObject.AddComponent<MeshFilter>();
		}
		renderFilter.hideFlags = HideFlags.HideInInspector;
		renderMesh = new Mesh
		{
			hideFlags = HideFlags.DontSave
		};
		renderMesh.MarkDynamic();
		renderFilter.sharedMesh = renderMesh;
	}
}
