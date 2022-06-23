using System;
using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using InControl;
using UnityEngine;

public class MinimapUIController : MonoBehaviour
{
	public dfSprite DockSprite;

	private dfGUIManager m_manager;

	public dfPanel QuestPanel;

	public dfPanel GrabbyPanel;

	public dfPanel ItemPanel_PC;

	public dfPanel ItemPanel_PC_Foreign;

	public dfPanel SonyControlsPanel01;

	public dfPanel SonyControlsPanel02;

	public dfPanel SonyControlsPanel01Foreign;

	public dfPanel SonyControlsPanel02Foreign;

	public dfPanel DockPanel;

	public dfPanel CoopDockPanelLeft;

	public dfPanel CoopDockPanelRight;

	public dfSprite ControllerCrosshair;

	public dfLabel LevelNameLabel;

	public dfButton DropItemButton;

	public dfSprite DropItemSprite;

	public dfLabel DropItemLabel;

	public dfButton DropItemButtonForeign;

	public dfSprite DropItemSpriteForeign;

	public dfLabel DropItemLabelForeign;

	public List<dfControl> AdditionalControlsToToggle;

	public Camera minimapCamera;

	public dfSprite TurboModeIndicator;

	public dfSprite DispenserIcon;

	public dfLabel DispenserLabel;

	public dfSprite RatTaunty;

	private int m_targetDockIndex;

	private int m_selectedDockItemIndex = -1;

	private List<Tuple<tk2dSprite, PassiveItem>> dockItems = new List<Tuple<tk2dSprite, PassiveItem>>();

	private List<Tuple<tk2dSprite, PassiveItem>> secondaryDockItems = new List<Tuple<tk2dSprite, PassiveItem>>();

	private List<dfControl> panels;

	private Dictionary<dfControl, Vector3> activePositions;

	private Dictionary<dfControl, Vector3> inactivePositions;

	private Dictionary<dfControl, Tuple<float, float>> panelTimings;

	private bool m_active;

	private bool m_isPanning;

	private Vector3 m_lastMousePosition;

	private float m_panPixelDistTravelled;

	private tk2dBaseSprite m_currentTeleportIconSprite;

	private RoomHandler m_currentTeleportTarget;

	private int m_currentTeleportTargetIndex;

	private const float ITEM_SPACING_MPX = 20f;

	private const int NUM_ITEMS_PER_LINE = 12;

	private const int NUM_ITEMS_PER_LINE_COOP = 5;

	private Vector3? m_cachedCoopDockPanelLeftRelativePosition;

	private Vector3? m_cachedCoopDockPanelRightRelativePosition;

	private List<dfSprite> extantSynergyArrows = new List<dfSprite>();

	private List<Tuple<tk2dSprite, PassiveItem>> SelectedDockItems
	{
		get
		{
			return (m_targetDockIndex != 1) ? dockItems : secondaryDockItems;
		}
	}

	private Vector3 GetActivePosition(dfPanel panel, DungeonData.Direction direction)
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

	private Vector3 GetInactivePosition(dfPanel panel, DungeonData.Direction direction)
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

	private void InitializeMasterPanel(dfPanel panel, float startTime, float endTime)
	{
		panel.ResolutionChangedPostLayout = (Action<dfControl, Vector3, Vector3>)Delegate.Combine(panel.ResolutionChangedPostLayout, new Action<dfControl, Vector3, Vector3>(OnControlResolutionChanged));
		panels.Add(panel);
		panelTimings.Add(panel, new Tuple<float, float>(startTime, endTime));
	}

	private void Start()
	{
		m_manager = GrabbyPanel.GetManager();
		m_manager.UIScaleLegacyMode = false;
		DropItemButton.Click += delegate
		{
			DropSelectedItem();
		};
		DropItemButtonForeign.Click += delegate
		{
			DropSelectedItem();
		};
		panels = new List<dfControl>();
		panelTimings = new Dictionary<dfControl, Tuple<float, float>>();
		InitializeMasterPanel(GrabbyPanel, 0f, 0.6f);
		InitializeMasterPanel(ItemPanel_PC, 0.2f, 0.8f);
		InitializeMasterPanel(ItemPanel_PC_Foreign, 0.2f, 0.8f);
		InitializeMasterPanel(SonyControlsPanel01, 0f, 0.6f);
		InitializeMasterPanel(SonyControlsPanel02, 0.2f, 0.8f);
		InitializeMasterPanel(SonyControlsPanel01Foreign, 0f, 0.6f);
		InitializeMasterPanel(SonyControlsPanel02Foreign, 0.2f, 0.8f);
		InitializeMasterPanel(DockPanel, 0f, 0.8f);
		InitializeMasterPanel(CoopDockPanelLeft, 0f, 0.8f);
		InitializeMasterPanel(CoopDockPanelRight, 0f, 0.8f);
		RecalculatePositions();
	}

	private void PostprocessPassiveDockSprite(PassiveItem item, tk2dSprite itemSprite)
	{
		if (item is YellowChamberItem)
		{
			tk2dSpriteAnimator orAddComponent = itemSprite.gameObject.GetOrAddComponent<tk2dSpriteAnimator>();
			orAddComponent.Library = item.GetComponent<tk2dSpriteAnimator>().Library;
		}
	}

	public void AddPassiveItemToDock(PassiveItem item, PlayerController itemOwner)
	{
		if ((bool)item && (bool)item.encounterTrackable && item.encounterTrackable.SuppressInInventory)
		{
			return;
		}
		for (int i = 0; i < dockItems.Count; i++)
		{
			if (dockItems[i].Second == item)
			{
				return;
			}
		}
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			for (int j = 0; j < secondaryDockItems.Count; j++)
			{
				if (secondaryDockItems[j].Second == item)
				{
					return;
				}
			}
			if (itemOwner.IsPrimaryPlayer)
			{
				tk2dSprite tk2dSprite2 = AddTK2DSpriteToPanel(item.GetComponent<tk2dBaseSprite>(), CoopDockPanelLeft.transform);
				PostprocessPassiveDockSprite(item, tk2dSprite2);
				Tuple<tk2dSprite, PassiveItem> item2 = new Tuple<tk2dSprite, PassiveItem>(tk2dSprite2, item);
				dockItems.Add(item2);
			}
			else
			{
				tk2dSprite tk2dSprite3 = AddTK2DSpriteToPanel(item.GetComponent<tk2dBaseSprite>(), CoopDockPanelRight.transform);
				PostprocessPassiveDockSprite(item, tk2dSprite3);
				Tuple<tk2dSprite, PassiveItem> item3 = new Tuple<tk2dSprite, PassiveItem>(tk2dSprite3, item);
				secondaryDockItems.Add(item3);
			}
		}
		else
		{
			tk2dSprite tk2dSprite4 = AddTK2DSpriteToPanel(item.GetComponent<tk2dBaseSprite>(), DockPanel.transform);
			PostprocessPassiveDockSprite(item, tk2dSprite4);
			Tuple<tk2dSprite, PassiveItem> item4 = new Tuple<tk2dSprite, PassiveItem>(tk2dSprite4, item);
			dockItems.Add(item4);
		}
	}

	public void InfoSelectedItem()
	{
		if (m_selectedDockItemIndex != -1 && m_selectedDockItemIndex < SelectedDockItems.Count && (bool)SelectedDockItems[m_selectedDockItemIndex].Second.encounterTrackable)
		{
			EncounterTrackable encounterTrackable = SelectedDockItems[m_selectedDockItemIndex].Second.encounterTrackable;
			if (!encounterTrackable.journalData.SuppressInAmmonomicon)
			{
				Minimap.Instance.ToggleMinimap(false);
				GameManager.Instance.Pause();
				GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().DoShowBestiaryToTarget(encounterTrackable);
			}
		}
	}

	public void DropSelectedItem()
	{
		ClearSynergyHighlights();
		if (m_selectedDockItemIndex == -1)
		{
			return;
		}
		PlayerController playerController = GameManager.Instance.PrimaryPlayer;
		if (m_targetDockIndex == 1)
		{
			playerController = GameManager.Instance.SecondaryPlayer;
		}
		if (m_selectedDockItemIndex >= SelectedDockItems.Count)
		{
			int index = m_selectedDockItemIndex - SelectedDockItems.Count;
			if (playerController.inventory.AllGuns[index].CanActuallyBeDropped(playerController))
			{
				playerController.ForceDropGun(playerController.inventory.AllGuns[index]);
				m_selectedDockItemIndex = -1;
			}
		}
		else
		{
			PassiveItem second = SelectedDockItems[m_selectedDockItemIndex].Second;
			if (second.CanActuallyBeDropped(second.Owner))
			{
				playerController.DropPassiveItem(second);
				m_selectedDockItemIndex = -1;
			}
		}
	}

	public void RemovePassiveItemFromDock(PassiveItem item)
	{
		for (int i = 0; i < dockItems.Count; i++)
		{
			Tuple<tk2dSprite, PassiveItem> tuple = dockItems[i];
			if (tuple.Second == item)
			{
				UnityEngine.Object.Destroy(tuple.First.gameObject);
				dockItems.RemoveAt(i);
				break;
			}
		}
		for (int j = 0; j < secondaryDockItems.Count; j++)
		{
			Tuple<tk2dSprite, PassiveItem> tuple2 = secondaryDockItems[j];
			if (tuple2.Second == item)
			{
				UnityEngine.Object.Destroy(tuple2.First.gameObject);
				secondaryDockItems.RemoveAt(j);
				break;
			}
		}
	}

	protected tk2dSprite AddTK2DSpriteToPanel(tk2dBaseSprite sourceSprite, Transform parent)
	{
		GameObject gameObject = new GameObject("tk2d item sprite");
		gameObject.transform.parent = parent;
		gameObject.layer = LayerMask.NameToLayer("SecondaryGUI");
		tk2dSprite tk2dSprite2 = tk2dBaseSprite.AddComponent<tk2dSprite>(gameObject, sourceSprite.Collection, sourceSprite.spriteId);
		Bounds untrimmedBounds = tk2dSprite2.GetUntrimmedBounds();
		Vector2 vector = GameUIUtility.TK2DtoDF(untrimmedBounds.size.XY());
		tk2dSprite2.scale = new Vector3(vector.x / untrimmedBounds.size.x, vector.y / untrimmedBounds.size.y, untrimmedBounds.size.z);
		tk2dSprite2.ignoresTiltworldDepth = true;
		gameObject.transform.localPosition = Vector3.zero;
		return tk2dSprite2;
	}

	protected void OnControlResolutionChanged(dfControl source, Vector3 oldRelativePosition, Vector3 newRelativePosition)
	{
	}

	protected void RecalculatePositions()
	{
		if (activePositions == null)
		{
			activePositions = new Dictionary<dfControl, Vector3>();
		}
		if (inactivePositions == null)
		{
			inactivePositions = new Dictionary<dfControl, Vector3>();
		}
		activePositions.Clear();
		inactivePositions.Clear();
		activePositions.Add(GrabbyPanel, GetActivePosition(GrabbyPanel, DungeonData.Direction.WEST));
		inactivePositions.Add(GrabbyPanel, GetInactivePosition(GrabbyPanel, DungeonData.Direction.WEST));
		activePositions.Add(ItemPanel_PC, GetActivePosition(ItemPanel_PC, DungeonData.Direction.WEST));
		inactivePositions.Add(ItemPanel_PC, GetInactivePosition(ItemPanel_PC, DungeonData.Direction.WEST));
		activePositions.Add(ItemPanel_PC_Foreign, GetActivePosition(ItemPanel_PC_Foreign, DungeonData.Direction.WEST));
		inactivePositions.Add(ItemPanel_PC_Foreign, GetInactivePosition(ItemPanel_PC_Foreign, DungeonData.Direction.WEST));
		activePositions.Add(SonyControlsPanel01, GetActivePosition(SonyControlsPanel01, DungeonData.Direction.WEST));
		inactivePositions.Add(SonyControlsPanel01, GetInactivePosition(SonyControlsPanel01, DungeonData.Direction.WEST));
		activePositions.Add(SonyControlsPanel02, GetActivePosition(SonyControlsPanel02, DungeonData.Direction.WEST));
		inactivePositions.Add(SonyControlsPanel02, GetInactivePosition(SonyControlsPanel02, DungeonData.Direction.WEST));
		activePositions.Add(SonyControlsPanel01Foreign, GetActivePosition(SonyControlsPanel01Foreign, DungeonData.Direction.WEST));
		inactivePositions.Add(SonyControlsPanel01Foreign, GetInactivePosition(SonyControlsPanel01Foreign, DungeonData.Direction.WEST));
		activePositions.Add(SonyControlsPanel02Foreign, GetActivePosition(SonyControlsPanel02Foreign, DungeonData.Direction.WEST));
		inactivePositions.Add(SonyControlsPanel02Foreign, GetInactivePosition(SonyControlsPanel02Foreign, DungeonData.Direction.WEST));
		activePositions.Add(DockPanel, GetActivePosition(DockPanel, DungeonData.Direction.SOUTH));
		inactivePositions.Add(DockPanel, GetInactivePosition(DockPanel, DungeonData.Direction.SOUTH));
		activePositions.Add(CoopDockPanelLeft, GetActivePosition(CoopDockPanelLeft, DungeonData.Direction.SOUTH));
		inactivePositions.Add(CoopDockPanelLeft, GetInactivePosition(CoopDockPanelLeft, DungeonData.Direction.SOUTH));
		activePositions.Add(CoopDockPanelRight, GetActivePosition(CoopDockPanelRight, DungeonData.Direction.SOUTH));
		inactivePositions.Add(CoopDockPanelRight, GetInactivePosition(CoopDockPanelRight, DungeonData.Direction.SOUTH));
	}

	private void PostStateChanged(bool newState)
	{
		TurboModeIndicator.IsVisible = GameManager.IsTurboMode;
		DispenserLabel.Text = HeartDispenser.CurrentHalfHeartsStored.ToString();
		DispenserLabel.IsVisible = HeartDispenser.CurrentHalfHeartsStored > 0;
		DispenserIcon.IsVisible = HeartDispenser.CurrentHalfHeartsStored > 0;
		for (int i = 0; i < dockItems.Count; i++)
		{
			if (!(dockItems[i].Second is YellowChamberItem))
			{
				continue;
			}
			if (newState)
			{
				if (UnityEngine.Random.value < 0.1f)
				{
					if (UnityEngine.Random.value < 0.25f)
					{
						StartCoroutine(HandleDelayedAnimation(dockItems[i].First.spriteAnimator, "yellow_chamber_eye", UnityEngine.Random.Range(2.5f, 10f)));
					}
					else
					{
						StartCoroutine(HandleDelayedAnimation(dockItems[i].First.spriteAnimator, "yellow_chamber_blink", UnityEngine.Random.Range(2.5f, 10f)));
					}
				}
				else
				{
					dockItems[i].First.spriteAnimator.StopAndResetFrameToDefault();
				}
			}
			else
			{
				dockItems[i].First.spriteAnimator.Stop();
			}
		}
		for (int j = 0; j < secondaryDockItems.Count; j++)
		{
			if (!(secondaryDockItems[j].Second is YellowChamberItem))
			{
				continue;
			}
			if (newState)
			{
				if (UnityEngine.Random.value < 0.1f)
				{
					if (UnityEngine.Random.value < 0.25f)
					{
						StartCoroutine(HandleDelayedAnimation(secondaryDockItems[j].First.spriteAnimator, "yellow_chamber_eye", UnityEngine.Random.Range(2.5f, 10f)));
					}
					else
					{
						StartCoroutine(HandleDelayedAnimation(secondaryDockItems[j].First.spriteAnimator, "yellow_chamber_blink", UnityEngine.Random.Range(2.5f, 10f)));
					}
				}
			}
			else
			{
				secondaryDockItems[j].First.spriteAnimator.Stop();
			}
		}
	}

	private IEnumerator HandleDelayedAnimation(tk2dSpriteAnimator targetAnimator, string animationName, float delayTime)
	{
		float elapsed = 0f;
		while (elapsed < delayTime)
		{
			if (!m_active)
			{
				yield break;
			}
			elapsed += BraveTime.DeltaTime;
			yield return null;
		}
		if (m_active)
		{
			targetAnimator.Play(animationName);
		}
	}

	public void ToggleState(bool active)
	{
		if (active == m_active)
		{
			return;
		}
		if (active)
		{
			Activate();
			for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
			{
				PlayerController playerController = GameManager.Instance.AllPlayers[i];
				if ((bool)playerController)
				{
					playerController.CurrentInputState = PlayerInputState.OnlyMovement;
				}
			}
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				dfSprite componentInChildren = CoopDockPanelLeft.GetComponentInChildren<dfSprite>();
				dfSprite componentInChildren2 = CoopDockPanelRight.GetComponentInChildren<dfSprite>();
				if (!m_cachedCoopDockPanelLeftRelativePosition.HasValue)
				{
					m_cachedCoopDockPanelLeftRelativePosition = componentInChildren.RelativePosition;
				}
				if (!m_cachedCoopDockPanelRightRelativePosition.HasValue)
				{
					m_cachedCoopDockPanelRightRelativePosition = componentInChildren2.RelativePosition;
				}
				ArrangeDockItems(dockItems, componentInChildren, 1);
				ArrangeDockItems(secondaryDockItems, componentInChildren2, 2);
			}
			else
			{
				ArrangeDockItems(dockItems, DockSprite);
			}
		}
		else
		{
			Deactivate();
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				if ((bool)GameManager.Instance.AllPlayers[j])
				{
					GameManager.Instance.AllPlayers[j].CurrentInputState = PlayerInputState.AllInput;
				}
			}
		}
		PostStateChanged(active);
	}

	protected void ArrangeDockItems(List<Tuple<tk2dSprite, PassiveItem>> targetDockItems, dfSprite targetDockSprite, int targetIndex = 0)
	{
		float num = DockPanel.PixelsToUnits() * Pixelator.Instance.CurrentTileScale;
		int count = targetDockItems.Count;
		float num2 = ((GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER) ? 270 : 132);
		float num3 = num2 / (float)count;
		float num4 = Pixelator.Instance.CurrentTileScale;
		for (int i = 0; i < count; i++)
		{
			num4 += targetDockItems[i].First.GetBounds().size.x / num;
			num4 += Pixelator.Instance.CurrentTileScale;
		}
		float height = 20f * Pixelator.Instance.CurrentTileScale / 3f;
		targetDockSprite.Width = Mathf.Min(num4, num2).Quantize(3f);
		targetDockSprite.Height = height;
		if (targetIndex == 1 && m_cachedCoopDockPanelLeftRelativePosition.HasValue)
		{
			targetDockSprite.RelativePosition = targetDockSprite.RelativePosition.WithX(targetDockSprite.Parent.Width - 132f - (132f - targetDockSprite.Width) * 2f);
		}
		else if (targetIndex == 2 && m_cachedCoopDockPanelRightRelativePosition.HasValue)
		{
			targetDockSprite.RelativePosition = targetDockSprite.RelativePosition.WithX((132f - targetDockSprite.Width) * 3f);
		}
		targetDockSprite.PerformLayout();
		Vector3 position = targetDockSprite.GetCorners()[2];
		float num5 = 0f;
		if (targetIndex != 2)
		{
			num5 = 20f * Pixelator.Instance.CurrentTileScale / 6f * num;
		}
		for (int j = 0; j < count; j++)
		{
			tk2dSprite first = targetDockItems[j].First;
			first.PlaceAtPositionByAnchor(position, tk2dBaseSprite.Anchor.LowerCenter);
			float num6 = 0f;
			float y = Pixelator.Instance.CurrentTileScale * 2f * num;
			if (num4 < num2)
			{
				if (j != 0 || targetIndex == 2)
				{
					num5 += targetDockItems[j].First.GetBounds().size.x / 2f + num;
				}
				num6 = num5;
				num5 += targetDockItems[j].First.GetBounds().size.x / 2f + num;
			}
			else
			{
				num6 = num5 + num3 * num * (float)j;
			}
			first.transform.localPosition += new Vector3(num6, y, 0f);
		}
	}

	protected void OldArrangeDockItems(List<Tuple<tk2dSprite, PassiveItem>> targetDockItems, dfSprite targetDockSprite)
	{
		float num = DockPanel.PixelsToUnits();
		int num2 = ((GameManager.Instance.CurrentGameType != GameManager.GameType.COOP_2_PLAYER) ? 12 : 5);
		int num3 = Mathf.CeilToInt((float)targetDockItems.Count / (1f * (float)num2));
		float height = (float)num3 * 20f * Pixelator.Instance.CurrentTileScale / 3f;
		float num4 = 0f;
		num4 = (targetDockSprite.Width = ((num2 < targetDockItems.Count) ? ((float)(num2 + 1) * (20f * Pixelator.Instance.CurrentTileScale) / 3f) : ((float)(targetDockItems.Count + 1) * (20f * Pixelator.Instance.CurrentTileScale) / 3f)));
		targetDockSprite.Height = height;
		targetDockSprite.PerformLayout();
		Vector3 position = targetDockSprite.GetCorners()[2];
		int num6 = targetDockItems.Count;
		for (int i = 0; i < num3; i++)
		{
			int num7 = Mathf.Min(num2, num6);
			for (int j = 0; j < num7; j++)
			{
				if (num6 <= 0)
				{
					break;
				}
				int index = targetDockItems.Count - num6;
				Tuple<tk2dSprite, PassiveItem> tuple = targetDockItems[index];
				tk2dSprite first = tuple.First;
				first.PlaceAtPositionByAnchor(position, tk2dBaseSprite.Anchor.LowerCenter);
				float x = 20f * Pixelator.Instance.CurrentTileScale * num * (float)(j + 1);
				float num8 = 20f * Pixelator.Instance.CurrentTileScale * num * (float)i;
				num8 += Pixelator.Instance.CurrentTileScale * 5f * num;
				first.transform.localPosition += new Vector3(x, num8, 0f);
				num6--;
			}
			if (num6 <= 0)
			{
				break;
			}
		}
	}

	private void HandlePanelVisibility()
	{
		bool flag = false;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(GameManager.Instance.AllPlayers[i].PlayerIDX);
			if (instanceForPlayer.ActiveActions.MapAction.IsPressed)
			{
				flag = instanceForPlayer.IsKeyboardAndMouse();
			}
		}
		bool flag2 = dockItems.Count > 0;
		bool isVisible = secondaryDockItems != null && secondaryDockItems.Count > 0;
		bool isVisible2 = flag2 && m_selectedDockItemIndex != -1;
		if (flag)
		{
			GrabbyPanel.IsVisible = true;
			if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
			{
				ItemPanel_PC.IsVisible = isVisible2;
				ItemPanel_PC_Foreign.IsVisible = false;
				ItemPanel_PC.Parent.IsInteractive = true;
				ItemPanel_PC_Foreign.Parent.IsInteractive = false;
			}
			else
			{
				ItemPanel_PC_Foreign.IsVisible = isVisible2;
				ItemPanel_PC.IsVisible = false;
				ItemPanel_PC.Parent.IsInteractive = false;
				ItemPanel_PC_Foreign.Parent.IsInteractive = true;
			}
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				CoopDockPanelLeft.IsVisible = flag2;
				CoopDockPanelRight.IsVisible = isVisible;
				DockPanel.IsVisible = false;
			}
			else
			{
				CoopDockPanelLeft.IsVisible = false;
				CoopDockPanelRight.IsVisible = false;
				DockPanel.IsVisible = flag2;
			}
			SonyControlsPanel01.IsVisible = false;
			SonyControlsPanel02.IsVisible = false;
			SonyControlsPanel01Foreign.IsVisible = false;
			SonyControlsPanel02Foreign.IsVisible = false;
		}
		else
		{
			GrabbyPanel.IsVisible = false;
			ItemPanel_PC.IsVisible = false;
			ItemPanel_PC_Foreign.IsVisible = false;
			if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
			{
				CoopDockPanelLeft.IsVisible = flag2;
				CoopDockPanelRight.IsVisible = isVisible;
				DockPanel.IsVisible = false;
			}
			else
			{
				CoopDockPanelLeft.IsVisible = false;
				CoopDockPanelRight.IsVisible = false;
				DockPanel.IsVisible = flag2;
			}
			if (GameManager.Instance.PrimaryPlayer.inventory != null && GameManager.Instance.PrimaryPlayer.inventory.AllGuns.Count >= 5)
			{
				SonyControlsPanel01.IsVisible = false;
				SonyControlsPanel02.IsVisible = false;
				SonyControlsPanel01Foreign.IsVisible = false;
				SonyControlsPanel02Foreign.IsVisible = false;
			}
			else if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
			{
				SonyControlsPanel01.IsVisible = true;
				SonyControlsPanel02.IsVisible = flag2;
			}
			else
			{
				SonyControlsPanel01Foreign.IsVisible = true;
				SonyControlsPanel02Foreign.IsVisible = flag2;
			}
		}
	}

	private Vector2 ModifyMousePosition(Vector2 inputPosition)
	{
		return inputPosition;
	}

	private void Update()
	{
		m_manager.UIScale = Pixelator.Instance.ScaleTileScale / 3f;
		Vector2 screenSize = CoopDockPanelLeft.GUIManager.GetScreenSize();
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			float num = screenSize.x / 2f;
			CoopDockPanelLeft.Parent.Width = num;
			CoopDockPanelRight.Parent.Width = num;
			CoopDockPanelLeft.Parent.RelativePosition = CoopDockPanelLeft.Parent.RelativePosition.WithX(0f);
			CoopDockPanelRight.Parent.RelativePosition = CoopDockPanelRight.Parent.RelativePosition.WithX(num);
			CoopDockPanelLeft.Parent.RelativePosition = CoopDockPanelLeft.Parent.RelativePosition.WithY(screenSize.y - CoopDockPanelLeft.Size.y);
			CoopDockPanelRight.Parent.RelativePosition = CoopDockPanelRight.Parent.RelativePosition.WithY(screenSize.y - CoopDockPanelRight.Size.y);
		}
		else
		{
			DockPanel.Parent.RelativePosition = DockPanel.Parent.RelativePosition.WithY(screenSize.y - DockPanel.Size.y);
		}
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GungeonActions activeActions = BraveInput.GetInstanceForPlayer(GameManager.Instance.AllPlayers[i].PlayerIDX).ActiveActions;
			if (activeActions != null)
			{
				if (activeActions.MinimapZoomOutAction.WasPressed)
				{
					Minimap.Instance.AttemptZoomMinimap(0.2f);
				}
				if (activeActions.MinimapZoomInAction.WasPressed)
				{
					Minimap.Instance.AttemptZoomMinimap(-0.2f);
				}
			}
		}
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.RESOURCEFUL_RAT)
		{
			RatTaunty.IsVisible = true;
		}
		if (m_active)
		{
			if (Minimap.Instance.HeldOpen)
			{
				GungeonActions activeActions2 = BraveInput.PrimaryPlayerInstance.ActiveActions;
				if (activeActions2.MapAction.WasPressed || activeActions2.PauseAction.WasPressed)
				{
					activeActions2.MapAction.Suppress();
					activeActions2.PauseAction.Suppress();
					Minimap.Instance.ToggleMinimap(false);
					return;
				}
				if (BraveInput.SecondaryPlayerInstance != null)
				{
					activeActions2 = BraveInput.SecondaryPlayerInstance.ActiveActions;
					if (activeActions2.MapAction.WasPressed || activeActions2.PauseAction.WasPressed)
					{
						activeActions2.MapAction.Suppress();
						activeActions2.PauseAction.Suppress();
						Minimap.Instance.ToggleMinimap(false);
						return;
					}
				}
			}
			else
			{
				if (BraveInput.PrimaryPlayerInstance.ActiveActions.MapAction.WasReleased && (BraveInput.SecondaryPlayerInstance == null || !BraveInput.SecondaryPlayerInstance.ActiveActions.MapAction.IsPressed))
				{
					Minimap.Instance.ToggleMinimap(false);
					return;
				}
				if (BraveInput.SecondaryPlayerInstance != null && BraveInput.SecondaryPlayerInstance.ActiveActions.MapAction.WasReleased && !BraveInput.PrimaryPlayerInstance.ActiveActions.MapAction.IsPressed)
				{
					Minimap.Instance.ToggleMinimap(false);
					return;
				}
				if (!BraveInput.PrimaryPlayerInstance.ActiveActions.MapAction.IsPressed && (GameManager.Instance.CurrentGameType == GameManager.GameType.SINGLE_PLAYER || !BraveInput.GetInstanceForPlayer(GameManager.Instance.SecondaryPlayer.PlayerIDX).ActiveActions.MapAction.IsPressed))
				{
					Minimap.Instance.ToggleMinimap(false);
					return;
				}
			}
			UpdateDockItemSpriteScales();
			HandlePanelVisibility();
			bool flag = false;
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				List<Tuple<tk2dSprite, PassiveItem>> list = ((j != 0) ? secondaryDockItems : dockItems);
				BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(GameManager.Instance.AllPlayers[j].PlayerIDX);
				if (!instanceForPlayer.ActiveActions.MapAction.IsPressed && !Minimap.Instance.HeldOpen)
				{
					continue;
				}
				if (instanceForPlayer.IsKeyboardAndMouse())
				{
					Vector2 vector = Input.mousePosition;
					Vector2 vector2 = ModifyMousePosition(vector);
					ControllerCrosshair.IsVisible = false;
					SelectNearbyTeleportIcon(vector2);
					if (Input.GetMouseButtonDown(0))
					{
						float num2 = DockPanel.PixelsToUnits();
						Vector2 b = minimapCamera.ScreenToWorldPoint(vector).XY();
						bool flag2 = false;
						for (int k = 0; k < list.Count; k++)
						{
							Vector2 worldCenter = list[k].First.WorldCenter;
							float num3 = Vector2.Distance(worldCenter, b);
							if (num3 < 20f * Pixelator.Instance.CurrentTileScale / 2f * num2)
							{
								flag2 = true;
								SelectDockItem(k, j);
								break;
							}
						}
						if (!flag2)
						{
							m_isPanning = true;
							m_lastMousePosition = vector2;
							m_panPixelDistTravelled = 0f;
						}
					}
					else if (Input.GetMouseButton(0) && m_isPanning)
					{
						Minimap.Instance.AttemptPanCamera((minimapCamera.ScreenToWorldPoint(vector2) - minimapCamera.ScreenToWorldPoint(m_lastMousePosition)) * -1f);
						m_panPixelDistTravelled += Vector2.Distance(m_lastMousePosition, vector2);
						m_lastMousePosition = vector2;
					}
					else if (Input.GetMouseButtonUp(0))
					{
						Vector2 vector3 = BraveUtility.GetMinimapViewportPosition(vector2);
						bool flag3 = vector3.x > 0.17f && vector3.y < 0.88f && vector3.x < 0.825f && vector3.y > 0.12f;
						bool flag4 = m_panPixelDistTravelled / (float)Screen.width < 0.005f;
						if (flag3 && flag4)
						{
							AttemptTeleport();
						}
						m_isPanning = false;
					}
					if (Input.GetAxis("Mouse ScrollWheel") != 0f)
					{
						Minimap.Instance.AttemptZoomCamera(Input.GetAxis("Mouse ScrollWheel") * -1f);
					}
					continue;
				}
				Vector2 vector4 = Vector2.zero;
				bool flag5 = false;
				bool flag6 = false;
				bool flag7 = false;
				bool flag8 = false;
				bool flag9 = false;
				bool flag10 = false;
				bool flag11 = false;
				if (instanceForPlayer.ActiveActions != null)
				{
					vector4 = instanceForPlayer.ActiveActions.Aim.Vector;
					flag6 = instanceForPlayer.ActiveActions.InteractAction;
					InputDevice device = instanceForPlayer.ActiveActions.Device;
					if (device != null)
					{
						flag5 = device.RightStickButton.WasPressed;
						flag7 = device.Action4.WasPressed;
						flag8 = device.DPadLeft.WasPressed;
						flag9 = device.DPadRight.WasPressed;
						flag10 = device.DPadUp.WasPressed;
						flag11 = device.DPadDown.WasPressed;
					}
				}
				ControllerCrosshair.IsVisible = true;
				if (vector4.magnitude > 0f && !flag)
				{
					flag = true;
					Minimap.Instance.AttemptPanCamera(vector4.ToVector3ZUp() * GameManager.INVARIANT_DELTA_TIME * 0.8f);
				}
				SelectNearbyTeleportIcon(new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f));
				if (flag5)
				{
					Minimap.Instance.TogglePresetZoomValue();
				}
				if (flag6)
				{
					AttemptTeleport();
				}
				if (flag7)
				{
					DropSelectedItem();
				}
				int count = list.Count;
				if (flag9 && list.Count > 0)
				{
					int i2 = Mathf.Max(0, (m_selectedDockItemIndex + 1) % count);
					if (j != m_targetDockIndex)
					{
						i2 = 0;
					}
					SelectDockItem(i2, j);
				}
				if (flag8 && list.Count > 0)
				{
					int i3 = Mathf.Max(0, (m_selectedDockItemIndex + count - 1) % count);
					if (j != m_targetDockIndex)
					{
						i3 = 0;
					}
					SelectDockItem(i3, j);
				}
				if (flag10 || flag11)
				{
					Minimap instance = Minimap.Instance;
					Vector2 screenPosition = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
					if (instanceForPlayer.IsKeyboardAndMouse())
					{
						screenPosition = ModifyMousePosition(Input.mousePosition);
					}
					RoomHandler roomHandler = null;
					float dist;
					RoomHandler nearestVisibleRoom = instance.GetNearestVisibleRoom(screenPosition, out dist);
					if (nearestVisibleRoom != null && !instance.IsPanning)
					{
						m_currentTeleportTargetIndex = instance.roomsContainingTeleporters.IndexOf(nearestVisibleRoom);
					}
					if (dist < 0.5f || instance.IsPanning)
					{
						int dir = (flag10 ? 1 : (-1));
						roomHandler = instance.NextSelectedTeleporter(ref m_currentTeleportTargetIndex, dir);
					}
					else
					{
						roomHandler = nearestVisibleRoom;
					}
					if (roomHandler != null && roomHandler.TeleportersActive)
					{
						instance.PanToPosition(instance.RoomToTeleportMap[roomHandler].GetComponent<tk2dBaseSprite>().WorldCenter);
					}
				}
			}
		}
		else
		{
			m_isPanning = false;
		}
	}

	private void SelectNearbyTeleportIcon(Vector2 positionToCheck)
	{
		GameObject icon = null;
		RoomHandler roomHandler = Minimap.Instance.CheckIconsNearCursor(positionToCheck, out icon);
		if (roomHandler != null && !roomHandler.TeleportersActive)
		{
			if (m_currentTeleportIconSprite != null)
			{
				SpriteOutlineManager.RemoveOutlineFromSprite(m_currentTeleportIconSprite);
				m_currentTeleportIconSprite = null;
				m_currentTeleportTarget = null;
			}
			return;
		}
		tk2dBaseSprite tk2dBaseSprite2 = ((!(icon != null)) ? null : icon.GetComponent<tk2dBaseSprite>());
		if (tk2dBaseSprite2 == null)
		{
			if (m_currentTeleportIconSprite != null)
			{
				SpriteOutlineManager.RemoveOutlineFromSprite(m_currentTeleportIconSprite);
				m_currentTeleportIconSprite = null;
				m_currentTeleportTarget = null;
			}
		}
		else
		{
			tk2dBaseSprite2.ignoresTiltworldDepth = true;
			if (m_currentTeleportIconSprite != null && m_currentTeleportIconSprite != tk2dBaseSprite2)
			{
				SpriteOutlineManager.RemoveOutlineFromSprite(m_currentTeleportIconSprite);
				m_currentTeleportIconSprite = null;
				m_currentTeleportTarget = null;
				SpriteOutlineManager.AddOutlineToSprite(tk2dBaseSprite2, Color.white);
				m_currentTeleportTarget = roomHandler;
				m_currentTeleportIconSprite = tk2dBaseSprite2;
			}
			else if (!(m_currentTeleportIconSprite == tk2dBaseSprite2))
			{
				SpriteOutlineManager.AddOutlineToSprite(tk2dBaseSprite2, Color.white);
				m_currentTeleportTarget = roomHandler;
				m_currentTeleportIconSprite = tk2dBaseSprite2;
			}
		}
		if (m_currentTeleportTarget != null)
		{
			ControllerCrosshair.SpriteName = "minimap_select_square_001";
			ControllerCrosshair.Size = ControllerCrosshair.SpriteInfo.sizeInPixels * 3f;
			ControllerCrosshair.GetComponentInChildren<dfPanel>().IsVisible = true;
		}
		else
		{
			ControllerCrosshair.SpriteName = "minimap_select_crosshair_001";
			ControllerCrosshair.Size = ControllerCrosshair.SpriteInfo.sizeInPixels * 3f;
			ControllerCrosshair.GetComponentInChildren<dfPanel>().IsVisible = false;
		}
	}

	private bool AttemptTeleport()
	{
		if ((bool)Minimap.Instance && Minimap.Instance.PreventAllTeleports)
		{
			return false;
		}
		if (GameUIRoot.Instance.DisplayingConversationBar)
		{
			return false;
		}
		PlayerController[] allPlayers = GameManager.Instance.AllPlayers;
		foreach (PlayerController playerController in allPlayers)
		{
			if (playerController.CurrentRoom != null && playerController.CurrentRoom.CompletelyPreventLeaving)
			{
				return false;
			}
		}
		if (m_currentTeleportTarget != null)
		{
			RoomHandler currentTeleportTarget = m_currentTeleportTarget;
			for (int j = 0; j < allPlayers.Length; j++)
			{
				allPlayers[j].AttemptTeleportToRoom(currentTeleportTarget);
			}
			return true;
		}
		return false;
	}

	private void DeselectDockItem()
	{
		ClearSynergyHighlights();
		if (m_selectedDockItemIndex != -1)
		{
			if (m_selectedDockItemIndex < SelectedDockItems.Count)
			{
				SpriteOutlineManager.RemoveOutlineFromSprite(SelectedDockItems[m_selectedDockItemIndex].First);
				m_selectedDockItemIndex = -1;
			}
			else
			{
				m_selectedDockItemIndex = -1;
			}
		}
	}

	private void UpdateDockItemSpriteScales()
	{
		for (int i = 0; i < dockItems.Count; i++)
		{
			tk2dSprite first = dockItems[i].First;
			first.scale = Vector3.one * GameUIUtility.GetCurrentTK2D_DFScale(m_manager);
			if (SpriteOutlineManager.HasOutline(first))
			{
				tk2dSprite[] outlineSprites = SpriteOutlineManager.GetOutlineSprites(first);
				for (int j = 0; j < outlineSprites.Length; j++)
				{
					outlineSprites[j].scale = first.scale;
				}
			}
		}
		for (int k = 0; k < secondaryDockItems.Count; k++)
		{
			tk2dSprite first2 = secondaryDockItems[k].First;
			first2.scale = Vector3.one * GameUIUtility.GetCurrentTK2D_DFScale(m_manager);
			if (SpriteOutlineManager.HasOutline(first2))
			{
				tk2dSprite[] outlineSprites2 = SpriteOutlineManager.GetOutlineSprites(first2);
				for (int l = 0; l < outlineSprites2.Length; l++)
				{
					outlineSprites2[l].scale = first2.scale;
				}
			}
		}
	}

	private void SelectDockItem(int i, int targetPlayerID)
	{
		if (m_selectedDockItemIndex == i && m_targetDockIndex == targetPlayerID)
		{
			return;
		}
		DeselectDockItem();
		List<Tuple<tk2dSprite, PassiveItem>> list = dockItems;
		if (targetPlayerID == 1)
		{
			list = secondaryDockItems;
		}
		if (i < list.Count)
		{
			SpriteOutlineManager.AddOutlineToSprite(list[i].First, Color.white);
			tk2dSprite[] outlineSprites = SpriteOutlineManager.GetOutlineSprites(list[i].First);
			for (int j = 0; j < outlineSprites.Length; j++)
			{
				outlineSprites[j].scale = list[i].First.scale;
			}
		}
		m_targetDockIndex = targetPlayerID;
		m_selectedDockItemIndex = i;
		PassiveItem second = list[i].Second;
		DropItemButton.IsEnabled = second.CanActuallyBeDropped(second.Owner);
		DropItemSprite.Color = ((!DropItemButton.IsEnabled) ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white);
		DropItemLabel.Color = ((!DropItemButton.IsEnabled) ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white);
		DropItemSpriteForeign.Color = ((!DropItemButton.IsEnabled) ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white);
		DropItemLabelForeign.Color = ((!DropItemButton.IsEnabled) ? new Color(0.5f, 0.5f, 0.5f, 1f) : Color.white);
		if ((bool)second)
		{
			UpdateSynergyHighlights(second.PickupObjectId);
		}
	}

	private void ClearSynergyHighlights()
	{
		for (int i = 0; i < extantSynergyArrows.Count; i++)
		{
			UnityEngine.Object.Destroy(extantSynergyArrows[i].gameObject);
		}
		extantSynergyArrows.Clear();
		for (int j = 0; j < dockItems.Count; j++)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(dockItems[j].First);
		}
		for (int k = 0; k < secondaryDockItems.Count; k++)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(secondaryDockItems[k].First);
		}
	}

	private void CreateArrow(tk2dBaseSprite targetSprite, dfControl targetParent)
	{
		dfSprite dfSprite2 = targetParent.AddControl<dfSprite>();
		dfSprite2.Atlas = Minimap.Instance.UIMinimap.DispenserIcon.Atlas;
		dfSprite2.SpriteName = "synergy_ammonomicon_arrow_001";
		dfSprite2.Size = dfSprite2.SpriteInfo.sizeInPixels * 4f;
		Bounds bounds = targetSprite.GetBounds();
		Bounds untrimmedBounds = targetSprite.GetUntrimmedBounds();
		Vector3 size = bounds.size;
		dfSprite2.transform.position = (targetSprite.WorldCenter.ToVector3ZisY() + new Vector3(-8f * targetParent.PixelsToUnits(), size.y / 2f + 32f * targetParent.PixelsToUnits(), 0f)).WithZ(0f);
		dfSprite2.BringToFront();
		dfSprite2.Invalidate();
		extantSynergyArrows.Add(dfSprite2);
	}

	private void UpdateSynergyHighlights(int selectedID)
	{
		AdvancedSynergyDatabase synergyManager = GameManager.Instance.SynergyManager;
		dfControl rootContainer = DockSprite.GetRootContainer();
		rootContainer.BringToFront();
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController playerController = GameManager.Instance.AllPlayers[i];
			for (int j = 0; j < synergyManager.synergies.Length; j++)
			{
				if (!playerController.ActiveExtraSynergies.Contains(j))
				{
					continue;
				}
				AdvancedSynergyEntry advancedSynergyEntry = synergyManager.synergies[j];
				if (!advancedSynergyEntry.ContainsPickup(selectedID))
				{
					continue;
				}
				for (int k = 0; k < dockItems.Count; k++)
				{
					int pickupObjectId = dockItems[k].Second.PickupObjectId;
					if (pickupObjectId != selectedID && advancedSynergyEntry.ContainsPickup(pickupObjectId))
					{
						SpriteOutlineManager.AddOutlineToSprite(dockItems[k].First, SynergyDatabase.SynergyBlue);
						CreateArrow(dockItems[k].First, rootContainer);
					}
				}
				for (int l = 0; l < secondaryDockItems.Count; l++)
				{
					int pickupObjectId2 = secondaryDockItems[l].Second.PickupObjectId;
					if (pickupObjectId2 != selectedID && advancedSynergyEntry.ContainsPickup(pickupObjectId2))
					{
						SpriteOutlineManager.AddOutlineToSprite(secondaryDockItems[l].First, SynergyDatabase.SynergyBlue);
						CreateArrow(secondaryDockItems[l].First, rootContainer);
					}
				}
				for (int m = 0; m < playerController.inventory.AllGuns.Count; m++)
				{
					int pickupObjectId3 = playerController.inventory.AllGuns[m].PickupObjectId;
					if (pickupObjectId3 != selectedID && advancedSynergyEntry.ContainsPickup(pickupObjectId3))
					{
						int num = playerController.inventory.AllGuns.IndexOf(playerController.CurrentGun);
						int gunIndex = playerController.inventory.AllGuns.Count - (num - m + playerController.inventory.AllGuns.Count - 1) % playerController.inventory.AllGuns.Count - 1;
						tk2dClippedSprite spriteForUnfoldedGun = GameUIRoot.Instance.GetSpriteForUnfoldedGun(playerController.PlayerIDX, gunIndex);
						if ((bool)spriteForUnfoldedGun)
						{
							SpriteOutlineManager.RemoveOutlineFromSprite(spriteForUnfoldedGun);
							SpriteOutlineManager.AddOutlineToSprite<tk2dClippedSprite>(spriteForUnfoldedGun, SynergyDatabase.SynergyBlue);
							CreateArrow(spriteForUnfoldedGun, spriteForUnfoldedGun.transform.parent.parent.GetComponent<dfControl>());
						}
					}
				}
				for (int n = 0; n < playerController.activeItems.Count; n++)
				{
					int pickupObjectId4 = playerController.activeItems[n].PickupObjectId;
					if (pickupObjectId4 != selectedID && advancedSynergyEntry.ContainsPickup(pickupObjectId4))
					{
						int num2 = playerController.activeItems.IndexOf(playerController.CurrentItem);
						int itemIndex = playerController.activeItems.Count - (num2 - n + playerController.activeItems.Count - 1) % playerController.activeItems.Count - 1;
						tk2dClippedSprite spriteForUnfoldedItem = GameUIRoot.Instance.GetSpriteForUnfoldedItem(playerController.PlayerIDX, itemIndex);
						if ((bool)spriteForUnfoldedItem)
						{
							SpriteOutlineManager.RemoveOutlineFromSprite(spriteForUnfoldedItem);
							SpriteOutlineManager.AddOutlineToSprite<tk2dClippedSprite>(spriteForUnfoldedItem, SynergyDatabase.SynergyBlue);
							CreateArrow(spriteForUnfoldedItem, spriteForUnfoldedItem.transform.parent.parent.GetComponent<dfControl>());
						}
					}
				}
			}
		}
	}

	private IEnumerator MoveAllThings()
	{
		float elapsed = 0f;
		float transitionTime = 0.25f;
		bool cachedActive = m_active;
		bool hasRepositioned = false;
		while (elapsed < transitionTime)
		{
			if (cachedActive != m_active)
			{
				yield break;
			}
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t = Mathf.Clamp01(elapsed / transitionTime);
			if (!m_active)
			{
				t = 1f - t;
			}
			for (int i = 0; i < panels.Count; i++)
			{
				float first = panelTimings[panels[i]].First;
				float second = panelTimings[panels[i]].Second;
				float t2 = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - first) / (second - first)));
				panels[i].RelativePosition = Vector3.Lerp(inactivePositions[panels[i]], activePositions[panels[i]], t2);
			}
			yield return null;
			if (!hasRepositioned)
			{
				hasRepositioned = true;
				if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
				{
					dfSprite componentInChildren = CoopDockPanelLeft.GetComponentInChildren<dfSprite>();
					dfSprite componentInChildren2 = CoopDockPanelRight.GetComponentInChildren<dfSprite>();
					ArrangeDockItems(dockItems, componentInChildren, 1);
					ArrangeDockItems(secondaryDockItems, componentInChildren2, 2);
				}
				else
				{
					ArrangeDockItems(dockItems, DockSprite);
				}
			}
		}
		if (!m_active)
		{
			minimapCamera.enabled = false;
			for (int j = 0; j < AdditionalControlsToToggle.Count; j++)
			{
				AdditionalControlsToToggle[j].IsVisible = false;
			}
			SonyControlsPanel01.IsVisible = false;
			SonyControlsPanel02.IsVisible = false;
			SonyControlsPanel01Foreign.IsVisible = false;
			SonyControlsPanel02Foreign.IsVisible = false;
		}
	}

	private void UpdateLevelNameLabel()
	{
		Dungeon dungeon = GameManager.Instance.Dungeon;
		string text = LevelNameLabel.ForceGetLocalizedValue(dungeon.DungeonFloorName);
		GameLevelDefinition lastLoadedLevelDefinition = GameManager.Instance.GetLastLoadedLevelDefinition();
		int num = -1;
		if (lastLoadedLevelDefinition != null)
		{
			num = GameManager.Instance.dungeonFloors.IndexOf(lastLoadedLevelDefinition);
		}
		string text2 = LevelNameLabel.ForceGetLocalizedValue("#LEVEL") + ((num >= 0) ? num.ToString() : "?") + ": " + text;
		LevelNameLabel.Text = text2;
		LevelNameLabel.Invalidate();
		LevelNameLabel.PerformLayout();
	}

	private void UpdateQuestText()
	{
	}

	private void Activate()
	{
		UpdateLevelNameLabel();
		UpdateQuestText();
		DeselectDockItem();
		m_active = true;
		minimapCamera.enabled = true;
		for (int i = 0; i < AdditionalControlsToToggle.Count; i++)
		{
			AdditionalControlsToToggle[i].IsVisible = true;
			dfSpriteAnimation componentInChildren = AdditionalControlsToToggle[i].GetComponentInChildren<dfSpriteAnimation>();
			if ((bool)componentInChildren)
			{
				componentInChildren.Play();
			}
		}
		RecalculatePositions();
		StartCoroutine(MoveAllThings());
	}

	private void Deactivate()
	{
		DeselectDockItem();
		m_active = false;
		RecalculatePositions();
		ControllerCrosshair.IsVisible = false;
		StartCoroutine(MoveAllThings());
		if (m_currentTeleportIconSprite != null)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(m_currentTeleportIconSprite);
			m_currentTeleportIconSprite = null;
			m_currentTeleportTarget = null;
		}
		for (int i = 0; i < AdditionalControlsToToggle.Count; i++)
		{
			AdditionalControlsToToggle[i].IsVisible = false;
		}
	}
}
