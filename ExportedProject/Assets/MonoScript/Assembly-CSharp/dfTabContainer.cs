using System;
using UnityEngine;

[Serializable]
[ExecuteInEditMode]
[AddComponentMenu("Daikon Forge/User Interface/Containers/Tab Control/Tab Page Container")]
[dfTooltip("Used in conjunction with the dfTabContainer class to implement tabbed containers. This control maintains the tabs that are displayed for the user to select, and the dfTabContainer class manages the display of the tab pages themselves.")]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_tab_container.html")]
[dfCategory("Basic Controls")]
public class dfTabContainer : dfControl
{
	[SerializeField]
	protected dfAtlas atlas;

	[SerializeField]
	protected string backgroundSprite;

	[SerializeField]
	protected RectOffset padding = new RectOffset();

	[SerializeField]
	protected int selectedIndex;

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
				arrangeTabPages();
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
			if (value != selectedIndex)
			{
				selectPageByIndex(value);
			}
		}
	}

	public event PropertyChangedEventHandler<int> SelectedIndexChanged;

	public dfControl AddTabPage()
	{
		dfPanel dfPanel2 = controls.Where((dfControl i) => i is dfPanel).FirstOrDefault() as dfPanel;
		string text = "Tab Page " + (controls.Count + 1);
		dfPanel dfPanel3 = AddControl<dfPanel>();
		dfPanel3.name = text;
		dfPanel3.Atlas = Atlas;
		dfPanel3.Anchor = dfAnchorStyle.All;
		dfPanel3.ClipChildren = true;
		if (dfPanel2 != null)
		{
			dfPanel3.Atlas = dfPanel2.Atlas;
			dfPanel3.BackgroundSprite = dfPanel2.BackgroundSprite;
		}
		arrangeTabPages();
		Invalidate();
		return dfPanel3;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (size.sqrMagnitude < float.Epsilon)
		{
			base.Size = new Vector2(256f, 256f);
		}
	}

	protected internal override void OnControlAdded(dfControl child)
	{
		base.OnControlAdded(child);
		attachEvents(child);
		arrangeTabPages();
	}

	protected internal override void OnControlRemoved(dfControl child)
	{
		base.OnControlRemoved(child);
		detachEvents(child);
		arrangeTabPages();
	}

	protected internal virtual void OnSelectedIndexChanged(int Index)
	{
		SignalHierarchy("OnSelectedIndexChanged", this, Index);
		if (this.SelectedIndexChanged != null)
		{
			this.SelectedIndexChanged(this, Index);
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

	private void selectPageByIndex(int value)
	{
		value = Mathf.Max(Mathf.Min(value, controls.Count - 1), -1);
		if (value == selectedIndex)
		{
			return;
		}
		selectedIndex = value;
		for (int i = 0; i < controls.Count; i++)
		{
			dfControl dfControl2 = controls[i];
			if (!(dfControl2 == null))
			{
				dfControl2.IsVisible = i == value;
			}
		}
		arrangeTabPages();
		Invalidate();
		OnSelectedIndexChanged(value);
	}

	private void arrangeTabPages()
	{
		if (padding == null)
		{
			padding = new RectOffset(0, 0, 0, 0);
		}
		Vector3 relativePosition = new Vector3(padding.left, padding.top);
		Vector2 vector = new Vector2(size.x - (float)padding.horizontal, size.y - (float)padding.vertical);
		for (int i = 0; i < controls.Count; i++)
		{
			dfPanel dfPanel2 = controls[i] as dfPanel;
			if (dfPanel2 != null)
			{
				dfPanel2.Size = vector;
				dfPanel2.RelativePosition = relativePosition;
			}
		}
	}

	private void attachEvents(dfControl control)
	{
		control.IsVisibleChanged += control_IsVisibleChanged;
		control.PositionChanged += childControlInvalidated;
		control.SizeChanged += childControlInvalidated;
	}

	private void detachEvents(dfControl control)
	{
		control.IsVisibleChanged -= control_IsVisibleChanged;
		control.PositionChanged -= childControlInvalidated;
		control.SizeChanged -= childControlInvalidated;
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
			arrangeTabPages();
			Invalidate();
		}
	}
}
