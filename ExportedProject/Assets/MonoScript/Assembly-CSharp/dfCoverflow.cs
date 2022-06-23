using System;
using UnityEngine;

[Serializable]
[ExecuteInEditMode]
[RequireComponent(typeof(dfPanel))]
[AddComponentMenu("Daikon Forge/Examples/Coverflow/Scroller")]
public class dfCoverflow : MonoBehaviour
{
	[SerializeField]
	public int selectedIndex;

	[SerializeField]
	public int itemSize = 200;

	[SerializeField]
	public float time = 0.33f;

	[SerializeField]
	public int spacing = 5;

	[SerializeField]
	protected AnimationCurve rotationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	protected AnimationCurve opacityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	[SerializeField]
	protected bool autoSelectOnStart = true;

	private dfPanel container;

	private dfList<dfControl> controls;

	private dfAnimatedFloat currentX;

	private Vector2 touchStartPosition;

	private int lastSelected = -1;

	private bool isMouseDown;

	public event ValueChangedEventHandler<int> SelectedIndexChanged;

	public void OnEnable()
	{
		container = GetComponent<dfPanel>();
		container.Pivot = dfPivotPoint.MiddleCenter;
		container.ControlAdded += container_ControlCollectionChanged;
		container.ControlRemoved += container_ControlCollectionChanged;
		controls = new dfList<dfControl>(container.Controls);
		if (rotationCurve.keys.Length == 0)
		{
			rotationCurve.AddKey(0f, 0f);
			rotationCurve.AddKey(1f, 1f);
		}
	}

	public void OnDisable()
	{
		if (container != null)
		{
			container.ControlAdded -= container_ControlCollectionChanged;
			container.ControlRemoved -= container_ControlCollectionChanged;
		}
	}

	public void Update()
	{
		if (controls == null || controls.Count == 0)
		{
			setSelectedIndex(0);
			return;
		}
		if (isMouseDown)
		{
			dfControl dfControl2 = findClosestItemToCenter();
			if (dfControl2 != null)
			{
				setSelectedIndex(controls.IndexOf(dfControl2));
				lastSelected = selectedIndex;
			}
		}
		int b = Mathf.Max(0, selectedIndex);
		b = Mathf.Min(controls.Count - 1, b);
		setSelectedIndex(b);
		if (Application.isPlaying)
		{
			updateSlides();
		}
		else
		{
			layoutSlidesForEditor();
		}
	}

	public void OnMouseEnter(dfControl control, dfMouseEventArgs args)
	{
		touchStartPosition = args.Position;
	}

	public void OnMouseDown(dfControl control, dfMouseEventArgs args)
	{
		touchStartPosition = args.Position;
		isMouseDown = true;
	}

	public void OnDragStart(dfControl control, dfDragEventArgs args)
	{
		if (args.Used)
		{
			isMouseDown = false;
		}
	}

	public void OnMouseUp(dfControl control, dfMouseEventArgs args)
	{
		if (isMouseDown)
		{
			isMouseDown = false;
			dfControl dfControl2 = findClosestItemToCenter();
			if (dfControl2 != null)
			{
				lastSelected = -1;
				setSelectedIndex(controls.IndexOf(dfControl2));
			}
		}
	}

	public void OnMouseMove(dfControl control, dfMouseEventArgs args)
	{
		if ((args is dfTouchEventArgs || isMouseDown) && !args.Used && (args.Position - touchStartPosition).magnitude > 5f)
		{
			currentX = (float)currentX + args.MoveDelta.x;
			args.Use();
		}
	}

	public void OnResolutionChanged(dfControl control, Vector2 previousResolution, Vector2 currentResolution)
	{
		lastSelected = -1;
	}

	private void container_ControlCollectionChanged(dfControl panel, dfControl child)
	{
		controls = new dfList<dfControl>(panel.Controls);
		if (autoSelectOnStart && Application.isPlaying)
		{
			setSelectedIndex(controls.Count / 2);
		}
	}

	public void OnKeyDown(dfControl sender, dfKeyEventArgs args)
	{
		if (!args.Used)
		{
			if (args.KeyCode == KeyCode.RightArrow)
			{
				setSelectedIndex(selectedIndex + 1);
			}
			else if (args.KeyCode == KeyCode.LeftArrow)
			{
				setSelectedIndex(selectedIndex - 1);
			}
			else if (args.KeyCode == KeyCode.Home)
			{
				setSelectedIndex(0);
			}
			else if (args.KeyCode == KeyCode.End)
			{
				setSelectedIndex(controls.Count - 1);
			}
		}
	}

	public void OnMouseWheel(dfControl sender, dfMouseEventArgs args)
	{
		if (!args.Used)
		{
			args.Use();
			container.Focus();
			setSelectedIndex(selectedIndex - (int)Mathf.Sign(args.WheelDelta));
		}
	}

	public void OnClick(dfControl sender, dfMouseEventArgs args)
	{
		if (!(args.Source == container) && !(Vector2.Distance(args.Position, touchStartPosition) > 20f))
		{
			dfControl dfControl2 = args.Source;
			while (dfControl2 != null && !controls.Contains(dfControl2))
			{
				dfControl2 = dfControl2.Parent;
			}
			if (dfControl2 != null)
			{
				lastSelected = -1;
				setSelectedIndex(controls.IndexOf(dfControl2));
				isMouseDown = false;
			}
		}
	}

	private void setSelectedIndex(int value)
	{
		if (value != selectedIndex)
		{
			selectedIndex = value;
			if (this.SelectedIndexChanged != null)
			{
				this.SelectedIndexChanged(this, value);
			}
			base.gameObject.Signal("OnSelectedIndexChanged", this, value);
		}
	}

	private dfControl findClosestItemToCenter()
	{
		float num = float.MaxValue;
		dfControl result = null;
		for (int i = 0; i < controls.Count; i++)
		{
			dfControl dfControl2 = controls[i];
			float sqrMagnitude = (dfControl2.transform.position - container.transform.position).sqrMagnitude;
			if (sqrMagnitude <= num)
			{
				num = sqrMagnitude;
				result = dfControl2;
			}
		}
		return result;
	}

	private void layoutSlidesForEditor()
	{
		dfList<dfControl> dfList2 = container.Controls;
		int num = 0;
		float y = (container.Height - (float)itemSize) * 0.5f;
		Vector2 size = Vector2.one * itemSize;
		for (int i = 0; i < dfList2.Count; i++)
		{
			dfList2[i].Size = size;
			dfList2[i].RelativePosition = new Vector3(num, y);
			num += itemSize + Mathf.Max(0, spacing);
		}
	}

	private void updateSlides()
	{
		if (currentX == null || selectedIndex != lastSelected)
		{
			float startValue = ((currentX == null) ? 0f : currentX.Value);
			currentX = new dfAnimatedFloat(startValue, calculateTargetPosition(), time)
			{
				EasingType = dfEasingType.SineEaseOut
			};
			lastSelected = selectedIndex;
		}
		float y = (container.Height - (float)itemSize) * 0.5f;
		Vector3 relativePosition = new Vector3(currentX, y);
		int count = controls.Count;
		for (int i = 0; i < count; i++)
		{
			dfControl dfControl2 = controls[i];
			dfControl2.Size = new Vector2(itemSize, itemSize);
			dfControl2.RelativePosition = relativePosition;
			dfControl2.Pivot = dfPivotPoint.MiddleCenter;
			if (Application.isPlaying)
			{
				Quaternion localRotation = Quaternion.Euler(0f, calcHorzRotation(relativePosition.x), 0f);
				dfControl2.transform.localRotation = localRotation;
				float num = calcScale(relativePosition.x);
				dfControl2.transform.localScale = Vector3.one * num;
				dfControl2.Opacity = calcItemOpacity(relativePosition.x);
			}
			else
			{
				dfControl2.transform.localScale = Vector3.one;
				dfControl2.transform.localRotation = Quaternion.identity;
			}
			relativePosition.x += itemSize + spacing;
		}
		if (Application.isPlaying)
		{
			int num2 = 0;
			for (int j = 0; j < selectedIndex; j++)
			{
				controls[j].ZOrder = num2++;
			}
			for (int num3 = count - 1; num3 >= selectedIndex; num3--)
			{
				controls[num3].ZOrder = num2++;
			}
		}
	}

	private float calcScale(float offset)
	{
		float num = (container.Width - (float)itemSize) * 0.5f;
		float num2 = Mathf.Abs(num - offset);
		int totalSize = getTotalSize();
		return Mathf.Max(1f - num2 / (float)totalSize, 0.85f);
	}

	private float calcItemOpacity(float offset)
	{
		float num = (container.Width - (float)itemSize) * 0.5f;
		float num2 = Mathf.Abs(num - offset);
		int totalSize = getTotalSize();
		float num3 = num2 / (float)totalSize;
		return 1f - opacityCurve.Evaluate(num3);
	}

	private float calcHorzRotation(float offset)
	{
		float num = (container.Width - (float)itemSize) * 0.5f;
		float num2 = Mathf.Abs(num - offset);
		float num3 = Mathf.Sign(num - offset);
		int totalSize = getTotalSize();
		float num4 = num2 / (float)totalSize;
		num4 = rotationCurve.Evaluate(num4);
		return num4 * 90f * (0f - num3);
	}

	private int getTotalSize()
	{
		int count = controls.Count;
		return count * itemSize + Mathf.Max(count, 0) * spacing;
	}

	private float calculateTargetPosition()
	{
		float num = (container.Width - (float)itemSize) * 0.5f;
		float num2 = num - (float)(selectedIndex * itemSize);
		if (selectedIndex > 0)
		{
			num2 -= (float)(selectedIndex * spacing);
		}
		return num2;
	}
}
