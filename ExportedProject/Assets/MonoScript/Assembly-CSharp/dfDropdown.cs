using System;
using UnityEngine;

[Serializable]
[dfTooltip("Implements a drop-down list control")]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_dropdown.html")]
[ExecuteInEditMode]
[dfCategory("Basic Controls")]
[AddComponentMenu("Daikon Forge/User Interface/Dropdown List")]
public class dfDropdown : dfInteractiveBase, IDFMultiRender, IRendersText
{
	public enum PopupListPosition
	{
		Below,
		Above,
		Automatic
	}

	[dfEventCategory("Popup")]
	public delegate void PopupEventHandler(dfDropdown dropdown, dfListbox popup, ref bool overridden);

	[SerializeField]
	protected dfFontBase font;

	[SerializeField]
	protected int selectedIndex = -1;

	[SerializeField]
	protected dfControl triggerButton;

	[SerializeField]
	protected Color32 disabledTextColor = UnityEngine.Color.gray;

	[SerializeField]
	protected Color32 textColor = UnityEngine.Color.white;

	[SerializeField]
	protected float textScale = 1f;

	[SerializeField]
	protected RectOffset textFieldPadding = new RectOffset();

	[SerializeField]
	protected PopupListPosition listPosition;

	[SerializeField]
	protected int listWidth;

	[SerializeField]
	protected int listHeight = 200;

	[SerializeField]
	protected RectOffset listPadding = new RectOffset();

	[SerializeField]
	protected dfScrollbar listScrollbar;

	[SerializeField]
	protected int itemHeight = 25;

	[SerializeField]
	protected string itemHighlight = string.Empty;

	[SerializeField]
	protected string itemHover = string.Empty;

	[SerializeField]
	protected string listBackground = string.Empty;

	[SerializeField]
	protected Vector2 listOffset = Vector2.zero;

	[SerializeField]
	protected string[] items = new string[0];

	[SerializeField]
	protected bool shadow;

	[SerializeField]
	protected Color32 shadowColor = UnityEngine.Color.black;

	[SerializeField]
	protected Vector2 shadowOffset = new Vector2(1f, -1f);

	[SerializeField]
	protected bool openOnMouseDown = true;

	[SerializeField]
	protected bool clickWhenSpacePressed = true;

	private bool eventsAttached;

	private bool isFontCallbackAssigned;

	private dfListbox popup;

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
				ClosePopup();
				unbindTextureRebuildCallback();
				font = value;
				bindTextureRebuildCallback();
				Invalidate();
			}
		}
	}

	public dfScrollbar ListScrollbar
	{
		get
		{
			return listScrollbar;
		}
		set
		{
			if (value != listScrollbar)
			{
				listScrollbar = value;
				Invalidate();
			}
		}
	}

	public Vector2 ListOffset
	{
		get
		{
			return listOffset;
		}
		set
		{
			if (Vector2.Distance(listOffset, value) > 1f)
			{
				listOffset = value;
				Invalidate();
			}
		}
	}

	public string ListBackground
	{
		get
		{
			return listBackground;
		}
		set
		{
			if (value != listBackground)
			{
				ClosePopup();
				listBackground = value;
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
				ClosePopup();
				itemHighlight = value;
				Invalidate();
			}
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
			Invalidate();
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
				if (popup != null)
				{
					popup.SelectedIndex = value;
				}
				selectedIndex = value;
				OnSelectedIndexChanged();
				Invalidate();
			}
		}
	}

	public RectOffset TextFieldPadding
	{
		get
		{
			if (textFieldPadding == null)
			{
				textFieldPadding = new RectOffset();
			}
			return textFieldPadding;
		}
		set
		{
			value = value.ConstrainPadding();
			if (!object.Equals(value, textFieldPadding))
			{
				textFieldPadding = value;
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
			ClosePopup();
			textColor = value;
			Invalidate();
		}
	}

	public Color32 DisabledTextColor
	{
		get
		{
			return disabledTextColor;
		}
		set
		{
			ClosePopup();
			disabledTextColor = value;
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
				ClosePopup();
				dfFontManager.Invalidate(Font);
				textScale = value;
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
			value = Mathf.Max(1, value);
			if (value != itemHeight)
			{
				ClosePopup();
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
			ClosePopup();
			if (value == null)
			{
				value = new string[0];
			}
			items = value;
			Invalidate();
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

	public PopupListPosition ListPosition
	{
		get
		{
			return listPosition;
		}
		set
		{
			if (value != ListPosition)
			{
				ClosePopup();
				listPosition = value;
				Invalidate();
			}
		}
	}

	public int MaxListWidth
	{
		get
		{
			return listWidth;
		}
		set
		{
			listWidth = value;
		}
	}

	public int MaxListHeight
	{
		get
		{
			return listHeight;
		}
		set
		{
			listHeight = value;
			Invalidate();
		}
	}

	public dfControl TriggerButton
	{
		get
		{
			return triggerButton;
		}
		set
		{
			if (value != triggerButton)
			{
				detachChildEvents();
				triggerButton = value;
				attachChildEvents();
				Invalidate();
			}
		}
	}

	public bool OpenOnMouseDown
	{
		get
		{
			return openOnMouseDown;
		}
		set
		{
			openOnMouseDown = value;
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

	public event PopupEventHandler DropdownOpen;

	public event PopupEventHandler DropdownClose;

	public event PropertyChangedEventHandler<int> SelectedIndexChanged;

	protected internal override void OnMouseWheel(dfMouseEventArgs args)
	{
		SelectedIndex = Mathf.Max(0, SelectedIndex - Mathf.RoundToInt(args.WheelDelta));
		args.Use();
		base.OnMouseWheel(args);
	}

	protected internal override void OnMouseDown(dfMouseEventArgs args)
	{
		if (openOnMouseDown && !args.Used && args.Buttons == dfMouseButtons.Left && args.Source == this)
		{
			args.Use();
			base.OnMouseDown(args);
			if (popup == null)
			{
				OpenPopup();
			}
			else
			{
				ClosePopup();
			}
		}
		else
		{
			base.OnMouseDown(args);
		}
	}

	protected internal override void OnKeyDown(dfKeyEventArgs args)
	{
		switch (args.KeyCode)
		{
		case KeyCode.UpArrow:
			SelectedIndex = Mathf.Max(0, selectedIndex - 1);
			break;
		case KeyCode.DownArrow:
			SelectedIndex = Mathf.Min(items.Length - 1, selectedIndex + 1);
			break;
		case KeyCode.Home:
			SelectedIndex = 0;
			break;
		case KeyCode.End:
			SelectedIndex = items.Length - 1;
			break;
		case KeyCode.Return:
		case KeyCode.Space:
			if (ClickWhenSpacePressed && IsInteractive)
			{
				OpenPopup();
			}
			break;
		}
		base.OnKeyDown(args);
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
		ClosePopup(false);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		ClosePopup(false);
		detachChildEvents();
	}

	public override void Update()
	{
		base.Update();
		checkForPopupClose();
	}

	private void checkForPopupClose()
	{
		if (!(popup == null) && Input.GetMouseButtonDown(0))
		{
			Camera camera = GetCamera();
			Ray ray = camera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hitInfo;
			if ((!(triggerButton != null) || !triggerButton.GetComponent<Collider>().Raycast(ray, out hitInfo, camera.farClipPlane)) && !popup.GetComponent<Collider>().Raycast(ray, out hitInfo, camera.farClipPlane) && (!(popup.Scrollbar != null) || !popup.Scrollbar.GetComponent<Collider>().Raycast(ray, out hitInfo, camera.farClipPlane)) && !GetComponent<Collider>().Raycast(ray, out hitInfo, camera.farClipPlane))
			{
				ClosePopup();
			}
		}
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		if (Application.isPlaying)
		{
			if (!eventsAttached)
			{
				attachChildEvents();
			}
			if (popup != null && !popup.ContainsFocus)
			{
				ClosePopup();
			}
		}
	}

	private void attachChildEvents()
	{
		if (triggerButton != null && !eventsAttached)
		{
			eventsAttached = true;
			triggerButton.Click += trigger_Click;
		}
	}

	private void detachChildEvents()
	{
		if (triggerButton != null && eventsAttached)
		{
			triggerButton.Click -= trigger_Click;
			eventsAttached = false;
		}
	}

	private void trigger_Click(dfControl control, dfMouseEventArgs mouseEvent)
	{
		if (mouseEvent.Source == triggerButton && !mouseEvent.Used)
		{
			mouseEvent.Use();
			if (popup == null)
			{
				OpenPopup();
			}
			else
			{
				ClosePopup();
			}
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

	private void renderText(dfRenderData buffer)
	{
		if (selectedIndex < 0 || selectedIndex >= items.Length)
		{
			return;
		}
		string text = items[selectedIndex];
		float num = PixelsToUnits();
		Vector2 vector = new Vector2(size.x - (float)textFieldPadding.horizontal, size.y - (float)textFieldPadding.vertical);
		Vector3 vector2 = pivot.TransformToUpperLeft(base.Size);
		Vector3 vectorOffset = new Vector3(vector2.x + (float)textFieldPadding.left, vector2.y - (float)textFieldPadding.top, 0f) * num;
		Color32 defaultColor = ((!base.IsEnabled) ? DisabledTextColor : TextColor);
		using (dfFontRendererBase dfFontRendererBase2 = font.ObtainRenderer())
		{
			dfFontRendererBase2.WordWrap = false;
			dfFontRendererBase2.MaxSize = vector;
			dfFontRendererBase2.PixelRatio = num;
			dfFontRendererBase2.TextScale = TextScale;
			dfFontRendererBase2.VectorOffset = vectorOffset;
			dfFontRendererBase2.MultiLine = false;
			dfFontRendererBase2.TextAlign = TextAlignment.Left;
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
				dynamicFontRenderer.SpriteBuffer = buffer;
			}
			dfFontRendererBase2.Render(text, buffer);
		}
	}

	public void AddItem(string item)
	{
		string[] array = new string[items.Length + 1];
		Array.Copy(items, array, items.Length);
		array[items.Length] = item;
		items = array;
	}

	public void OpenPopup()
	{
		if (popup != null || items.Length == 0)
		{
			return;
		}
		Vector2 vector = calculatePopupSize();
		popup = GetManager().AddControl<dfListbox>();
		popup.name = base.name + " - Dropdown List";
		popup.gameObject.hideFlags = HideFlags.DontSave;
		popup.Atlas = base.Atlas;
		popup.Anchor = dfAnchorStyle.Top | dfAnchorStyle.Left;
		popup.Color = base.Color;
		popup.Font = Font;
		popup.Pivot = dfPivotPoint.TopLeft;
		popup.Size = vector;
		popup.Font = Font;
		popup.ItemHeight = ItemHeight;
		popup.ItemHighlight = ItemHighlight;
		popup.ItemHover = ItemHover;
		popup.ItemPadding = TextFieldPadding;
		popup.ItemTextColor = TextColor;
		popup.ItemTextScale = TextScale;
		popup.Items = Items;
		popup.ListPadding = ListPadding;
		popup.BackgroundSprite = ListBackground;
		popup.Shadow = Shadow;
		popup.ShadowColor = ShadowColor;
		popup.ShadowOffset = ShadowOffset;
		popup.BringToFront();
		if (dfGUIManager.GetModalControl() != null)
		{
			dfGUIManager.PushModal(popup);
		}
		if (vector.y >= (float)MaxListHeight && listScrollbar != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(listScrollbar.gameObject);
			dfScrollbar activeScrollbar = gameObject.GetComponent<dfScrollbar>();
			float num = PixelsToUnits();
			Vector3 vector2 = popup.transform.TransformDirection(Vector3.right);
			Vector3 position = popup.transform.position + vector2 * (vector.x - activeScrollbar.Width) * num;
			popup.AddControl(activeScrollbar);
			popup.Width -= activeScrollbar.Width;
			popup.Scrollbar = activeScrollbar;
			popup.SizeChanged += delegate(dfControl control, Vector2 size)
			{
				activeScrollbar.Height = control.Height;
			};
			activeScrollbar.transform.parent = popup.transform;
			activeScrollbar.transform.position = position;
			activeScrollbar.Anchor = dfAnchorStyle.Top | dfAnchorStyle.Bottom;
			activeScrollbar.Height = popup.Height;
		}
		Vector3 position2 = calculatePopupPosition((int)popup.Size.y);
		popup.transform.position = position2;
		popup.transform.rotation = base.transform.rotation;
		popup.SelectedIndexChanged += popup_SelectedIndexChanged;
		popup.LeaveFocus += popup_LostFocus;
		popup.ItemClicked += popup_ItemClicked;
		popup.KeyDown += popup_KeyDown;
		popup.SelectedIndex = Mathf.Max(0, SelectedIndex);
		popup.EnsureVisible(popup.SelectedIndex);
		popup.Focus();
		if (this.DropdownOpen != null)
		{
			bool overridden = false;
			this.DropdownOpen(this, popup, ref overridden);
		}
		Signal("OnDropdownOpen", this, popup);
	}

	public void ClosePopup()
	{
		ClosePopup(true);
	}

	public void ClosePopup(bool allowOverride)
	{
		if (popup == null)
		{
			return;
		}
		if (dfGUIManager.GetModalControl() == popup)
		{
			dfGUIManager.PopModal();
		}
		popup.LostFocus -= popup_LostFocus;
		popup.SelectedIndexChanged -= popup_SelectedIndexChanged;
		popup.ItemClicked -= popup_ItemClicked;
		popup.KeyDown -= popup_KeyDown;
		if (!allowOverride)
		{
			UnityEngine.Object.Destroy(popup.gameObject);
			popup = null;
			return;
		}
		bool overridden = false;
		if (this.DropdownClose != null)
		{
			this.DropdownClose(this, popup, ref overridden);
		}
		if (!overridden)
		{
			Signal("OnDropdownClose", this, popup);
		}
		if (!overridden)
		{
			UnityEngine.Object.Destroy(popup.gameObject);
		}
		popup = null;
	}

	private Vector3 calculatePopupPosition(int height)
	{
		float num = PixelsToUnits();
		Vector3 vector = base.transform.position + pivot.TransformToUpperLeft(size) * num;
		Vector3 scaledDirection = getScaledDirection(Vector3.down);
		Vector3 vector2 = transformOffset(listOffset);
		Vector3 result = vector + (vector2 + scaledDirection * base.Size.y) * num;
		Vector3 result2 = vector + (vector2 - scaledDirection * popup.Size.y) * num;
		if (listPosition == PopupListPosition.Above)
		{
			return result2;
		}
		if (listPosition == PopupListPosition.Below)
		{
			return result;
		}
		Vector2 screenSize = GetManager().GetScreenSize();
		float num2 = GetAbsolutePosition().y + base.Height + (float)height;
		if (num2 >= screenSize.y)
		{
			return result2;
		}
		return result;
	}

	private Vector2 calculatePopupSize()
	{
		float x = ((MaxListWidth <= 0) ? size.x : ((float)MaxListWidth));
		int b = items.Length * itemHeight + listPadding.vertical;
		if (items.Length == 0)
		{
			b = itemHeight / 2 + listPadding.vertical;
		}
		return new Vector2(x, Mathf.Min(MaxListHeight, b));
	}

	private void popup_KeyDown(dfControl control, dfKeyEventArgs args)
	{
		if (args.KeyCode == KeyCode.Escape || args.KeyCode == KeyCode.Return)
		{
			ClosePopup();
			Focus();
		}
	}

	private void popup_ItemClicked(dfControl control, int selectedIndex)
	{
		Focus();
	}

	private void popup_LostFocus(dfControl control, dfFocusEventArgs args)
	{
		if (popup != null && !popup.ContainsFocus)
		{
			ClosePopup();
		}
	}

	private void popup_SelectedIndexChanged(dfControl control, int selectedIndex)
	{
		SelectedIndex = selectedIndex;
		Invalidate();
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
		if (!isControlInvalidated)
		{
			for (int i = 0; i < buffers.Count; i++)
			{
				buffers[i].Transform = base.transform.localToWorldMatrix;
			}
			return buffers;
		}
		buffers.Clear();
		renderData.Clear();
		renderData.Material = base.Atlas.Material;
		renderData.Transform = base.transform.localToWorldMatrix;
		buffers.Add(renderData);
		textRenderData.Clear();
		textRenderData.Material = base.Atlas.Material;
		textRenderData.Transform = base.transform.localToWorldMatrix;
		buffers.Add(textRenderData);
		renderBackground();
		renderText(textRenderData);
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
		if (!(dfDynamicFont2 == null) && dfFontManager.IsDirty(Font))
		{
			string selectedValue = SelectedValue;
			if (!string.IsNullOrEmpty(selectedValue))
			{
				float num = TextScale;
				int fontSize = Mathf.CeilToInt((float)font.FontSize * num);
				dfDynamicFont2.AddCharacterRequest(selectedValue, fontSize, FontStyle.Normal);
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
