using System.Collections;
using UnityEngine;

public class PreOptionsMenuController : MonoBehaviour
{
	public dfButton TabAudioSelector;

	public dfButton TabVideoSelector;

	public dfButton TabGameplaySelector;

	public dfButton TabControlsSelector;

	public dfLabel HeaderLabel;

	public FullOptionsMenuController FullOptionsMenu;

	private dfPanel m_panel;

	private float m_timeOpen;

	public bool IsVisible
	{
		get
		{
			return m_panel.IsVisible;
		}
		set
		{
			if (m_panel.IsVisible == value)
			{
				return;
			}
			if (value)
			{
				m_panel.IsVisible = value;
				ShwoopOpen();
				ShowPreOptionsMenu();
				return;
			}
			m_timeOpen = 0f;
			ShwoopClosed();
			if (dfGUIManager.GetModalControl() == m_panel)
			{
				dfGUIManager.PopModal();
			}
			else
			{
				Debug.LogError("failure.");
			}
		}
	}

	public void MakeVisibleWithoutAnim()
	{
		if (!m_panel.IsVisible)
		{
			m_panel.IsVisible = true;
			if (!HeaderLabel.Text.StartsWith("#"))
			{
				HeaderLabel.ModifyLocalizedText(HeaderLabel.Text.ToUpperInvariant());
			}
			m_panel.Opacity = 1f;
			m_panel.transform.localScale = Vector3.one;
			m_panel.MakePixelPerfect();
			ShowPreOptionsMenu();
		}
	}

	private void ShowPreOptionsMenu()
	{
		dfGUIManager.PushModal(m_panel);
		TabGameplaySelector.Focus();
	}

	public void ReturnToPreOptionsMenu()
	{
		FullOptionsMenu.IsVisible = false;
		IsVisible = true;
		TabGameplaySelector.Focus();
		AkSoundEngine.PostEvent("Play_UI_menu_back_01", base.gameObject);
		dfGUIManager.PopModalToControl(m_panel, false);
	}

	public void ToggleToPanel(dfScrollPanel targetPanel, bool val, bool force = false)
	{
		if (force || !(m_timeOpen < 0.2f))
		{
			FullOptionsMenu.ToggleToPanel(targetPanel, val);
			m_panel.IsVisible = false;
		}
	}

	private void Awake()
	{
		m_panel = GetComponent<dfPanel>();
		TabAudioSelector.Click += delegate
		{
			ToggleToPanel(FullOptionsMenu.TabAudio, false);
		};
		TabVideoSelector.Click += delegate
		{
			ToggleToPanel(FullOptionsMenu.TabVideo, false);
		};
		TabGameplaySelector.Click += delegate
		{
			ToggleToPanel(FullOptionsMenu.TabGameplay, false);
		};
		TabControlsSelector.Click += delegate
		{
			ToggleToPanel(FullOptionsMenu.TabControls, false);
		};
	}

	private void Update()
	{
		if (IsVisible)
		{
			m_timeOpen += GameManager.INVARIANT_DELTA_TIME;
		}
		else
		{
			m_timeOpen = 0f;
		}
	}

	public void ShwoopOpen()
	{
		if (!HeaderLabel.Text.StartsWith("#"))
		{
			HeaderLabel.ModifyLocalizedText(HeaderLabel.Text.ToUpperInvariant());
		}
		StartCoroutine(HandleShwoop(false));
	}

	private IEnumerator HandleShwoop(bool reverse)
	{
		float timer = 0.1f;
		float elapsed = 0f;
		Vector3 smallScale = new Vector3(0.01f, 0.01f, 1f);
		Vector3 bigScale = Vector3.one;
		PauseMenuController pmc = GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>();
		while (elapsed < timer)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t = elapsed / timer;
			AnimationCurve targetCurve = ((!reverse) ? pmc.ShwoopInCurve : pmc.ShwoopOutCurve);
			m_panel.Opacity = Mathf.Lerp(0f, 1f, (!reverse) ? (t * 2f) : (1f - t * 2f));
			m_panel.transform.localScale = smallScale + bigScale * Mathf.Clamp01(targetCurve.Evaluate(t));
			yield return null;
		}
		if (!reverse)
		{
			m_panel.transform.localScale = Vector3.one;
			m_panel.MakePixelPerfect();
		}
		if (reverse)
		{
			m_panel.IsVisible = false;
		}
	}

	public void ShwoopClosed()
	{
		StartCoroutine(HandleShwoop(true));
	}
}
