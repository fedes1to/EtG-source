using System;
using System.Collections;
using Dungeonator;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalIntroSequenceManager : MonoBehaviour
{
	public tk2dSpriteAnimator PT1_DodgeRoll_Guy;

	public tk2dSpriteAnimator PT1_DodgeRoll_Logo;

	public tk2dSpriteAnimator PT2_Devolver_Logo;

	public Material FadeMaterial;

	public GameObject QuickStartObject;

	public tk2dTextMesh QuickStartKeyboard;

	public tk2dSprite QuickStartController;

	public tk2dTextMesh IntroTextMesh;

	public FinalIntroSequenceCard[] IntroCards;

	public bool IsDoingIntro;

	private bool m_inFoyer;

	private bool m_isDoingQuickStart;

	private bool m_skipCycle;

	private float customTextFadeInTime = -1f;

	private float customTextFadeOutTime = -1f;

	private string[] introKeys = new string[11]
	{
		"#INTRO_VIDEO_01", "#INTRO_VIDEO_02a", "#INTRO_VIDEO_02b", "#INTRO_VIDEO_03", "#INTRO_VIDEO_04a", "#INTRO_VIDEO_04b", "#INTRO_VIDEO_05", "#INTRO_VIDEO_06a", "#INTRO_VIDEO_06b", "#INTRO_VIDEO_07",
		"#INTRO_VIDEO_08"
	};

	private string m_cachedLastFirstString;

	private string[] m_lastAssignedStrings;

	public float FirstTextFadeInTime = 3f;

	public float FirstTextHoldTime = 7f;

	public float LastTextHoldTime = 7f;

	public float LastTextFadeOutTime = 3f;

	public float LastTextSecondStringTriggerTime = 5f;

	private bool m_skipLegend;

	private Vector3 m_currentIntroTextMeshLocalPosition;

	private void Awake()
	{
		if (Foyer.DoIntroSequence)
		{
			GameManager.Instance.IsSelectingCharacter = true;
			m_inFoyer = true;
			if (!m_inFoyer)
			{
				GameManager.PreventGameManagerExistence = true;
			}
			if (GameManager.Options == null)
			{
				GameOptions.Load();
			}
			Pixelator.DEBUG_LogSystemRenderingData();
		}
	}

	public void TriggerSequence()
	{
		if (Foyer.DoIntroSequence)
		{
			GameManager.Instance.StartCoroutine(CoreSequence());
			StartCoroutine(HandleBackgroundSkipChecks());
		}
	}

	private void OnDestroy()
	{
		GameManager.PreventGameManagerExistence = false;
	}

	private bool QuickStartAvailable()
	{
		if (GameStatsManager.Instance != null && GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.NUMBER_ATTEMPTS) >= 1f)
		{
			return true;
		}
		return false;
	}

	private IEnumerator MoveQuickstartOffscreen()
	{
		float elapsed = 0f;
		Vector3 startLocal = QuickStartObject.transform.localPosition;
		Vector3 offsetLocal = startLocal + new Vector3(0f, -3f, 0f);
		QuickStartController.HeightOffGround = 3f;
		while (elapsed < 0.5f)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / 0.5f;
			QuickStartObject.transform.localPosition = Vector3.Lerp(startLocal, offsetLocal, t);
			QuickStartController.UpdateZDepth();
			yield return null;
		}
	}

	private IEnumerator MoveQuickstartOnScreen()
	{
		float elapsed = 0f;
		Vector3 startLocal = QuickStartObject.transform.localPosition;
		Vector3 offsetLocal = startLocal + new Vector3(0f, 3f, 0f);
		QuickStartController.HeightOffGround = 3f;
		while (elapsed < 1f)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / 1f;
			QuickStartObject.transform.localPosition = Vector3.Lerp(startLocal, offsetLocal, t);
			QuickStartController.UpdateZDepth();
			yield return null;
		}
	}

	private IEnumerator Start()
	{
		if (!QuickStartAvailable() || !Foyer.DoIntroSequence)
		{
			QuickStartObject.SetActive(false);
		}
		else
		{
			QuickStartObject.SetActive(true);
			tk2dTextMesh componentInChildren = QuickStartObject.GetComponentInChildren<tk2dTextMesh>();
			componentInChildren.text = StringTableManager.GetString("#MAINMENU_NEW_QUICKSTART");
			StartCoroutine(MoveQuickstartOnScreen());
		}
		yield return null;
		if (Foyer.DoIntroSequence)
		{
			CameraController mainCameraController = GameManager.Instance.MainCameraController;
			mainCameraController.SetManualControl(true, false);
			mainCameraController.OverridePosition = base.transform.parent.position + new Vector3(16f, 3.5f, -5f);
			base.transform.parent.position += CameraController.PLATFORM_CAMERA_OFFSET;
			RenderSettings.ambientLight = Color.white;
		}
		if (QuickStartObject.activeSelf)
		{
			tk2dTextMesh componentInChildren2 = QuickStartObject.GetComponentInChildren<tk2dTextMesh>();
			componentInChildren2.text = StringTableManager.GetString("#MAINMENU_NEW_QUICKSTART");
		}
	}

	private IEnumerator HandleBackgroundSkipChecks()
	{
		yield return null;
		while (true)
		{
			if (QuickStartObject.activeSelf)
			{
				if (!BraveInput.PlayerlessInstance.IsKeyboardAndMouse())
				{
					QuickStartController.gameObject.SetActive(true);
					QuickStartController.renderer.enabled = true;
					QuickStartKeyboard.gameObject.SetActive(false);
				}
				else
				{
					QuickStartKeyboard.gameObject.SetActive(true);
					QuickStartController.gameObject.SetActive(false);
				}
			}
			if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0))
			{
				m_skipCycle = true;
			}
			if (!m_isDoingQuickStart && !m_skipCycle)
			{
				if (QuickStartAvailable() && (BraveInput.PlayerlessInstance.ActiveActions.Device.Action4.WasPressed || Input.GetKeyDown(KeyCode.Q)))
				{
					m_skipCycle = true;
					m_isDoingQuickStart = true;
					StartCoroutine(DoQuickStart());
				}
				if (BraveInput.PlayerlessInstance.ActiveActions.Device.Action1.WasPressed || BraveInput.PlayerlessInstance.ActiveActions.Device.Action2.WasPressed || BraveInput.PlayerlessInstance.ActiveActions.Device.Action3.WasPressed || BraveInput.PlayerlessInstance.ActiveActions.Device.CommandWasPressed || BraveInput.PlayerlessInstance.ActiveActions.MenuSelectAction.WasPressed)
				{
					m_skipCycle = true;
				}
			}
			yield return null;
		}
	}

	private IEnumerator SkippableWait(float duration)
	{
		float elapsed = 0f;
		while (elapsed < duration && !m_skipCycle)
		{
			elapsed += Time.deltaTime;
			yield return null;
		}
	}

	private IEnumerator CoreSequence()
	{
		IsDoingIntro = true;
		DebugTime.Log("FinalIntroSequenceManager.CoreSequence()");
		yield return new WaitForSeconds(0.5f);
		AkSoundEngine.PostEvent("Play_MUS_map_intro_01", GameManager.Instance.gameObject);
		if (!m_skipCycle)
		{
			yield return StartCoroutine(HandleDodgeRollLogo());
		}
		if (!m_skipCycle)
		{
			yield return StartCoroutine(SkippableWait(2.5f));
		}
		if (!m_skipCycle)
		{
			yield return StartCoroutine(FadeToBlack(0.5f));
		}
		if (!m_skipCycle)
		{
			yield return StartCoroutine(HandleDevolverLogo());
		}
		if (!m_skipCycle)
		{
			yield return StartCoroutine(SkippableWait(2.5f));
		}
		if (!m_skipCycle)
		{
			yield return StartCoroutine(FadeToBlack(0.5f));
		}
		if (m_skipCycle)
		{
			m_skipCycle = false;
			AkSoundEngine.PostEvent("Play_MUS_Intro_Beat_02", GameManager.Instance.gameObject);
			yield return StartCoroutine(FadeToBlack(0.5f, true));
			yield return StartCoroutine(SkippableWait(0.5f));
		}
		PT1_DodgeRoll_Guy.StopAndResetFrame();
		PT1_DodgeRoll_Logo.StopAndResetFrame();
		PT2_Devolver_Logo.StopAndResetFrame();
		PT1_DodgeRoll_Guy.renderer.enabled = false;
		PT1_DodgeRoll_Logo.renderer.enabled = false;
		PT2_Devolver_Logo.renderer.enabled = false;
		if (m_isDoingQuickStart)
		{
			yield break;
		}
		if (m_inFoyer)
		{
			TitleDioramaController tdc = UnityEngine.Object.FindObjectOfType<TitleDioramaController>();
			if ((bool)tdc && (bool)tdc.FadeQuad)
			{
				tdc.FadeQuad.enabled = false;
			}
			yield return StartCoroutine(MoveQuickstartOffscreen());
			RenderSettings.ambientLight = GameManager.Instance.Dungeon.decoSettings.ambientLightColor;
			yield return StartCoroutine(LegendCore());
			CameraController mainCameraController = GameManager.Instance.MainCameraController;
			mainCameraController.OnFinishedFrame = (Action)Delegate.Remove(mainCameraController.OnFinishedFrame, new Action(HandleOffsetUpdate));
			if (m_isDoingQuickStart)
			{
				yield break;
			}
			if ((bool)IntroTextMesh)
			{
			}
			AkSoundEngine.PostEvent("Stop_MUS_All", base.gameObject);
			IsDoingIntro = false;
			if ((bool)tdc)
			{
				while (!tdc.IsRevealed())
				{
					yield return null;
				}
				if ((bool)IntroTextMesh)
				{
					IntroTextMesh.gameObject.SetActive(false);
				}
			}
		}
		else
		{
			AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("tt_foyer");
			asyncOperation.allowSceneActivation = false;
			GameManager.PreventGameManagerExistence = false;
			asyncOperation.allowSceneActivation = true;
			IsDoingIntro = false;
		}
	}

	private void SetIntroString(bool fadePrevious, bool resetToCenter, params string[] keys)
	{
		StartCoroutine(SetIntroStringCR(fadePrevious, resetToCenter, -1f, keys));
	}

	private void SetIntroString(bool fadePrevious, bool resetToCenter, float customDura, params string[] keys)
	{
		StartCoroutine(SetIntroStringCR(fadePrevious, resetToCenter, customDura, keys));
	}

	private IEnumerator SetIntroStringCR(bool fadePrevious, bool resetToCenter, float customDura, params string[] keys)
	{
		float ela2 = 0f;
		float dura2 = ((!(customDura > 0f)) ? 1f : customDura);
		if (customTextFadeOutTime > 0f)
		{
			dura2 = customTextFadeOutTime;
		}
		if (fadePrevious)
		{
			while (ela2 < dura2)
			{
				ela2 += GameManager.INVARIANT_DELTA_TIME;
				Color textColor2 = Color.Lerp(t: 1f - ela2 / dura2, a: Color.black, b: Color.white);
				IntroTextMesh.color = textColor2;
				if (m_skipLegend)
				{
					yield break;
				}
				yield return null;
			}
			IntroTextMesh.text = string.Empty;
			yield return new WaitForSeconds(1f);
		}
		if (resetToCenter)
		{
			IntroTextMesh.transform.localPosition = new Vector3(0f, -0.8125f, 10f) + new Vector3(1f / 64f, 1f / 64f, 0f) + CameraController.PLATFORM_CAMERA_OFFSET;
			m_currentIntroTextMeshLocalPosition = IntroTextMesh.transform.localPosition;
			IntroTextMesh.LineSpacing = -0.25f;
		}
		IntroTextMesh.color = Color.white;
		ela2 = 0f;
		dura2 = 1f;
		if (customTextFadeInTime > 0f)
		{
			dura2 = customTextFadeInTime;
		}
		while (ela2 < dura2 && !m_skipLegend)
		{
			ela2 += GameManager.INVARIANT_DELTA_TIME;
			Color textColor = Color.Lerp(t: ela2 / dura2, a: Color.black, b: Color.white);
			string colorTag = "^C" + BraveUtility.ColorToHexWithAlpha(textColor);
			string introString = string.Empty;
			bool hasUsedColorTag = false;
			for (int i = 0; i < keys.Length; i++)
			{
				if (i > 0)
				{
					introString += "\n";
				}
				if (!hasUsedColorTag && (i == keys.Length - 1 || (keys[i + 1] == string.Empty && (keys.Length == 2 || (keys.Length == 3 && keys[2] == string.Empty)))))
				{
					hasUsedColorTag = true;
					introString += colorTag;
				}
				if (keys[i] != string.Empty)
				{
					introString += StringTableManager.GetIntroString(keys[i]);
				}
			}
			IntroTextMesh.text = introString;
			yield return null;
		}
	}

	private void UpdateText(float totalElapsed, float cardElapsed, int currentCardIndex, ref int currentIndex)
	{
		if (currentCardIndex < 0 || currentCardIndex >= IntroCards.Length)
		{
			return;
		}
		string[] targetKeys = IntroCards[currentCardIndex].GetTargetKeys(cardElapsed);
		bool flag = false;
		if (m_lastAssignedStrings == null || targetKeys.Length != m_lastAssignedStrings.Length)
		{
			flag = true;
		}
		else
		{
			for (int i = 0; i < targetKeys.Length; i++)
			{
				if (targetKeys[i] != m_lastAssignedStrings[i])
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			customTextFadeInTime = -1f;
			customTextFadeOutTime = -1f;
			bool fadePrevious = m_cachedLastFirstString != targetKeys[0];
			m_cachedLastFirstString = targetKeys[0];
			m_lastAssignedStrings = targetKeys;
			SetIntroString(fadePrevious, false, targetKeys);
		}
	}

	private IEnumerator LegendSkippableWait(float dura)
	{
		float ela = 0f;
		while (ela < dura)
		{
			ela += GameManager.INVARIANT_DELTA_TIME;
			UpdateSkipLegend();
			if (m_skipLegend)
			{
				customTextFadeInTime = -1f;
				customTextFadeOutTime = -1f;
				break;
			}
			yield return null;
		}
	}

	private IEnumerator ContinueMovingPreviousCard(FinalIntroSequenceCard previousCard)
	{
		Vector3 previousCardVelocity = previousCard.EndCameraTransform.position - previousCard.StartCameraTransform.position;
		previousCardVelocity = previousCardVelocity.normalized * (previousCardVelocity.magnitude / previousCard.PanTime);
		float ela = 0f;
		float dura = 1f;
		while (ela < dura)
		{
			ela += GameManager.INVARIANT_DELTA_TIME;
			Vector3 delta = previousCardVelocity * GameManager.INVARIANT_DELTA_TIME;
			previousCard.transform.position += delta * -1f;
			yield return null;
		}
	}

	private void UpdateSkipLegend()
	{
		if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.E))
		{
			m_skipLegend = true;
		}
		if (BraveInput.PlayerlessInstance.ActiveActions.IntroSkipActionPressed())
		{
			m_skipLegend = true;
		}
	}

	private void HandleOffsetUpdate()
	{
		IntroTextMesh.transform.parent = null;
		IntroTextMesh.transform.position = GameManager.Instance.MainCameraController.transform.position + m_currentIntroTextMeshLocalPosition;
	}

	private IEnumerator LegendCore()
	{
		if ((bool)FadeMaterial)
		{
			FadeMaterial.SetColor("_Color", new Color(0f, 0f, 0f, 0f));
		}
		CameraController cc = GameManager.Instance.MainCameraController;
		Camera c = cc.Camera;
		yield return StartCoroutine(LegendSkippableWait(2f));
		if (!m_skipLegend)
		{
			int cardIndex = 0;
			FinalIntroSequenceCard currentCard = IntroCards[0];
			cc.SetManualControl(true, false);
			cc.OverridePosition = currentCard.StartCameraTransform.position.XY();
			Pixelator.Instance.DoFinalNonFadedLayer = true;
			Pixelator.Instance.CompositePixelatedUnfadedLayer = true;
			IntroTextMesh.transform.parent = c.transform;
			IntroTextMesh.transform.localPosition = new Vector3(0f, -0.8125f, 10f) + new Vector3(1f / 64f, 1f / 64f, 0f) + CameraController.PLATFORM_CAMERA_OFFSET;
			m_currentIntroTextMeshLocalPosition = IntroTextMesh.transform.localPosition;
			IntroTextMesh.gameObject.SetLayerRecursively(LayerMask.NameToLayer("Unfaded"));
			cc.OnFinishedFrame = (Action)Delegate.Combine(cc.OnFinishedFrame, new Action(HandleOffsetUpdate));
			customTextFadeInTime = FirstTextFadeInTime;
			SetIntroString(false, false, introKeys[0], string.Empty);
			int currentStringIndex = 0;
			float additionalWaitTime = 1f;
			Pixelator.Instance.FadeToBlack(0.7f, true, FirstTextHoldTime + FirstTextFadeInTime + 1f + additionalWaitTime);
			Pixelator.Instance.SetVignettePower(2.25f);
			Pixelator.Instance.DoOcclusionLayer = false;
			yield return StartCoroutine(LegendSkippableWait(FirstTextFadeInTime + FirstTextHoldTime));
			if (!m_skipLegend)
			{
				customTextFadeInTime = currentCard.CustomTextFadeInTime;
				customTextFadeOutTime = currentCard.CustomTextFadeOutTime;
				SetIntroString(true, false, string.Empty);
				yield return StartCoroutine(LegendSkippableWait(additionalWaitTime));
				if (!m_skipLegend)
				{
					UpdateText(0f, 0f, cardIndex, ref currentStringIndex);
					IntroTextMesh.transform.localPosition = new Vector3(0f, -7f, 10f) + new Vector3(1f / 64f, 1f / 64f, 0f) + CameraController.PLATFORM_CAMERA_OFFSET;
					m_currentIntroTextMeshLocalPosition = IntroTextMesh.transform.localPosition;
					yield return StartCoroutine(LegendSkippableWait(1f));
					if (!m_skipLegend)
					{
						bool continueDoing = true;
						float ela = 0f - currentCard.StartHoldTime;
						float totalElapsed = 0f;
						for (int i = 0; i < IntroCards.Length; i++)
						{
							if (i != 0)
							{
								IntroCards[i].ToggleLighting(false);
							}
							IntroCards[i].SetVisibility(0f);
							if (i > 0)
							{
								IntroCards[i].transform.position = IntroCards[i - 1].EndCameraTransform.position - IntroCards[i].StartCameraTransform.localPosition;
								tk2dBaseSprite[] componentsInChildren = IntroCards[i].GetComponentsInChildren<tk2dBaseSprite>();
								for (int j = 0; j < componentsInChildren.Length; j++)
								{
									componentsInChildren[j].UpdateZDepth();
								}
							}
						}
						currentCard.SetVisibility(1f);
						FinalIntroSequenceCard previousCard = null;
						bool hasPrefaded = false;
						while (true)
						{
							if (continueDoing)
							{
								if (ela > currentCard.PanTime + currentCard.EndHoldTime)
								{
									previousCard = currentCard;
									cardIndex++;
									if (cardIndex >= IntroCards.Length)
									{
										goto IL_0a13;
									}
									hasPrefaded = false;
									currentCard = IntroCards[cardIndex];
									customTextFadeInTime = currentCard.CustomTextFadeInTime;
									customTextFadeOutTime = currentCard.CustomTextFadeOutTime;
									ela = 0f - currentCard.StartHoldTime;
									previousCard.ToggleLighting(false);
									currentCard.ToggleLighting(true);
									if (previousCard.EndHoldTime == 0f)
									{
										StartCoroutine(ContinueMovingPreviousCard(previousCard));
									}
									if (currentCard.StartHoldTime > 0f)
									{
										float tempEla = 0f;
										while (tempEla < 1f)
										{
											tempEla += GameManager.INVARIANT_DELTA_TIME;
											previousCard.SetVisibility(1f - tempEla);
											currentCard.SetVisibility(tempEla);
											UpdateSkipLegend();
											if (m_skipLegend)
											{
												goto end_IL_0a08;
											}
											yield return null;
										}
									}
								}
								else if (!hasPrefaded && ela > currentCard.PanTime + currentCard.EndHoldTime - 0.5f)
								{
									hasPrefaded = true;
									SetIntroString(true, false, 0.45f, string.Empty);
								}
								yield return null;
								if (cardIndex > 0 && currentCard.StartHoldTime == 0f && ela < 1f)
								{
									previousCard.SetVisibility(1f - (ela + GameManager.INVARIANT_DELTA_TIME));
									currentCard.SetVisibility(ela + GameManager.INVARIANT_DELTA_TIME);
								}
								ela += GameManager.INVARIANT_DELTA_TIME;
								totalElapsed += GameManager.INVARIANT_DELTA_TIME;
								UpdateText(totalElapsed, ela + currentCard.StartHoldTime, cardIndex, ref currentStringIndex);
								float t = Mathf.SmoothStep(0f, 1f, ela / currentCard.PanTime);
								if (currentCard.StartHoldTime == 0f && currentCard.EndHoldTime == 0f)
								{
									t = Mathf.Clamp01(ela / currentCard.PanTime);
								}
								else if (currentCard.StartHoldTime == 0f)
								{
									t = BraveMathCollege.LinearToSmoothStepInterpolate(0f, 1f, Mathf.Clamp01(ela / currentCard.PanTime));
								}
								else if (currentCard.EndHoldTime == 0f)
								{
									t = BraveMathCollege.SmoothStepToLinearStepInterpolate(0f, 1f, Mathf.Clamp01(ela / currentCard.PanTime));
								}
								cc.OverridePosition = Vector2.Lerp(currentCard.StartCameraTransform.position.XY(), currentCard.EndCameraTransform.position.XY(), t) + CameraController.PLATFORM_CAMERA_OFFSET.XY();
								UpdateSkipLegend();
								continueDoing = Foyer.DoIntroSequence && !m_skipLegend;
								continue;
							}
							goto IL_0a13;
							IL_0a13:
							if (m_skipLegend)
							{
								break;
							}
							if (!m_skipLegend && Foyer.DoIntroSequence)
							{
								Pixelator.Instance.FadeToBlack(1f);
								float finalEla = 0f;
								bool hasTriggeredFirstString = false;
								bool hasTriggeredSecondString = false;
								while (finalEla < LastTextHoldTime && !m_skipLegend)
								{
									finalEla += GameManager.INVARIANT_DELTA_TIME;
									totalElapsed += GameManager.INVARIANT_DELTA_TIME;
									if (!hasTriggeredFirstString)
									{
										hasTriggeredFirstString = true;
										SetIntroString(true, true, "#INTRO_VIDEO_07", string.Empty, string.Empty);
									}
									else if (!hasTriggeredSecondString && finalEla > LastTextSecondStringTriggerTime)
									{
										hasTriggeredSecondString = true;
										SetIntroString(false, false, "#INTRO_VIDEO_07", string.Empty, "#INTRO_VIDEO_08");
									}
									UpdateSkipLegend();
									yield return null;
								}
							}
							if (!m_skipLegend)
							{
								Pixelator.Instance.CacheCurrentFrameToBuffer = true;
								yield return null;
								TitleDioramaController tdc = UnityEngine.Object.FindObjectOfType<TitleDioramaController>();
								tdc.CacheFrameToFadeBuffer(cc.Camera);
							}
							customTextFadeInTime = -1f;
							customTextFadeOutTime = LastTextFadeOutTime;
							yield return null;
							break;
							continue;
							end_IL_0a08:
							break;
						}
					}
				}
			}
		}
		if (m_isDoingQuickStart)
		{
			yield break;
		}
		if (m_skipLegend)
		{
			yield return null;
			if (m_isDoingQuickStart)
			{
				yield break;
			}
			m_skipLegend = false;
			customTextFadeOutTime = 0.5f;
			SetIntroString(true, false, string.Empty);
			Pixelator.Instance.KillAllFades = true;
			yield return null;
			Pixelator.Instance.KillAllFades = false;
			Pixelator.Instance.FadeToBlack(0.25f);
			yield return new WaitForSeconds(0.25f);
			Pixelator.Instance.FadeToBlack(0.25f, true, 0.05f);
		}
		else
		{
			Pixelator.Instance.FadeToBlack(0.25f, true, 0.1f);
		}
		cc.OverrideZoomScale = 1f;
		cc.CurrentZoomScale = 1f;
		Pixelator.Instance.SetVignettePower(1f);
		Pixelator.Instance.DoOcclusionLayer = true;
	}

	private IEnumerator FadeToBlack(float duration, bool startAtCurrent = false, bool force = false)
	{
		float elapsed = 0f;
		float startValue = 0f;
		if (startAtCurrent)
		{
			startValue = FadeMaterial.GetColor("_Color").a;
		}
		while (elapsed < duration)
		{
			if (!force && m_skipCycle)
			{
				yield break;
			}
			elapsed += Time.deltaTime;
			float t = elapsed / duration;
			FadeMaterial.SetColor("_Color", new Color(0f, 0f, 0f, Mathf.Lerp(startValue, 1f, t)));
			yield return null;
		}
		FadeMaterial.SetColor("_Color", new Color(0f, 0f, 0f, 1f));
	}

	private IEnumerator HandleDevolverLogo()
	{
		FadeMaterial.SetColor("_Color", new Color(0f, 0f, 0f, 0f));
		PT1_DodgeRoll_Logo.sprite.renderer.enabled = false;
		PT1_DodgeRoll_Guy.sprite.renderer.enabled = false;
		PT2_Devolver_Logo.sprite.renderer.enabled = true;
		PT2_Devolver_Logo.AudioBaseObject = GameManager.Instance.gameObject;
		PT2_Devolver_Logo.Play();
		while (PT2_Devolver_Logo.IsPlaying(PT2_Devolver_Logo.CurrentClip) && !m_skipCycle)
		{
			yield return null;
		}
	}

	private IEnumerator HandleDodgeRollLogo()
	{
		FadeMaterial.SetColor("_Color", new Color(0f, 0f, 0f, 0f));
		PT1_DodgeRoll_Logo.sprite.renderer.enabled = false;
		PT1_DodgeRoll_Guy.sprite.renderer.enabled = true;
		PT2_Devolver_Logo.sprite.renderer.enabled = false;
		PT1_DodgeRoll_Guy.Play();
		while (PT1_DodgeRoll_Guy.CurrentFrame < 9)
		{
			if (m_skipCycle)
			{
				yield break;
			}
			yield return null;
		}
		if (!m_skipCycle)
		{
			PT1_DodgeRoll_Logo.sprite.renderer.enabled = true;
			PT1_DodgeRoll_Logo.AudioBaseObject = GameManager.Instance.gameObject;
			PT1_DodgeRoll_Logo.Play();
			while ((PT1_DodgeRoll_Logo.IsPlaying(PT1_DodgeRoll_Logo.CurrentClip) || PT1_DodgeRoll_Guy.IsPlaying(PT1_DodgeRoll_Guy.CurrentClip)) && !m_skipCycle)
			{
				yield return null;
			}
		}
	}

	private IEnumerator DoQuickStart()
	{
		QuickStartObject.SetActive(false);
		StartCoroutine(FadeToBlack(0.1f, true, true));
		GameManager.PreventGameManagerExistence = false;
		GameManager.SKIP_FOYER = true;
		Foyer.DoMainMenu = false;
		if (!m_inFoyer)
		{
			uint out_bankID = 1u;
			DebugTime.RecordStartTime();
			AkSoundEngine.LoadBank("SFX.bnk", -1, out out_bankID);
			DebugTime.Log("FinalIntroSequenceManager.DoQuickStart.LoadBank()");
			GameManager.EnsureExistence();
		}
		AkSoundEngine.PostEvent("Play_UI_menu_confirm_01", base.gameObject);
		MidGameSaveData saveToContinue = null;
		if (GameManager.VerifyAndLoadMidgameSave(out saveToContinue))
		{
			yield return null;
			Dungeon.ShouldAttemptToLoadFromMidgameSave = true;
			GameManager.Instance.SetNextLevelIndex(GameManager.Instance.GetTargetLevelIndexFromSavedTileset(saveToContinue.levelSaved));
			GameManager.Instance.GeneratePlayersFromMidGameSave(saveToContinue);
			if (!m_inFoyer)
			{
				GameManager.Instance.FlushAudio();
			}
			GameManager.Instance.IsFoyer = false;
			Foyer.DoIntroSequence = false;
			Foyer.DoMainMenu = false;
			GameManager.Instance.IsSelectingCharacter = false;
			GameManager.Instance.DelayedLoadMidgameSave(0.25f, saveToContinue);
			yield break;
		}
		GameManager.PlayerPrefabForNewGame = (GameObject)BraveResources.Load(CharacterSelectController.GetCharacterPathFromQuickStart());
		GameManager.Instance.GlobalInjectionData.PreprocessRun();
		yield return null;
		PlayerController playerController = GameManager.PlayerPrefabForNewGame.GetComponent<PlayerController>();
		GameStatsManager.Instance.BeginNewSession(playerController);
		GameObject instantiatedPlayer = UnityEngine.Object.Instantiate(GameManager.PlayerPrefabForNewGame, Vector3.zero, Quaternion.identity);
		GameManager.PlayerPrefabForNewGame = null;
		instantiatedPlayer.SetActive(true);
		PlayerController extantPlayer = instantiatedPlayer.GetComponent<PlayerController>();
		extantPlayer.PlayerIDX = 0;
		GameManager.Instance.PrimaryPlayer = extantPlayer;
		if (!m_inFoyer)
		{
			GameManager.Instance.FlushAudio();
		}
		GameManager.Instance.FlushMusicAudio();
		GameManager.Instance.SetNextLevelIndex(1);
		GameManager.Instance.IsSelectingCharacter = false;
		GameManager.Instance.IsFoyer = false;
		GameManager.Instance.DelayedLoadNextLevel(0.5f);
		yield return null;
		yield return null;
		yield return null;
		Foyer.Instance.OnDepartedFoyer();
	}
}
