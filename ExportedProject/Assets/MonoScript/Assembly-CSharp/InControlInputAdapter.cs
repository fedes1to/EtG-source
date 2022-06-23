using InControl;
using UnityEngine;

public class InControlInputAdapter : MonoBehaviour
{
	private static int m_skipInputForRestOfFrame;

	public static bool CurrentlyUsingAllDevices;

	public static bool SkipInputForRestOfFrame
	{
		get
		{
			return m_skipInputForRestOfFrame > 0;
		}
		set
		{
			m_skipInputForRestOfFrame = (value ? Mathf.Max(5, m_skipInputForRestOfFrame) : 0);
		}
	}

	private void OnEnable()
	{
	}

	private void Update()
	{
		if (GameManager.Instance.IsLoadingLevel)
		{
			return;
		}
		bool didProcessInput = SkipInputForRestOfFrame;
		ProcessPrimaryPlayerInput(ref didProcessInput);
		if (GameManager.Instance.IsPaused && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			ProcessSecondaryPlayerInput(ref didProcessInput);
		}
		if (CurrentlyUsingAllDevices)
		{
			for (int i = 0; i < InputManager.Devices.Count; i++)
			{
				ProcessRawDeviceInput(InputManager.Devices[i], ref didProcessInput);
			}
		}
	}

	private void ProcessRawDeviceInput(InputDevice device, ref bool didProcessInput)
	{
		dfControl activeControl = dfGUIManager.ActiveControl;
		if (!(activeControl == null) && activeControl.transform.IsChildOf(base.transform))
		{
			if (!device.LeftStickUp.IsPressed)
			{
				HandleControl(device.DPadUp, activeControl, KeyCode.UpArrow, ref didProcessInput, true);
			}
			if (!device.LeftStickRight.IsPressed)
			{
				HandleControl(device.DPadRight, activeControl, KeyCode.RightArrow, ref didProcessInput, true);
			}
			if (!device.LeftStickDown.IsPressed)
			{
				HandleControl(device.DPadDown, activeControl, KeyCode.DownArrow, ref didProcessInput, true);
			}
			if (!device.LeftStickLeft.IsPressed)
			{
				HandleControl(device.DPadLeft, activeControl, KeyCode.LeftArrow, ref didProcessInput, true);
			}
			if (!device.DPadUp.IsPressed)
			{
				HandleControlAsDpad(device.LeftStickUp, activeControl, KeyCode.UpArrow, ref didProcessInput, true);
			}
			if (!device.DPadRight.IsPressed)
			{
				HandleControlAsDpad(device.LeftStickRight, activeControl, KeyCode.RightArrow, ref didProcessInput, true);
			}
			if (!device.DPadDown.IsPressed)
			{
				HandleControlAsDpad(device.LeftStickDown, activeControl, KeyCode.DownArrow, ref didProcessInput, true);
			}
			if (!device.DPadLeft.IsPressed)
			{
				HandleControlAsDpad(device.LeftStickLeft, activeControl, KeyCode.LeftArrow, ref didProcessInput, true);
			}
			switch (GungeonActions.LocalizedMenuSelectAction)
			{
			case InputControlType.Action1:
				HandleControl(device.Action1, activeControl, KeyCode.Return, ref didProcessInput);
				break;
			case InputControlType.Action2:
				HandleControl(device.Action2, activeControl, KeyCode.Return, ref didProcessInput);
				break;
			}
			switch (GungeonActions.LocalizedMenuCancelAction)
			{
			case InputControlType.Action1:
				HandleControl(device.Action1, activeControl, KeyCode.Escape, ref didProcessInput);
				break;
			case InputControlType.Action2:
				HandleControl(device.Action2, activeControl, KeyCode.Escape, ref didProcessInput);
				break;
			}
		}
	}

	private void ProcessPrimaryPlayerInput(ref bool didProcessInput)
	{
		if (BraveInput.PrimaryPlayerInstance == null)
		{
			return;
		}
		GungeonActions activeActions = BraveInput.PrimaryPlayerInstance.ActiveActions;
		dfControl activeControl = dfGUIManager.ActiveControl;
		if (!(activeControl == null) && activeControl.transform.IsChildOf(base.transform))
		{
			HandleControlAsDpad(activeActions.SelectUp, activeControl, KeyCode.UpArrow, ref didProcessInput, true);
			HandleControlAsDpad(activeActions.SelectDown, activeControl, KeyCode.DownArrow, ref didProcessInput, true);
			HandleControlAsDpad(activeActions.SelectLeft, activeControl, KeyCode.LeftArrow, ref didProcessInput, true);
			HandleControlAsDpad(activeActions.SelectRight, activeControl, KeyCode.RightArrow, ref didProcessInput, true);
			HandleControl(activeActions.MenuSelectAction, activeControl, KeyCode.Return, ref didProcessInput);
			HandleControl(activeActions.CancelAction, activeControl, KeyCode.Escape, ref didProcessInput);
			if (Input.GetKeyUp(KeyCode.Return))
			{
				activeControl.OnKeyUp(new dfKeyEventArgs(activeControl, KeyCode.Return, false, false, false));
				didProcessInput = true;
			}
		}
	}

	private void ProcessSecondaryPlayerInput(ref bool didProcessInput)
	{
		GungeonActions activeActions = BraveInput.SecondaryPlayerInstance.ActiveActions;
		if (activeActions != null && !activeActions.ForceDisable)
		{
			dfControl activeControl = dfGUIManager.ActiveControl;
			if (!(activeControl == null) && activeControl.transform.IsChildOf(base.transform))
			{
				HandleControlAsDpad(activeActions.SelectUp, activeControl, KeyCode.UpArrow, ref didProcessInput, true);
				HandleControlAsDpad(activeActions.SelectDown, activeControl, KeyCode.DownArrow, ref didProcessInput, true);
				HandleControlAsDpad(activeActions.SelectLeft, activeControl, KeyCode.LeftArrow, ref didProcessInput, true);
				HandleControlAsDpad(activeActions.SelectRight, activeControl, KeyCode.RightArrow, ref didProcessInput, true);
				HandleControl(activeActions.MenuSelectAction, activeControl, KeyCode.Return, ref didProcessInput);
				HandleControl(activeActions.CancelAction, activeControl, KeyCode.Escape, ref didProcessInput);
			}
		}
	}

	public void LateUpdate()
	{
		m_skipInputForRestOfFrame = Mathf.Max(m_skipInputForRestOfFrame - 1, 0);
	}

	private static void HandleControl(OneAxisInputControl control, dfControl target, KeyCode keyCode, ref bool didProcessInput, bool repeating = false)
	{
		if ((!repeating) ? control.WasPressed : control.WasPressedRepeating)
		{
			if (!didProcessInput)
			{
				target.OnKeyDown(new dfKeyEventArgs(target, keyCode, false, false, false));
				didProcessInput = true;
			}
		}
		else if (control.WasReleased && !didProcessInput)
		{
			target.OnKeyUp(new dfKeyEventArgs(target, keyCode, false, false, false));
			didProcessInput = true;
		}
	}

	private static void HandleControlAsDpad(OneAxisInputControl control, dfControl target, KeyCode keyCode, ref bool didProcessInput, bool repeating = false)
	{
		if ((!repeating) ? control.WasPressedAsDpad : control.WasPressedAsDpadRepeating)
		{
			if (!didProcessInput)
			{
				target.OnKeyDown(new dfKeyEventArgs(target, keyCode, false, false, false));
				didProcessInput = true;
			}
		}
		else if (control.WasReleased && !didProcessInput)
		{
			target.OnKeyUp(new dfKeyEventArgs(target, keyCode, false, false, false));
			didProcessInput = true;
		}
	}
}
