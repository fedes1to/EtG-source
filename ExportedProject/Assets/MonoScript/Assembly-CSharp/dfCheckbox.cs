using System;
using UnityEngine;

[Serializable]
[AddComponentMenu("Daikon Forge/User Interface/Checkbox")]
[dfHelp("http://www.daikonforge.com/docs/df-gui/classdf_checkbox.html")]
[dfCategory("Basic Controls")]
[ExecuteInEditMode]
[dfTooltip("Implements a standard checkbox (or toggle) control")]
public class dfCheckbox : dfControl
{
	[SerializeField]
	protected bool isChecked;

	[SerializeField]
	protected dfControl checkIcon;

	[SerializeField]
	protected dfLabel label;

	[SerializeField]
	protected dfControl group;

	[SerializeField]
	protected bool clickWhenSpacePressed = true;

	public bool ClickWhenSpacePressed
	{
		get
		{
			return clickWhenSpacePressed;
		}
		set
		{
			clickWhenSpacePressed = value;
		}
	}

	public bool IsChecked
	{
		get
		{
			return isChecked;
		}
		set
		{
			if (value != isChecked)
			{
				isChecked = value;
				OnCheckChanged();
				if (value && group != null)
				{
					handleGroupedCheckboxChecked();
				}
			}
		}
	}

	public dfControl CheckIcon
	{
		get
		{
			return checkIcon;
		}
		set
		{
			if (value != checkIcon)
			{
				checkIcon = value;
				Invalidate();
			}
		}
	}

	public dfLabel Label
	{
		get
		{
			return label;
		}
		set
		{
			if (value != label)
			{
				label = value;
				Invalidate();
			}
		}
	}

	public dfControl GroupContainer
	{
		get
		{
			return group;
		}
		set
		{
			if (value != group)
			{
				group = value;
				Invalidate();
			}
		}
	}

	public string Text
	{
		get
		{
			if (label != null)
			{
				return label.Text;
			}
			return "[LABEL NOT SET]";
		}
		set
		{
			if (label != null)
			{
				label.Text = value;
			}
		}
	}

	public override bool CanFocus
	{
		get
		{
			return base.IsEnabled && base.IsVisible;
		}
	}

	public event PropertyChangedEventHandler<bool> CheckChanged;

	public override void Start()
	{
		base.Start();
		if (checkIcon != null)
		{
			checkIcon.BringToFront();
			checkIcon.IsVisible = IsChecked;
		}
	}

	protected internal override void OnKeyPress(dfKeyEventArgs args)
	{
		if (ClickWhenSpacePressed && IsInteractive && args.KeyCode == KeyCode.Space)
		{
			OnClick(new dfMouseEventArgs(this, dfMouseButtons.Left, 1, default(Ray), Vector2.zero, 0f));
		}
		else
		{
			base.OnKeyPress(args);
		}
	}

	protected internal override void OnClick(dfMouseEventArgs args)
	{
		base.OnClick(args);
		if (IsInteractive)
		{
			if (group == null)
			{
				IsChecked = !IsChecked;
			}
			else
			{
				handleGroupedCheckboxChecked();
			}
			args.Use();
		}
	}

	protected internal void OnCheckChanged()
	{
		SignalHierarchy("OnCheckChanged", this, isChecked);
		if (this.CheckChanged != null)
		{
			this.CheckChanged(this, isChecked);
		}
		if (checkIcon != null)
		{
			if (IsChecked)
			{
				checkIcon.BringToFront();
			}
			checkIcon.IsVisible = IsChecked;
		}
	}

	private void handleGroupedCheckboxChecked()
	{
		if (group == null)
		{
			return;
		}
		dfCheckbox[] componentsInChildren = group.transform.GetComponentsInChildren<dfCheckbox>();
		foreach (dfCheckbox dfCheckbox2 in componentsInChildren)
		{
			if (dfCheckbox2 != this && dfCheckbox2.GroupContainer == GroupContainer && dfCheckbox2.IsChecked)
			{
				dfCheckbox2.IsChecked = false;
			}
		}
		IsChecked = true;
	}
}
