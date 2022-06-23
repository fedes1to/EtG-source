using System;
using System.Collections;
using InControl;
using UnityEngine;

public class AmmonomiconDeathPageController : MonoBehaviour, ILevelLoadedListener
{
	public static bool LastKilledPlayerPrimary = true;

	public bool isRightPage;

	public bool isVictoryPage;

	[HideInInspectorIf("isRightPage", false)]
	public dfLabel youDiedLabel;

	[HideInInspectorIf("isRightPage", false)]
	public dfLabel gungeoneerTitle;

	[HideInInspectorIf("isRightPage", false)]
	public dfLabel areaTitle;

	[HideInInspectorIf("isRightPage", false)]
	public dfLabel timeTitle;

	[HideInInspectorIf("isRightPage", false)]
	public dfLabel moneyTitle;

	[HideInInspectorIf("isRightPage", false)]
	public dfLabel killsTitle;

	[HideInInspectorIf("isRightPage", false)]
	public dfLabel gungeoneerLabel;

	[HideInInspectorIf("isRightPage", false)]
	public dfLabel areaLabel;

	[HideInInspectorIf("isRightPage", false)]
	public dfLabel timeLabel;

	[HideInInspectorIf("isRightPage", false)]
	public dfLabel moneyLabel;

	[HideInInspectorIf("isRightPage", false)]
	public dfLabel killsLabel;

	[HideInInspectorIf("isRightPage", false)]
	public dfLabel deathNumberLabel;

	[HideInInspectorIf("isRightPage", false)]
	public dfLabel coopDeathNumberLabel;

	[HideInInspectorIf("isRightPage", false)]
	public dfLabel coopIndividualDeathsNumberLabel;

	[HideInInspectorIf("isRightPage", false)]
	public dfLabel hauntingLabel;

	[HideInInspectorIf("isRightPage", false)]
	public dfButton quickRestartButton;

	[HideInInspectorIf("isRightPage", false)]
	public dfButton mainMenuButton;

	[ShowInInspectorIf("isRightPage", false)]
	public dfLabel killedByHeaderLabel;

	[ShowInInspectorIf("isRightPage", false)]
	public dfLabel killedByLabel;

	[ShowInInspectorIf("isRightPage", false)]
	public dfTextureSprite photoSprite;

	[ShowInInspectorIf("isRightPage", false)]
	public dfSprite ChallengeModeRibbon;

	[ShowInInspectorIf("isRightPage", false)]
	public dfSprite RatDeathDrawings;

	private Vector4 m_cachedUVRescale;

	private RenderTexture m_temporaryPhotoTex;

	private bool m_doingSomething;

	public void DoInitialize()
	{
		m_doingSomething = false;
		if (isRightPage)
		{
			InitializeRightPage();
		}
		else
		{
			InitializeLeftPage();
		}
	}

	private string GetDeathPortraitName()
	{
		switch (GameManager.Instance.PrimaryPlayer.characterIdentity)
		{
		case PlayableCharacters.Convict:
			return "coop_page_death_jailbird_001";
		case PlayableCharacters.Cosmonaut:
			return "coop_page_death_cultist_001";
		case PlayableCharacters.Guide:
			return "coop_page_death_scholar_001";
		case PlayableCharacters.Ninja:
			return "coop_page_death_ninja_001";
		case PlayableCharacters.Pilot:
			return "coop_page_death_rogue_001";
		case PlayableCharacters.Robot:
			return "coop_page_death_robot_001";
		case PlayableCharacters.Soldier:
			return "coop_page_death_marine_001";
		case PlayableCharacters.Bullet:
			return "coop_page_death_bullet_001";
		case PlayableCharacters.Eevee:
			return "coop_page_death_eevee_001";
		case PlayableCharacters.Gunslinger:
			return "coop_page_death_slinger_001";
		default:
			return "coop_page_death_cultist_001";
		}
	}

	private string GetStringKeyForCharacter(PlayableCharacters identity)
	{
		switch (identity)
		{
		case PlayableCharacters.Convict:
			return "#CHAR_CONVICT_SHORT";
		case PlayableCharacters.Guide:
			return "#CHAR_GUIDE_SHORT";
		case PlayableCharacters.Soldier:
			return "#CHAR_MARINE_SHORT";
		case PlayableCharacters.Pilot:
			return "#CHAR_ROGUE_SHORT";
		case PlayableCharacters.CoopCultist:
			return "#CHAR_CULTIST_SHORT";
		case PlayableCharacters.Robot:
			return "#CHAR_ROBOT_SHORT";
		case PlayableCharacters.Bullet:
			return "#CHAR_BULLET_SHORT";
		case PlayableCharacters.Eevee:
			return "#CHAR_PARADOX_SHORT";
		case PlayableCharacters.Gunslinger:
			return "#CHAR_GUNSLINGER_SHORT";
		default:
			return "#CHAR_CULTIST_SHORT";
		}
	}

	public void UpdateWidth(dfLabel target, int min = -1)
	{
		dfSlicedSprite componentInChildren = target.Parent.GetComponentInChildren<dfSlicedSprite>();
		if ((bool)componentInChildren)
		{
			componentInChildren.Width = Mathf.CeilToInt(target.Width / 4f) + 5;
			if (min > 0)
			{
				componentInChildren.Width = Mathf.Max(min, componentInChildren.Width);
			}
		}
	}

	public void ToggleBG(dfLabel target)
	{
		if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
		{
			target.BackgroundSprite = string.Empty;
			target.Padding = new RectOffset(0, 0, 0, 0);
		}
		else
		{
			target.BackgroundSprite = "chamber_flash_small_001";
			target.Padding = new RectOffset(6, 6, 0, 0);
			target.BackgroundColor = new Color(0.1764706f, 10f / 51f, 26f / 85f);
		}
	}

	private void InitializeLeftPage()
	{
		if (isVictoryPage)
		{
			youDiedLabel.Text = youDiedLabel.ForceGetLocalizedValue("#DEATH_YOUWON");
			dfLabel[] componentsInChildren = youDiedLabel.GetComponentsInChildren<dfLabel>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Text = youDiedLabel.ForceGetLocalizedValue("#DEATH_YOUWON");
			}
		}
		else
		{
			youDiedLabel.Text = youDiedLabel.ForceGetLocalizedValue("#DEATH_YOUDIED");
			dfLabel[] componentsInChildren2 = youDiedLabel.GetComponentsInChildren<dfLabel>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				componentsInChildren2[j].Text = youDiedLabel.ForceGetLocalizedValue("#DEATH_YOUDIED");
			}
		}
		string text = hauntingLabel.ForceGetLocalizedValue("#DEATH_PASTKILLED");
		string text2 = hauntingLabel.ForceGetLocalizedValue("#DEATH_PASTHAUNTS");
		string text3 = hauntingLabel.ForceGetLocalizedValue("#DEATH_HELLCLEARED");
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.BOSSKILLED_LICH))
		{
			hauntingLabel.Text = text3;
		}
		else if (GameStatsManager.Instance.GetCharacterSpecificFlag(GameManager.Instance.PrimaryPlayer.characterIdentity, CharacterSpecificGungeonFlags.KILLED_PAST))
		{
			hauntingLabel.Text = text;
		}
		else
		{
			hauntingLabel.Text = text2;
		}
		UpdateWidth(hauntingLabel, 142);
		hauntingLabel.PerformLayout();
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			deathNumberLabel.Parent.Parent.IsVisible = false;
			coopDeathNumberLabel.Parent.Parent.IsVisible = true;
			coopDeathNumberLabel.Text = GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.NUMBER_DEATHS).ToString();
			coopIndividualDeathsNumberLabel.Text = "[sprite \"" + GetDeathPortraitName() + "\"] x" + GameManager.Instance.PrimaryPlayer.DeathsThisRun + " [sprite \"coop_page_death_cultist_001\"] x" + GameManager.Instance.SecondaryPlayer.DeathsThisRun;
		}
		else
		{
			deathNumberLabel.Parent.Parent.IsVisible = true;
			coopDeathNumberLabel.Parent.Parent.IsVisible = false;
			deathNumberLabel.Text = GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.NUMBER_DEATHS).ToString();
		}
		UpdateWidth(gungeoneerTitle);
		UpdateWidth(killsTitle);
		UpdateWidth(areaTitle);
		UpdateWidth(timeTitle);
		UpdateWidth(moneyTitle);
		gungeoneerLabel.Text = gungeoneerLabel.ForceGetLocalizedValue(GetStringKeyForCharacter(GameManager.Instance.PrimaryPlayer.characterIdentity));
		areaLabel.Text = areaLabel.ForceGetLocalizedValue(GameManager.Instance.Dungeon.DungeonShortName);
		int seconds = Mathf.FloorToInt(GameStatsManager.Instance.GetSessionStatValue(TrackedStats.TIME_PLAYED));
		if (GameManager.Options.SpeedrunMode)
		{
			int milliseconds = Mathf.FloorToInt(1000f * (GameStatsManager.Instance.GetSessionStatValue(TrackedStats.TIME_PLAYED) % 1f));
			TimeSpan timeSpan = new TimeSpan(0, 0, 0, seconds, milliseconds);
			string text4 = string.Format("{0:0}:{1:00}:{2:00}.{3:000}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
			timeLabel.Text = text4;
		}
		else
		{
			TimeSpan timeSpan2 = new TimeSpan(0, 0, seconds);
			string text5 = string.Format("{0:0}:{1:00}:{2:00}", timeSpan2.Hours, timeSpan2.Minutes, timeSpan2.Seconds);
			timeLabel.Text = text5;
		}
		moneyLabel.Text = GameStatsManager.Instance.GetSessionStatValue(TrackedStats.TOTAL_MONEY_COLLECTED).ToString();
		killsLabel.Text = string.Empty;
		killsLabel.Parent.Width = 30f;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			killsLabel.ProcessMarkup = true;
			killsLabel.ColorizeSymbols = true;
			if (GameManager.Instance.PrimaryPlayer.KillsThisRun > 99 && GameManager.Instance.SecondaryPlayer.KillsThisRun > 99)
			{
				killsLabel.TabSize = 2;
			}
			else
			{
				killsLabel.TabSize = 4;
			}
			killsLabel.Text = "[sprite \"" + GetDeathPortraitName() + "\"]\t" + GameManager.Instance.PrimaryPlayer.KillsThisRun + "\t[sprite \"coop_page_death_cultist_001\"]\t" + GameManager.Instance.SecondaryPlayer.KillsThisRun;
		}
		else
		{
			killsLabel.Text = GameStatsManager.Instance.GetSessionStatValue(TrackedStats.ENEMIES_KILLED).ToString();
		}
		UpdateTapeLabel(gungeoneerLabel);
		UpdateTapeLabel(areaLabel);
		UpdateTapeLabel(timeLabel);
		UpdateTapeLabel(moneyLabel);
		killsLabel.PerformLayout();
		UpdateTapeLabel(killsLabel, killsLabel.GetAutosizeWidth());
		string text6 = quickRestartButton.ForceGetLocalizedValue("#DEATH_QUICKRESTART");
		string text7 = mainMenuButton.ForceGetLocalizedValue("#DEATH_MAINMENU");
		if (!text6.StartsWith(" "))
		{
			text6 = " " + text6;
		}
		if (!text7.StartsWith(" "))
		{
			text7 = " " + text7;
		}
		if (BraveInput.PrimaryPlayerInstance.IsKeyboardAndMouse())
		{
			text6 = "[sprite \"space_bar_up_001\"" + text6;
			text7 = "[sprite \"esc_up_001\"" + text7;
		}
		else
		{
			text6 = UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.Action1, BraveInput.PlayerOneCurrentSymbology) + text6;
			text7 = UIControllerButtonHelper.GetUnifiedControllerButtonTag(InputControlType.Action2, BraveInput.PlayerOneCurrentSymbology) + text7;
		}
		quickRestartButton.Text = text6;
		mainMenuButton.Text = text7;
		quickRestartButton.Click += DoQuickRestart;
		mainMenuButton.Click += DoMainMenu;
		quickRestartButton.Focus();
		if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.SUPERBOSSRUSH)
		{
			quickRestartButton.IsVisible = false;
			mainMenuButton.Focus();
		}
		else
		{
			quickRestartButton.IsVisible = true;
			quickRestartButton.Focus();
		}
	}

	private void OnDestroy()
	{
		if ((bool)m_temporaryPhotoTex)
		{
			RenderTexture.ReleaseTemporary(m_temporaryPhotoTex);
			m_temporaryPhotoTex = null;
		}
	}

	private void InitializeRightPage()
	{
		if ((bool)ChallengeModeRibbon)
		{
			ChallengeModeRibbon.IsVisible = false;
		}
		if (isVictoryPage)
		{
			if ((bool)ChallengeModeRibbon && ChallengeManager.CHALLENGE_MODE_ACTIVE)
			{
				ChallengeModeRibbon.IsVisible = true;
				ChallengeModeRibbon.RelativePosition += new Vector3(-200f, -68f, 0f);
			}
			killedByLabel.Glitchy = false;
			killedByLabel.Text = killedByLabel.ForceGetLocalizedValue("#DEATH_NOBODY");
			UpdateTapeLabel(killedByLabel);
			UpdateWidth(killedByHeaderLabel);
			SetWinPic();
			return;
		}
		string text = ((!LastKilledPlayerPrimary) ? GameManager.Instance.SecondaryPlayer.healthHaver.lastIncurredDamageSource : GameManager.Instance.PrimaryPlayer.healthHaver.lastIncurredDamageSource);
		if (string.IsNullOrEmpty(text))
		{
			text = StringTableManager.GetEnemiesString("#KILLEDBYDEFAULT");
		}
		killedByLabel.Text = string.Empty;
		killedByLabel.Parent.Width = 30f;
		killedByLabel.Glitchy = false;
		if (GameManager.Instance.Dungeon.IsGlitchDungeon)
		{
			killedByLabel.Glitchy = true;
		}
		if (text == "primaryplayer" || text == "secondaryplayer")
		{
			text = StringTableManager.GetEnemiesString("#KILLEDBYDEFAULT");
		}
		if ((bool)RatDeathDrawings)
		{
			RatDeathDrawings.IsVisible = false;
		}
		if (text == StringTableManager.GetEnemiesString("#RESOURCEFULRAT_ENCNAME") || text == StringTableManager.GetEnemiesString("#METALGEARRAT_ENCNAME"))
		{
			if ((bool)RatDeathDrawings)
			{
				RatDeathDrawings.IsVisible = true;
			}
			text = StringTableManager.GetEnemiesString("#KILLEDBY_RESOURCEFULRAT");
		}
		killedByLabel.Text = text;
		killedByLabel.PerformLayout();
		UpdateTapeLabel(killedByLabel, killedByLabel.GetAutosizeWidth());
		UpdateWidth(killedByHeaderLabel);
		if (!(photoSprite != null))
		{
			return;
		}
		float num = photoSprite.Width / photoSprite.Height;
		float num2 = 1.77777779f;
		if (isVictoryPage)
		{
			Texture texture = BraveResources.Load("Win_Pic_Gun_Get_001") as Texture;
			photoSprite.Texture = texture;
			return;
		}
		RenderTexture renderTexture = Pixelator.Instance.GetCachedFrame();
		if (!Mathf.Approximately(1.77777779f, BraveCameraUtility.ASPECT))
		{
			int height = renderTexture.height;
			int num3 = Mathf.RoundToInt((float)height * 1.77777779f);
			RenderTextureDescriptor desc = new RenderTextureDescriptor(num3, height, renderTexture.format, renderTexture.depth);
			RenderTexture temporary = RenderTexture.GetTemporary(desc);
			temporary.filterMode = FilterMode.Point;
			float num4 = (float)renderTexture.width / ((float)num3 * 1f);
			float x = (float)(renderTexture.width - num3) / 2f / (float)renderTexture.width;
			Graphics.Blit(renderTexture, temporary, new Vector2(1f / num4, 1f), new Vector2(x, 0f));
			m_temporaryPhotoTex = temporary;
			renderTexture = temporary;
		}
		renderTexture.filterMode = FilterMode.Point;
		photoSprite.Texture = renderTexture;
		m_cachedUVRescale = new Vector4(0f, 0f, 1f, 1f);
		Vector3 cachedPlayerViewportPoint = Pixelator.Instance.CachedPlayerViewportPoint;
		if (cachedPlayerViewportPoint.x > 0f && cachedPlayerViewportPoint.x < 1f && cachedPlayerViewportPoint.y > 0f && cachedPlayerViewportPoint.y < 1f)
		{
			float num5 = num / num2;
			int num6 = Mathf.RoundToInt(photoSprite.Height / 4f);
			float num7 = (float)num6 / 270f;
			float num8 = num7;
			float num9 = num7;
			if (num > num2)
			{
				num9 = num8 / num5;
			}
			else if (num < num2)
			{
				num8 = num9 * num5;
			}
			Vector2 vector = new Vector2(cachedPlayerViewportPoint.x - num8 / 2f, cachedPlayerViewportPoint.y - num9 / 2f);
			Vector2 vector2 = new Vector2(cachedPlayerViewportPoint.x + num8 / 2f, cachedPlayerViewportPoint.y + num9 / 2f);
			if (vector.x < 0f)
			{
				vector2.x += -1f * vector.x;
				vector.x = 0f;
			}
			if (vector2.x > 1f)
			{
				vector.x -= vector2.x - 1f;
				vector2.x = 1f;
			}
			if (vector.y < 0f)
			{
				vector2.y += -1f * vector.y;
				vector.y = 0f;
			}
			if (vector2.y > 1f)
			{
				vector.y -= vector2.y - 1f;
				vector2.y = 1f;
			}
			m_cachedUVRescale = new Vector4(vector.x, vector.y, vector2.x, vector2.y);
		}
		else
		{
			m_cachedUVRescale = new Vector4(0f, 0f, 1f, 1f / num);
		}
	}

	private bool ShouldUseJunkPic()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			if (!playerController)
			{
				continue;
			}
			for (int j = 0; j < playerController.passiveItems.Count; j++)
			{
				if (playerController.passiveItems[j] is CompanionItem)
				{
					CompanionItem companionItem = playerController.passiveItems[j] as CompanionItem;
					if ((bool)companionItem.ExtantCompanion && (bool)companionItem.ExtantCompanion.GetComponent<SackKnightController>() && companionItem.ExtantCompanion.GetComponent<SackKnightController>().CurrentForm == SackKnightController.SackKnightPhase.ANGELIC_KNIGHT)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	private void SetWinPic()
	{
		if (ShouldUseJunkPic() && GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.FINALGEON)
		{
			switch (GameManager.Instance.PrimaryPlayer.characterIdentity)
			{
			case PlayableCharacters.Convict:
				photoSprite.Texture = BraveResources.Load("win_pic_junkan_convict_001", ".png") as Texture;
				break;
			case PlayableCharacters.Pilot:
				photoSprite.Texture = BraveResources.Load("win_pic_junkan_pilot_001", ".png") as Texture;
				break;
			case PlayableCharacters.Soldier:
				photoSprite.Texture = BraveResources.Load("win_pic_junkan_marine_001", ".png") as Texture;
				break;
			case PlayableCharacters.Guide:
				photoSprite.Texture = BraveResources.Load("win_pic_junkan_hunter_001", ".png") as Texture;
				break;
			case PlayableCharacters.Robot:
				photoSprite.Texture = BraveResources.Load("win_pic_junkan_robot_001", ".png") as Texture;
				break;
			case PlayableCharacters.Bullet:
				photoSprite.Texture = BraveResources.Load("win_pic_junkan_bullet_001", ".png") as Texture;
				break;
			case PlayableCharacters.Eevee:
				photoSprite.Texture = BraveResources.Load("win_pic_junkan_eevee_001", ".png") as Texture;
				break;
			case PlayableCharacters.Gunslinger:
				photoSprite.Texture = BraveResources.Load("win_pic_junkan_slinger_001", ".png") as Texture;
				break;
			default:
				photoSprite.Texture = BraveResources.Load("Win_Pic_Gun_Get_001", ".png") as Texture;
				break;
			}
			return;
		}
		if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.BOSSRUSH)
		{
			photoSprite.Texture = BraveResources.Load("Win_Pic_BossRush_001", ".png") as Texture;
			return;
		}
		switch (GameManager.Instance.Dungeon.tileIndices.tilesetId)
		{
		case GlobalDungeonData.ValidTilesets.FORGEGEON:
			photoSprite.Texture = BraveResources.Load("Win_Pic_Gun_Get_001", ".png") as Texture;
			break;
		case GlobalDungeonData.ValidTilesets.FINALGEON:
			switch (GameManager.Instance.PrimaryPlayer.characterIdentity)
			{
			case PlayableCharacters.Convict:
				photoSprite.Texture = BraveResources.Load("Win_Pic_Convict_001", ".png") as Texture;
				break;
			case PlayableCharacters.Pilot:
				photoSprite.Texture = BraveResources.Load("Win_Pic_Pilot_001", ".png") as Texture;
				break;
			case PlayableCharacters.Soldier:
				photoSprite.Texture = BraveResources.Load("Win_Pic_Marine_001", ".png") as Texture;
				break;
			case PlayableCharacters.Guide:
				photoSprite.Texture = BraveResources.Load("Win_Pic_Hunter_001", ".png") as Texture;
				break;
			case PlayableCharacters.Robot:
				photoSprite.Texture = BraveResources.Load("Win_Pic_Robot_001", ".png") as Texture;
				break;
			case PlayableCharacters.Bullet:
				photoSprite.Texture = BraveResources.Load("Win_Pic_Bullet_001", ".png") as Texture;
				break;
			default:
				photoSprite.Texture = BraveResources.Load("Win_Pic_Gun_Get_001", ".png") as Texture;
				break;
			}
			break;
		case GlobalDungeonData.ValidTilesets.HELLGEON:
			if (GameManager.IsGunslingerPast)
			{
				photoSprite.Texture = BraveResources.Load("Win_Pic_Slinger_001", ".png") as Texture;
			}
			else
			{
				photoSprite.Texture = BraveResources.Load("Win_Pic_Lich_Kill_001", ".png") as Texture;
			}
			break;
		default:
			photoSprite.Texture = BraveResources.Load("Win_Pic_Gun_Get_001", ".png") as Texture;
			break;
		}
	}

	public void BraveOnLevelWasLoaded()
	{
		m_doingSomething = false;
	}

	private void UpdateTapeLabel(dfLabel targetLabel, float overrideWidth = -1f)
	{
		dfPanel dfPanel2 = targetLabel.Parent as dfPanel;
		if (overrideWidth > 0f)
		{
			dfPanel2.Width = overrideWidth.Quantize(4f) + 32f;
		}
		else
		{
			dfPanel2.Width = targetLabel.Width + 32f;
		}
	}

	private void Update()
	{
		if (isRightPage)
		{
			if (!isVictoryPage && photoSprite.RenderMaterial != null)
			{
				photoSprite.RenderMaterial.SetVector("_UVRescale", m_cachedUVRescale);
			}
			return;
		}
		if (!quickRestartButton.HasFocus && !mainMenuButton.HasFocus)
		{
			quickRestartButton.Focus();
		}
		if ((bool)AmmonomiconController.Instance && !AmmonomiconController.Instance.HandlingQueuedUnlocks && !AmmonomiconController.Instance.IsOpening && !AmmonomiconController.Instance.IsClosing)
		{
			if (Input.GetKeyDown(KeyCode.Escape) || (!BraveInput.PrimaryPlayerInstance.IsKeyboardAndMouse() && BraveInput.WasCancelPressed()))
			{
				DoMainMenu(null, null);
			}
			else if (Input.GetKeyDown(KeyCode.Space) || (!BraveInput.PrimaryPlayerInstance.IsKeyboardAndMouse() && BraveInput.WasSelectPressed()))
			{
				DoQuickRestart(null, null);
			}
		}
	}

	private void DoMainMenu(dfControl control, dfMouseEventArgs mouseEvent)
	{
		if (!AmmonomiconController.Instance.IsOpening && !AmmonomiconController.Instance.IsClosing)
		{
			SaveManager.DeleteCurrentSlotMidGameSave();
			GameManager.Instance.StartCoroutine(HandleMainMenu());
		}
	}

	private IEnumerator HandleMainMenu()
	{
		if (!m_doingSomething)
		{
			m_doingSomething = true;
			if (BraveInput.PrimaryPlayerInstance.IsKeyboardAndMouse())
			{
				mainMenuButton.Text = "[sprite \"esc_up_002\"" + mainMenuButton.ForceGetLocalizedValue("#DEATH_MAINMENU");
			}
			Pixelator.Instance.ClearCachedFrame();
			if ((bool)m_temporaryPhotoTex)
			{
				RenderTexture.ReleaseTemporary(m_temporaryPhotoTex);
				m_temporaryPhotoTex = null;
			}
			Pixelator.Instance.DoFinalNonFadedLayer = false;
			GameUIRoot.Instance.ToggleUICamera(false);
			Pixelator.Instance.FadeToBlack(0.4f);
			AmmonomiconController.Instance.CloseAmmonomicon();
			AkSoundEngine.PostEvent("Play_UI_menu_cancel_01", base.gameObject);
			while (AmmonomiconController.Instance.IsOpen)
			{
				yield return null;
			}
			if (AmmonomiconController.Instance.CurrentLeftPageRenderer != null)
			{
				AmmonomiconController.Instance.CurrentLeftPageRenderer.Disable();
				AmmonomiconController.Instance.CurrentLeftPageRenderer.Dispose();
			}
			if (AmmonomiconController.Instance.CurrentRightPageRenderer != null)
			{
				AmmonomiconController.Instance.CurrentRightPageRenderer.Disable();
				AmmonomiconController.Instance.CurrentRightPageRenderer.Dispose();
			}
			yield return null;
			GameManager.Instance.LoadCharacterSelect(true);
		}
	}

	private void DoQuickRestart(dfControl control, dfMouseEventArgs mouseEvent)
	{
		if (!AmmonomiconController.Instance.IsOpening && !AmmonomiconController.Instance.IsClosing && GameManager.Instance.CurrentGameMode != GameManager.GameMode.SUPERBOSSRUSH)
		{
			SaveManager.DeleteCurrentSlotMidGameSave();
			GameManager.Instance.StartCoroutine(HandleQuickRestart());
		}
	}

	public static QuickRestartOptions GetNumMetasToQuickRestart()
	{
		QuickRestartOptions result = default(QuickRestartOptions);
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if ((bool)GameManager.Instance.AllPlayers[i] && GameManager.Instance.AllPlayers[i].CharacterUsesRandomGuns)
			{
				result.GunGame = true;
				result.NumMetas += 6;
				break;
			}
			if (GameManager.Instance.AllPlayers[i].characterIdentity == PlayableCharacters.Eevee)
			{
				result.NumMetas += 5;
			}
			else if (GameManager.Instance.AllPlayers[i].characterIdentity == PlayableCharacters.Gunslinger)
			{
				if (!GameStatsManager.Instance.GetFlag(GungeonFlags.GUNSLINGER_UNLOCKED))
				{
					result.NumMetas += 5;
				}
				else
				{
					result.NumMetas += 7;
				}
			}
		}
		if (GameManager.Instance.CurrentGameMode == GameManager.GameMode.BOSSRUSH)
		{
			result.BossRush = true;
			result.NumMetas += 3;
		}
		if (ChallengeManager.CHALLENGE_MODE_ACTIVE)
		{
			result.ChallengeMode = ChallengeManager.ChallengeModeType;
			if (ChallengeManager.ChallengeModeType == ChallengeModeType.ChallengeMode)
			{
				if (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.CHALLENGE_MODE_ATTEMPTS) >= 30f)
				{
					result.NumMetas++;
				}
				else
				{
					result.NumMetas += 6;
				}
			}
			else if (ChallengeManager.ChallengeModeType == ChallengeModeType.ChallengeMegaMode)
			{
				if (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.CHALLENGE_MODE_ATTEMPTS) >= 30f)
				{
					result.NumMetas += 2;
				}
				else
				{
					result.NumMetas += 12;
				}
			}
		}
		return result;
	}

	private IEnumerator HandleQuickRestart()
	{
		if (GameManager.Instance.IsLoadingLevel || m_doingSomething)
		{
			yield break;
		}
		m_doingSomething = true;
		if (BraveInput.PrimaryPlayerInstance.IsKeyboardAndMouse())
		{
			quickRestartButton.Text = "[sprite \"space_bar_down_001\"" + quickRestartButton.ForceGetLocalizedValue("#DEATH_QUICKRESTART");
		}
		AkSoundEngine.PostEvent("Play_UI_menu_characterselect_01", base.gameObject);
		Pixelator.Instance.ClearCachedFrame();
		if ((bool)m_temporaryPhotoTex)
		{
			RenderTexture.ReleaseTemporary(m_temporaryPhotoTex);
			m_temporaryPhotoTex = null;
		}
		AmmonomiconController.Instance.CloseAmmonomicon();
		while (AmmonomiconController.Instance.IsOpen)
		{
			if (!AmmonomiconController.Instance.IsClosing)
			{
				AmmonomiconController.Instance.CloseAmmonomicon();
			}
			yield return null;
		}
		if (AmmonomiconController.Instance.CurrentLeftPageRenderer != null)
		{
			AmmonomiconController.Instance.CurrentLeftPageRenderer.Disable();
			AmmonomiconController.Instance.CurrentLeftPageRenderer.Dispose();
		}
		if (AmmonomiconController.Instance.CurrentRightPageRenderer != null)
		{
			AmmonomiconController.Instance.CurrentRightPageRenderer.Disable();
			AmmonomiconController.Instance.CurrentRightPageRenderer.Dispose();
		}
		yield return null;
		if ((bool)GameManager.LastUsedPlayerPrefab && GameManager.LastUsedPlayerPrefab.GetComponent<PlayerController>().characterIdentity == PlayableCharacters.Gunslinger && !GameStatsManager.Instance.GetFlag(GungeonFlags.GUNSLINGER_UNLOCKED))
		{
			GameManager.LastUsedPlayerPrefab = (GameObject)ResourceCache.Acquire("PlayerEevee");
		}
		QuickRestartOptions qrOptions = GetNumMetasToQuickRestart();
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
		Pixelator.Instance.DoFinalNonFadedLayer = false;
		Pixelator.Instance.FadeToBlack(0.4f);
		GameManager.Instance.DelayedQuickRestart(0.5f, qrOptions);
	}
}
