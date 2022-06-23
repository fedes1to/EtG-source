using System;
using UnityEngine;

[Serializable]
[dfTooltip("Used in conjunction with the dfTabContainer class to implement tabbed containers. This control maintains the tabs that are displayed for the user to select, and the dfTabContainer class manages the display of the tab pages themselves.")]
[dfCategory("Basic Controls")]
[AddComponentMenu("Daikon Forge/User Interface/Containers/Tab Control/Tab Strip")]
[ExecuteInEditMode]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_tabstrip.html")]
public class dfTabstrip : dfControl
{
	[SerializeField]
	protected dfAtlas atlas;

	[SerializeField]
	protected string backgroundSprite;

	[SerializeField]
	protected RectOffset layoutPadding = new RectOffset();

	[SerializeField]
	protected Vector2 scrollPosition = Vector2.zero;

	[SerializeField]
	protected int selectedIndex;

	[SerializeField]
	protected dfTabContainer pageContainer;

	[SerializeField]
	protected bool allowKeyboardNavigation = true;

	public dfTabContainer TabPages
	{
		get
		{
			return pageContainer;
		}
		set
		{
			if (!(pageContainer != value))
			{
				return;
			}
			pageContainer = value;
			if (value != null)
			{
				while (value.Controls.Count < controls.Count)
				{
					value.AddTabPage();
				}
			}
			pageContainer.SelectedIndex = SelectedIndex;
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
			if (value != selectedIndex)
			{
				selectTabByIndex(value);
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

	public RectOffset LayoutPadding
	{
		get
		{
			if (layoutPadding == null)
			{
				layoutPadding = new RectOffset();
			}
			return layoutPadding;
		}
		set
		{
			value = value.ConstrainPadding();
			if (!object.Equals(value, layoutPadding))
			{
				layoutPadding = value;
				arrangeTabs();
			}
		}
	}

	public bool AllowKeyboardNavigation
	{
		get
		{
			return allowKeyboardNavigation;
		}
		set
		{
			allowKeyboardNavigation = value;
		}
	}

	public event PropertyChangedEventHandler<int> SelectedIndexChanged;

	public void EnableTab(int index)
	{
		if (selectedIndex >= 0 && selectedIndex <= controls.Count - 1)
		{
			controls[index].Enable();
		}
	}

	public void DisableTab(int index)
	{
		if (selectedIndex >= 0 && selectedIndex <= controls.Count - 1)
		{
			controls[index].Disable();
		}
	}

	public dfControl AddTab(string Text)
	{
		if (Text == null)
		{
			Text = string.Empty;
		}
		dfButton dfButton2 = controls.Where((dfControl i) => i is dfButton).FirstOrDefault() as dfButton;
		string text = "Tab " + (controls.Count + 1);
		if (string.IsNullOrEmpty(Text))
		{
			Text = text;
		}
		dfButton dfButton3 = AddControl<dfButton>();
		dfButton3.name = text;
		dfButton3.Atlas = Atlas;
		dfButton3.Text = Text;
		dfButton3.ButtonGroup = this;
		if (dfButton2 != null)
		{
			dfButton3.Atlas = dfButton2.Atlas;
			dfButton3.Font = dfButton2.Font;
			dfButton3.AutoSize = dfButton2.AutoSize;
			dfButton3.Size = dfButton2.Size;
			dfButton3.BackgroundSprite = dfButton2.BackgroundSprite;
			dfButton3.DisabledSprite = dfButton2.DisabledSprite;
			dfButton3.FocusSprite = dfButton2.FocusSprite;
			dfButton3.HoverSprite = dfButton2.HoverSprite;
			dfButton3.PressedSprite = dfButton2.PressedSprite;
			dfButton3.Shadow = dfButton2.Shadow;
			dfButton3.ShadowColor = dfButton2.ShadowColor;
			dfButton3.ShadowOffset = dfButton2.ShadowOffset;
			dfButton3.TextColor = dfButton2.TextColor;
			dfButton3.TextAlignment = dfButton2.TextAlignment;
			RectOffset padding = dfButton2.Padding;
			dfButton3.Padding = new RectOffset(padding.left, padding.right, padding.top, padding.bottom);
		}
		if (pageContainer != null)
		{
			pageContainer.AddTabPage();
		}
		arrangeTabs();
		Invalidate();
		return dfButton3;
	}

	protected internal override void OnGotFocus(dfFocusEventArgs args)
	{
		if (controls.Contains(args.GotFocus))
		{
			SelectedIndex = args.GotFocus.ZOrder;
		}
		base.OnGotFocus(args);
	}

	protected internal override void OnLostFocus(dfFocusEventArgs args)
	{
		base.OnLostFocus(args);
		if (controls.Contains(args.LostFocus))
		{
			showSelectedTab();
		}
	}

	protected internal override void OnClick(dfMouseEventArgs args)
	{
		if (controls.Contains(args.Source))
		{
			SelectedIndex = args.Source.ZOrder;
		}
		base.OnClick(args);
	}

	private void OnClick(dfControl sender, dfMouseEventArgs args)
	{
		if (controls.Contains(args.Source))
		{
			SelectedIndex = args.Source.ZOrder;
		}
	}

	protected internal override void OnKeyDown(dfKeyEventArgs args)
	{
		if (args.Used)
		{
			return;
		}
		if (allowKeyboardNavigation)
		{
			if (args.KeyCode == KeyCode.LeftArrow || (args.KeyCode == KeyCode.Tab && args.Shift))
			{
				SelectedIndex = Mathf.Max(0, SelectedIndex - 1);
				args.Use();
				return;
			}
			if (args.KeyCode == KeyCode.RightArrow || args.KeyCode == KeyCode.Tab)
			{
				SelectedIndex++;
				args.Use();
				return;
			}
		}
		base.OnKeyDown(args);
	}

	protected internal override void OnControlAdded(dfControl child)
	{
		base.OnControlAdded(child);
		attachEvents(child);
		arrangeTabs();
	}

	protected internal override void OnControlRemoved(dfControl child)
	{
		base.OnControlRemoved(child);
		detachEvents(child);
		arrangeTabs();
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (size.sqrMagnitude < float.Epsilon)
		{
			base.Size = new Vector2(256f, 26f);
		}
		if (Application.isPlaying)
		{
			selectTabByIndex(Mathf.Max(selectedIndex, 0));
		}
	}

	public override void Update()
	{
		base.Update();
		if (isControlInvalidated)
		{
			arrangeTabs();
		}
		showSelectedTab();
	}

	protected internal virtual void OnSelectedIndexChanged()
	{
		SignalHierarchy("OnSelectedIndexChanged", this, SelectedIndex);
		if (this.SelectedIndexChanged != null)
		{
			this.SelectedIndexChanged(this, SelectedIndex);
		}
	}

	protected override void OnRebuildRenderData()
	{
		if (Atlas == null || string.IsNullOrEmpty(backgroundSprite))
		{
			return;
		}
		dfAtlas.ItemInfo itemInfo = Atlas[backgroundSprite];
		if (!(itemInfo == null))
		{
			renderData.Material = Atlas.Material;
			Color32 color = ApplyOpacity((!base.IsEnabled) ? disabledColor : base.color);
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

	private void showSelectedTab()
	{
		if (selectedIndex >= 0 && selectedIndex <= controls.Count - 1)
		{
			dfButton dfButton2 = controls[selectedIndex] as dfButton;
			if (dfButton2 != null && !dfButton2.ContainsMouse)
			{
				dfButton2.State = dfButton.ButtonState.Focus;
			}
		}
	}

	private void selectTabByIndex(int value)
	{
		value = Mathf.Max(Mathf.Min(value, controls.Count - 1), -1);
		if (value == selectedIndex)
		{
			return;
		}
		selectedIndex = value;
		for (int i = 0; i < controls.Count; i++)
		{
			dfButton dfButton2 = controls[i] as dfButton;
			if (!(dfButton2 == null))
			{
				if (i == value)
				{
					dfButton2.State = dfButton.ButtonState.Focus;
				}
				else
				{
					dfButton2.State = dfButton.ButtonState.Default;
				}
			}
		}
		Invalidate();
		OnSelectedIndexChanged();
		if (pageContainer != null)
		{
			pageContainer.SelectedIndex = value;
		}
	}

	private void arrangeTabs()
	{
		SuspendLayout();
		try
		{
			layoutPadding = layoutPadding.ConstrainPadding();
			float num = (float)layoutPadding.left - scrollPosition.x;
			float y = (float)layoutPadding.top - scrollPosition.y;
			float b = 0f;
			float b2 = 0f;
			for (int i = 0; i < base.Controls.Count; i++)
			{
				dfControl dfControl2 = controls[i];
				if (dfControl2.IsVisible && dfControl2.enabled && dfControl2.gameObject.activeSelf)
				{
					Vector2 vector = new Vector2(num, y);
					dfControl2.RelativePosition = vector;
					float num2 = dfControl2.Width + (float)layoutPadding.horizontal;
					float a = dfControl2.Height + (float)layoutPadding.vertical;
					b = Mathf.Max(num2, b);
					b2 = Mathf.Max(a, b2);
					num += num2;
				}
			}
		}
		finally
		{
			ResumeLayout();
		}
	}

	private void attachEvents(dfControl control)
	{
		control.IsVisibleChanged += control_IsVisibleChanged;
		control.PositionChanged += childControlInvalidated;
		control.SizeChanged += childControlInvalidated;
		control.ZOrderChanged += childControlZOrderChanged;
	}

	private void detachEvents(dfControl control)
	{
		control.IsVisibleChanged -= control_IsVisibleChanged;
		control.PositionChanged -= childControlInvalidated;
		control.SizeChanged -= childControlInvalidated;
	}

	private void childControlZOrderChanged(dfControl control, int value)
	{
		onChildControlInvalidatedLayout();
	}

	private void control_IsVisibleChanged(dfControl control, bool value)
	{
		onChildControlInvalidatedLayout();
	}

	private void childControlInvalidated(dfControl control, Vector2 value)
	{
		onChildControlInvalidatedLayout();
	}

	private void onChildControlInvalidatedLayout()
	{
		if (!base.IsLayoutSuspended)
		{
			arrangeTabs();
			Invalidate();
		}
	}
}
