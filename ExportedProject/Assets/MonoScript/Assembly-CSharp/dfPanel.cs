using System;
using UnityEngine;

[Serializable]
[dfCategory("Basic Controls")]
[ExecuteInEditMode]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_panel.html")]
[dfTooltip("Basic container control to facilitate user interface layout")]
[AddComponentMenu("Daikon Forge/User Interface/Containers/Panel")]
public class dfPanel : dfControl
{
	[SerializeField]
	protected dfAtlas atlas;

	[SerializeField]
	protected string backgroundSprite;

	[SerializeField]
	protected Color32 backgroundColor = UnityEngine.Color.white;

	[SerializeField]
	protected RectOffset padding = new RectOffset();

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
			value = getLocalizedValue(value);
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

	protected internal override void OnLocalize()
	{
		base.OnLocalize();
		BackgroundSprite = getLocalizedValue(backgroundSprite);
	}

	protected internal override RectOffset GetClipPadding()
	{
		return padding ?? dfRectOffsetExtensions.Empty;
	}

	protected internal override Plane[] GetClippingPlanes()
	{
		if (!base.ClipChildren)
		{
			return null;
		}
		Vector3[] corners = GetCorners();
		Vector3 vector = base.transform.TransformDirection(Vector3.right);
		Vector3 vector2 = base.transform.TransformDirection(Vector3.left);
		Vector3 vector3 = base.transform.TransformDirection(Vector3.up);
		Vector3 vector4 = base.transform.TransformDirection(Vector3.down);
		float num = PixelsToUnits();
		RectOffset rectOffset = Padding;
		corners[0] += vector * rectOffset.left * num + vector4 * rectOffset.top * num;
		corners[1] += vector2 * rectOffset.right * num + vector4 * rectOffset.top * num;
		corners[2] += vector * rectOffset.left * num + vector3 * rectOffset.bottom * num;
		return new Plane[4]
		{
			new Plane(vector, corners[0]),
			new Plane(vector2, corners[1]),
			new Plane(vector3, corners[2]),
			new Plane(vector4, corners[0])
		};
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (size == Vector2.zero)
		{
			SuspendLayout();
			Camera camera = GetCamera();
			base.Size = new Vector3(camera.pixelWidth / 2, camera.pixelHeight / 2);
			ResumeLayout();
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

	public void FitToContents()
	{
		if (controls.Count != 0)
		{
			Vector2 vector = Vector2.zero;
			for (int i = 0; i < controls.Count; i++)
			{
				dfControl dfControl2 = controls[i];
				Vector2 rhs = (Vector2)dfControl2.RelativePosition + dfControl2.Size;
				vector = Vector2.Max(vector, rhs);
			}
			base.Size = vector + new Vector2(padding.right, padding.bottom);
		}
	}

	public void CenterChildControls()
	{
		if (controls.Count != 0)
		{
			Vector2 vector = Vector2.one * float.MaxValue;
			Vector2 vector2 = Vector2.one * float.MinValue;
			for (int i = 0; i < controls.Count; i++)
			{
				dfControl dfControl2 = controls[i];
				Vector2 vector3 = dfControl2.RelativePosition;
				Vector2 rhs = vector3 + dfControl2.Size;
				vector = Vector2.Min(vector, vector3);
				vector2 = Vector2.Max(vector2, rhs);
			}
			Vector2 vector4 = vector2 - vector;
			Vector2 vector5 = (base.Size - vector4) * 0.5f;
			for (int j = 0; j < controls.Count; j++)
			{
				dfControl dfControl3 = controls[j];
				dfControl3.RelativePosition = (Vector2)dfControl3.RelativePosition - vector + vector5;
			}
		}
	}
}
