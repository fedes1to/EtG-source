using System;
using InControl;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/User Interface/Scrollbar")]
[ExecuteInEditMode]
[dfCategory("Basic Controls")]
[dfTooltip("Implements a common Scrollbar control")]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_scrollbar.html")]
public class dfScrollbar : dfControl
{
	[SerializeField]
	public bool ControlledByRightStick;

	[NonSerialized]
	public bool IsBeingMovedByRightStick;

	[SerializeField]
	protected dfAtlas atlas;

	[SerializeField]
	protected dfControlOrientation orientation;

	[SerializeField]
	protected float rawValue = 1f;

	[SerializeField]
	protected float minValue;

	[SerializeField]
	protected float maxValue = 100f;

	[SerializeField]
	protected float stepSize = 1f;

	[SerializeField]
	protected float scrollSize = 1f;

	[SerializeField]
	protected float increment = 1f;

	[SerializeField]
	protected dfControl thumb;

	[SerializeField]
	protected dfControl track;

	[SerializeField]
	protected dfControl incButton;

	[SerializeField]
	protected dfControl decButton;

	[SerializeField]
	protected RectOffset thumbPadding = new RectOffset();

	[SerializeField]
	protected bool autoHide;

	private Vector3 thumbMouseOffset = Vector3.zero;

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
				Value = Value;
				Invalidate();
				doAutoHide();
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
				Value = Value;
				Invalidate();
				doAutoHide();
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
				Value = Value;
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
				Value = Value;
				Invalidate();
				doAutoHide();
			}
		}
	}

	public float IncrementAmount
	{
		get
		{
			return increment;
		}
		set
		{
			value = Mathf.Max(0f, value);
			if (!Mathf.Approximately(value, increment))
			{
				increment = value;
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
			value = adjustValue(value);
			if (!Mathf.Approximately(value, rawValue))
			{
				rawValue = value;
				OnValueChanged();
			}
			updateThumb(rawValue);
			doAutoHide();
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
			}
		}
	}

	public dfControl Track
	{
		get
		{
			return track;
		}
		set
		{
			if (value != track)
			{
				track = value;
				Invalidate();
			}
		}
	}

	public dfControl IncButton
	{
		get
		{
			return incButton;
		}
		set
		{
			if (value != incButton)
			{
				incButton = value;
				Invalidate();
			}
		}
	}

	public dfControl DecButton
	{
		get
		{
			return decButton;
		}
		set
		{
			if (value != decButton)
			{
				decButton = value;
				Invalidate();
			}
		}
	}

	public RectOffset ThumbPadding
	{
		get
		{
			if (thumbPadding == null)
			{
				thumbPadding = new RectOffset();
			}
			return thumbPadding;
		}
		set
		{
			if (orientation != 0)
			{
				int num3 = (value.left = (value.right = 0));
			}
			else
			{
				int num3 = (value.top = (value.bottom = 0));
			}
			if (!object.Equals(value, thumbPadding))
			{
				thumbPadding = value;
				updateThumb(rawValue);
			}
		}
	}

	public bool AutoHide
	{
		get
		{
			return autoHide;
		}
		set
		{
			if (value != autoHide)
			{
				autoHide = value;
				Invalidate();
				doAutoHide();
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

	public override Vector2 CalculateMinimumSize()
	{
		Vector2[] array = new Vector2[3];
		if (decButton != null)
		{
			array[0] = decButton.CalculateMinimumSize();
		}
		if (incButton != null)
		{
			array[1] = incButton.CalculateMinimumSize();
		}
		if (thumb != null)
		{
			array[2] = thumb.CalculateMinimumSize();
		}
		Vector2 zero = Vector2.zero;
		if (orientation == dfControlOrientation.Horizontal)
		{
			zero.x = array[0].x + array[1].x + array[2].x;
			zero.y = Mathf.Max(array[0].y, array[1].y, array[2].y);
		}
		else
		{
			zero.x = Mathf.Max(array[0].x, array[1].x, array[2].x);
			zero.y = array[0].y + array[1].y + array[2].y;
		}
		return Vector2.Max(zero, base.CalculateMinimumSize());
	}

	protected override void OnRebuildRenderData()
	{
		updateThumb(rawValue);
		base.OnRebuildRenderData();
	}

	public override void Start()
	{
		base.Start();
		attachEvents();
	}

	public override void Update()
	{
		base.Update();
		if (ControlledByRightStick && InputManager.ActiveDevice != null)
		{
			float y = InputManager.ActiveDevice.RightStick.Y;
			if (Mathf.Abs(y) > 0.1f)
			{
				IsBeingMovedByRightStick = true;
				Value += IncrementAmount * y * -6f * GameManager.INVARIANT_DELTA_TIME;
				IsBeingMovedByRightStick = false;
			}
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		detachEvents();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		detachEvents();
	}

	private void attachEvents()
	{
		if (Application.isPlaying)
		{
			if (IncButton != null)
			{
				IncButton.MouseDown += incrementPressed;
				IncButton.MouseHover += incrementPressed;
			}
			if (DecButton != null)
			{
				DecButton.MouseDown += decrementPressed;
				DecButton.MouseHover += decrementPressed;
			}
		}
	}

	private void detachEvents()
	{
		if (Application.isPlaying)
		{
			if (IncButton != null)
			{
				IncButton.MouseDown -= incrementPressed;
				IncButton.MouseHover -= incrementPressed;
			}
			if (DecButton != null)
			{
				DecButton.MouseDown -= decrementPressed;
				DecButton.MouseHover -= decrementPressed;
			}
		}
	}

	protected internal override void OnKeyDown(dfKeyEventArgs args)
	{
		if (Orientation == dfControlOrientation.Horizontal)
		{
			if (args.KeyCode == KeyCode.LeftArrow)
			{
				Value -= IncrementAmount;
				args.Use();
				return;
			}
			if (args.KeyCode == KeyCode.RightArrow)
			{
				Value += IncrementAmount;
				args.Use();
				return;
			}
		}
		else
		{
			if (args.KeyCode == KeyCode.UpArrow)
			{
				Value -= IncrementAmount;
				args.Use();
				return;
			}
			if (args.KeyCode == KeyCode.DownArrow)
			{
				Value += IncrementAmount;
				args.Use();
				return;
			}
		}
		base.OnKeyDown(args);
	}

	protected internal override void OnMouseWheel(dfMouseEventArgs args)
	{
		if (!args.Used)
		{
			Value += IncrementAmount * (0f - args.WheelDelta);
			args.Use();
			Signal("OnMouseWheel", this, args);
		}
	}

	protected internal override void OnMouseHover(dfMouseEventArgs args)
	{
		if (!(args.Source == incButton) && !(args.Source == decButton) && !(args.Source == thumb))
		{
			if (args.Source != track || !args.Buttons.IsSet(dfMouseButtons.Left))
			{
				base.OnMouseHover(args);
				return;
			}
			updateFromTrackClick(args);
			args.Use();
			Signal("OnMouseHover", this, args);
		}
	}

	protected internal override void OnMouseMove(dfMouseEventArgs args)
	{
		if (!(args.Source == incButton) && !(args.Source == decButton))
		{
			if ((args.Source != track && args.Source != thumb) || !args.Buttons.IsSet(dfMouseButtons.Left))
			{
				base.OnMouseMove(args);
				return;
			}
			float valueFromMouseEvent = getValueFromMouseEvent(args);
			float num = thumb.Height / 2f;
			float t = Mathf.InverseLerp(minValue, maxValue, valueFromMouseEvent);
			float num3 = (Value = Mathf.Lerp(minValue - num, maxValue - scrollSize + num, t));
			args.Use();
			Signal("OnMouseMove", this, args);
		}
	}

	protected internal override void OnMouseDown(dfMouseEventArgs args)
	{
		if (args.Buttons.IsSet(dfMouseButtons.Left))
		{
			Focus();
		}
		if (args.Source == incButton || args.Source == decButton)
		{
			return;
		}
		if ((args.Source != track && args.Source != thumb) || !args.Buttons.IsSet(dfMouseButtons.Left))
		{
			base.OnMouseDown(args);
			return;
		}
		if (args.Source == thumb)
		{
			RaycastHit hitInfo;
			thumb.GetComponent<Collider>().Raycast(args.Ray, out hitInfo, 1000f);
			Vector3 vector = thumb.transform.position + thumb.Pivot.TransformToCenter(thumb.Size * PixelsToUnits());
			thumbMouseOffset = vector - hitInfo.point;
		}
		else
		{
			updateFromTrackClick(args);
		}
		args.Use();
		Signal("OnMouseDown", this, args);
	}

	protected internal virtual void OnValueChanged()
	{
		doAutoHide();
		Invalidate();
		SignalHierarchy("OnValueChanged", this, Value);
		if (this.ValueChanged != null)
		{
			this.ValueChanged(this, Value);
		}
	}

	protected internal override void OnSizeChanged()
	{
		base.OnSizeChanged();
		updateThumb(rawValue);
	}

	private void doAutoHide()
	{
		if (autoHide && Application.isPlaying)
		{
			if (Mathf.CeilToInt(ScrollSize) >= Mathf.CeilToInt(maxValue - minValue))
			{
				Hide();
			}
			else
			{
				Show();
			}
		}
	}

	private void incrementPressed(dfControl sender, dfMouseEventArgs args)
	{
		if (args.Buttons.IsSet(dfMouseButtons.Left))
		{
			Value += IncrementAmount;
			args.Use();
		}
	}

	private void decrementPressed(dfControl sender, dfMouseEventArgs args)
	{
		if (args.Buttons.IsSet(dfMouseButtons.Left))
		{
			Value -= IncrementAmount;
			args.Use();
		}
	}

	private void updateFromTrackClick(dfMouseEventArgs args)
	{
		float valueFromMouseEvent = getValueFromMouseEvent(args);
		if (valueFromMouseEvent > rawValue + scrollSize)
		{
			Value += scrollSize;
		}
		else if (valueFromMouseEvent < rawValue)
		{
			Value -= scrollSize;
		}
	}

	private float adjustValue(float value)
	{
		float num = Mathf.Max(maxValue - minValue, 0f);
		float a = Mathf.Max(num - scrollSize, 0f) + minValue;
		float value2 = Mathf.Max(Mathf.Min(a, value), minValue);
		return value2.Quantize(stepSize);
	}

	private void updateThumb(float rawValue)
	{
		if (controls.Count == 0 || thumb == null || track == null || !base.IsVisible)
		{
			return;
		}
		float num = maxValue - minValue;
		if (num <= 0f || num <= scrollSize)
		{
			thumb.IsVisible = false;
			return;
		}
		thumb.IsVisible = true;
		float num2 = ((orientation != 0) ? track.Height : track.Width);
		float num3 = ((orientation != 0) ? Mathf.Min(thumb.MaximumSize.y, Mathf.Max(scrollSize / num * num2, thumb.MinimumSize.y)) : Mathf.Min(thumb.MaximumSize.x, Mathf.Max(scrollSize / num * num2, thumb.MinimumSize.x)));
		Vector2 vector = ((orientation != 0) ? new Vector2(thumb.Width, num3) : new Vector2(num3, thumb.Height));
		if (Orientation == dfControlOrientation.Horizontal)
		{
			vector.x -= thumbPadding.horizontal;
		}
		else
		{
			vector.y -= thumbPadding.vertical;
		}
		thumb.Size = vector;
		float num4 = (rawValue - minValue) / (num - scrollSize);
		float num5 = num4 * (num2 - num3);
		Vector3 vector2 = ((orientation != 0) ? Vector3.up : Vector3.right);
		Vector3 vector3 = ((Orientation != 0) ? new Vector3((track.Width - thumb.Width) * 0.5f, 0f) : new Vector3(0f, (track.Height - thumb.Height) * 0.5f));
		if (Orientation == dfControlOrientation.Horizontal)
		{
			vector3.x = thumbPadding.left;
		}
		else
		{
			vector3.y = thumbPadding.top;
		}
		if (thumb.Parent == this)
		{
			thumb.RelativePosition = track.RelativePosition + vector3 + vector2 * num5;
		}
		else
		{
			thumb.RelativePosition = vector2 * num5 + vector3;
		}
	}

	private float getValueFromMouseEvent(dfMouseEventArgs args)
	{
		Vector3[] corners = track.GetCorners();
		Vector3 vector = corners[0];
		Vector3 vector2 = corners[(orientation == dfControlOrientation.Horizontal) ? 1 : 2];
		Plane plane = new Plane(base.transform.TransformDirection(Vector3.back), vector);
		Ray ray = args.Ray;
		float enter = 0f;
		if (!plane.Raycast(ray, out enter))
		{
			return rawValue;
		}
		Vector3 test = ray.origin + ray.direction * enter;
		if (args.Source == thumb)
		{
			test += thumbMouseOffset;
		}
		Vector3 vector3 = closestPoint(vector, vector2, test, true);
		float num = (vector3 - vector).magnitude / (vector2 - vector).magnitude;
		return minValue + (maxValue - minValue) * num;
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
