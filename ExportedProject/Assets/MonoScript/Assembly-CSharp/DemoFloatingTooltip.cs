using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Tooltip/Floating Tooltip")]
public class DemoFloatingTooltip : MonoBehaviour
{
	public float tooltipDelay = 1f;

	private dfLabel _tooltip;

	private dfControl lastControl;

	private float tooltipDelayStart;

	public void Start()
	{
		_tooltip = GetComponent<dfLabel>();
		_tooltip.IsInteractive = false;
		_tooltip.IsEnabled = false;
	}

	public void Update()
	{
		dfControl controlUnderMouse = dfInputManager.ControlUnderMouse;
		if (controlUnderMouse == null)
		{
			_tooltip.Hide();
		}
		else if (controlUnderMouse != lastControl)
		{
			tooltipDelayStart = Time.realtimeSinceStartup;
			if (string.IsNullOrEmpty(controlUnderMouse.Tooltip))
			{
				_tooltip.Hide();
			}
			else
			{
				_tooltip.Text = controlUnderMouse.Tooltip;
			}
		}
		else if (lastControl != null && !string.IsNullOrEmpty(lastControl.Tooltip) && Time.realtimeSinceStartup - tooltipDelayStart > tooltipDelay)
		{
			_tooltip.Show();
			_tooltip.BringToFront();
		}
		if (_tooltip.IsVisible)
		{
			setPosition(Input.mousePosition);
		}
		lastControl = controlUnderMouse;
	}

	private void setPosition(Vector2 position)
	{
		Vector2 vector = new Vector2(0f, _tooltip.Height + 25f);
		dfGUIManager manager = _tooltip.GetManager();
		position = manager.ScreenToGui(position) - vector;
		if (position.y < 0f)
		{
			position.y = 0f;
		}
		if (position.x + _tooltip.Width > manager.GetScreenSize().x)
		{
			position.x = manager.GetScreenSize().x - _tooltip.Width;
		}
		_tooltip.RelativePosition = position;
	}
}
