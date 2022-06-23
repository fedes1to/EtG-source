using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using InControl;
using UnityEngine;
using UnityEngine.Serialization;

public class MainMenuFoyerController : MonoBehaviour
{
	public dfButton NewGameButton;

	public dfButton ControlsButton;

	public dfButton XboxLiveButton;

	public dfButton QuitGameButton;

	public dfButton ContinueGameButton;

	[FormerlySerializedAs("BetaLabel")]
	public dfLabel VersionLabel;

	public dfControl TitleCard;

	public dfSprite TEMP_ControlsPrefab;

	public dfSprite TEMP_ControlsSonyPrefab;

	private GameObject m_extantControlsPanel;

	private TempControlsController m_controlsPanelController;

	private dfGUIManager m_guiManager;

	private bool Initialized;

	private TitleDioramaController m_tdc;

	private float m_timeWithoutInput;

	private Vector3 m_cachedMousePosition;

	private bool m_faded;

	private bool m_wasFadedThisFrame;

	private float c_fadeTimer = 20f;

	private bool m_optionsOpen;

	private float m_cachedDepth = -1f;

	private void Awake()
	{
		m_guiManager = GetComponent<dfGUIManager>();
		NewGameButton.forceUpperCase = true;
		ControlsButton.forceUpperCase = true;
		XboxLiveButton.forceUpperCase = true;
		QuitGameButton.forceUpperCase = true;
		ContinueGameButton.forceUpperCase = true;
		NewGameButton.ModifyLocalizedText(NewGameButton.Text.ToUpperInvariant());
		ControlsButton.ModifyLocalizedText(ControlsButton.Text.ToUpperInvariant());
		XboxLiveButton.ModifyLocalizedText(XboxLiveButton.Text.ToUpperInvariant());
		QuitGameButton.ModifyLocalizedText(QuitGameButton.Text.ToUpperInvariant());
		List<dfButton> list = new List<dfButton>();
		list.Add(ContinueGameButton);
		if (GameManager.HasValidMidgameSave())
		{
			ContinueGameButton.IsEnabled = true;
			ContinueGameButton.IsVisible = true;
		}
		else
		{
			ContinueGameButton.IsEnabled = false;
			ContinueGameButton.IsVisible = false;
		}
		list.Add(NewGameButton);
		list.Add(ControlsButton);
		XboxLiveButton.IsEnabled = false;
		XboxLiveButton.IsVisible = false;
		list.Add(QuitGameButton);
		int count = list.Count;
		if (count > 0)
		{
			dfButton dfButton2 = list[count - 1];
			for (int i = 0; i < list.Count; i++)
			{
				dfButton dfButton3 = list[i];
				dfButton3.GetComponent<UIKeyControls>().up = dfButton2;
				dfButton2.GetComponent<UIKeyControls>().down = dfButton3;
				dfButton2 = dfButton3;
			}
		}
		FixButtonPositions();
		if (!Foyer.DoMainMenu)
		{
			NewGameButton.GUIManager.RenderCamera.enabled = false;
			AkSoundEngine.PostEvent("Play_MUS_State_Reset", base.gameObject);
		}
		VersionLabel.Text = VersionManager.DisplayVersionNumber;
	}

	private void FixButtonPositions()
	{
		NewGameButton.RelativePosition = NewGameButton.RelativePosition.WithX(QuitGameButton.RelativePosition.x).WithY(QuitGameButton.RelativePosition.y - 153f);
		ControlsButton.RelativePosition = ControlsButton.RelativePosition.WithX(QuitGameButton.RelativePosition.x).WithY(QuitGameButton.RelativePosition.y - 102f);
		XboxLiveButton.RelativePosition = XboxLiveButton.RelativePosition.WithX(QuitGameButton.RelativePosition.x).WithY(QuitGameButton.RelativePosition.y - 51f);
		ContinueGameButton.RelativePosition = ContinueGameButton.RelativePosition.WithX(QuitGameButton.RelativePosition.x).WithY(QuitGameButton.RelativePosition.y - 204f);
		if (!XboxLiveButton.IsEnabled)
		{
			ContinueGameButton.RelativePosition += new Vector3(0f, 51f, 0f);
			NewGameButton.RelativePosition += new Vector3(0f, 51f, 0f);
			ControlsButton.RelativePosition += new Vector3(0f, 51f, 0f);
		}
	}

	public void InitializeMainMenu()
	{
		GameManager.Instance.TargetQuickRestartLevel = -1;
		GameUIRoot.Instance.Manager.RenderCamera.enabled = false;
		FixButtonPositions();
		if (!Initialized)
		{
			NewGameButton.GotFocus += PlayFocusNoise;
			ControlsButton.GotFocus += PlayFocusNoise;
			XboxLiveButton.GotFocus += PlayFocusNoise;
			QuitGameButton.GotFocus += PlayFocusNoise;
			ContinueGameButton.GotFocus += PlayFocusNoise;
			NewGameButton.Click += OnNewGameSelected;
			ContinueGameButton.Click += OnContinueGameSelected;
			if (GameManager.HasValidMidgameSave())
			{
				ContinueGameButton.Focus();
			}
			ControlsButton.Click += ShowOptionsPanel;
			XboxLiveButton.Click += SignInToPlatform;
			QuitGameButton.Click += Quit;
			Initialized = true;
		}
		if (Time.timeScale != 1f)
		{
			BraveTime.ClearAllMultipliers();
		}
	}

	private void PlayFocusNoise(dfControl control, dfFocusEventArgs args)
	{
		if (Foyer.DoMainMenu)
		{
			AkSoundEngine.PostEvent("Play_UI_menu_select_01", GameManager.Instance.gameObject);
		}
	}

	public void UpdateMainMenuText()
	{
		if (GameManager.HasValidMidgameSave())
		{
			ContinueGameButton.IsEnabled = true;
			ContinueGameButton.IsVisible = true;
		}
		else
		{
			ContinueGameButton.IsEnabled = false;
			ContinueGameButton.IsVisible = false;
		}
	}

	public void DisableMainMenu()
	{
		BraveCameraUtility.OverrideAspect = null;
		GameUIRoot.Instance.Manager.RenderCamera.enabled = true;
		NewGameButton.GUIManager.RenderCamera.enabled = false;
		NewGameButton.GUIManager.enabled = false;
		NewGameButton.Click -= OnNewGameSelected;
		ControlsButton.Click -= ShowOptionsPanel;
		XboxLiveButton.Click -= SignInToPlatform;
		QuitGameButton.Click -= Quit;
		NewGameButton.IsInteractive = false;
		ControlsButton.IsInteractive = false;
		XboxLiveButton.IsInteractive = false;
		QuitGameButton.IsInteractive = false;
		if ((bool)NewGameButton && (bool)NewGameButton.GetComponent<UIKeyControls>())
		{
			NewGameButton.GetComponent<UIKeyControls>().enabled = false;
		}
		if ((bool)ControlsButton && (bool)ControlsButton.GetComponent<UIKeyControls>())
		{
			ControlsButton.GetComponent<UIKeyControls>().enabled = false;
		}
		if ((bool)XboxLiveButton && (bool)XboxLiveButton.GetComponent<UIKeyControls>())
		{
			XboxLiveButton.GetComponent<UIKeyControls>().enabled = false;
		}
		if ((bool)QuitGameButton && (bool)QuitGameButton.GetComponent<UIKeyControls>())
		{
			QuitGameButton.GetComponent<UIKeyControls>().enabled = false;
		}
		ShadowSystem.ForceAllLightsUpdate();
	}

	private void NewGameInternal()
	{
		DisableMainMenu();
		Pixelator.Instance.FadeToBlack(0.15f, true, 0.05f);
		GameManager.Instance.FlushAudio();
		Foyer.DoIntroSequence = false;
		Foyer.DoMainMenu = false;
		AkSoundEngine.PostEvent("Play_UI_menu_confirm_01", base.gameObject);
	}

	private bool IsDioramaRevealed(bool doReveal = false)
	{
		if (m_tdc == null)
		{
			m_tdc = Object.FindObjectOfType<TitleDioramaController>();
		}
		if ((bool)m_tdc)
		{
			return m_tdc.IsRevealed(doReveal);
		}
		return true;
	}

	private void OnContinueGameSelected(dfControl control, dfMouseEventArgs mouseEvent)
	{
		MidGameSaveData.ContinuePressedDevice = InputManager.ActiveDevice;
		if (!m_faded && !m_wasFadedThisFrame && IsDioramaRevealed(true) && Foyer.DoMainMenu)
		{
			MidGameSaveData midgameSave = null;
			GameManager.VerifyAndLoadMidgameSave(out midgameSave);
			Dungeon.ShouldAttemptToLoadFromMidgameSave = true;
			DisableMainMenu();
			Pixelator.Instance.FadeToBlack(0.15f, false, 0.05f);
			GameManager.Instance.FlushAudio();
			AkSoundEngine.PostEvent("Play_UI_menu_confirm_01", base.gameObject);
			GameManager.Instance.SetNextLevelIndex(GameManager.Instance.GetTargetLevelIndexFromSavedTileset(midgameSave.levelSaved));
			GameManager.Instance.GeneratePlayersFromMidGameSave(midgameSave);
			GameManager.Instance.IsFoyer = false;
			Foyer.DoIntroSequence = false;
			Foyer.DoMainMenu = false;
			GameManager.Instance.IsSelectingCharacter = false;
			GameManager.Instance.DelayedLoadMidgameSave(0.25f, midgameSave);
		}
	}

	private void OnNewGameSelected(dfControl control, dfMouseEventArgs mouseEvent)
	{
		if (!m_faded && !m_wasFadedThisFrame && IsDioramaRevealed(true))
		{
			GameManager.Instance.CurrentGameType = GameManager.GameType.SINGLE_PLAYER;
			NewGameInternal();
			GameManager.Instance.InjectedFlowPath = null;
		}
	}

	private IEnumerator ToggleFade(bool targetFade)
	{
		m_faded = targetFade;
		float ela = 0f;
		float dura = 1f;
		float startVal = ((!targetFade) ? 0f : 1f);
		float endVal = ((!targetFade) ? 1f : 0f);
		while (ela < dura)
		{
			ela += GameManager.INVARIANT_DELTA_TIME;
			float t2 = ela / dura;
			t2 = Mathf.Lerp(startVal, endVal, t2);
			NewGameButton.Opacity = t2;
			ControlsButton.Opacity = t2;
			XboxLiveButton.Opacity = t2;
			QuitGameButton.Opacity = t2;
			ContinueGameButton.Opacity = t2;
			VersionLabel.Opacity = t2;
			TitleCard.Opacity = t2;
			yield return null;
		}
	}

	private void Update()
	{
		m_wasFadedThisFrame = m_faded;
		if ((bool)m_guiManager && !GameManager.Instance.IsLoadingLevel)
		{
			m_guiManager.UIScale = Pixelator.Instance.ScaleTileScale / 3f;
		}
		if (!Foyer.DoMainMenu && !Foyer.DoIntroSequence && !GameManager.Instance.IsSelectingCharacter && !GameManager.IsReturningToBreach)
		{
			return;
		}
		if (IsDioramaRevealed())
		{
			m_timeWithoutInput += GameManager.INVARIANT_DELTA_TIME;
		}
		if (Input.anyKeyDown || Input.mousePosition != m_cachedMousePosition)
		{
			m_timeWithoutInput = 0f;
		}
		m_cachedMousePosition = Input.mousePosition;
		if ((bool)BraveInput.PlayerlessInstance && BraveInput.PlayerlessInstance.ActiveActions != null && BraveInput.PlayerlessInstance.ActiveActions.AnyActionPressed())
		{
			m_timeWithoutInput = 0f;
		}
		if (GameManager.Instance.PREVENT_MAIN_MENU_TEXT)
		{
			NewGameButton.Opacity = 0f;
			ControlsButton.Opacity = 0f;
			XboxLiveButton.Opacity = 0f;
			QuitGameButton.Opacity = 0f;
			VersionLabel.Opacity = 0f;
			TitleCard.Opacity = 0f;
		}
		else if (m_timeWithoutInput > c_fadeTimer && !m_faded)
		{
			StartCoroutine(ToggleFade(true));
		}
		else if (m_timeWithoutInput < c_fadeTimer && m_faded)
		{
			StartCoroutine(ToggleFade(false));
		}
		if (Foyer.DoMainMenu && !m_optionsOpen && (!IsDioramaRevealed() || (!NewGameButton.HasFocus && !ControlsButton.HasFocus && !XboxLiveButton.HasFocus && !QuitGameButton.HasFocus && !ContinueGameButton.HasFocus)))
		{
			dfGUIManager.PopModalToControl(null, false);
			if (ContinueGameButton.IsEnabled && ContinueGameButton.IsVisible)
			{
				ContinueGameButton.Focus();
			}
			else
			{
				NewGameButton.Focus();
			}
		}
		if (m_optionsOpen && !GameUIRoot.Instance.AreYouSurePanel.IsVisible)
		{
			if ((bool)BraveInput.PlayerlessInstance && BraveInput.PlayerlessInstance.ActiveActions != null && BraveInput.PlayerlessInstance.ActiveActions.CancelAction.WasPressed)
			{
				PauseMenuController component = GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>();
				if (!component.OptionsMenu.ModalKeyBindingDialog.IsVisible)
				{
					HideOptionsPanel();
				}
			}
			else if (Input.GetKeyDown(KeyCode.Escape))
			{
				HideOptionsPanel();
			}
		}
		if (Input.anyKeyDown && m_controlsPanelController != null && m_controlsPanelController.CanClose && !Input.GetMouseButtonDown(0))
		{
			HideControlsPanel();
		}
	}

	private void SignInToPlatform(dfControl control, dfMouseEventArgs eventArg)
	{
		GameManager.Instance.platformInterface.SignIn();
	}

	private void Quit(dfControl control, dfMouseEventArgs eventArg)
	{
		if (!m_faded && !m_wasFadedThisFrame && IsDioramaRevealed(true) && Foyer.DoMainMenu)
		{
			Application.Quit();
		}
	}

	private void ShowOptionsPanel(dfControl control, dfMouseEventArgs eventArg)
	{
		if (!m_faded && !m_wasFadedThisFrame && IsDioramaRevealed(true) && Foyer.DoMainMenu)
		{
			m_optionsOpen = true;
			m_cachedDepth = GameUIRoot.Instance.Manager.RenderCamera.depth;
			GameUIRoot.Instance.Manager.RenderCamera.depth += 10f;
			GameUIRoot.Instance.Manager.RenderCamera.enabled = true;
			GameUIRoot.Instance.Manager.overrideClearFlags = CameraClearFlags.Color;
			GameUIRoot.Instance.Manager.RenderCamera.backgroundColor = Color.black;
			PauseMenuController component = GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>();
			if (component != null)
			{
				component.OptionsMenu.PreOptionsMenu.IsVisible = true;
			}
		}
	}

	private void HideOptionsPanel()
	{
		PauseMenuController component = GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>();
		if (component != null && !GameUIRoot.Instance.AreYouSurePanel.IsVisible)
		{
			if (component.OptionsMenu.IsVisible)
			{
				component.OptionsMenu.CloseAndMaybeApplyChangesWithPrompt();
			}
			else
			{
				m_optionsOpen = false;
				GameUIRoot.Instance.Manager.RenderCamera.depth = m_cachedDepth;
				GameUIRoot.Instance.Manager.RenderCamera.enabled = false;
				GameUIRoot.Instance.Manager.overrideClearFlags = CameraClearFlags.Depth;
				if (component != null)
				{
					component.OptionsMenu.PreOptionsMenu.IsVisible = false;
				}
			}
		}
		BraveInput.SavePlayerlessBindingsToOptions();
	}

	private void ShowControlsPanel(dfControl control, dfMouseEventArgs eventArg)
	{
		if (Foyer.DoMainMenu && !(m_extantControlsPanel != null))
		{
			GameObject original = TEMP_ControlsPrefab.gameObject;
			if (!BraveInput.GetInstanceForPlayer(0).IsKeyboardAndMouse())
			{
				original = TEMP_ControlsSonyPrefab.gameObject;
			}
			GameObject gameObject = (m_extantControlsPanel = Object.Instantiate(original));
			m_controlsPanelController = gameObject.GetComponent<TempControlsController>();
			NewGameButton.GetManager().AddControl(gameObject.GetComponent<dfSprite>());
		}
	}

	private void HideControlsPanel()
	{
		if (m_extantControlsPanel != null)
		{
			m_controlsPanelController = null;
			Object.Destroy(m_extantControlsPanel);
		}
	}
}
