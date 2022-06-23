using System.Collections;
using System.Collections.Generic;
using InControl;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroSequenceManager : MonoBehaviour
{
	public List<IntroSequenceElement> elements;

	public dfControl postVideoPanel;

	public string nextSceneName;

	private AsyncOperation async;

	private IEnumerator Start()
	{
		if (SceneManager.GetActiveScene().name == "Outro_Demo")
		{
			GameManager.Instance.ClearActiveGameData(false, false);
			Object.Destroy(GameManager.Instance.DungeonMusicController);
			AkSoundEngine.PostEvent("Stop_SND_All", base.gameObject);
			AkSoundEngine.ClearPreparedEvents();
			AkSoundEngine.StopAll();
		}
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(HandleElement(0));
		BraveCameraUtility.GenerateBackgroundCamera(Camera.main);
	}

	private void Update()
	{
		BraveCameraUtility.MaintainCameraAspect(Camera.main);
	}

	private IEnumerator HandleElement(int index)
	{
		yield return null;
		IntroSequenceElement element = elements[index];
		element.panel.IsVisible = true;
		if (index == 0)
		{
			IntroMovieClipPlayer component = element.panel.GetComponent<IntroMovieClipPlayer>();
			if (component != null)
			{
				component.TriggerMovie();
			}
		}
		if (index == 1 && SceneManager.GetActiveScene().name.Contains("Intro"))
		{
			GameManager.AttemptSoundEngineInitialization();
			AkSoundEngine.PostEvent("Play_MUS_Dungeon_state_loopA", base.gameObject);
			AkSoundEngine.PostEvent("Play_MUS_space_intro_01", base.gameObject);
		}
		float elapsed3 = 0f;
		element.panel.Opacity = 0f;
		for (int i = 0; i < element.additionalElements.Length; i++)
		{
			element.additionalElements[i].IsVisible = true;
			element.additionalElements[i].Opacity = 0f;
		}
		while (true)
		{
			if (elapsed3 < element.fadeInTime)
			{
				if (Input.anyKeyDown && index > 0)
				{
					if (!element.waitsForInput)
					{
						break;
					}
					yield return null;
					element.fadeOutTime = 0.5f;
				}
				else
				{
					if (((InputManager.ActiveDevice == null || !InputManager.ActiveDevice.LeftStickButton.IsPressed || !InputManager.ActiveDevice.RightStickButton.WasPressed) && (!InputManager.ActiveDevice.RightStickButton.IsPressed || !InputManager.ActiveDevice.LeftStickButton.WasPressed) && (!InputManager.ActiveDevice.RightStickButton.WasPressed || !InputManager.ActiveDevice.LeftStickButton.WasPressed)) || !element.waitsForInput)
					{
						element.panel.Opacity = elapsed3 / element.fadeInTime;
						for (int j = 0; j < element.additionalElements.Length; j++)
						{
							element.additionalElements[j].Opacity = elapsed3 / element.fadeInTime;
						}
						elapsed3 += BraveTime.DeltaTime;
						yield return null;
						continue;
					}
					yield return null;
					element.fadeOutTime = 0.5f;
				}
			}
			else
			{
				elapsed3 = 0f;
				element.panel.Opacity = 1f;
				float targetTime = ((!element.waitsForInput) ? element.hangTime : float.MaxValue);
				while (elapsed3 < targetTime)
				{
					if (!Input.anyKeyDown || index <= 0)
					{
						if (((InputManager.ActiveDevice != null && InputManager.ActiveDevice.LeftStickButton.IsPressed && InputManager.ActiveDevice.RightStickButton.WasPressed) || (InputManager.ActiveDevice.RightStickButton.IsPressed && InputManager.ActiveDevice.LeftStickButton.WasPressed) || (InputManager.ActiveDevice.RightStickButton.WasPressed && InputManager.ActiveDevice.LeftStickButton.WasPressed)) && element.waitsForInput)
						{
							yield return null;
							element.fadeOutTime = 0.5f;
							break;
						}
						elapsed3 += BraveTime.DeltaTime;
						yield return null;
						continue;
					}
					goto IL_03a8;
				}
			}
			goto IL_04ed;
			IL_04ed:
			elapsed3 = 0f;
			while (elapsed3 < element.fadeOutTime && (!Input.anyKeyDown || index <= 0))
			{
				element.panel.Opacity = 1f - elapsed3 / element.fadeOutTime;
				for (int k = 0; k < element.additionalElements.Length; k++)
				{
					if (!(element.additionalElements[k] == postVideoPanel))
					{
						element.additionalElements[k].Opacity = 1f - elapsed3 / element.fadeOutTime;
					}
				}
				elapsed3 += BraveTime.DeltaTime;
				yield return null;
			}
			break;
			IL_03a8:
			if (element.waitsForInput)
			{
				yield return null;
				element.fadeOutTime = 0.5f;
				goto IL_04ed;
			}
			break;
		}
		element.panel.IsVisible = false;
		for (int l = 0; l < element.additionalElements.Length; l++)
		{
			if (!(element.additionalElements[l] == postVideoPanel))
			{
				element.additionalElements[l].IsVisible = false;
			}
		}
		if (elements.Count > index + 1)
		{
			StartCoroutine(HandleElement(index + 1));
		}
		else if (!string.IsNullOrEmpty(nextSceneName))
		{
			Cursor.visible = true;
			if (nextSceneName == "MainMenu" && GameManager.Instance != null)
			{
				GameManager.Instance.LoadMainMenu();
				yield break;
			}
			AkSoundEngine.PostEvent("Stop_SND_All", base.gameObject);
			AkSoundEngine.ClearPreparedEvents();
			AkSoundEngine.StopAll();
			SceneManager.LoadScene(nextSceneName);
		}
	}
}
