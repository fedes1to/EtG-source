using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using InControl;
using UnityEngine;

public class Foyer : MonoBehaviour
{
	public static bool DoIntroSequence = true;

	public static bool DoMainMenu = true;

	private static Foyer m_instance;

	public SpeculativeRigidbody TutorialBlocker;

	public FinalIntroSequenceManager IntroDoer;

	public Action<PlayerController> OnPlayerCharacterChanged;

	public Action OnCoopModeChanged;

	public Renderer PrimerSprite;

	public Renderer PowderSprite;

	public Renderer SlugSprite;

	public Renderer CasingSprite;

	public Renderer StatueSprite;

	public FoyerCharacterSelectFlag CurrentSelectedCharacterFlag;

	public static bool IsCurrentlyPlayingCharacterSelect;

	public static Foyer Instance
	{
		get
		{
			if (!m_instance)
			{
				m_instance = UnityEngine.Object.FindObjectOfType<Foyer>();
			}
			return m_instance;
		}
	}

	public static void ClearInstance()
	{
		m_instance = null;
	}

	private void Awake()
	{
		DebugTime.Log("Foyer.Awake()");
		GameManager.EnsureExistence();
		GameManager.Instance.IsFoyer = true;
	}

	private void CheckHeroStatue()
	{
		PrimerSprite.enabled = GameStatsManager.Instance.GetFlag(GungeonFlags.BLACKSMITH_ELEMENT1);
		PowderSprite.enabled = GameStatsManager.Instance.GetFlag(GungeonFlags.BLACKSMITH_ELEMENT2);
		SlugSprite.enabled = GameStatsManager.Instance.GetFlag(GungeonFlags.BLACKSMITH_ELEMENT3);
		CasingSprite.enabled = GameStatsManager.Instance.GetFlag(GungeonFlags.BLACKSMITH_ELEMENT4);
		if (PowderSprite.enabled && GameStatsManager.Instance.GetFlag(GungeonFlags.BOSSKILLED_HIGHDRAGUN))
		{
			PowderSprite.GetComponent<tk2dBaseSprite>().SetSprite("statue_of_time_gunpowder_gold_001");
		}
		if (PrimerSprite.enabled && GameStatsManager.Instance.GetFlag(GungeonFlags.BOSSKILLED_HIGHDRAGUN))
		{
			PrimerSprite.GetComponent<tk2dBaseSprite>().SetSprite("statue_of_time_shield_gold_001");
		}
		if (CasingSprite.enabled && GameStatsManager.Instance.GetFlag(GungeonFlags.BOSSKILLED_HIGHDRAGUN))
		{
			CasingSprite.GetComponent<tk2dBaseSprite>().SetSprite("statue_of_time_shell_gold_001");
		}
		if ((bool)StatueSprite && GameStatsManager.Instance.GetFlag(GungeonFlags.BOSSKILLED_HIGHDRAGUN))
		{
			StatueSprite.GetComponent<tk2dBaseSprite>().SetSprite("statue_of_time_dragun_001");
			Transform transform = StatueSprite.transform.Find("shadow");
			if ((bool)transform)
			{
				transform.GetComponent<tk2dBaseSprite>().SetSprite("statue_of_time_dragun_shadow_001");
			}
		}
	}

	private IEnumerator Start()
	{
		yield return null;
		while (Dungeon.IsGenerating || GameManager.Instance.IsLoadingLevel)
		{
			yield return null;
		}
		RenderSettings.ambientIntensity = 1f;
		CheckHeroStatue();
		GameManager.IsReturningToBreach = false;
		Foyer foyer = this;
		foyer.OnPlayerCharacterChanged = (Action<PlayerController>)Delegate.Combine(foyer.OnPlayerCharacterChanged, new Action<PlayerController>(ToggleTutorialBlocker));
		Foyer foyer2 = this;
		foyer2.OnCoopModeChanged = (Action)Delegate.Combine(foyer2.OnCoopModeChanged, (Action)delegate
		{
			ToggleTutorialBlocker(null);
		});
		Foyer foyer3 = this;
		foyer3.OnCoopModeChanged = (Action)Delegate.Combine(foyer3.OnCoopModeChanged, new Action(GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().ToggleExitCoopButtonOnCoopChange));
		AmmonomiconController.EnsureExistence();
		if (DoIntroSequence || DoMainMenu)
		{
			BraveCameraUtility.OverrideAspect = 1.77777779f;
		}
		if (DoIntroSequence)
		{
			GameManager.Instance.IsSelectingCharacter = true;
			yield return StartCoroutine(HandleIntroSequence());
			IntroDoer.transform.parent.gameObject.SetActive(false);
		}
		else
		{
			IntroDoer = UnityEngine.Object.FindObjectOfType<FinalIntroSequenceManager>();
			if (IntroDoer != null)
			{
				IntroDoer.transform.parent.gameObject.SetActive(false);
			}
		}
		DoIntroSequence = false;
		if (DoMainMenu)
		{
			AkSoundEngine.PostEvent("Play_MUS_title_theme_01", base.gameObject);
			yield return StartCoroutine(HandleMainMenu());
		}
		else
		{
			MainMenuFoyerController mmfc = UnityEngine.Object.FindObjectOfType<MainMenuFoyerController>();
			if ((bool)mmfc)
			{
				mmfc.DisableMainMenu();
			}
			TitleDioramaController tdc = UnityEngine.Object.FindObjectOfType<TitleDioramaController>();
			if ((bool)tdc)
			{
				tdc.ForceHideFadeQuad();
			}
			ToggleTutorialBlocker(GameManager.Instance.PrimaryPlayer);
			yield return null;
			Pixelator.Instance.FadeToBlack(0.125f, true, 0.05f);
		}
		while (GameManager.Instance.IsLoadingLevel)
		{
			yield return null;
		}
		bool didCharacterSelect = true;
		GameManager.Instance.MainCameraController.OverrideZoomScale = 1f;
		GameManager.Instance.MainCameraController.CurrentZoomScale = 1f;
		GameManager.Instance.DungeonMusicController.ResetForNewFloor(GameManager.Instance.Dungeon);
		if (GameManager.Instance.PrimaryPlayer == null)
		{
			StartCoroutine(HandleCharacterSelect());
		}
		else
		{
			didCharacterSelect = false;
			SetUpCharacterCallbacks();
			DisableActiveCharacterSelectCharacter();
			GameManager.Instance.IsSelectingCharacter = false;
		}
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			ProcessPlayerEnteredFoyer(GameManager.Instance.AllPlayers[i]);
		}
		Component[] ixables = GetComponentsInChildren<Component>();
		for (int j = 0; j < ixables.Length; j++)
		{
			if (ixables[j] is IPlayerInteractable && !(ixables[j] is PickupObject))
			{
				RoomHandler.unassignedInteractableObjects.Add(ixables[j] as IPlayerInteractable);
			}
		}
		yield return null;
		if (!didCharacterSelect)
		{
			ShadowSystem.ForceAllLightsUpdate();
		}
		FlagPitSRBsAsUnpathableCells();
	}

	private void ToggleTutorialBlocker(PlayerController player)
	{
		bool flag = false;
		if (player != null)
		{
			flag = player.characterIdentity == PlayableCharacters.Convict || player.characterIdentity == PlayableCharacters.Guide || player.characterIdentity == PlayableCharacters.Pilot || player.characterIdentity == PlayableCharacters.Soldier;
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			flag = false;
		}
		if (flag)
		{
			TutorialBlocker.gameObject.SetActive(false);
			TutorialBlocker.enabled = false;
		}
		else
		{
			TutorialBlocker.enabled = true;
			TutorialBlocker.gameObject.SetActive(true);
		}
	}

	private IEnumerator HandleIntroSequence()
	{
		yield return null;
		IntroDoer = UnityEngine.Object.FindObjectOfType<FinalIntroSequenceManager>();
		if (IntroDoer != null)
		{
			IntroDoer.TriggerSequence();
			while (IntroDoer.IsDoingIntro)
			{
				yield return null;
			}
		}
	}

	private IEnumerator HandleMainMenu()
	{
		MainMenuFoyerController mmfc = UnityEngine.Object.FindObjectOfType<MainMenuFoyerController>();
		if ((bool)mmfc)
		{
			mmfc.InitializeMainMenu();
		}
		GameUIRoot.Instance.Manager.RenderCamera.enabled = false;
		GameManager.Instance.IsSelectingCharacter = true;
		while (DoMainMenu)
		{
			yield return null;
			if (DoMainMenu)
			{
				mmfc.NewGameButton.GUIManager.RenderCamera.enabled = true;
			}
		}
		GameUIRoot.Instance.Manager.RenderCamera.enabled = true;
	}

	private void DisableActiveCharacterSelectCharacter()
	{
		FoyerCharacterSelectFlag[] array = UnityEngine.Object.FindObjectsOfType<FoyerCharacterSelectFlag>();
		List<FoyerCharacterSelectFlag> list = new List<FoyerCharacterSelectFlag>();
		while (list.Count < array.Length)
		{
			FoyerCharacterSelectFlag foyerCharacterSelectFlag = null;
			for (int i = 0; i < array.Length; i++)
			{
				if (!list.Contains(array[i]))
				{
					foyerCharacterSelectFlag = ((!(foyerCharacterSelectFlag == null)) ? ((!(foyerCharacterSelectFlag.transform.position.x < array[i].transform.position.x)) ? array[i] : foyerCharacterSelectFlag) : array[i]);
				}
			}
			list.Add(foyerCharacterSelectFlag);
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].IsCoopCharacter)
			{
				list.RemoveAt(j);
				j--;
			}
			else if (!list[j].PrerequisitesFulfilled())
			{
				list.RemoveAt(j);
				j--;
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			list[k].OnSelectedCharacterCallback(GameManager.Instance.PrimaryPlayer);
		}
	}

	private List<FoyerCharacterSelectFlag> SetUpCharacterCallbacks()
	{
		FoyerCharacterSelectFlag[] array = UnityEngine.Object.FindObjectsOfType<FoyerCharacterSelectFlag>();
		List<FoyerCharacterSelectFlag> list = new List<FoyerCharacterSelectFlag>();
		while (list.Count < array.Length)
		{
			FoyerCharacterSelectFlag foyerCharacterSelectFlag = null;
			for (int i = 0; i < array.Length; i++)
			{
				if (!list.Contains(array[i]))
				{
					foyerCharacterSelectFlag = ((!(foyerCharacterSelectFlag == null)) ? ((!(foyerCharacterSelectFlag.transform.position.x < array[i].transform.position.x)) ? array[i] : foyerCharacterSelectFlag) : array[i]);
				}
			}
			list.Add(foyerCharacterSelectFlag);
		}
		for (int j = 0; j < list.Count; j++)
		{
			if (list[j].IsCoopCharacter)
			{
				OnCoopModeChanged = (Action)Delegate.Combine(OnCoopModeChanged, new Action(list[j].OnCoopChangedCallback));
				list.RemoveAt(j);
				j--;
			}
			else if (!list[j].PrerequisitesFulfilled())
			{
				FoyerCharacterSelectFlag foyerCharacterSelectFlag2 = list[j];
				UnityEngine.Object.Destroy(foyerCharacterSelectFlag2.gameObject);
				list.RemoveAt(j);
				j--;
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			OnPlayerCharacterChanged = (Action<PlayerController>)Delegate.Combine(OnPlayerCharacterChanged, new Action<PlayerController>(list[k].OnSelectedCharacterCallback));
			tk2dBaseSprite sprite = list[k].sprite;
			sprite.usesOverrideMaterial = true;
			Renderer renderer = sprite.renderer;
			if (!renderer.material.shader.name.Contains("PlayerPalettized"))
			{
				renderer.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTiltedCutout");
			}
		}
		return list;
	}

	private IEnumerator HandleAmmonomiconLabel()
	{
		float ela = 0f;
		while (GameManager.Instance.IsSelectingCharacter)
		{
			ela += BraveTime.DeltaTime;
			int counter = 0;
			while (AmmonomiconController.Instance == null && counter < 15)
			{
				counter++;
				yield return null;
			}
			GameUIRoot.Instance.FoyerAmmonomiconLabel.IsVisible = !AmmonomiconController.Instance.IsOpen && !GameManager.Instance.IsPaused;
			GameUIRoot.Instance.FoyerAmmonomiconLabel.Opacity = Mathf.Clamp01(ela);
			string targetLabelString3 = GameUIRoot.Instance.FoyerAmmonomiconLabel.ForceGetLocalizedValue("#OPTIONS_INVENTORY") + " (";
			targetLabelString3 = (BraveInput.PlayerlessInstance.IsKeyboardAndMouse() ? (targetLabelString3 + StringTableManager.GetBindingText(GungeonActions.GungeonActionType.EquipmentMenu)) : ((BraveInput.PlayerOneCurrentSymbology == GameOptions.ControllerSymbology.PS4) ? (targetLabelString3 + UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.TouchPadButton, BraveInput.PlayerOneCurrentSymbology)) : ((Application.platform != RuntimePlatform.XboxOne && Application.platform != RuntimePlatform.MetroPlayerX64 && Application.platform != RuntimePlatform.MetroPlayerX86) ? (targetLabelString3 + UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.Pause, BraveInput.PlayerOneCurrentSymbology)) : (targetLabelString3 + UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.Select, BraveInput.PlayerOneCurrentSymbology)))));
			targetLabelString3 += "\t)";
			GameUIRoot.Instance.FoyerAmmonomiconLabel.TabSize = 1;
			GameUIRoot.Instance.FoyerAmmonomiconLabel.ProcessMarkup = true;
			GameUIRoot.Instance.FoyerAmmonomiconLabel.Text = targetLabelString3;
			yield return null;
		}
		ela = 0f;
		while (ela < 0.5f)
		{
			ela += BraveTime.DeltaTime;
			GameUIRoot.Instance.FoyerAmmonomiconLabel.Opacity = 1f - ela / 0.5f;
			yield return null;
		}
	}

	private IEnumerator HandleCharacterSelect()
	{
		GameManager.Instance.IsSelectingCharacter = true;
		StartCoroutine(HandleAmmonomiconLabel());
		GameManager.Instance.Dungeon.data.Entrance.visibility = RoomHandler.VisibilityStatus.CURRENT;
		Pixelator.Instance.ProcessOcclusionChange(IntVector2.Zero, 1f, GameManager.Instance.Dungeon.data.Entrance, false);
		yield return null;
		GameManager.Instance.Dungeon.data.Entrance.visibility = RoomHandler.VisibilityStatus.CURRENT;
		Pixelator.Instance.ProcessOcclusionChange(IntVector2.Zero, 1f, GameManager.Instance.Dungeon.data.Entrance, false);
		GameUIRoot.Instance.HideCoreUI(string.Empty);
		GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
		GameManager.Instance.MainCameraController.SetManualControl(true, false);
		GameManager.Instance.MainCameraController.OverridePosition = new Vector3(27f, 25f, 0f);
		yield return null;
		Pixelator.Instance.SetOcclusionDirty();
		GameManager.Instance.Dungeon.data.Entrance.OnBecameVisible(null);
		List<FoyerCharacterSelectFlag> sortedByXAxis = SetUpCharacterCallbacks();
		bool hasSelected = false;
		int currentSelected = 0;
		int m_queuedChange = 0;
		Vector2 m_lastMousePosition = Vector2.zero;
		int m_lastMouseSelected = -1;
		FoyerCharacterSelectFlag currentlySelectedCharacter = sortedByXAxis[currentSelected];
		yield return new WaitForSeconds(0.25f);
		currentlySelectedCharacter.CreateOverheadElement();
		Action HandleShiftLeft = delegate
		{
			if (FoyerInfoPanelController.IsTransitioning)
			{
				m_queuedChange = -1;
			}
			else
			{
				currentSelected = (currentSelected - 1 + sortedByXAxis.Count) % sortedByXAxis.Count;
				AkSoundEngine.PostEvent("Play_UI_menu_select_01", base.gameObject);
			}
		};
		Action HandleShiftRight = delegate
		{
			if (FoyerInfoPanelController.IsTransitioning)
			{
				m_queuedChange = 1;
			}
			else
			{
				currentSelected = (currentSelected + 1 + sortedByXAxis.Count) % sortedByXAxis.Count;
				AkSoundEngine.PostEvent("Play_UI_menu_select_01", base.gameObject);
			}
		};
		Action HandleSelect = delegate
		{
			if (!hasSelected && !(GameManager.Instance.PrimaryPlayer != null))
			{
				CurrentSelectedCharacterFlag = null;
				hasSelected = true;
				AkSoundEngine.PostEvent("Play_UI_menu_characterselect_01", base.gameObject);
				CharacterSelectIdleDoer component = sortedByXAxis[currentSelected].GetComponent<CharacterSelectIdleDoer>();
				component.enabled = false;
				float delayTime = 0.25f;
				if (!component.IsEevee && !sortedByXAxis[currentSelected].IsAlternateCostume && component != null && !string.IsNullOrEmpty(component.onSelectedAnimation))
				{
					tk2dSpriteAnimationClip clipByName = component.spriteAnimator.GetClipByName(component.onSelectedAnimation);
					if (clipByName != null)
					{
						delayTime = (float)clipByName.frames.Length / clipByName.fps;
						component.spriteAnimator.Play(clipByName);
					}
					else
					{
						delayTime = 1f;
					}
				}
				else if (component.IsEevee)
				{
					delayTime = 1f;
				}
				StartCoroutine(OnSelectedCharacter(delayTime, sortedByXAxis[currentSelected]));
			}
		};
		bool pauseMenuWasJustOpen = false;
		while (!hasSelected)
		{
			GameManager.Instance.MainCameraController.OverrideZoomScale = 1f;
			GameManager.Instance.MainCameraController.CurrentZoomScale = 1f;
			GungeonActions activeActions = BraveInput.GetInstanceForPlayer(0).ActiveActions;
			if (GameManager.Instance.IsPaused)
			{
				pauseMenuWasJustOpen = true;
				sortedByXAxis[currentSelected].ToggleOverheadElementVisibility(false);
				yield return null;
				continue;
			}
			if ((bool)AmmonomiconController.Instance && AmmonomiconController.Instance.IsOpen)
			{
				pauseMenuWasJustOpen = true;
				if (activeActions.EquipmentMenuAction.WasPressed || activeActions.PauseAction.WasPressed)
				{
					AmmonomiconController.Instance.CloseAmmonomicon();
				}
				yield return null;
				continue;
			}
			if (currentSelected >= 0 && currentSelected < sortedByXAxis.Count)
			{
				CurrentSelectedCharacterFlag = sortedByXAxis[currentSelected];
			}
			else
			{
				CurrentSelectedCharacterFlag = null;
			}
			int cachedSelected = currentSelected;
			if (activeActions.EquipmentMenuAction.WasPressed)
			{
				AmmonomiconController.Instance.OpenAmmonomicon(false, false);
				yield return null;
				continue;
			}
			if (activeActions.SelectLeft.WasPressedAsDpadRepeating)
			{
				HandleShiftLeft();
			}
			if (activeActions.SelectRight.WasPressedAsDpadRepeating)
			{
				HandleShiftRight();
			}
			if (activeActions.MenuSelectAction.WasPressed || activeActions.InteractAction.WasPressed || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
			{
				bool flag = CurrentSelectedCharacterFlag.CanBeSelected();
				if (!pauseMenuWasJustOpen && flag)
				{
					HandleSelect();
				}
				else if (!flag)
				{
					AkSoundEngine.PostEvent("Play_UI_menu_cancel_01", base.gameObject);
				}
			}
			Vector2 mouseDelta = Input.mousePosition.XY() - m_lastMousePosition;
			m_lastMousePosition = Input.mousePosition.XY();
			if (mouseDelta.magnitude > 2f)
			{
				int num = -1;
				float num2 = float.MaxValue;
				Vector2 a = GameManager.Instance.MainCameraController.Camera.ScreenToWorldPoint(Input.mousePosition).XY();
				for (int i = 0; i < sortedByXAxis.Count; i++)
				{
					tk2dBaseSprite sprite = sortedByXAxis[i].GetComponent<CharacterSelectIdleDoer>().sprite;
					float num3 = Vector2.Distance(a, sprite.WorldCenter);
					if (num3 < num2 && num3 < 1.5f)
					{
						num2 = num3;
						num = i;
					}
				}
				if (!FoyerInfoPanelController.IsTransitioning)
				{
					if (num != -1 && num != currentSelected)
					{
						currentSelected = num;
						AkSoundEngine.PostEvent("Play_UI_menu_select_01", base.gameObject);
					}
					m_lastMouseSelected = num;
				}
			}
			if (Input.GetMouseButtonDown(0) && m_lastMouseSelected != -1)
			{
				currentSelected = m_lastMouseSelected;
				if (currentSelected >= 0 && currentSelected < sortedByXAxis.Count && sortedByXAxis[currentSelected].CanBeSelected())
				{
					HandleSelect();
				}
				else
				{
					AkSoundEngine.PostEvent("Play_UI_menu_cancel_01", base.gameObject);
				}
			}
			if (m_queuedChange != 0 && !FoyerInfoPanelController.IsTransitioning)
			{
				if (cachedSelected == currentSelected)
				{
					currentSelected = (currentSelected + m_queuedChange + sortedByXAxis.Count) % sortedByXAxis.Count;
					AkSoundEngine.PostEvent("Play_UI_menu_select_01", base.gameObject);
				}
				m_queuedChange = 0;
			}
			if (cachedSelected != currentSelected)
			{
				sortedByXAxis[currentSelected].CreateOverheadElement();
			}
			else
			{
				sortedByXAxis[currentSelected].ToggleOverheadElementVisibility(true);
			}
			if (Time.frameCount % 5 == 0 && Time.timeSinceLevelLoad < 15f)
			{
				Pixelator.Instance.ProcessOcclusionChange(IntVector2.Zero, 1f, GameManager.Instance.Dungeon.data.Entrance, false);
				Pixelator.Instance.SetOcclusionDirty();
			}
			yield return null;
			pauseMenuWasJustOpen = false;
		}
	}

	public IEnumerator OnSelectedCharacter(float delayTime, FoyerCharacterSelectFlag flag)
	{
		IsCurrentlyPlayingCharacterSelect = true;
		GameManager.Instance.MainCameraController.OverrideRecoverySpeed = 3f;
		float ela = 0f;
		Vector2 startCamPos = GameManager.Instance.MainCameraController.OverridePosition;
		while (ela < delayTime)
		{
			ela += GameManager.INVARIANT_DELTA_TIME;
			float t = Mathf.SmoothStep(0f, 1f, ela / delayTime);
			GameManager.Instance.MainCameraController.OverridePosition = Vector2.Lerp(startCamPos, flag.transform.position.XY(), t);
			yield return null;
		}
		GameManager.PlayerPrefabForNewGame = (GameObject)BraveResources.Load(flag.CharacterPrefabPath);
		PlayerController playerController = GameManager.PlayerPrefabForNewGame.GetComponent<PlayerController>();
		GameStatsManager.Instance.BeginNewSession(playerController);
		PlayerController extantPlayer = null;
		if (extantPlayer == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(GameManager.PlayerPrefabForNewGame, flag.transform.position, Quaternion.identity);
			GameManager.PlayerPrefabForNewGame = null;
			gameObject.SetActive(true);
			extantPlayer = gameObject.GetComponent<PlayerController>();
		}
		extantPlayer.PlayerIDX = 0;
		GameManager.Instance.PrimaryPlayer = extantPlayer;
		if (flag.IsAlternateCostume)
		{
			extantPlayer.SwapToAlternateCostume();
		}
		PlayerCharacterChanged(extantPlayer);
		IsCurrentlyPlayingCharacterSelect = false;
		GameManager.Instance.IsSelectingCharacter = false;
		GameUIRoot.Instance.ShowCoreUI(string.Empty);
		GameManager.Instance.MainCameraController.OverrideRecoverySpeed = 3f;
		GameManager.Instance.MainCameraController.SetManualControl(false);
		StartCoroutine(HandleInputDelay(extantPlayer, 0.33f));
		yield return null;
		PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(extantPlayer.specRigidbody);
	}

	private IEnumerator HandleInputDelay(PlayerController p, float d)
	{
		p.SetInputOverride("extra foyer delay");
		yield return new WaitForSeconds(d);
		p.ClearInputOverride("extra foyer delay");
	}

	public void PlayerCharacterChanged(PlayerController newCharacter)
	{
		if (OnPlayerCharacterChanged != null)
		{
			OnPlayerCharacterChanged(newCharacter);
		}
	}

	public void ProcessPlayerEnteredFoyer(PlayerController p)
	{
		if ((!Dungeon.ShouldAttemptToLoadFromMidgameSave || !GameManager.Instance.IsLoadingLevel) && (bool)p)
		{
			p.ForceStaticFaceDirection(Vector2.up);
			if (p.characterIdentity != PlayableCharacters.Eevee)
			{
				p.SetOverrideShader(ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTiltedCutout"));
			}
			if (p.CurrentGun != null)
			{
				p.CurrentGun.gameObject.SetActive(false);
			}
			if (p.inventory != null)
			{
				p.inventory.ForceNoGun = true;
			}
			p.ProcessHandAttachment();
		}
	}

	public void OnDepartedFoyer()
	{
		GameManager.Instance.IsFoyer = false;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].inventory.ForceNoGun = false;
			GameManager.Instance.AllPlayers[i].CurrentGun.gameObject.SetActive(true);
			GameManager.Instance.AllPlayers[i].ProcessHandAttachment();
			GameManager.Instance.AllPlayers[i].ClearOverrideShader();
			GameManager.Instance.AllPlayers[i].AlternateCostumeLibrary = null;
		}
	}

	private void PlacePlayerAtStart(PlayerController extantPlayer, Vector2 spot)
	{
		Vector3 position = new Vector3(spot.x + 0.5f, spot.y + 0.5f, -0.1f);
		extantPlayer.transform.position = position;
		extantPlayer.Reinitialize();
	}

	private void FlagPitSRBsAsUnpathableCells()
	{
		RoomHandler entrance = GameManager.Instance.Dungeon.data.Entrance;
		for (int i = entrance.area.basePosition.x; i < entrance.area.basePosition.x + entrance.area.dimensions.x; i++)
		{
			for (int j = entrance.area.basePosition.y; j < entrance.area.basePosition.y + entrance.area.dimensions.y; j++)
			{
				for (int k = 0; k < DebrisObject.SRB_Pits.Count; k++)
				{
					Vector2 point = new Vector2((float)i + 0.5f, (float)j + 0.5f);
					if (DebrisObject.SRB_Pits[k].ContainsPoint(point, int.MaxValue, true))
					{
						GameManager.Instance.Dungeon.data[i, j].isOccupied = true;
					}
				}
				for (int l = 0; l < DebrisObject.SRB_Walls.Count; l++)
				{
					Vector2 point2 = new Vector2((float)i + 0.5f, (float)j + 0.5f);
					if (DebrisObject.SRB_Walls[l].ContainsPoint(point2, int.MaxValue, true))
					{
						GameManager.Instance.Dungeon.data[i, j].isOccupied = true;
					}
				}
			}
		}
	}
}
