using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/User Interface/Label")]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_label.html")]
[dfTooltip("Displays text information, optionally allowing the use of markup to specify colors and embedded sprites")]
[ExecuteInEditMode]
[dfCategory("Basic Controls")]
public class dfLabel : dfControl, IDFMultiRender, IRendersText
{
	public Vector3 PerCharacterOffset;

	[NonSerialized]
	protected dfFontBase m_defaultAssignedFont;

	protected float m_defaultAssignedFontTextScale;

	[SerializeField]
	public bool PreventFontChanges;

	[SerializeField]
	protected dfAtlas atlas;

	[SerializeField]
	protected dfFontBase font;

	[SerializeField]
	protected string backgroundSprite;

	[SerializeField]
	protected Color32 backgroundColor = UnityEngine.Color.white;

	[SerializeField]
	protected bool autoSize;

	[SerializeField]
	protected bool autoHeight;

	[SerializeField]
	protected bool wordWrap;

	[SerializeField]
	protected string text = "Label";

	[SerializeField]
	protected Color32 bottomColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

	[SerializeField]
	protected TextAlignment align;

	[SerializeField]
	protected dfVerticalAlignment vertAlign;

	[SerializeField]
	protected float textScale = 1f;

	[SerializeField]
	protected dfTextScaleMode textScaleMode;

	[SerializeField]
	protected int charSpacing;

	[SerializeField]
	protected bool colorizeSymbols;

	[SerializeField]
	protected bool processMarkup;

	[SerializeField]
	protected bool outline;

	[SerializeField]
	protected int outlineWidth = 1;

	[SerializeField]
	protected bool enableGradient;

	[SerializeField]
	protected Color32 outlineColor = UnityEngine.Color.black;

	[SerializeField]
	protected bool shadow;

	[SerializeField]
	protected Color32 shadowColor = UnityEngine.Color.black;

	[SerializeField]
	protected Vector2 shadowOffset = new Vector2(1f, -1f);

	[SerializeField]
	protected RectOffset padding = new RectOffset();

	[SerializeField]
	protected int tabSize = 48;

	[SerializeField]
	protected List<int> tabStops = new List<int>();

	private Vector2 startSize = Vector2.zero;

	private bool isFontCallbackAssigned;

	private StringTableManager.GungeonSupportedLanguages m_cachedLanguage;

	private RectOffset m_cachedPadding;

	private static float m_cachedScaleTileScale = 3f;

	public bool MaintainJapaneseFont;

	public bool MaintainKoreanFont;

	public bool MaintainRussianFont;

	private dfRenderData textRenderData;

	private dfList<dfRenderData> renderDataBuffers = dfList<dfRenderData>.Obtain();

	[NonSerialized]
	public bool Glitchy;

	public dfFontBase DefaultAssignedFont
	{
		get
		{
			return m_defaultAssignedFont;
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

	public string BackgroundSprite
	{
		get
		{
			return backgroundSprite;
		}
		set
		{
			if (value != backgroundSprite)
			{
				backgroundSprite = value;
				Invalidate();
			}
		}
	}

	public Color32 BackgroundColor
	{
		get
		{
			return backgroundColor;
		}
		set
		{
			if (!object.Equals(value, backgroundColor))
			{
				backgroundColor = value;
				Invalidate();
			}
		}
	}

	public float TextScale
	{
		get
		{
			return textScale;
		}
		set
		{
			value = Mathf.Max(0.1f, value);
			if (!Mathf.Approximately(textScale, value))
			{
				dfFontManager.Invalidate(Font);
				textScale = value;
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

	public int CharacterSpacing
	{
		get
		{
			return charSpacing;
		}
		set
		{
			value = Mathf.Max(0, value);
			if (value != charSpacing)
			{
				charSpacing = value;
				Invalidate();
			}
		}
	}

	public bool ColorizeSymbols
	{
		get
		{
			return colorizeSymbols;
		}
		set
		{
			if (value != colorizeSymbols)
			{
				colorizeSymbols = value;
				Invalidate();
			}
		}
	}

	public bool ProcessMarkup
	{
		get
		{
			return processMarkup;
		}
		set
		{
			if (value != processMarkup)
			{
				processMarkup = value;
				Invalidate();
			}
		}
	}

	public bool ShowGradient
	{
		get
		{
			return enableGradient;
		}
		set
		{
			if (value != enableGradient)
			{
				enableGradient = value;
				Invalidate();
			}
		}
	}

	public Color32 BottomColor
	{
		get
		{
			return bottomColor;
		}
		set
		{
			if (!bottomColor.EqualsNonAlloc(value))
			{
				bottomColor = value;
				OnColorChanged();
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
			value = ((value != null) ? value.Replace("\\t", "\t").Replace("\\n", "\n") : string.Empty);
			if (!string.Equals(value, text))
			{
				dfFontManager.Invalidate(Font);
				localizationKey = value;
				text = getLocalizedValue(value);
				OnTextChanged();
			}
		}
	}

	public bool AutoSize
	{
		get
		{
			if (autoSize && autoHeight)
			{
				autoHeight = false;
			}
			return autoSize;
		}
		set
		{
			if (value != autoSize)
			{
				if (value)
				{
					autoHeight = false;
				}
				autoSize = value;
				Invalidate();
			}
		}
	}

	public bool AutoHeight
	{
		get
		{
			return autoHeight && !autoSize;
		}
		set
		{
			if (value != autoHeight)
			{
				if (value)
				{
					autoSize = false;
				}
				autoHeight = value;
				Invalidate();
			}
		}
	}

	public bool WordWrap
	{
		get
		{
			return wordWrap;
		}
		set
		{
			if (value != wordWrap)
			{
				wordWrap = value;
				Invalidate();
			}
		}
	}

	public TextAlignment TextAlignment
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

	public dfVerticalAlignment VerticalAlignment
	{
		get
		{
			return vertAlign;
		}
		set
		{
			if (value != vertAlign)
			{
				vertAlign = value;
				Invalidate();
			}
		}
	}

	public bool Outline
	{
		get
		{
			return outline;
		}
		set
		{
			if (value != outline)
			{
				outline = value;
				Invalidate();
			}
		}
	}

	public int OutlineSize
	{
		get
		{
			return outlineWidth;
		}
		set
		{
			value = Mathf.Max(0, value);
			if (value != outlineWidth)
			{
				outlineWidth = value;
				Invalidate();
			}
		}
	}

	public Color32 OutlineColor
	{
		get
		{
			return outlineColor;
		}
		set
		{
			if (!value.Equals(outlineColor))
			{
				outlineColor = value;
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

	public RectOffset Padding
	{
		get
		{
			if (padding == null)
			{
				padding = new RectOffset();
			}
			return padding;
		}
		set
		{
			if (!object.Equals(value, padding))
			{
				padding = value;
				Invalidate();
			}
		}
	}

	public int TabSize
	{
		get
		{
			return tabSize;
		}
		set
		{
			value = Mathf.Max(0, value);
			if (value != tabSize)
			{
				tabSize = value;
				Invalidate();
			}
		}
	}

	public List<int> TabStops
	{
		get
		{
			return tabStops;
		}
	}

	public event PropertyChangedEventHandler<string> TextChanged;

	public void ModifyLocalizedText(string text)
	{
		dfFontManager.Invalidate(Font);
		this.text = text;
		OnTextChanged();
	}

	protected void CheckFontsForLanguage()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		StringTableManager.GungeonSupportedLanguages gungeonSupportedLanguages = GameManager.Options.CurrentLanguage;
		if (PreventFontChanges)
		{
			gungeonSupportedLanguages = StringTableManager.GungeonSupportedLanguages.ENGLISH;
		}
		if (m_cachedLanguage == gungeonSupportedLanguages)
		{
			return;
		}
		if (m_defaultAssignedFont == null)
		{
			m_defaultAssignedFontTextScale = textScale;
			m_defaultAssignedFont = font;
		}
		if (m_defaultAssignedFont == null)
		{
			font = base.GUIManager.DefaultFont;
			m_defaultAssignedFont = font;
		}
		if (m_cachedPadding == null)
		{
			m_cachedPadding = padding;
		}
		dfFontBase dfFontBase2 = null;
		float num = m_defaultAssignedFontTextScale;
		if ((bool)Pixelator.Instance)
		{
			m_cachedScaleTileScale = Pixelator.Instance.ScaleTileScale;
		}
		if (gungeonSupportedLanguages == StringTableManager.GungeonSupportedLanguages.JAPANESE && !MaintainJapaneseFont)
		{
			dfFontBase2 = (ResourceCache.Acquire("Alternate Fonts/JackeyFont12_DF") as GameObject).GetComponent<dfFont>();
			float num2 = Mathf.Max(1f, (float)m_defaultAssignedFont.FontSize / 14f);
			num = Mathf.CeilToInt(m_defaultAssignedFontTextScale * num2);
			if (m_defaultAssignedFont.LineHeight < 16)
			{
				int num3 = ((GetManager().FixedHeight <= 1000) ? 3 : 4);
				RectOffset rectOffset = (padding = new RectOffset(m_cachedPadding.left, m_cachedPadding.right, m_cachedPadding.top + num3 * 2, m_cachedPadding.bottom));
			}
			else
			{
				padding = m_cachedPadding;
			}
		}
		else if (gungeonSupportedLanguages == StringTableManager.GungeonSupportedLanguages.CHINESE && !MaintainJapaneseFont)
		{
			dfFontBase2 = (ResourceCache.Acquire("Alternate Fonts/SimSun12_DF") as GameObject).GetComponent<dfFont>();
			float num4 = Mathf.Max(1f, (float)m_defaultAssignedFont.FontSize / 14f);
			num = Mathf.CeilToInt(m_defaultAssignedFontTextScale * num4);
			if (m_defaultAssignedFont.LineHeight < 16)
			{
				int num5 = ((GetManager().FixedHeight <= 1000) ? 3 : 4);
				RectOffset rectOffset2 = (padding = new RectOffset(m_cachedPadding.left, m_cachedPadding.right, m_cachedPadding.top + num5 * 2, m_cachedPadding.bottom));
			}
			else
			{
				padding = m_cachedPadding;
			}
		}
		else if (gungeonSupportedLanguages == StringTableManager.GungeonSupportedLanguages.KOREAN && !MaintainKoreanFont)
		{
			dfFontBase2 = (ResourceCache.Acquire("Alternate Fonts/NanumGothic16_DF") as GameObject).GetComponent<dfFont>();
			float num6 = (float)m_defaultAssignedFont.FontSize / (float)dfFontBase2.FontSize;
			float num7 = Mathf.Max(3f, m_cachedScaleTileScale);
			if (num6 < 1f)
			{
				num6 = (num7 - 1f) / num7;
			}
			num = ((!(num6 > 1f)) ? (m_defaultAssignedFontTextScale * num6) : ((float)Mathf.CeilToInt(m_defaultAssignedFontTextScale * num6)));
			if (m_defaultAssignedFont.LineHeight < 16)
			{
				int num8 = ((GetManager().FixedHeight <= 1000) ? 3 : 4);
				RectOffset rectOffset3 = (padding = new RectOffset(m_cachedPadding.left, m_cachedPadding.right, m_cachedPadding.top + num8 * 2, m_cachedPadding.bottom));
			}
			else
			{
				padding = m_cachedPadding;
			}
		}
		else if (gungeonSupportedLanguages == StringTableManager.GungeonSupportedLanguages.RUSSIAN && !MaintainRussianFont)
		{
			dfFontBase2 = (ResourceCache.Acquire("Alternate Fonts/PixelaCYR_15_DF") as GameObject).GetComponent<dfFont>();
			float num9 = (float)m_defaultAssignedFont.FontSize / (float)dfFontBase2.FontSize;
			if (num9 < 1f)
			{
				num9 = 1f;
			}
			num = ((!(num9 > 1f)) ? (m_defaultAssignedFontTextScale * num9) : ((float)Mathf.CeilToInt(m_defaultAssignedFontTextScale * num9)));
			padding = m_cachedPadding;
		}
		else if (gungeonSupportedLanguages == StringTableManager.GungeonSupportedLanguages.POLISH && !MaintainRussianFont && m_defaultAssignedFont != null && base.GUIManager.DefaultFont != null && m_defaultAssignedFont.name.StartsWith("04b03"))
		{
			dfFontBase2 = base.GUIManager.DefaultFont;
			float num10 = (float)m_defaultAssignedFont.FontSize / (float)dfFontBase2.FontSize;
			if (num10 < 1f)
			{
				num10 = 1f;
			}
			num = ((!(num10 > 1f)) ? (m_defaultAssignedFontTextScale * num10) : ((float)Mathf.CeilToInt(m_defaultAssignedFontTextScale * num10)));
			padding = m_cachedPadding;
		}
		else
		{
			dfFontBase2 = m_defaultAssignedFont;
			padding = m_cachedPadding;
		}
		if (dfFontBase2 != null && Font != dfFontBase2)
		{
			Font = dfFontBase2;
			if (dfFontBase2 is dfFont)
			{
				Atlas = (dfFontBase2 as dfFont).Atlas;
			}
			TextScale = num;
		}
		m_cachedLanguage = gungeonSupportedLanguages;
	}

	protected internal override void OnLocalize()
	{
		base.OnLocalize();
		CheckFontsForLanguage();
		if (text.StartsWith("#"))
		{
			localizationKey = text;
		}
		if (!string.IsNullOrEmpty(localizationKey) && localizationKey.StartsWith("#"))
		{
			text = getLocalizedValue(localizationKey);
		}
		else
		{
			Text = getLocalizedValue(text);
		}
		if (text.StartsWith("#") && text.Contains("ENCNAME"))
		{
			ModifyLocalizedText(StringTableManager.GetItemsString(localizationKey));
		}
		PerformLayout();
	}

	protected internal void OnTextChanged()
	{
		CheckFontsForLanguage();
		Invalidate();
		Signal("OnTextChanged", this, text);
		if (this.TextChanged != null)
		{
			this.TextChanged(this, text);
		}
	}

	public override void Start()
	{
		base.Start();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		bool flag = Font != null && Font.IsValid;
		if (Application.isPlaying && !flag)
		{
			Font = GetManager().DefaultFont;
		}
		bindTextureRebuildCallback();
		if (size.sqrMagnitude <= float.Epsilon)
		{
			base.Size = new Vector2(150f, 25f);
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		unbindTextureRebuildCallback();
	}

	public override void Awake()
	{
		localizationKey = Text;
		base.Awake();
		startSize = ((!Application.isPlaying) ? Vector2.zero : base.Size);
	}

	public override Vector2 CalculateMinimumSize()
	{
		if (Font != null)
		{
			float num = (float)Font.FontSize * TextScale * 0.75f;
			return Vector2.Max(base.CalculateMinimumSize(), new Vector2(num, num));
		}
		return base.CalculateMinimumSize();
	}

	public float GetAutosizeWidth()
	{
		using (dfFontRendererBase dfFontRendererBase2 = obtainRenderer())
		{
			return dfFontRendererBase2.MeasureString(text).RoundToInt().x + (float)padding.horizontal;
		}
	}

	[HideInInspector]
	public override void Invalidate()
	{
		base.Invalidate();
		if (m_cachedLanguage != GameManager.Options.CurrentLanguage)
		{
			CheckFontsForLanguage();
		}
		if (Font == null || !Font.IsValid || GetManager() == null)
		{
			return;
		}
		bool flag = size.sqrMagnitude <= float.Epsilon;
		if (!AutoSize && !autoHeight && !flag)
		{
			return;
		}
		if (string.IsNullOrEmpty(Text))
		{
			Vector2 vector = size;
			Vector2 vector2 = vector;
			if (flag)
			{
				vector2 = new Vector2(150f, 24f);
			}
			if (AutoSize || AutoHeight)
			{
				vector2.y = Mathf.CeilToInt((float)Font.LineHeight * TextScale);
			}
			if (vector != vector2)
			{
				SuspendLayout();
				base.Size = vector2;
				ResumeLayout();
			}
			return;
		}
		Vector2 vector3 = size;
		using (dfFontRendererBase dfFontRendererBase2 = obtainRenderer())
		{
			Vector2 vector4 = dfFontRendererBase2.MeasureString(text).RoundToInt();
			if (AutoSize || flag)
			{
				size = vector4 + new Vector2(padding.horizontal, padding.vertical);
			}
			else if (AutoHeight)
			{
				size = new Vector2(size.x, vector4.y + (float)padding.vertical);
			}
		}
		if ((size - vector3).sqrMagnitude >= 1f)
		{
			raiseSizeChangedEvent();
		}
	}

	private dfFontRendererBase obtainRenderer()
	{
		bool flag = base.Size.sqrMagnitude <= float.Epsilon;
		Vector2 vector = base.Size - new Vector2(padding.horizontal, padding.vertical);
		Vector2 vector2 = ((!AutoSize && !flag) ? vector : getAutoSizeDefault());
		if (autoHeight)
		{
			vector2 = new Vector2(vector.x, 2.14748365E+09f);
		}
		float num = PixelsToUnits();
		Vector3 vector3 = (pivot.TransformToUpperLeft(base.Size) + new Vector3(padding.left, -padding.top)) * num;
		float num2 = TextScale * getTextScaleMultiplier();
		dfFontRendererBase dfFontRendererBase2 = Font.ObtainRenderer();
		dfFontRendererBase2.WordWrap = WordWrap;
		dfFontRendererBase2.MaxSize = vector2;
		dfFontRendererBase2.PixelRatio = num;
		dfFontRendererBase2.TextScale = num2;
		dfFontRendererBase2.CharacterSpacing = CharacterSpacing;
		dfFontRendererBase2.VectorOffset = vector3.Quantize(num);
		dfFontRendererBase2.PerCharacterAccumulatedOffset = PerCharacterOffset * num;
		dfFontRendererBase2.MultiLine = true;
		dfFontRendererBase2.TabSize = TabSize;
		dfFontRendererBase2.TabStops = TabStops;
		dfFontRendererBase2.TextAlign = ((!AutoSize) ? TextAlignment : TextAlignment.Left);
		dfFontRendererBase2.ColorizeSymbols = ColorizeSymbols;
		dfFontRendererBase2.ProcessMarkup = ProcessMarkup;
		dfFontRendererBase2.DefaultColor = ((!base.IsEnabled) ? base.DisabledColor : base.Color);
		dfFontRendererBase2.BottomColor = ((!enableGradient) ? null : new Color32?(BottomColor));
		dfFontRendererBase2.OverrideMarkupColors = !base.IsEnabled;
		dfFontRendererBase2.Opacity = CalculateOpacity();
		dfFontRendererBase2.Outline = Outline;
		dfFontRendererBase2.OutlineSize = OutlineSize;
		dfFontRendererBase2.OutlineColor = OutlineColor;
		dfFontRendererBase2.Shadow = Shadow;
		dfFontRendererBase2.ShadowColor = ShadowColor;
		dfFontRendererBase2.ShadowOffset = ShadowOffset;
		dfDynamicFont.DynamicFontRenderer dynamicFontRenderer = dfFontRendererBase2 as dfDynamicFont.DynamicFontRenderer;
		if (dynamicFontRenderer != null)
		{
			dynamicFontRenderer.SpriteAtlas = Atlas;
			dynamicFontRenderer.SpriteBuffer = renderData;
		}
		if (vertAlign != 0)
		{
			dfFontRendererBase2.VectorOffset = getVertAlignOffset(dfFontRendererBase2);
		}
		return dfFontRendererBase2;
	}

	private float getTextScaleMultiplier()
	{
		if (textScaleMode == dfTextScaleMode.None || !Application.isPlaying)
		{
			return 1f;
		}
		if (textScaleMode == dfTextScaleMode.ScreenResolution)
		{
			return GetManager().GetScreenSize().y / (float)GetManager().FixedHeight;
		}
		if (AutoSize)
		{
			return 1f;
		}
		return base.Size.y / startSize.y;
	}

	private Vector2 getAutoSizeDefault()
	{
		float x = ((!(maxSize.x > float.Epsilon)) ? 2.14748365E+09f : maxSize.x);
		float y = ((!(maxSize.y > float.Epsilon)) ? 2.14748365E+09f : maxSize.y);
		return new Vector2(x, y);
	}

	private Vector3 getVertAlignOffset(dfFontRendererBase textRenderer)
	{
		float num = PixelsToUnits();
		Vector2 vector = textRenderer.MeasureString(text) * num;
		Vector3 vectorOffset = textRenderer.VectorOffset;
		float num2 = (base.Height - (float)padding.vertical) * num;
		if (vector.y >= num2)
		{
			return vectorOffset;
		}
		switch (vertAlign)
		{
		case dfVerticalAlignment.Middle:
			vectorOffset.y -= (num2 - vector.y) * 0.5f;
			break;
		case dfVerticalAlignment.Bottom:
			vectorOffset.y -= num2 - vector.y;
			break;
		}
		return vectorOffset;
	}

	protected internal virtual void renderBackground()
	{
		if (Atlas == null)
		{
			return;
		}
		dfAtlas.ItemInfo itemInfo = Atlas[backgroundSprite];
		if (!(itemInfo == null))
		{
			Color32 color = ApplyOpacity(BackgroundColor);
			dfSprite.RenderOptions renderOptions = default(dfSprite.RenderOptions);
			renderOptions.atlas = atlas;
			renderOptions.color = color;
			renderOptions.fillAmount = 1f;
			renderOptions.flip = dfSpriteFlip.None;
			renderOptions.offset = pivot.TransformToUpperLeft(base.Size);
			renderOptions.pixelsToUnits = PixelsToUnits();
			renderOptions.size = base.Size;
			renderOptions.spriteInfo = itemInfo;
			dfSprite.RenderOptions options = renderOptions;
			if (itemInfo.border.horizontal == 0 && itemInfo.border.vertical == 0)
			{
				dfSprite.renderSprite(renderData, options);
			}
			else
			{
				dfSlicedSprite.renderSprite(renderData, options);
			}
		}
	}

	public dfList<dfRenderData> RenderMultiple()
	{
		try
		{
			if (!isControlInvalidated && (textRenderData != null || renderData != null))
			{
				Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
				for (int i = 0; i < renderDataBuffers.Count; i++)
				{
					renderDataBuffers[i].Transform = localToWorldMatrix;
				}
				return renderDataBuffers;
			}
			if (Atlas == null || Font == null || !isVisible)
			{
				return null;
			}
			if (renderData == null)
			{
				renderData = dfRenderData.Obtain();
				textRenderData = dfRenderData.Obtain();
				isControlInvalidated = true;
			}
			resetRenderBuffers();
			renderBackground();
			if (string.IsNullOrEmpty(Text))
			{
				if (AutoSize || AutoHeight)
				{
					base.Height = Mathf.CeilToInt((float)Font.LineHeight * TextScale);
				}
				return renderDataBuffers;
			}
			bool flag = size.sqrMagnitude <= float.Epsilon;
			using (dfFontRendererBase dfFontRendererBase2 = obtainRenderer())
			{
				textRenderData.Glitchy = Glitchy;
				dfFontRendererBase2.Render(text, textRenderData);
				if (AutoSize || flag)
				{
					base.Size = (dfFontRendererBase2.RenderedSize + new Vector2(padding.horizontal, padding.vertical)).CeilToInt();
				}
				else if (AutoHeight)
				{
					base.Size = new Vector2(size.x, dfFontRendererBase2.RenderedSize.y + (float)padding.vertical).CeilToInt();
				}
			}
			updateCollider();
			return renderDataBuffers;
		}
		finally
		{
			isControlInvalidated = false;
		}
	}

	private void resetRenderBuffers()
	{
		renderDataBuffers.Clear();
		Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
		if (renderData != null)
		{
			renderData.Clear();
			renderData.Material = Atlas.Material;
			renderData.Transform = localToWorldMatrix;
			renderDataBuffers.Add(renderData);
		}
		if (textRenderData != null)
		{
			textRenderData.Clear();
			textRenderData.Material = Atlas.Material;
			textRenderData.Transform = localToWorldMatrix;
			renderDataBuffers.Add(textRenderData);
		}
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
		if (!(dfDynamicFont2 == null) && dfFontManager.IsDirty(Font) && !string.IsNullOrEmpty(text))
		{
			float num = TextScale * getTextScaleMultiplier();
			int fontSize = Mathf.CeilToInt((float)font.FontSize * num);
			dfDynamicFont2.AddCharacterRequest(text, fontSize, FontStyle.Normal);
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
