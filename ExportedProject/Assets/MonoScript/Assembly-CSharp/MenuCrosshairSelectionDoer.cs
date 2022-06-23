using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCrosshairSelectionDoer : TimeInvariantMonoBehaviour
{
	public dfControl controlToPlace;

	public dfControl targetControlToEncrosshair;

	public IntVector2 leftCrosshairPixelOffset;

	public IntVector2 rightCrosshairPixelOffset;

	public bool mouseFocusable = true;

	private List<dfControl> m_extantControls;

	private dfControl m_control;

	private bool m_suppressed;

	public dfControl CrosshairControl
	{
		get
		{
			return (!(targetControlToEncrosshair == null)) ? targetControlToEncrosshair : m_control;
		}
	}

	private void Start()
	{
		m_control = GetComponent<dfControl>();
		m_extantControls = new List<dfControl>();
		if (mouseFocusable || true)
		{
			m_control.MouseEnter += delegate
			{
				if (!InControlInputAdapter.SkipInputForRestOfFrame)
				{
					m_control.Focus(false);
				}
			};
		}
		m_control.GotFocus += GotFocus;
		m_control.LostFocus += LostFocus;
		UIKeyControls component = GetComponent<UIKeyControls>();
		if (component != null)
		{
			component.OnNewControlSelected = (Action<dfControl>)Delegate.Combine(component.OnNewControlSelected, new Action<dfControl>(DifferentControlSelected));
		}
		BraveOptionsMenuItem component2 = GetComponent<BraveOptionsMenuItem>();
		if (component2 != null)
		{
			component2.OnNewControlSelected = (Action<dfControl>)Delegate.Combine(component2.OnNewControlSelected, new Action<dfControl>(DifferentControlSelected));
		}
	}

	private void LateUpdate()
	{
		if (!m_suppressed && m_extantControls != null && m_extantControls.Count > 0)
		{
			UpdatedOwnedControls();
		}
	}

	private void DifferentControlSelected(dfControl newControl)
	{
		MenuCrosshairSelectionDoer component = newControl.GetComponent<MenuCrosshairSelectionDoer>();
		if (component != null && m_extantControls.Count == 2)
		{
			m_suppressed = true;
			component.m_suppressed = true;
			StartCoroutine(HandleLerpyDerpy(component));
		}
	}

	private IEnumerator HandleLerpyDerpy(MenuCrosshairSelectionDoer targetCrosshairDoer)
	{
		yield return null;
		float elapsed = 0f;
		float duration = 0.1f;
		dfControl leftControl = m_extantControls[0];
		dfControl rightControl = m_extantControls[1];
		Vector3 startPosL = leftControl.transform.position;
		Vector3 startPosR = rightControl.transform.position;
		float offsetWidth2 = targetCrosshairDoer.CrosshairControl.Size.x / 2f + leftControl.Size.x / 2f + 6f;
		offsetWidth2 *= leftControl.PixelsToUnits();
		float rightOffset = -3f * CrosshairControl.PixelsToUnits();
		Vector3 endPosL = targetCrosshairDoer.CrosshairControl.GetCenter() + new Vector3(0f - offsetWidth2, 0f, 0f) + targetCrosshairDoer.leftCrosshairPixelOffset.ToVector3(0f) * targetCrosshairDoer.CrosshairControl.PixelsToUnits();
		Vector3 endPosR = targetCrosshairDoer.CrosshairControl.GetCenter() + new Vector3(offsetWidth2 + rightOffset, 0f, 0f) + targetCrosshairDoer.rightCrosshairPixelOffset.ToVector3(0f) * targetCrosshairDoer.CrosshairControl.PixelsToUnits();
		leftControl.GUIManager.AddControl(leftControl);
		rightControl.GUIManager.AddControl(rightControl);
		leftControl.BringToFront();
		rightControl.BringToFront();
		while (elapsed < duration)
		{
			elapsed += m_deltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			leftControl.transform.position = Vector3.Lerp(startPosL, endPosL, t);
			rightControl.transform.position = Vector3.Lerp(startPosR, endPosR, t);
			leftControl.BringToFront();
			rightControl.BringToFront();
			yield return null;
			if (!targetCrosshairDoer.m_control.HasFocus)
			{
				ClearExtantControls();
				m_suppressed = false;
				targetCrosshairDoer.m_suppressed = false;
				yield break;
			}
		}
		ClearExtantControls();
		targetCrosshairDoer.ClearExtantControls();
		targetCrosshairDoer.CreateOwnedControls();
		m_suppressed = false;
		targetCrosshairDoer.m_suppressed = false;
	}

	private void GotFocus(dfControl control, dfFocusEventArgs args)
	{
		AkSoundEngine.PostEvent("Play_UI_menu_select_01", base.gameObject);
		if (!m_suppressed)
		{
		}
	}

	private void LostFocus(dfControl control, dfFocusEventArgs args)
	{
		if (!m_suppressed)
		{
		}
	}

	private void UpdatedOwnedControls()
	{
		dfControl dfControl2 = m_extantControls[0];
		dfControl dfControl3 = m_extantControls[1];
		float num = CrosshairControl.Size.x / 2f + dfControl2.Size.x / 2f + 6f;
		num *= CrosshairControl.transform.lossyScale.x;
		num *= CrosshairControl.PixelsToUnits();
		float num2 = -3f * CrosshairControl.PixelsToUnits();
		dfControl2.transform.position = CrosshairControl.GetCenter() + new Vector3(num * -1f, 0f, 0f) + leftCrosshairPixelOffset.ToVector3(0f) * CrosshairControl.PixelsToUnits();
		dfControl3.transform.position = CrosshairControl.GetCenter() + new Vector3(num + num2, 0f, 0f) + rightCrosshairPixelOffset.ToVector3(0f) * CrosshairControl.PixelsToUnits();
	}

	private void CreateOwnedControls()
	{
		dfControl dfControl2 = CrosshairControl.AddPrefab(controlToPlace.gameObject);
		dfControl dfControl3 = CrosshairControl.AddPrefab(controlToPlace.gameObject);
		dfControl2.IsVisible = false;
		dfControl3.IsVisible = false;
		dfControl2.transform.localScale = Vector3.one;
		dfControl3.transform.localScale = Vector3.one;
		dfControl3.GetComponent<dfSpriteAnimation>().Direction = dfPlayDirection.Reverse;
		float num = CrosshairControl.Size.x / 2f + dfControl2.Size.x / 2f + 6f;
		num *= CrosshairControl.transform.lossyScale.x;
		num *= CrosshairControl.PixelsToUnits();
		float num2 = -3f * CrosshairControl.PixelsToUnits();
		dfControl2.transform.position = CrosshairControl.GetCenter() + new Vector3(num * -1f, 0f, 0f) + leftCrosshairPixelOffset.ToVector3(0f) * CrosshairControl.PixelsToUnits();
		dfControl3.transform.position = CrosshairControl.GetCenter() + new Vector3(num + num2, 0f, 0f) + rightCrosshairPixelOffset.ToVector3(0f) * CrosshairControl.PixelsToUnits();
		m_extantControls.Add(dfControl2);
		m_extantControls.Add(dfControl3);
	}

	private void ClearExtantControls()
	{
		if (m_extantControls.Count > 0)
		{
			for (int i = 0; i < m_extantControls.Count; i++)
			{
				UnityEngine.Object.Destroy(m_extantControls[i].gameObject);
			}
			m_extantControls.Clear();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
