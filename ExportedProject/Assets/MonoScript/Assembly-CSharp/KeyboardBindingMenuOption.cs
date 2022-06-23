using System.Collections;
using InControl;
using UnityEngine;

public class KeyboardBindingMenuOption : MonoBehaviour
{
	public dfLabel CenterColumnLabel;

	public dfLabel CommandLabel;

	public dfButton KeyButton;

	public dfButton AltKeyButton;

	public dfLabel AltAlignLabel;

	public GungeonActions.GungeonActionType ActionType;

	public bool IsControllerMode;

	private FullOptionsMenuController m_parentOptionsMenu;

	private Vector2 m_cachedKeyButtonPosition;

	public bool NonBindable { get; set; }

	public string OverrideKeyString { get; set; }

	public string OverrideAltKeyString { get; set; }

	public void Initialize()
	{
		if (m_parentOptionsMenu == null)
		{
			m_parentOptionsMenu = GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().OptionsMenu;
		}
		if (IsControllerMode)
		{
			InitializeController();
		}
		else
		{
			InitializeKeyboard();
		}
		if (NonBindable)
		{
			CommandLabel.IsInteractive = false;
			CommandLabel.Color = new Color(0.25f, 0.25f, 0.25f);
		}
		else
		{
			CommandLabel.IsInteractive = true;
			CommandLabel.Color = new Color(0.596f, 0.596f, 0.596f, 1f);
		}
	}

	private void InitializeController()
	{
		GungeonActions activeActions = GetBestInputInstance().ActiveActions;
		PlayerAction actionFromType = activeActions.GetActionFromType(ActionType);
		bool flag = false;
		string text = "-";
		bool flag2 = false;
		string text2 = "-";
		for (int i = 0; i < actionFromType.Bindings.Count; i++)
		{
			BindingSource bindingSource = actionFromType.Bindings[i];
			if (bindingSource.BindingSourceType == BindingSourceType.DeviceBindingSource)
			{
				DeviceBindingSource deviceBindingSource = bindingSource as DeviceBindingSource;
				GameOptions.ControllerSymbology currentSymbology = BraveInput.GetCurrentSymbology(FullOptionsMenuController.CurrentBindingPlayerTargetIndex);
				if (!flag)
				{
					flag = true;
					text = UIControllerButtonHelper.GetUnifiedControllerButtonTag(deviceBindingSource.Control, currentSymbology, activeActions);
				}
				else if (!flag2)
				{
					flag2 = true;
					text2 = UIControllerButtonHelper.GetUnifiedControllerButtonTag(deviceBindingSource.Control, currentSymbology, activeActions);
					break;
				}
			}
			if (bindingSource.BindingSourceType == BindingSourceType.UnknownDeviceBindingSource)
			{
				UnknownDeviceBindingSource unknownDeviceBindingSource = bindingSource as UnknownDeviceBindingSource;
				if (!flag)
				{
					flag = true;
					text = unknownDeviceBindingSource.Control.Control.ToString();
				}
				else if (!flag2)
				{
					flag2 = true;
					text2 = unknownDeviceBindingSource.Control.Control.ToString();
				}
			}
		}
		KeyButton.Text = (string.IsNullOrEmpty(OverrideKeyString) ? text.Trim() : OverrideKeyString);
		AltKeyButton.Text = (string.IsNullOrEmpty(OverrideAltKeyString) ? text2.Trim() : OverrideAltKeyString);
		AltKeyButton.transform.position = AltKeyButton.transform.position.WithX(AltAlignLabel.GetCenter().x);
		if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ITALIAN || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.GERMAN)
		{
			KeyButton.Padding = new RectOffset(60, 0, 0, 0);
		}
		else if (GameManager.Options.CurrentLanguage != 0)
		{
			KeyButton.Padding = new RectOffset(180, 0, 0, 0);
		}
		else
		{
			KeyButton.Padding = new RectOffset(0, 0, 0, 0);
		}
		if ((bool)CenterColumnLabel)
		{
			CenterColumnLabel.Padding = KeyButton.Padding;
		}
		GetComponent<dfPanel>().PerformLayout();
		CommandLabel.RelativePosition = CommandLabel.RelativePosition.WithX(0f);
	}

	public BraveInput GetBestInputInstance()
	{
		BraveInput braveInput = null;
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER && Foyer.DoMainMenu)
		{
			return BraveInput.PlayerlessInstance;
		}
		return BraveInput.GetInstanceForPlayer(FullOptionsMenuController.CurrentBindingPlayerTargetIndex);
	}

	private void InitializeKeyboard()
	{
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(0);
			BraveInput instanceForPlayer2 = BraveInput.GetInstanceForPlayer(1);
			if ((bool)instanceForPlayer && (bool)instanceForPlayer2)
			{
				GungeonActions activeActions = instanceForPlayer.ActiveActions;
				GungeonActions activeActions2 = instanceForPlayer2.ActiveActions;
				if (activeActions != null && activeActions2 != null)
				{
					PlayerAction actionFromType = activeActions.GetActionFromType(ActionType);
					PlayerAction actionFromType2 = activeActions2.GetActionFromType(ActionType);
					actionFromType2.ClearBindingsOfType(BindingSourceType.KeyBindingSource);
					for (int i = 0; i < actionFromType.Bindings.Count; i++)
					{
						BindingSource bindingSource = actionFromType.Bindings[i];
						if (bindingSource.BindingSourceType == BindingSourceType.KeyBindingSource && bindingSource is KeyBindingSource)
						{
							BindingSource binding = new KeyBindingSource((bindingSource as KeyBindingSource).Control);
							actionFromType2.AddBinding(binding);
						}
					}
				}
			}
		}
		BraveInput bestInputInstance = GetBestInputInstance();
		GungeonActions activeActions3 = bestInputInstance.ActiveActions;
		PlayerAction actionFromType3 = activeActions3.GetActionFromType(ActionType);
		bool flag = false;
		string text = "-";
		bool flag2 = false;
		string text2 = "-";
		for (int j = 0; j < actionFromType3.Bindings.Count; j++)
		{
			BindingSource bindingSource2 = actionFromType3.Bindings[j];
			if (bindingSource2.BindingSourceType == BindingSourceType.KeyBindingSource || bindingSource2.BindingSourceType == BindingSourceType.MouseBindingSource)
			{
				if (!flag)
				{
					flag = true;
					text = bindingSource2.Name;
				}
				else if (!flag2)
				{
					flag2 = true;
					text2 = bindingSource2.Name;
					break;
				}
			}
		}
		KeyButton.Text = text.Trim();
		AltKeyButton.Text = text2.Trim();
		AltKeyButton.transform.position = AltKeyButton.transform.position.WithX(AltAlignLabel.GetCenter().x);
		if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ITALIAN || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.GERMAN)
		{
			KeyButton.Padding = new RectOffset(60, 0, 0, 0);
		}
		else if (GameManager.Options.CurrentLanguage != 0)
		{
			KeyButton.Padding = new RectOffset(180, 0, 0, 0);
		}
		else
		{
			KeyButton.Padding = new RectOffset(0, 0, 0, 0);
		}
		if ((bool)CenterColumnLabel)
		{
			CenterColumnLabel.Padding = KeyButton.Padding;
		}
		GetComponent<dfPanel>().PerformLayout();
		CommandLabel.RelativePosition = CommandLabel.RelativePosition.WithX(0f);
	}

	public void KeyClicked(dfControl source, dfControlEventArgs args)
	{
		if (!NonBindable)
		{
			EnterAssignmentMode(false);
			GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().OptionsMenu.DoModalKeyBindingDialog(CommandLabel.Text);
			StartCoroutine(WaitForAssignmentModeToEnd());
		}
	}

	public void AltKeyClicked(dfControl source, dfControlEventArgs args)
	{
		if (!NonBindable)
		{
			EnterAssignmentMode(true);
			GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().OptionsMenu.DoModalKeyBindingDialog(CommandLabel.Text);
			StartCoroutine(WaitForAssignmentModeToEnd());
		}
	}

	private void Update()
	{
		if (!Input.GetKeyDown(KeyCode.Delete))
		{
			return;
		}
		if (KeyButton.HasFocus)
		{
			GungeonActions activeActions = GetBestInputInstance().ActiveActions;
			PlayerAction actionFromType = activeActions.GetActionFromType(ActionType);
			if (IsControllerMode)
			{
				actionFromType.ClearSpecificBindingByType(0, BindingSourceType.DeviceBindingSource);
				InitializeController();
			}
			else
			{
				actionFromType.ClearSpecificBindingByType(0, BindingSourceType.KeyBindingSource, BindingSourceType.MouseBindingSource);
				InitializeKeyboard();
			}
		}
		else if (AltKeyButton.HasFocus)
		{
			GungeonActions activeActions2 = GetBestInputInstance().ActiveActions;
			PlayerAction actionFromType2 = activeActions2.GetActionFromType(ActionType);
			if (IsControllerMode)
			{
				actionFromType2.ClearSpecificBindingByType(1, BindingSourceType.DeviceBindingSource);
				InitializeController();
			}
			else
			{
				actionFromType2.ClearSpecificBindingByType(1, BindingSourceType.KeyBindingSource, BindingSourceType.MouseBindingSource);
				InitializeKeyboard();
			}
		}
	}

	private IEnumerator WaitForAssignmentModeToEnd()
	{
		GungeonActions activeActions = GetBestInputInstance().ActiveActions;
		PlayerAction targetAction = activeActions.GetActionFromType(ActionType);
		while (targetAction.IsListeningForBinding)
		{
			yield return null;
		}
		Initialize();
	}

	public void EnterAssignmentMode(bool isAlternateKey)
	{
		GungeonActions activeActions = GetBestInputInstance().ActiveActions;
		PlayerAction actionFromType = activeActions.GetActionFromType(ActionType);
		BindingListenOptions bindingOptions = new BindingListenOptions();
		if (IsControllerMode)
		{
			bindingOptions.IncludeControllers = true;
			bindingOptions.IncludeNonStandardControls = true;
			bindingOptions.IncludeKeys = true;
			bindingOptions.IncludeMouseButtons = false;
			bindingOptions.IncludeMouseScrollWheel = false;
			bindingOptions.IncludeModifiersAsFirstClassKeys = false;
			bindingOptions.IncludeUnknownControllers = GameManager.Options.allowUnknownControllers;
		}
		else
		{
			bindingOptions.IncludeControllers = false;
			bindingOptions.IncludeNonStandardControls = false;
			bindingOptions.IncludeKeys = true;
			bindingOptions.IncludeMouseButtons = true;
			bindingOptions.IncludeMouseScrollWheel = true;
			bindingOptions.IncludeModifiersAsFirstClassKeys = true;
		}
		bindingOptions.MaxAllowedBindingsPerType = 2u;
		bindingOptions.OnBindingFound = delegate(PlayerAction action, BindingSource binding)
		{
			if (binding == new KeyBindingSource(Key.Escape))
			{
				action.StopListeningForBinding();
				GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().OptionsMenu.ClearModalKeyBindingDialog(null, null);
				return false;
			}
			if (binding == new KeyBindingSource(Key.Delete))
			{
				action.StopListeningForBinding();
				GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().OptionsMenu.ClearModalKeyBindingDialog(null, null);
				return false;
			}
			if (IsControllerMode && binding is KeyBindingSource)
			{
				return false;
			}
			action.StopListeningForBinding();
			if (!m_parentOptionsMenu.ActionIsMultibindable(ActionType, activeActions))
			{
				m_parentOptionsMenu.ClearBindingFromAllControls(FullOptionsMenuController.CurrentBindingPlayerTargetIndex, binding);
			}
			action.SetBindingOfTypeByNumber(binding, binding.BindingSourceType, isAlternateKey ? 1 : 0, bindingOptions.OnBindingAdded);
			GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().OptionsMenu.ToggleKeyBindingDialogState(binding);
			Initialize();
			return false;
		};
		bindingOptions.OnBindingAdded = delegate
		{
			if (FullOptionsMenuController.CurrentBindingPlayerTargetIndex == 1)
			{
				GameManager.Options.CurrentControlPresetP2 = GameOptions.ControlPreset.CUSTOM;
			}
			else
			{
				GameManager.Options.CurrentControlPreset = GameOptions.ControlPreset.CUSTOM;
			}
			BraveOptionsMenuItem[] componentsInChildren = CenterColumnLabel.Parent.Parent.Parent.GetComponentsInChildren<BraveOptionsMenuItem>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].InitializeFromOptions();
				componentsInChildren[i].ForceRefreshDisplayLabel();
			}
			Initialize();
		};
		actionFromType.ListenOptions = bindingOptions;
		if (!actionFromType.IsListeningForBinding)
		{
			actionFromType.ListenForBinding();
		}
	}
}
