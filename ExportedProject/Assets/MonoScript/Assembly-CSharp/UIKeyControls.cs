using System;
using InControl;
using UnityEngine;

public class UIKeyControls : MonoBehaviour
{
	public dfControl up;

	public dfControl down;

	public dfControl left;

	public dfControl right;

	public bool selectOnAction;

	public bool clearRepeatingOnSelect;

	private dfControl selfControl;

	public Action OnUpDown;

	public Action OnDownDown;

	public Action OnLeftDown;

	public Action OnRightDown;

	public Action<dfControl> OnNewControlSelected;

	private static bool m_hasCheckedThisFrame;

	private static UIKeyControls m_lastFocusedUIKeyControl;

	private static float m_timer;

	private const float TIMER_THRESHOLD = 1.5f;

	private dfButton button;

	public void Awake()
	{
		button = GetComponent<dfButton>();
		selfControl = GetComponent<dfControl>();
		if (clearRepeatingOnSelect)
		{
			button.GotFocus += GotFocus;
		}
	}

	private static void CheckForControllerFails()
	{
		if (BraveInput.PrimaryPlayerInstance != null && BraveInput.PrimaryPlayerInstance.ActiveActions != null && BraveInput.PrimaryPlayerInstance.ActiveActions.LastInputType != BindingSourceType.MouseBindingSource)
		{
			if (m_lastFocusedUIKeyControl != null && dfGUIManager.ActiveControl != null && dfGUIManager.ActiveControl.GetComponent<UIKeyControls>() == null && dfGUIManager.ActiveControl.GetComponent<BraveOptionsMenuItem>() == null)
			{
				m_timer += GameManager.INVARIANT_DELTA_TIME;
				if (m_timer > 1.5f)
				{
					m_lastFocusedUIKeyControl.selfControl.Focus();
				}
			}
			else
			{
				m_timer = 0f;
			}
		}
		else
		{
			m_timer = 0f;
		}
		m_hasCheckedThisFrame = true;
	}

	public void Update()
	{
		if (selfControl.HasFocus)
		{
			m_lastFocusedUIKeyControl = this;
		}
		if (!m_hasCheckedThisFrame)
		{
			CheckForControllerFails();
		}
	}

	public void LateUpdate()
	{
		m_hasCheckedThisFrame = false;
	}

	public void OnKeyDown(dfControl sender, dfKeyEventArgs args)
	{
		if (args.Used)
		{
			return;
		}
		if (args.KeyCode == KeyCode.UpArrow)
		{
			if ((bool)up)
			{
				if (OnNewControlSelected != null)
				{
					OnNewControlSelected(up);
				}
				up.Focus();
			}
			if (OnUpDown != null)
			{
				OnUpDown();
			}
		}
		else if (args.KeyCode == KeyCode.DownArrow)
		{
			if ((bool)down)
			{
				if (OnNewControlSelected != null)
				{
					OnNewControlSelected(down);
				}
				down.Focus();
			}
			if (OnDownDown != null)
			{
				OnDownDown();
			}
		}
		else if (args.KeyCode == KeyCode.LeftArrow)
		{
			if ((bool)left)
			{
				if (OnNewControlSelected != null)
				{
					OnNewControlSelected(left);
				}
				left.Focus();
			}
			if (OnLeftDown != null)
			{
				OnLeftDown();
			}
		}
		else if (args.KeyCode == KeyCode.RightArrow)
		{
			if ((bool)right)
			{
				if (OnNewControlSelected != null)
				{
					OnNewControlSelected(right);
				}
				right.Focus();
			}
			if (OnRightDown != null)
			{
				OnRightDown();
			}
		}
		if (selectOnAction && (bool)button && args.KeyCode == KeyCode.Return)
		{
			AkSoundEngine.PostEvent("Play_UI_menu_confirm_01", base.gameObject);
			button.DoClick();
		}
	}

	private void GotFocus(dfControl control, dfFocusEventArgs args)
	{
		if (clearRepeatingOnSelect)
		{
			if (BraveInput.PrimaryPlayerInstance != null)
			{
				GungeonActions activeActions = BraveInput.PrimaryPlayerInstance.ActiveActions;
				activeActions.SelectUp.ResetRepeating();
				activeActions.SelectDown.ResetRepeating();
				activeActions.SelectLeft.ResetRepeating();
				activeActions.SelectRight.ResetRepeating();
			}
			if (BraveInput.SecondaryPlayerInstance != null)
			{
				GungeonActions activeActions = BraveInput.SecondaryPlayerInstance.ActiveActions;
				activeActions.SelectUp.ResetRepeating();
				activeActions.SelectDown.ResetRepeating();
				activeActions.SelectLeft.ResetRepeating();
				activeActions.SelectRight.ResetRepeating();
			}
		}
	}
}
