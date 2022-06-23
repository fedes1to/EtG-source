using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/General/Control Navigation")]
public class ControlNavigation : MonoBehaviour
{
	public bool FocusOnStart;

	public bool FocusOnMouseEnter;

	public dfControl SelectOnLeft;

	public dfControl SelectOnRight;

	public dfControl SelectOnUp;

	public dfControl SelectOnDown;

	public dfControl SelectOnTab;

	public dfControl SelectOnShiftTab;

	public dfControl SelectOnClick;

	private void OnMouseEnter(dfControl sender, dfMouseEventArgs args)
	{
		if (FocusOnMouseEnter)
		{
			dfControl component = GetComponent<dfControl>();
			if (component != null)
			{
				component.Focus();
			}
		}
	}

	private void OnClick(dfControl sender, dfMouseEventArgs args)
	{
		if (SelectOnClick != null)
		{
			SelectOnClick.Focus();
		}
	}

	private void OnKeyDown(dfControl sender, dfKeyEventArgs args)
	{
		switch (args.KeyCode)
		{
		case KeyCode.Tab:
			if (args.Shift)
			{
				if (SelectOnShiftTab != null)
				{
					SelectOnShiftTab.Focus();
					args.Use();
				}
			}
			else if (SelectOnTab != null)
			{
				SelectOnTab.Focus();
				args.Use();
			}
			break;
		case KeyCode.LeftArrow:
			if (SelectOnLeft != null)
			{
				SelectOnLeft.Focus();
				args.Use();
			}
			break;
		case KeyCode.RightArrow:
			if (SelectOnRight != null)
			{
				SelectOnRight.Focus();
				args.Use();
			}
			break;
		case KeyCode.UpArrow:
			if (SelectOnUp != null)
			{
				SelectOnUp.Focus();
				args.Use();
			}
			break;
		case KeyCode.DownArrow:
			if (SelectOnDown != null)
			{
				SelectOnDown.Focus();
				args.Use();
			}
			break;
		}
	}

	private void Awake()
	{
	}

	private void OnEnable()
	{
	}

	private void Start()
	{
		if (FocusOnStart)
		{
			dfControl component = GetComponent<dfControl>();
			if (component != null)
			{
				component.Focus();
			}
		}
	}
}
