using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using InControl;
using UnityEngine;

public class GameUIRoot : TimeInvariantMonoBehaviour
{
	public static GameUIRoot m_root;

	public dfLabel p_playerAmmoLabel;

	public dfLabel FoyerAmmonomiconLabel;

	public List<GameUIHeartController> heartControllers;

	public List<GameUIAmmoController> ammoControllers;

	public List<GameUIItemController> itemControllers;

	public List<GameUIBlankController> blankControllers;

	public GameUIBossHealthController bossController;

	public GameUIBossHealthController bossController2;

	public GameUIBossHealthController bossControllerSide;

	public UINotificationController notificationController;

	public dfPanel AreYouSurePanel;

	public bool KeepMetasIsVisible;

	public dfLabel p_playerCoinLabel;

	public dfLabel p_playerKeyLabel;

	public dfSprite p_specialKeySprite;

	[NonSerialized]
	private List<dfSprite> m_extantSpecialKeySprites = new List<dfSprite>();

	public dfLabel p_needsReloadLabel;

	[NonSerialized]
	private List<dfLabel> m_extantReloadLabels;

	public List<dfLabel> gunNameLabels;

	public List<dfLabel> itemNameLabels;

	public LevelNameUIManager levelNameUI;

	public GameUIReloadBarController p_playerReloadBar;

	public GameUIReloadBarController p_secondaryPlayerReloadBar;

	[NonSerialized]
	private List<GameUIReloadBarController> m_extantReloadBars;

	public GameObject undiePanel;

	[NonSerialized]
	[Header("Dynamism")]
	private List<dfControl> customNonCoreMotionGroups = new List<dfControl>();

	public List<dfControl> motionGroups;

	public List<DungeonData.Direction> motionDirections;

	[NonSerialized]
	private List<dfControl> lockedMotionGroups = new List<dfControl>();

	[NonSerialized]
	protected Dictionary<dfControl, Vector3> motionInteriorPositions;

	[NonSerialized]
	protected Dictionary<dfControl, Vector3> motionExteriorPositions;

	[NonSerialized]
	public List<DefaultLabelController> extantBasicLabels = new List<DefaultLabelController>();

	[Header("Demo Tutorial Panels")]
	public List<dfPanel> demoTutorialPanels_Keyboard;

	public List<dfPanel> demoTutorialPanels_Controller;

	private bool m_forceHideGunPanel;

	private bool m_forceHideItemPanel;

	private List<bool> m_displayingReloadNeeded;

	private bool m_bossKillCamActive;

	[NonSerialized]
	private float[] m_gunNameVisibilityTimers;

	[NonSerialized]
	private float[] m_itemNameVisibilityTimers;

	private List<dfLabel> m_inactiveDamageLabels = new List<dfLabel>();

	private OverridableBool m_defaultLabelsHidden = new OverridableBool(false);

	private float MotionGroupBufferWidth = 21f;

	private List<dfSprite> additionalGunBoxes = new List<dfSprite>();

	private List<dfSprite> additionalItemBoxes = new List<dfSprite>();

	private List<dfSprite> additionalGunBoxesSecondary = new List<dfSprite>();

	private List<dfSprite> additionalItemBoxesSecondary = new List<dfSprite>();

	protected OverridableBool CoreUIHidden = new OverridableBool(false);

	public bool GunventoryFolded = true;

	private OverridableBool ForceLowerPanelsInvisibleOverride = new OverridableBool(false);

	private bool m_metalGearGunSelectActive;

	private Dictionary<Texture, Material> MetalGearAtlasToFadeMaterialMapR = new Dictionary<Texture, Material>();

	private Dictionary<Material, Material> MetalGearFadeToOutlineMaterialMapR = new Dictionary<Material, Material>();

	private Dictionary<Material, Material> MetalGearDFAtlasMapR = new Dictionary<Material, Material>();

	private Dictionary<Texture, Material> MetalGearAtlasToFadeMaterialMapL = new Dictionary<Texture, Material>();

	private Dictionary<Material, Material> MetalGearFadeToOutlineMaterialMapL = new Dictionary<Material, Material>();

	private Dictionary<Material, Material> MetalGearDFAtlasMapL = new Dictionary<Material, Material>();

	public Action OnScaleUpdate;

	private dfGUIManager m_manager;

	private dfSprite p_playerCoinSprite;

	private Dictionary<AIActor, dfSlider> m_enemyToHealthbarMap = new Dictionary<AIActor, dfSlider>();

	private List<dfSlider> m_unusedHealthbars = new List<dfSlider>();

	private bool m_isDisplayingCustomReload;

	public dfPanel PauseMenuPanel;

	private PauseMenuController m_pmc;

	public ConversationBarController ConversationBar;

	protected bool m_displayingPlayerConversationOptions;

	protected bool hasSelectedOption;

	protected int selectedResponse = -1;

	private bool m_hasSelectedAreYouSureOption;

	private bool m_AreYouSureSelection;

	private dfButton m_AreYouSureYesButton;

	private dfButton m_AreYouSureNoButton;

	private dfLabel m_AreYouSurePrimaryLabel;

	private dfLabel m_AreYouSureSecondaryLabel;

	public bool ForceHideGunPanel
	{
		get
		{
			return m_forceHideGunPanel;
		}
		set
		{
			m_forceHideGunPanel = value;
			if (!m_forceHideGunPanel)
			{
				for (int i = 0; i < ammoControllers.Count; i++)
				{
					ammoControllers[i].TriggerUIDisabled();
				}
			}
		}
	}

	public bool ForceHideItemPanel
	{
		get
		{
			return m_forceHideItemPanel;
		}
		set
		{
			m_forceHideItemPanel = value;
			if (!m_forceHideItemPanel)
			{
				for (int i = 0; i < itemControllers.Count; i++)
				{
					itemControllers[i].TriggerUIDisabled();
				}
			}
		}
	}

	public static GameUIRoot Instance
	{
		get
		{
			if (m_root == null || !m_root)
			{
				m_root = (GameUIRoot)UnityEngine.Object.FindObjectOfType(typeof(GameUIRoot));
			}
			return m_root;
		}
		set
		{
			m_root = value;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return m_root;
		}
	}

	public bool BossHealthBarVisible
	{
		get
		{
			return bossController.IsActive || bossController2.IsActive || bossControllerSide.IsActive;
		}
	}

	public bool MetalGearActive
	{
		get
		{
			return m_metalGearGunSelectActive;
		}
	}

	public static float GameUIScalar
	{
		get
		{
			if (GameManager.Instance.IsPaused)
			{
				return 1f;
			}
			if (TimeTubeCreditsController.IsTimeTubing)
			{
				return 1f;
			}
			return (!GameManager.Options.SmallUIEnabled) ? 1f : 0.5f;
		}
	}

	public dfGUIManager Manager
	{
		get
		{
			if (m_manager == null)
			{
				m_manager = GetComponent<dfGUIManager>();
			}
			return m_manager;
		}
	}

	public bool DisplayingConversationBar
	{
		get
		{
			return m_displayingPlayerConversationOptions;
		}
	}

	public GameUIReloadBarController GetReloadBarForPlayer(PlayerController p)
	{
		if (m_extantReloadBars != null && m_extantReloadBars.Count > 1 && (bool)p)
		{
			return m_extantReloadBars[(!p.IsPrimaryPlayer) ? 1 : 0];
		}
		if (m_extantReloadBars != null)
		{
			return m_extantReloadBars[0];
		}
		return null;
	}

	public dfPanel AddControlToMotionGroups(dfControl control, DungeonData.Direction dir, bool nonCore = false)
	{
		dfAnchorStyle anchor = control.Anchor;
		control.Anchor = dfAnchorStyle.None;
		if (!motionGroups.Contains(control))
		{
			motionGroups.Add(control);
			motionDirections.Add(dir);
		}
		if (nonCore && !customNonCoreMotionGroups.Contains(control))
		{
			customNonCoreMotionGroups.Add(control);
		}
		Vector3 vector = Vector3.zero;
		Vector3 vector2 = Vector3.zero;
		Vector3 relativePosition = control.RelativePosition;
		Vector2 size = control.Size;
		Vector3 initialInactivePosition = GetInitialInactivePosition(control, dir);
		switch (dir)
		{
		case DungeonData.Direction.NORTH:
			vector = initialInactivePosition;
			vector2 = relativePosition + size.ToVector3ZUp();
			break;
		case DungeonData.Direction.EAST:
			vector = initialInactivePosition + size.ToVector3ZUp();
			vector2 = relativePosition;
			break;
		case DungeonData.Direction.SOUTH:
			vector = initialInactivePosition + size.ToVector3ZUp();
			vector2 = relativePosition;
			break;
		case DungeonData.Direction.WEST:
			vector = initialInactivePosition;
			vector2 = relativePosition + size.ToVector3ZUp();
			break;
		}
		Vector2 vector3 = Vector2.Min(vector.XY(), vector2.XY());
		Vector2 vector4 = Vector2.Max(vector.XY(), vector2.XY());
		Vector2 size2 = vector4 - vector3;
		dfPanel dfPanel2 = ((!(control.Parent == null)) ? control.Parent.AddControl<dfPanel>() : control.GetManager().AddControl<dfPanel>());
		dfPanel2.RelativePosition = vector3;
		dfPanel2.Size = size2;
		dfPanel2.Pivot = control.Pivot;
		dfPanel2.Anchor = anchor;
		dfPanel2.IsInteractive = false;
		dfPanel2.AddControl(control);
		switch (dir)
		{
		case DungeonData.Direction.NORTH:
			control.RelativePosition = new Vector3(0f, size2.y - control.Size.y, 0f);
			break;
		case DungeonData.Direction.EAST:
			control.RelativePosition = new Vector3(0f, 0f, 0f);
			break;
		case DungeonData.Direction.SOUTH:
			control.RelativePosition = new Vector3(0f, 0f, 0f);
			break;
		case DungeonData.Direction.WEST:
			control.RelativePosition = new Vector3(size2.x - control.Size.x, 0f, 0f);
			break;
		}
		control.Anchor = anchor;
		if (nonCore)
		{
			RecalculateTargetPositions();
		}
		return dfPanel2;
	}

	public void UpdateControlMotionGroup(dfControl control)
	{
		if (!(control == null) && (bool)control && motionGroups.Contains(control))
		{
			DungeonData.Direction dir = motionDirections[motionGroups.IndexOf(control)];
			RemoveControlFromMotionGroups(control);
			AddControlToMotionGroups(control, dir);
			control.enabled = true;
		}
	}

	public dfPanel GetMotionGroupParent(dfControl control)
	{
		if (motionGroups.Contains(control))
		{
			return motionGroups[motionGroups.IndexOf(control)].Parent as dfPanel;
		}
		return null;
	}

	public void RemoveControlFromMotionGroups(dfControl control)
	{
		int num = motionGroups.IndexOf(control);
		if (num != -1)
		{
			motionGroups.Remove(control);
			motionDirections.RemoveAt(num);
		}
		dfControl parent = control.Parent;
		if ((bool)control.Parent && (bool)control.Parent.Parent)
		{
			control.Parent.Parent.AddControl(control);
		}
		else if ((bool)control.Parent)
		{
			control.Parent.RemoveControl(control);
		}
		UnityEngine.Object.Destroy(parent.gameObject);
	}

	private Vector3 GetActivePosition(dfControl panel, DungeonData.Direction direction)
	{
		Vector3 relativePosition = panel.RelativePosition;
		Vector3 vector = panel.Size.ToVector3ZUp();
		dfPanel dfPanel2 = panel.Parent as dfPanel;
		if (dfPanel2 != null)
		{
			switch (direction)
			{
			case DungeonData.Direction.NORTH:
				return new Vector3(0f, dfPanel2.Size.y - vector.y, 0f);
			case DungeonData.Direction.EAST:
				return Vector3.zero;
			case DungeonData.Direction.SOUTH:
				return Vector3.zero;
			case DungeonData.Direction.WEST:
				return new Vector3(dfPanel2.Size.x - vector.x, 0f, 0f);
			}
		}
		return relativePosition;
	}

	public void DoDamageNumber(Vector3 worldPosition, float heightOffGround, int damage)
	{
		string stringForInt = IntToStringSansGarbage.GetStringForInt(damage);
		if (m_inactiveDamageLabels.Count == 0)
		{
			GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("DamagePopupLabel"), base.transform);
			m_inactiveDamageLabels.Add(gameObject.GetComponent<dfLabel>());
		}
		dfLabel dfLabel2 = m_inactiveDamageLabels[0];
		m_inactiveDamageLabels.RemoveAt(0);
		dfLabel2.gameObject.SetActive(true);
		dfLabel2.Text = stringForInt;
		dfLabel2.Color = Color.red;
		dfLabel2.Opacity = 1f;
		dfLabel2.transform.position = dfFollowObject.ConvertWorldSpaces(worldPosition, GameManager.Instance.MainCameraController.Camera, m_manager.RenderCamera).WithZ(0f);
		dfLabel2.transform.position = dfLabel2.transform.position.QuantizeFloor(dfLabel2.PixelsToUnits() / (Pixelator.Instance.ScaleTileScale / Pixelator.Instance.CurrentTileScale));
		dfLabel2.StartCoroutine(HandleDamageNumberCR(worldPosition, worldPosition.y - heightOffGround, dfLabel2));
	}

	private IEnumerator HandleDamageNumberCR(Vector3 startWorldPosition, float worldFloorHeight, dfLabel damageLabel)
	{
		float elapsed = 0f;
		float duration = 1.5f;
		float holdTime = 0f;
		Camera mainCam = GameManager.Instance.MainCameraController.Camera;
		Vector3 worldPosition = startWorldPosition;
		Vector3 lastVelocity = new Vector3(Mathf.Lerp(-8f, 8f, UnityEngine.Random.value), UnityEngine.Random.Range(15f, 25f), 0f);
		while (elapsed < duration)
		{
			float dt = BraveTime.DeltaTime;
			elapsed += dt;
			if (GameManager.Instance.IsPaused)
			{
				break;
			}
			if (elapsed > holdTime)
			{
				lastVelocity += new Vector3(0f, -50f, 0f) * dt;
				Vector3 vector = lastVelocity * dt + worldPosition;
				if (vector.y < worldFloorHeight)
				{
					float num = worldFloorHeight - vector.y;
					float num2 = worldFloorHeight + num;
					vector.y = num2 * 0.5f;
					lastVelocity.y *= -0.5f;
				}
				worldPosition = vector;
				damageLabel.transform.position = dfFollowObject.ConvertWorldSpaces(worldPosition, mainCam, m_manager.RenderCamera).WithZ(0f);
			}
			float t = elapsed / duration;
			damageLabel.Opacity = 1f - t;
			yield return null;
		}
		damageLabel.gameObject.SetActive(false);
		m_inactiveDamageLabels.Add(damageLabel);
	}

	public bool TransformHasDefaultLabel(Transform attachTransform)
	{
		for (int i = 0; i < extantBasicLabels.Count; i++)
		{
			if (extantBasicLabels[i].targetObject == attachTransform)
			{
				return true;
			}
		}
		return false;
	}

	public GameObject RegisterDefaultLabel(Transform attachTransform, Vector3 offset, string text)
	{
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("DefaultLabelPanel"));
		DefaultLabelController component = gameObject.GetComponent<DefaultLabelController>();
		m_manager.AddControl(component.panel);
		component.label.Text = text;
		component.Trigger(attachTransform, offset);
		extantBasicLabels.Add(component);
		return gameObject;
	}

	public void ToggleAllDefaultLabels(bool visible, string reason)
	{
		if (visible)
		{
			m_defaultLabelsHidden.RemoveOverride(reason);
		}
		else
		{
			m_defaultLabelsHidden.SetOverride(reason, true);
		}
		for (int i = 0; i < extantBasicLabels.Count; i++)
		{
			if ((bool)extantBasicLabels[i] && (bool)extantBasicLabels[i].panel)
			{
				extantBasicLabels[i].panel.IsVisible = !m_defaultLabelsHidden.Value;
			}
		}
	}

	public void ClearAllDefaultLabels()
	{
		int num;
		for (num = 0; num < extantBasicLabels.Count; num++)
		{
			UnityEngine.Object.Destroy(extantBasicLabels[num].gameObject);
			extantBasicLabels.RemoveAt(num);
			num--;
		}
	}

	public void ForceRemoveDefaultLabel(DefaultLabelController label)
	{
		int num = extantBasicLabels.IndexOf(label);
		if (num >= 0)
		{
			extantBasicLabels.RemoveAt(num);
		}
		UnityEngine.Object.Destroy(label.gameObject);
	}

	public void DeregisterDefaultLabel(Transform attachTransform)
	{
		for (int i = 0; i < extantBasicLabels.Count; i++)
		{
			if (extantBasicLabels[i].targetObject == attachTransform)
			{
				UnityEngine.Object.Destroy(extantBasicLabels[i].gameObject);
				extantBasicLabels.RemoveAt(i);
				i--;
			}
		}
	}

	public void TriggerDemoModeTutorialScreens()
	{
		if (demoTutorialPanels_Controller.Count != 0 && !GameStatsManager.Instance.GetFlag(GungeonFlags.TUTORIAL_COMPLETED) && !GameStatsManager.Instance.GetFlag(GungeonFlags.INTERNALDEBUG_HAS_SEEN_DEMO_TEXT))
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.INTERNALDEBUG_HAS_SEEN_DEMO_TEXT, true);
			StartCoroutine(HandleDemoModeTutorialScreens());
		}
	}

	private IEnumerator HandleDemoModeTutorialScreens()
	{
		levelNameUI.BanishLevelNameText();
		GameManager.Instance.PauseRaw(true);
		int currentPanelIndex = 0;
		List<dfPanel> panelList = ((!BraveInput.GetInstanceForPlayer(0).IsKeyboardAndMouse()) ? demoTutorialPanels_Controller : demoTutorialPanels_Keyboard);
		bool cachedKM = BraveInput.GetInstanceForPlayer(0).IsKeyboardAndMouse();
		while (currentPanelIndex < panelList.Count)
		{
			dfPanel currentPanel = panelList[currentPanelIndex];
			if (cachedKM != BraveInput.GetInstanceForPlayer(0).IsKeyboardAndMouse())
			{
				currentPanel.IsVisible = false;
				cachedKM = BraveInput.GetInstanceForPlayer(0).IsKeyboardAndMouse();
				panelList = ((!cachedKM) ? demoTutorialPanels_Controller : demoTutorialPanels_Keyboard);
				currentPanel = panelList[currentPanelIndex];
			}
			if (!currentPanel.IsVisible)
			{
				currentPanel.IsVisible = true;
			}
			else if (BraveInput.GetInstanceForPlayer(0).ActiveActions.MenuSelectAction.WasPressed || BraveInput.GetInstanceForPlayer(0).ActiveActions.ShootAction.WasPressed)
			{
				currentPanel.IsVisible = false;
				currentPanelIndex++;
				if (currentPanelIndex < panelList.Count)
				{
					panelList[currentPanelIndex].IsVisible = true;
				}
			}
			yield return null;
		}
		GameManager.Instance.ForceUnpause();
		GameManager.Instance.PreventPausing = false;
	}

	private Vector3 GetInactivePosition(dfControl panel, DungeonData.Direction direction)
	{
		Vector3 relativePosition = panel.RelativePosition;
		Vector3 vector = panel.Size.ToVector3ZUp();
		dfPanel dfPanel2 = panel.Parent as dfPanel;
		if (dfPanel2 != null)
		{
			switch (direction)
			{
			case DungeonData.Direction.NORTH:
				return Vector3.zero;
			case DungeonData.Direction.EAST:
				return new Vector3(dfPanel2.Size.x - vector.x, 0f, 0f);
			case DungeonData.Direction.SOUTH:
				return new Vector3(0f, dfPanel2.Size.y - vector.y, 0f);
			case DungeonData.Direction.WEST:
				return Vector3.zero;
			}
		}
		return relativePosition;
	}

	private Vector3 GetInitialInactivePosition(dfControl panel, DungeonData.Direction direction)
	{
		Vector3 result = panel.RelativePosition;
		Vector3 vector = panel.Size.ToVector3ZUp();
		Vector2 screenSize = panel.GetManager().GetScreenSize();
		switch (direction)
		{
		case DungeonData.Direction.NORTH:
			result = new Vector3(result.x, 0f - vector.y - MotionGroupBufferWidth, result.z);
			break;
		case DungeonData.Direction.EAST:
			result = new Vector3(screenSize.x + vector.x + MotionGroupBufferWidth, result.y, result.z);
			break;
		case DungeonData.Direction.SOUTH:
			result = new Vector3(result.x, screenSize.y + vector.y + MotionGroupBufferWidth, result.z);
			break;
		case DungeonData.Direction.WEST:
			result = new Vector3(0f - vector.x - MotionGroupBufferWidth, result.y, result.z);
			break;
		}
		return result;
	}

	public void AddPassiveItemToDock(PassiveItem item, PlayerController sourcePlayer)
	{
		MinimapUIController minimapUIController = null;
		if ((bool)Minimap.Instance)
		{
			minimapUIController = Minimap.Instance.UIMinimap;
		}
		if (!minimapUIController)
		{
			minimapUIController = UnityEngine.Object.FindObjectOfType<MinimapUIController>();
		}
		minimapUIController.AddPassiveItemToDock(item, sourcePlayer);
	}

	public void RemovePassiveItemFromDock(PassiveItem item)
	{
		MinimapUIController minimapUIController = UnityEngine.Object.FindObjectOfType<MinimapUIController>();
		minimapUIController.RemovePassiveItemFromDock(item);
	}

	private IEnumerator Start()
	{
		for (int i = 0; i < motionGroups.Count; i++)
		{
			AddControlToMotionGroups(motionGroups[i], motionDirections[i]);
		}
		RecalculateTargetPositions();
		if (AreYouSurePanel != null)
		{
			m_AreYouSureYesButton = AreYouSurePanel.transform.Find("YesButton").GetComponent<dfButton>();
			m_AreYouSureNoButton = AreYouSurePanel.transform.Find("NoButton").GetComponent<dfButton>();
			m_AreYouSurePrimaryLabel = AreYouSurePanel.transform.Find("TopLabel").GetComponent<dfLabel>();
			m_AreYouSureSecondaryLabel = AreYouSurePanel.transform.Find("SecondaryLabel").GetComponent<dfLabel>();
		}
		notificationController.Initialize();
		m_extantReloadLabels = new List<dfLabel>();
		m_extantReloadLabels.Add(p_needsReloadLabel);
		m_displayingReloadNeeded = new List<bool>();
		m_displayingReloadNeeded.Add(false);
		m_extantReloadBars = new List<GameUIReloadBarController>();
		m_extantReloadBars.Add(p_playerReloadBar);
		m_gunNameVisibilityTimers = new float[gunNameLabels.Count];
		m_itemNameVisibilityTimers = new float[itemNameLabels.Count];
		if (GameManager.Instance.PrimaryPlayer == null)
		{
			HideCoreUI(string.Empty);
			ToggleLowerPanels(false, false, string.Empty);
		}
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
		{
			if (Foyer.DoMainMenu)
			{
				Manager.RenderCamera.enabled = false;
			}
			for (int j = 0; j < motionGroups.Count; j++)
			{
				if (!(motionGroups[j] == notificationController.Panel))
				{
					motionGroups[j].Parent.IsVisible = false;
					motionGroups.RemoveAt(j);
					j--;
				}
			}
		}
		CameraController mainCameraController = GameManager.Instance.MainCameraController;
		mainCameraController.OnFinishedFrame = (Action)Delegate.Combine(mainCameraController.OnFinishedFrame, new Action(UpdateReloadLabelsOnCameraFinishedFrame));
		yield return null;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			ConvertCoreUIToCoopMode();
		}
	}

	public void DisableCoopPlayerUI(PlayerController deadPlayer)
	{
		if (deadPlayer.IsPrimaryPlayer)
		{
			ammoControllers[1].ToggleRenderers(false);
			itemControllers[0].ToggleRenderers(false);
			itemControllers[0].temporarilyPreventVisible = true;
		}
		else
		{
			ammoControllers[0].ToggleRenderers(false);
			itemControllers[1].ToggleRenderers(false);
			itemControllers[1].temporarilyPreventVisible = true;
		}
	}

	public void ReenableCoopPlayerUI(PlayerController deadPlayer)
	{
		if (deadPlayer.IsPrimaryPlayer)
		{
			ammoControllers[1].GetComponent<dfPanel>().IsVisible = true;
			ammoControllers[1].ToggleRenderers(true);
			itemControllers[0].GetComponent<dfPanel>().IsVisible = true;
			itemControllers[0].ToggleRenderers(true);
			itemControllers[0].temporarilyPreventVisible = false;
		}
		else
		{
			ammoControllers[0].GetComponent<dfPanel>().IsVisible = true;
			ammoControllers[0].ToggleRenderers(true);
			itemControllers[1].GetComponent<dfPanel>().IsVisible = true;
			itemControllers[1].ToggleRenderers(true);
			itemControllers[1].temporarilyPreventVisible = false;
		}
	}

	public void ConvertCoreUIToCoopMode()
	{
		float num = gunNameLabels[0].PixelsToUnits();
		bool flag = GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER;
		bool flag2 = !flag;
		heartControllers[1].GetComponent<dfPanel>().IsVisible = flag2;
		blankControllers[1].GetComponent<dfPanel>().IsVisible = flag2;
		ammoControllers[1].GetComponent<dfPanel>().IsEnabled = flag2;
		ammoControllers[1].GetComponent<dfPanel>().IsVisible = flag2;
		itemControllers[0].transform.position = ammoControllers[1].GunBoxSprite.transform.position + new Vector3((3f + ammoControllers[1].GunBoxSprite.Width + (float)(2 * ammoControllers[1].AdditionalGunBoxSprites.Count)) * num, 0f, 0f);
		itemControllers[1].transform.position = ammoControllers[0].GunBoxSprite.transform.position + new Vector3((3f + ammoControllers[0].GunBoxSprite.Width + (float)(2 * ammoControllers[0].AdditionalGunBoxSprites.Count)) * -1f * num, 0f, 0f);
		itemNameLabels[0].RelativePosition += new Vector3(ammoControllers[0].GunBoxSprite.Width * num, 0f, 0f);
		itemNameLabels[1].RelativePosition += new Vector3((0f - ammoControllers[0].GunBoxSprite.Width) * num, 0f, 0f);
		dfLabel component = Manager.AddPrefab(p_needsReloadLabel.gameObject).GetComponent<dfLabel>();
		component.IsVisible = false;
		m_extantReloadLabels.Add(component);
		m_displayingReloadNeeded.Add(false);
		m_extantReloadBars.Add(p_secondaryPlayerReloadBar);
	}

	protected void RecalculateTargetPositions()
	{
		if (motionInteriorPositions == null)
		{
			motionInteriorPositions = new Dictionary<dfControl, Vector3>();
		}
		else
		{
			motionInteriorPositions.Clear();
		}
		if (motionExteriorPositions == null)
		{
			motionExteriorPositions = new Dictionary<dfControl, Vector3>();
		}
		else
		{
			motionExteriorPositions.Clear();
		}
		for (int i = 0; i < motionGroups.Count; i++)
		{
			motionInteriorPositions.Add(motionGroups[i], GetActivePosition(motionGroups[i], motionDirections[i]));
			motionExteriorPositions.Add(motionGroups[i], GetInactivePosition(motionGroups[i], motionDirections[i]));
		}
	}

	public bool IsCoreUIVisible()
	{
		return !CoreUIHidden.Value;
	}

	public void HideCoreUI(string reason = "")
	{
		if (string.IsNullOrEmpty(reason))
		{
			reason = "generic";
		}
		bool value = CoreUIHidden.Value;
		CoreUIHidden.SetOverride(reason, true);
		if (CoreUIHidden.Value != value)
		{
			RecalculateTargetPositions();
			StartCoroutine(CoreUITransition());
			for (int i = 0; i < m_extantReloadBars.Count; i++)
			{
				m_extantReloadBars[i].SetInvisibility(true, "CoreUI");
			}
		}
	}

	public GameUIAmmoController GetAmmoControllerForPlayerID(int playerID)
	{
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			return (playerID != 1) ? ammoControllers[1] : ammoControllers[0];
		}
		if (ammoControllers.Count > 1)
		{
			return (playerID != 0) ? ammoControllers[1] : ammoControllers[0];
		}
		return ammoControllers[0];
	}

	private GameUIItemController GetItemControllerForPlayerID(int playerID)
	{
		if (playerID >= itemControllers.Count)
		{
			return null;
		}
		return itemControllers[playerID];
	}

	public void ToggleLowerPanels(bool targetVisible, bool permanent = false, string source = "")
	{
		if (targetVisible && GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES)
		{
			return;
		}
		if (string.IsNullOrEmpty(source))
		{
			source = "generic";
		}
		ForceLowerPanelsInvisibleOverride.SetOverride(source, !targetVisible);
		for (int i = 0; i < ammoControllers.Count; i++)
		{
			bool flag = targetVisible;
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
			{
				flag = false;
			}
			if (i >= GameManager.Instance.AllPlayers.Length)
			{
				flag = false;
			}
			if (i < GameManager.Instance.AllPlayers.Length && GameManager.Instance.AllPlayers[i].IsGhost)
			{
				continue;
			}
			if (ForceLowerPanelsInvisibleOverride.Value)
			{
				flag = false;
			}
			GameUIAmmoController ammoControllerForPlayerID = GetAmmoControllerForPlayerID(i);
			GameUIItemController itemControllerForPlayerID = GetItemControllerForPlayerID(i);
			if (!ammoControllerForPlayerID.forceInvisiblePermanent)
			{
				dfPanel component = ammoControllerForPlayerID.GetComponent<dfPanel>();
				dfPanel component2 = itemControllerForPlayerID.GetComponent<dfPanel>();
				component.IsVisible = flag;
				component2.IsVisible = flag;
				ammoControllerForPlayerID.ToggleRenderers(flag);
				itemControllerForPlayerID.ToggleRenderers(flag);
				if (permanent)
				{
					ammoControllerForPlayerID.forceInvisiblePermanent = !flag;
				}
				ammoControllerForPlayerID.temporarilyPreventVisible = !flag;
				itemControllerForPlayerID.temporarilyPreventVisible = !flag;
			}
		}
	}

	public void ToggleItemPanels(bool targetVisible)
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			bool flag = targetVisible;
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER)
			{
				flag = false;
			}
			if (i >= GameManager.Instance.AllPlayers.Length)
			{
				flag = false;
			}
			if (i < GameManager.Instance.AllPlayers.Length && GameManager.Instance.AllPlayers[i].IsGhost)
			{
				flag = false;
			}
			GameUIItemController itemControllerForPlayerID = GetItemControllerForPlayerID(i);
			if ((bool)itemControllerForPlayerID)
			{
				dfPanel component = itemControllerForPlayerID.GetComponent<dfPanel>();
				component.IsVisible = flag;
				itemControllerForPlayerID.ToggleRenderers(flag);
				itemControllerForPlayerID.temporarilyPreventVisible = !flag;
			}
		}
	}

	public void MoveNonCoreGroupImmediately(dfControl control, bool offScreen = false)
	{
		if (motionInteriorPositions.ContainsKey(control) && motionExteriorPositions.ContainsKey(control))
		{
			if (offScreen)
			{
				control.RelativePosition = motionExteriorPositions[control];
			}
			else
			{
				control.RelativePosition = motionInteriorPositions[control];
			}
		}
	}

	public void MoveNonCoreGroupOnscreen(dfControl control, bool reversed = false)
	{
		if (customNonCoreMotionGroups.Contains(control))
		{
			StartCoroutine(NonCoreControlTransition(control, reversed));
		}
	}

	private IEnumerator NonCoreControlTransition(dfControl control, bool reversed = false)
	{
		float transitionTime = 0.25f;
		float elapsed = 0f;
		while (elapsed < transitionTime)
		{
			elapsed += m_deltaTime;
			float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / transitionTime));
			if (reversed)
			{
				t = 1f - t;
			}
			control.RelativePosition = Vector3.Lerp(motionExteriorPositions[control], motionInteriorPositions[control], t);
			yield return null;
		}
	}

	public void ShowCoreUI(string reason = "")
	{
		if (string.IsNullOrEmpty(reason))
		{
			reason = "generic";
		}
		bool value = CoreUIHidden.Value;
		CoreUIHidden.SetOverride(reason, false);
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES && GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER && value != CoreUIHidden.Value)
		{
			RecalculateTargetPositions();
			StartCoroutine(CoreUITransition());
			for (int i = 0; i < m_extantReloadBars.Count; i++)
			{
				m_extantReloadBars[i].SetInvisibility(false, "CoreUI");
			}
		}
	}

	public void TransitionTargetMotionGroup(dfControl motionGroup, bool targetVisibility, bool targetLockState, bool instant)
	{
		RecalculateTargetPositions();
		StartCoroutine(TransitionTargetMotionGroup_CR(motionGroup, targetVisibility, targetLockState, instant));
	}

	private IEnumerator TransitionTargetMotionGroup_CR(dfControl motionGroup, bool targetVisibility, bool targetLockState, bool instant)
	{
		if (!motionExteriorPositions.ContainsKey(motionGroup) || !motionInteriorPositions.ContainsKey(motionGroup))
		{
			yield break;
		}
		Vector3 interiorPosition = motionInteriorPositions[motionGroup];
		Vector3 exteriorPosition = motionExteriorPositions[motionGroup];
		float transitionTime = 0.25f;
		float elapsed = 0f;
		if (instant)
		{
			transitionTime = 0f;
		}
		Color targetColor = ((!targetVisibility) ? new Color(0.3f, 0.3f, 0.3f) : Color.white);
		dfControl[] controls = motionGroup.GetComponentsInChildren<dfControl>();
		for (int i = 0; i < controls.Length; i++)
		{
			controls[i].Color = targetColor;
		}
		if (targetLockState && !lockedMotionGroups.Contains(motionGroup))
		{
			lockedMotionGroups.Add(motionGroup);
		}
		if ((targetVisibility && motionGroup.RelativePosition.XY() == interiorPosition.XY()) || (!targetVisibility && motionGroup.RelativePosition.XY() == exteriorPosition.XY()))
		{
			yield break;
		}
		if (instant)
		{
			motionGroup.RelativePosition = ((!targetVisibility) ? exteriorPosition : interiorPosition);
		}
		else
		{
			while (elapsed < transitionTime)
			{
				elapsed += m_deltaTime;
				float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / transitionTime));
				if (!targetVisibility)
				{
					t = 1f - t;
				}
				motionGroup.RelativePosition = Vector3.Lerp(exteriorPosition, interiorPosition, t);
				yield return null;
			}
		}
		if (!targetLockState && lockedMotionGroups.Contains(motionGroup))
		{
			lockedMotionGroups.Remove(motionGroup);
		}
	}

	private IEnumerator CoreUITransition()
	{
		bool cachedVisibility = !CoreUIHidden.Value;
		float transitionTime = 0.25f;
		float elapsed = 0f;
		Color targetColor = (CoreUIHidden.Value ? new Color(0.3f, 0.3f, 0.3f) : Color.white);
		for (int i = 0; i < motionGroups.Count; i++)
		{
			if (!customNonCoreMotionGroups.Contains(motionGroups[i]) && !lockedMotionGroups.Contains(motionGroups[i]))
			{
				dfControl[] componentsInChildren = motionGroups[i].GetComponentsInChildren<dfControl>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].Color = targetColor;
				}
			}
		}
		while (elapsed < transitionTime && cachedVisibility == !CoreUIHidden.Value)
		{
			elapsed += m_deltaTime;
			float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / transitionTime));
			if (CoreUIHidden.Value)
			{
				t = 1f - t;
			}
			for (int k = 0; k < motionGroups.Count; k++)
			{
				if (!customNonCoreMotionGroups.Contains(motionGroups[k]) && !lockedMotionGroups.Contains(motionGroups[k]) && motionExteriorPositions.ContainsKey(motionGroups[k]) && motionInteriorPositions.ContainsKey(motionGroups[k]))
				{
					motionGroups[k].RelativePosition = Vector3.Lerp(motionExteriorPositions[motionGroups[k]], motionInteriorPositions[motionGroups[k]], t);
				}
			}
			yield return null;
		}
	}

	public tk2dClippedSprite GetSpriteForUnfoldedItem(int playerID, int itemIndex)
	{
		GameUIItemController itemControllerForPlayerID = GetItemControllerForPlayerID(playerID);
		Transform parent = itemControllerForPlayerID.ItemBoxSprite.transform.parent;
		Transform transform = parent.Find("AdditionalItemBox" + IntToStringSansGarbage.GetStringForInt(itemIndex));
		if (transform != null)
		{
			dfSprite component = transform.GetComponent<dfSprite>();
			return component.transform.Find("AdditionalItemSprite").GetComponent<tk2dClippedSprite>();
		}
		return null;
	}

	public tk2dClippedSprite GetSpriteForUnfoldedGun(int playerID, int gunIndex)
	{
		GameUIAmmoController ammoControllerForPlayerID = GetAmmoControllerForPlayerID(playerID);
		Transform parent = ammoControllerForPlayerID.GunBoxSprite.transform.parent;
		Transform transform = parent.Find("AdditionalWeaponBox" + IntToStringSansGarbage.GetStringForInt(gunIndex));
		if (transform != null)
		{
			dfSprite component = transform.GetComponent<dfSprite>();
			dfSprite component2 = component.transform.GetChild(0).GetComponent<dfSprite>();
			return component.transform.Find("AdditionalGunSprite").GetComponent<tk2dClippedSprite>();
		}
		return null;
	}

	public void ToggleHighlightUnfoldedGun(int gunIndex, bool highlighted)
	{
		if (gunIndex == 0)
		{
			for (int i = 0; i < ammoControllers[0].gunSprites.Length; i++)
			{
				tk2dClippedSprite tk2dClippedSprite2 = ammoControllers[0].gunSprites[i];
				if (highlighted)
				{
					tk2dClippedSprite2.renderer.enabled = false;
				}
				else
				{
					tk2dClippedSprite2.renderer.enabled = true;
				}
			}
		}
		else
		{
			tk2dClippedSprite component = additionalGunBoxes[gunIndex - 1].transform.Find("AdditionalGunSprite").GetComponent<tk2dClippedSprite>();
			if (highlighted)
			{
				component.renderer.enabled = false;
			}
			else
			{
				component.renderer.enabled = true;
			}
		}
	}

	public void UnfoldGunventory(bool doItems = true)
	{
		if (GameManager.Instance.PrimaryPlayer.inventory.AllGuns.Count <= 8 && GunventoryFolded)
		{
			GunventoryFolded = false;
			StartCoroutine(HandlePauseInventoryFolding(GameManager.Instance.PrimaryPlayer, true, doItems));
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				StartCoroutine(HandlePauseInventoryFolding(GameManager.Instance.SecondaryPlayer));
			}
		}
	}

	public void RefoldGunventory()
	{
		if (!GunventoryFolded)
		{
			GunventoryFolded = true;
			StartCoroutine(HandlePauseInventoryFolding(GameManager.Instance.PrimaryPlayer));
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				StartCoroutine(HandlePauseInventoryFolding(GameManager.Instance.SecondaryPlayer));
			}
		}
	}

	private void DestroyAdditionalFrames(bool GunventoryFolded, GameUIAmmoController ammoController, GameUIItemController itemController, List<dfSprite> additionalGunFrames, List<dfSprite> additionalItemFrames, bool forceDestroy = false)
	{
		if (!GunventoryFolded)
		{
			if (ammoController != null)
			{
				for (int i = 0; i < ammoController.AdditionalGunBoxSprites.Count; i++)
				{
					dfControl dfControl2 = ammoController.AdditionalGunBoxSprites[i];
					if ((bool)dfControl2)
					{
						dfControl2.transform.parent = null;
						UnityEngine.Object.Destroy(dfControl2.gameObject);
					}
				}
				ammoController.AdditionalGunBoxSprites.Clear();
			}
			if (itemController != null)
			{
				for (int j = 0; j < itemController.AdditionalItemBoxSprites.Count; j++)
				{
					dfControl dfControl3 = itemController.AdditionalItemBoxSprites[j];
					if ((bool)dfControl3)
					{
						dfControl3.transform.parent = null;
						UnityEngine.Object.Destroy(dfControl3.gameObject);
					}
				}
				itemController.AdditionalItemBoxSprites.Clear();
			}
		}
		if (!GunventoryFolded || forceDestroy)
		{
			if (additionalGunFrames != null)
			{
				for (int k = 0; k < additionalGunFrames.Count; k++)
				{
					dfSprite dfSprite2 = additionalGunFrames[k];
					if ((bool)dfSprite2)
					{
						UnityEngine.Object.Destroy(dfSprite2.gameObject);
					}
				}
			}
			if (additionalItemFrames != null)
			{
				for (int l = 0; l < additionalItemFrames.Count; l++)
				{
					dfSprite dfSprite3 = additionalItemFrames[l];
					if ((bool)dfSprite3)
					{
						UnityEngine.Object.Destroy(dfSprite3.gameObject);
					}
				}
			}
		}
		if (additionalGunFrames != null)
		{
			additionalGunFrames.Clear();
		}
		if (additionalItemFrames != null)
		{
			additionalItemFrames.Clear();
		}
	}

	private void HandleStackedFrameFoldMotion(float t, dfSprite baseBoxSprite, List<dfSprite> additionalGunFrames, List<tk2dClippedSprite> gunSpritesByBox, Dictionary<tk2dClippedSprite, tk2dClippedSprite[]> gunToOutlineMap)
	{
		float num = gunNameLabels[0].PixelsToUnits();
		for (int i = 0; i < additionalGunFrames.Count; i++)
		{
			float num2 = 1f / (float)additionalGunFrames.Count;
			Vector3 vector = baseBoxSprite.RelativePosition - baseBoxSprite.Size.WithX(0f).ToVector3ZUp() * i;
			Vector3 b = vector - baseBoxSprite.Size.WithX(0f).ToVector3ZUp();
			float num3 = num2 * (float)i;
			float num4 = Mathf.Clamp01((t - num3) / num2);
			float num5 = Mathf.SmoothStep(0f, 1f, num4);
			additionalGunFrames[i].FillAmount = num5;
			additionalGunFrames[i].IsVisible = additionalGunFrames[i].FillAmount > 0f;
			tk2dClippedSprite tk2dClippedSprite2 = gunSpritesByBox[i];
			if (tk2dClippedSprite2 != null)
			{
				float num6 = tk2dClippedSprite2.GetUntrimmedBounds().size.y / (additionalGunFrames[i].Size.y * num);
				float num7 = (1f - num6) / 2f;
				float num8 = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((num4 - num7) / num6));
				tk2dClippedSprite2.clipBottomLeft = new Vector2(0f, 1f - num8);
				for (int j = 0; j < gunToOutlineMap[tk2dClippedSprite2].Length; j++)
				{
					gunToOutlineMap[tk2dClippedSprite2][j].clipBottomLeft = new Vector2(0f, 1f - num8);
				}
			}
			additionalGunFrames[i].RelativePosition = Vector3.Lerp(vector, b, num5);
		}
	}

	private void UpdateFramedGunSprite(Gun sourceGun, dfSprite targetFrame, GameUIAmmoController ammoController)
	{
		tk2dBaseSprite tk2dBaseSprite2 = sourceGun.GetSprite();
		tk2dClippedSprite componentInChildren = targetFrame.GetComponentInChildren<tk2dClippedSprite>();
		componentInChildren.SetSprite(tk2dBaseSprite2.Collection, tk2dBaseSprite2.spriteId);
		componentInChildren.scale = GameUIUtility.GetCurrentTK2D_DFScale(m_manager) * Vector3.one;
		Vector3 center = targetFrame.GetCenter();
		componentInChildren.transform.position = center + ammoController.GetOffsetVectorForGun(sourceGun, false);
	}

	public void TriggerMetalGearGunSelect(PlayerController sourcePlayer)
	{
		if (!sourcePlayer.IsGhost && sourcePlayer.inventory.AllGuns.Count >= 2)
		{
			int numToL = -1;
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				numToL = (sourcePlayer.IsPrimaryPlayer ? 1 : (-1));
			}
			if (sourcePlayer.inventory.AllGuns.Count == 2)
			{
				numToL = 0;
			}
			StartCoroutine(HandleMetalGearGunSelect(sourcePlayer, numToL));
		}
	}

	private void AssignClippedSpriteFadeFractions(tk2dClippedSprite gunSpr, float fadeScreenSpaceY, float fadeScreenSpaceXStart, float fadeScreenSpaceXEnd, bool leftAligned)
	{
		Dictionary<Texture, Material> dictionary = ((!leftAligned) ? MetalGearAtlasToFadeMaterialMapR : MetalGearAtlasToFadeMaterialMapL);
		Dictionary<Material, Material> dictionary2 = ((!leftAligned) ? MetalGearFadeToOutlineMaterialMapR : MetalGearFadeToOutlineMaterialMapL);
		Material sharedMaterial = gunSpr.renderer.sharedMaterial;
		Material material = null;
		if (dictionary.ContainsKey(sharedMaterial.mainTexture))
		{
			material = dictionary[sharedMaterial.mainTexture];
		}
		else
		{
			material = gunSpr.renderer.material;
			dictionary.Add(sharedMaterial.mainTexture, material);
		}
		if (sharedMaterial != material)
		{
			gunSpr.renderer.sharedMaterial = material;
		}
		gunSpr.usesOverrideMaterial = true;
		gunSpr.renderer.sharedMaterial.shader = ShaderCache.Acquire("tk2d/BlendVertexColorFadeRange");
		gunSpr.renderer.sharedMaterial.SetFloat("_YFadeStart", Mathf.Min(0.75f, fadeScreenSpaceY));
		gunSpr.renderer.sharedMaterial.SetFloat("_YFadeEnd", 0.03f);
		gunSpr.renderer.sharedMaterial.SetFloat("_XFadeStart", fadeScreenSpaceXStart);
		gunSpr.renderer.sharedMaterial.SetFloat("_XFadeEnd", fadeScreenSpaceXEnd);
		tk2dClippedSprite[] outlineSprites = SpriteOutlineManager.GetOutlineSprites<tk2dClippedSprite>(gunSpr);
		if (outlineSprites == null || outlineSprites.Length <= 0)
		{
			return;
		}
		Material material2 = null;
		if (dictionary2.ContainsKey(material))
		{
			material2 = dictionary2[material];
		}
		else
		{
			material2 = UnityEngine.Object.Instantiate(gunSpr.renderer.sharedMaterial);
			dictionary2.Add(material, material2);
		}
		material2.SetFloat("_YFadeStart", Mathf.Min(0.75f, fadeScreenSpaceY));
		material2.SetFloat("_YFadeEnd", 0.03f);
		material2.SetFloat("_XFadeStart", fadeScreenSpaceXStart);
		material2.SetFloat("_XFadeEnd", fadeScreenSpaceXEnd);
		material2.SetColor("_OverrideColor", new Color(1f, 1f, 1f, 1f));
		material2.SetFloat("_DivPower", 4f);
		for (int i = 0; i < outlineSprites.Length; i++)
		{
			if ((bool)outlineSprites[i])
			{
				outlineSprites[i].usesOverrideMaterial = true;
				outlineSprites[i].renderer.sharedMaterial = material2;
			}
		}
	}

	private Material GetDFAtlasMaterialForMetalGear(Material source, bool leftAligned)
	{
		Dictionary<Material, Material> dictionary = ((!leftAligned) ? MetalGearDFAtlasMapR : MetalGearDFAtlasMapL);
		Material material = null;
		if (dictionary.ContainsKey(source))
		{
			material = dictionary[source];
		}
		else
		{
			material = UnityEngine.Object.Instantiate(source);
			material.shader = ShaderCache.Acquire("Daikon Forge/Default UI Shader FadeRange");
			dictionary.Add(source, material);
		}
		return material;
	}

	private void SetFadeMaterials(dfSprite targetSprite, bool leftAligned)
	{
		Material material = (targetSprite.OverrideMaterial = GetDFAtlasMaterialForMetalGear(targetSprite.Atlas.Material, leftAligned));
	}

	private void SetFadeFractions(dfSprite targetSprite, float fadeScreenSpaceXStart, float fadeScreenSpaceXEnd, float fadeScreenSpaceY, bool isLeftAligned)
	{
		Material dFAtlasMaterialForMetalGear = GetDFAtlasMaterialForMetalGear(targetSprite.Atlas.Material, isLeftAligned);
		dFAtlasMaterialForMetalGear.SetFloat("_YFadeStart", Mathf.Min(0.75f, fadeScreenSpaceY));
		dFAtlasMaterialForMetalGear.SetFloat("_YFadeEnd", 0.03f);
		dFAtlasMaterialForMetalGear.SetFloat("_XFadeStart", fadeScreenSpaceXStart);
		dFAtlasMaterialForMetalGear.SetFloat("_XFadeEnd", fadeScreenSpaceXEnd);
		targetSprite.OverrideMaterial = dFAtlasMaterialForMetalGear;
		dfMaterialCache.ForceUpdate(targetSprite.OverrideMaterial);
		targetSprite.Invalidate();
	}

	private IEnumerator HandleMetalGearGunSelect(PlayerController targetPlayer, int numToL)
	{
		GameUIAmmoController ammoController = ammoControllers[0];
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			ammoController = ammoControllers[targetPlayer.IsPrimaryPlayer ? 1 : 0];
		}
		BraveInput inputInstance = BraveInput.GetInstanceForPlayer(targetPlayer.PlayerIDX);
		while (ammoController.IsFlipping)
		{
			if (!inputInstance.ActiveActions.GunQuickEquipAction.IsPressed && !targetPlayer.ForceMetalGearMenu)
			{
				targetPlayer.DoQuickEquip();
				yield break;
			}
			yield return null;
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			ToggleItemPanels(false);
		}
		ClearGunName(targetPlayer.IsPrimaryPlayer);
		targetPlayer.SetInputOverride("metal gear");
		BraveTime.RegisterTimeScaleMultiplier(0.05f, base.gameObject);
		m_metalGearGunSelectActive = true;
		Tribool gunSelectPhase = Tribool.Unready;
		List<dfSprite> additionalGunFrames = ((!targetPlayer.IsPrimaryPlayer) ? additionalGunBoxesSecondary : additionalGunBoxes);
		GunInventory playerInventory = targetPlayer.inventory;
		List<Gun> playerGuns = playerInventory.AllGuns;
		dfSprite baseBoxSprite = ammoController.GunBoxSprite;
		if (playerGuns.Count <= 1)
		{
			m_metalGearGunSelectActive = false;
			yield break;
		}
		Vector3 originalBaseBoxSpriteRelativePosition = baseBoxSprite.RelativePosition;
		dfSprite boxToMoveOffTop2 = null;
		dfSprite boxToMoveOffBottom2 = null;
		int totalGunShift2 = 0;
		float totalTimeMetalGeared = 0f;
		bool isTransitioning = false;
		int queuedTransition = 0;
		float transitionSpeed = 12.5f;
		float boxWidth = baseBoxSprite.Size.x + 3f;
		List<tk2dSprite> noAmmoIcons = new List<tk2dSprite>();
		Dictionary<dfSprite, Gun> frameToGunMap = new Dictionary<dfSprite, Gun>();
		int lastShiftDir3 = 1;
		Pixelator.Instance.FadeColor = Color.black;
		bool triedQueueLeft2 = false;
		bool triedQueueRight2 = false;
		bool prevStickLeft = true;
		bool prevStickRight = true;
		float ignoreStickTimer = 0f;
		bool isLeftAligned = targetPlayer.IsPrimaryPlayer && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER;
		while (m_metalGearGunSelectActive)
		{
			Pixelator.Instance.fade = 1f - Mathf.Clamp01(totalTimeMetalGeared * 8f) * 0.5f;
			if ((!inputInstance.ActiveActions.GunQuickEquipAction.IsPressed && !GameManager.Instance.PrimaryPlayer.ForceMetalGearMenu) || GameManager.IsBossIntro || GameManager.Instance.IsPaused || GameManager.Instance.IsLoadingLevel)
			{
				m_metalGearGunSelectActive = false;
				break;
			}
			if (gunSelectPhase == Tribool.Unready)
			{
				GunventoryFolded = false;
				StartCoroutine(HandlePauseInventoryFolding(targetPlayer, true, false, 0.1f, numToL));
				yield return null;
				for (int i = 0; i < additionalGunFrames.Count; i++)
				{
					dfSprite dfSprite2 = additionalGunFrames[i];
					dfSprite component = dfSprite2.transform.GetChild(0).GetComponent<dfSprite>();
					SetFadeMaterials(dfSprite2, isLeftAligned);
					SetFadeMaterials(component, isLeftAligned);
					float y = dfSprite2.GUIManager.RenderCamera.WorldToViewportPoint(baseBoxSprite.transform.position + new Vector3(0f, baseBoxSprite.Size.y * (float)(additionalGunFrames.Count - (Mathf.Abs(numToL) + ((numToL != 0) ? (-1) : 0))) * baseBoxSprite.PixelsToUnits(), 0f)).y;
					float num = 0f;
					float num2 = 1f;
					if (numToL < 0)
					{
						num = dfSprite2.GUIManager.RenderCamera.WorldToViewportPoint(additionalGunFrames[0].transform.position + new Vector3(boxWidth * -2f * baseBoxSprite.PixelsToUnits(), 0f, 0f)).x;
					}
					else if (numToL > 0)
					{
						num2 = dfSprite2.GUIManager.RenderCamera.WorldToViewportPoint(additionalGunFrames[0].transform.position + new Vector3(boxWidth * 2f * baseBoxSprite.PixelsToUnits(), 0f, 0f)).x;
					}
					tk2dClippedSprite componentInChildren = dfSprite2.GetComponentInChildren<tk2dClippedSprite>();
					AssignClippedSpriteFadeFractions(componentInChildren, y, num, num2, isLeftAligned);
					frameToGunMap.Add(dfSprite2, playerGuns[(i + playerGuns.IndexOf(playerInventory.CurrentGun)) % playerGuns.Count]);
					if (frameToGunMap[dfSprite2].CurrentAmmo == 0)
					{
						componentInChildren.renderer.material.SetFloat("_Saturation", 0f);
						tk2dSprite component2 = componentInChildren.transform.Find("NoAmmoIcon").GetComponent<tk2dSprite>();
						component2.transform.parent = dfSprite2.transform;
						component2.HeightOffGround = 2f;
						component2.OverrideMaterialMode = tk2dBaseSprite.SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_SIMPLE;
						component2.renderer.material.shader = ShaderCache.Acquire("tk2d/BlendVertexColorFadeRange");
						component2.renderer.material.SetFloat("_YFadeStart", Mathf.Min(0.75f, y));
						component2.renderer.material.SetFloat("_YFadeEnd", 0.03f);
						component2.renderer.material.SetFloat("_XFadeStart", num);
						component2.renderer.material.SetFloat("_XFadeEnd", num2);
						component2.scale = componentInChildren.scale;
						component2.transform.position = dfSprite2.GetCenter().Quantize(0.0625f * component2.scale.x);
						noAmmoIcons.Add(component2);
					}
					SetFadeFractions(dfSprite2, num, num2, y, isLeftAligned);
					SetFadeFractions(component, num, num2, y, isLeftAligned);
					dfSprite2.Invalidate();
				}
				gunSelectPhase = Tribool.Ready;
			}
			else if (gunSelectPhase == Tribool.Ready)
			{
				if (!isTransitioning)
				{
					if (triedQueueLeft2 || queuedTransition > 0)
					{
						isTransitioning = true;
						queuedTransition = Mathf.Max(queuedTransition - 1, 0);
						totalGunShift2--;
						lastShiftDir3 = 1;
						if (boxToMoveOffTop2 != null)
						{
							UnityEngine.Object.Destroy(boxToMoveOffTop2.gameObject);
							boxToMoveOffTop2 = null;
						}
						dfSprite dfSprite3 = additionalGunFrames[additionalGunFrames.Count - 1];
						if (numToL != 0 && additionalGunFrames.Count > 2)
						{
							dfSprite3 = additionalGunFrames[additionalGunFrames.Count - 2];
						}
						GameObject gameObject = UnityEngine.Object.Instantiate(dfSprite3.gameObject, dfSprite3.transform.position, Quaternion.identity);
						boxToMoveOffTop2 = gameObject.GetComponent<dfSprite>();
						dfSprite3.Parent.AddControl(boxToMoveOffTop2);
						boxToMoveOffTop2.RelativePosition = dfSprite3.RelativePosition;
						dfSprite component3 = boxToMoveOffTop2.transform.GetChild(0).GetComponent<dfSprite>();
						if (numToL != 0 && additionalGunFrames.Count > 2)
						{
							dfSprite3.RelativePosition = originalBaseBoxSpriteRelativePosition.WithX(originalBaseBoxSpriteRelativePosition.x + boxWidth * 2f * Mathf.Sign(numToL));
						}
						else
						{
							dfSprite3.RelativePosition = originalBaseBoxSpriteRelativePosition.WithY(originalBaseBoxSpriteRelativePosition.y + baseBoxSprite.Size.y);
						}
						SetFadeMaterials(boxToMoveOffTop2, isLeftAligned);
						SetFadeMaterials(component3, isLeftAligned);
						boxToMoveOffTop2.Invalidate();
						additionalGunFrames.Insert(0, additionalGunFrames[additionalGunFrames.Count - 1]);
						additionalGunFrames.RemoveAt(additionalGunFrames.Count - 1);
					}
					else if (triedQueueRight2 || queuedTransition < 0)
					{
						isTransitioning = true;
						queuedTransition = Mathf.Min(queuedTransition + 1, 0);
						totalGunShift2++;
						lastShiftDir3 = -1;
						if (boxToMoveOffBottom2 != null)
						{
							UnityEngine.Object.Destroy(boxToMoveOffBottom2.gameObject);
							boxToMoveOffBottom2 = null;
						}
						dfSprite dfSprite4 = additionalGunFrames[0];
						if (numToL != 0 && additionalGunFrames.Count > 2)
						{
							dfSprite4 = additionalGunFrames[additionalGunFrames.Count - 1];
						}
						GameObject gameObject2 = UnityEngine.Object.Instantiate(dfSprite4.gameObject, dfSprite4.transform.position, Quaternion.identity);
						boxToMoveOffBottom2 = gameObject2.GetComponent<dfSprite>();
						dfSprite4.Parent.AddControl(boxToMoveOffBottom2);
						boxToMoveOffBottom2.RelativePosition = dfSprite4.RelativePosition;
						dfSprite component4 = boxToMoveOffBottom2.transform.GetChild(0).GetComponent<dfSprite>();
						if (numToL != 0 && additionalGunFrames.Count > 2)
						{
							dfSprite4.RelativePosition = originalBaseBoxSpriteRelativePosition.WithY(originalBaseBoxSpriteRelativePosition.y - baseBoxSprite.Size.y * (float)(additionalGunFrames.Count - 1));
						}
						else
						{
							dfSprite4.RelativePosition = originalBaseBoxSpriteRelativePosition.WithY(originalBaseBoxSpriteRelativePosition.y - baseBoxSprite.Size.y * (float)additionalGunFrames.Count);
						}
						SetFadeMaterials(boxToMoveOffBottom2, isLeftAligned);
						SetFadeMaterials(component4, isLeftAligned);
						boxToMoveOffBottom2.Invalidate();
						additionalGunFrames.Add(additionalGunFrames[0]);
						additionalGunFrames.RemoveAt(0);
					}
				}
				else if (isTransitioning)
				{
					if (triedQueueLeft2)
					{
						queuedTransition++;
						triedQueueLeft2 = false;
					}
					else if (triedQueueRight2)
					{
						queuedTransition--;
						triedQueueRight2 = false;
					}
				}
				bool flag = true;
				for (int j = 0; j < additionalGunFrames.Count; j++)
				{
					dfSprite dfSprite5 = additionalGunFrames[j];
					float num3 = 1f / (float)(additionalGunFrames.Count + 1);
					Vector3 vector = originalBaseBoxSpriteRelativePosition - baseBoxSprite.Size.WithX(0f).ToVector3ZUp() * (j - 1);
					Vector3 b = vector - baseBoxSprite.Size.WithX(0f).ToVector3ZUp();
					if (numToL != 0 && additionalGunFrames.Count > 2 && j == additionalGunFrames.Count - 1)
					{
						vector = originalBaseBoxSpriteRelativePosition;
						b = vector + new Vector3(boxWidth, 0f, 0f) * Mathf.Sign(numToL);
					}
					float num4 = num3 * (float)j;
					float t = Mathf.Clamp01((1f - num4) / num3);
					float t2 = Mathf.SmoothStep(0f, 1f, t);
					Vector3 vector2 = Vector3.Lerp(vector, b, t2);
					if (dfSprite5.RelativePosition.IntXY() != vector2.IntXY())
					{
						flag = false;
					}
					float num5 = m_deltaTime * baseBoxSprite.Size.y * transitionSpeed;
					float maxDeltaX = num5 * (baseBoxSprite.Size.x / baseBoxSprite.Size.y);
					dfSprite5.RelativePosition = BraveMathCollege.LShapedMoveTowards(dfSprite5.RelativePosition, vector2, maxDeltaX, num5);
				}
				if (flag)
				{
					isTransitioning = false;
				}
				if (boxToMoveOffTop2 != null)
				{
					Vector3 vector3 = originalBaseBoxSpriteRelativePosition - baseBoxSprite.Size.WithX(0f).ToVector3ZUp() * (additionalGunFrames.Count - 1 - Mathf.Abs(numToL));
					Vector3 vector4 = vector3 - baseBoxSprite.Size.WithX(0f).ToVector3ZUp();
					float num6 = m_deltaTime * baseBoxSprite.Size.y * transitionSpeed;
					float maxDeltaX2 = num6 * (baseBoxSprite.Size.x / baseBoxSprite.Size.y);
					boxToMoveOffTop2.RelativePosition = BraveMathCollege.LShapedMoveTowards(boxToMoveOffTop2.RelativePosition, vector4, maxDeltaX2, num6);
					if (boxToMoveOffTop2.RelativePosition.IntXY() == vector4.IntXY())
					{
						UnityEngine.Object.Destroy(boxToMoveOffTop2.gameObject);
						boxToMoveOffTop2 = null;
					}
				}
				if (boxToMoveOffBottom2 != null)
				{
					Vector3 vector5 = originalBaseBoxSpriteRelativePosition + baseBoxSprite.Size.WithX(0f).ToVector3ZUp();
					if (numToL != 0 && additionalGunFrames.Count > 2)
					{
						Vector3 vector6 = originalBaseBoxSpriteRelativePosition + new Vector3(boxWidth * Mathf.Sign(numToL), 0f, 0f);
						vector5 = vector6 + new Vector3(boxWidth * Mathf.Sign(numToL), 0f, 0f);
					}
					float num7 = m_deltaTime * baseBoxSprite.Size.y * transitionSpeed;
					float maxDeltaX3 = num7 * (baseBoxSprite.Size.x / baseBoxSprite.Size.y);
					boxToMoveOffBottom2.RelativePosition = BraveMathCollege.LShapedMoveTowards(boxToMoveOffBottom2.RelativePosition, vector5, maxDeltaX3, num7);
					if (boxToMoveOffBottom2.RelativePosition.IntXY() == vector5.IntXY())
					{
						UnityEngine.Object.Destroy(boxToMoveOffBottom2.gameObject);
						boxToMoveOffBottom2 = null;
					}
				}
			}
			GungeonActions currentActions = inputInstance.ActiveActions;
			InputDevice currentDevice = inputInstance.ActiveActions.Device;
			bool gunUp = inputInstance.IsKeyboardAndMouse(true) && currentActions.GunUpAction.WasPressed;
			bool gunDown = inputInstance.IsKeyboardAndMouse(true) && currentActions.GunDownAction.WasPressed;
			if (targetPlayer.ForceMetalGearMenu)
			{
				gunUp = true;
			}
			if (!gunUp && !gunDown && currentDevice != null && (!inputInstance.IsKeyboardAndMouse(true) || GameManager.Options.AllowMoveKeysToChangeGuns))
			{
				bool flag2 = currentDevice.DPadRight.WasPressedRepeating || currentDevice.DPadUp.WasPressedRepeating;
				bool flag3 = currentDevice.DPadLeft.WasPressedRepeating || currentDevice.DPadDown.WasPressedRepeating;
				if (flag2 || flag3)
				{
					ignoreStickTimer = 0.25f;
				}
				bool flag4 = false;
				bool flag5 = false;
				if (ignoreStickTimer <= 0f)
				{
					flag4 |= currentDevice.LeftStickDown.RawValue > 0.4f || currentActions.Down.RawValue > 0.4f;
					flag5 |= currentDevice.LeftStickUp.RawValue > 0.4f || currentActions.Up.RawValue > 0.4f;
					flag4 |= currentDevice.LeftStickLeft.RawValue > 0.4f || currentActions.Left.RawValue > 0.4f;
					flag5 |= currentDevice.LeftStickRight.RawValue > 0.4f || currentActions.Right.RawValue > 0.4f;
				}
				triedQueueLeft2 = flag3 || (flag4 && !prevStickLeft);
				triedQueueRight2 = flag2 || (flag5 && !prevStickRight);
				prevStickLeft = flag4;
				prevStickRight = flag5;
			}
			else
			{
				triedQueueLeft2 = gunUp;
				triedQueueRight2 = gunDown;
			}
			yield return null;
			ignoreStickTimer = Mathf.Max(0f, ignoreStickTimer - GameManager.INVARIANT_DELTA_TIME);
			totalTimeMetalGeared += GameManager.INVARIANT_DELTA_TIME;
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			ToggleItemPanels(true);
		}
		Pixelator.Instance.fade = 1f;
		if (boxToMoveOffTop2 != null)
		{
			UnityEngine.Object.Destroy(boxToMoveOffTop2.gameObject);
			boxToMoveOffTop2 = null;
		}
		if (boxToMoveOffBottom2 != null)
		{
			UnityEngine.Object.Destroy(boxToMoveOffBottom2.gameObject);
			boxToMoveOffBottom2 = null;
		}
		totalGunShift2 -= queuedTransition;
		if (totalGunShift2 % targetPlayer.inventory.AllGuns.Count != 0)
		{
			targetPlayer.CacheQuickEquipGun();
			targetPlayer.ChangeGun(totalGunShift2, true);
			ammoController.SuppressNextGunFlip = true;
		}
		else
		{
			TemporarilyShowGunName(targetPlayer.IsPrimaryPlayer);
		}
		BraveTime.ClearMultiplier(base.gameObject);
		targetPlayer.ClearInputOverride("metal gear");
		m_metalGearGunSelectActive = false;
		if (totalGunShift2 == 0 && totalTimeMetalGeared < 0.005f)
		{
			targetPlayer.DoQuickEquip();
		}
		for (int k = 0; k < noAmmoIcons.Count; k++)
		{
			UnityEngine.Object.Destroy(noAmmoIcons[k].gameObject);
		}
		GunventoryFolded = true;
		yield return StartCoroutine(HandlePauseInventoryFolding(targetPlayer, true, false, 0.25f, numToL, true));
		ammoController.GunAmmoCountLabel.IsVisible = true;
	}

	private IEnumerator HandlePauseInventoryFolding(PlayerController targetPlayer, bool doGuns = true, bool doItems = true, float overrideTransitionTime = -1f, int numToL = 0, bool forceUseExistingList = false)
	{
		if (targetPlayer.IsGhost)
		{
			yield break;
		}
		GunInventory playerInventory = targetPlayer.inventory;
		List<Gun> playerGuns = playerInventory.AllGuns;
		List<PlayerItem> playerItems = targetPlayer.activeItems;
		GameUIAmmoController ammoController = ammoControllers[0];
		GameUIItemController itemController = itemControllers[0];
		List<dfSprite> additionalGunFrames = ((!targetPlayer.IsPrimaryPlayer) ? additionalGunBoxesSecondary : additionalGunBoxes);
		List<dfSprite> additionalItemFrames = ((!targetPlayer.IsPrimaryPlayer) ? additionalItemBoxesSecondary : additionalItemBoxes);
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			ammoController = ammoControllers[targetPlayer.IsPrimaryPlayer ? 1 : 0];
			itemController = itemControllers[(!targetPlayer.IsPrimaryPlayer) ? 1 : 0];
		}
		ammoController.GunAmmoCountLabel.IsVisible = GunventoryFolded && ammoController.GunBoxSprite.IsVisible;
		bool cachedFoldedness = GunventoryFolded;
		float transitionTime = ((!GunventoryFolded) ? 0.4f : 0.2f);
		if (overrideTransitionTime > 0f)
		{
			transitionTime = overrideTransitionTime;
		}
		float elapsed = 0f;
		if (!forceUseExistingList)
		{
			DestroyAdditionalFrames(GunventoryFolded, (!doGuns) ? null : ammoController, (!doItems) ? null : itemController, (!doGuns) ? null : additionalGunFrames, (!doItems) ? null : additionalItemFrames);
		}
		List<tk2dClippedSprite> gunSpritesByBox = new List<tk2dClippedSprite>();
		Dictionary<tk2dClippedSprite, tk2dClippedSprite[]> gunToOutlineMap = new Dictionary<tk2dClippedSprite, tk2dClippedSprite[]>();
		List<tk2dClippedSprite> itemSpritesByBox = new List<tk2dClippedSprite>();
		Dictionary<tk2dClippedSprite, tk2dClippedSprite[]> itemToOutlineMap = new Dictionary<tk2dClippedSprite, tk2dClippedSprite[]>();
		dfSprite baseBoxSprite = ammoController.GunBoxSprite;
		dfSprite baseItemBoxSprite = itemController.ItemBoxSprite;
		bool isLeftAligned = targetPlayer.IsPrimaryPlayer && GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER;
		if (doGuns)
		{
			baseBoxSprite.IsVisible = GunventoryFolded;
			if (GunventoryFolded)
			{
				ammoController.UndimGunSprite();
			}
			else
			{
				ammoController.DimGunSprite();
			}
		}
		if (doItems)
		{
			baseItemBoxSprite.IsVisible = GunventoryFolded;
			if (GunventoryFolded)
			{
				itemController.UndimItemSprite();
			}
			else
			{
				itemController.DimItemSprite();
			}
		}
		if (GunventoryFolded)
		{
			Transform parent = ammoController.GunBoxSprite.transform.parent;
			Transform parent2 = itemController.ItemBoxSprite.transform.parent;
			if (doGuns)
			{
				if (!forceUseExistingList)
				{
					for (int i = 0; i < playerGuns.Count; i++)
					{
						Transform transform = parent.Find("AdditionalWeaponBox" + IntToStringSansGarbage.GetStringForInt(i));
						if (transform != null)
						{
							dfSprite component = transform.GetComponent<dfSprite>();
							additionalGunFrames.Add(component);
							dfSprite component2 = component.transform.GetChild(0).GetComponent<dfSprite>();
							component2.IsVisible = targetPlayer.IsQuickEquipGun(playerGuns[i]);
							tk2dClippedSprite component3 = component.transform.Find("AdditionalGunSprite").GetComponent<tk2dClippedSprite>();
							gunSpritesByBox.Add(component3);
							gunToOutlineMap.Add(component3, SpriteOutlineManager.GetOutlineSprites<tk2dClippedSprite>(component3));
						}
					}
				}
				else
				{
					for (int j = 0; j < additionalGunFrames.Count; j++)
					{
						dfSprite dfSprite2 = additionalGunFrames[j];
						dfSprite component4 = dfSprite2.transform.GetChild(0).GetComponent<dfSprite>();
						component4.IsVisible = false;
						tk2dClippedSprite component5 = dfSprite2.transform.Find("AdditionalGunSprite").GetComponent<tk2dClippedSprite>();
						gunSpritesByBox.Add(component5);
						gunToOutlineMap.Add(component5, SpriteOutlineManager.GetOutlineSprites<tk2dClippedSprite>(component5));
					}
				}
			}
			if (doItems)
			{
				Transform transform2 = parent2.Find("AdditionalItemBox" + IntToStringSansGarbage.GetStringForInt(0));
				int num = 0;
				while ((bool)transform2 && num < 50)
				{
					dfSprite component6 = transform2.GetComponent<dfSprite>();
					additionalItemFrames.Add(component6);
					tk2dClippedSprite component7 = component6.transform.Find("AdditionalItemSprite").GetComponent<tk2dClippedSprite>();
					itemSpritesByBox.Add(component7);
					itemToOutlineMap.Add(component7, SpriteOutlineManager.GetOutlineSprites<tk2dClippedSprite>(component7));
					num++;
					transform2 = parent2.Find("AdditionalItemBox" + IntToStringSansGarbage.GetStringForInt(num));
				}
			}
		}
		else
		{
			int num2 = 0;
			if (doGuns)
			{
				int num3 = playerGuns.IndexOf(targetPlayer.CurrentGun);
				int num4 = num3 + playerGuns.Count;
				for (int k = num3; k < num4; k++)
				{
					int num5 = k % playerGuns.Count;
					if (num5 < 0 || num5 >= playerGuns.Count)
					{
						continue;
					}
					dfSprite component8 = UnityEngine.Object.Instantiate(baseBoxSprite.gameObject).GetComponent<dfSprite>();
					component8.IsVisible = true;
					component8.enabled = true;
					baseBoxSprite.Parent.AddControl(component8);
					component8.RelativePosition = baseBoxSprite.RelativePosition;
					component8.gameObject.name = "AdditionalWeaponBox" + IntToStringSansGarbage.GetStringForInt(num2);
					component8.FillDirection = dfFillDirection.Vertical;
					component8.FillAmount = 0f;
					component8.OverrideMaterial = GetDFAtlasMaterialForMetalGear(component8.Atlas.Material, isLeftAligned);
					component8.OverrideMaterial.SetFloat("_YFadeStart", 0.75f);
					tk2dBaseSprite tk2dBaseSprite2 = playerGuns[num5].GetSprite();
					GameObject gameObject = new GameObject("AdditionalGunSprite");
					tk2dClippedSprite tk2dClippedSprite2 = tk2dBaseSprite.AddComponent<tk2dClippedSprite>(gameObject, tk2dBaseSprite2.Collection, tk2dBaseSprite2.spriteId);
					tk2dClippedSprite2.scale = GameUIUtility.GetCurrentTK2D_DFScale(m_manager) * Vector3.one;
					Vector3 center = component8.GetCenter();
					tk2dClippedSprite2.transform.position = center + ammoController.GetOffsetVectorForGun(playerGuns[num5], false);
					tk2dClippedSprite2.transform.position = tk2dClippedSprite2.transform.position.Quantize(component8.PixelsToUnits() * 3f);
					gameObject.transform.parent = component8.transform;
					gameObject.SetLayerRecursively(LayerMask.NameToLayer("GUI"));
					tk2dClippedSprite2.ignoresTiltworldDepth = true;
					Material overrideOutlineMaterial = null;
					Texture mainTexture = tk2dClippedSprite2.renderer.sharedMaterial.mainTexture;
					Dictionary<Texture, Material> dictionary = ((!isLeftAligned) ? MetalGearAtlasToFadeMaterialMapR : MetalGearAtlasToFadeMaterialMapL);
					Dictionary<Material, Material> dictionary2 = ((!isLeftAligned) ? MetalGearFadeToOutlineMaterialMapR : MetalGearFadeToOutlineMaterialMapL);
					if (dictionary.ContainsKey(mainTexture))
					{
						Material key = dictionary[mainTexture];
						if (dictionary2.ContainsKey(key))
						{
							overrideOutlineMaterial = dictionary2[key];
						}
					}
					SpriteOutlineManager.AddOutlineToSprite<tk2dClippedSprite>(tk2dClippedSprite2, Color.white, overrideOutlineMaterial);
					tk2dClippedSprite[] outlineSprites = SpriteOutlineManager.GetOutlineSprites<tk2dClippedSprite>(tk2dClippedSprite2);
					for (int l = 0; l < outlineSprites.Length; l++)
					{
						outlineSprites[l].scale = tk2dClippedSprite2.scale;
					}
					dfSprite component9 = component8.transform.GetChild(0).GetComponent<dfSprite>();
					component9.IsVisible = targetPlayer.IsQuickEquipGun(playerGuns[num5]);
					additionalGunFrames.Add(component8);
					gunSpritesByBox.Add(tk2dClippedSprite2);
					gunToOutlineMap.Add(tk2dClippedSprite2, outlineSprites);
					num2++;
					AssignClippedSpriteFadeFractions(tk2dClippedSprite2, 0.75f, 0f, 1f, isLeftAligned);
					if (doGuns && !doItems && playerGuns[num5].CurrentAmmo == 0)
					{
						tk2dClippedSprite2.renderer.material.SetFloat("_Saturation", 0f);
						tk2dSprite component10 = ((GameObject)UnityEngine.Object.Instantiate(BraveResources.Load("Global Prefabs/NoAmmoIcon"))).GetComponent<tk2dSprite>();
						component10.name = "NoAmmoIcon";
						component10.transform.parent = tk2dClippedSprite2.transform;
						component10.HeightOffGround = 2f;
						component10.OverrideMaterialMode = tk2dBaseSprite.SpriteMaterialOverrideMode.OVERRIDE_MATERIAL_SIMPLE;
						component10.renderer.material.shader = ShaderCache.Acquire("tk2d/BlendVertexColorFadeRange");
						component10.scale = tk2dClippedSprite2.scale;
						component10.transform.position = component8.GetCenter().Quantize(0.0625f * component10.scale.x);
						SpriteOutlineManager.RemoveOutlineFromSprite(tk2dClippedSprite2, true);
					}
				}
			}
			num2 = 0;
			if (doItems)
			{
				int num6 = playerItems.IndexOf(targetPlayer.CurrentItem);
				int num7 = num6 + playerItems.Count;
				for (int m = num6; m < num7; m++)
				{
					int num8 = m % playerItems.Count;
					if (num8 >= 0 && num8 < playerItems.Count)
					{
						dfSprite component11 = UnityEngine.Object.Instantiate(baseItemBoxSprite.gameObject).GetComponent<dfSprite>();
						baseItemBoxSprite.Parent.AddControl(component11);
						component11.RelativePosition = baseItemBoxSprite.RelativePosition;
						component11.gameObject.name = "AdditionalItemBox" + IntToStringSansGarbage.GetStringForInt(num2);
						component11.FillDirection = dfFillDirection.Vertical;
						component11.FillAmount = 0f;
						tk2dBaseSprite tk2dBaseSprite3 = playerItems[num8].sprite;
						GameObject gameObject2 = new GameObject("AdditionalItemSprite");
						tk2dClippedSprite tk2dClippedSprite3 = tk2dBaseSprite.AddComponent<tk2dClippedSprite>(gameObject2, tk2dBaseSprite3.Collection, tk2dBaseSprite3.spriteId);
						tk2dClippedSprite3.scale = GameUIUtility.GetCurrentTK2D_DFScale(m_manager) * Vector3.one;
						Vector3 center2 = component11.GetCenter();
						tk2dClippedSprite3.transform.position = center2 + itemController.GetOffsetVectorForItem(playerItems[num8], false);
						tk2dClippedSprite3.transform.position = tk2dClippedSprite3.transform.position.Quantize(component11.PixelsToUnits() * 3f);
						gameObject2.transform.parent = component11.transform;
						gameObject2.SetLayerRecursively(LayerMask.NameToLayer("GUI"));
						tk2dClippedSprite3.ignoresTiltworldDepth = true;
						SpriteOutlineManager.AddOutlineToSprite<tk2dClippedSprite>(tk2dClippedSprite3, Color.white);
						tk2dClippedSprite[] outlineSprites2 = SpriteOutlineManager.GetOutlineSprites<tk2dClippedSprite>(tk2dClippedSprite3);
						for (int n = 0; n < outlineSprites2.Length; n++)
						{
							outlineSprites2[n].scale = tk2dClippedSprite3.scale;
						}
						additionalItemFrames.Add(component11);
						itemSpritesByBox.Add(tk2dClippedSprite3);
						itemToOutlineMap.Add(tk2dClippedSprite3, outlineSprites2);
						num2++;
					}
				}
			}
		}
		while (elapsed < transitionTime)
		{
			if (cachedFoldedness != GunventoryFolded)
			{
				yield break;
			}
			elapsed += m_deltaTime;
			float p2u = gunNameLabels[0].PixelsToUnits();
			float t = Mathf.Clamp01(elapsed / transitionTime);
			if (GunventoryFolded)
			{
				t = 1f - t;
			}
			if (doGuns)
			{
				for (int num9 = 0; num9 < additionalGunFrames.Count; num9++)
				{
					float num10 = 1f / (float)additionalGunFrames.Count;
					Vector3 vector = baseBoxSprite.RelativePosition - baseBoxSprite.Size.WithX(0f).ToVector3ZUp() * (num9 - 1);
					Vector3 b = vector - baseBoxSprite.Size.WithX(0f).ToVector3ZUp();
					if (numToL != 0 && additionalGunFrames.Count > 2 && num9 == additionalGunFrames.Count - 1)
					{
						vector = baseBoxSprite.RelativePosition;
						b = vector + new Vector3(baseBoxSprite.Size.x + 3f, 0f, 0f) * Mathf.Sign(numToL);
					}
					float num11 = num10 * (float)num9;
					float num12 = Mathf.Clamp01((t - num11) / num10);
					float num13 = Mathf.SmoothStep(0f, 1f, num12);
					if (num9 == 0)
					{
						num12 = ((!GunventoryFolded) ? 1 : 0);
					}
					if (num9 == 0)
					{
						num13 = ((!GunventoryFolded) ? 1 : 0);
					}
					if (numToL != 0 && additionalGunFrames.Count > 2 && num9 == additionalGunFrames.Count - 1)
					{
						additionalGunFrames[num9].FillDirection = dfFillDirection.Horizontal;
						additionalGunFrames[num9].FillAmount = num13;
					}
					else
					{
						additionalGunFrames[num9].FillDirection = dfFillDirection.Vertical;
						additionalGunFrames[num9].FillAmount = num13;
					}
					additionalGunFrames[num9].IsVisible = additionalGunFrames[num9].FillAmount > 0f;
					tk2dClippedSprite tk2dClippedSprite4 = gunSpritesByBox[num9];
					if (tk2dClippedSprite4 != null)
					{
						if (numToL != 0 && additionalGunFrames.Count > 2 && num9 == additionalGunFrames.Count - 1)
						{
							float num14 = tk2dClippedSprite4.GetUntrimmedBounds().size.x / (additionalGunFrames[num9].Size.x * p2u);
							float num15 = (1f - num14) / 2f;
							float x = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((num12 - num15) / num14));
							tk2dClippedSprite4.clipTopRight = new Vector2(x, 1f);
							if (gunToOutlineMap[tk2dClippedSprite4] != null)
							{
								for (int num16 = 0; num16 < gunToOutlineMap[tk2dClippedSprite4].Length; num16++)
								{
									if ((bool)gunToOutlineMap[tk2dClippedSprite4][num16])
									{
										gunToOutlineMap[tk2dClippedSprite4][num16].clipTopRight = new Vector2(x, 1f);
									}
								}
							}
						}
						else
						{
							float num17 = tk2dClippedSprite4.GetUntrimmedBounds().size.y / (additionalGunFrames[num9].Size.y * p2u);
							float num18 = (1f - num17) / 2f;
							float num19 = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((num12 - num18) / num17));
							tk2dClippedSprite4.clipBottomLeft = new Vector2(0f, 1f - num19);
							if (gunToOutlineMap[tk2dClippedSprite4] != null)
							{
								for (int num20 = 0; num20 < gunToOutlineMap[tk2dClippedSprite4].Length; num20++)
								{
									if ((bool)gunToOutlineMap[tk2dClippedSprite4][num20])
									{
										gunToOutlineMap[tk2dClippedSprite4][num20].clipBottomLeft = new Vector2(0f, 1f - num19);
									}
								}
							}
						}
					}
					additionalGunFrames[num9].RelativePosition = Vector3.Lerp(vector, b, num13);
				}
			}
			if (doItems)
			{
				for (int num21 = 0; num21 < additionalItemFrames.Count; num21++)
				{
					float num22 = 1f / (float)additionalItemFrames.Count;
					Vector3 vector2 = baseItemBoxSprite.RelativePosition - baseItemBoxSprite.Size.WithX(0f).ToVector3ZUp() * (num21 - 1);
					Vector3 b2 = vector2 - baseItemBoxSprite.Size.WithX(0f).ToVector3ZUp();
					float num23 = num22 * (float)num21;
					float num24 = Mathf.Clamp01((t - num23) / num22);
					float num25 = Mathf.SmoothStep(0f, 1f, num24);
					if (num21 == 0)
					{
						num24 = ((!GunventoryFolded) ? 1 : 0);
					}
					if (num21 == 0)
					{
						num25 = ((!GunventoryFolded) ? 1 : 0);
					}
					additionalItemFrames[num21].FillAmount = num25;
					additionalItemFrames[num21].IsVisible = additionalItemFrames[num21].FillAmount > 0f;
					tk2dClippedSprite tk2dClippedSprite5 = itemSpritesByBox[num21];
					if (tk2dClippedSprite5 != null)
					{
						float num26 = tk2dClippedSprite5.GetUntrimmedBounds().size.y / (additionalItemFrames[num21].Size.y * p2u);
						float num27 = (1f - num26) / 2f;
						float num28 = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((num24 - num27) / num26));
						tk2dClippedSprite5.clipBottomLeft = new Vector2(0f, 1f - num28);
						for (int num29 = 0; num29 < itemToOutlineMap[tk2dClippedSprite5].Length; num29++)
						{
							if ((bool)itemToOutlineMap[tk2dClippedSprite5][num29])
							{
								itemToOutlineMap[tk2dClippedSprite5][num29].clipBottomLeft = new Vector2(0f, 1f - num28);
							}
						}
					}
					additionalItemFrames[num21].RelativePosition = Vector3.Lerp(vector2, b2, num25);
				}
			}
			yield return null;
		}
		if (GunventoryFolded)
		{
			DestroyAdditionalFrames(GunventoryFolded, (!doGuns) ? null : ammoController, (!doItems) ? null : itemController, (!doGuns) ? null : additionalGunFrames, (!doItems) ? null : additionalItemFrames, true);
		}
		ammoController.GunAmmoCountLabel.IsVisible = GunventoryFolded && ammoController.GunBoxSprite.IsVisible;
	}

	public void ToggleUICamera(bool enable)
	{
		gunNameLabels[0].GetManager().RenderCamera.enabled = enable;
	}

	public void UpdateScale()
	{
		for (int i = 0; i < heartControllers.Count; i++)
		{
			heartControllers[i].UpdateScale();
		}
		for (int j = 0; j < blankControllers.Count; j++)
		{
			blankControllers[j].UpdateScale();
		}
		for (int k = 0; k < ammoControllers.Count; k++)
		{
			ammoControllers[k].UpdateScale();
		}
		for (int l = 0; l < itemControllers.Count; l++)
		{
			itemControllers[l].UpdateScale();
		}
		for (int m = 0; m < gunNameLabels.Count; m++)
		{
			gunNameLabels[m].TextScale = Pixelator.Instance.CurrentTileScale;
		}
		for (int n = 0; n < itemNameLabels.Count; n++)
		{
			itemNameLabels[n].TextScale = Pixelator.Instance.CurrentTileScale;
		}
		if (m_manager != null)
		{
			m_manager.UIScale = Pixelator.Instance.ScaleTileScale / 3f * GameUIScalar;
		}
		if (OnScaleUpdate != null)
		{
			OnScaleUpdate();
		}
	}

	public void DisplayUndiePanel()
	{
		dfPanel component = undiePanel.GetComponent<dfPanel>();
		undiePanel.SetActive(true);
		component.ZOrder = 1500;
		dfGUIManager.PushModal(component);
	}

	public float PixelsToUnits()
	{
		return Manager.PixelsToUnits();
	}

	public void DoNotification(EncounterTrackable trackable)
	{
		notificationController.DoNotification(trackable);
	}

	public void UpdatePlayerBlankUI(PlayerController player)
	{
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER)
		{
			if (player.IsPrimaryPlayer)
			{
				blankControllers[0].UpdateBlanks(player.Blanks);
			}
			else
			{
				blankControllers[1].UpdateBlanks(player.Blanks);
			}
		}
	}

	private IEnumerator HandleGenericPositionLerp(dfControl targetControl, Vector3 delta, float duration)
	{
		float ela = 0f;
		Vector3 startPos = targetControl.RelativePosition;
		while (ela < duration)
		{
			ela += BraveTime.DeltaTime;
			targetControl.RelativePosition = Vector3.Lerp(startPos, startPos + delta, ela / duration);
			yield return null;
		}
	}

	public void TransitionToGhostUI(PlayerController player)
	{
	}

	public void UpdateGhostUI(PlayerController player)
	{
		if (!player.IsGhost)
		{
		}
	}

	public void UpdatePlayerHealthUI(PlayerController player, HealthHaver hh)
	{
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER)
		{
			if (player.IsPrimaryPlayer)
			{
				heartControllers[0].UpdateHealth(hh);
			}
			else
			{
				heartControllers[1].UpdateHealth(hh);
			}
		}
	}

	public void SetAmmoCountColor(Color targetcolor, PlayerController sourcePlayer)
	{
		int index = 0;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			index = (sourcePlayer.IsPrimaryPlayer ? 1 : 0);
		}
		ammoControllers[index].SetAmmoCountLabelColor(targetcolor);
	}

	public void UpdateGunData(GunInventory inventory, int inventoryShift, PlayerController sourcePlayer)
	{
		if (!sourcePlayer.healthHaver.IsDead)
		{
			int num = 0;
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				num = (sourcePlayer.IsPrimaryPlayer ? 1 : 0);
			}
			UpdateGunDataInternal(sourcePlayer, inventory, inventoryShift, ammoControllers[num], num);
		}
	}

	private void UpdateGunDataInternal(PlayerController targetPlayer, GunInventory inventory, int inventoryShift, GameUIAmmoController targetAmmoController, int labelTarget)
	{
		Gun currentGun = inventory.CurrentGun;
		float num = gunNameLabels[labelTarget].PixelsToUnits();
		if (currentGun != null)
		{
			EncounterTrackable component = currentGun.GetComponent<EncounterTrackable>();
			gunNameLabels[labelTarget].Text = ((!(component != null)) ? currentGun.gunName : component.GetModifiedDisplayName());
		}
		else
		{
			gunNameLabels[labelTarget].Text = string.Empty;
		}
		targetAmmoController.UpdateUIGun(inventory, inventoryShift);
		if (inventoryShift != 0)
		{
			TemporarilyShowGunName(targetPlayer.IsPrimaryPlayer);
		}
		if (currentGun != null && currentGun.ClipShotsRemaining == 0 && (currentGun.ClipCapacity > 1 || currentGun.ammo == 0) && !currentGun.IsReloading && !targetPlayer.IsInputOverridden && !currentGun.IsHeroSword)
		{
			targetPlayer.gunReloadDisplayTimer += BraveTime.DeltaTime;
			if (targetPlayer.gunReloadDisplayTimer > 0.25f)
			{
				InformNeedsReload(targetPlayer, new Vector3(targetPlayer.specRigidbody.UnitCenter.x - targetPlayer.transform.position.x, 1.25f, 0f), -1f, string.Empty);
			}
		}
		else if (!m_isDisplayingCustomReload)
		{
			if (m_displayingReloadNeeded.Count < 2)
			{
				m_displayingReloadNeeded.Add(false);
			}
			targetPlayer.gunReloadDisplayTimer = 0f;
			m_displayingReloadNeeded[(!targetPlayer.IsPrimaryPlayer) ? 1 : 0] = false;
		}
		else
		{
			targetPlayer.gunReloadDisplayTimer = 0f;
		}
		m_gunNameVisibilityTimers[labelTarget] -= m_deltaTime;
		if (m_gunNameVisibilityTimers[labelTarget] > 1f)
		{
			gunNameLabels[labelTarget].IsVisible = true;
			gunNameLabels[labelTarget].Opacity = 1f;
		}
		else if (m_gunNameVisibilityTimers[labelTarget] > 0f)
		{
			gunNameLabels[labelTarget].IsVisible = true;
			gunNameLabels[labelTarget].Opacity = m_gunNameVisibilityTimers[labelTarget];
		}
		else
		{
			gunNameLabels[labelTarget].IsVisible = false;
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			itemControllers[0].transform.position = ammoControllers[1].GunBoxSprite.transform.position + new Vector3((3f + ammoControllers[1].GunBoxSprite.Width + (float)(2 * ammoControllers[1].AdditionalGunBoxSprites.Count)) * num, 0f, 0f);
			itemControllers[1].transform.position = ammoControllers[0].GunBoxSprite.transform.position + new Vector3((3f + ammoControllers[0].GunBoxSprite.Width + (float)(2 * ammoControllers[0].AdditionalGunBoxSprites.Count)) * -1f * num, 0f, 0f);
			if (itemControllers[(labelTarget == 0) ? 1 : 0].ItemBoxSprite.IsVisible)
			{
				gunNameLabels[labelTarget].transform.position = itemNameLabels[(labelTarget == 0) ? 1 : 0].transform.position + new Vector3(0f, -1f * (itemNameLabels[labelTarget].Height * num), 0f);
			}
			else if (targetAmmoController.IsLeftAligned)
			{
				gunNameLabels[labelTarget].transform.position = gunNameLabels[labelTarget].transform.position.WithX(targetAmmoController.GunBoxSprite.transform.position.x + (targetAmmoController.GunBoxSprite.Width + 4f) * num).WithY(targetAmmoController.GunBoxSprite.transform.position.y);
			}
			else
			{
				gunNameLabels[labelTarget].transform.position = gunNameLabels[labelTarget].transform.position.WithX(targetAmmoController.GunBoxSprite.transform.position.x - (targetAmmoController.GunBoxSprite.Width + 4f) * num).WithY(targetAmmoController.GunBoxSprite.transform.position.y);
			}
		}
		else if (targetAmmoController.IsLeftAligned)
		{
			gunNameLabels[labelTarget].transform.position = gunNameLabels[labelTarget].transform.position.WithX(targetAmmoController.GunBoxSprite.transform.position.x + (targetAmmoController.GunBoxSprite.Width + 4f) * num).WithY(targetAmmoController.GunBoxSprite.transform.position.y);
		}
		else
		{
			gunNameLabels[labelTarget].transform.position = gunNameLabels[labelTarget].transform.position.WithX(targetAmmoController.GunBoxSprite.transform.position.x - (targetAmmoController.GunBoxSprite.Width + 4f) * num).WithY(targetAmmoController.GunBoxSprite.transform.position.y);
		}
	}

	public void TemporarilyShowGunName(bool primaryPlayer)
	{
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES)
		{
			int num = 0;
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				num = (primaryPlayer ? 1 : 0);
			}
			m_gunNameVisibilityTimers[num] = 3f;
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				m_itemNameVisibilityTimers[(num == 0) ? 1 : 0] = 0f;
			}
		}
	}

	public void TemporarilyShowItemName(bool primaryPlayer)
	{
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.END_TIMES)
		{
			int num = ((!primaryPlayer) ? 1 : 0);
			m_itemNameVisibilityTimers[num] = 3f;
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				m_gunNameVisibilityTimers[(num == 0) ? 1 : 0] = 0f;
			}
		}
	}

	public void ClearGunName(bool primaryPlayer)
	{
		int num = 0;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			num = (primaryPlayer ? 1 : 0);
		}
		m_gunNameVisibilityTimers[num] = 0f;
		gunNameLabels[num].IsVisible = false;
	}

	public void ClearItemName(bool primaryPlayer)
	{
		int num = ((!primaryPlayer) ? 1 : 0);
		m_itemNameVisibilityTimers[num] = 0f;
		itemNameLabels[num].IsVisible = false;
	}

	public void UpdateItemData(PlayerController targetPlayer, PlayerItem item, List<PlayerItem> items)
	{
		int num = 0;
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			num = ((!targetPlayer.IsPrimaryPlayer) ? 1 : 0);
		}
		string text = string.Empty;
		if (item != null)
		{
			EncounterTrackable component = item.GetComponent<EncounterTrackable>();
			text = ((!(component != null)) ? item.DisplayName : component.journalData.GetPrimaryDisplayName());
			if (item.consumable && item.numberOfUses <= 1)
			{
			}
		}
		m_itemNameVisibilityTimers[num] -= m_deltaTime;
		if (m_itemNameVisibilityTimers[num] > 1f)
		{
			itemNameLabels[num].IsVisible = true;
			itemNameLabels[num].Opacity = 1f;
		}
		else if (m_itemNameVisibilityTimers[num] > 0f)
		{
			itemNameLabels[num].IsVisible = true;
			itemNameLabels[num].Opacity = m_itemNameVisibilityTimers[num];
		}
		else
		{
			itemNameLabels[num].IsVisible = false;
		}
		itemNameLabels[num].Text = text;
		GameUIItemController gameUIItemController = itemControllers[num];
		float num2 = gameUIItemController.ItemBoxSprite.PixelsToUnits();
		if (gameUIItemController.IsRightAligned)
		{
			itemNameLabels[num].transform.position = itemNameLabels[num].transform.position.WithX(gameUIItemController.ItemBoxSprite.transform.position.x + -4f * num2).WithY(gameUIItemController.ItemBoxSprite.transform.position.y + itemNameLabels[num].Height * num2);
		}
		else
		{
			itemNameLabels[num].transform.position = itemNameLabels[num].transform.position.WithX(gameUIItemController.ItemBoxSprite.transform.position.x + (gameUIItemController.ItemBoxSprite.Size.x + 4f) * num2).WithY(gameUIItemController.ItemBoxSprite.transform.position.y + itemNameLabels[num].Height * num2);
		}
		gameUIItemController.UpdateItem(item, items);
	}

	public void UpdatePlayerConsumables(PlayerConsumables playerConsumables)
	{
		p_playerCoinLabel.Text = IntToStringSansGarbage.GetStringForInt(playerConsumables.Currency);
		p_playerKeyLabel.Text = IntToStringSansGarbage.GetStringForInt(playerConsumables.KeyBullets);
		UpdateSpecialKeys(playerConsumables);
		if (GameManager.Instance.PrimaryPlayer != null && GameManager.Instance.PrimaryPlayer.Blanks == 0)
		{
			p_playerCoinLabel.Parent.Parent.RelativePosition = p_playerCoinLabel.Parent.Parent.RelativePosition.WithY(blankControllers[0].Panel.RelativePosition.y);
			p_playerKeyLabel.Parent.Parent.RelativePosition = p_playerKeyLabel.Parent.Parent.RelativePosition.WithY(blankControllers[0].Panel.RelativePosition.y);
		}
		else
		{
			p_playerCoinLabel.Parent.Parent.RelativePosition = p_playerCoinLabel.Parent.Parent.RelativePosition.WithY(blankControllers[0].Panel.RelativePosition.y + blankControllers[0].Panel.Height - 9f);
			p_playerKeyLabel.Parent.Parent.RelativePosition = p_playerKeyLabel.Parent.Parent.RelativePosition.WithY(blankControllers[0].Panel.RelativePosition.y + blankControllers[0].Panel.Height - 9f);
		}
		if (GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER)
		{
			return;
		}
		int num = Mathf.RoundToInt(GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.META_CURRENCY));
		if (num > 0)
		{
			p_playerCoinLabel.Text = IntToStringSansGarbage.GetStringForInt(num);
			if (p_playerCoinSprite == null)
			{
				p_playerCoinSprite = p_playerCoinLabel.Parent.GetComponentInChildren<dfSprite>();
			}
			p_playerCoinSprite.SpriteName = "hbux_text_icon";
			p_playerCoinSprite.Size = p_playerCoinSprite.SpriteInfo.sizeInPixels * 3f;
		}
		else
		{
			if (p_playerCoinSprite == null)
			{
				p_playerCoinSprite = p_playerCoinLabel.Parent.GetComponentInChildren<dfSprite>();
			}
			p_playerCoinLabel.IsVisible = false;
			p_playerCoinSprite.IsVisible = false;
		}
	}

	private void UpdateSpecialKeys(PlayerConsumables playerConsumables)
	{
		bool flag = false;
		bool flag2 = false;
		int resourcefulRatKeys = playerConsumables.ResourcefulRatKeys;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			for (int j = 0; j < playerController.additionalItems.Count; j++)
			{
				if (playerController.additionalItems[j] is NPCCellKeyItem)
				{
					flag = true;
				}
			}
			for (int k = 0; k < playerController.passiveItems.Count; k++)
			{
				if (playerController.passiveItems[k] is SpecialKeyItem)
				{
					SpecialKeyItem specialKeyItem = playerController.passiveItems[k] as SpecialKeyItem;
					if (specialKeyItem.keyType == SpecialKeyItem.SpecialKeyType.RESOURCEFUL_RAT_LAIR)
					{
						flag2 = true;
					}
				}
			}
		}
		int count = m_extantSpecialKeySprites.Count;
		int num = resourcefulRatKeys + (flag2 ? 1 : 0) + (flag ? 1 : 0);
		if (num == count)
		{
			return;
		}
		for (int l = 0; l < m_extantSpecialKeySprites.Count; l++)
		{
			UnityEngine.Object.Destroy(m_extantSpecialKeySprites[l].gameObject);
		}
		m_extantSpecialKeySprites.Clear();
		for (int m = 0; m < num; m++)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(p_specialKeySprite.gameObject);
			dfSprite component = gameObject.GetComponent<dfSprite>();
			component.IsVisible = true;
			p_specialKeySprite.Parent.AddControl(component);
			component.RelativePosition = p_specialKeySprite.RelativePosition + new Vector3(33 * m, 0f, 0f);
			m_extantSpecialKeySprites.Add(component);
			bool flag3 = flag && m == 0;
			bool flag4 = ((flag && flag2) ? (m == 1) : (flag2 && m == 0));
			bool flag5 = !flag3 && !flag4;
			if (!flag3)
			{
				if (flag4)
				{
					component.SpriteName = "resourcefulrat_key_001";
					component.RelativePosition += new Vector3(9f, 15f, 0f);
				}
				else if (flag5)
				{
					component.SpriteName = "room_rat_reward_key_001";
					component.RelativePosition += new Vector3(6f, 12f, 0f);
				}
			}
			component.Size = component.SpriteInfo.sizeInPixels * 3f;
		}
		p_playerCoinLabel.Parent.Parent.RelativePosition = p_playerCoinLabel.Parent.Parent.RelativePosition.WithX(p_playerCoinLabel.Parent.Parent.RelativePosition.x + (float)((num - count) * 33));
	}

	public bool AttemptActiveReload(PlayerController targetPlayer)
	{
		int index = ((!targetPlayer.IsPrimaryPlayer) ? 1 : 0);
		bool flag = m_extantReloadBars[index].AttemptActiveReload();
		if (flag)
		{
		}
		return flag;
	}

	public void DoHealthBarForEnemy(AIActor sourceEnemy)
	{
		if (m_enemyToHealthbarMap.ContainsKey(sourceEnemy))
		{
			m_enemyToHealthbarMap[sourceEnemy].Value = sourceEnemy.healthHaver.GetCurrentHealthPercentage();
		}
		else if (m_unusedHealthbars.Count <= 0)
		{
			dfControl dfControl2 = m_manager.AddPrefab((GameObject)BraveResources.Load("Global Prefabs/EnemyHealthBar"));
			dfFollowObject component = dfControl2.GetComponent<dfFollowObject>();
			component.mainCamera = GameManager.Instance.MainCameraController.GetComponent<Camera>();
			component.attach = sourceEnemy.gameObject;
			component.offset = new Vector3(0.5f, 2f, 0f);
			component.enabled = true;
			dfSlider component2 = component.GetComponent<dfSlider>();
			component2.Value = sourceEnemy.healthHaver.GetCurrentHealthPercentage();
			m_enemyToHealthbarMap.Add(sourceEnemy, component2);
		}
	}

	public void ForceClearReload(int targetPlayerIndex = -1)
	{
		for (int i = 0; i < m_extantReloadBars.Count; i++)
		{
			if (targetPlayerIndex == -1 || targetPlayerIndex == i)
			{
				m_extantReloadBars[i].CancelReload();
				m_extantReloadBars[i].UpdateStatusBars(null);
			}
		}
		for (int j = 0; j < m_displayingReloadNeeded.Count; j++)
		{
			if (targetPlayerIndex == -1 || targetPlayerIndex == j)
			{
				m_displayingReloadNeeded[j] = false;
			}
		}
	}

	public void InformNeedsReload(PlayerController attachPlayer, Vector3 offset, float customDuration = -1f, string customKey = "")
	{
		if (!attachPlayer)
		{
			return;
		}
		int num = ((!attachPlayer.IsPrimaryPlayer) ? 1 : 0);
		if (m_displayingReloadNeeded == null || num >= m_displayingReloadNeeded.Count || m_extantReloadLabels == null || num >= m_extantReloadLabels.Count || m_displayingReloadNeeded[num])
		{
			return;
		}
		dfLabel dfLabel2 = m_extantReloadLabels[num];
		if (!(dfLabel2 == null) && !dfLabel2.IsVisible)
		{
			dfFollowObject component = dfLabel2.GetComponent<dfFollowObject>();
			dfLabel2.IsVisible = true;
			if ((bool)component)
			{
				component.enabled = false;
			}
			StartCoroutine(FlashReloadLabel(dfLabel2, attachPlayer, offset, customDuration, customKey));
		}
	}

	protected override void InvariantUpdate(float realDeltaTime)
	{
		if (GameManager.Instance.IsLoadingLevel)
		{
			levelNameUI.BanishLevelNameText();
			return;
		}
		if (ForceLowerPanelsInvisibleOverride.HasOverride("conversation") && !GameManager.Instance.IsSelectingCharacter && GameManager.Instance.AllPlayers != null)
		{
			bool flag = true;
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				if ((bool)GameManager.Instance.AllPlayers[i] && GameManager.Instance.AllPlayers[i].CurrentInputState != 0)
				{
					flag = false;
				}
			}
			if (flag)
			{
				ToggleLowerPanels(true, false, "conversation");
			}
		}
		if (!m_displayingPlayerConversationOptions && ForceLowerPanelsInvisibleOverride.HasOverride("conversationBar"))
		{
			ToggleLowerPanels(true, false, "conversationBar");
		}
	}

	private void UpdateReloadLabelsOnCameraFinishedFrame()
	{
		for (int i = 0; i < m_displayingReloadNeeded.Count; i++)
		{
			if (m_displayingReloadNeeded[i])
			{
				PlayerController playerController = GameManager.Instance.PrimaryPlayer;
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER && i != 0)
				{
					playerController = GameManager.Instance.SecondaryPlayer;
				}
				dfControl dfControl2 = m_extantReloadLabels[i];
				float num = 0.125f;
				if (m_extantReloadLabels[i].GetLocalizationKey() == "#RELOAD_FULL")
				{
					num = 0.1875f;
				}
				float num2 = 0f;
				if ((bool)playerController && (bool)playerController.CurrentGun && playerController.CurrentGun.Handedness == GunHandedness.NoHanded)
				{
					num2 += 0.5f;
				}
				Vector3 vector = new Vector3(playerController.specRigidbody.UnitCenter.x - playerController.transform.position.x + num, playerController.SpriteDimensions.y + num2, 0f);
				Vector2 vector2 = dfFollowObject.ConvertWorldSpaces(playerController.transform.position + vector, GameManager.Instance.MainCameraController.Camera, Manager.RenderCamera).WithZ(0f);
				dfControl2.transform.position = vector2;
				dfControl2.transform.position = dfControl2.transform.position.QuantizeFloor(dfControl2.PixelsToUnits() / (Pixelator.Instance.ScaleTileScale / Pixelator.Instance.CurrentTileScale));
			}
		}
	}

	private IEnumerator FlashReloadLabel(dfControl target, PlayerController attachPlayer, Vector3 offset, float customDuration = -1f, string customStringKey = "")
	{
		int targetIndex = ((!attachPlayer.IsPrimaryPlayer) ? 1 : 0);
		m_displayingReloadNeeded[targetIndex] = true;
		target.transform.localScale = Vector3.one / GameUIScalar;
		dfLabel targetLabel = target as dfLabel;
		string customString = string.Empty;
		if (!string.IsNullOrEmpty(customStringKey))
		{
			customString = target.getLocalizedValue(customStringKey);
		}
		string reloadString = target.getLocalizedValue("#RELOAD");
		string emptyString = target.getLocalizedValue("#RELOAD_EMPTY");
		if (customDuration > 0f)
		{
			m_isDisplayingCustomReload = true;
			float outerElapsed = 0f;
			while (outerElapsed < customDuration && !GameManager.Instance.IsPaused)
			{
				target.IsVisible = true;
				targetLabel.Text = customString;
				targetLabel.Color = Color.white;
				outerElapsed += BraveTime.DeltaTime;
				yield return null;
			}
			m_isDisplayingCustomReload = false;
		}
		else
		{
			while (m_displayingReloadNeeded[targetIndex] && !GameManager.Instance.IsPaused)
			{
				target.IsVisible = true;
				if (!string.IsNullOrEmpty(customString))
				{
					targetLabel.Text = customString;
					targetLabel.Color = Color.white;
				}
				else if (attachPlayer.CurrentGun.CurrentAmmo != 0)
				{
					if (attachPlayer.CurrentGun.name.Contains("Beholster_Gun") && GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
					{
						targetLabel.Text = target.getLocalizedValue("#RELOAD_BEHOLD");
					}
					else
					{
						targetLabel.Text = reloadString;
					}
					targetLabel.Color = Color.white;
				}
				else
				{
					targetLabel.Text = emptyString;
					targetLabel.Color = Color.red;
				}
				bool shouldShowEver = customDuration > 0f || attachPlayer.CurrentGun.CurrentAmmo != 0 || attachPlayer.IsInCombat;
				float elapsed2 = 0f;
				while (elapsed2 < 0.6f)
				{
					elapsed2 += m_deltaTime;
					if (!m_displayingReloadNeeded[targetIndex])
					{
						target.IsVisible = false;
						yield break;
					}
					if (!shouldShowEver)
					{
						target.IsVisible = false;
					}
					if (GameManager.Instance.IsPaused)
					{
						target.IsVisible = false;
					}
					yield return null;
				}
				target.IsVisible = false;
				elapsed2 = 0f;
				while (elapsed2 < 0.6f)
				{
					elapsed2 += m_deltaTime;
					if (!m_displayingReloadNeeded[targetIndex])
					{
						target.IsVisible = false;
						yield break;
					}
					yield return null;
				}
			}
		}
		m_displayingReloadNeeded[targetIndex] = false;
		target.IsVisible = false;
	}

	public void StartPlayerReloadBar(PlayerController attachObject, Vector3 offset, float duration)
	{
		int num = ((!attachObject.IsPrimaryPlayer) ? 1 : 0);
		if (num >= 0 && num < m_displayingReloadNeeded.Count)
		{
			m_displayingReloadNeeded[num] = false;
		}
		m_extantReloadBars[num].TriggerReload(attachObject, offset, duration, 0.65f, 1);
	}

	public void TriggerBossKillCam(Projectile killerProjectile, SpeculativeRigidbody bossSRB)
	{
		if (!m_bossKillCamActive)
		{
			if (GameManager.Instance.InTutorial)
			{
				StaticReferenceManager.DestroyAllEnemyProjectiles();
				return;
			}
			StaticReferenceManager.DestroyAllEnemyProjectiles();
			m_bossKillCamActive = true;
			BossKillCam bossKillCam = base.gameObject.AddComponent<BossKillCam>();
			bossKillCam.TriggerSequence(killerProjectile, bossSRB);
		}
	}

	public void EndBossKillCam()
	{
		m_bossKillCamActive = false;
	}

	public void ShowPauseMenu()
	{
		AkSoundEngine.PostEvent("Play_UI_menu_pause_01", base.gameObject);
		Instance.ToggleLowerPanels(false, false, "gm_pause");
		Instance.HideCoreUI("gm_pause");
		levelNameUI.BanishLevelNameText();
		notificationController.ForceHide();
		Instance.ForceClearReload();
		PauseMenuController component = PauseMenuPanel.GetComponent<PauseMenuController>();
		PauseMenuPanel.IsVisible = true;
		PauseMenuPanel.IsInteractive = true;
		PauseMenuPanel.IsEnabled = true;
		component.SetDefaultFocus();
		component.ShwoopOpen();
		component.SetDefaultFocus();
		dfGUIManager.PushModal(PauseMenuPanel);
	}

	public bool HasOpenPauseSubmenu()
	{
		if (PauseMenuPanel == null)
		{
			return false;
		}
		if (m_pmc == null)
		{
			m_pmc = PauseMenuPanel.GetComponent<PauseMenuController>();
		}
		if (m_pmc == null)
		{
			return false;
		}
		return (m_pmc.OptionsMenu != null && (m_pmc.OptionsMenu.IsVisible || m_pmc.OptionsMenu.PreOptionsMenu.IsVisible || m_pmc.OptionsMenu.ModalKeyBindingDialog.IsVisible)) || m_pmc.AdditionalMenuElementsToClear.Count > 0;
	}

	public void ReturnToBasePause()
	{
		PauseMenuController component = PauseMenuPanel.GetComponent<PauseMenuController>();
		component.RevertToBaseState();
	}

	public void HidePauseMenu()
	{
		PauseMenuController component = PauseMenuPanel.GetComponent<PauseMenuController>();
		if (PauseMenuPanel.IsVisible)
		{
			component.ShwoopClosed();
		}
		PauseMenuPanel.IsInteractive = false;
		if (component.OptionsMenu != null)
		{
			component.OptionsMenu.IsVisible = false;
			component.OptionsMenu.PreOptionsMenu.IsVisible = false;
		}
		if (PauseMenuPanel.IsVisible)
		{
			dfGUIManager.PopModalToControl(PauseMenuPanel, true);
		}
		if (AmmonomiconController.Instance != null && AmmonomiconController.Instance.IsOpen)
		{
			AmmonomiconController.Instance.CloseAmmonomicon();
		}
		AkSoundEngine.PostEvent("Play_UI_menu_cancel_01", base.gameObject);
		AkSoundEngine.PostEvent("Play_UI_menu_unpause_01", base.gameObject);
	}

	public void InitializeConversationPortrait(PlayerController player)
	{
		PlayableCharacters characterIdentity = player.characterIdentity;
		dfSprite component = ConversationBar.transform.Find("FacecardFrame/Facecard").GetComponent<dfSprite>();
		switch (characterIdentity)
		{
		case PlayableCharacters.Pilot:
			component.SpriteName = "talking_bar_character_window_rogue_001";
			break;
		case PlayableCharacters.Convict:
			component.SpriteName = "talking_bar_character_window_convict_001";
			break;
		case PlayableCharacters.Guide:
			component.SpriteName = "talking_bar_character_window_guide_001";
			break;
		case PlayableCharacters.Soldier:
			component.SpriteName = "talking_bar_character_window_marine_001";
			break;
		case PlayableCharacters.Bullet:
			component.SpriteName = "talking_bar_character_window_bullet_001";
			break;
		case PlayableCharacters.Gunslinger:
			component.SpriteName = "talking_bar_character_window_slinger_003";
			break;
		case PlayableCharacters.Robot:
		case PlayableCharacters.Ninja:
		case PlayableCharacters.Cosmonaut:
		case PlayableCharacters.CoopCultist:
		case PlayableCharacters.Eevee:
			break;
		}
	}

	public bool DisplayPlayerConversationOptions(PlayerController interactingPlayer, TalkModule sourceModule, string overrideResponse1 = "", string overrideResponse2 = "")
	{
		int num = ((sourceModule != null) ? sourceModule.responses.Count : 0);
		if (!string.IsNullOrEmpty(overrideResponse1))
		{
			num = Mathf.Max(1, num);
		}
		if (!string.IsNullOrEmpty(overrideResponse2))
		{
			num = Mathf.Max(2, num);
		}
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			if (sourceModule != null && sourceModule.responses.Count > i)
			{
				array[i] = StringTableManager.GetString(sourceModule.responses[i].response);
			}
		}
		if (!string.IsNullOrEmpty(overrideResponse1))
		{
			array[0] = overrideResponse1;
		}
		if (!string.IsNullOrEmpty(overrideResponse2))
		{
			array[1] = overrideResponse2;
		}
		return DisplayPlayerConversationOptions(interactingPlayer, array);
	}

	public bool DisplayPlayerConversationOptions(PlayerController interactingPlayer, string[] responses)
	{
		if (m_displayingPlayerConversationOptions)
		{
			return false;
		}
		m_displayingPlayerConversationOptions = true;
		hasSelectedOption = false;
		selectedResponse = 0;
		for (int i = 0; i < itemControllers.Count; i++)
		{
			itemControllers[i].DimItemSprite();
		}
		for (int j = 0; j < ammoControllers.Count; j++)
		{
			ammoControllers[j].DimGunSprite();
		}
		ToggleLowerPanels(false, false, "conversationBar");
		StartCoroutine(HandlePlayerConversationResponse(interactingPlayer, responses));
		return true;
	}

	public void SetConversationResponse(int selected)
	{
		if (selectedResponse != selected)
		{
			selectedResponse = selected;
			AkSoundEngine.PostEvent("Play_UI_menu_select_01", base.gameObject);
		}
	}

	public void SelectConversationResponse()
	{
		hasSelectedOption = true;
		AkSoundEngine.PostEvent("Play_UI_menu_confirm_01", base.gameObject);
	}

	private IEnumerator HandlePlayerConversationResponse(PlayerController interactingPlayer, string[] responses)
	{
		ConversationBar.ShowBar(interactingPlayer, responses);
		float timer = 0f;
		int numResponses = ((responses == null) ? 2 : responses.Length);
		while (!hasSelectedOption)
		{
			if (GameManager.Instance.IsPaused)
			{
				timer += BraveTime.DeltaTime;
				yield return null;
				continue;
			}
			if (BraveInput.GetInstanceForPlayer(interactingPlayer.PlayerIDX).ActiveActions.SelectUp.WasPressedAsDpadRepeating)
			{
				selectedResponse = Mathf.Clamp(selectedResponse - 1, 0, numResponses - 1);
				AkSoundEngine.PostEvent("Play_UI_menu_select_01", base.gameObject);
			}
			else if (BraveInput.GetInstanceForPlayer(interactingPlayer.PlayerIDX).ActiveActions.SelectDown.WasPressedAsDpadRepeating)
			{
				selectedResponse = Mathf.Clamp(selectedResponse + 1, 0, numResponses - 1);
				AkSoundEngine.PostEvent("Play_UI_menu_select_01", base.gameObject);
			}
			else if (BraveInput.GetInstanceForPlayer(interactingPlayer.PlayerIDX).MenuInteractPressed && timer > 0.4f)
			{
				hasSelectedOption = true;
				AkSoundEngine.PostEvent("Play_UI_menu_confirm_01", base.gameObject);
				ToggleLowerPanels(true, false, "conversationBar");
			}
			ConversationBar.SetSelectedResponse(selectedResponse);
			yield return null;
			timer += BraveTime.DeltaTime;
		}
		ConversationBar.HideBar();
		for (int i = 0; i < itemControllers.Count; i++)
		{
			itemControllers[i].UndimItemSprite();
		}
		for (int j = 0; j < ammoControllers.Count; j++)
		{
			ammoControllers[j].UndimGunSprite();
		}
		m_displayingPlayerConversationOptions = false;
	}

	public bool GetPlayerConversationResponse(out int responseIndex)
	{
		responseIndex = selectedResponse;
		return hasSelectedOption;
	}

	public static void ToggleBG(dfControl rawTarget)
	{
		if (rawTarget is dfButton)
		{
			dfButton dfButton2 = rawTarget as dfButton;
			if (StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
			{
				dfButton2.BackgroundSprite = string.Empty;
				dfButton2.Padding = new RectOffset(0, 0, 0, 0);
				return;
			}
			dfButton2.BackgroundSprite = "chamber_flash_small_001";
			dfButton2.Padding = new RectOffset(6, 6, 0, 0);
			dfButton2.NormalBackgroundColor = Color.black;
			dfButton2.FocusBackgroundColor = Color.black;
			dfButton2.HoverBackgroundColor = Color.black;
			dfButton2.DisabledColor = Color.black;
			dfButton2.PressedBackgroundColor = Color.black;
		}
		else if (rawTarget is dfLabel)
		{
			dfLabel dfLabel2 = rawTarget as dfLabel;
			if (StringTableManager.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
			{
				dfLabel2.BackgroundSprite = string.Empty;
				dfLabel2.Padding = new RectOffset(0, 0, 0, 0);
			}
			else
			{
				dfLabel2.BackgroundSprite = "chamber_flash_small_001";
				dfLabel2.Padding = new RectOffset(6, 6, 0, 0);
				dfLabel2.BackgroundColor = Color.black;
			}
		}
	}

	public void CheckKeepModifiersQuickRestart(int requiredCredits)
	{
		m_hasSelectedAreYouSureOption = false;
		KeepMetasIsVisible = true;
		dfPanel QuestionPanel = (dfPanel)m_manager.AddPrefab((GameObject)BraveResources.Load("QuickRestartDetailsPanel"));
		QuestionPanel.BringToFront();
		dfGUIManager.PushModal(QuestionPanel);
		dfControl component = QuestionPanel.transform.Find("AreYouSurePanelBGSlicedSprite").GetComponent<dfControl>();
		QuestionPanel.PerformLayout();
		component.PerformLayout();
		dfButton component2 = QuestionPanel.transform.Find("YesButton").GetComponent<dfButton>();
		dfButton component3 = QuestionPanel.transform.Find("NoButton").GetComponent<dfButton>();
		component2.ModifyLocalizedText(component2.Text + " (" + requiredCredits + "[sprite \"hbux_text_icon\"])");
		float metas = GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.META_CURRENCY);
		if (metas >= (float)requiredCredits)
		{
			component2.Focus();
		}
		else
		{
			component2.Disable();
			component3.GetComponent<UIKeyControls>().up = null;
			component3.Focus();
		}
		dfLabel component4 = QuestionPanel.transform.Find("TopLabel").GetComponent<dfLabel>();
		component4.IsLocalized = true;
		component4.Text = component4.getLocalizedValue("#QUICKRESTARTDETAIL");
		Action<bool> HandleChoice = delegate(bool choice)
		{
			if (!m_hasSelectedAreYouSureOption)
			{
				if (choice)
				{
					GameStatsManager.Instance.ClearStatValueGlobal(TrackedStats.META_CURRENCY);
					GameStatsManager.Instance.SetStat(TrackedStats.META_CURRENCY, metas - (float)requiredCredits);
				}
				m_hasSelectedAreYouSureOption = true;
				m_AreYouSureSelection = choice;
				dfGUIManager.PopModal();
				QuestionPanel.IsVisible = false;
				KeepMetasIsVisible = false;
			}
		};
		component2.Click += delegate(dfControl control, dfMouseEventArgs mouseEvent)
		{
			mouseEvent.Use();
			HandleChoice(true);
		};
		component3.Click += delegate(dfControl control, dfMouseEventArgs mouseEvent)
		{
			mouseEvent.Use();
			HandleChoice(false);
		};
		StartCoroutine(DelayedCenterControl(component));
	}

	private IEnumerator DelayedCenterControl(dfControl panel)
	{
		yield return null;
		panel.Anchor = dfAnchorStyle.CenterHorizontal | dfAnchorStyle.CenterVertical;
		panel.PerformLayout();
	}

	public void DoAreYouSure(string questionKey, bool focusYes = false, string secondaryKey = null)
	{
		m_hasSelectedAreYouSureOption = false;
		AreYouSurePanel.IsVisible = true;
		dfGUIManager.PushModal(AreYouSurePanel);
		if (focusYes)
		{
			m_AreYouSureYesButton.Focus();
		}
		else
		{
			m_AreYouSureNoButton.Focus();
		}
		ToggleBG(m_AreYouSureYesButton);
		ToggleBG(m_AreYouSureNoButton);
		ToggleBG(m_AreYouSurePrimaryLabel);
		ToggleBG(m_AreYouSureSecondaryLabel);
		m_AreYouSurePrimaryLabel.IsLocalized = true;
		m_AreYouSurePrimaryLabel.Text = m_AreYouSurePrimaryLabel.getLocalizedValue(questionKey);
		if (!string.IsNullOrEmpty(secondaryKey))
		{
			m_AreYouSureSecondaryLabel.IsLocalized = true;
			m_AreYouSureSecondaryLabel.Text = m_AreYouSureSecondaryLabel.getLocalizedValue(secondaryKey);
			if (m_AreYouSureSecondaryLabel.Text.Contains("%CURRENTSLOT"))
			{
				string key;
				switch (SaveManager.CurrentSaveSlot)
				{
				case SaveManager.SaveSlot.A:
					key = "#OPTIONS_SAVESLOTA";
					break;
				case SaveManager.SaveSlot.B:
					key = "#OPTIONS_SAVESLOTB";
					break;
				case SaveManager.SaveSlot.C:
					key = "#OPTIONS_SAVESLOTC";
					break;
				case SaveManager.SaveSlot.D:
					key = "#OPTIONS_SAVESLOTD";
					break;
				default:
					key = "#OPTIONS_SAVESLOTA";
					break;
				}
				string text = m_AreYouSureSecondaryLabel.Text;
				text = text.Replace("%CURRENTSLOT", m_AreYouSureSecondaryLabel.getLocalizedValue(key));
				m_AreYouSureSecondaryLabel.ModifyLocalizedText(StringTableManager.PostprocessString(text));
			}
			else
			{
				m_AreYouSureSecondaryLabel.ModifyLocalizedText(StringTableManager.PostprocessString(m_AreYouSureSecondaryLabel.Text));
			}
			m_AreYouSureSecondaryLabel.IsVisible = true;
		}
		else
		{
			m_AreYouSureSecondaryLabel.IsVisible = false;
		}
		m_AreYouSureYesButton.Click += SelectedAreYouSureYes;
		m_AreYouSureNoButton.Click += SelectedAreYouSureNo;
	}

	private void SelectedAreYouSureNo(dfControl control, dfMouseEventArgs mouseEvent)
	{
		mouseEvent.Use();
		SelectAreYouSureOption(false);
	}

	private void SelectedAreYouSureYes(dfControl control, dfMouseEventArgs mouseEvent)
	{
		mouseEvent.Use();
		SelectAreYouSureOption(true);
	}

	public void SelectAreYouSureOption(bool isSure)
	{
		m_AreYouSureNoButton.Click -= SelectedAreYouSureNo;
		m_AreYouSureYesButton.Click -= SelectedAreYouSureYes;
		m_hasSelectedAreYouSureOption = true;
		m_AreYouSureSelection = isSure;
		dfGUIManager.PopModal();
		AreYouSurePanel.IsVisible = false;
	}

	public bool HasSelectedAreYouSureOption()
	{
		return m_hasSelectedAreYouSureOption;
	}

	public bool GetAreYouSureOption()
	{
		return m_AreYouSureSelection;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Instance = null;
	}
}
