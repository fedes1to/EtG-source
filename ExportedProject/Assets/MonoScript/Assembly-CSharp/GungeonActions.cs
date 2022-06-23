using InControl;
using UnityEngine;

public class GungeonActions : PlayerActionSet
{
	public enum GungeonActionType
	{
		Left,
		Right,
		Up,
		Down,
		AimLeft,
		AimRight,
		AimUp,
		AimDown,
		Shoot,
		DodgeRoll,
		Interact,
		Reload,
		UseItem,
		Map,
		CycleGunUp,
		CycleGunDown,
		CycleItemUp,
		CycleItemDown,
		Keybullet,
		Pause,
		SelectLeft,
		SelectRight,
		SelectUp,
		SelectDown,
		Cancel,
		DropGun,
		EquipmentMenu,
		Blank,
		GunQuickEquip,
		MenuInteract,
		DropItem,
		MinimapZoomIn,
		MinimapZoomOut,
		SwapDualGuns,
		PunchoutDodgeLeft,
		PunchoutDodgeRight,
		PunchoutBlock,
		PunchoutDuck,
		PunchoutPunchLeft,
		PunchoutPunchRight,
		PunchoutSuper
	}

	public PlayerAction Left;

	public PlayerAction Right;

	public PlayerAction Up;

	public PlayerAction Down;

	public PlayerTwoAxisAction Move;

	public PlayerAction AimLeft;

	public PlayerAction AimRight;

	public PlayerAction AimUp;

	public PlayerAction AimDown;

	public PlayerTwoAxisAction Aim;

	public PlayerAction SelectLeft;

	public PlayerAction SelectRight;

	public PlayerAction SelectUp;

	public PlayerAction SelectDown;

	public PlayerTwoAxisAction SelectAxis;

	public PlayerAction ShootAction;

	public PlayerAction DodgeRollAction;

	public PlayerAction InteractAction;

	public PlayerAction ReloadAction;

	public PlayerAction UseItemAction;

	public PlayerAction MapAction;

	public PlayerAction GunUpAction;

	public PlayerAction GunDownAction;

	public PlayerAction ItemUpAction;

	public PlayerAction ItemDownAction;

	public PlayerAction KeybulletAction;

	public PlayerAction PauseAction;

	public PlayerAction CancelAction;

	public PlayerAction MenuSelectAction;

	public PlayerAction EquipmentMenuAction;

	public PlayerAction BlankAction;

	public PlayerAction DropGunAction;

	public PlayerAction DropItemAction;

	public PlayerAction GunQuickEquipAction;

	public PlayerAction MinimapZoomInAction;

	public PlayerAction MinimapZoomOutAction;

	public PlayerAction SwapDualGunsAction;

	public PlayerAction PunchoutDodgeLeft;

	public PlayerAction PunchoutDodgeRight;

	public PlayerAction PunchoutBlock;

	public PlayerAction PunchoutDuck;

	public PlayerAction PunchoutPunchLeft;

	public PlayerAction PunchoutPunchRight;

	public PlayerAction PunchoutSuper;

	private bool m_highAccuraceAimMode;

	public static InputControlType LocalizedMenuSelectAction
	{
		get
		{
			if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE)
			{
				return InputControlType.Action2;
			}
			return InputControlType.Action1;
		}
	}

	public static InputControlType LocalizedMenuCancelAction
	{
		get
		{
			if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.JAPANESE)
			{
				return InputControlType.Action1;
			}
			return InputControlType.Action2;
		}
	}

	public bool HighAccuracyAimMode
	{
		get
		{
			return m_highAccuraceAimMode;
		}
		set
		{
			if (value != m_highAccuraceAimMode)
			{
				SetHighAccuracy(AimLeft, value);
				SetHighAccuracy(AimRight, value);
				SetHighAccuracy(AimUp, value);
				SetHighAccuracy(AimDown, value);
				m_highAccuraceAimMode = value;
			}
		}
	}

	public GungeonActions()
	{
		Left = CreatePlayerAction("Move Left");
		Right = CreatePlayerAction("Move Right");
		Up = CreatePlayerAction("Move Up");
		Down = CreatePlayerAction("Move Down");
		Move = CreateTwoAxisPlayerAction(Left, Right, Down, Up);
		AimLeft = CreatePlayerAction("Aim Left");
		AimRight = CreatePlayerAction("Aim Right");
		AimUp = CreatePlayerAction("Aim Up");
		AimDown = CreatePlayerAction("Aim Down");
		Aim = CreateTwoAxisPlayerAction(AimLeft, AimRight, AimDown, AimUp);
		SelectLeft = CreatePlayerAction("Select Left");
		SelectRight = CreatePlayerAction("Select Right");
		SelectUp = CreatePlayerAction("Select Up");
		SelectDown = CreatePlayerAction("Select Down");
		SelectUp.StateThreshold = 0.5f;
		SelectDown.StateThreshold = 0.5f;
		SelectLeft.StateThreshold = 0.5f;
		SelectRight.StateThreshold = 0.5f;
		SelectAxis = CreateTwoAxisPlayerAction(SelectLeft, SelectRight, SelectDown, SelectUp);
		SelectAxis.StateThreshold = 0.9f;
		ShootAction = CreatePlayerAction("Shoot");
		DodgeRollAction = CreatePlayerAction("Dodge Roll");
		InteractAction = CreatePlayerAction("Interact");
		CancelAction = CreatePlayerAction("Cancel");
		ReloadAction = CreatePlayerAction("Reload");
		UseItemAction = CreatePlayerAction("Use Item");
		MapAction = CreatePlayerAction("Map");
		GunUpAction = CreatePlayerAction("Cycle Gun Up");
		GunDownAction = CreatePlayerAction("Cycle Gun Down");
		ItemUpAction = CreatePlayerAction("Cycle Item Up");
		ItemDownAction = CreatePlayerAction("Cycle Item Down");
		KeybulletAction = CreatePlayerAction("Keybullet");
		PauseAction = CreatePlayerAction("Pause");
		DropGunAction = CreatePlayerAction("Drop Gun");
		DropItemAction = CreatePlayerAction("Drop Item");
		EquipmentMenuAction = CreatePlayerAction("Equipment Menu");
		BlankAction = CreatePlayerAction("Blank");
		GunQuickEquipAction = CreatePlayerAction("Gun Quick Equip");
		SwapDualGunsAction = CreatePlayerAction("Swap Dual Guns");
		MenuSelectAction = CreatePlayerAction("Menu Select");
		MinimapZoomInAction = CreatePlayerAction("Minimap Zoom In");
		MinimapZoomOutAction = CreatePlayerAction("Minimap Zoom Out");
		PunchoutDodgeLeft = CreatePlayerAction("Dodge Left");
		PunchoutDodgeRight = CreatePlayerAction("Dodge Right");
		PunchoutBlock = CreatePlayerAction("Block");
		PunchoutDuck = CreatePlayerAction("Duck");
		PunchoutPunchLeft = CreatePlayerAction("Punch Left");
		PunchoutPunchRight = CreatePlayerAction("Punch Right");
		PunchoutSuper = CreatePlayerAction("Super");
		PunchoutDodgeLeft.StateThreshold = 0.3f;
		PunchoutDodgeRight.StateThreshold = 0.3f;
		PunchoutBlock.StateThreshold = 0.3f;
		PunchoutDuck.StateThreshold = 0.3f;
	}

	public PlayerAction GetActionFromType(GungeonActionType type)
	{
		switch (type)
		{
		case GungeonActionType.Left:
			return Left;
		case GungeonActionType.Right:
			return Right;
		case GungeonActionType.Up:
			return Up;
		case GungeonActionType.Down:
			return Down;
		case GungeonActionType.AimLeft:
			return AimLeft;
		case GungeonActionType.AimRight:
			return AimRight;
		case GungeonActionType.AimUp:
			return AimUp;
		case GungeonActionType.AimDown:
			return AimDown;
		case GungeonActionType.Shoot:
			return ShootAction;
		case GungeonActionType.DodgeRoll:
			return DodgeRollAction;
		case GungeonActionType.Interact:
			return InteractAction;
		case GungeonActionType.Reload:
			return ReloadAction;
		case GungeonActionType.UseItem:
			return UseItemAction;
		case GungeonActionType.Map:
			return MapAction;
		case GungeonActionType.CycleGunUp:
			return GunUpAction;
		case GungeonActionType.CycleGunDown:
			return GunDownAction;
		case GungeonActionType.CycleItemDown:
			return ItemDownAction;
		case GungeonActionType.CycleItemUp:
			return ItemUpAction;
		case GungeonActionType.Keybullet:
			return KeybulletAction;
		case GungeonActionType.Pause:
			return PauseAction;
		case GungeonActionType.SelectDown:
			return SelectDown;
		case GungeonActionType.SelectLeft:
			return SelectLeft;
		case GungeonActionType.SelectRight:
			return SelectRight;
		case GungeonActionType.SelectUp:
			return SelectUp;
		case GungeonActionType.Cancel:
			return CancelAction;
		case GungeonActionType.DropGun:
			return DropGunAction;
		case GungeonActionType.DropItem:
			return DropItemAction;
		case GungeonActionType.EquipmentMenu:
			return EquipmentMenuAction;
		case GungeonActionType.Blank:
			return BlankAction;
		case GungeonActionType.GunQuickEquip:
			return GunQuickEquipAction;
		case GungeonActionType.MenuInteract:
			return MenuSelectAction;
		case GungeonActionType.MinimapZoomIn:
			return MinimapZoomInAction;
		case GungeonActionType.MinimapZoomOut:
			return MinimapZoomOutAction;
		case GungeonActionType.SwapDualGuns:
			return SwapDualGunsAction;
		case GungeonActionType.PunchoutDodgeLeft:
			return PunchoutDodgeLeft;
		case GungeonActionType.PunchoutDodgeRight:
			return PunchoutDodgeRight;
		case GungeonActionType.PunchoutBlock:
			return PunchoutBlock;
		case GungeonActionType.PunchoutDuck:
			return PunchoutDuck;
		case GungeonActionType.PunchoutPunchLeft:
			return PunchoutPunchLeft;
		case GungeonActionType.PunchoutPunchRight:
			return PunchoutPunchRight;
		case GungeonActionType.PunchoutSuper:
			return PunchoutSuper;
		default:
			return null;
		}
	}

	public bool IntroSkipActionPressed()
	{
		return MenuSelectAction.WasPressed || PauseAction.WasPressed || CancelAction.WasPressed;
	}

	public bool AnyActionPressed()
	{
		for (int i = 0; i < base.Actions.Count; i++)
		{
			if (base.Actions[i].WasPressed)
			{
				return true;
			}
		}
		return false;
	}

	public void IgnoreBindingsOfType(BindingSourceType sourceType)
	{
		for (int i = 0; i < base.Actions.Count; i++)
		{
			base.Actions[i].IgnoreBindingsOfType(sourceType);
		}
	}

	public void ClearBindingsOfType(BindingSourceType sourceType)
	{
		for (int i = 0; i < base.Actions.Count; i++)
		{
			base.Actions[i].ClearBindingsOfType(sourceType);
		}
	}

	public bool CheckBothSticksButton()
	{
		if (base.Device == null)
		{
			return false;
		}
		if (base.Device.LeftStickButton.WasPressed && base.Device.RightStickButton.IsPressed)
		{
			return true;
		}
		if (base.Device.LeftStickButton.IsPressed && base.Device.RightStickButton.WasPressed)
		{
			return true;
		}
		if (base.Device.LeftStickButton.WasPressed && base.Device.RightStickButton.WasPressed)
		{
			return true;
		}
		return false;
	}

	public bool CheckAllFaceButtonsPressed()
	{
		int num = 0;
		if (base.Device.Action1.IsPressed)
		{
			num++;
		}
		if (base.Device.Action2.IsPressed)
		{
			num++;
		}
		if (base.Device.Action3.IsPressed)
		{
			num++;
		}
		if (base.Device.Action4.IsPressed)
		{
			num++;
		}
		return num >= 3;
	}

	public void PostProcessAdditionalBlankControls(int playerNum)
	{
		switch ((playerNum != 0) ? GameManager.Options.additionalBlankControlTwo : GameManager.Options.additionalBlankControl)
		{
		case GameOptions.ControllerBlankControl.CIRCLE:
			DodgeRollAction.RemoveBindingOfType(InputControlType.Action2);
			if (BlankAction.Bindings.Count < 2)
			{
				BlankAction.AddBinding(new DeviceBindingSource(InputControlType.Action2));
			}
			switch (playerNum)
			{
			case 0:
				GameManager.Options.additionalBlankControl = GameOptions.ControllerBlankControl.NONE;
				GameManager.Options.CurrentControlPreset = GameOptions.ControlPreset.CUSTOM;
				break;
			case 1:
				GameManager.Options.additionalBlankControlTwo = GameOptions.ControllerBlankControl.NONE;
				GameManager.Options.CurrentControlPresetP2 = GameOptions.ControlPreset.CUSTOM;
				break;
			}
			break;
		case GameOptions.ControllerBlankControl.L1:
			DodgeRollAction.RemoveBindingOfType(InputControlType.LeftBumper);
			if (BlankAction.Bindings.Count < 2)
			{
				BlankAction.AddBinding(new DeviceBindingSource(InputControlType.LeftBumper));
			}
			switch (playerNum)
			{
			case 0:
				GameManager.Options.additionalBlankControl = GameOptions.ControllerBlankControl.NONE;
				GameManager.Options.CurrentControlPreset = GameOptions.ControlPreset.CUSTOM;
				break;
			case 1:
				GameManager.Options.additionalBlankControlTwo = GameOptions.ControllerBlankControl.NONE;
				GameManager.Options.CurrentControlPresetP2 = GameOptions.ControlPreset.CUSTOM;
				break;
			}
			break;
		}
	}

	public void ReinitializeDefaults()
	{
		for (int i = 0; i < base.Actions.Count; i++)
		{
			base.Actions[i].ResetBindings();
		}
	}

	public void InitializeSwappedTriggersPreset()
	{
		for (int i = 0; i < base.Actions.Count; i++)
		{
			base.Actions[i].ResetBindings();
		}
		ShootAction.ClearBindings();
		ShootAction.AddBinding(new DeviceBindingSource(InputControlType.RightTrigger));
		DodgeRollAction.ClearBindings();
		DodgeRollAction.AddBinding(new DeviceBindingSource(InputControlType.LeftTrigger));
		DodgeRollAction.AddBinding(new DeviceBindingSource(InputControlType.Action2));
		MapAction.ClearBindings();
		MapAction.AddBinding(new DeviceBindingSource(InputControlType.LeftBumper));
		MapAction.AddBinding(new KeyBindingSource(Key.Tab));
		UseItemAction.ClearBindings();
		UseItemAction.AddBinding(new DeviceBindingSource(InputControlType.RightBumper));
		UseItemAction.AddBinding(new KeyBindingSource(Key.Space));
		ShootAction.AddBinding(new MouseBindingSource(Mouse.LeftButton));
		DodgeRollAction.AddBinding(new MouseBindingSource(Mouse.RightButton));
	}

	public void ReinitializeMenuDefaults()
	{
		CancelAction.ResetBindings();
		MenuSelectAction.ResetBindings();
		CancelAction.ClearBindings();
		MenuSelectAction.ClearBindings();
		CancelAction.AddDefaultBinding(LocalizedMenuCancelAction);
		CancelAction.AddDefaultBinding(Key.Escape);
		MenuSelectAction.AddDefaultBinding(LocalizedMenuSelectAction);
		MenuSelectAction.AddDefaultBinding(Key.Return);
	}

	public void InitializeDefaults()
	{
		Left.AddDefaultBinding(InputControlType.LeftStickLeft);
		Left.AddDefaultBinding(Key.A);
		Right.AddDefaultBinding(InputControlType.LeftStickRight);
		Right.AddDefaultBinding(Key.D);
		Up.AddDefaultBinding(InputControlType.LeftStickUp);
		Up.AddDefaultBinding(Key.W);
		Down.AddDefaultBinding(InputControlType.LeftStickDown);
		Down.AddDefaultBinding(Key.S);
		AimLeft.AddDefaultBinding(InputControlType.RightStickLeft);
		AimRight.AddDefaultBinding(InputControlType.RightStickRight);
		AimUp.AddDefaultBinding(InputControlType.RightStickUp);
		AimDown.AddDefaultBinding(InputControlType.RightStickDown);
		SelectLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
		SelectLeft.AddDefaultBinding(InputControlType.DPadLeft);
		SelectLeft.AddDefaultBinding(Key.A);
		SelectLeft.AddDefaultBinding(Key.LeftArrow);
		SelectRight.AddDefaultBinding(InputControlType.LeftStickRight);
		SelectRight.AddDefaultBinding(InputControlType.DPadRight);
		SelectRight.AddDefaultBinding(Key.D);
		SelectRight.AddDefaultBinding(Key.RightArrow);
		SelectUp.AddDefaultBinding(InputControlType.LeftStickUp);
		SelectUp.AddDefaultBinding(InputControlType.DPadUp);
		SelectUp.AddDefaultBinding(Key.W);
		SelectUp.AddDefaultBinding(Key.UpArrow);
		SelectDown.AddDefaultBinding(InputControlType.LeftStickDown);
		SelectDown.AddDefaultBinding(InputControlType.DPadDown);
		SelectDown.AddDefaultBinding(Key.S);
		SelectDown.AddDefaultBinding(Key.DownArrow);
		ShootAction.AddDefaultBinding(InputControlType.RightBumper);
		DodgeRollAction.AddDefaultBinding(InputControlType.LeftBumper);
		DodgeRollAction.AddDefaultBinding(InputControlType.Action2);
		InteractAction.AddDefaultBinding(InputControlType.Action1);
		InteractAction.AddDefaultBinding(Key.E);
		ReloadAction.AddDefaultBinding(InputControlType.Action3);
		ReloadAction.AddDefaultBinding(Key.R);
		UseItemAction.AddDefaultBinding(InputControlType.RightTrigger);
		UseItemAction.AddDefaultBinding(Key.Space);
		MapAction.AddDefaultBinding(InputControlType.LeftTrigger);
		MapAction.AddDefaultBinding(Key.Tab);
		GunUpAction.AddDefaultBinding(InputControlType.DPadLeft);
		GunDownAction.AddDefaultBinding(InputControlType.DPadRight);
		DropGunAction.AddDefaultBinding(Key.F);
		DropGunAction.AddDefaultBinding(InputControlType.DPadDown);
		DropItemAction.AddDefaultBinding(Key.G);
		DropItemAction.AddDefaultBinding(InputControlType.DPadUp);
		ItemUpAction.AddDefaultBinding(InputControlType.DPadUp);
		ItemUpAction.AddDefaultBinding(Key.LeftShift);
		GunQuickEquipAction.AddDefaultBinding(InputControlType.Action4);
		GunQuickEquipAction.AddDefaultBinding(Key.LeftControl);
		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
		{
			GunQuickEquipAction.AddDefaultBinding(Key.LeftCommand);
		}
		SwapDualGunsAction.AddDefaultBinding(Mouse.MiddleButton);
		PauseAction.AddDefaultBinding(InputControlType.Start);
		PauseAction.AddDefaultBinding(InputControlType.Select);
		PauseAction.AddDefaultBinding(InputControlType.Options);
		PauseAction.AddDefaultBinding(Key.Escape);
		EquipmentMenuAction.AddDefaultBinding(InputControlType.TouchPadButton);
		EquipmentMenuAction.AddDefaultBinding(InputControlType.Back);
		EquipmentMenuAction.AddDefaultBinding(Key.I);
		BlankAction.AddDefaultBinding(Key.Q);
		ReinitializeMenuDefaults();
		MinimapZoomInAction.AddDefaultBinding(Key.Equals);
		MinimapZoomInAction.AddDefaultBinding(Key.PadPlus);
		MinimapZoomOutAction.AddDefaultBinding(Key.Minus);
		MinimapZoomOutAction.AddDefaultBinding(Key.PadMinus);
		GunUpAction.AddDefaultBinding(Mouse.PositiveScrollWheel);
		GunDownAction.AddDefaultBinding(Mouse.NegativeScrollWheel);
		DodgeRollAction.AddDefaultBinding(Mouse.RightButton);
		ShootAction.AddDefaultBinding(Mouse.LeftButton);
		PunchoutDodgeLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
		PunchoutDodgeLeft.AddDefaultBinding(InputControlType.DPadLeft);
		PunchoutDodgeLeft.AddDefaultBinding(Key.A);
		PunchoutDodgeRight.AddDefaultBinding(InputControlType.LeftStickRight);
		PunchoutDodgeRight.AddDefaultBinding(InputControlType.DPadRight);
		PunchoutDodgeRight.AddDefaultBinding(Key.D);
		PunchoutBlock.AddDefaultBinding(InputControlType.LeftStickUp);
		PunchoutBlock.AddDefaultBinding(InputControlType.DPadUp);
		PunchoutBlock.AddDefaultBinding(Key.W);
		PunchoutDuck.AddDefaultBinding(InputControlType.LeftStickDown);
		PunchoutDuck.AddDefaultBinding(InputControlType.DPadDown);
		PunchoutDuck.AddDefaultBinding(Key.S);
		PunchoutPunchLeft.AddDefaultBinding(InputControlType.Action1);
		PunchoutPunchLeft.AddDefaultBinding(InputControlType.LeftBumper);
		PunchoutPunchRight.AddDefaultBinding(InputControlType.Action2);
		PunchoutPunchRight.AddDefaultBinding(InputControlType.RightBumper);
		PunchoutSuper.AddDefaultBinding(InputControlType.Action3);
		PunchoutSuper.AddDefaultBinding(Key.Space);
		PunchoutPunchLeft.AddDefaultBinding(Mouse.LeftButton);
		PunchoutPunchRight.AddDefaultBinding(Mouse.RightButton);
	}

	private void SetHighAccuracy(PlayerAction action, bool value)
	{
		foreach (BindingSource binding in action.Bindings)
		{
			DeviceBindingSource deviceBindingSource = binding as DeviceBindingSource;
			if (deviceBindingSource != null)
			{
				deviceBindingSource.ForceRawInput = value;
			}
		}
	}
}
