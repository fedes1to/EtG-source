using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/User Interface/Rich Text Label")]
[ExecuteInEditMode]
public class dfRichTextLabel : dfControl, IDFMultiRender, IRendersText
{
	[dfEventCategory("Markup")]
	public delegate void LinkClickEventHandler(dfRichTextLabel sender, dfMarkupTagAnchor tag);

	[SerializeField]
	protected dfAtlas atlas;

	[SerializeField]
	protected dfDynamicFont font;

	[SerializeField]
	protected string text = "Rich Text Label";

	[SerializeField]
	protected int fontSize = 16;

	[SerializeField]
	protected int lineheight = 16;

	[SerializeField]
	protected dfTextScaleMode textScaleMode;

	[SerializeField]
	protected FontStyle fontStyle;

	[SerializeField]
	protected bool preserveWhitespace;

	[SerializeField]
	protected string blankTextureSprite;

	[SerializeField]
	protected dfMarkupTextAlign align;

	[SerializeField]
	protected bool allowScrolling;

	[SerializeField]
	protected dfScrollbar horzScrollbar;

	[SerializeField]
	protected dfScrollbar vertScrollbar;

	[SerializeField]
	protected bool useScrollMomentum;

	[SerializeField]
	protected bool autoHeight;

	private static dfRenderData clipBuffer = new dfRenderData();

	private dfList<dfRenderData> buffers = new dfList<dfRenderData>();

	private dfList<dfMarkupElement> elements;

	private dfMarkupBox viewportBox;

	private dfMarkupTag mouseDownTag;

	private Vector2 mouseDownScrollPosition = Vector2.zero;

	private Vector2 scrollPosition = Vector2.zero;

	private bool initialized;

	private bool isMouseDown;

	private Vector2 touchStartPosition = Vector2.zero;

	private Vector2 scrollMomentum = Vector2.zero;

	private bool isMarkupInvalidated = true;

	private Vector2 startSize = Vector2.zero;

	private bool isFontCallbackAssigned;

	public bool AutoHeight
	{
		get
		{
			return autoHeight;
		}
		set
		{
			if (autoHeight != value)
			{
				autoHeight = value;
				scrollPosition = Vector2.zero;
				Invalidate();
			}
		}
	}

	public dfAtlas Atlas
	{
		get
		{
			if (atlas == null)
			{
				dfGUIManager manager = GetManager();
				if (manager != null)
				{
					return atlas = manager.DefaultAtlas;
				}
			}
			return atlas;
		}
		set
		{
			if (!dfAtlas.Equals(value, atlas))
			{
				atlas = value;
				Invalidate();
			}
		}
	}

	public dfDynamicFont Font
	{
		get
		{
			return font;
		}
		set
		{
			if (value != font)
			{
				unbindTextureRebuildCallback();
				font = value;
				bindTextureRebuildCallback();
				LineHeight = value.FontSize;
				dfFontManager.Invalidate(Font);
				Invalidate();
			}
		}
	}

	public string BlankTextureSprite
	{
		get
		{
			return blankTextureSprite;
		}
		set
		{
			if (value != blankTextureSprite)
			{
				blankTextureSprite = value;
				Invalidate();
			}
		}
	}

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			value = getLocalizedValue(value);
			if (!string.Equals(text, value))
			{
				dfFontManager.Invalidate(Font);
				text = value;
				scrollPosition = Vector2.zero;
				Invalidate();
				OnTextChanged();
			}
		}
	}

	public int FontSize
	{
		get
		{
			return fontSize;
		}
		set
		{
			value = Mathf.Max(6, value);
			if (value != fontSize)
			{
				dfFontManager.Invalidate(Font);
				fontSize = value;
				Invalidate();
			}
			LineHeight = value;
		}
	}

	public int LineHeight
	{
		get
		{
			return lineheight;
		}
		set
		{
			value = Mathf.Max(FontSize, value);
			if (value != lineheight)
			{
				lineheight = value;
				Invalidate();
			}
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

	public bool PreserveWhitespace
	{
		get
		{
			return preserveWhitespace;
		}
		set
		{
			if (value != preserveWhitespace)
			{
				preserveWhitespace = value;
				Invalidate();
			}
		}
	}

	public FontStyle FontStyle
	{
		get
		{
			return fontStyle;
		}
		set
		{
			if (value != fontStyle)
			{
				fontStyle = value;
				Invalidate();
			}
		}
	}

	public dfMarkupTextAlign TextAlignment
	{
		get
		{
			return align;
		}
		set
		{
			if (value != align)
			{
				align = value;
				Invalidate();
			}
		}
	}

	public bool AllowScrolling
	{
		get
		{
			return allowScrolling;
		}
		set
		{
			allowScrolling = value;
			if (!value)
			{
				ScrollPosition = Vector2.zero;
			}
		}
	}

	public Vector2 ScrollPosition
	{
		get
		{
			return scrollPosition;
		}
		set
		{
			if (!allowScrolling || autoHeight)
			{
				value = Vector2.zero;
			}
			if (isMarkupInvalidated)
			{
				processMarkup();
			}
			Vector2 lhs = ContentSize - base.Size;
			value = Vector2.Min(lhs, value);
			value = Vector2.Max(Vector2.zero, value);
			value = value.RoundToInt();
			if ((value - scrollPosition).sqrMagnitude > float.Epsilon)
			{
				scrollPosition = value;
				updateScrollbars();
				OnScrollPositionChanged();
			}
		}
	}

	public dfScrollbar HorizontalScrollbar
	{
		get
		{
			return horzScrollbar;
		}
		set
		{
			horzScrollbar = value;
			updateScrollbars();
		}
	}

	public dfScrollbar VerticalScrollbar
	{
		get
		{
			return vertScrollbar;
		}
		set
		{
			vertScrollbar = value;
			updateScrollbars();
		}
	}

	public Vector2 ContentSize
	{
		get
		{
			if (viewportBox != null)
			{
				return viewportBox.Size;
			}
			return base.Size;
		}
	}

	public bool UseScrollMomentum
	{
		get
		{
			return useScrollMomentum;
		}
		set
		{
			useScrollMomentum = value;
			scrollMomentum = Vector2.zero;
		}
	}

	public event PropertyChangedEventHandler<string> TextChanged;

	public event PropertyChangedEventHandler<Vector2> ScrollPositionChanged;

	public event LinkClickEventHandler LinkClicked;

	protected internal override void OnLocalize()
	{
		base.OnLocalize();
		Text = getLocalizedValue(text);
	}

	[HideInInspector]
	public override void Invalidate()
	{
		base.Invalidate();
		dfFontManager.Invalidate(Font);
		isMarkupInvalidated = true;
	}

	public override void Awake()
	{
		base.Awake();
		startSize = base.Size;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		bindTextureRebuildCallback();
		if (size.sqrMagnitude <= float.Epsilon)
		{
			base.Size = new Vector2(320f, 200f);
			int num3 = (FontSize = (LineHeight = 16));
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		unbindTextureRebuildCallback();
	}

	public override void Update()
	{
		base.Update();
		if (useScrollMomentum && !isMouseDown && scrollMomentum.magnitude > 0.5f)
		{
			ScrollPosition += scrollMomentum;
			scrollMomentum *= 0.95f - BraveTime.DeltaTime;
		}
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		initialize();
	}

	protected internal void OnTextChanged()
	{
		Invalidate();
		Signal("OnTextChanged", this, text);
		if (this.TextChanged != null)
		{
			this.TextChanged(this, text);
		}
	}

	protected internal void OnScrollPositionChanged()
	{
		base.Invalidate();
		SignalHierarchy("OnScrollPositionChanged", this, ScrollPosition);
		if (this.ScrollPositionChanged != null)
		{
			this.ScrollPositionChanged(this, ScrollPosition);
		}
	}

	protected internal override void OnKeyDown(dfKeyEventArgs args)
	{
		if (args.Used)
		{
			base.OnKeyDown(args);
			return;
		}
		int num = FontSize;
		int num2 = FontSize;
		switch (args.KeyCode)
		{
		case KeyCode.LeftArrow:
			ScrollPosition += new Vector2(-num, 0f);
			args.Use();
			break;
		case KeyCode.RightArrow:
			ScrollPosition += new Vector2(num, 0f);
			args.Use();
			break;
		case KeyCode.UpArrow:
			ScrollPosition += new Vector2(0f, -num2);
			args.Use();
			break;
		case KeyCode.DownArrow:
			ScrollPosition += new Vector2(0f, num2);
			args.Use();
			break;
		case KeyCode.Home:
			ScrollToTop();
			args.Use();
			break;
		case KeyCode.End:
			ScrollToBottom();
			args.Use();
			break;
		}
		base.OnKeyDown(args);
	}

	internal override void OnDragEnd(dfDragEventArgs args)
	{
		base.OnDragEnd(args);
		isMouseDown = false;
	}

	protected internal override void OnMouseEnter(dfMouseEventArgs args)
	{
		base.OnMouseEnter(args);
		touchStartPosition = args.Position;
	}

	protected internal override void OnMouseDown(dfMouseEventArgs args)
	{
		base.OnMouseDown(args);
		mouseDownTag = hitTestTag(args);
		mouseDownScrollPosition = scrollPosition;
		scrollMomentum = Vector2.zero;
		touchStartPosition = args.Position;
		isMouseDown = true;
	}

	protected internal override void OnMouseUp(dfMouseEventArgs args)
	{
		base.OnMouseUp(args);
		isMouseDown = false;
		if (Vector2.Distance(scrollPosition, mouseDownScrollPosition) <= 2f && hitTestTag(args) == mouseDownTag)
		{
			dfMarkupTag dfMarkupTag2 = mouseDownTag;
			while (dfMarkupTag2 != null && !(dfMarkupTag2 is dfMarkupTagAnchor))
			{
				dfMarkupTag2 = dfMarkupTag2.Parent as dfMarkupTag;
			}
			if (dfMarkupTag2 is dfMarkupTagAnchor)
			{
				Signal("OnLinkClicked", this, dfMarkupTag2);
				if (this.LinkClicked != null)
				{
					this.LinkClicked(this, dfMarkupTag2 as dfMarkupTagAnchor);
				}
			}
		}
		mouseDownTag = null;
		mouseDownScrollPosition = scrollPosition;
	}

	protected internal override void OnMouseMove(dfMouseEventArgs args)
	{
		base.OnMouseMove(args);
		if (allowScrolling && !autoHeight && (args is dfTouchEventArgs || isMouseDown) && (args.Position - touchStartPosition).magnitude > 5f)
		{
			Vector2 vector = args.MoveDelta.Scale(-1f, 1f);
			dfGUIManager manager = GetManager();
			Vector2 screenSize = manager.GetScreenSize();
			Camera camera = Camera.main ?? GetCamera();
			vector.x = screenSize.x * (vector.x / (float)camera.pixelWidth);
			vector.y = screenSize.y * (vector.y / (float)camera.pixelHeight);
			ScrollPosition += vector;
			scrollMomentum = (scrollMomentum + vector) * 0.5f;
		}
	}

	protected internal override void OnMouseWheel(dfMouseEventArgs args)
	{
		try
		{
			if (!args.Used && allowScrolling && !autoHeight)
			{
				int num = (UseScrollMomentum ? 1 : 3);
				float num2 = ((!(vertScrollbar != null)) ? ((float)(FontSize * num)) : vertScrollbar.IncrementAmount);
				ScrollPosition = new Vector2(scrollPosition.x, scrollPosition.y - num2 * args.WheelDelta);
				scrollMomentum = new Vector2(0f, (0f - num2) * args.WheelDelta);
				args.Use();
				Signal("OnMouseWheel", this, args);
			}
		}
		finally
		{
			base.OnMouseWheel(args);
		}
	}

	public void ScrollToTop()
	{
		ScrollPosition = new Vector2(scrollPosition.x, 0f);
	}

	public void ScrollToBottom()
	{
		ScrollPosition = new Vector2(scrollPosition.x, 2.14748365E+09f);
	}

	public void ScrollToLeft()
	{
		ScrollPosition = new Vector2(0f, scrollPosition.y);
	}

	public void ScrollToRight()
	{
		ScrollPosition = new Vector2(2.14748365E+09f, scrollPosition.y);
	}

	public dfList<dfRenderData> RenderMultiple()
	{
		if (!isVisible || Font == null)
		{
			return null;
		}
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		if (!isControlInvalidated && viewportBox != null)
		{
			for (int i = 0; i < buffers.Count; i++)
			{
				buffers[i].Transform = localToWorldMatrix;
			}
			return buffers;
		}
		try
		{
			isControlInvalidated = false;
			if (isMarkupInvalidated)
			{
				isMarkupInvalidated = false;
				processMarkup();
			}
			viewportBox.FitToContents();
			if (autoHeight)
			{
				base.Height = viewportBox.Height;
			}
			updateScrollbars();
			buffers.Clear();
			gatherRenderBuffers(viewportBox, buffers);
			return buffers;
		}
		finally
		{
			updateCollider();
		}
	}

	private dfMarkupTag hitTestTag(dfMouseEventArgs args)
	{
		Vector2 point = GetHitPosition(args) + scrollPosition;
		dfMarkupBox dfMarkupBox2 = viewportBox.HitTest(point);
		if (dfMarkupBox2 != null)
		{
			dfMarkupElement element = dfMarkupBox2.Element;
			while (element != null && !(element is dfMarkupTag))
			{
				element = element.Parent;
			}
			return element as dfMarkupTag;
		}
		return null;
	}

	private void processMarkup()
	{
		releaseMarkupReferences();
		elements = dfMarkupParser.Parse(this, text);
		float textScaleMultiplier = getTextScaleMultiplier();
		int num = Mathf.CeilToInt((float)FontSize * textScaleMultiplier);
		int lineHeight = Mathf.CeilToInt((float)LineHeight * textScaleMultiplier);
		dfMarkupStyle dfMarkupStyle2 = default(dfMarkupStyle);
		dfMarkupStyle2.Host = this;
		dfMarkupStyle2.Atlas = Atlas;
		dfMarkupStyle2.Font = Font;
		dfMarkupStyle2.FontSize = num;
		dfMarkupStyle2.FontStyle = FontStyle;
		dfMarkupStyle2.LineHeight = lineHeight;
		dfMarkupStyle2.Color = ApplyOpacity(base.Color);
		dfMarkupStyle2.Opacity = CalculateOpacity();
		dfMarkupStyle2.Align = TextAlignment;
		dfMarkupStyle2.PreserveWhitespace = preserveWhitespace;
		dfMarkupStyle style = dfMarkupStyle2;
		viewportBox = new dfMarkupBox(null, dfMarkupDisplayType.block, style)
		{
			Size = base.Size
		};
		for (int i = 0; i < elements.Count; i++)
		{
			dfMarkupElement dfMarkupElement2 = elements[i];
			if (dfMarkupElement2 != null)
			{
				dfMarkupElement2.PerformLayout(viewportBox, style);
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

	private void releaseMarkupReferences()
	{
		mouseDownTag = null;
		if (viewportBox != null)
		{
			viewportBox.Release();
		}
		if (elements != null)
		{
			for (int i = 0; i < elements.Count; i++)
			{
				elements[i].Release();
			}
			elements.Release();
		}
	}

	[HideInInspector]
	private void initialize()
	{
		if (initialized)
		{
			return;
		}
		initialized = true;
		if (Application.isPlaying)
		{
			if (horzScrollbar != null)
			{
				horzScrollbar.ValueChanged += horzScroll_ValueChanged;
			}
			if (vertScrollbar != null)
			{
				vertScrollbar.ValueChanged += vertScroll_ValueChanged;
			}
		}
		Invalidate();
		ScrollPosition = Vector2.zero;
		updateScrollbars();
	}

	private void vertScroll_ValueChanged(dfControl control, float value)
	{
		ScrollPosition = new Vector2(scrollPosition.x, value);
	}

	private void horzScroll_ValueChanged(dfControl control, float value)
	{
		ScrollPosition = new Vector2(value, ScrollPosition.y);
	}

	private void updateScrollbars()
	{
		if (horzScrollbar != null)
		{
			horzScrollbar.MinValue = 0f;
			horzScrollbar.MaxValue = ContentSize.x;
			horzScrollbar.ScrollSize = base.Size.x;
			horzScrollbar.Value = ScrollPosition.x;
		}
		if (vertScrollbar != null)
		{
			vertScrollbar.MinValue = 0f;
			vertScrollbar.MaxValue = ContentSize.y;
			vertScrollbar.ScrollSize = base.Size.y;
			vertScrollbar.Value = ScrollPosition.y;
		}
	}

	private void gatherRenderBuffers(dfMarkupBox box, dfList<dfRenderData> buffers)
	{
		dfIntersectionType viewportIntersection = getViewportIntersection(box);
		if (viewportIntersection == dfIntersectionType.None)
		{
			return;
		}
		dfRenderData dfRenderData2 = box.Render();
		if (dfRenderData2 != null)
		{
			if (dfRenderData2.Material == null && atlas != null)
			{
				dfRenderData2.Material = atlas.Material;
			}
			float num = PixelsToUnits();
			Vector2 vector = -scrollPosition.Scale(1f, -1f).RoundToInt();
			Vector3 vector2 = (Vector3)(vector + box.GetOffset().Scale(1f, -1f)) + pivot.TransformToUpperLeft(base.Size);
			dfList<Vector3> vertices = dfRenderData2.Vertices;
			for (int i = 0; i < dfRenderData2.Vertices.Count; i++)
			{
				vertices[i] = (vector2 + vertices[i]) * num;
			}
			if (viewportIntersection == dfIntersectionType.Intersecting)
			{
				clipToViewport(dfRenderData2);
			}
			dfRenderData2.Transform = base.transform.localToWorldMatrix;
			buffers.Add(dfRenderData2);
		}
		for (int j = 0; j < box.Children.Count; j++)
		{
			gatherRenderBuffers(box.Children[j], buffers);
		}
	}

	private dfIntersectionType getViewportIntersection(dfMarkupBox box)
	{
		if (box.Display == dfMarkupDisplayType.none)
		{
			return dfIntersectionType.None;
		}
		Vector2 vector = base.Size;
		Vector2 vector2 = box.GetOffset() - scrollPosition;
		Vector2 vector3 = vector2 + box.Size;
		if (vector3.x <= 0f || vector3.y <= 0f)
		{
			return dfIntersectionType.None;
		}
		if (vector2.x >= vector.x || vector2.y >= vector.y)
		{
			return dfIntersectionType.None;
		}
		if (vector2.x < 0f || vector2.y < 0f || vector3.x > vector.x || vector3.y > vector.y)
		{
			return dfIntersectionType.Intersecting;
		}
		return dfIntersectionType.Inside;
	}

	private void clipToViewport(dfRenderData renderData)
	{
		Plane[] viewportClippingPlanes = getViewportClippingPlanes();
		Material material = renderData.Material;
		Matrix4x4 matrix4x = renderData.Transform;
		clipBuffer.Clear();
		dfClippingUtil.Clip(viewportClippingPlanes, renderData, clipBuffer);
		renderData.Clear();
		renderData.Merge(clipBuffer, false);
		renderData.Material = material;
		renderData.Transform = matrix4x;
	}

	private Plane[] getViewportClippingPlanes()
	{
		Vector3[] corners = GetCorners();
		Matrix4x4 worldToLocalMatrix = base.transform.worldToLocalMatrix;
		for (int i = 0; i < corners.Length; i++)
		{
			corners[i] = worldToLocalMatrix.MultiplyPoint(corners[i]);
		}
		cachedClippingPlanes[0] = new Plane(Vector3.right, corners[0]);
		cachedClippingPlanes[1] = new Plane(Vector3.left, corners[1]);
		cachedClippingPlanes[2] = new Plane(Vector3.up, corners[2]);
		cachedClippingPlanes[3] = new Plane(Vector3.down, corners[0]);
		return cachedClippingPlanes;
	}

	public void UpdateFontInfo()
	{
		if (dfFontManager.IsDirty(Font) && !string.IsNullOrEmpty(text))
		{
			updateFontInfo(viewportBox);
		}
	}

	private void updateFontInfo(dfMarkupBox box)
	{
		if (box != null && (box == viewportBox || getViewportIntersection(box) != 0))
		{
			dfMarkupBoxText dfMarkupBoxText2 = box as dfMarkupBoxText;
			if (dfMarkupBoxText2 != null)
			{
				font.AddCharacterRequest(dfMarkupBoxText2.Text, dfMarkupBoxText2.Style.FontSize, dfMarkupBoxText2.Style.FontStyle);
			}
			for (int i = 0; i < box.Children.Count; i++)
			{
				updateFontInfo(box.Children[i]);
			}
		}
	}

	private void onFontTextureRebuilt(Font font)
	{
		if ((object)Font != null && font == Font.BaseFont)
		{
			Invalidate();
			updateFontInfo(viewportBox);
		}
	}

	private void bindTextureRebuildCallback()
	{
		if (!isFontCallbackAssigned && !(Font == null))
		{
			UnityEngine.Font.textureRebuilt += onFontTextureRebuilt;
			isFontCallbackAssigned = true;
		}
	}

	private void unbindTextureRebuildCallback()
	{
		if (isFontCallbackAssigned && !(Font == null))
		{
			UnityEngine.Font.textureRebuilt -= onFontTextureRebuilt;
			isFontCallbackAssigned = false;
		}
	}
}
