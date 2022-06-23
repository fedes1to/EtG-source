using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
[ExecuteInEditMode]
[AddComponentMenu("Daikon Forge/Examples/Menus/Radial Menu")]
public class dfRadialMenu : MonoBehaviour
{
	public delegate void CircularMenuEventHandler(dfRadialMenu sender);

	public float radius = 200f;

	public float startAngle;

	public float openAngle = 360f;

	public bool rotateButtons;

	public bool animateOpacity;

	public bool animateOpenAngle;

	public bool animateRadius;

	public bool autoToggle;

	public bool closeOnLostFocus;

	public float animationLength = 0.5f;

	public List<dfControl> excludedControls = new List<dfControl>();

	public dfControl host;

	private bool isAnimating;

	private bool isOpen;

	public bool IsOpen
	{
		get
		{
			return isOpen;
		}
		set
		{
			if (isOpen != value)
			{
				if (value)
				{
					Open();
				}
				else
				{
					Close();
				}
			}
		}
	}

	public event CircularMenuEventHandler BeforeMenuOpened;

	public event CircularMenuEventHandler MenuOpened;

	public event CircularMenuEventHandler MenuClosed;

	public void Open()
	{
		if (!isOpen && !isAnimating && base.enabled && base.gameObject.activeSelf)
		{
			StartCoroutine(openMenu());
		}
	}

	public void Close()
	{
		if (isOpen && !isAnimating && base.enabled && base.gameObject.activeSelf)
		{
			StartCoroutine(closeMenu());
			if (host.ContainsFocus)
			{
				dfGUIManager.SetFocus(null);
			}
		}
	}

	public void Toggle()
	{
		if (!isAnimating)
		{
			if (isOpen)
			{
				Close();
			}
			else
			{
				Open();
			}
		}
	}

	public void OnEnable()
	{
		if (host == null)
		{
			host = GetComponent<dfControl>();
		}
	}

	public void Start()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		using (dfList<dfControl> dfList2 = getButtons())
		{
			for (int i = 0; i < dfList2.Count; i++)
			{
				dfList2[i].Hide();
			}
		}
	}

	public void Update()
	{
		if (!Application.isPlaying)
		{
			arrangeButtons();
		}
	}

	public void OnLeaveFocus(dfControl sender, dfFocusEventArgs args)
	{
		if (closeOnLostFocus && !host.ContainsFocus && Application.isPlaying)
		{
			Close();
		}
	}

	public void OnClick(dfControl sender, dfMouseEventArgs args)
	{
		if (autoToggle || args.Source == host)
		{
			Toggle();
		}
	}

	private dfList<dfControl> getButtons()
	{
		return host.Controls.Where((dfControl x) => x.enabled && !excludedControls.Contains(x));
	}

	private void arrangeButtons()
	{
		arrangeButtons(startAngle, radius, openAngle, 1f);
	}

	private IEnumerator openMenu()
	{
		if (this.BeforeMenuOpened != null)
		{
			this.BeforeMenuOpened(this);
		}
		host.Signal("OnBeforeMenuOpened", this);
		isAnimating = true;
		if (animateOpacity || animateOpenAngle || animateRadius)
		{
			float time = Mathf.Max(0.1f, animationLength);
			dfAnimatedFloat animOpenAngle = new dfAnimatedFloat((!animateOpenAngle) ? openAngle : 0f, openAngle, time);
			dfAnimatedFloat animRadius = new dfAnimatedFloat((!animateRadius) ? radius : 0f, radius, time);
			dfAnimatedFloat animOpacity = new dfAnimatedFloat((!animateOpacity) ? 1 : 0, 1f, time);
			float endTime = Time.realtimeSinceStartup + time;
			while (Time.realtimeSinceStartup < endTime)
			{
				arrangeButtons(startAngle, animRadius, animOpenAngle, animOpacity);
				yield return null;
			}
		}
		arrangeButtons();
		isOpen = true;
		isAnimating = false;
		if (this.MenuOpened != null)
		{
			this.MenuOpened(this);
		}
		host.Signal("OnMenuOpened", this);
	}

	private IEnumerator closeMenu()
	{
		isAnimating = true;
		if (animateOpacity || animateOpenAngle || animateRadius)
		{
			float time = Mathf.Max(0.1f, animationLength);
			dfAnimatedFloat animOpenAngle = new dfAnimatedFloat(openAngle, (!animateOpenAngle) ? openAngle : 0f, time);
			dfAnimatedFloat animRadius = new dfAnimatedFloat(radius, (!animateRadius) ? radius : 0f, time);
			dfAnimatedFloat animOpacity = new dfAnimatedFloat(1f, (!animateOpacity) ? 1 : 0, time);
			float endTime = Time.realtimeSinceStartup + time;
			while (Time.realtimeSinceStartup < endTime)
			{
				arrangeButtons(startAngle, animRadius, animOpenAngle, animOpacity);
				yield return null;
			}
		}
		using (dfList<dfControl> dfList2 = getButtons())
		{
			for (int i = 0; i < dfList2.Count; i++)
			{
				dfList2[i].IsVisible = false;
			}
		}
		isOpen = false;
		isAnimating = false;
		if (this.MenuClosed != null)
		{
			this.MenuClosed(this);
		}
		host.Signal("OnMenuOpened", this);
	}

	private void arrangeButtons(float startAngle, float radius, float openAngle, float opacity)
	{
		float num = clampRotation(startAngle);
		Vector3 vector = (Vector3)host.Size * 0.5f;
		using (dfList<dfControl> dfList2 = getButtons())
		{
			if (dfList2.Count == 0)
			{
				return;
			}
			float num2 = Mathf.Sign(openAngle);
			float num3 = num2 * Mathf.Min(Mathf.Abs(clampRotation(openAngle)) / (float)(dfList2.Count - 1), 360f / (float)dfList2.Count);
			for (int i = 0; i < dfList2.Count; i++)
			{
				dfControl dfControl2 = dfList2[i];
				Quaternion quaternion = Quaternion.Euler(0f, 0f, num);
				Vector3 vector2 = vector + quaternion * Vector3.down * radius;
				dfControl2.RelativePosition = vector2 - (Vector3)dfControl2.Size * 0.5f;
				if (rotateButtons)
				{
					dfControl2.Pivot = dfPivotPoint.MiddleCenter;
					dfControl2.transform.localRotation = Quaternion.Euler(0f, 0f, 0f - num);
				}
				else
				{
					dfControl2.transform.localRotation = Quaternion.identity;
				}
				dfControl2.IsVisible = true;
				dfControl2.Opacity = opacity;
				num += num3;
			}
		}
	}

	private float clampRotation(float rotation)
	{
		return Mathf.Sign(rotation) * Mathf.Max(0.1f, Mathf.Min(360f, Mathf.Abs(rotation)));
	}
}
