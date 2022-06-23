using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/User Interface/Button")]
[ExecuteInEditMode]
[dfCategory("Basic Controls")]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_button.html")]
[dfTooltip("Provides a basic Button implementation that allows the developer to specify individual sprite images to be used to represent common button states.")]
public class dfButton : dfInteractiveBase, IDFMultiRender, IRendersText
{
	public enum ButtonState
	{
		Default,
		Focus,
		Hover,
		Pressed,
		Disabled
	}

	[SerializeField]
	protected dfFontBase font;

	[SerializeField]
	protected string pressedSprite;

	[SerializeField]
	protected ButtonState state;

	[SerializeField]
	protected dfControl group;

	[SerializeField]
	protected string text = string.Empty;

	[SerializeField]
	public int textPixelOffset;

	[SerializeField]
	public int hoverTextPixelOffset;

	[SerializeField]
	public int downTextPixelOffset;

	[SerializeField]
	protected TextAlignment textAlign = TextAlignment.Center;

	[SerializeField]
	protected dfVerticalAlignment vertAlign = dfVerticalAlignment.Middle;

	[SerializeField]
	protected Color32 normalColor = UnityEngine.Color.white;

	[SerializeField]
	protected Color32 textColor = UnityEngine.Color.white;

	[SerializeField]
	protected Color32 hoverText = UnityEngine.Color.white;

	[SerializeField]
	protected Color32 pressedText = UnityEngine.Color.white;

	[SerializeField]
	protected Color32 focusText = UnityEngine.Color.white;

	[SerializeField]
	protected Color32 disabledText = UnityEngine.Color.white;

	[SerializeField]
	protected Color32 hoverColor = UnityEngine.Color.white;

	[SerializeField]
	protected Color32 pressedColor = UnityEngine.Color.white;

	[SerializeField]
	protected Color32 focusColor = UnityEngine.Color.white;

	[SerializeField]
	protected float textScale = 1f;

	[SerializeField]
	protected dfTextScaleMode textScaleMode;

	[SerializeField]
	protected bool wordWrap;

	[SerializeField]
	protected RectOffset padding = new RectOffset();

	[SerializeField]
	protected bool textShadow;

	[SerializeField]
	protected Color32 shadowColor = UnityEngine.Color.black;

	[SerializeField]
	protected Vector2 shadowOffset = new Vector2(1f, -1f);

	[SerializeField]
	protected bool autoSize;

	[SerializeField]
	protected bool clickWhenSpacePressed = true;

	[SerializeField]
	public bool forceUpperCase;

	[SerializeField]
	public bool manualStateControl;

	protected dfFontBase m_defaultAssignedFont;

	protected float m_defaultAssignedFontTextScale;

	private StringTableManager.GungeonSupportedLanguages m_cachedLanguage;

	private static float m_cachedScaleTileScale = 3f;

	private Vector2 startSize = Vector2.zero;

	private bool isFontCallbackAssigned;

	private dfRenderData textRenderData;

	private dfList<dfRenderData> buffers = dfList<dfRenderData>.Obtain();

	public bool ClickWhenSpacePressed
	{
		get
		{
			return clickWhenSpacePressed;
		}
		set
		{
			clickWhenSpacePressed = value;
		}
	}

	public ButtonState State
	{
		get
		{
			return state;
		}
		set
		{
			if (!manualStateControl && value != state)
			{
				OnButtonStateChanged(value);
				Invalidate();
			}
		}
	}

	public string PressedSprite
	{
		get
		{
			return pressedSprite;
		}
		set
		{
			if (value != pressedSprite)
			{
				pressedSprite = value;
				Invalidate();
			}
		}
	}

	public dfControl ButtonGroup
	{
		get
		{
			return group;
		}
		set
		{
			if (value != group)
			{
				group = value;
				Invalidate();
			}
		}
	}

	public bool AutoSize
	{
		get
		{
			return autoSize;
		}
		set
		{
			if (value != autoSize)
			{
				autoSize = value;
				if (value)
				{
					textAlign = TextAlignment.Left;
				}
				Invalidate();
			}
		}
	}

	public TextAlignment TextAlignment
	{
		get
		{
			if (autoSize)
			{
				return TextAlignment.Left;
			}
			return textAlign;
		}
		set
		{
			if (value != textAlign)
			{
				textAlign = value;
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
			value = value.ConstrainPadding();
			if (!object.Equals(value, padding))
			{
				padding = value;
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
			}
			Invalidate();
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
			if (value != text)
			{
				CheckFontsForLanguage();
				dfFontManager.Invalidate(Font);
				localizationKey = value;
				text = getLocalizedValue(value);
				Invalidate();
			}
		}
	}

	public Color32 TextColor
	{
		get
		{
			return textColor;
		}
		set
		{
			textColor = value;
			Invalidate();
		}
	}

	public Color32 HoverTextColor
	{
		get
		{
			return hoverText;
		}
		set
		{
			hoverText = value;
			Invalidate();
		}
	}

	public Color32 NormalBackgroundColor
	{
		get
		{
			return normalColor;
		}
		set
		{
			normalColor = value;
			Invalidate();
		}
	}

	public Color32 HoverBackgroundColor
	{
		get
		{
			return hoverColor;
		}
		set
		{
			hoverColor = value;
			Invalidate();
		}
	}

	public Color32 PressedTextColor
	{
		get
		{
			return pressedText;
		}
		set
		{
			pressedText = value;
			Invalidate();
		}
	}

	public Color32 PressedBackgroundColor
	{
		get
		{
			return pressedColor;
		}
		set
		{
			pressedColor = value;
			Invalidate();
		}
	}

	public Color32 FocusTextColor
	{
		get
		{
			return focusText;
		}
		set
		{
			focusText = value;
			Invalidate();
		}
	}

	public Color32 FocusBackgroundColor
	{
		get
		{
			return focusColor;
		}
		set
		{
			focusColor = value;
			Invalidate();
		}
	}

	public Color32 DisabledTextColor
	{
		get
		{
			return disabledText;
		}
		set
		{
			disabledText = value;
			Invalidate();
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

	public bool Shadow
	{
		get
		{
			return textShadow;
		}
		set
		{
			if (value != textShadow)
			{
				textShadow = value;
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

	public event PropertyChangedEventHandler<ButtonState> ButtonStateChanged;

	public void ForceState(ButtonState newState)
	{
		if (newState != state)
		{
			OnButtonStateChanged(newState);
			Invalidate();
		}
	}

	public void ModifyLocalizedText(string text)
	{
		CheckFontsForLanguage();
		dfFontManager.Invalidate(Font);
		this.text = text;
		Invalidate();
	}

	protected void CheckFontsForLanguage()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		StringTableManager.GungeonSupportedLanguages currentLanguage = GameManager.Options.CurrentLanguage;
		if (m_cachedLanguage == currentLanguage)
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
			return;
		}
		dfFontBase dfFontBase2 = null;
		float num = m_defaultAssignedFontTextScale;
		if ((bool)Pixelator.Instance)
		{
			m_cachedScaleTileScale = Pixelator.Instance.ScaleTileScale;
		}
		switch (currentLanguage)
		{
		case StringTableManager.GungeonSupportedLanguages.JAPANESE:
		{
			dfFontBase2 = (ResourceCache.Acquire("Alternate Fonts/JackeyFont12_DF") as GameObject).GetComponent<dfFont>();
			float num6 = Mathf.Max(1f, (float)m_defaultAssignedFont.FontSize / 14f);
			num = Mathf.CeilToInt(m_defaultAssignedFontTextScale * num6);
			break;
		}
		case StringTableManager.GungeonSupportedLanguages.CHINESE:
		{
			dfFontBase2 = (ResourceCache.Acquire("Alternate Fonts/SimSun12_DF") as GameObject).GetComponent<dfFont>();
			float num5 = Mathf.Max(1f, (float)m_defaultAssignedFont.FontSize / 14f);
			num = Mathf.CeilToInt(m_defaultAssignedFontTextScale * num5);
			break;
		}
		case StringTableManager.GungeonSupportedLanguages.KOREAN:
		{
			dfFontBase2 = (ResourceCache.Acquire("Alternate Fonts/NanumGothic16_DF") as GameObject).GetComponent<dfFont>();
			float num3 = (float)m_defaultAssignedFont.FontSize / (float)dfFontBase2.FontSize;
			float num4 = Mathf.Max(3f, m_cachedScaleTileScale);
			if (num3 < 1f)
			{
				num3 = (num4 - 1f) / num4;
			}
			num = ((!(num3 > 1f)) ? (m_defaultAssignedFontTextScale * num3) : ((float)Mathf.CeilToInt(m_defaultAssignedFontTextScale * num3)));
			break;
		}
		case StringTableManager.GungeonSupportedLanguages.RUSSIAN:
		{
			dfFontBase2 = (ResourceCache.Acquire("Alternate Fonts/PixelaCYR_15_DF") as GameObject).GetComponent<dfFont>();
			float num2 = (float)m_defaultAssignedFont.FontSize / (float)dfFontBase2.FontSize;
			if (num2 < 1f)
			{
				num2 = 1f;
			}
			num = ((!(num2 > 1f)) ? (m_defaultAssignedFontTextScale * num2) : ((float)Mathf.CeilToInt(m_defaultAssignedFontTextScale * num2)));
			break;
		}
		default:
			dfFontBase2 = m_defaultAssignedFont;
			break;
		}
		if (dfFontBase2 != null && Font != dfFontBase2)
		{
			Font = dfFontBase2;
			if (dfFontBase2 is dfFont)
			{
				base.Atlas = (dfFontBase2 as dfFont).Atlas;
			}
			TextScale = num;
		}
		m_cachedLanguage = currentLanguage;
	}

	protected internal override void OnLocalize()
	{
		base.OnLocalize();
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
		if (forceUpperCase)
		{
			text = text.ToUpperInvariant();
		}
		CheckFontsForLanguage();
		PerformLayout();
	}

	[HideInInspector]
	public override void Invalidate()
	{
		base.Invalidate();
		if (m_cachedLanguage != GameManager.Options.CurrentLanguage)
		{
			CheckFontsForLanguage();
		}
		if (AutoSize)
		{
			autoSizeToText();
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
		startSize = base.Size;
	}

	public float GetAutosizeWidth()
	{
		using (dfFontRendererBase dfFontRendererBase2 = obtainTextRenderer())
		{
			return dfFontRendererBase2.MeasureString(text).RoundToInt().x + (float)padding.horizontal;
		}
	}

	protected internal override void OnEnterFocus(dfFocusEventArgs args)
	{
		if (State != ButtonState.Pressed)
		{
			State = ButtonState.Focus;
		}
		base.OnEnterFocus(args);
	}

	protected internal override void OnLeaveFocus(dfFocusEventArgs args)
	{
		State = ButtonState.Default;
		base.OnLeaveFocus(args);
	}

	protected internal override void OnKeyPress(dfKeyEventArgs args)
	{
		if (ClickWhenSpacePressed && IsInteractive && args.KeyCode == KeyCode.Space)
		{
			OnClick(new dfMouseEventArgs(this, dfMouseButtons.Left, 1, default(Ray), Vector2.zero, 0f));
		}
		else
		{
			base.OnKeyPress(args);
		}
	}

	protected internal override void OnClick(dfMouseEventArgs args)
	{
		if (group != null)
		{
			dfButton[] componentsInChildren = base.transform.parent.GetComponentsInChildren<dfButton>();
			foreach (dfButton dfButton2 in componentsInChildren)
			{
				if (dfButton2 != this && dfButton2.ButtonGroup == ButtonGroup && dfButton2 != this)
				{
					dfButton2.State = ButtonState.Default;
				}
			}
			if (!base.transform.IsChildOf(group.transform))
			{
				Signal(group.gameObject, "OnClick", args);
			}
		}
		base.OnClick(args);
	}

	protected internal override void OnMouseDown(dfMouseEventArgs args)
	{
		if (!(parent is dfTabstrip) || State != ButtonState.Focus)
		{
			State = ButtonState.Pressed;
		}
		base.OnMouseDown(args);
	}

	protected internal override void OnMouseUp(dfMouseEventArgs args)
	{
		if (!base.IsEnabled)
		{
			State = ButtonState.Disabled;
			base.OnMouseUp(args);
			return;
		}
		if (isMouseHovering)
		{
			if (parent is dfTabstrip && ContainsFocus)
			{
				State = ButtonState.Focus;
			}
			else
			{
				State = ButtonState.Hover;
			}
		}
		else if (HasFocus)
		{
			State = ButtonState.Focus;
		}
		else
		{
			State = ButtonState.Default;
		}
		base.OnMouseUp(args);
	}

	protected internal override void OnMouseEnter(dfMouseEventArgs args)
	{
		if (!(parent is dfTabstrip) || State != ButtonState.Focus)
		{
			State = ButtonState.Hover;
		}
		base.OnMouseEnter(args);
	}

	protected internal override void OnMouseLeave(dfMouseEventArgs args)
	{
		if (ContainsFocus)
		{
			State = ButtonState.Focus;
		}
		else
		{
			State = ButtonState.Default;
		}
		base.OnMouseLeave(args);
	}

	protected internal override void OnIsEnabledChanged()
	{
		if (!base.IsEnabled)
		{
			State = ButtonState.Disabled;
		}
		else
		{
			State = ButtonState.Default;
		}
		base.OnIsEnabledChanged();
	}

	protected virtual void OnButtonStateChanged(ButtonState value)
	{
		if (value != ButtonState.Disabled && !base.IsEnabled)
		{
			value = ButtonState.Disabled;
		}
		state = value;
		Signal("OnButtonStateChanged", this, value);
		if (this.ButtonStateChanged != null)
		{
			this.ButtonStateChanged(this, value);
		}
		Invalidate();
	}

	protected override Color32 getActiveColor()
	{
		switch (State)
		{
		case ButtonState.Focus:
			return FocusBackgroundColor;
		case ButtonState.Hover:
			return HoverBackgroundColor;
		case ButtonState.Pressed:
			return PressedBackgroundColor;
		case ButtonState.Disabled:
			return base.DisabledColor;
		default:
			return NormalBackgroundColor;
		}
	}

	private void autoSizeToText()
	{
		if (Font == null || !Font.IsValid || string.IsNullOrEmpty(Text))
		{
			return;
		}
		using (dfFontRendererBase dfFontRendererBase2 = obtainTextRenderer())
		{
			Vector2 vector = dfFontRendererBase2.MeasureString(Text);
			Vector2 vector2 = new Vector2(vector.x + (float)padding.horizontal, vector.y + (float)padding.vertical);
			if (size != vector2)
			{
				SuspendLayout();
				base.Size = vector2;
				ResumeLayout();
			}
		}
	}

	private dfRenderData renderText()
	{
		if (Font == null || !Font.IsValid || string.IsNullOrEmpty(Text))
		{
			return null;
		}
		dfRenderData dfRenderData2 = renderData;
		if (font is dfDynamicFont)
		{
			dfDynamicFont dfDynamicFont2 = (dfDynamicFont)font;
			dfRenderData2 = textRenderData;
			dfRenderData2.Clear();
			dfRenderData2.Material = dfDynamicFont2.Material;
		}
		using (dfFontRendererBase dfFontRendererBase2 = obtainTextRenderer())
		{
			dfFontRendererBase2.Render(text, dfRenderData2);
			return dfRenderData2;
		}
	}

	private dfFontRendererBase obtainTextRenderer()
	{
		Vector2 vector = base.Size - new Vector2(padding.horizontal, padding.vertical);
		Vector2 vector2 = ((!autoSize) ? vector : (Vector2.one * 2.14748365E+09f));
		float num = PixelsToUnits();
		Vector3 vector3 = (pivot.TransformToUpperLeft(base.Size) + new Vector3(padding.left, -padding.top)) * num;
		float num2 = TextScale * getTextScaleMultiplier();
		Color32 defaultColor = ApplyOpacity(getTextColorForState());
		dfFontRendererBase dfFontRendererBase2 = Font.ObtainRenderer();
		dfFontRendererBase2.WordWrap = WordWrap;
		dfFontRendererBase2.MultiLine = WordWrap;
		dfFontRendererBase2.MaxSize = vector2;
		dfFontRendererBase2.PixelRatio = num;
		dfFontRendererBase2.TextScale = num2;
		dfFontRendererBase2.CharacterSpacing = 0;
		int num3 = textPixelOffset;
		if (state == ButtonState.Hover)
		{
			num3 = hoverTextPixelOffset;
		}
		if (state == ButtonState.Pressed)
		{
			num3 = downTextPixelOffset;
		}
		if (state == ButtonState.Disabled)
		{
			num3 = downTextPixelOffset;
		}
		dfFontRendererBase2.VectorOffset = vector3.Quantize(num) + new Vector3(0f, (float)num3 * num, 0f);
		dfFontRendererBase2.TabSize = 0;
		dfFontRendererBase2.TextAlign = ((!autoSize) ? TextAlignment : TextAlignment.Left);
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
			return (float)Screen.height / (float)cachedManager.FixedHeight;
		}
		if (autoSize)
		{
			return 1f;
		}
		return base.Size.y / startSize.y;
	}

	private Color32 getTextColorForState()
	{
		if (!base.IsEnabled)
		{
			return DisabledTextColor;
		}
		switch (state)
		{
		case ButtonState.Default:
			return TextColor;
		case ButtonState.Focus:
			return FocusTextColor;
		case ButtonState.Hover:
			return HoverTextColor;
		case ButtonState.Pressed:
			return PressedTextColor;
		case ButtonState.Disabled:
			return DisabledTextColor;
		default:
			return UnityEngine.Color.white;
		}
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

	protected internal override dfAtlas.ItemInfo getBackgroundSprite()
	{
		if (base.Atlas == null)
		{
			return null;
		}
		dfAtlas.ItemInfo itemInfo = null;
		switch (state)
		{
		case ButtonState.Default:
			itemInfo = atlas[backgroundSprite];
			break;
		case ButtonState.Focus:
			itemInfo = atlas[focusSprite];
			break;
		case ButtonState.Hover:
			itemInfo = atlas[hoverSprite];
			break;
		case ButtonState.Pressed:
			itemInfo = atlas[pressedSprite];
			break;
		case ButtonState.Disabled:
			itemInfo = atlas[disabledSprite];
			break;
		}
		if (itemInfo == null)
		{
			itemInfo = atlas[backgroundSprite];
		}
		return itemInfo;
	}

	public dfList<dfRenderData> RenderMultiple()
	{
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
		isControlInvalidated = false;
		buffers.Clear();
		renderData.Clear();
		if (base.Atlas != null)
		{
			renderData.Material = base.Atlas.Material;
			renderData.Transform = localToWorldMatrix;
			renderBackground();
			buffers.Add(renderData);
		}
		dfRenderData dfRenderData2 = renderText();
		if (dfRenderData2 != null && dfRenderData2 != renderData)
		{
			dfRenderData2.Transform = localToWorldMatrix;
			buffers.Add(dfRenderData2);
		}
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
