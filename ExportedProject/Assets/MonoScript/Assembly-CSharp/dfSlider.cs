using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/User Interface/Slider")]
[ExecuteInEditMode]
[dfCategory("Basic Controls")]
[dfTooltip("Allows the user to select any value from a specified range by moving an indicator along a horizontal or vertical track")]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_slider.html")]
public class dfSlider : dfControl
{
	[SerializeField]
	protected dfAtlas atlas;

	[SerializeField]
	protected string backgroundSprite;

	[SerializeField]
	protected dfControlOrientation orientation;

	[SerializeField]
	protected float rawValue = 10f;

	[SerializeField]
	protected float minValue;

	[SerializeField]
	protected float maxValue = 100f;

	[SerializeField]
	protected float stepSize = 1f;

	[SerializeField]
	protected float scrollSize = 1f;

	[SerializeField]
	protected dfControl thumb;

	[SerializeField]
	protected dfControl fillIndicator;

	[SerializeField]
	protected dfProgressFillMode fillMode = dfProgressFillMode.Fill;

	[SerializeField]
	protected RectOffset fillPadding = new RectOffset();

	[SerializeField]
	protected Vector2 thumbOffset = Vector2.zero;

	[SerializeField]
	protected bool rightToLeft;

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
				updateValueIndicators(rawValue);
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
				updateValueIndicators(rawValue);
				Invalidate();
			}
		}
	}

	public float StepSize
	{
		get
		{
			return stepSize;
		}
		set
		{
			value = Mathf.Max(0f, value);
			if (value != stepSize)
			{
				stepSize = value;
				Value = rawValue.Quantize(value);
				Invalidate();
			}
		}
	}

	public float ScrollSize
	{
		get
		{
			return scrollSize;
		}
		set
		{
			value = Mathf.Max(0f, value);
			if (value != scrollSize)
			{
				scrollSize = value;
				Invalidate();
			}
		}
	}

	public dfControlOrientation Orientation
	{
		get
		{
			return orientation;
		}
		set
		{
			if (value != orientation)
			{
				orientation = value;
				Invalidate();
				updateValueIndicators(rawValue);
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
			value = Mathf.Max(minValue, Mathf.Min(maxValue, value.RoundToNearest(stepSize)));
			if (!Mathf.Approximately(value, rawValue))
			{
				rawValue = value;
				OnValueChanged();
			}
		}
	}

	public dfControl Thumb
	{
		get
		{
			return thumb;
		}
		set
		{
			if (value != thumb)
			{
				thumb = value;
				Invalidate();
				updateValueIndicators(rawValue);
			}
		}
	}

	public dfControl Progress
	{
		get
		{
			return fillIndicator;
		}
		set
		{
			if (value != fillIndicator)
			{
				fillIndicator = value;
				Invalidate();
				updateValueIndicators(rawValue);
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

	public RectOffset FillPadding
	{
		get
		{
			if (fillPadding == null)
			{
				fillPadding = new RectOffset();
			}
			return fillPadding;
		}
		set
		{
			if (!object.Equals(value, fillPadding))
			{
				fillPadding = value;
				updateValueIndicators(rawValue);
				Invalidate();
			}
		}
	}

	public Vector2 ThumbOffset
	{
		get
		{
			return thumbOffset;
		}
		set
		{
			if (Vector2.Distance(value, thumbOffset) > float.Epsilon)
			{
				thumbOffset = value;
				updateValueIndicators(rawValue);
			}
		}
	}

	public bool RightToLeft
	{
		get
		{
			return rightToLeft;
		}
		set
		{
			if (value != rightToLeft)
			{
				rightToLeft = value;
				updateValueIndicators(rawValue);
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

	public event PropertyChangedEventHandler<float> ValueChanged;

	protected internal override void OnKeyDown(dfKeyEventArgs args)
	{
		if (args.Used)
		{
			return;
		}
		if (Orientation == dfControlOrientation.Horizontal)
		{
			if (args.KeyCode == KeyCode.LeftArrow)
			{
				Value -= ((!rightToLeft) ? scrollSize : (0f - scrollSize));
				args.Use();
				return;
			}
			if (args.KeyCode == KeyCode.RightArrow)
			{
				Value += ((!rightToLeft) ? scrollSize : (0f - scrollSize));
				args.Use();
				return;
			}
		}
		else
		{
			if (args.KeyCode == KeyCode.UpArrow)
			{
				Value += ScrollSize;
				args.Use();
				return;
			}
			if (args.KeyCode == KeyCode.DownArrow)
			{
				Value -= ScrollSize;
				args.Use();
				return;
			}
		}
		base.OnKeyDown(args);
	}

	public override void Start()
	{
		base.Start();
		updateValueIndicators(rawValue);
	}

	public override void OnEnable()
	{
		if (size.magnitude < float.Epsilon)
		{
			size = new Vector2(100f, 25f);
		}
		base.OnEnable();
		updateValueIndicators(rawValue);
	}

	protected internal override void OnMouseWheel(dfMouseEventArgs args)
	{
		int num = ((orientation != 0) ? 1 : (-1));
		Value += scrollSize * args.WheelDelta * (float)num;
		args.Use();
		Signal("OnMouseWheel", args);
		raiseMouseWheelEvent(args);
	}

	protected internal override void OnMouseMove(dfMouseEventArgs args)
	{
		if (!args.Buttons.IsSet(dfMouseButtons.Left))
		{
			base.OnMouseMove(args);
			return;
		}
		Value = getValueFromMouseEvent(args);
		args.Use();
		Signal("OnMouseMove", this, args);
		raiseMouseMoveEvent(args);
	}

	protected internal override void OnMouseDown(dfMouseEventArgs args)
	{
		if (!args.Buttons.IsSet(dfMouseButtons.Left))
		{
			base.OnMouseMove(args);
			return;
		}
		Focus();
		Value = getValueFromMouseEvent(args);
		args.Use();
		Signal("OnMouseDown", this, args);
		raiseMouseDownEvent(args);
	}

	protected internal override void OnSizeChanged()
	{
		base.OnSizeChanged();
		updateValueIndicators(rawValue);
	}

	protected internal virtual void OnValueChanged()
	{
		Invalidate();
		updateValueIndicators(rawValue);
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
		}
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

	private void updateValueIndicators(float rawValue)
	{
		if (Mathf.Approximately(MinValue, MaxValue))
		{
			if (Application.isEditor)
			{
				Debug.LogWarning("Slider Min and Max values cannot be the same", this);
			}
			if (thumb != null)
			{
				thumb.IsVisible = false;
			}
			if (fillIndicator != null)
			{
				fillIndicator.IsVisible = false;
			}
			return;
		}
		if (thumb != null)
		{
			thumb.IsVisible = true;
		}
		if (fillIndicator != null)
		{
			fillIndicator.IsVisible = true;
		}
		if (thumb != null)
		{
			Vector3[] endPoints = getEndPoints(true);
			Vector3 vector = endPoints[1] - endPoints[0];
			float num = maxValue - minValue;
			float num2 = (rawValue - minValue) / num * vector.magnitude;
			Vector3 vector2 = (Vector3)thumbOffset * PixelsToUnits();
			Vector3 position = endPoints[0] + vector.normalized * num2 + vector2;
			if (orientation == dfControlOrientation.Vertical || rightToLeft)
			{
				position = endPoints[1] + -vector.normalized * num2 + vector2;
			}
			thumb.transform.position = position;
			thumb.transform.localPosition = thumb.transform.localPosition.WithY(0f);
		}
		if (!(fillIndicator == null))
		{
			RectOffset rectOffset = FillPadding;
			float num3 = (rawValue - minValue) / (maxValue - minValue);
			Vector3 relativePosition = new Vector3(rectOffset.left, rectOffset.top);
			Vector2 vector3 = size - new Vector2(rectOffset.horizontal, rectOffset.vertical);
			dfSprite dfSprite2 = fillIndicator as dfSprite;
			if (dfSprite2 != null && fillMode == dfProgressFillMode.Fill)
			{
				dfSprite2.FillAmount = num3;
				dfSprite2.FillDirection = ((orientation != 0) ? dfFillDirection.Vertical : dfFillDirection.Horizontal);
				dfSprite2.InvertFill = rightToLeft || orientation == dfControlOrientation.Vertical;
			}
			else if (orientation == dfControlOrientation.Horizontal)
			{
				vector3.x = base.Width * num3 - (float)rectOffset.horizontal;
			}
			else
			{
				vector3.y = base.Height * num3 - (float)rectOffset.vertical;
				relativePosition.y = base.Height - vector3.y;
			}
			fillIndicator.Size = vector3;
			fillIndicator.RelativePosition = relativePosition;
		}
	}

	private float getValueFromMouseEvent(dfMouseEventArgs args)
	{
		Vector3[] endPoints = getEndPoints(true);
		Vector3 vector = endPoints[0];
		Vector3 vector2 = endPoints[1];
		if (orientation == dfControlOrientation.Vertical || rightToLeft)
		{
			vector = endPoints[1];
			vector2 = endPoints[0];
		}
		Plane plane = new Plane(base.transform.TransformDirection(Vector3.back), vector);
		Ray ray = args.Ray;
		float enter = 0f;
		if (!plane.Raycast(ray, out enter))
		{
			return rawValue;
		}
		Vector3 point = ray.GetPoint(enter);
		Vector3 vector3 = closestPoint(vector, vector2, point, true);
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
		Vector3 vector2 = new Vector3(vector.x, vector.y - size.y * 0.5f);
		Vector3 vector3 = vector2 + new Vector3(size.x, 0f);
		if (orientation == dfControlOrientation.Vertical)
		{
			vector2 = new Vector3(vector.x + size.x * 0.5f, vector.y);
			vector3 = vector2 - new Vector3(0f, size.y);
		}
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
}
