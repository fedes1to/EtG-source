using System;
using UnityEngine;

[Serializable]
[ExecuteInEditMode]
[AddComponentMenu("Daikon Forge/User Interface/Listbox")]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_listbox.html")]
[dfCategory("Basic Controls")]
[dfTooltip("Allows the user to select from a list of options")]
public class dfListbox : dfInteractiveBase, IDFMultiRender, IRendersText
{
	[SerializeField]
	protected dfFontBase font;

	[SerializeField]
	protected RectOffset listPadding = new RectOffset();

	[SerializeField]
	protected int selectedIndex = -1;

	[SerializeField]
	protected Color32 itemTextColor = UnityEngine.Color.white;

	[SerializeField]
	protected float itemTextScale = 1f;

	[SerializeField]
	protected int itemHeight = 25;

	[SerializeField]
	protected RectOffset itemPadding = new RectOffset();

	[SerializeField]
	protected string[] items = new string[0];

	[SerializeField]
	protected string itemHighlight = string.Empty;

	[SerializeField]
	protected string itemHover = string.Empty;

	[SerializeField]
	protected dfScrollbar scrollbar;

	[SerializeField]
	protected bool animateHover;

	[SerializeField]
	protected bool shadow;

	[SerializeField]
	protected dfTextScaleMode textScaleMode;

	[SerializeField]
	protected Color32 shadowColor = UnityEngine.Color.black;

	[SerializeField]
	protected Vector2 shadowOffset = new Vector2(1f, -1f);

	[SerializeField]
	protected TextAlignment itemAlignment;

	private bool isFontCallbackAssigned;

	private bool eventsAttached;

	private float scrollPosition;

	private int hoverIndex = -1;

	private float hoverTweenLocation;

	private Vector2 touchStartPosition = Vector2.zero;

	private Vector2 startSize = Vector2.zero;

	private dfRenderData textRenderData;

	private dfList<dfRenderData> buffers = dfList<dfRenderData>.Obtain();

	public dfFontBase Font
	{
		get
		{
			if (font == null)
			{
				dfGUIManager manager = GetManager();
				if (manager != null)
				{
					font = manager.DefaultFont;
				}
			}
			return font;
		}
		set
		{
			if (value != font)
			{
				unbindTextureRebuildCallback();
				font = value;
				bindTextureRebuildCallback();
				Invalidate();
			}
		}
	}

	public float ScrollPosition
	{
		get
		{
			return scrollPosition;
		}
		set
		{
			if (!Mathf.Approximately(value, scrollPosition))
			{
				scrollPosition = constrainScrollPosition(value);
				Invalidate();
			}
		}
	}

	public TextAlignment ItemAlignment
	{
		get
		{
			return itemAlignment;
		}
		set
		{
			if (value != itemAlignment)
			{
				itemAlignment = value;
				Invalidate();
			}
		}
	}

	public string ItemHighlight
	{
		get
		{
			return itemHighlight;
		}
		set
		{
			if (value != itemHighlight)
			{
				itemHighlight = value;
				Invalidate();
			}
		}
	}

	public string ItemHover
	{
		get
		{
			return itemHover;
		}
		set
		{
			if (value != itemHover)
			{
				itemHover = value;
				Invalidate();
			}
		}
	}

	public string SelectedItem
	{
		get
		{
			if (selectedIndex == -1)
			{
				return null;
			}
			return items[selectedIndex];
		}
	}

	public string SelectedValue
	{
		get
		{
			return items[selectedIndex];
		}
		set
		{
			selectedIndex = -1;
			for (int i = 0; i < items.Length; i++)
			{
				if (items[i] == value)
				{
					selectedIndex = i;
					break;
				}
			}
		}
	}

	public int SelectedIndex
	{
		get
		{
			return selectedIndex;
		}
		set
		{
			value = Mathf.Max(-1, value);
			value = Mathf.Min(items.Length - 1, value);
			if (value != selectedIndex)
			{
				selectedIndex = value;
				EnsureVisible(value);
				OnSelectedIndexChanged();
				Invalidate();
			}
		}
	}

	public RectOffset ItemPadding
	{
		get
		{
			if (itemPadding == null)
			{
				itemPadding = new RectOffset();
			}
			return itemPadding;
		}
		set
		{
			value = value.ConstrainPadding();
			if (!value.Equals(itemPadding))
			{
				itemPadding = value;
				Invalidate();
			}
		}
	}

	public Color32 ItemTextColor
	{
		get
		{
			return itemTextColor;
		}
		set
		{
			if (!value.Equals(itemTextColor))
			{
				itemTextColor = value;
				Invalidate();
			}
		}
	}

	public float ItemTextScale
	{
		get
		{
			return itemTextScale;
		}
		set
		{
			value = Mathf.Max(0.1f, value);
			if (!Mathf.Approximately(itemTextScale, value))
			{
				itemTextScale = value;
				Invalidate();
			}
		}
	}

	public int ItemHeight
	{
		get
		{
			return itemHeight;
		}
		set
		{
			scrollPosition = 0f;
			value = Mathf.Max(1, value);
			if (value != itemHeight)
			{
				itemHeight = value;
				Invalidate();
			}
		}
	}

	public string[] Items
	{
		get
		{
			if (items == null)
			{
				items = new string[0];
			}
			return items;
		}
		set
		{
			if (value != items)
			{
				scrollPosition = 0f;
				if (value == null)
				{
					value = new string[0];
				}
				items = value;
				Invalidate();
			}
		}
	}

	public dfScrollbar Scrollbar
	{
		get
		{
			return scrollbar;
		}
		set
		{
			scrollPosition = 0f;
			if (value != scrollbar)
			{
				detachScrollbarEvents();
				scrollbar = value;
				attachScrollbarEvents();
				Invalidate();
			}
		}
	}

	public RectOffset ListPadding
	{
		get
		{
			if (listPadding == null)
			{
				listPadding = new RectOffset();
			}
			return listPadding;
		}
		set
		{
			value = value.ConstrainPadding();
			if (!object.Equals(value, listPadding))
			{
				listPadding = value;
				Invalidate();
			}
		}
	}

	public bool Shadow
	{
		get
		{
			return shadow;
		}
		set
		{
			if (value != shadow)
			{
				shadow = value;
				Invalidate();
			}
		}
	}

	public Color32 ShadowColor
	{
		get
		{
			return shadowColor;
		}
		set
		{
			if (!value.Equals(shadowColor))
			{
				shadowColor = value;
				Invalidate();
			}
		}
	}

	public Vector2 ShadowOffset
	{
		get
		{
			return shadowOffset;
		}
		set
		{
			if (value != shadowOffset)
			{
				shadowOffset = value;
				Invalidate();
			}
		}
	}

	public bool AnimateHover
	{
		get
		{
			return animateHover;
		}
		set
		{
			animateHover = value;
		}
	}

	public dfTextScaleMode TextScaleMode
	{
		get
		{
			return textScaleMode;
		}
		set
		{
			textScaleMode = value;
			Invalidate();
		}
	}

	public event PropertyChangedEventHandler<int> SelectedIndexChanged;

	public event PropertyChangedEventHandler<int> ItemClicked;

	public override void Awake()
	{
		base.Awake();
		startSize = base.Size;
	}

	public override void Update()
	{
		base.Update();
		if (size.magnitude == 0f)
		{
			size = new Vector2(200f, 150f);
		}
		if (animateHover && hoverIndex != -1)
		{
			float num = (float)(hoverIndex * itemHeight) * PixelsToUnits();
			if (Mathf.Abs(hoverTweenLocation - num) < 1f)
			{
				Invalidate();
			}
		}
		if (isControlInvalidated)
		{
			synchronizeScrollbar();
		}
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		if (Application.isPlaying)
		{
			attachScrollbarEvents();
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		bindTextureRebuildCallback();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		detachScrollbarEvents();
	}

	public override void OnDisable()
	{
		base.OnDisable();
		unbindTextureRebuildCallback();
		detachScrollbarEvents();
	}

	protected internal override void OnLocalize()
	{
		base.OnLocalize();
		bool flag = false;
		for (int i = 0; i < items.Length; i++)
		{
			string localizedValue = getLocalizedValue(items[i]);
			if (localizedValue != items[i])
			{
				flag = true;
				items[i] = localizedValue;
			}
		}
		if (flag)
		{
			Invalidate();
		}
	}

	protected internal virtual void OnSelectedIndexChanged()
	{
		SignalHierarchy("OnSelectedIndexChanged", this, selectedIndex);
		if (this.SelectedIndexChanged != null)
		{
			this.SelectedIndexChanged(this, selectedIndex);
		}
	}

	protected internal virtual void OnItemClicked()
	{
		Signal("OnItemClicked", this, selectedIndex);
		if (this.ItemClicked != null)
		{
			this.ItemClicked(this, selectedIndex);
		}
	}

	protected internal override void OnMouseMove(dfMouseEventArgs args)
	{
		base.OnMouseMove(args);
		if (args is dfTouchEventArgs)
		{
			if (!(Mathf.Abs(args.Position.y - touchStartPosition.y) < (float)(itemHeight / 2)))
			{
				ScrollPosition = Mathf.Max(0f, ScrollPosition + args.MoveDelta.y);
				synchronizeScrollbar();
				hoverIndex = -1;
			}
		}
		else
		{
			updateItemHover(args);
		}
	}

	protected internal override void OnMouseEnter(dfMouseEventArgs args)
	{
		base.OnMouseEnter(args);
		touchStartPosition = args.Position;
	}

	protected internal override void OnMouseLeave(dfMouseEventArgs args)
	{
		base.OnMouseLeave(args);
		hoverIndex = -1;
	}

	protected internal override void OnMouseWheel(dfMouseEventArgs args)
	{
		base.OnMouseWheel(args);
		ScrollPosition = Mathf.Max(0f, ScrollPosition - (float)((int)args.WheelDelta * ItemHeight));
		synchronizeScrollbar();
		updateItemHover(args);
	}

	protected internal override void OnMouseUp(dfMouseEventArgs args)
	{
		hoverIndex = -1;
		base.OnMouseUp(args);
		if (args is dfTouchEventArgs && Mathf.Abs(args.Position.y - touchStartPosition.y) < (float)itemHeight)
		{
			selectItemUnderMouse(args);
		}
	}

	protected internal override void OnMouseDown(dfMouseEventArgs args)
	{
		base.OnMouseDown(args);
		if (args is dfTouchEventArgs)
		{
			touchStartPosition = args.Position;
		}
		else
		{
			selectItemUnderMouse(args);
		}
	}

	protected internal override void OnKeyDown(dfKeyEventArgs args)
	{
		switch (args.KeyCode)
		{
		case KeyCode.PageDown:
			SelectedIndex += Mathf.FloorToInt((size.y - (float)listPadding.vertical) / (float)itemHeight);
			break;
		case KeyCode.PageUp:
		{
			int b = SelectedIndex - Mathf.FloorToInt((size.y - (float)listPadding.vertical) / (float)itemHeight);
			SelectedIndex = Mathf.Max(0, b);
			break;
		}
		case KeyCode.UpArrow:
			SelectedIndex = Mathf.Max(0, selectedIndex - 1);
			break;
		case KeyCode.DownArrow:
			SelectedIndex++;
			break;
		case KeyCode.Home:
			SelectedIndex = 0;
			break;
		case KeyCode.End:
			SelectedIndex = items.Length;
			break;
		}
		base.OnKeyDown(args);
	}

	public void AddItem(string item)
	{
		string[] array = new string[items.Length + 1];
		Array.Copy(items, array, items.Length);
		array[items.Length] = item;
		items = array;
		Invalidate();
	}

	public void EnsureVisible(int index)
	{
		int num = index * ItemHeight;
		if (scrollPosition > (float)num)
		{
			ScrollPosition = num;
		}
		float num2 = size.y - (float)listPadding.vertical;
		if (scrollPosition + num2 < (float)(num + itemHeight))
		{
			ScrollPosition = (float)num - num2 + (float)itemHeight;
		}
	}

	private void selectItemUnderMouse(dfMouseEventArgs args)
	{
		float num = pivot.TransformToUpperLeft(base.Size).y + ((float)(-itemHeight) * ((float)selectedIndex - scrollPosition) - (float)listPadding.top);
		float num2 = ((float)selectedIndex - scrollPosition + 1f) * (float)itemHeight + (float)listPadding.vertical;
		float num3 = num2 - size.y;
		if (num3 > 0f)
		{
			num += num3;
		}
		float num4 = GetHitPosition(args).y - (float)listPadding.top;
		if (!(num4 < 0f) && !(num4 > size.y - (float)listPadding.bottom))
		{
			SelectedIndex = (int)((scrollPosition + num4) / (float)itemHeight);
			OnItemClicked();
		}
	}

	private void renderHover()
	{
		if (!Application.isPlaying || base.Atlas == null || !base.IsEnabled || hoverIndex < 0 || hoverIndex > items.Length - 1 || string.IsNullOrEmpty(ItemHover))
		{
			return;
		}
		dfAtlas.ItemInfo itemInfo = base.Atlas[ItemHover];
		if (itemInfo == null)
		{
			return;
		}
		Vector3 vector = pivot.TransformToUpperLeft(base.Size);
		Vector3 offset = new Vector3(vector.x + (float)listPadding.left, vector.y - (float)listPadding.top + scrollPosition, 0f);
		float num = PixelsToUnits();
		int num2 = hoverIndex * itemHeight;
		if (animateHover)
		{
			float num3 = Mathf.Abs(hoverTweenLocation - (float)num2);
			float num4 = (size.y - (float)listPadding.vertical) * 0.5f;
			if (num3 > num4)
			{
				hoverTweenLocation = (float)num2 + Mathf.Sign(hoverTweenLocation - (float)num2) * num4;
				num3 = num4;
			}
			float maxDelta = BraveTime.DeltaTime / num * 2f;
			hoverTweenLocation = Mathf.MoveTowards(hoverTweenLocation, num2, maxDelta);
		}
		else
		{
			hoverTweenLocation = num2;
		}
		offset.y -= hoverTweenLocation.Quantize(num);
		Color32 color = ApplyOpacity(base.color);
		dfSprite.RenderOptions renderOptions = default(dfSprite.RenderOptions);
		renderOptions.atlas = atlas;
		renderOptions.color = color;
		renderOptions.fillAmount = 1f;
		renderOptions.flip = dfSpriteFlip.None;
		renderOptions.pixelsToUnits = PixelsToUnits();
		renderOptions.size = new Vector3(size.x - (float)listPadding.horizontal, itemHeight);
		renderOptions.spriteInfo = itemInfo;
		renderOptions.offset = offset;
		dfSprite.RenderOptions options = renderOptions;
		if (itemInfo.border.horizontal > 0 || itemInfo.border.vertical > 0)
		{
			dfSlicedSprite.renderSprite(renderData, options);
		}
		else
		{
			dfSprite.renderSprite(renderData, options);
		}
		if ((float)num2 != hoverTweenLocation)
		{
			Invalidate();
		}
	}

	private void renderSelection()
	{
		if (base.Atlas == null || selectedIndex < 0)
		{
			return;
		}
		dfAtlas.ItemInfo itemInfo = base.Atlas[ItemHighlight];
		if (!(itemInfo == null))
		{
			float pixelsToUnits = PixelsToUnits();
			Vector3 vector = pivot.TransformToUpperLeft(base.Size);
			Vector3 offset = new Vector3(vector.x + (float)listPadding.left, vector.y - (float)listPadding.top + scrollPosition, 0f);
			offset.y -= selectedIndex * itemHeight;
			Color32 color = ApplyOpacity(base.color);
			dfSprite.RenderOptions renderOptions = default(dfSprite.RenderOptions);
			renderOptions.atlas = atlas;
			renderOptions.color = color;
			renderOptions.fillAmount = 1f;
			renderOptions.flip = dfSpriteFlip.None;
			renderOptions.pixelsToUnits = pixelsToUnits;
			renderOptions.size = new Vector3(size.x - (float)listPadding.horizontal, itemHeight);
			renderOptions.spriteInfo = itemInfo;
			renderOptions.offset = offset;
			dfSprite.RenderOptions options = renderOptions;
			if (itemInfo.border.horizontal > 0 || itemInfo.border.vertical > 0)
			{
				dfSlicedSprite.renderSprite(renderData, options);
			}
			else
			{
				dfSprite.renderSprite(renderData, options);
			}
		}
	}

	private float getTextScaleMultiplier()
	{
		if (textScaleMode == dfTextScaleMode.None || !Application.isPlaying)
		{
			return 1f;
		}
		if (textScaleMode == dfTextScaleMode.ScreenResolution)
		{
			return (float)Screen.height / (float)cachedManager.FixedHeight;
		}
		return base.Size.y / startSize.y;
	}

	private void renderItems(dfRenderData buffer)
	{
		if (font == null || items == null || items.Length == 0)
		{
			return;
		}
		float num = PixelsToUnits();
		Vector2 vector = new Vector2(size.x - (float)itemPadding.horizontal - (float)listPadding.horizontal, itemHeight - itemPadding.vertical);
		Vector3 vector2 = pivot.TransformToUpperLeft(base.Size);
		Vector3 vectorOffset = new Vector3(vector2.x + (float)itemPadding.left + (float)listPadding.left, vector2.y - (float)itemPadding.top - (float)listPadding.top, 0f) * num;
		vectorOffset.y += scrollPosition * num;
		Color32 defaultColor = ((!base.IsEnabled) ? base.DisabledColor : ItemTextColor);
		float num2 = vector2.y * num;
		float num3 = num2 - size.y * num;
		for (int i = 0; i < items.Length; i++)
		{
			using (dfFontRendererBase dfFontRendererBase2 = font.ObtainRenderer())
			{
				dfFontRendererBase2.WordWrap = false;
				dfFontRendererBase2.MaxSize = vector;
				dfFontRendererBase2.PixelRatio = num;
				dfFontRendererBase2.TextScale = ItemTextScale * getTextScaleMultiplier();
				dfFontRendererBase2.VectorOffset = vectorOffset;
				dfFontRendererBase2.MultiLine = false;
				dfFontRendererBase2.TextAlign = ItemAlignment;
				dfFontRendererBase2.ProcessMarkup = true;
				dfFontRendererBase2.DefaultColor = defaultColor;
				dfFontRendererBase2.OverrideMarkupColors = false;
				dfFontRendererBase2.Opacity = CalculateOpacity();
				dfFontRendererBase2.Shadow = Shadow;
				dfFontRendererBase2.ShadowColor = ShadowColor;
				dfFontRendererBase2.ShadowOffset = ShadowOffset;
				dfDynamicFont.DynamicFontRenderer dynamicFontRenderer = dfFontRendererBase2 as dfDynamicFont.DynamicFontRenderer;
				if (dynamicFontRenderer != null)
				{
					dynamicFontRenderer.SpriteAtlas = base.Atlas;
					dynamicFontRenderer.SpriteBuffer = renderData;
				}
				if (vectorOffset.y - (float)itemHeight * num <= num2)
				{
					dfFontRendererBase2.Render(items[i], buffer);
				}
				vectorOffset.y -= (float)itemHeight * num;
				dfFontRendererBase2.VectorOffset = vectorOffset;
				if (vectorOffset.y < num3)
				{
					break;
				}
			}
		}
	}

	private void clipQuads(dfRenderData buffer, int startIndex)
	{
		dfList<Vector3> vertices = buffer.Vertices;
		dfList<Vector2> uV = buffer.UV;
		float num = PixelsToUnits();
		float num2 = (base.Pivot.TransformToUpperLeft(base.Size).y - (float)listPadding.top) * num;
		float num3 = num2 - (size.y - (float)listPadding.vertical) * num;
		for (int i = startIndex; i < vertices.Count; i += 4)
		{
			Vector3 vector = vertices[i];
			Vector3 vector2 = vertices[i + 1];
			Vector3 vector3 = vertices[i + 2];
			Vector3 vector4 = vertices[i + 3];
			float num4 = vector.y - vector4.y;
			if (vector4.y < num3)
			{
				float t = 1f - Mathf.Abs(0f - num3 + vector.y) / num4;
				vector = (vertices[i] = new Vector3(vector.x, Mathf.Max(vector.y, num3), vector2.z));
				vector2 = (vertices[i + 1] = new Vector3(vector2.x, Mathf.Max(vector2.y, num3), vector2.z));
				vector3 = (vertices[i + 2] = new Vector3(vector3.x, Mathf.Max(vector3.y, num3), vector3.z));
				vector4 = (vertices[i + 3] = new Vector3(vector4.x, Mathf.Max(vector4.y, num3), vector4.z));
				float y = Mathf.Lerp(uV[i + 3].y, uV[i].y, t);
				uV[i + 3] = new Vector2(uV[i + 3].x, y);
				uV[i + 2] = new Vector2(uV[i + 2].x, y);
				num4 = Mathf.Abs(vector4.y - vector.y);
			}
			if (vector.y > num2)
			{
				float t2 = Mathf.Abs(num2 - vector.y) / num4;
				vertices[i] = new Vector3(vector.x, Mathf.Min(num2, vector.y), vector.z);
				vertices[i + 1] = new Vector3(vector2.x, Mathf.Min(num2, vector2.y), vector2.z);
				vertices[i + 2] = new Vector3(vector3.x, Mathf.Min(num2, vector3.y), vector3.z);
				vertices[i + 3] = new Vector3(vector4.x, Mathf.Min(num2, vector4.y), vector4.z);
				float y2 = Mathf.Lerp(uV[i].y, uV[i + 3].y, t2);
				uV[i] = new Vector2(uV[i].x, y2);
				uV[i + 1] = new Vector2(uV[i + 1].x, y2);
			}
		}
	}

	private void updateItemHover(dfMouseEventArgs args)
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Ray ray = args.Ray;
		RaycastHit hitInfo;
		if (!GetComponent<Collider>().Raycast(ray, out hitInfo, 1000f))
		{
			hoverIndex = -1;
			hoverTweenLocation = 0f;
			return;
		}
		Vector2 position;
		GetHitPosition(ray, out position);
		float num = base.Pivot.TransformToUpperLeft(base.Size).y + ((float)(-itemHeight) * ((float)selectedIndex - scrollPosition) - (float)listPadding.top);
		float num2 = ((float)selectedIndex - scrollPosition + 1f) * (float)itemHeight + (float)listPadding.vertical;
		float num3 = num2 - size.y;
		if (num3 > 0f)
		{
			num += num3;
		}
		float num4 = position.y - (float)listPadding.top;
		int num5 = (int)(scrollPosition + num4) / itemHeight;
		if (num5 != hoverIndex)
		{
			hoverIndex = num5;
			Invalidate();
		}
	}

	private float constrainScrollPosition(float value)
	{
		value = Mathf.Max(0f, value);
		int num = items.Length * itemHeight;
		float num2 = size.y - (float)listPadding.vertical;
		if ((float)num < num2)
		{
			return 0f;
		}
		return Mathf.Min(value, (float)num - num2);
	}

	private void attachScrollbarEvents()
	{
		if (!(scrollbar == null) && !eventsAttached)
		{
			eventsAttached = true;
			scrollbar.ValueChanged += scrollbar_ValueChanged;
			scrollbar.GotFocus += scrollbar_GotFocus;
		}
	}

	private void detachScrollbarEvents()
	{
		if (!(scrollbar == null) && eventsAttached)
		{
			eventsAttached = false;
			scrollbar.ValueChanged -= scrollbar_ValueChanged;
			scrollbar.GotFocus -= scrollbar_GotFocus;
		}
	}

	private void scrollbar_GotFocus(dfControl control, dfFocusEventArgs args)
	{
		Focus();
	}

	private void scrollbar_ValueChanged(dfControl control, float value)
	{
		ScrollPosition = value;
	}

	private void synchronizeScrollbar()
	{
		if (!(scrollbar == null))
		{
			int num = items.Length * itemHeight;
			float scrollSize = size.y - (float)listPadding.vertical;
			scrollbar.IncrementAmount = itemHeight;
			scrollbar.MinValue = 0f;
			scrollbar.MaxValue = num;
			scrollbar.ScrollSize = scrollSize;
			scrollbar.Value = scrollPosition;
		}
	}

	public dfList<dfRenderData> RenderMultiple()
	{
		if (base.Atlas == null || Font == null)
		{
			return null;
		}
		if (!isVisible)
		{
			return null;
		}
		if (renderData == null)
		{
			renderData = dfRenderData.Obtain();
			textRenderData = dfRenderData.Obtain();
			isControlInvalidated = true;
		}
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		if (!isControlInvalidated)
		{
			for (int i = 0; i < buffers.Count; i++)
			{
				buffers[i].Transform = localToWorldMatrix;
			}
			return buffers;
		}
		buffers.Clear();
		renderData.Clear();
		renderData.Material = base.Atlas.Material;
		renderData.Transform = localToWorldMatrix;
		buffers.Add(renderData);
		textRenderData.Clear();
		textRenderData.Material = base.Atlas.Material;
		textRenderData.Transform = localToWorldMatrix;
		buffers.Add(textRenderData);
		renderBackground();
		int count = renderData.Vertices.Count;
		renderHover();
		renderSelection();
		renderItems(textRenderData);
		clipQuads(renderData, count);
		clipQuads(textRenderData, 0);
		isControlInvalidated = false;
		updateCollider();
		return buffers;
	}

	private void bindTextureRebuildCallback()
	{
		if (!isFontCallbackAssigned && !(Font == null) && Font is dfDynamicFont)
		{
			UnityEngine.Font.textureRebuilt += onFontTextureRebuilt;
			isFontCallbackAssigned = true;
		}
	}

	private void unbindTextureRebuildCallback()
	{
		if (isFontCallbackAssigned && !(Font == null))
		{
			if (Font is dfDynamicFont)
			{
				UnityEngine.Font.textureRebuilt -= onFontTextureRebuilt;
			}
			isFontCallbackAssigned = false;
		}
	}

	private void requestCharacterInfo()
	{
		dfDynamicFont dfDynamicFont2 = Font as dfDynamicFont;
		if (!(dfDynamicFont2 == null) && dfFontManager.IsDirty(Font) && items != null && items.Length != 0)
		{
			float textScaleMultiplier = getTextScaleMultiplier();
			int fontSize = Mathf.CeilToInt((float)font.FontSize * textScaleMultiplier);
			for (int i = 0; i < items.Length; i++)
			{
				dfDynamicFont2.AddCharacterRequest(items[i], fontSize, FontStyle.Normal);
			}
		}
	}

	private void onFontTextureRebuilt(Font font)
	{
		if (Font is dfDynamicFont && font == (Font as dfDynamicFont).BaseFont)
		{
			requestCharacterInfo();
			Invalidate();
		}
	}

	public void UpdateFontInfo()
	{
		requestCharacterInfo();
	}
}
