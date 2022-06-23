using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
	public dfButton ExitToMainMenuButton;

	public dfButton ReturnToGameButton;

	public dfButton BestiaryButton;

	public dfButton QuickRestartButton;

	public dfButton QuitGameButton;

	public dfButton OptionsButton;

	public dfTextureSprite PauseBGSprite;

	public FullOptionsMenuController OptionsMenu;

	public List<GameObject> AdditionalMenuElementsToClear;

	public dfPanel metaCurrencyPanel;

	public AnimationCurve ShwoopInCurve;

	public AnimationCurve ShwoopOutCurve;

	public float DelayDFAnimatorsTime = 0.3f;

	private dfPanel m_panel;

	private bool m_buttonsOffsetForDoubleHeight;

	private const float c_FrenchVertOffsetUp = 18f;

	private const float c_FrenchVertOffsetDown = 24f;

	private void Start()
	{
		m_panel = GetComponent<dfPanel>();
		AdditionalMenuElementsToClear = new List<GameObject>();
		m_panel.IsVisibleChanged += OnVisibilityChanged;
		ExitToMainMenuButton.Click += DoExitToMainMenu;
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
		{
			ExitToMainMenuButton.Text = "#EXIT_COOP";
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER)
			{
				ExitToMainMenuButton.Disable();
				UIKeyControls component = ExitToMainMenuButton.GetComponent<UIKeyControls>();
				component.up.GetComponent<UIKeyControls>().down = component.down;
				component.down.GetComponent<UIKeyControls>().up = component.up;
			}
		}
		ReturnToGameButton.Click += DoReturnToGame;
		BestiaryButton.Click += DoShowBestiary;
		QuitGameButton.Click += DoQuitGameEntirely;
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.TUTORIAL || GameManager.Instance.CurrentGameMode == GameManager.GameMode.SUPERBOSSRUSH)
		{
			QuickRestartButton.Disable();
			UIKeyControls component2 = QuickRestartButton.GetComponent<UIKeyControls>();
			component2.up.GetComponent<UIKeyControls>().down = component2.down;
			component2.down.GetComponent<UIKeyControls>().up = component2.up;
		}
		else
		{
			QuickRestartButton.Click += DoQuickRestart;
		}
		OptionsButton.Click += DoShowOptions;
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
		{
			metaCurrencyPanel.IsVisible = true;
			return;
		}
		GameUIRoot.Instance.AddControlToMotionGroups(metaCurrencyPanel, DungeonData.Direction.EAST, true);
		GameUIRoot.Instance.MoveNonCoreGroupOnscreen(metaCurrencyPanel, true);
	}

	private void OnVisibilityChanged(dfControl control, bool value)
	{
		if (!value)
		{
			return;
		}
		if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.FRENCH)
		{
			BestiaryButton.Text = BestiaryButton.ForceGetLocalizedValue("#AMMONOMICON");
			BestiaryButton.ModifyLocalizedText(BestiaryButton.Text.Replace(" ", "\n"));
			BestiaryButton.AutoSize = false;
			BestiaryButton.TextAlignment = TextAlignment.Center;
			BestiaryButton.Width = Mathf.Max(240f, BestiaryButton.Width);
			if (!m_buttonsOffsetForDoubleHeight)
			{
				m_buttonsOffsetForDoubleHeight = true;
				BestiaryButton.RelativePosition -= new Vector3(0f, 18f, 0f);
				ReturnToGameButton.RelativePosition -= new Vector3(0f, 18f, 0f);
				OptionsButton.RelativePosition += new Vector3(0f, 24f, 0f);
				ExitToMainMenuButton.RelativePosition += new Vector3(0f, 24f, 0f);
				QuickRestartButton.RelativePosition += new Vector3(0f, 24f, 0f);
				QuitGameButton.RelativePosition += new Vector3(0f, 24f, 0f);
			}
		}
		else if (m_buttonsOffsetForDoubleHeight)
		{
			BestiaryButton.Text = BestiaryButton.ForceGetLocalizedValue("#AMMONOMICON");
			BestiaryButton.AutoSize = true;
			BestiaryButton.TextAlignment = TextAlignment.Left;
			m_buttonsOffsetForDoubleHeight = false;
			BestiaryButton.RelativePosition += new Vector3(0f, 18f, 0f);
			ReturnToGameButton.RelativePosition += new Vector3(0f, 18f, 0f);
			OptionsButton.RelativePosition -= new Vector3(0f, 24f, 0f);
			ExitToMainMenuButton.RelativePosition -= new Vector3(0f, 24f, 0f);
			QuickRestartButton.RelativePosition -= new Vector3(0f, 24f, 0f);
			QuitGameButton.RelativePosition -= new Vector3(0f, 24f, 0f);
		}
	}

	private void RemoveQuitButtonAndRealignVertically()
	{
		QuitGameButton.Disable();
		Object.Destroy(QuitGameButton.gameObject);
		ReturnToGameButton.RelativePosition += new Vector3(0f, 9f, 0f);
		BestiaryButton.RelativePosition += new Vector3(0f, 12f, 0f);
		OptionsButton.RelativePosition += new Vector3(0f, 15f, 0f);
		QuickRestartButton.RelativePosition += new Vector3(0f, 21f, 0f);
		ExitToMainMenuButton.RelativePosition += new Vector3(0f, 24f, 0f);
	}

	public void ForceRevealMetaCurrencyPanel()
	{
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
		{
			GameUIRoot.Instance.MoveNonCoreGroupOnscreen(metaCurrencyPanel, true);
		}
	}

	public void ForceHideMetaCurrencyPanel()
	{
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
		{
			GameUIRoot.Instance.AddControlToMotionGroups(metaCurrencyPanel, DungeonData.Direction.EAST, true);
			GameUIRoot.Instance.MoveNonCoreGroupOnscreen(metaCurrencyPanel);
		}
	}

	public void ToggleExitCoopButtonOnCoopChange()
	{
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER)
		{
			ExitToMainMenuButton.Disable();
			UIKeyControls component = ExitToMainMenuButton.GetComponent<UIKeyControls>();
			component.up.GetComponent<UIKeyControls>().down = component.down;
			component.down.GetComponent<UIKeyControls>().up = component.up;
			return;
		}
		ExitToMainMenuButton.Enable();
		UIKeyControls component2 = ExitToMainMenuButton.GetComponent<UIKeyControls>();
		if ((bool)component2.up && !component2.up.IsEnabled)
		{
			component2.up = component2.up.GetComponent<UIKeyControls>().up;
		}
		if ((bool)component2.up)
		{
			component2.up.GetComponent<UIKeyControls>().down = ExitToMainMenuButton;
		}
		if ((bool)component2.down)
		{
			component2.down.GetComponent<UIKeyControls>().up = ExitToMainMenuButton;
		}
	}

	public void ToggleVisibility(bool value)
	{
		if (value)
		{
			m_panel.IsVisible = value;
			PauseBGSprite.Parent.IsVisible = value;
		}
		else
		{
			m_panel.IsVisible = value;
			PauseBGSprite.Parent.IsVisible = value;
		}
	}

	private void HandleVisibilityChange(dfControl control, bool value)
	{
	}

	private void DoQuickRestart(dfControl control, dfMouseEventArgs mouseEvent)
	{
		StartCoroutine(HandleQuickRestart());
	}

	private IEnumerator HandleQuickRestart()
	{
		GameUIRoot.Instance.DoAreYouSure("#AYS_QUICKRESTART");
		ToggleVisibility(false);
		while (!GameUIRoot.Instance.HasSelectedAreYouSureOption())
		{
			yield return null;
		}
		if (GameUIRoot.Instance.GetAreYouSureOption())
		{
			if ((bool)GameManager.LastUsedPlayerPrefab && GameManager.LastUsedPlayerPrefab.GetComponent<PlayerController>().characterIdentity == PlayableCharacters.Gunslinger && !GameStatsManager.Instance.GetFlag(GungeonFlags.GUNSLINGER_UNLOCKED))
			{
				GameManager.LastUsedPlayerPrefab = (GameObject)ResourceCache.Acquire("PlayerEevee");
			}
			QuickRestartOptions qrOptions = AmmonomiconDeathPageController.GetNumMetasToQuickRestart();
			if (qrOptions.NumMetas > 0)
			{
				GameUIRoot.Instance.CheckKeepModifiersQuickRestart(qrOptions.NumMetas);
				while (!GameUIRoot.Instance.HasSelectedAreYouSureOption())
				{
					yield return null;
				}
				if (!GameUIRoot.Instance.GetAreYouSureOption())
				{
					qrOptions = default(QuickRestartOptions);
					if ((bool)GameManager.LastUsedPlayerPrefab && (GameManager.LastUsedPlayerPrefab.GetComponent<PlayerController>().characterIdentity == PlayableCharacters.Eevee || GameManager.LastUsedPlayerPrefab.GetComponent<PlayerController>().characterIdentity == PlayableCharacters.Gunslinger))
					{
						GameManager.LastUsedPlayerPrefab = (GameObject)ResourceCache.Acquire(CharacterSelectController.GetCharacterPathFromQuickStart());
					}
				}
			}
			GameUIRoot.Instance.ToggleUICamera(false);
			if ((bool)GameManager.Instance.Dungeon && GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.CASTLEGEON)
			{
				GameStatsManager.Instance.isChump = true;
			}
			GameManager.Instance.DelayedQuickRestart(0.05f, qrOptions);
		}
		else
		{
			ToggleVisibility(true);
			QuickRestartButton.Focus();
		}
	}

	private IEnumerator HandleCloseGameEntirely()
	{
		GameUIRoot.Instance.DoAreYouSure("#AYS_QUITTODESKTOP");
		ToggleVisibility(false);
		while (!GameUIRoot.Instance.HasSelectedAreYouSureOption())
		{
			yield return null;
		}
		if (GameUIRoot.Instance.GetAreYouSureOption())
		{
			Application.Quit();
			yield break;
		}
		ToggleVisibility(true);
		QuitGameButton.Focus();
	}

	public void ToggleBG(dfButton target)
	{
		if (StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
		{
			target.BackgroundSprite = string.Empty;
			target.Padding = new RectOffset(0, 0, 0, 0);
			return;
		}
		target.BackgroundSprite = "chamber_flash_small_001";
		target.Padding = new RectOffset(6, 6, 0, 0);
		target.NormalBackgroundColor = Color.black;
		target.FocusBackgroundColor = Color.black;
		target.HoverBackgroundColor = Color.black;
		target.DisabledColor = Color.black;
		target.PressedBackgroundColor = Color.black;
	}

	public void HandleBGs()
	{
		ToggleBG(OptionsButton);
		ToggleBG(QuickRestartButton);
		ToggleBG(ReturnToGameButton);
		ToggleBG(BestiaryButton);
		ToggleBG(ExitToMainMenuButton);
		if ((bool)QuitGameButton)
		{
			ToggleBG(QuitGameButton);
		}
	}

	public void ShwoopOpen()
	{
		float num = ((!PunchoutController.IsActive || !PunchoutController.OverrideControlsButton) ? 1 : 4);
		HandleBGs();
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER)
		{
			metaCurrencyPanel.IsVisible = true;
			if ((bool)metaCurrencyPanel && (bool)metaCurrencyPanel.Parent && (bool)metaCurrencyPanel.Parent.Parent)
			{
				metaCurrencyPanel.Parent.Parent.BringToFront();
			}
			GameUIRoot.Instance.MoveNonCoreGroupOnscreen(metaCurrencyPanel);
		}
		ForceMaterialInvisibility();
		StartCoroutine(DelayTriggerAnimators(num));
		StartCoroutine(HandleBlockReveal(false, num));
		StartCoroutine(HandleShwoop(false, num));
		if (m_panel.ZOrder < PauseBGSprite.Parent.ZOrder)
		{
			m_panel.ZOrder = PauseBGSprite.Parent.ZOrder + 1;
		}
	}

	private Material GrabBGRenderMaterial()
	{
		Material result = PauseBGSprite.RenderMaterial;
		for (int i = 0; i < PauseBGSprite.GUIManager.MeshRenderer.sharedMaterials.Length; i++)
		{
			Material material = PauseBGSprite.GUIManager.MeshRenderer.sharedMaterials[i];
			if (material != null && material.shader != null && material.shader.name.Contains("MaskReveal"))
			{
				result = material;
				break;
			}
		}
		return result;
	}

	private IEnumerator DelayTriggerAnimators(float timeMultiplier = 1f)
	{
		float ela = 0f;
		while (ela < DelayDFAnimatorsTime)
		{
			ela += GameManager.INVARIANT_DELTA_TIME * timeMultiplier;
			yield return null;
		}
		dfSprite[] childSprites = PauseBGSprite.GetComponentsInChildren<dfSprite>();
		for (int i = 0; i < childSprites.Length; i++)
		{
			childSprites[i].IsVisible = true;
			childSprites[i].GetComponent<dfSpriteAnimation>().Play();
		}
	}

	private IEnumerator HandleBlockReveal(bool reverse, float timeMultiplier = 1f)
	{
		float timer = 0.3f;
		float elapsed = 0f;
		if (reverse)
		{
			timer = 0.075f;
		}
		if (reverse)
		{
			dfSprite[] componentsInChildren = PauseBGSprite.GetComponentsInChildren<dfSprite>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].IsVisible = false;
				componentsInChildren[i].GetComponent<dfSpriteAnimation>().Stop();
			}
		}
		while (elapsed < timer)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME * timeMultiplier;
			float t = Mathf.Clamp01(elapsed / timer);
			if (reverse)
			{
				t = 1f - t;
			}
			Material targetMaterial = GrabBGRenderMaterial();
			if (PauseBGSprite.Material != null)
			{
				PauseBGSprite.Material.SetFloat("_RevealPercent", t);
			}
			if (PauseBGSprite.RenderMaterial != null)
			{
				PauseBGSprite.RenderMaterial.SetFloat("_RevealPercent", t);
			}
			if (targetMaterial != null)
			{
				targetMaterial.SetFloat("_RevealPercent", t);
			}
			yield return null;
			if (!reverse)
			{
				PauseBGSprite.Parent.IsVisible = true;
			}
		}
		if (reverse)
		{
			ForceMaterialInvisibility();
		}
		else
		{
			ForceMaterialVisibility();
		}
		PauseBGSprite.Parent.IsVisible = m_panel.IsVisible;
		if (PunchoutController.IsActive && PunchoutController.OverrideControlsButton && !reverse)
		{
			Debug.Log("aaa visibility");
			ToggleVisibility(false);
			Debug.Log("aaa MakeVisibleWithoutAnim");
			OptionsMenu.PreOptionsMenu.MakeVisibleWithoutAnim();
			Debug.Log("aaa ToggleToPanel");
			OptionsMenu.PreOptionsMenu.ToggleToPanel(OptionsMenu.TabControls, false, true);
			Debug.Log("aaa ToggleToKeyboardBindingsPanel");
			FullOptionsMenuController.CurrentBindingPlayerTargetIndex = 0;
			OptionsMenu.ToggleToKeyboardBindingsPanel(!BraveInput.PrimaryPlayerInstance.IsKeyboardAndMouse());
		}
	}

	private IEnumerator HandleShwoop(bool reverse, float timeMultlier = 1f)
	{
		float timer = 0.1f;
		float elapsed = 0f;
		if (reverse)
		{
			timer = 0.075f;
		}
		Vector3 smallScale = new Vector3(0.01f, 0.01f, 1f);
		Vector3 bigScale = Vector3.one;
		while (elapsed < timer)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME * timeMultlier;
			float t = Mathf.Clamp01(elapsed / timer);
			AnimationCurve targetCurve = ((!reverse) ? ShwoopInCurve : ShwoopOutCurve);
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
			m_panel.IsInteractive = false;
			m_panel.IsEnabled = false;
		}
	}

	public void ShwoopClosed()
	{
		StartCoroutine(HandleShwoop(true));
		StartCoroutine(HandleBlockReveal(true));
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER)
		{
			GameUIRoot.Instance.MoveNonCoreGroupOnscreen(metaCurrencyPanel, true);
		}
	}

	private void DoQuitGameEntirely(dfControl control, dfMouseEventArgs mouseEvent)
	{
		StartCoroutine(HandleCloseGameEntirely());
	}

	public void ForceMaterialInvisibility()
	{
		Material material = GrabBGRenderMaterial();
		if (PauseBGSprite.Material != null)
		{
			PauseBGSprite.Material.SetFloat("_RevealPercent", 0f);
		}
		if (PauseBGSprite.RenderMaterial != null)
		{
			PauseBGSprite.RenderMaterial.SetFloat("_RevealPercent", 0f);
		}
		if (material != null)
		{
			material.SetFloat("_RevealPercent", 0f);
		}
	}

	public void ForceMaterialVisibility()
	{
		Material material = GrabBGRenderMaterial();
		if (PauseBGSprite.Material != null)
		{
			PauseBGSprite.Material.SetFloat("_RevealPercent", 1f);
		}
		if (PauseBGSprite.RenderMaterial != null)
		{
			PauseBGSprite.RenderMaterial.SetFloat("_RevealPercent", 1f);
		}
		if (material != null)
		{
			material.SetFloat("_RevealPercent", 1f);
		}
	}

	public void DoShowBestiaryToTarget(EncounterTrackable target)
	{
		ToggleVisibility(false);
		if (dfGUIManager.GetModalControl() != null)
		{
			dfGUIManager.PopModal();
		}
		AmmonomiconController.Instance.OpenAmmonomiconToTrackable(target);
	}

	public void DoShowBestiary(dfControl control, dfMouseEventArgs mouseEvent)
	{
		if (!AmmonomiconController.Instance.IsClosing && !AmmonomiconController.Instance.IsOpening)
		{
			ToggleVisibility(false);
			if (dfGUIManager.GetModalControl() != null)
			{
				dfGUIManager.PopModal();
			}
			AmmonomiconController.Instance.OpenAmmonomicon(false, false);
		}
	}

	private void DoReturnToGame(dfControl control, dfMouseEventArgs mouseEvent)
	{
		if (!GameManager.Instance.IsLoadingLevel)
		{
			GameManager.Instance.Unpause();
		}
	}

	private void DoShowOptions(dfControl control, dfMouseEventArgs mouseEvent)
	{
		if (!GameManager.Instance.IsLoadingLevel)
		{
			ToggleVisibility(false);
			OptionsMenu.PreOptionsMenu.IsVisible = true;
		}
	}

	private void DoExitToMainMenu(dfControl control, dfMouseEventArgs mouseEvent)
	{
		StartCoroutine(HandleExitToMainMenu());
	}

	private IEnumerator HandleExitToMainMenu()
	{
		if (GameManager.Instance.IsLoadingLevel)
		{
			yield break;
		}
		GameUIRoot.Instance.DoAreYouSure("#AREYOUSURE");
		ToggleVisibility(false);
		while (!GameUIRoot.Instance.HasSelectedAreYouSureOption())
		{
			yield return null;
		}
		if (GameUIRoot.Instance.GetAreYouSureOption())
		{
			GameUIRoot.Instance.ToggleUICamera(false);
			if (GameManager.Instance.IsFoyer)
			{
				Foyer.Instance.OnDepartedFoyer();
			}
			else
			{
				SaveManager.DeleteCurrentSlotMidGameSave();
			}
			Pixelator.Instance.FadeToBlack(0.15f);
			GameManager.Instance.DelayedLoadCharacterSelect(0.15f, true);
		}
		else
		{
			ToggleVisibility(true);
			ExitToMainMenuButton.Focus();
		}
	}

	public void SetDefaultFocus()
	{
		ReturnToGameButton.Focus();
	}

	public void RevertToBaseState()
	{
		HandleBGs();
		ToggleVisibility(true);
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER)
		{
			metaCurrencyPanel.IsVisible = m_panel.IsVisible;
		}
		m_panel.IsInteractive = true;
		m_panel.IsEnabled = true;
		OptionsMenu.IsVisible = false;
		OptionsMenu.PreOptionsMenu.IsVisible = false;
		for (int i = 0; i < AdditionalMenuElementsToClear.Count; i++)
		{
			Object.Destroy(AdditionalMenuElementsToClear[i]);
		}
		AdditionalMenuElementsToClear.Clear();
		dfGUIManager.PopModalToControl(m_panel, false);
		ForceMaterialVisibility();
		SetDefaultFocus();
		AkSoundEngine.PostEvent("Play_UI_menu_back_01", base.gameObject);
	}
}
