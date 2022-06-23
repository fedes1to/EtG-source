using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Containers/Auto-Arrange Options")]
[ExecuteInEditMode]
public class AutoArrangeOptions : MonoBehaviour
{
	public dfScrollPanel Panel;

	public int FlowDirection
	{
		get
		{
			return (int)Panel.FlowDirection;
		}
		set
		{
			Panel.FlowDirection = (dfScrollPanel.LayoutDirection)value;
		}
	}

	public int PaddingLeft
	{
		get
		{
			return Panel.FlowPadding.left;
		}
		set
		{
			Panel.FlowPadding.left = value;
			Panel.Reset();
		}
	}

	public int PaddingRight
	{
		get
		{
			return Panel.FlowPadding.right;
		}
		set
		{
			Panel.FlowPadding.right = value;
			Panel.Reset();
		}
	}

	public int PaddingTop
	{
		get
		{
			return Panel.FlowPadding.top;
		}
		set
		{
			Panel.FlowPadding.top = value;
			Panel.Reset();
		}
	}

	public int PaddingBottom
	{
		get
		{
			return Panel.FlowPadding.bottom;
		}
		set
		{
			Panel.FlowPadding.bottom = value;
			Panel.Reset();
		}
	}

	private void Start()
	{
		if (Panel == null)
		{
			Panel = GetComponent<dfScrollPanel>();
		}
	}

	public void ExpandAll()
	{
		for (int i = 0; i < Panel.Controls.Count; i++)
		{
			AutoArrangeDemoItem component = Panel.Controls[i].GetComponent<AutoArrangeDemoItem>();
			component.Expand();
		}
	}

	public void CollapseAll()
	{
		for (int i = 0; i < Panel.Controls.Count; i++)
		{
			AutoArrangeDemoItem component = Panel.Controls[i].GetComponent<AutoArrangeDemoItem>();
			component.Collapse();
		}
	}
}
