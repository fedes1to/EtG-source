using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Daikon Forge/User Interface/Panel Addon/Flow Layout")]
[ExecuteInEditMode]
public class dfPanelFlowLayout : MonoBehaviour
{
	[SerializeField]
	protected RectOffset borderPadding = new RectOffset();

	[SerializeField]
	protected Vector2 itemSpacing = default(Vector2);

	[SerializeField]
	protected dfControlOrientation flowDirection;

	[SerializeField]
	protected bool hideClippedControls;

	[SerializeField]
	protected int maxLayoutSize;

	[SerializeField]
	protected List<dfControl> excludedControls = new List<dfControl>();

	private dfPanel panel;

	public dfControlOrientation Direction
	{
		get
		{
			return flowDirection;
		}
		set
		{
			if (value != flowDirection)
			{
				flowDirection = value;
				PerformLayout();
			}
		}
	}

	public Vector2 ItemSpacing
	{
		get
		{
			return itemSpacing;
		}
		set
		{
			value = Vector2.Max(value, Vector2.zero);
			if (!object.Equals(value, itemSpacing))
			{
				itemSpacing = value;
				PerformLayout();
			}
		}
	}

	public RectOffset BorderPadding
	{
		get
		{
			if (borderPadding == null)
			{
				borderPadding = new RectOffset();
			}
			return borderPadding;
		}
		set
		{
			value = value.ConstrainPadding();
			if (!object.Equals(value, borderPadding))
			{
				borderPadding = value;
				PerformLayout();
			}
		}
	}

	public bool HideClippedControls
	{
		get
		{
			return hideClippedControls;
		}
		set
		{
			if (value != hideClippedControls)
			{
				hideClippedControls = value;
				PerformLayout();
			}
		}
	}

	public int MaxLayoutSize
	{
		get
		{
			return maxLayoutSize;
		}
		set
		{
			if (value != maxLayoutSize)
			{
				maxLayoutSize = value;
				PerformLayout();
			}
		}
	}

	public List<dfControl> ExcludedControls
	{
		get
		{
			return excludedControls;
		}
	}

	public void OnEnable()
	{
		panel = GetComponent<dfPanel>();
		if (panel == null)
		{
			Debug.LogError("The " + GetType().Name + " component requires a dfPanel component.", base.gameObject);
			base.enabled = false;
		}
		else
		{
			panel.SizeChanged += OnSizeChanged;
		}
	}

	public void OnDisable()
	{
		if (panel != null)
		{
			panel.SizeChanged -= OnSizeChanged;
			panel = null;
		}
	}

	public void OnControlAdded(dfControl container, dfControl child)
	{
		child.ZOrderChanged += child_ZOrderChanged;
		child.SizeChanged += child_SizeChanged;
		PerformLayout();
	}

	public void OnControlRemoved(dfControl container, dfControl child)
	{
		child.ZOrderChanged -= child_ZOrderChanged;
		child.SizeChanged -= child_SizeChanged;
		PerformLayout();
	}

	public void OnSizeChanged(dfControl control, Vector2 value)
	{
		PerformLayout();
	}

	private void child_SizeChanged(dfControl control, Vector2 value)
	{
		PerformLayout();
	}

	private void child_ZOrderChanged(dfControl control, int value)
	{
		PerformLayout();
	}

	public void PerformLayout()
	{
		if (panel == null)
		{
			panel = GetComponent<dfPanel>();
		}
		Vector3 relativePosition = new Vector3(borderPadding.left, borderPadding.top);
		bool flag = true;
		float num = ((flowDirection != 0 || maxLayoutSize <= 0) ? (panel.Width - (float)borderPadding.right) : ((float)maxLayoutSize));
		float num2 = ((flowDirection != dfControlOrientation.Vertical || maxLayoutSize <= 0) ? (panel.Height - (float)borderPadding.bottom) : ((float)maxLayoutSize));
		int num3 = 0;
		dfList<dfControl> controls = panel.Controls;
		int num4 = 0;
		while (num4 < controls.Count)
		{
			dfControl dfControl2 = controls[num4];
			if (dfControl2.enabled && dfControl2.gameObject.activeSelf && !excludedControls.Contains(dfControl2))
			{
				if (!flag)
				{
					if (flowDirection == dfControlOrientation.Horizontal)
					{
						relativePosition.x += itemSpacing.x;
					}
					else
					{
						relativePosition.y += itemSpacing.y;
					}
				}
				if (flowDirection == dfControlOrientation.Horizontal)
				{
					if (!flag && relativePosition.x + dfControl2.Width > num + float.Epsilon)
					{
						relativePosition.x = borderPadding.left;
						relativePosition.y += num3;
						num3 = 0;
						flag = true;
					}
				}
				else if (!flag && relativePosition.y + dfControl2.Height > num2 + float.Epsilon)
				{
					relativePosition.y = borderPadding.top;
					relativePosition.x += num3;
					num3 = 0;
					flag = true;
				}
				dfControl2.RelativePosition = relativePosition;
				if (flowDirection == dfControlOrientation.Horizontal)
				{
					relativePosition.x += dfControl2.Width;
					num3 = Mathf.Max(Mathf.CeilToInt(dfControl2.Height + itemSpacing.y), num3);
				}
				else
				{
					relativePosition.y += dfControl2.Height;
					num3 = Mathf.Max(Mathf.CeilToInt(dfControl2.Width + itemSpacing.x), num3);
				}
				dfControl2.IsVisible = canShowControlUnclipped(dfControl2);
			}
			num4++;
			flag = false;
		}
	}

	private bool canShowControlUnclipped(dfControl control)
	{
		if (!hideClippedControls)
		{
			return true;
		}
		Vector3 relativePosition = control.RelativePosition;
		if (relativePosition.x + control.Width >= panel.Width - (float)borderPadding.right)
		{
			return false;
		}
		if (relativePosition.y + control.Height >= panel.Height - (float)borderPadding.bottom)
		{
			return false;
		}
		return true;
	}
}
