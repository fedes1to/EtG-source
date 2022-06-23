using System;
using UnityEngine;

public class SpeedrunTimer : MonoBehaviour
{
	public tk2dTextMesh tk2dTarget;

	private Renderer tk2dRenderer;

	private dfLabel m_label;

	private int m_lastPlayedSeconds;

	private char[] m_formattedTimeSpan = new char[11];

	private void Start()
	{
		m_label = GetComponent<dfLabel>();
		m_lastPlayedSeconds = 0;
		if ((bool)tk2dTarget)
		{
			tk2dRenderer = tk2dTarget.GetComponent<Renderer>();
		}
	}

	private void Update()
	{
		if ((bool)tk2dTarget)
		{
			if (!tk2dRenderer.enabled && GameManager.Options.SpeedrunMode)
			{
				m_label.Parent.IsVisible = true;
				m_label.IsVisible = false;
				tk2dRenderer.enabled = true;
			}
			if (tk2dRenderer.enabled && !GameManager.Options.SpeedrunMode)
			{
				m_label.Parent.IsVisible = false;
				m_label.IsVisible = false;
				tk2dRenderer.enabled = false;
			}
			if (!GameManager.HasInstance || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
			{
				m_label.Parent.IsVisible = false;
				m_label.IsVisible = false;
				tk2dRenderer.enabled = false;
			}
			if (!tk2dRenderer.enabled)
			{
				return;
			}
		}
		else
		{
			if (!m_label.Parent.IsVisible && GameManager.Options.SpeedrunMode)
			{
				m_label.Parent.IsVisible = true;
			}
			if (m_label.Parent.IsVisible && !GameManager.Options.SpeedrunMode)
			{
				m_label.Parent.IsVisible = false;
			}
			if (!m_label.IsVisible)
			{
				return;
			}
		}
		m_label.Parent.Parent.RelativePosition = m_label.Parent.Parent.RelativePosition.WithY(GameUIRoot.Instance.p_playerCoinLabel.Parent.Parent.RelativePosition.y + GameUIRoot.Instance.p_playerCoinLabel.Parent.Parent.Height + 3f);
		float sessionStatValue = GameStatsManager.Instance.GetSessionStatValue(TrackedStats.TIME_PLAYED);
		int num = Mathf.FloorToInt(sessionStatValue);
		if ((bool)tk2dTarget)
		{
			int num2 = num / 3600;
			int num3 = num / 60 % 60;
			int num4 = num % 60;
			int num5 = Mathf.FloorToInt(1000f * (sessionStatValue % 1f));
			int num6 = 48;
			m_formattedTimeSpan[0] = (char)(num6 + num2 % 10);
			m_formattedTimeSpan[1] = ':';
			m_formattedTimeSpan[2] = (char)(num6 + num3 / 10 % 10);
			m_formattedTimeSpan[3] = (char)(num6 + num3 % 10);
			m_formattedTimeSpan[4] = ':';
			m_formattedTimeSpan[5] = (char)(num6 + num4 / 10 % 10);
			m_formattedTimeSpan[6] = (char)(num6 + num4 % 10);
			m_formattedTimeSpan[7] = '.';
			m_formattedTimeSpan[8] = (char)(num6 + num5 / 100 % 10);
			m_formattedTimeSpan[9] = (char)(num6 + num5 / 10 % 10);
			m_formattedTimeSpan[10] = (char)(num6 + num5 % 10);
			tk2dTarget.text = new string(m_formattedTimeSpan);
			float num7 = m_label.PixelsToUnits();
			tk2dTarget.scale = new Vector3(num7, num7, num7) * 16f * 3f;
		}
		else if (!GameManager.Options.DisplaySpeedrunCentiseconds)
		{
			if (num != m_lastPlayedSeconds || num <= 0)
			{
				m_lastPlayedSeconds = num;
				TimeSpan timeSpan = new TimeSpan(0, 0, 0, num);
				string text = string.Format("{0:0}:{1:00}:{2:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
				m_label.Text = text;
			}
		}
		else
		{
			int milliseconds = Mathf.FloorToInt(1000f * (GameStatsManager.Instance.GetSessionStatValue(TrackedStats.TIME_PLAYED) % 1f));
			TimeSpan timeSpan2 = new TimeSpan(0, 0, 0, num, milliseconds);
			string text2 = string.Format("{0:0}:{1:00}:{2:00}.{3:00}", timeSpan2.Hours, timeSpan2.Minutes, timeSpan2.Seconds, timeSpan2.Milliseconds / 10);
			m_label.Text = text2;
		}
	}
}
