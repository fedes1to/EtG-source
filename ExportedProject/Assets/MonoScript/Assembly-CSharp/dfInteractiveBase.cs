using System;
using UnityEngine;

[Serializable]
[ExecuteInEditMode]
public class dfInteractiveBase : dfControl
{
	[SerializeField]
	protected dfAtlas atlas;

	[SerializeField]
	protected string backgroundSprite;

	[SerializeField]
	protected string hoverSprite;

	[SerializeField]
	protected string disabledSprite;

	[SerializeField]
	protected string focusSprite;

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
				setDefaultSize(value);
				Invalidate();
			}
		}
	}

	public string DisabledSprite
	{
		get
		{
			return disabledSprite;
		}
		set
		{
			if (value != disabledSprite)
			{
				disabledSprite = value;
				Invalidate();
			}
		}
	}

	public string FocusSprite
	{
		get
		{
			return focusSprite;
		}
		set
		{
			if (value != focusSprite)
			{
				focusSprite = value;
				Invalidate();
			}
		}
	}

	public string HoverSprite
	{
		get
		{
			return hoverSprite;
		}
		set
		{
			if (value != hoverSprite)
			{
				hoverSprite = value;
				Invalidate();
			}
		}
	}

	public override bool CanFocus
	{
		get
		{
			if (base.IsEnabled && base.IsVisible)
			{
				return true;
			}
			return base.CanFocus;
		}
	}

	protected internal override void OnGotFocus(dfFocusEventArgs args)
	{
		base.OnGotFocus(args);
		Invalidate();
	}

	protected internal override void OnLostFocus(dfFocusEventArgs args)
	{
		base.OnLostFocus(args);
		Invalidate();
	}

	protected internal override void OnMouseEnter(dfMouseEventArgs args)
	{
		base.OnMouseEnter(args);
		Invalidate();
	}

	protected internal override void OnMouseLeave(dfMouseEventArgs args)
	{
		base.OnMouseLeave(args);
		Invalidate();
	}

	public override Vector2 CalculateMinimumSize()
	{
		dfAtlas.ItemInfo itemInfo = getBackgroundSprite();
		if (itemInfo == null)
		{
			return base.CalculateMinimumSize();
		}
		RectOffset border = itemInfo.border;
		if (border.horizontal > 0 || border.vertical > 0)
		{
			return Vector2.Max(base.CalculateMinimumSize(), new Vector2(border.horizontal, border.vertical));
		}
		return base.CalculateMinimumSize();
	}

	protected internal virtual void renderBackground()
	{
		if (Atlas == null)
		{
			return;
		}
		dfAtlas.ItemInfo itemInfo = getBackgroundSprite();
		if (!(itemInfo == null))
		{
			Color32 color = ApplyOpacity(getActiveColor());
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

	protected virtual Color32 getActiveColor()
	{
		if (base.IsEnabled)
		{
			return color;
		}
		if (!string.IsNullOrEmpty(disabledSprite) && Atlas != null && Atlas[DisabledSprite] != null)
		{
			return color;
		}
		return disabledColor;
	}

	protected internal virtual dfAtlas.ItemInfo getBackgroundSprite()
	{
		if (Atlas == null)
		{
			return null;
		}
		if (!base.IsEnabled)
		{
			dfAtlas.ItemInfo itemInfo = atlas[DisabledSprite];
			if (itemInfo != null)
			{
				return itemInfo;
			}
			return atlas[BackgroundSprite];
		}
		if (HasFocus)
		{
			dfAtlas.ItemInfo itemInfo2 = atlas[FocusSprite];
			if (itemInfo2 != null)
			{
				return itemInfo2;
			}
			return atlas[BackgroundSprite];
		}
		if (isMouseHovering)
		{
			dfAtlas.ItemInfo itemInfo3 = atlas[HoverSprite];
			if (itemInfo3 != null)
			{
				return itemInfo3;
			}
		}
		return Atlas[BackgroundSprite];
	}

	private void setDefaultSize(string spriteName)
	{
		if (!(Atlas == null))
		{
			dfAtlas.ItemInfo itemInfo = Atlas[spriteName];
			if (size == Vector2.zero && itemInfo != null)
			{
				base.Size = itemInfo.sizeInPixels;
			}
		}
	}
}
