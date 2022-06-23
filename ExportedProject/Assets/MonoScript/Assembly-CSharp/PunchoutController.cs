using System.Collections;
using System.Collections.ObjectModel;
using System.Text;
using Dungeonator;
using InControl;
using UnityEngine;

public class PunchoutController : MonoBehaviour
{
	public enum TutorialControlState
	{
		Hidden,
		Shown,
		Completed
	}

	public static bool IsActive;

	public static bool OverrideControlsButton;

	public static bool InTutorial;

	public static TutorialControlState[] TutorialControls = new TutorialControlState[7];

	public static float TutorialUiUpdateTimer;

	public PunchoutPlayerController Player;

	public PunchoutAIActor Opponent;

	public tk2dSprite CoopCultist;

	public AIAnimator TimerAnimator;

	public tk2dTextMesh TimerTextMin1;

	public tk2dTextMesh TimerTextMin2;

	public tk2dTextMesh TimerColon;

	public tk2dTextMesh TimerTextSec1;

	public tk2dTextMesh TimerTextSec2;

	public dfGUIManager UiManager;

	public dfPanel UiPanel;

	public dfSprite PlayerHealthBarBase;

	public dfSprite RatHealthBarBase;

	public dfLabel ControlsLabel;

	public dfLabel TutorialLabel;

	[Header("Rewards")]
	public float NormalHitRewardChance = 1f;

	[PickupIdentifier]
	public int[] NormalHitRewards;

	public int MaxGlassGuonStones = 3;

	[Header("Post-Punchout Stuff")]
	public DungeonPlaceableBehaviour PlayerLostNotePrefab;

	public TalkDoerLite PlayerWonRatNPC;

	[Header("Constants")]
	public float TimerStartTime = 120f;

	private bool m_isFadingControlsUi;

	private Vector2 m_cameraCenterPos;

	private bool m_isInitialized;

	private bool m_tutorialSuperReady;

	public float Timer { get; set; }

	public float HideUiAmount { get; set; }

	public float HideControlsUiAmount { get; set; }

	public float HideTutorialUiAmount { get; set; }

	public bool ShouldDoTutorial
	{
		get
		{
			return !GameStatsManager.Instance.GetFlag(GungeonFlags.ITEMSPECIFIC_BOXING_GLOVE);
		}
	}

	public IEnumerator Start()
	{
		if (!m_isInitialized)
		{
			yield return null;
			yield return null;
			yield return null;
			yield return null;
			yield return null;
		}
		InitPunchout();
		SpriteOutlineManager.AddOutlineToSprite(CoopCultist, Color.black, 0.1f);
		Timer = TimerStartTime;
		IsActive = true;
	}

	public void Update()
	{
		UiManager.RenderCamera.enabled = !GameManager.Instance.IsPaused;
		Player.ManualUpdate();
		Opponent.ManualUpdate();
		if (HideControlsUiAmount <= 0f && !m_isFadingControlsUi && !(Opponent.state is PunchoutAIActor.IntroState))
		{
			StartCoroutine(ControlsUiFadeOutCR());
		}
		GameManager.Instance.MainCameraController.OverridePosition = m_cameraCenterPos + Player.CameraOffset + Opponent.CameraOffset;
		if (Opponent.state is PunchoutAIActor.IntroState)
		{
			Timer = TimerStartTime;
		}
		else if (!Opponent.IsDead && !(Opponent.state is PunchoutAIActor.WinState))
		{
			Timer = Mathf.Max(0f, Timer - BraveTime.DeltaTime);
		}
		UpdateTimer();
		if (InTutorial)
		{
			TutorialUiUpdateTimer -= BraveTime.DeltaTime;
			if (TutorialUiUpdateTimer < 0f)
			{
				UpdateTutorialText();
				TutorialUiUpdateTimer = 0.5f;
			}
			if (!m_tutorialSuperReady)
			{
				if (TutorialControls[5] == TutorialControlState.Completed)
				{
					m_tutorialSuperReady = true;
					Player.AddStar();
					TutorialUiUpdateTimer = 0f;
				}
			}
			else if (TutorialControls[6] == TutorialControlState.Completed && Player.state == null)
			{
				InTutorial = false;
				StartCoroutine(TutorialUiFadeCR());
			}
		}
		if (Timer <= 0f)
		{
			if (TimerAnimator.IsIdle())
			{
				TimerAnimator.PlayUntilCancelled("explode");
				TimerTextMin1.gameObject.SetActive(false);
				TimerTextMin2.gameObject.SetActive(false);
				TimerColon.gameObject.SetActive(false);
				TimerTextSec1.gameObject.SetActive(false);
				TimerTextSec2.gameObject.SetActive(false);
			}
			if (Opponent.state == null)
			{
				Opponent.state = new PunchoutAIActor.EscapeState();
				Player.Exhaust(4f);
			}
		}
		UpdateUI();
	}

	private void OnDestroy()
	{
		IsActive = false;
		OverrideControlsButton = false;
	}

	public void Init()
	{
		switch (GameManager.Instance.PrimaryPlayer.characterIdentity)
		{
		case PlayableCharacters.Convict:
			Player.SwapPlayer(0);
			break;
		case PlayableCharacters.Guide:
			Player.SwapPlayer(1);
			break;
		case PlayableCharacters.Soldier:
			Player.SwapPlayer(2);
			break;
		case PlayableCharacters.Pilot:
			Player.SwapPlayer(3);
			break;
		case PlayableCharacters.Bullet:
			Player.SwapPlayer(4);
			break;
		case PlayableCharacters.Robot:
			Player.SwapPlayer(5);
			break;
		case PlayableCharacters.Gunslinger:
			Player.SwapPlayer(6);
			break;
		case PlayableCharacters.Eevee:
			Player.SwapPlayer(7);
			break;
		default:
			Player.SwapPlayer(Random.Range(0, 8));
			break;
		}
		CoopCultist.gameObject.SetActive(GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER);
		StartCoroutine(UiFadeInCR());
		m_isInitialized = true;
	}

	public void Reset()
	{
		Timer = TimerStartTime;
		TimerAnimator.EndAnimation();
		TimerTextMin1.gameObject.SetActive(true);
		TimerTextMin2.gameObject.SetActive(true);
		TimerColon.gameObject.SetActive(true);
		TimerTextSec1.gameObject.SetActive(true);
		TimerTextSec2.gameObject.SetActive(true);
		Player.SwapPlayer(Random.Range(0, 8));
		BraveTime.ClearMultiplier(Player.gameObject);
		StartCoroutine(UiFadeInCR());
		HideControlsUiAmount = 0f;
		OverrideControlsButton = true;
		InTutorial = ShouldDoTutorial;
		HideTutorialUiAmount = ((!InTutorial) ? 1 : 0);
		TutorialControls = new TutorialControlState[7]
		{
			TutorialControlState.Shown,
			TutorialControlState.Hidden,
			TutorialControlState.Hidden,
			TutorialControlState.Hidden,
			TutorialControlState.Hidden,
			TutorialControlState.Hidden,
			TutorialControlState.Hidden
		};
		m_tutorialSuperReady = false;
		TutorialUiUpdateTimer = 0f;
		UiManager.Invalidate();
		Opponent.Reset();
	}

	private IEnumerator UiFadeInCR()
	{
		HideUiAmount = 1f;
		UpdateUI();
		yield return new WaitForSeconds(1f);
		float ela = 0f;
		while (ela < 0.2f)
		{
			ela += BraveTime.DeltaTime;
			float t = Mathf.Lerp(0f, 1f, ela / 0.2f);
			HideUiAmount = 1f - t;
			UiManager.Invalidate();
			yield return null;
		}
	}

	private IEnumerator ControlsUiFadeOutCR()
	{
		m_isFadingControlsUi = true;
		HideControlsUiAmount = 0f;
		OverrideControlsButton = true;
		UpdateUI();
		yield return new WaitForSeconds(1f);
		float ela = 0f;
		while (ela < 0.2f)
		{
			ela += BraveTime.DeltaTime;
			float t = (HideControlsUiAmount = Mathf.Lerp(0f, 1f, ela / 0.2f));
			UiManager.Invalidate();
			yield return null;
		}
		OverrideControlsButton = false;
		m_isFadingControlsUi = false;
	}

	private IEnumerator TutorialUiFadeCR()
	{
		HideTutorialUiAmount = 0f;
		UpdateUI();
		yield return new WaitForSeconds(1f);
		float ela = 0f;
		while (ela < 0.5f)
		{
			ela += BraveTime.DeltaTime;
			float t = (HideTutorialUiAmount = Mathf.Lerp(0f, 1f, ela / 0.5f));
			UiManager.Invalidate();
			yield return null;
		}
	}

	public void DoWinFade(bool skipDelay)
	{
		StartCoroutine(DoWinFadeCR(skipDelay));
	}

	private IEnumerator DoWinFadeCR(bool skipDelay)
	{
		if (!skipDelay)
		{
			yield return new WaitForSeconds(5f);
		}
		CameraController camera = GameManager.Instance.MainCameraController;
		Pixelator.Instance.FadeToColor(2.5f, Color.white);
		yield return new WaitForSeconds(0.5f);
		float ela = 0f;
		float duration = 2f;
		Vector2 startPos = camera.OverridePosition;
		Vector2 endPos = startPos + new Vector2(0f, 4f);
		while (ela < duration)
		{
			ela += BraveTime.DeltaTime;
			float t2 = Mathf.Lerp(0f, 1f, ela / (duration * 2f));
			camera.OverridePosition = Vector2Extensions.SmoothStep(startPos, endPos, t2);
			t2 = (HideUiAmount = Mathf.Lerp(0f, 1f, ela / 0.2f));
			UiManager.Invalidate();
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		GameStatsManager.Instance.SetFlag(GungeonFlags.RESOURCEFUL_RAT_PUNCHOUT_BEATEN, true);
		PickupObject.RatBeatenAtPunchout = true;
		PlaceNPC();
		Pixelator.Instance.FadeToColor(1f, Color.white, true);
		TeardownPunchout();
	}

	public void DoLoseFade(bool skipDelay)
	{
		StartCoroutine(DoLoseFadeCR(skipDelay));
	}

	private IEnumerator DoLoseFadeCR(bool skipDelay)
	{
		if (!skipDelay)
		{
			yield return new WaitForSeconds(2f);
		}
		float ela = 0f;
		float duration = 3f;
		Material vignetteMaterial = Pixelator.Instance.FadeMaterial;
		while (ela < duration)
		{
			ela += BraveTime.DeltaTime;
			float t2 = Mathf.Lerp(0f, 1f, ela / duration);
			vignetteMaterial.SetColor("_VignetteColor", Color.black);
			vignetteMaterial.SetFloat("_VignettePower", Mathf.Lerp(0.5f, 10f, t2));
			t2 = (HideUiAmount = Mathf.Lerp(0f, 1f, ela / 0.2f));
			UiManager.Invalidate();
			yield return null;
		}
		Pixelator.Instance.FadeToColor(1f, Color.black);
		yield return new WaitForSeconds(1.5f);
		PlaceNote(PlayerLostNotePrefab.gameObject);
		Pixelator.Instance.FadeToColor(1f, Color.black, true);
		vignetteMaterial.SetColor("_VignetteColor", Color.black);
		vignetteMaterial.SetFloat("_VignettePower", 1f);
		TeardownPunchout();
	}

	private void PlaceNPC()
	{
		if (!PlayerWonRatNPC)
		{
			return;
		}
		RoomHandler currentRoom = GameManager.Instance.BestActivePlayer.CurrentRoom;
		bool success = false;
		IntVector2 centeredVisibleClearSpot = currentRoom.GetCenteredVisibleClearSpot(3, 3, out success, true);
		centeredVisibleClearSpot = centeredVisibleClearSpot - currentRoom.area.basePosition + IntVector2.One;
		if (!success)
		{
			return;
		}
		GameObject gameObject = DungeonPlaceableUtility.InstantiateDungeonPlaceable(PlayerWonRatNPC.gameObject, currentRoom, centeredVisibleClearSpot, false);
		if ((bool)gameObject)
		{
			IPlayerInteractable[] interfacesInChildren = gameObject.GetInterfacesInChildren<IPlayerInteractable>();
			for (int i = 0; i < interfacesInChildren.Length; i++)
			{
				currentRoom.RegisterInteractable(interfacesInChildren[i]);
			}
		}
	}

	private void PlaceNote(GameObject notePrefab)
	{
		if (!(notePrefab != null))
		{
			return;
		}
		RoomHandler currentRoom = GameManager.Instance.BestActivePlayer.CurrentRoom;
		bool success = false;
		IntVector2 centeredVisibleClearSpot = currentRoom.GetCenteredVisibleClearSpot(3, 3, out success, true);
		centeredVisibleClearSpot = centeredVisibleClearSpot - currentRoom.area.basePosition + IntVector2.One;
		if (!success)
		{
			return;
		}
		GameObject gameObject = DungeonPlaceableUtility.InstantiateDungeonPlaceable(notePrefab.gameObject, currentRoom, centeredVisibleClearSpot, false);
		if ((bool)gameObject)
		{
			IPlayerInteractable[] interfacesInChildren = gameObject.GetInterfacesInChildren<IPlayerInteractable>();
			for (int i = 0; i < interfacesInChildren.Length; i++)
			{
				currentRoom.RegisterInteractable(interfacesInChildren[i]);
			}
		}
	}

	public void DoBombFade()
	{
		StartCoroutine(DoBombFadeCR());
	}

	private IEnumerator DoBombFadeCR()
	{
		float fadeOutTime = 1.66f;
		Pixelator.Instance.FadeToColor(fadeOutTime, Color.white);
		float ela = 0f;
		while (ela < fadeOutTime)
		{
			ela += BraveTime.DeltaTime;
			float t = (HideUiAmount = Mathf.Lerp(0f, 1f, ela / 0.2f));
			UiManager.Invalidate();
			yield return null;
		}
		GameManager.Instance.PrimaryPlayer.DoVibration(Vibration.Time.Normal, Vibration.Strength.Hard);
		AkSoundEngine.PostEvent("Play_OBJ_nuke_blast_01", base.gameObject);
		float timer4 = 0f;
		float duration4 = 0.1f;
		while (timer4 < duration4)
		{
			yield return null;
			timer4 += BraveTime.DeltaTime;
			Pixelator.Instance.FadeColor = Color.Lerp(Color.white, Color.yellow, Mathf.Clamp01(timer4 / duration4));
		}
		timer4 = 0f;
		duration4 = 0.1f;
		while (timer4 < duration4)
		{
			yield return null;
			timer4 += BraveTime.DeltaTime;
			Pixelator.Instance.FadeColor = Color.Lerp(Color.yellow, Color.red, Mathf.Clamp01(timer4 / duration4));
		}
		timer4 = 0f;
		duration4 = 0.1f;
		while (timer4 < duration4)
		{
			yield return null;
			timer4 += BraveTime.DeltaTime;
			Pixelator.Instance.FadeColor = Color.Lerp(Color.red, Color.yellow, Mathf.Clamp01(timer4 / duration4));
		}
		timer4 = 0f;
		duration4 = 0.1f;
		while (timer4 < duration4)
		{
			yield return null;
			timer4 += BraveTime.DeltaTime;
			Pixelator.Instance.FadeColor = Color.Lerp(Color.yellow, Color.white, Mathf.Clamp01(timer4 / duration4));
		}
		yield return new WaitForSeconds(1.5f);
		Pixelator.Instance.FadeToColor(1f, Color.white, true);
		TeardownPunchout();
	}

	private void InitPunchout()
	{
		if (Minimap.HasInstance)
		{
			Minimap.Instance.TemporarilyPreventMinimap = true;
			GameUIRoot.Instance.HideCoreUI("punchout");
			GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
			m_cameraCenterPos = GameManager.Instance.BestActivePlayer.specRigidbody.GetUnitCenter(ColliderType.HitBox) + new Vector2(0f, -25f);
			base.transform.position = m_cameraCenterPos - PhysicsEngine.PixelToUnit(new IntVector2(240, 130));
			tk2dBaseSprite[] componentsInChildren = GetComponentsInChildren<tk2dBaseSprite>();
			foreach (tk2dBaseSprite tk2dBaseSprite2 in componentsInChildren)
			{
				tk2dBaseSprite2.UpdateZDepth();
			}
			CameraController mainCameraController = GameManager.Instance.MainCameraController;
			mainCameraController.OverridePosition = m_cameraCenterPos;
			mainCameraController.SetManualControl(true, false);
			mainCameraController.LockToRoom = true;
			mainCameraController.SetZoomScaleImmediate(1.6f);
			PlayerController[] allPlayers = GameManager.Instance.AllPlayers;
			foreach (PlayerController playerController in allPlayers)
			{
				playerController.SetInputOverride("punchout");
				playerController.healthHaver.IsVulnerable = false;
				playerController.SuppressEffectUpdates = true;
				playerController.IsOnFire = false;
				playerController.CurrentFireMeterValue = 0f;
				playerController.CurrentPoisonMeterValue = 0f;
				if ((bool)playerController.specRigidbody)
				{
					DeadlyDeadlyGoopManager.DelayedClearGoopsInRadius(playerController.specRigidbody.UnitCenter, 1f);
				}
			}
			GameManager.Instance.DungeonMusicController.SwitchToBossMusic("Play_MUS_RatPunch_Intro_01", base.gameObject);
			ParticleSystem[] componentsInChildren2 = base.transform.Find("arena").GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem particleSystem in componentsInChildren2)
			{
				particleSystem.transform.position = particleSystem.transform.position.XY().ToVector3ZisY();
			}
			Light[] componentsInChildren3 = base.transform.Find("arena").GetComponentsInChildren<Light>();
			foreach (Light light in componentsInChildren3)
			{
				light.transform.position = light.transform.position.XY().ToVector3ZisY(-18f);
			}
		}
		else
		{
			AkSoundEngine.PostEvent("Play_MUS_RatPunch_Intro_01", base.gameObject);
		}
		InTutorial = ShouldDoTutorial;
		HideTutorialUiAmount = ((!InTutorial) ? 1 : 0);
		TutorialControls = new TutorialControlState[7]
		{
			TutorialControlState.Shown,
			TutorialControlState.Hidden,
			TutorialControlState.Hidden,
			TutorialControlState.Hidden,
			TutorialControlState.Hidden,
			TutorialControlState.Hidden,
			TutorialControlState.Hidden
		};
		OverrideControlsButton = true;
	}

	private void TeardownPunchout()
	{
		if (m_isInitialized)
		{
			Minimap.Instance.TemporarilyPreventMinimap = false;
			GameUIRoot.Instance.ShowCoreUI("punchout");
			GameUIRoot.Instance.ShowCoreUI(string.Empty);
			GameUIRoot.Instance.ToggleLowerPanels(true, false, string.Empty);
			CameraController mainCameraController = GameManager.Instance.MainCameraController;
			mainCameraController.SetManualControl(false, false);
			mainCameraController.LockToRoom = false;
			mainCameraController.SetZoomScaleImmediate(1f);
			PlayerController[] allPlayers = GameManager.Instance.AllPlayers;
			foreach (PlayerController playerController in allPlayers)
			{
				playerController.ClearInputOverride("punchout");
				playerController.healthHaver.IsVulnerable = true;
				playerController.SuppressEffectUpdates = false;
				playerController.IsOnFire = false;
				playerController.CurrentFireMeterValue = 0f;
				playerController.CurrentPoisonMeterValue = 0f;
			}
			GameManager.Instance.DungeonMusicController.EndBossMusic();
			MetalGearRatRoomController metalGearRatRoomController = Object.FindObjectOfType<MetalGearRatRoomController>();
			if ((bool)metalGearRatRoomController)
			{
				GameObject item = PickupObjectDatabase.GetById(GlobalItemIds.RatKey).gameObject;
				Vector3 position = metalGearRatRoomController.transform.position;
				if (Opponent.NumKeysDropped >= 1)
				{
					LootEngine.SpawnItem(item, position + new Vector3(14.25f, 17f), Vector2.zero, 0f);
				}
				if (Opponent.NumKeysDropped >= 2)
				{
					LootEngine.SpawnItem(item, position + new Vector3(13.25f, 14.5f), Vector2.zero, 0f);
				}
				if (Opponent.NumKeysDropped >= 3)
				{
					LootEngine.SpawnItem(item, position + new Vector3(14.25f, 12f), Vector2.zero, 0f);
				}
				if (Opponent.NumKeysDropped >= 4)
				{
					LootEngine.SpawnItem(item, position + new Vector3(30.25f, 17f), Vector2.zero, 0f);
				}
				if (Opponent.NumKeysDropped >= 5)
				{
					LootEngine.SpawnItem(item, position + new Vector3(31.25f, 14.5f), Vector2.zero, 0f);
				}
				if (Opponent.NumKeysDropped >= 6)
				{
					LootEngine.SpawnItem(item, position + new Vector3(30.25f, 12f), Vector2.zero, 0f);
				}
				Vector2 vector = position + new Vector3(22.25f, 14.5f);
				foreach (int droppedRewardId in Opponent.DroppedRewardIds)
				{
					float degrees = (float)((!BraveUtility.RandomBool()) ? 180 : 0) + Random.Range(-30f, 30f);
					GameObject item2 = PickupObjectDatabase.GetById(droppedRewardId).gameObject;
					LootEngine.SpawnItem(item2, vector + new Vector2(11f, 0f).Rotate(degrees), Vector2.zero, 0f);
				}
			}
			GameStatsManager.Instance.SetFlag(GungeonFlags.ITEMSPECIFIC_BOXING_GLOVE, true);
			BraveTime.ClearMultiplier(Player.gameObject);
			Object.Destroy(base.gameObject);
		}
		else
		{
			Reset();
		}
	}

	private void UpdateUI()
	{
		string text = ControlsLabel.ForceGetLocalizedValue("#MAINMENU_CONTROLS");
		if (text == "CONTROLS")
		{
			text = "Controls";
		}
		ControlsLabel.Text = text + " (" + StringTableManager.EvaluateReplacementToken("%CONTROL_PAUSE") + ")";
		float scaleTileScale = Pixelator.Instance.ScaleTileScale;
		PlayerHealthBarBase.transform.localScale = new Vector3(scaleTileScale, scaleTileScale, 1f);
		RatHealthBarBase.transform.localScale = new Vector3(scaleTileScale, scaleTileScale, 1f);
		ControlsLabel.transform.localScale = new Vector3(scaleTileScale, scaleTileScale, 1f);
		TutorialLabel.transform.localScale = new Vector3(scaleTileScale, scaleTileScale, 1f);
		float num = (PlayerHealthBarBase.Height + 8f) * scaleTileScale * HideUiAmount;
		PlayerHealthBarBase.Position = new Vector3(4f * scaleTileScale, -8f * scaleTileScale + num);
		RatHealthBarBase.Position = new Vector3(UiPanel.Size.x - RatHealthBarBase.Size.x - 4f * scaleTileScale, -8f * scaleTileScale + num);
		float num2 = (0f - (ControlsLabel.Height * scaleTileScale + 8f)) * scaleTileScale * HideControlsUiAmount;
		ControlsLabel.Position = new Vector3(UiPanel.Size.x - ControlsLabel.Size.x * scaleTileScale - 4f * scaleTileScale, 0f - UiPanel.Size.y + ControlsLabel.Size.y + 4f * scaleTileScale + num2);
		float num3 = (0f - (TutorialLabel.Width + 4f)) * scaleTileScale * HideTutorialUiAmount;
		TutorialLabel.Position = new Vector3(scaleTileScale * 4f + num3, 0f - UiPanel.Size.y + TutorialLabel.Size.y + 4f * scaleTileScale);
	}

	private void UpdateTutorialText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		HandleTutorialLine(stringBuilder, 0, "#OPTIONS_PUNCHOUT_DODGELEFT", GungeonActions.GungeonActionType.PunchoutDodgeLeft);
		HandleTutorialLine(stringBuilder, 1, "#OPTIONS_PUNCHOUT_DODGERIGHT", GungeonActions.GungeonActionType.PunchoutDodgeRight);
		HandleTutorialLine(stringBuilder, 2, "#OPTIONS_PUNCHOUT_BLOCK", GungeonActions.GungeonActionType.PunchoutBlock);
		HandleTutorialLine(stringBuilder, 3, "#OPTIONS_PUNCHOUT_DUCK", GungeonActions.GungeonActionType.PunchoutDuck);
		HandleTutorialLine(stringBuilder, 4, "#OPTIONS_PUNCHOUT_PUNCHLEFT", GungeonActions.GungeonActionType.PunchoutPunchLeft);
		HandleTutorialLine(stringBuilder, 5, "#OPTIONS_PUNCHOUT_PUNCHRIGHT", GungeonActions.GungeonActionType.PunchoutPunchRight);
		HandleTutorialLine(stringBuilder, 6, "#OPTIONS_PUNCHOUT_SUPER", GungeonActions.GungeonActionType.PunchoutSuper);
		TutorialLabel.Text = stringBuilder.ToString();
	}

	public static void InputWasPressed(int action)
	{
		if (TutorialControls[action] == TutorialControlState.Shown)
		{
			TutorialControls[action] = TutorialControlState.Completed;
			if (action < TutorialControls.Length - 1)
			{
				TutorialControls[action + 1] = TutorialControlState.Shown;
			}
			TutorialUiUpdateTimer = 0f;
		}
	}

	private void HandleTutorialLine(StringBuilder str, int i, string commandName, GungeonActions.GungeonActionType action)
	{
		if (TutorialControls[i] == TutorialControlState.Hidden)
		{
			str.AppendLine();
			return;
		}
		bool flag = TutorialControls[i] == TutorialControlState.Completed;
		if (flag)
		{
			str.Append("[color green]");
		}
		str.Append(TutorialLabel.ForceGetLocalizedValue(commandName));
		if (flag)
		{
			str.Append("[/color]");
		}
		str.Append(" (").Append(GetTutorialText(action)).AppendLine(")");
	}

	private string GetTutorialText(GungeonActions.GungeonActionType action)
	{
		BraveInput primaryPlayerInstance = BraveInput.PrimaryPlayerInstance;
		if (primaryPlayerInstance.IsKeyboardAndMouse())
		{
			return StringTableManager.GetBindingText(action);
		}
		if (primaryPlayerInstance != null)
		{
			ReadOnlyCollection<BindingSource> bindings = primaryPlayerInstance.ActiveActions.GetActionFromType(action).Bindings;
			if (bindings.Count > 0)
			{
				for (int i = 0; i < bindings.Count; i++)
				{
					DeviceBindingSource deviceBindingSource = bindings[i] as DeviceBindingSource;
					if (deviceBindingSource != null && deviceBindingSource.Control != 0)
					{
						return UIControllerButtonHelper.GetUnifiedControllerButtonTag(deviceBindingSource.Control, BraveInput.PlayerOneCurrentSymbology);
					}
				}
			}
		}
		return UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.Start, BraveInput.PlayerOneCurrentSymbology);
	}

	private void UpdateTimer()
	{
		int num = Mathf.CeilToInt(Timer);
		int num2 = (int)((float)num / 60f);
		num -= num2 * 60;
		TimerTextMin1.text = (num2 / 10).ToString();
		TimerTextMin2.text = (num2 % 10).ToString();
		TimerTextSec1.text = (num / 10).ToString();
		TimerTextSec2.text = (num % 10).ToString();
	}
}
