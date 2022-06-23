using System.Collections;
using System.Collections.Generic;
using InControl;
using UnityEngine;

public class CreditsController : MonoBehaviour
{
	public dfScrollPanel creditsPanel;

	public List<int> scrollThresholds;

	public List<float> scrollDelays;

	public float maxScrollSpeed = 20f;

	private int m_currentThreshold;

	private void Start()
	{
		GameManager.Instance.ClearActiveGameData(false, false);
		Object.Destroy(GameManager.Instance.DungeonMusicController);
		StartCoroutine(ScrollToNextThreshold());
	}

	private IEnumerator ScrollToNextThreshold()
	{
		float elapsed = 0f;
		float startYScroll = creditsPanel.ScrollPosition.y;
		float duration = ((float)scrollThresholds[m_currentThreshold] - creditsPanel.ScrollPosition.y) / maxScrollSpeed;
		while (elapsed < duration)
		{
			InputDevice input = InputManager.ActiveDevice;
			if (input.AnyButton.WasPressed || Input.anyKeyDown)
			{
				GoToMainMenu();
				yield break;
			}
			creditsPanel.ScrollPosition = new Vector2(creditsPanel.ScrollPosition.x, BraveMathCollege.SmoothLerp(startYScroll, scrollThresholds[m_currentThreshold], Mathf.Clamp01(elapsed / duration)));
			creditsPanel.ScrollPosition = creditsPanel.ScrollPosition.Quantize(3f);
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		StartCoroutine(WaitForNextThreshold());
	}

	private IEnumerator WaitForNextThreshold()
	{
		float elapsed = 0f;
		while (elapsed < scrollDelays[m_currentThreshold])
		{
			InputDevice input = InputManager.ActiveDevice;
			if (input.AnyButton.WasPressed || Input.anyKeyDown)
			{
				GoToMainMenu();
				yield break;
			}
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		m_currentThreshold++;
		if (m_currentThreshold < scrollThresholds.Count)
		{
			StartCoroutine(ScrollToNextThreshold());
		}
		else
		{
			GoToMainMenu();
		}
	}

	private void GoToMainMenu()
	{
		Cursor.visible = true;
		GameManager.Instance.LoadCharacterSelect();
	}
}
