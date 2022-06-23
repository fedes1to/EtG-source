using System;
using System.Collections;
using System.Collections.Generic;
using InControl;
using UnityEngine;

public class FullOptionsMenuController : MonoBehaviour
{
	public dfButton PrimaryCancelButton;

	public dfButton PrimaryResetDefaultsButton;

	public dfButton PrimaryConfirmButton;

	public dfScrollPanel TabAudio;

	public dfScrollPanel TabVideo;

	public dfScrollPanel TabGameplay;

	public dfScrollPanel TabControls;

	public dfScrollPanel TabKeyboardBindings;

	public dfScrollPanel TabCredits;

	public dfScrollPanel TabHowToPlay;

	public dfPanel ModalKeyBindingDialog;

	public PreOptionsMenuController PreOptionsMenu;

	protected GameOptions cloneOptions;

	protected dfPanel m_panel;

	private bool finishedInitialization;

	private List<BraveOptionsMenuItem> m_menuItems;

	private dfControl m_cachedFocusedControl;

	private dfControl m_lastSelectedBottomRowControl;

	private List<KeyboardBindingMenuOption> m_keyboardBindingLines = new List<KeyboardBindingMenuOption>();

	private Vector2 m_cachedResolution;

	public static int CurrentBindingPlayerTargetIndex;

	private bool m_firstTimeBindingsInitialization = true;

	private float m_justResetToDefaultsWithPrompt;

	private Vector2 m_cachedRelativePositionPrimaryConfirm;

	private Vector2 m_cachedRelativePositionPrimaryCancel;

	private StringTableManager.GungeonSupportedLanguages m_cachedLanguage;

	private bool m_hasCachedPositions;

	public bool IsVisible
	{
		get
		{
			return m_panel.IsVisible;
		}
		set
		{
			if (m_panel.IsVisible == value)
			{
				return;
			}
			if (value)
			{
				EnableHierarchy();
				m_panel.IsVisible = value;
				ShwoopOpen();
				ShowOptionsMenu();
				return;
			}
			ShwoopClosed();
			if (dfGUIManager.GetModalControl() == m_panel)
			{
				dfGUIManager.PopModal();
			}
			else if (dfGUIManager.ModalStackContainsControl(m_panel))
			{
				dfGUIManager.PopModalToControl(m_panel, true);
			}
			else
			{
				Debug.LogError("failure.");
			}
		}
	}

	public dfPanel MainPanel
	{
		get
		{
			return m_panel;
		}
	}

	private void Awake()
	{
		m_panel = GetComponent<dfPanel>();
	}

	public void EnableHierarchy()
	{
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(true);
			dfGUIManager gUIManager = m_panel.GUIManager;
			Vector2 screenSize = gUIManager.GetScreenSize();
			dfControl[] componentsInChildren = m_panel.GetComponentsInChildren<dfControl>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].OnResolutionChanged(m_cachedResolution, screenSize);
			}
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].PerformLayout();
			}
		}
	}

	public void DisableHierarchy()
	{
		if (base.gameObject.activeSelf)
		{
			dfGUIManager gUIManager = m_panel.GUIManager;
			float num = gUIManager.RenderCamera.pixelHeight;
			float num2 = ((!gUIManager.FixedAspect) ? gUIManager.RenderCamera.aspect : 1.77777779f);
			m_cachedResolution = new Vector2(num2 * num, num);
			base.gameObject.SetActive(false);
		}
	}

	public void DoModalKeyBindingDialog(string controlName)
	{
		m_cachedFocusedControl = dfGUIManager.ActiveControl;
		ModalKeyBindingDialog.IsVisible = true;
		m_panel.IsVisible = false;
		ModalKeyBindingDialog.BringToFront();
		dfGUIManager.PushModal(ModalKeyBindingDialog);
		if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
		{
			ModalKeyBindingDialog.transform.Find("TopLabel").GetComponent<dfLabel>().Text = "BIND " + controlName.ToUpperInvariant() + "?";
			ModalKeyBindingDialog.transform.Find("SecondaryLabel").GetComponent<dfLabel>().IsVisible = true;
		}
		else
		{
			ModalKeyBindingDialog.transform.Find("TopLabel").GetComponent<dfLabel>().Text = controlName.ToUpperInvariant();
			ModalKeyBindingDialog.transform.Find("SecondaryLabel").GetComponent<dfLabel>().IsVisible = false;
		}
		dfButton component = ModalKeyBindingDialog.transform.Find("Input Thing").GetComponent<dfButton>();
		component.Text = "___";
		component.PerformLayout();
	}

	public void ToggleKeyBindingDialogState(BindingSource binding)
	{
		dfButton component = ModalKeyBindingDialog.transform.Find("Input Thing").GetComponent<dfButton>();
		if (binding is DeviceBindingSource)
		{
			GameOptions.ControllerSymbology currentSymbology = BraveInput.GetCurrentSymbology(CurrentBindingPlayerTargetIndex);
			component.Text = UIControllerButtonHelper.GetUnifiedControllerButtonTag((binding as DeviceBindingSource).Control, currentSymbology);
		}
		else
		{
			component.Text = binding.Name;
		}
		component.PerformLayout();
		component.Focus();
		component.Click += ClearModalKeyBindingDialog;
		StartCoroutine(HandleTimedKeyBindingClear());
	}

	private IEnumerator HandleTimedKeyBindingClear()
	{
		float elapsed = 0f;
		float duration = 0.25f;
		while (elapsed < duration)
		{
			if (!ModalKeyBindingDialog.IsVisible)
			{
				yield break;
			}
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		ClearModalKeyBindingDialog(null, null);
	}

	public void ClearModalKeyBindingDialog(dfControl source, dfControlEventArgs args)
	{
		if (ModalKeyBindingDialog.IsVisible)
		{
			m_panel.IsVisible = true;
			dfGUIManager.PopModalToControl(m_panel, false);
			ModalKeyBindingDialog.IsVisible = false;
			if (m_cachedFocusedControl != null)
			{
				m_cachedFocusedControl.Focus();
			}
		}
	}

	public void ReinitializeKeyboardBindings()
	{
		for (int i = 0; i < m_keyboardBindingLines.Count; i++)
		{
			m_keyboardBindingLines[i].Initialize();
		}
	}

	public bool ActionIsMultibindable(GungeonActions.GungeonActionType actionType, GungeonActions activeActions)
	{
		switch (actionType)
		{
		case GungeonActions.GungeonActionType.DropGun:
			return true;
		case GungeonActions.GungeonActionType.DropItem:
			return true;
		case GungeonActions.GungeonActionType.SelectUp:
			return true;
		case GungeonActions.GungeonActionType.SelectDown:
			return true;
		case GungeonActions.GungeonActionType.SelectLeft:
			return true;
		case GungeonActions.GungeonActionType.SelectRight:
			return true;
		case GungeonActions.GungeonActionType.MenuInteract:
			return true;
		case GungeonActions.GungeonActionType.Cancel:
			return true;
		case GungeonActions.GungeonActionType.PunchoutDodgeLeft:
			return true;
		case GungeonActions.GungeonActionType.PunchoutDodgeRight:
			return true;
		case GungeonActions.GungeonActionType.PunchoutBlock:
			return true;
		case GungeonActions.GungeonActionType.PunchoutDuck:
			return true;
		case GungeonActions.GungeonActionType.PunchoutPunchLeft:
			return true;
		case GungeonActions.GungeonActionType.PunchoutPunchRight:
			return true;
		case GungeonActions.GungeonActionType.PunchoutSuper:
			return true;
		default:
			return false;
		}
	}

	public void ClearBindingFromAllControls(int targetPlayerIndex, BindingSource bindingSource)
	{
		GungeonActions activeActions = BraveInput.GetInstanceForPlayer(targetPlayerIndex).ActiveActions;
		for (int i = 0; i < m_keyboardBindingLines.Count; i++)
		{
			bool flag = false;
			GungeonActions.GungeonActionType actionType = m_keyboardBindingLines[i].ActionType;
			if (ActionIsMultibindable(actionType, activeActions))
			{
				continue;
			}
			PlayerAction actionFromType = activeActions.GetActionFromType(actionType);
			for (int j = 0; j < actionFromType.Bindings.Count; j++)
			{
				BindingSource bindingSource2 = actionFromType.Bindings[j];
				if (bindingSource2 == bindingSource)
				{
					actionFromType.RemoveBinding(bindingSource2);
					flag = true;
				}
			}
			if (flag)
			{
				actionFromType.ForceUpdateVisibleBindings();
				m_keyboardBindingLines[i].Initialize();
			}
		}
	}

	public void SwitchBindingsMenuMode(bool isController)
	{
		int index = 0;
		BraveOptionsMenuItem component = TabKeyboardBindings.Controls[index].GetComponent<BraveOptionsMenuItem>();
		component.optionType = ((CurrentBindingPlayerTargetIndex != 0) ? BraveOptionsMenuItem.BraveOptionsOptionType.CURRENT_BINDINGS_PRESET_P2 : BraveOptionsMenuItem.BraveOptionsOptionType.CURRENT_BINDINGS_PRESET);
		if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
		{
			component.labelControl.IsLocalized = false;
			component.labelControl.Text = "Binding Preset";
			component.labelControl.PerformLayout();
			component.labelControl.Parent.PerformLayout();
		}
		else
		{
			component.labelControl.IsLocalized = true;
			if (isController)
			{
				if (CurrentBindingPlayerTargetIndex == 1)
				{
					component.labelControl.Text = "#OPTIONS_EDITP2BINDINGS";
				}
				else
				{
					component.labelControl.Text = "#OPTIONS_EDITP1BINDINGS";
				}
			}
			else
			{
				component.labelControl.Text = "#OPTIONS_EDITKEYBOARDBINDINGS";
			}
		}
		component.InitializeFromOptions();
		component.ForceRefreshDisplayLabel();
		for (int i = 0; i < m_keyboardBindingLines.Count; i++)
		{
			m_keyboardBindingLines[i].IsControllerMode = isController;
			m_keyboardBindingLines[i].Initialize();
		}
		if (TabKeyboardBindings != null)
		{
			TabKeyboardBindings.PerformLayout();
		}
	}

	public void FullyReinitializeKeyboardBindings()
	{
		DebugTime.Log("FullyReinitializeKeyboardBindings");
		KeyboardBindingMenuOption componentInChildren = TabKeyboardBindings.GetComponentInChildren<KeyboardBindingMenuOption>();
		dfPanel component = componentInChildren.GetComponent<dfPanel>();
		for (int num = m_keyboardBindingLines.Count - 1; num >= 1; num--)
		{
			KeyboardBindingMenuOption keyboardBindingMenuOption = m_keyboardBindingLines[num];
			dfPanel component2 = keyboardBindingMenuOption.GetComponent<dfPanel>();
			component.RemoveControl(component2);
			KeyboardBindingMenuOption keyboardBindingMenuOption2 = m_keyboardBindingLines[num - 1];
			keyboardBindingMenuOption2.KeyButton.GetComponent<UIKeyControls>().down = keyboardBindingMenuOption.KeyButton.GetComponent<UIKeyControls>().down;
			keyboardBindingMenuOption2.AltKeyButton.GetComponent<UIKeyControls>().down = keyboardBindingMenuOption.AltKeyButton.GetComponent<UIKeyControls>().down;
			UnityEngine.Object.Destroy(component2.gameObject);
		}
		m_keyboardBindingLines.Clear();
		finishedInitialization = true;
		InitializeKeyboardBindingsPanel();
		KeyboardBindingMenuOption keyboardBindingMenuOption3 = m_keyboardBindingLines[m_keyboardBindingLines.Count - 1];
		keyboardBindingMenuOption3.KeyButton.GetComponent<UIKeyControls>().down = PrimaryConfirmButton;
		keyboardBindingMenuOption3.AltKeyButton.GetComponent<UIKeyControls>().down = PrimaryConfirmButton;
		PrimaryCancelButton.GetComponent<UIKeyControls>().up = keyboardBindingMenuOption3.KeyButton;
		PrimaryConfirmButton.GetComponent<UIKeyControls>().up = keyboardBindingMenuOption3.KeyButton;
		PrimaryResetDefaultsButton.GetComponent<UIKeyControls>().up = keyboardBindingMenuOption3.KeyButton;
	}

	private void InitializeKeyboardBindingsPanel()
	{
		KeyboardBindingMenuOption componentInChildren = TabKeyboardBindings.GetComponentInChildren<KeyboardBindingMenuOption>();
		dfPanel component = componentInChildren.GetComponent<dfPanel>();
		componentInChildren.KeyButton.GetComponent<UIKeyControls>().up = TabKeyboardBindings.Controls[0];
		componentInChildren.AltKeyButton.GetComponent<UIKeyControls>().up = TabKeyboardBindings.Controls[0];
		TabKeyboardBindings.Controls[0].GetComponent<BraveOptionsMenuItem>().down = componentInChildren.KeyButton;
		if (m_firstTimeBindingsInitialization)
		{
			componentInChildren.KeyButton.Click += componentInChildren.KeyClicked;
			componentInChildren.AltKeyButton.Click += componentInChildren.AltKeyClicked;
			m_firstTimeBindingsInitialization = false;
		}
		componentInChildren.Initialize();
		m_keyboardBindingLines.Add(componentInChildren);
		KeyboardBindingMenuOption previousMenuOption = componentInChildren;
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.DodgeRoll, "#OPTIONS_DODGEROLL", previousMenuOption);
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.Interact, "#OPTIONS_INTERACT", previousMenuOption);
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.Reload, "#OPTIONS_RELOAD", previousMenuOption);
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.Up, "#OPTIONS_MOVEUP", previousMenuOption);
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.Down, "#OPTIONS_MOVEDOWN", previousMenuOption);
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.Left, "#OPTIONS_MOVELEFT", previousMenuOption);
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.Right, "#OPTIONS_MOVERIGHT", previousMenuOption);
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.AimUp, "#OPTIONS_AIMUP", previousMenuOption);
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.AimDown, "#OPTIONS_AIMDOWN", previousMenuOption);
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.AimLeft, "#OPTIONS_AIMLEFT", previousMenuOption);
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.AimRight, "#OPTIONS_AIMRIGHT", previousMenuOption);
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.UseItem, "#OPTIONS_USEITEM", previousMenuOption);
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.Blank, "#OPTIONS_USEBLANK", previousMenuOption);
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.Map, "#OPTIONS_MAP", previousMenuOption);
		if (true)
		{
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.CycleGunUp, "#OPTIONS_CYCLEGUNUP", previousMenuOption);
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.CycleGunDown, "#OPTIONS_CYCLEGUNDOWN", previousMenuOption);
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.CycleItemUp, "#OPTIONS_CYCLEITEMUP", previousMenuOption);
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.CycleItemDown, "#OPTIONS_CYCLEITEMDOWN", previousMenuOption);
		}
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.GunQuickEquip, "#OPTIONS_GUNMENU", previousMenuOption);
		if (true)
		{
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.DropGun, "#OPTIONS_DROPGUN", previousMenuOption);
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.DropItem, "#OPTIONS_DROPITEM", previousMenuOption);
		}
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.Pause, "#OPTIONS_PAUSE", previousMenuOption);
		previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.EquipmentMenu, "#OPTIONS_INVENTORY", previousMenuOption);
		if (GameManager.Options.allowUnknownControllers)
		{
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.SelectUp, "#OPTIONS_MENUUP", previousMenuOption);
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.SelectDown, "#OPTIONS_MENUDOWN", previousMenuOption);
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.SelectLeft, "#OPTIONS_MENULEFT", previousMenuOption);
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.SelectRight, "#OPTIONS_MENURIGHT", previousMenuOption);
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.MenuInteract, "#OPTIONS_MENUSELECT", previousMenuOption);
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.Cancel, "#OPTIONS_MENUCANCEL", previousMenuOption);
		}
		if (PunchoutController.IsActive)
		{
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.PunchoutDodgeLeft, "#OPTIONS_PUNCHOUT_DODGELEFT", previousMenuOption);
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.PunchoutDodgeRight, "#OPTIONS_PUNCHOUT_DODGERIGHT", previousMenuOption);
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.PunchoutBlock, "#OPTIONS_PUNCHOUT_BLOCK", previousMenuOption);
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.PunchoutDuck, "#OPTIONS_PUNCHOUT_DUCK", previousMenuOption);
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.PunchoutPunchLeft, "#OPTIONS_PUNCHOUT_PUNCHLEFT", previousMenuOption);
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.PunchoutPunchRight, "#OPTIONS_PUNCHOUT_PUNCHRIGHT", previousMenuOption);
			previousMenuOption = AddKeyboardBindingLine(component.Parent, component.gameObject, GungeonActions.GungeonActionType.PunchoutSuper, "#OPTIONS_PUNCHOUT_SUPER", previousMenuOption);
		}
	}

	private KeyboardBindingMenuOption AddKeyboardBindingLine(dfControl parentPanel, GameObject prefabObject, GungeonActions.GungeonActionType actionType, string CommandStringKey, KeyboardBindingMenuOption previousMenuOption, bool nonbindable = false)
	{
		dfControl dfControl2 = parentPanel.AddPrefab(prefabObject);
		dfControl2.transform.localScale = prefabObject.transform.localScale;
		KeyboardBindingMenuOption component = dfControl2.GetComponent<KeyboardBindingMenuOption>();
		component.ActionType = actionType;
		component.CommandLabel.Text = CommandStringKey;
		component.NonBindable = nonbindable;
		component.KeyButton.GetComponent<UIKeyControls>().up = previousMenuOption.KeyButton;
		component.AltKeyButton.GetComponent<UIKeyControls>().up = previousMenuOption.AltKeyButton;
		previousMenuOption.KeyButton.GetComponent<UIKeyControls>().down = component.KeyButton;
		previousMenuOption.AltKeyButton.GetComponent<UIKeyControls>().down = component.AltKeyButton;
		component.KeyButton.Click += component.KeyClicked;
		component.AltKeyButton.Click += component.AltKeyClicked;
		component.Initialize();
		m_keyboardBindingLines.Add(component);
		return component;
	}

	public void RegisterItem(BraveOptionsMenuItem item)
	{
		if (m_menuItems == null)
		{
			m_menuItems = new List<BraveOptionsMenuItem>();
		}
		m_menuItems.Add(item);
	}

	public void ReinitializeFromOptions()
	{
		for (int i = 0; i < m_menuItems.Count; i++)
		{
			m_menuItems[i].InitializeFromOptions();
		}
	}

	private void ShowOptionsMenu()
	{
		dfGUIManager.PushModal(m_panel);
		cloneOptions = GameOptions.CloneOptions(GameManager.Options);
		if (!finishedInitialization)
		{
			finishedInitialization = true;
			InitializeKeyboardBindingsPanel();
		}
	}

	private void Update()
	{
		if (m_panel.IsVisible)
		{
			HandleLanguageSpecificModifications();
			if (m_justResetToDefaultsWithPrompt > 0f)
			{
				m_justResetToDefaultsWithPrompt -= GameManager.INVARIANT_DELTA_TIME;
			}
		}
	}

	private void ResetToDefaultsWithPrompt()
	{
		if (m_justResetToDefaultsWithPrompt > 0f)
		{
			return;
		}
		m_justResetToDefaultsWithPrompt = 0.25f;
		m_cachedFocusedControl = dfGUIManager.ActiveControl;
		m_panel.IsVisible = false;
		GameUIRoot.Instance.DoAreYouSure("#AYS_RESETDEFAULTS");
		StartCoroutine(WaitForAreYouSure(delegate
		{
			m_panel.IsVisible = true;
			if (m_cachedFocusedControl != null)
			{
				m_cachedFocusedControl.Focus();
			}
			ResetToDefaults();
		}, delegate
		{
			m_panel.IsVisible = true;
			if (m_cachedFocusedControl != null)
			{
				m_cachedFocusedControl.Focus();
			}
		}));
	}

	private void ResetToDefaults()
	{
		GameManager.Options.RevertToDefaults();
		BraveInput.ResetBindingsToDefaults();
		ReinitializeKeyboardBindings();
		ReinitializeFromOptions();
	}

	public void CloseAndApplyChangesWithPrompt()
	{
		m_cachedFocusedControl = dfGUIManager.ActiveControl;
		m_panel.IsVisible = false;
		GameUIRoot.Instance.DoAreYouSure("#AYS_SAVEOPTIONS");
		StartCoroutine(WaitForAreYouSure(CloseAndApplyChanges, delegate
		{
			m_panel.IsVisible = true;
			if (m_cachedFocusedControl != null)
			{
				m_cachedFocusedControl.Focus();
			}
		}));
	}

	public void CloseAndMaybeApplyChangesWithPrompt()
	{
		if (cloneOptions != null)
		{
			SaveManager.TargetSaveSlot = null;
			BraveInput.SaveBindingInfoToOptions();
			if (GameOptions.CompareSettings(cloneOptions, GameManager.Options))
			{
				CloseAndRevertChanges();
				return;
			}
			m_cachedFocusedControl = dfGUIManager.ActiveControl;
			m_panel.IsVisible = false;
			GameUIRoot.Instance.DoAreYouSure("#AYS_SAVEOPTIONS");
			StartCoroutine(WaitForAreYouSure(CloseAndApplyChanges, CloseAndRevertChanges));
		}
	}

	private IEnumerator WaitForAreYouSure(Action OnYes, Action OnNo)
	{
		while (!GameUIRoot.Instance.HasSelectedAreYouSureOption())
		{
			yield return null;
		}
		if (GameUIRoot.Instance.GetAreYouSureOption())
		{
			if (OnYes != null)
			{
				OnYes();
			}
		}
		else if (OnNo != null)
		{
			OnNo();
		}
	}

	private void CloseAndApplyChanges()
	{
		cloneOptions = null;
		BraveInput.SaveBindingInfoToOptions();
		GameOptions.Save();
		UpAllLevels();
	}

	private void CloseAndRevertChangesWithPrompt()
	{
		BraveInput.SaveBindingInfoToOptions();
		if (GameOptions.CompareSettings(cloneOptions, GameManager.Options))
		{
			CloseAndRevertChanges();
			return;
		}
		m_cachedFocusedControl = dfGUIManager.ActiveControl;
		m_panel.IsVisible = false;
		GameUIRoot.Instance.DoAreYouSure("#AYS_MADECHANGES", true);
		StartCoroutine(WaitForAreYouSure(CloseAndRevertChanges, CloseAndApplyChanges));
	}

	private void CloseAndRevertChanges()
	{
		if (cloneOptions != null)
		{
			GameManager.Options.CurrentLanguage = cloneOptions.CurrentLanguage;
			GameManager.Options.ApplySettings(cloneOptions);
		}
		else
		{
			Debug.LogError("Clone Options is NULL: this should never happen.");
		}
		cloneOptions = null;
		ReinitializeFromOptions();
		StringTableManager.SetNewLanguage(GameManager.Options.CurrentLanguage);
		GameOptions.Save();
		UpAllLevels();
	}

	private IEnumerator Start()
	{
		PrimaryCancelButton.GotFocus += BottomOptionFocused;
		PrimaryResetDefaultsButton.GotFocus += BottomOptionFocused;
		PrimaryConfirmButton.GotFocus += BottomOptionFocused;
		PrimaryCancelButton.Click += delegate
		{
			CloseAndRevertChangesWithPrompt();
		};
		PrimaryResetDefaultsButton.Click += delegate
		{
			ResetToDefaultsWithPrompt();
		};
		PrimaryConfirmButton.Click += delegate
		{
			CloseAndApplyChanges();
		};
		yield return null;
		yield return null;
		yield return null;
		DisableHierarchy();
	}

	private void BottomOptionFocused(dfControl control, dfFocusEventArgs args)
	{
		m_lastSelectedBottomRowControl = control;
		if (TabAudio.IsVisible)
		{
			TabAudio.Controls[TabAudio.Controls.Count - 1].GetComponent<BraveOptionsMenuItem>().down = m_lastSelectedBottomRowControl;
		}
		else if (TabVideo.IsVisible)
		{
			TabVideo.Controls[TabVideo.Controls.Count - 1].GetComponent<BraveOptionsMenuItem>().down = m_lastSelectedBottomRowControl;
		}
		else if (TabControls.IsVisible)
		{
			TabControls.Controls[TabControls.Controls.Count - 2].GetComponent<BraveOptionsMenuItem>().down = m_lastSelectedBottomRowControl;
		}
		else if (TabGameplay.IsVisible)
		{
			TabGameplay.Controls[TabGameplay.Controls.Count - 1].GetComponent<BraveOptionsMenuItem>().down = m_lastSelectedBottomRowControl;
		}
		else if (TabKeyboardBindings.IsVisible)
		{
			TabKeyboardBindings.Controls[TabKeyboardBindings.Controls.Count - 1].GetComponent<KeyboardBindingMenuOption>().KeyButton.GetComponent<UIKeyControls>().down = m_lastSelectedBottomRowControl;
			TabKeyboardBindings.Controls[TabKeyboardBindings.Controls.Count - 1].GetComponent<KeyboardBindingMenuOption>().AltKeyButton.GetComponent<UIKeyControls>().down = m_lastSelectedBottomRowControl;
		}
	}

	public void UpAllLevels()
	{
		InControlInputAdapter.CurrentlyUsingAllDevices = false;
		if (ModalKeyBindingDialog.IsVisible)
		{
			ClearModalKeyBindingDialog(null, null);
		}
		else
		{
			PreOptionsMenu.ReturnToPreOptionsMenu();
		}
	}

	public void ToggleToHowToPlay()
	{
		TabGameplay.IsVisible = false;
		TabHowToPlay.IsVisible = true;
		TabHowToPlay.Controls[0].Focus();
	}

	public void ToggleToCredits()
	{
		TabGameplay.IsVisible = false;
		TabCredits.IsVisible = true;
		TabCredits.Controls[0].Focus();
	}

	public void ToggleToKeyboardBindingsPanel(bool isController)
	{
		FullyReinitializeKeyboardBindings();
		SwitchBindingsMenuMode(isController);
		TabHowToPlay.IsVisible = false;
		TabCredits.IsVisible = false;
		TabAudio.IsVisible = false;
		TabVideo.IsVisible = false;
		TabGameplay.IsVisible = false;
		TabControls.IsVisible = false;
		TabKeyboardBindings.IsVisible = true;
		int index = 0;
		BraveOptionsMenuItem component = TabKeyboardBindings.Controls[index].GetComponent<BraveOptionsMenuItem>();
		component.optionType = ((CurrentBindingPlayerTargetIndex != 0) ? BraveOptionsMenuItem.BraveOptionsOptionType.CURRENT_BINDINGS_PRESET_P2 : BraveOptionsMenuItem.BraveOptionsOptionType.CURRENT_BINDINGS_PRESET);
		component.InitializeFromOptions();
		KeyboardBindingMenuOption component2 = TabKeyboardBindings.Controls[TabKeyboardBindings.Controls.Count - 1].GetComponent<KeyboardBindingMenuOption>();
		component2.KeyButton.GetComponent<UIKeyControls>().down = m_lastSelectedBottomRowControl ?? PrimaryConfirmButton;
		component2.AltKeyButton.GetComponent<UIKeyControls>().down = m_lastSelectedBottomRowControl ?? PrimaryConfirmButton;
		PrimaryCancelButton.GetComponent<UIKeyControls>().up = component2.KeyButton;
		PrimaryConfirmButton.GetComponent<UIKeyControls>().up = component2.KeyButton;
		PrimaryResetDefaultsButton.GetComponent<UIKeyControls>().up = component2.KeyButton;
		PrimaryCancelButton.GetComponent<UIKeyControls>().down = TabKeyboardBindings.Controls[0];
		PrimaryConfirmButton.GetComponent<UIKeyControls>().down = TabKeyboardBindings.Controls[0];
		PrimaryResetDefaultsButton.GetComponent<UIKeyControls>().down = TabKeyboardBindings.Controls[0];
		TabKeyboardBindings.Controls[0].GetComponent<BraveOptionsMenuItem>().up = PrimaryConfirmButton;
		TabKeyboardBindings.Controls[0].Focus();
		if (PunchoutController.IsActive)
		{
			TabKeyboardBindings.Controls[TabKeyboardBindings.Controls.Count - 7].GetComponent<KeyboardBindingMenuOption>().KeyButton.Focus();
			TabKeyboardBindings.ScrollToBottom();
		}
	}

	public void HandleLanguageSpecificModifications()
	{
		if (!m_hasCachedPositions)
		{
			m_hasCachedPositions = true;
			m_cachedRelativePositionPrimaryConfirm = PrimaryConfirmButton.RelativePosition;
			m_cachedRelativePositionPrimaryCancel = PrimaryCancelButton.RelativePosition;
		}
		if (GameManager.Options.CurrentLanguage != m_cachedLanguage)
		{
			if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.KOREAN || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE)
			{
				PrimaryConfirmButton.RelativePosition = m_cachedRelativePositionPrimaryConfirm;
				PrimaryCancelButton.RelativePosition = m_cachedRelativePositionPrimaryCancel;
			}
			else
			{
				PrimaryConfirmButton.RelativePosition = m_cachedRelativePositionPrimaryConfirm + new Vector2(15f, 0f);
				PrimaryCancelButton.RelativePosition = m_cachedRelativePositionPrimaryCancel + new Vector2(-45f, 0f);
			}
			m_cachedLanguage = GameManager.Options.CurrentLanguage;
		}
	}

	public void ToggleToPanel(dfScrollPanel targetPanel, bool doFocus = false)
	{
		IsVisible = true;
		TabHowToPlay.IsVisible = false;
		TabCredits.IsVisible = false;
		if (TabKeyboardBindings.IsVisible)
		{
			BraveInput.SaveBindingInfoToOptions();
		}
		TabKeyboardBindings.IsVisible = false;
		TabAudio.IsVisible = targetPanel == TabAudio;
		TabVideo.IsVisible = targetPanel == TabVideo;
		TabGameplay.IsVisible = targetPanel == TabGameplay;
		TabControls.IsVisible = targetPanel == TabControls;
		PrimaryCancelButton.GetComponent<UIKeyControls>().down = targetPanel.Controls[0];
		PrimaryConfirmButton.GetComponent<UIKeyControls>().down = targetPanel.Controls[0];
		PrimaryResetDefaultsButton.GetComponent<UIKeyControls>().down = targetPanel.Controls[0];
		targetPanel.Controls[0].Focus();
		BraveOptionsMenuItem component = targetPanel.Controls[0].GetComponent<BraveOptionsMenuItem>();
		if ((bool)component)
		{
			component.up = PrimaryConfirmButton;
		}
		for (int num = targetPanel.Controls.Count - 1; num > 0; num--)
		{
			BraveOptionsMenuItem component2 = targetPanel.Controls[num].GetComponent<BraveOptionsMenuItem>();
			if (component2 != null && component2.GetComponent<dfControl>().IsEnabled)
			{
				component2.down = ((!(m_lastSelectedBottomRowControl != null)) ? PrimaryConfirmButton : m_lastSelectedBottomRowControl);
				PrimaryCancelButton.GetComponent<UIKeyControls>().up = targetPanel.Controls[num];
				PrimaryConfirmButton.GetComponent<UIKeyControls>().up = targetPanel.Controls[num];
				PrimaryResetDefaultsButton.GetComponent<UIKeyControls>().up = targetPanel.Controls[num];
				break;
			}
		}
		if (doFocus)
		{
			targetPanel.Controls[0].Focus();
		}
	}

	public void ShwoopOpen()
	{
		StartCoroutine(HandleShwoop(false));
	}

	private IEnumerator HandleShwoop(bool reverse)
	{
		m_justResetToDefaultsWithPrompt = 0f;
		float timer = 0.1f;
		float elapsed = 0f;
		Vector3 smallScale = new Vector3(0.01f, 0.01f, 1f);
		Vector3 bigScale = Vector3.one;
		PauseMenuController pmc = GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>();
		while (elapsed < timer)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t = elapsed / timer;
			AnimationCurve targetCurve = ((!reverse) ? pmc.ShwoopInCurve : pmc.ShwoopOutCurve);
			m_panel.transform.localScale = smallScale + bigScale * Mathf.Clamp01(targetCurve.Evaluate(t));
			m_panel.Opacity = Mathf.Lerp(0f, 1f, (!reverse) ? ((t - 0.5f) * 2f) : (1f - t * 2f));
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
			DisableHierarchy();
		}
	}

	public void ShwoopClosed()
	{
		StartCoroutine(HandleShwoop(true));
	}
}
