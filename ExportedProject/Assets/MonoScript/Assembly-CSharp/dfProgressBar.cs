using System;
using UnityEngine;

[Serializable]
[dfCategory("Basic Controls")]
[dfTooltip("Implements a progress bar that can be used to display the progress from a start value toward an end value, such as the amount of work completed or a player's progress toward some goal, etc.")]
[AddComponentMenu("Daikon Forge/User Interface/Progress Bar")]
[ExecuteInEditMode]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_progress_bar.html")]
public class dfProgressBar : dfControl
{
	[SerializeField]
	protected dfAtlas atlas;

	[SerializeField]
	protected string backgroundSprite;

	[SerializeField]
	protected string progressSprite;

	[SerializeField]
	protected Color32 progressColor = UnityEngine.Color.white;

	[SerializeField]
	protected float rawValue = 0.25f;

	[SerializeField]
	protected float minValue;

	[SerializeField]
	protected float maxValue = 1f;

	[SerializeField]
	protected dfProgressFillMode fillMode;

	[SerializeField]
	protected dfFillDirection progressFillDirection;

	[SerializeField]
	protected RectOffset padding = new RectOffset();

	[SerializeField]
	protected bool actAsSlider;

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

	public string ProgressSprite
	{
		get
		{
			return progressSprite;
		}
		set
		{
			if (value != progressSprite)
			{
				progressSprite = value;
				Invalidate();
			}
		}
	}

	public Color32 ProgressColor
	{
		get
		{
			return progressColor;
		}
		set
		{
			if (!object.Equals(value, progressColor))
			{
				progressColor = value;
				Invalidate();
			}
		}
	}

	public float MinValue
	{
		get
		{
			return minValue;
		}
		set
		{
			if (value != minValue)
			{
				minValue = value;
				if (rawValue < value)
				{
					Value = value;
				}
				Invalidate();
			}
		}
	}

	public float MaxValue
	{
		get
		{
			return maxValue;
		}
		set
		{
			if (value != maxValue)
			{
				maxValue = value;
				if (rawValue > value)
				{
					Value = value;
				}
				Invalidate();
			}
		}
	}

	public float Value
	{
		get
		{
			return rawValue;
		}
		set
		{
			value = Mathf.Max(minValue, Mathf.Min(maxValue, value));
			if (!Mathf.Approximately(value, rawValue))
			{
				rawValue = value;
				OnValueChanged();
			}
		}
	}

	public dfProgressFillMode FillMode
	{
		get
		{
			return fillMode;
		}
		set
		{
			if (value != fillMode)
			{
				fillMode = value;
				Invalidate();
			}
		}
	}

	public dfFillDirection ProgressFillDirection
	{
		get
		{
			return progressFillDirection;
		}
		set
		{
			if (value != progressFillDirection)
			{
				progressFillDirection = value;
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

	public bool ActAsSlider
	{
		get
		{
			return actAsSlider;
		}
		set
		{
			actAsSlider = value;
		}
	}

	public event PropertyChangedEventHandler<float> ValueChanged;

	protected internal override void OnMouseWheel(dfMouseEventArgs args)
	{
		try
		{
			if (actAsSlider)
			{
				float num = (maxValue - minValue) * 0.1f;
				Value += num * (float)Mathf.RoundToInt(0f - args.WheelDelta);
				args.Use();
			}
		}
		finally
		{
			base.OnMouseWheel(args);
		}
	}

	protected internal override void OnMouseMove(dfMouseEventArgs args)
	{
		try
		{
			if (actAsSlider && args.Buttons.IsSet(dfMouseButtons.Left))
			{
				Value = getValueFromMouseEvent(args);
				args.Use();
			}
		}
		finally
		{
			base.OnMouseMove(args);
		}
	}

	protected internal override void OnMouseDown(dfMouseEventArgs args)
	{
		try
		{
			if (actAsSlider && args.Buttons.IsSet(dfMouseButtons.Left))
			{
				Focus();
				Value = getValueFromMouseEvent(args);
				args.Use();
			}
		}
		finally
		{
			base.OnMouseDown(args);
		}
	}

	protected internal override void OnKeyDown(dfKeyEventArgs args)
	{
		try
		{
			if (actAsSlider)
			{
				float num = (maxValue - minValue) * 0.1f;
				if (args.KeyCode == KeyCode.LeftArrow)
				{
					Value -= num;
					args.Use();
				}
				else if (args.KeyCode == KeyCode.RightArrow)
				{
					Value += num;
					args.Use();
				}
			}
		}
		finally
		{
			base.OnKeyDown(args);
		}
	}

	protected internal virtual void OnValueChanged()
	{
		Invalidate();
		SignalHierarchy("OnValueChanged", this, Value);
		if (this.ValueChanged != null)
		{
			this.ValueChanged(this, Value);
		}
	}

	protected override void OnRebuildRenderData()
	{
		if (!(Atlas == null))
		{
			renderData.Material = Atlas.Material;
			renderBackground();
			renderProgressFill();
		}
	}

	private void renderProgressFill()
	{
		if (Atlas == null)
		{
			return;
		}
		dfAtlas.ItemInfo itemInfo = Atlas[progressSprite];
		if (!(itemInfo == null))
		{
			Vector3 vector = new Vector3(padding.left, -padding.top);
			Vector2 vector2 = new Vector2(size.x - (float)padding.horizontal, size.y - (float)padding.vertical);
			float fillAmount = 1f;
			float num = maxValue - minValue;
			float num2 = (rawValue - minValue) / num;
			dfProgressFillMode dfProgressFillMode2 = fillMode;
			if (dfProgressFillMode2 != 0 || vector2.x * num2 < (float)itemInfo.border.horizontal)
			{
			}
			if (dfProgressFillMode2 == dfProgressFillMode.Fill)
			{
				fillAmount = num2;
			}
			else
			{
				vector2.x = Mathf.Max(itemInfo.border.horizontal, vector2.x * num2);
			}
			Color32 color = ApplyOpacity((!base.IsEnabled) ? base.DisabledColor : ProgressColor);
			dfSprite.RenderOptions renderOptions = default(dfSprite.RenderOptions);
			renderOptions.atlas = atlas;
			renderOptions.color = color;
			renderOptions.fillAmount = fillAmount;
			renderOptions.flip = dfSpriteFlip.None;
			renderOptions.offset = pivot.TransformToUpperLeft(base.Size) + vector;
			renderOptions.pixelsToUnits = PixelsToUnits();
			renderOptions.size = vector2;
			renderOptions.spriteInfo = itemInfo;
			dfSprite.RenderOptions options = renderOptions;
			if (progressFillDirection == dfFillDirection.Vertical)
			{
				options.invertFill = true;
			}
			options.fillDirection = progressFillDirection;
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

	private void renderBackground()
	{
		if (Atlas == null)
		{
			return;
		}
		dfAtlas.ItemInfo itemInfo = Atlas[backgroundSprite];
		if (!(itemInfo == null))
		{
			Color32 color = ApplyOpacity((!base.IsEnabled) ? base.DisabledColor : base.Color);
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

	private float getValueFromMouseEvent(dfMouseEventArgs args)
	{
		Vector3[] endPoints = getEndPoints(true);
		Vector3 vector = endPoints[0];
		Vector3 vector2 = endPoints[1];
		Plane plane = new Plane(base.transform.TransformDirection(Vector3.back), vector);
		Ray ray = args.Ray;
		float enter = 0f;
		if (!plane.Raycast(ray, out enter))
		{
			return rawValue;
		}
		Vector3 test = ray.origin + ray.direction * enter;
		Vector3 vector3 = closestPoint(vector, vector2, test, true);
		float num = (vector3 - vector).magnitude / (vector2 - vector).magnitude;
		return minValue + (maxValue - minValue) * num;
	}

	private Vector3[] getEndPoints()
	{
		return getEndPoints(false);
	}

	private Vector3[] getEndPoints(bool convertToWorld)
	{
		Vector3 vector = pivot.TransformToUpperLeft(base.Size);
		Vector3 vector2 = new Vector3(vector.x + (float)padding.left, vector.y - size.y * 0.5f);
		Vector3 vector3 = vector2 + new Vector3(size.x - (float)padding.right, 0f);
		if (convertToWorld)
		{
			float num = PixelsToUnits();
			Matrix4x4 localToWorldMatrix = base.transform.localToWorldMatrix;
			vector2 = localToWorldMatrix.MultiplyPoint(vector2 * num);
			vector3 = localToWorldMatrix.MultiplyPoint(vector3 * num);
		}
		return new Vector3[2] { vector2, vector3 };
	}

	private static Vector3 closestPoint(Vector3 start, Vector3 end, Vector3 test, bool clamp)
	{
		Vector3 rhs = test - start;
		Vector3 normalized = (end - start).normalized;
		float magnitude = (end - start).magnitude;
		float num = Vector3.Dot(normalized, rhs);
		if (clamp)
		{
			if (num < 0f)
			{
				return start;
			}
			if (num > magnitude)
			{
				return end;
			}
		}
		normalized *= num;
		return start + normalized;
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
