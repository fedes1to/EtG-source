using System.Collections;
using System.Diagnostics;
using InControl;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
	public dfButton NewGameButton;

	public dfButton CoopGameButton;

	public dfButton NewGameDebugModeButton;

	public dfButton ControlsButton;

	public dfButton PlayVideoButton;

	public dfButton QuitGameButton;

	public dfSprite TEMP_ControlsPrefab;

	public dfSprite TEMP_ControlsSonyPrefab;

	public Image FadeImage;

	public RawImage SizzleImage;

	public AudioClip movieAudio;

	private GameObject m_extantControlsPanel;

	private TempControlsController m_controlsPanelController;

	private void Start()
	{
		GameManager.Instance.TargetQuickRestartLevel = -1;
		PhysicsEngine.Instance = null;
		Pixelator.Instance = null;
		GameUIRoot.Instance = null;
		SpawnManager.Instance = null;
		Minimap.Instance = null;
		NewGameButton.Click += OnNewGameSelected;
		CoopGameButton.Click += OnNewCoopGameSelected;
		ControlsButton.Click += ShowControlsPanel;
		if (PlayVideoButton != null)
		{
			PlayVideoButton.Click += delegate
			{
				PlayWindowsMediaPlayerMovie();
			};
		}
		QuitGameButton.Click += Quit;
		if (Time.timeScale != 1f)
		{
			BraveTime.ClearAllMultipliers();
		}
	}

	private void OnNewCoopGameSelected(dfControl control, dfMouseEventArgs mouseEvent)
	{
		DoQuickStart();
	}

	private void OnStageModeSelected(dfControl control, dfMouseEventArgs mouseEvent)
	{
		GameManager.Instance.CurrentGameType = GameManager.GameType.COOP_2_PLAYER;
		NewGameInternal();
	}

	private void OnStageModeBackupSelected(dfControl control, dfMouseEventArgs mouseEvent)
	{
		GameManager.Instance.CurrentGameType = GameManager.GameType.COOP_2_PLAYER;
		NewGameInternal();
	}

	private void DoQuickStart()
	{
		GameManager.SKIP_FOYER = true;
		GameManager.Instance.ClearPerLevelData();
		GameManager.Instance.ClearPlayers();
		uint out_bankID = 1u;
		AkSoundEngine.LoadBank("SFX.bnk", -1, out out_bankID);
		GameManager.PlayerPrefabForNewGame = (GameObject)BraveResources.Load(CharacterSelectController.GetCharacterPathFromQuickStart());
		PlayerController component = GameManager.PlayerPrefabForNewGame.GetComponent<PlayerController>();
		GameStatsManager.Instance.BeginNewSession(component);
		StartCoroutine(LerpFadeAlpha(0f, 1f, 0.15f));
		GameManager.Instance.FlushAudio();
		GameManager.Instance.GlobalInjectionData.PreprocessRun();
		GameManager.Instance.DelayedLoadNextLevel(0.15f);
		AkSoundEngine.PostEvent("Play_UI_menu_confirm_01", base.gameObject);
	}

	private void NewGameInternal()
	{
		StartCoroutine(LerpFadeAlpha(0f, 1f, 0.15f));
		GameManager.Instance.DelayedLoadNextLevel(0.15f);
		AkSoundEngine.PostEvent("Play_UI_menu_confirm_01", base.gameObject);
	}

	private void OnNewGameSelected(dfControl control, dfMouseEventArgs mouseEvent)
	{
		GameManager.Instance.CurrentGameType = GameManager.GameType.SINGLE_PLAYER;
		NewGameInternal();
	}

	private IEnumerator LerpFadeAlpha(float startAlpha, float targetAlpha, float duration)
	{
		float elapsed = 0f;
		Color startColor = new Color(0f, 0f, 0f, startAlpha);
		Color endColor = new Color(0f, 0f, 0f, targetAlpha);
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t = elapsed / duration;
			FadeImage.color = Color.Lerp(startColor, endColor, t);
			yield return null;
		}
	}

	private void Update()
	{
		if ((InputManager.ActiveDevice != null && InputManager.ActiveDevice.Action4.WasPressed) || Input.GetKeyDown(KeyCode.Q))
		{
			DoQuickStart();
		}
		if (InputManager.ActiveDevice != null && InputManager.ActiveDevice.LeftStickDown.IsPressed && InputManager.ActiveDevice.RightStickDown.WasPressed)
		{
			OnNewCoopGameSelected(null, null);
		}
		if (Input.anyKeyDown && m_controlsPanelController != null && m_controlsPanelController.CanClose && !Input.GetMouseButtonDown(0))
		{
			HideControlsPanel();
		}
	}

	private void Quit(dfControl control, dfMouseEventArgs eventArg)
	{
		Application.Quit();
	}

	private void PlayWindowsMediaPlayerMovie()
	{
		string text = Application.streamingAssetsPath + "/SonyVidya.mp4";
		ProcessStartInfo startInfo = new ProcessStartInfo("wmplayer.exe", "\"" + text + "\"");
		Process.Start(startInfo);
	}

	private void ShowControlsPanel(dfControl control, dfMouseEventArgs eventArg)
	{
		if (!(m_extantControlsPanel != null))
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
