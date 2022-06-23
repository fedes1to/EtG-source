using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using InControl;
using UnityEngine;
using UnityEngine.Analytics;

public class BraveOptionsMenuItem : MonoBehaviour
{
	public enum BraveOptionsMenuItemType
	{
		LeftRightArrow,
		LeftRightArrowInfo,
		Fillbar,
		Checkbox,
		Button
	}

	public enum BraveOptionsOptionType
	{
		NONE,
		MUSIC_VOLUME,
		SOUND_VOLUME,
		UI_VOLUME,
		SPEAKER_TYPE,
		VISUAL_PRESET,
		RESOLUTION,
		SCALING_MODE,
		FULLSCREEN,
		MONITOR_SELECT,
		VSYNC,
		LIGHTING_QUALITY,
		SHADER_QUALITY,
		DEBRIS_QUANTITY,
		SCREEN_SHAKE_AMOUNT,
		STICKY_FRICTION_AMOUNT,
		TEXT_SPEED,
		CONTROLLER_AIM_ASSIST_AMOUNT,
		BEASTMODE,
		EDIT_KEYBOARD_BINDINGS,
		PLAYER_ONE_CONTROL_PORT,
		PLAYER_ONE_CONTROLLER_SYMBOLOGY,
		PLAYER_ONE_CONTROLLER_BINDINGS,
		PLAYER_TWO_CONTROL_PORT,
		PLAYER_TWO_CONTROLLER_SYMBOLOGY,
		PLAYER_TWO_CONTROLLER_BINDINGS,
		VIEW_CREDITS,
		MINIMAP_STYLE,
		COOP_SCREEN_SHAKE_AMOUNT,
		CONTROLLER_AIM_LOOK,
		GAMMA,
		REALTIME_REFLECTIONS,
		QUICKSELECT,
		HIDE_EMPTY_GUNS,
		HOW_TO_PLAY,
		LANGUAGE,
		SPEEDRUN,
		QUICKSTART_CHARACTER,
		ADDITIONAL_BLANK_CONTROL,
		ADDITIONAL_BLANK_CONTROL_TWO,
		CURRENT_BINDINGS_PRESET,
		CURRENT_BINDINGS_PRESET_P2,
		SAVE_SLOT,
		RESET_SAVE_SLOT,
		RUMBLE,
		CURSOR_VARIATION,
		ADDITIONAL_BLANK_CONTROL_PS4,
		PLAYER_ONE_CONTROLLER_CURSOR,
		PLAYER_TWO_CONTROLLER_CURSOR,
		ALLOWED_CONTROLLER_TYPES,
		ALLOW_UNKNOWN_CONTROLLERS,
		SMALL_UI,
		BOTH_CONTROLLER_CURSOR,
		DISPLAY_SAFE_AREA,
		GAMEPLAY_SPEED,
		OUT_OF_COMBAT_SPEED_INCREASE,
		CONTROLLER_BEAM_AIM_ASSIST,
		SWITCH_PERFORMANCE_MODE,
		SWITCH_REASSIGN_CONTROLLERS,
		LOOT_PROFILE,
		AUTOAIM,
		VIEW_PRIVACY
	}

	public class WindowsResolutionManager
	{
		public enum DisplayModes
		{
			Fullscreen,
			Borderless,
			Windowed
		}

		public struct RECT
		{
			public int Left;

			public int Top;

			public int Right;

			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
		public class MONITORINFOEX
		{
			public int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));

			public RECT rcMonitor = default(RECT);

			public RECT rcWork = default(RECT);

			public int dwFlags;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public char[] szDevice = new char[32];
		}

		internal static class Flags
		{
			public static void Set<T>(ref T mask, T flag) where T : struct
			{
				int num = (int)(object)mask;
				int num2 = (int)(object)flag;
				mask = (T)(object)(num | num2);
			}

			public static void Unset<T>(ref T mask, T flag) where T : struct
			{
				int num = (int)(object)mask;
				int num2 = (int)(object)flag;
				mask = (T)(object)(num & ~num2);
			}

			public static void Toggle<T>(ref T mask, T flag) where T : struct
			{
				if (Contains(mask, flag))
				{
					Unset(ref mask, flag);
				}
				else
				{
					Set(ref mask, flag);
				}
			}

			public static bool Contains<T>(T mask, T flag) where T : struct
			{
				return Contains((int)(object)mask, (int)(object)flag);
			}

			public static bool Contains(int mask, int flag)
			{
				return (mask & flag) != 0;
			}
		}

		private string _title;

		private int _borderWidth;

		private int _captionHeight;

		private const int WS_BORDER = 8388608;

		private const int WS_CAPTION = 12582912;

		private const int WS_CHILD = 1073741824;

		private const int WS_CHILDWINDOW = 1073741824;

		private const int WS_CLIPCHILDREN = 33554432;

		private const int WS_CLIPSIBLINGS = 67108864;

		private const int WS_DISABLED = 134217728;

		private const int WS_DLGFRAME = 4194304;

		private const int WS_GROUP = 131072;

		private const int WS_HSCROLL = 1048576;

		private const int WS_ICONIC = 536870912;

		private const int WS_MAXIMIZE = 16777216;

		private const int WS_MAXIMIZEBOX = 65536;

		private const int WS_MINIMIZE = 536870912;

		private const int WS_MINIMIZEBOX = 131072;

		private const int WS_OVERLAPPED = 0;

		private const int WS_OVERLAPPEDWINDOW = 13565952;

		private const int WS_POPUP = int.MinValue;

		private const int WS_POPUPWINDOW = -2138570752;

		private const int WS_SIZEBOX = 262144;

		private const int WS_SYSMENU = 524288;

		private const int WS_TABSTOP = 65536;

		private const int WS_THICKFRAME = 262144;

		private const int WS_TILED = 0;

		private const int WS_TILEDWINDOW = 13565952;

		private const int WS_VISIBLE = 268435456;

		private const int WS_VSCROLL = 2097152;

		private const int WS_EX_DLGMODALFRAME = 1;

		private const int WS_EX_CLIENTEDGE = 512;

		private const int WS_EX_STATICEDGE = 131072;

		private const int SWP_FRAMECHANGED = 32;

		private const int SWP_NOMOVE = 2;

		private const int SWP_NOSIZE = 1;

		private const int SWP_NOZORDER = 4;

		private const int SWP_NOOWNERZORDER = 512;

		private const int SWP_SHOWWINDOW = 64;

		private const int SWP_NOSENDCHANGING = 1024;

		private const int GWL_STYLE = -16;

		private const int GWL_EXSTYLE = -20;

		private IntPtr Window
		{
			get
			{
				return FindWindowByCaption(IntPtr.Zero, _title);
			}
		}

		private IntPtr Desktop
		{
			get
			{
				return GetDesktopWindow();
			}
		}

		public WindowsResolutionManager(string title)
		{
			_title = title;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong", SetLastError = true)]
		public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLong", SetLastError = true)]
		public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
		public static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr GetDesktopWindow();

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int uFlags);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr MonitorFromWindow(IntPtr hWnd, uint flags);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool GetClientRect(IntPtr hWnd, out RECT rect);

		[DllImport("User32.dll", CharSet = CharSet.Auto)]
		public static extern bool GetMonitorInfo(IntPtr hmonitor, [In][Out] MONITORINFOEX info);

		public Position? GetWindowPosition()
		{
			RECT rect;
			if (!GetWindowRect(Window, out rect))
			{
				return null;
			}
			return new Position(rect.Left, rect.Top);
		}

		public Position? GetCenteredPosition(Resolution resolution, DisplayModes displayMode)
		{
			RECT rect;
			if (!GetWindowRect(Desktop, out rect))
			{
				return null;
			}
			int num = rect.Right - rect.Left;
			int num2 = rect.Bottom - rect.Top;
			int num3 = 0;
			int num4 = 0;
			IntPtr hmonitor = MonitorFromWindow(Window, 2u);
			MONITORINFOEX mONITORINFOEX = new MONITORINFOEX();
			if (GetMonitorInfo(hmonitor, mONITORINFOEX))
			{
				num = mONITORINFOEX.rcMonitor.Right - mONITORINFOEX.rcMonitor.Left;
				num2 = mONITORINFOEX.rcMonitor.Bottom - mONITORINFOEX.rcMonitor.Top;
				num3 = mONITORINFOEX.rcMonitor.Left;
				num4 = mONITORINFOEX.rcMonitor.Top;
			}
			int num5;
			int num6;
			if (displayMode == DisplayModes.Windowed)
			{
				num5 = (num - (resolution.width + _borderWidth * 2)) / 2;
				num6 = (num2 - (resolution.height + _borderWidth * 2 + _captionHeight)) / 2;
			}
			else
			{
				num5 = (num - resolution.width) / 2;
				num6 = (num2 - resolution.height) / 2;
			}
			num5 += num3;
			num6 += num4;
			return new Position(num5, num6);
		}

		private void UpdateWindowRect(IntPtr window, int x, int y, int width, int height)
		{
			SetWindowPos(window, -2, x, y, width, height, 32);
		}

		private bool UpdateDecorationSize(IntPtr window)
		{
			RECT rect;
			if (!GetWindowRect(Window, out rect))
			{
				return false;
			}
			RECT rect2;
			if (!GetClientRect(Window, out rect2))
			{
				return false;
			}
			int num = rect.Right - rect.Left - (rect2.Right - rect2.Left);
			int num2 = rect.Bottom - rect.Top - (rect2.Bottom - rect2.Top);
			_borderWidth = num / 2;
			_captionHeight = num2 - _borderWidth * 2;
			return true;
		}

		public bool TrySetDisplay(DisplayModes targetDisplayMode, Resolution targetResolution, bool setPosition, Position? position)
		{
			int mask = (int)GetWindowLongPtr(Window, -16);
			if (targetDisplayMode == DisplayModes.Windowed && Flags.Contains(mask, 8388608))
			{
				position = GetWindowPosition();
			}
			switch (targetDisplayMode)
			{
			case DisplayModes.Fullscreen:
				return true;
			case DisplayModes.Borderless:
				Flags.Unset(ref mask, 8388608);
				Flags.Unset(ref mask, 262144);
				Flags.Unset(ref mask, 12582912);
				SetWindowLongPtr(Window, -16, mask);
				if (!setPosition || !position.HasValue)
				{
					position = GetCenteredPosition(targetResolution, targetDisplayMode);
				}
				UpdateWindowRect(Window, position.Value.X, position.Value.Y, targetResolution.width, targetResolution.height);
				SetWindowLongPtr(Window, -16, mask);
				SetWindowLongPtr(Window, -16, mask);
				return true;
			case DisplayModes.Windowed:
			{
				Flags.Set(ref mask, 8388608);
				Flags.Set(ref mask, 262144);
				Flags.Set(ref mask, 12582912);
				SetWindowLongPtr(Window, -16, mask);
				UpdateDecorationSize(Window);
				if (!position.HasValue)
				{
					position = GetCenteredPosition(targetResolution, targetDisplayMode);
				}
				int width = targetResolution.width + _borderWidth * 2;
				int height = targetResolution.height + _captionHeight + _borderWidth * 2;
				UpdateWindowRect(Window, position.Value.X, position.Value.Y, width, height);
				return true;
			}
			default:
				return false;
			}
		}
	}

	public BraveOptionsOptionType optionType;

	[Header("Control Options")]
	public BraveOptionsMenuItemType itemType;

	public dfLabel labelControl;

	[Space(5f)]
	public dfLabel selectedLabelControl;

	public dfLabel infoControl;

	public dfProgressBar fillbarControl;

	public dfButton buttonControl;

	public dfControl checkboxChecked;

	public dfControl checkboxUnchecked;

	public string[] labelOptions;

	public string[] infoOptions;

	[Header("UI Key Controls")]
	public dfControl up;

	public dfControl down;

	public dfControl left;

	public dfControl right;

	public bool selectOnAction;

	public Action<dfControl> OnNewControlSelected;

	private int m_selectedIndex;

	private dfControl m_self;

	private bool m_isLocalized;

	private float m_actualFillbarValue;

	private const float c_arrowScale = 3f;

	private static WindowsResolutionManager m_windowsResolutionManager;

	private bool m_changedThisFrame;

	private Vector3? m_cachedLeftArrowRelativePosition;

	private Vector3? m_cachedRightArrowRelativePosition;

	private List<GameOptions.PreferredScalingMode> m_scalingModes;

	private List<GameOptions.QuickstartCharacter> m_quickStartCharacters;

	private float m_panelStartHeight = -1f;

	private float m_additionalStartHeight = -1f;

	private bool m_infoControlHeightModified;

	private static Color m_unselectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

	private bool m_ignoreLeftRightUntilReleased;

	public static WindowsResolutionManager ResolutionManagerWin
	{
		get
		{
			if (m_windowsResolutionManager == null)
			{
				m_windowsResolutionManager = new WindowsResolutionManager("Enter the Gungeon");
			}
			return m_windowsResolutionManager;
		}
	}

	private float FillbarDelta
	{
		get
		{
			return (optionType != BraveOptionsOptionType.DISPLAY_SAFE_AREA) ? 0.05f : 0.2f;
		}
	}

	private void OnDestroy()
	{
		if (optionType == BraveOptionsOptionType.PLAYER_ONE_CONTROL_PORT || optionType == BraveOptionsOptionType.PLAYER_TWO_CONTROL_PORT)
		{
			InputManager.OnDeviceAttached -= InputManager_OnDeviceAttached;
			InputManager.OnDeviceDetached -= InputManager_OnDeviceDetached;
		}
	}

	private void ToggleAbledness(bool value)
	{
		if (!value)
		{
			if ((bool)m_self)
			{
				m_self.Disable();
				m_self.CanFocus = false;
				m_self.IsInteractive = false;
				m_self.DisabledColor = new Color(0.2f, 0.2f, 0.2f, 1f);
			}
			if ((bool)labelControl)
			{
				labelControl.DisabledColor = new Color(0.2f, 0.2f, 0.2f, 1f);
			}
			if ((bool)left)
			{
				left.DisabledColor = new Color(0.2f, 0.2f, 0.2f, 1f);
			}
			if ((bool)right)
			{
				right.DisabledColor = new Color(0.2f, 0.2f, 0.2f, 1f);
			}
			if ((bool)selectedLabelControl)
			{
				selectedLabelControl.DisabledColor = new Color(0.2f, 0.2f, 0.2f, 1f);
			}
			if ((bool)buttonControl)
			{
				buttonControl.Disable();
				buttonControl.CanFocus = false;
				buttonControl.IsInteractive = false;
				buttonControl.DisabledColor = new Color(0.2f, 0.2f, 0.2f, 1f);
				buttonControl.DisabledTextColor = new Color(0.2f, 0.2f, 0.2f, 1f);
			}
			if ((bool)up && (bool)down)
			{
				up.GetComponent<BraveOptionsMenuItem>().down = down;
				down.GetComponent<BraveOptionsMenuItem>().up = up;
			}
			else if ((bool)up)
			{
				up.GetComponent<BraveOptionsMenuItem>().down = null;
			}
			else if ((bool)down)
			{
				down.GetComponent<BraveOptionsMenuItem>().up = null;
			}
		}
	}

	private bool DisablePlatformSpecificOptions()
	{
		if (optionType == BraveOptionsOptionType.GAMEPLAY_SPEED)
		{
			DelControl();
			return true;
		}
		if (optionType == BraveOptionsOptionType.SWITCH_PERFORMANCE_MODE || optionType == BraveOptionsOptionType.SWITCH_REASSIGN_CONTROLLERS)
		{
			DelControl();
			return true;
		}
		if (optionType == BraveOptionsOptionType.ADDITIONAL_BLANK_CONTROL_PS4 || optionType == BraveOptionsOptionType.STICKY_FRICTION_AMOUNT || optionType == BraveOptionsOptionType.BOTH_CONTROLLER_CURSOR)
		{
			DelControl();
			return true;
		}
		return false;
	}

	public void Awake()
	{
		if (optionType == BraveOptionsOptionType.FULLSCREEN)
		{
			if (m_windowsResolutionManager == null)
			{
				m_windowsResolutionManager = new WindowsResolutionManager("Enter the Gungeon");
			}
			labelOptions = new string[3] { "Fullscreen", "Borderless", "Windowed" };
		}
		dfControl dfControl2 = (m_self = GetComponent<dfControl>());
		dfControl2.IsVisibleChanged += self_IsVisibleChanged;
		m_isLocalized = dfControl2.IsLocalized;
		dfControl2.CanFocus = true;
		dfControl2.GotFocus += DoFocus;
		dfControl2.LostFocus += LostFocus;
		if ((optionType == BraveOptionsOptionType.RESET_SAVE_SLOT || optionType == BraveOptionsOptionType.SAVE_SLOT) && GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER)
		{
			ToggleAbledness(false);
		}
		if (optionType == BraveOptionsOptionType.LOOT_PROFILE && GameManager.Instance.CurrentLevelOverrideState != GameManager.LevelOverrideState.FOYER)
		{
			ToggleAbledness(false);
		}
		if (labelOptions != null && labelOptions.Length > 0 && labelOptions[0].StartsWith("#"))
		{
			selectedLabelControl.IsLocalized = true;
			selectedLabelControl.Localize();
		}
		RelocalizeOptions();
		if (optionType == BraveOptionsOptionType.PLAYER_ONE_CONTROL_PORT || optionType == BraveOptionsOptionType.PLAYER_TWO_CONTROL_PORT)
		{
			InputManager.OnDeviceAttached += InputManager_OnDeviceAttached;
			InputManager.OnDeviceDetached += InputManager_OnDeviceDetached;
		}
		if (optionType == BraveOptionsOptionType.RESOLUTION || optionType == BraveOptionsOptionType.SCALING_MODE || optionType == BraveOptionsOptionType.VISUAL_PRESET)
		{
			dfControl2.ResolutionChangedPostLayout = (Action<dfControl, Vector3, Vector3>)Delegate.Combine(dfControl2.ResolutionChangedPostLayout, new Action<dfControl, Vector3, Vector3>(HandleResolutionChanged));
		}
		if (itemType == BraveOptionsMenuItemType.LeftRightArrow || itemType == BraveOptionsMenuItemType.LeftRightArrowInfo)
		{
			if (left != null)
			{
				left.HotZoneScale = Vector2.one * 3f;
				left.Click += DecrementArrow;
			}
			if (right != null)
			{
				right.HotZoneScale = Vector2.one * 3f;
				right.Click += IncrementArrow;
			}
		}
		else if (itemType == BraveOptionsMenuItemType.Fillbar)
		{
			fillbarControl.Click += HandleFillbarClick;
			fillbarControl.MouseDown += HandleFillbarDown;
			fillbarControl.MouseMove += HandleFillbarMove;
			fillbarControl.MouseHover += HandleFillbarHover;
		}
		else if (itemType == BraveOptionsMenuItemType.Checkbox)
		{
			dfControl2.Click += ToggleCheckbox;
			checkboxUnchecked.Click += ToggleCheckbox;
			checkboxUnchecked.IsInteractive = false;
			checkboxChecked.IsInteractive = false;
		}
		else if (itemType == BraveOptionsMenuItemType.Button)
		{
			buttonControl.Click += OnButtonClicked;
		}
		if (itemType == BraveOptionsMenuItemType.LeftRightArrow || itemType == BraveOptionsMenuItemType.LeftRightArrowInfo)
		{
			left.Color = m_unselectedColor;
			right.Color = m_unselectedColor;
			selectedLabelControl.Color = m_unselectedColor;
		}
		else if (itemType == BraveOptionsMenuItemType.Checkbox)
		{
			checkboxChecked.Color = m_unselectedColor;
			checkboxUnchecked.Color = m_unselectedColor;
		}
	}

	private void HandleLocalTextChangeReposition(dfControl control, string value)
	{
		if ((bool)labelControl)
		{
			labelControl.Pivot = dfPivotPoint.BottomLeft;
		}
	}

	private void InputManager_OnDeviceAttached(InputDevice obj)
	{
		InitializeFromOptions();
	}

	private void InputManager_OnDeviceDetached(InputDevice obj)
	{
		InitializeFromOptions();
	}

	private void OnButtonClicked(dfControl control, dfMouseEventArgs mouseEvent)
	{
		DoSelectedAction();
	}

	private void ToggleCheckbox(dfControl control, dfMouseEventArgs args)
	{
		if (!m_changedThisFrame && (args == null || !args.Used))
		{
			m_changedThisFrame = true;
			if (args != null)
			{
				args.Use();
			}
			m_selectedIndex = (m_selectedIndex + 1) % 2;
			checkboxChecked.IsVisible = m_selectedIndex == 1;
			HandleCheckboxValueChanged();
		}
	}

	private void self_IsVisibleChanged(dfControl control, bool value)
	{
		if (value)
		{
			ConvertPivots();
			dfControl component = GetComponent<dfControl>();
			if ((bool)component)
			{
				component.PerformLayout();
			}
			UpdateSelectedLabelText();
			UpdateInfoControl();
		}
	}

	private void RelocalizeOptions()
	{
		if (m_isLocalized && optionType == BraveOptionsOptionType.SCALING_MODE)
		{
			for (int i = 0; i < labelOptions.Length; i++)
			{
				string key = labelOptions[i];
				string value = m_self.GetLanguageManager().GetValue(key);
				labelOptions[i] = value;
			}
			for (int j = 0; j < infoOptions.Length; j++)
			{
				string key2 = infoOptions[j];
				infoOptions[j] = m_self.GetLanguageManager().GetValue(key2);
			}
		}
	}

	public void LateUpdate()
	{
		m_changedThisFrame = false;
	}

	public void Update()
	{
		if (labelControl != null && labelControl.IsVisible)
		{
			if (GameManager.Options.CurrentVisualPreset == GameOptions.VisualPresetMode.RECOMMENDED)
			{
				if (optionType == BraveOptionsOptionType.RESOLUTION)
				{
					Resolution recommendedResolution = GameManager.Options.GetRecommendedResolution();
					if (Screen.width != recommendedResolution.width || Screen.height != recommendedResolution.height)
					{
						HandleScreenDataChanged(recommendedResolution.width, recommendedResolution.height);
					}
				}
				else if (optionType == BraveOptionsOptionType.SCALING_MODE)
				{
					if (GameManager.Options.CurrentPreferredScalingMode != GameManager.Options.GetRecommendedScalingMode())
					{
						GameManager.Options.CurrentPreferredScalingMode = GameManager.Options.GetRecommendedScalingMode();
						HandleScreenDataChanged(Screen.width, Screen.height);
					}
				}
				else if (optionType == BraveOptionsOptionType.FULLSCREEN && GameManager.Options.CurrentPreferredFullscreenMode != 0)
				{
					GameManager.Options.CurrentPreferredFullscreenMode = GameOptions.PreferredFullscreenMode.FULLSCREEN;
					HandleScreenDataChanged(Screen.width, Screen.height);
				}
			}
			if (optionType == BraveOptionsOptionType.FULLSCREEN)
			{
				int indexFromFullscreenMode = GetIndexFromFullscreenMode(GameManager.Options.CurrentPreferredFullscreenMode);
				if (m_selectedIndex != indexFromFullscreenMode)
				{
					m_selectedIndex = indexFromFullscreenMode;
					DetermineAvailableOptions();
				}
			}
		}
		if (optionType == BraveOptionsOptionType.SCALING_MODE && m_scalingModes != null && m_scalingModes.Count > 0)
		{
			if (m_selectedIndex < 0 || m_selectedIndex >= m_scalingModes.Count)
			{
				m_selectedIndex = GetScalingIndex(GameManager.Options.CurrentPreferredScalingMode);
			}
			if (m_selectedIndex < 0 || m_selectedIndex >= m_scalingModes.Count)
			{
				m_selectedIndex = GetScalingIndex(GameOptions.PreferredScalingMode.PIXEL_PERFECT);
			}
			if (m_selectedIndex < 0 || m_selectedIndex >= m_scalingModes.Count)
			{
				m_selectedIndex = GetScalingIndex(GameOptions.PreferredScalingMode.FORCE_PIXEL_PERFECT);
			}
			if (m_selectedIndex < 0 || m_selectedIndex >= m_scalingModes.Count)
			{
				if (m_scalingModes.Contains(GameOptions.PreferredScalingMode.FORCE_PIXEL_PERFECT))
				{
					m_selectedIndex = GetScalingIndex(GameOptions.PreferredScalingMode.FORCE_PIXEL_PERFECT);
				}
				else
				{
					m_selectedIndex = 0;
				}
			}
			if (m_selectedIndex < m_scalingModes.Count && m_scalingModes[m_selectedIndex] != GameManager.Options.CurrentPreferredScalingMode)
			{
				m_selectedIndex = GetScalingIndex(GameManager.Options.CurrentPreferredScalingMode);
				if (m_selectedIndex < labelOptions.Length && m_selectedIndex >= 0)
				{
					UpdateSelectedLabelText();
				}
			}
		}
		if (optionType == BraveOptionsOptionType.VISUAL_PRESET && m_selectedIndex != (int)GameManager.Options.CurrentVisualPreset)
		{
			m_selectedIndex = (int)GameManager.Options.CurrentVisualPreset;
			if (m_selectedIndex >= 0 && m_selectedIndex < labelOptions.Length)
			{
				UpdateSelectedLabelText();
			}
		}
		if (optionType == BraveOptionsOptionType.RESOLUTION && GameManager.Options.CurrentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.BORDERLESS)
		{
			int width = Screen.currentResolution.width;
			int height = Screen.currentResolution.height;
			HandleScreenDataChanged(width, height);
		}
	}

	private void HandleResolutionChanged(dfControl arg1, Vector3 arg2, Vector3 arg3)
	{
		if (optionType == BraveOptionsOptionType.RESOLUTION)
		{
			DetermineAvailableOptions();
		}
		else if (optionType == BraveOptionsOptionType.SCALING_MODE)
		{
			DetermineAvailableOptions();
			if (m_scalingModes[m_selectedIndex] == GameOptions.PreferredScalingMode.PIXEL_PERFECT)
			{
				GameManager.Options.CurrentPreferredScalingMode = GameOptions.PreferredScalingMode.FORCE_PIXEL_PERFECT;
			}
			else if (m_scalingModes[m_selectedIndex] == GameOptions.PreferredScalingMode.UNIFORM_SCALING)
			{
				GameManager.Options.CurrentPreferredScalingMode = GameOptions.PreferredScalingMode.UNIFORM_SCALING;
			}
			else if (m_scalingModes[m_selectedIndex] == GameOptions.PreferredScalingMode.UNIFORM_SCALING_FAST)
			{
				GameManager.Options.CurrentPreferredScalingMode = GameOptions.PreferredScalingMode.UNIFORM_SCALING_FAST;
			}
			else if (m_scalingModes[m_selectedIndex] == GameOptions.PreferredScalingMode.FORCE_PIXEL_PERFECT)
			{
				GameManager.Options.CurrentPreferredScalingMode = GameOptions.PreferredScalingMode.FORCE_PIXEL_PERFECT;
			}
			HandleScreenDataChanged(Screen.width, Screen.height);
		}
		else if (optionType == BraveOptionsOptionType.FULLSCREEN)
		{
			m_selectedIndex = GetIndexFromFullscreenMode(GameManager.Options.CurrentPreferredFullscreenMode);
			DetermineAvailableOptions();
		}
	}

	private IEnumerator DeferFunctionNumberOfFrames(int numFrames, Action action)
	{
		for (int i = 0; i < numFrames; i++)
		{
			yield return null;
		}
		action();
	}

	private void UpdateSelectedLabelText()
	{
		if (!selectedLabelControl || labelOptions == null || labelOptions.Length == 0 || m_selectedIndex < 0 || m_selectedIndex >= labelOptions.Length)
		{
			return;
		}
		string text = labelOptions[m_selectedIndex];
		if (text.StartsWith("%"))
		{
			string[] array = text.Split(' ');
			string text2 = string.Empty;
			for (int i = 0; i < array.Length; i++)
			{
				text2 += StringTableManager.EvaluateReplacementToken(array[i]);
			}
			selectedLabelControl.ModifyLocalizedText(text2);
		}
		else
		{
			selectedLabelControl.Text = labelOptions[m_selectedIndex];
		}
		if (left.IsVisible && right.IsVisible)
		{
			if (!m_cachedLeftArrowRelativePosition.HasValue && (itemType == BraveOptionsMenuItemType.LeftRightArrow || itemType == BraveOptionsMenuItemType.LeftRightArrowInfo) && (bool)left)
			{
				m_cachedLeftArrowRelativePosition = left.RelativePosition;
			}
			if (!m_cachedRightArrowRelativePosition.HasValue && (itemType == BraveOptionsMenuItemType.LeftRightArrow || itemType == BraveOptionsMenuItemType.LeftRightArrowInfo) && (bool)right)
			{
				m_cachedRightArrowRelativePosition = right.RelativePosition;
			}
			left.PerformLayout();
			right.PerformLayout();
			if (m_cachedLeftArrowRelativePosition.HasValue && m_cachedRightArrowRelativePosition.HasValue)
			{
				float a = m_cachedRightArrowRelativePosition.Value.x - m_cachedLeftArrowRelativePosition.Value.x;
				float b = selectedLabelControl.Width + 45f;
				float num = Mathf.Max(a, b).Quantize(3f, VectorConversions.Ceil);
				float num2 = right.RelativePosition.x - left.RelativePosition.x;
				float num3 = (num - num2) / 2f;
				left.RelativePosition += new Vector3(0f - num3, 0f, 0f);
				right.RelativePosition += new Vector3(num3, 0f, 0f);
			}
		}
	}

	private void InitializeVisualPreset()
	{
		Resolution recommendedResolution = GameManager.Options.GetRecommendedResolution();
		GameOptions.PreferredScalingMode recommendedScalingMode = GameManager.Options.GetRecommendedScalingMode();
		if (Screen.width == recommendedResolution.width && Screen.height == recommendedResolution.height && recommendedScalingMode == GameManager.Options.CurrentPreferredScalingMode && GameManager.Options.CurrentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.FULLSCREEN)
		{
			m_selectedIndex = 0;
		}
		else
		{
			m_selectedIndex = 1;
		}
		GameManager.Options.CurrentVisualPreset = (GameOptions.VisualPresetMode)m_selectedIndex;
		UpdateSelectedLabelText();
	}

	private StringTableManager.GungeonSupportedLanguages IntToLanguage(int index)
	{
		switch (index)
		{
		case 0:
			return StringTableManager.GungeonSupportedLanguages.ENGLISH;
		case 1:
			return StringTableManager.GungeonSupportedLanguages.SPANISH;
		case 2:
			return StringTableManager.GungeonSupportedLanguages.FRENCH;
		case 3:
			return StringTableManager.GungeonSupportedLanguages.ITALIAN;
		case 4:
			return StringTableManager.GungeonSupportedLanguages.GERMAN;
		case 5:
			return StringTableManager.GungeonSupportedLanguages.BRAZILIANPORTUGUESE;
		case 6:
			return StringTableManager.GungeonSupportedLanguages.POLISH;
		case 7:
			return StringTableManager.GungeonSupportedLanguages.RUSSIAN;
		case 8:
			return StringTableManager.GungeonSupportedLanguages.JAPANESE;
		case 9:
			return StringTableManager.GungeonSupportedLanguages.KOREAN;
		case 10:
			return StringTableManager.GungeonSupportedLanguages.CHINESE;
		default:
			return StringTableManager.GungeonSupportedLanguages.ENGLISH;
		}
	}

	private int LanguageToInt(StringTableManager.GungeonSupportedLanguages language)
	{
		switch (language)
		{
		case StringTableManager.GungeonSupportedLanguages.ENGLISH:
			return 0;
		case StringTableManager.GungeonSupportedLanguages.SPANISH:
			return 1;
		case StringTableManager.GungeonSupportedLanguages.FRENCH:
			return 2;
		case StringTableManager.GungeonSupportedLanguages.ITALIAN:
			return 3;
		case StringTableManager.GungeonSupportedLanguages.GERMAN:
			return 4;
		case StringTableManager.GungeonSupportedLanguages.BRAZILIANPORTUGUESE:
			return 5;
		case StringTableManager.GungeonSupportedLanguages.POLISH:
			return 6;
		case StringTableManager.GungeonSupportedLanguages.RUSSIAN:
			return 7;
		case StringTableManager.GungeonSupportedLanguages.JAPANESE:
			return 8;
		case StringTableManager.GungeonSupportedLanguages.KOREAN:
			return 9;
		case StringTableManager.GungeonSupportedLanguages.CHINESE:
			return 10;
		default:
			return 0;
		}
	}

	private void DetermineAvailableOptions()
	{
		switch (optionType)
		{
		case BraveOptionsOptionType.RESOLUTION:
			HandleResolutionDetermination();
			break;
		case BraveOptionsOptionType.SCALING_MODE:
		{
			int width = Screen.width;
			int height = Screen.height;
			int num3 = BraveMathCollege.GreatestCommonDivisor(width, height);
			int num4 = width / num3;
			int num5 = height / num3;
			List<string> list6 = new List<string>();
			if (m_scalingModes == null)
			{
				m_scalingModes = new List<GameOptions.PreferredScalingMode>();
			}
			m_scalingModes.Clear();
			if (num4 == 16 && num5 == 9)
			{
				if (width % 480 == 0 && height % 270 == 0)
				{
					list6.Add("#OPTIONS_PIXELPERFECT");
					m_scalingModes.Add(GameOptions.PreferredScalingMode.PIXEL_PERFECT);
				}
				else
				{
					list6.Add("#OPTIONS_UNIFORMSCALING");
					m_scalingModes.Add(GameOptions.PreferredScalingMode.UNIFORM_SCALING);
					list6.Add("#OPTIONS_FORCEPIXELPERFECT");
					m_scalingModes.Add(GameOptions.PreferredScalingMode.FORCE_PIXEL_PERFECT);
					list6.Add("#OPTIONS_UNIFORMSCALINGFAST");
					m_scalingModes.Add(GameOptions.PreferredScalingMode.UNIFORM_SCALING_FAST);
				}
			}
			else
			{
				list6.Add("#OPTIONS_UNIFORMSCALING");
				m_scalingModes.Add(GameOptions.PreferredScalingMode.UNIFORM_SCALING);
				list6.Add("#OPTIONS_FORCEPIXELPERFECT");
				m_scalingModes.Add(GameOptions.PreferredScalingMode.FORCE_PIXEL_PERFECT);
				list6.Add("#OPTIONS_UNIFORMSCALINGFAST");
				m_scalingModes.Add(GameOptions.PreferredScalingMode.UNIFORM_SCALING_FAST);
			}
			labelOptions = list6.ToArray();
			if (m_scalingModes.Contains(GameManager.Options.CurrentPreferredScalingMode))
			{
				m_selectedIndex = GetScalingIndex(GameManager.Options.CurrentPreferredScalingMode);
			}
			else
			{
				m_selectedIndex = 0;
				if (list6.Count >= 2 && GameManager.Options.CurrentPreferredScalingMode == GameOptions.PreferredScalingMode.PIXEL_PERFECT)
				{
					m_selectedIndex = 1;
				}
			}
			RelocalizeOptions();
			break;
		}
		case BraveOptionsOptionType.PLAYER_ONE_CONTROL_PORT:
		{
			List<string> list2 = new List<string>();
			for (int i = 0; i < InputManager.Devices.Count; i++)
			{
				string text = InputManager.Devices[i].Name;
				int num = 1;
				string item = text;
				while (list2.Contains(item))
				{
					num++;
					item = text + " " + num;
				}
				list2.Add(item);
			}
			list2.Add(m_self.ForceGetLocalizedValue("#OPTIONS_KEYBOARDMOUSE"));
			labelOptions = list2.ToArray();
			break;
		}
		case BraveOptionsOptionType.PLAYER_TWO_CONTROL_PORT:
		{
			List<string> list4 = new List<string>();
			for (int k = 0; k < InputManager.Devices.Count; k++)
			{
				string text2 = InputManager.Devices[k].Name;
				int num2 = 1;
				string item2 = text2;
				while (list4.Contains(item2))
				{
					num2++;
					item2 = text2 + " " + num2;
				}
				list4.Add(item2);
			}
			list4.Add(m_self.ForceGetLocalizedValue("#OPTIONS_KEYBOARDMOUSE"));
			labelOptions = list4.ToArray();
			break;
		}
		case BraveOptionsOptionType.LANGUAGE:
		{
			List<string> list5 = new List<string>();
			list5.Add("#LANGUAGE_ENGLISH");
			list5.Add("#LANGUAGE_SPANISH");
			list5.Add("#LANGUAGE_FRENCH");
			list5.Add("#LANGUAGE_ITALIAN");
			list5.Add("#LANGUAGE_GERMAN");
			list5.Add("#LANGUAGE_PORTUGUESE");
			list5.Add("#LANGUAGE_POLISH");
			list5.Add("#LANGUAGE_RUSSIAN");
			list5.Add("#LANGUAGE_JAPANESE");
			list5.Add("#LANGUAGE_KOREAN");
			list5.Add("#LANGUAGE_CHINESE");
			labelOptions = list5.ToArray();
			RelocalizeOptions();
			break;
		}
		case BraveOptionsOptionType.MONITOR_SELECT:
		{
			List<string> list3 = new List<string>();
			for (int j = 0; j < Display.displays.Length; j++)
			{
				list3.Add((j + 1).ToString());
			}
			labelOptions = list3.ToArray();
			break;
		}
		case BraveOptionsOptionType.QUICKSTART_CHARACTER:
		{
			if (m_quickStartCharacters == null)
			{
				m_quickStartCharacters = new List<GameOptions.QuickstartCharacter>();
			}
			else
			{
				m_quickStartCharacters.Clear();
			}
			List<string> list = new List<string>(7);
			m_quickStartCharacters.Add(GameOptions.QuickstartCharacter.LAST_USED);
			list.Add("#CHAR_LASTUSED");
			m_quickStartCharacters.Add(GameOptions.QuickstartCharacter.PILOT);
			list.Add("#CHAR_ROGUE");
			m_quickStartCharacters.Add(GameOptions.QuickstartCharacter.CONVICT);
			list.Add("#CHAR_CONVICT");
			m_quickStartCharacters.Add(GameOptions.QuickstartCharacter.SOLDIER);
			list.Add("#CHAR_MARINE");
			m_quickStartCharacters.Add(GameOptions.QuickstartCharacter.GUIDE);
			list.Add("#CHAR_GUIDE");
			if (GameStatsManager.HasInstance && GameStatsManager.Instance.GetFlag(GungeonFlags.SECRET_BULLETMAN_SEEN_05))
			{
				m_quickStartCharacters.Add(GameOptions.QuickstartCharacter.BULLET);
				list.Add("#CHAR_BULLET");
			}
			if (GameStatsManager.HasInstance && GameStatsManager.Instance.GetFlag(GungeonFlags.BLACKSMITH_RECEIVED_BUSTED_TELEVISION))
			{
				m_quickStartCharacters.Add(GameOptions.QuickstartCharacter.ROBOT);
				list.Add("#CHAR_ROBOT");
			}
			labelOptions = list.ToArray();
			m_selectedIndex = GetQuickStartCharIndex(GameManager.Options.PreferredQuickstartCharacter);
			if (m_selectedIndex < 0 || m_selectedIndex >= labelOptions.Length)
			{
				m_selectedIndex = 0;
			}
			UpdateSelectedLabelText();
			break;
		}
		}
		if (itemType == BraveOptionsMenuItemType.LeftRightArrow || itemType == BraveOptionsMenuItemType.LeftRightArrowInfo)
		{
			if (m_selectedIndex >= labelOptions.Length)
			{
				m_selectedIndex = 0;
			}
			if (labelOptions != null && m_selectedIndex > -1 && m_selectedIndex < labelOptions.Length)
			{
				UpdateSelectedLabelText();
				if (itemType == BraveOptionsMenuItemType.LeftRightArrowInfo)
				{
					UpdateInfoControl();
				}
			}
			else
			{
				selectedLabelControl.Text = "?";
			}
		}
		if (itemType == BraveOptionsMenuItemType.Checkbox)
		{
			RepositionCheckboxControl();
		}
		if ((bool)labelControl)
		{
			labelControl.PerformLayout();
		}
		UpdateSelectedLabelText();
		UpdateInfoControl();
	}

	private void UpdateInfoControl()
	{
		if (optionType == BraveOptionsOptionType.RESOLUTION)
		{
			List<Resolution> availableResolutions = GetAvailableResolutions();
			m_selectedIndex = Mathf.Clamp(m_selectedIndex, 0, availableResolutions.Count - 1);
			int width = availableResolutions[m_selectedIndex].width;
			int height = availableResolutions[m_selectedIndex].height;
			int num = BraveMathCollege.GreatestCommonDivisor(width, height);
			int num2 = width / num;
			int num3 = height / num;
			bool flag = num2 == 16 && num3 == 9;
			float a = (float)width / 480f;
			float b = (float)height / 270f;
			bool flag2 = Mathf.Min(a, b) % 1f == 0f;
			if (flag && flag2)
			{
				infoControl.Color = Color.green;
				infoControl.Text = infoControl.ForceGetLocalizedValue("#OPTIONS_RESOLUTION_BEST");
			}
			else if (flag2)
			{
				infoControl.Color = Color.green;
				infoControl.Text = infoControl.ForceGetLocalizedValue("#OPTIONS_RESOLUTION_GOOD");
			}
			else
			{
				infoControl.Color = Color.red;
				infoControl.Text = infoControl.ForceGetLocalizedValue("#OPTIONS_RESOLUTION_BAD");
			}
		}
		else if (optionType == BraveOptionsOptionType.SCALING_MODE)
		{
			if (GameManager.Options.CurrentPreferredScalingMode == GameOptions.PreferredScalingMode.PIXEL_PERFECT)
			{
				infoControl.Color = Color.green;
				infoControl.Text = infoControl.ForceGetLocalizedValue("#OPTIONS_PIXELPERFECT_INFO");
			}
			else if (GameManager.Options.CurrentPreferredScalingMode == GameOptions.PreferredScalingMode.UNIFORM_SCALING || GameManager.Options.CurrentPreferredScalingMode == GameOptions.PreferredScalingMode.UNIFORM_SCALING_FAST)
			{
				float a2 = (float)Screen.width / 480f;
				float b2 = (float)Screen.height / 270f;
				if (Mathf.Min(a2, b2) % 1f == 0f)
				{
					infoControl.Color = Color.green;
					infoControl.Text = infoControl.ForceGetLocalizedValue("#OPTIONS_UNIFORMSCALING_INFOGOOD");
				}
				else
				{
					infoControl.Color = Color.red;
					infoControl.Text = infoControl.ForceGetLocalizedValue("#OPTIONS_UNIFORMSCALING_INFOBAD");
				}
			}
			else
			{
				infoControl.Color = Color.green;
				infoControl.Text = infoControl.ForceGetLocalizedValue("#OPTIONS_FORCEPIXELPERFECT_INFO");
			}
		}
		UpdateInfoControlHeight();
	}

	private int GetScalingIndex(GameOptions.PreferredScalingMode scalingMode)
	{
		for (int i = 0; i < m_scalingModes.Count; i++)
		{
			if (m_scalingModes[i] == scalingMode)
			{
				return i;
			}
		}
		return -1;
	}

	private int GetQuickStartCharIndex(GameOptions.QuickstartCharacter quickstartChar)
	{
		for (int i = 0; i < m_quickStartCharacters.Count; i++)
		{
			if (m_quickStartCharacters[i] == quickstartChar)
			{
				return i;
			}
		}
		return -1;
	}

	private void HandleResolutionDetermination()
	{
		List<Resolution> availableResolutions = GetAvailableResolutions();
		labelOptions = new string[availableResolutions.Count];
		m_selectedIndex = 0;
		for (int i = 0; i < availableResolutions.Count; i++)
		{
			int width = availableResolutions[i].width;
			int height = availableResolutions[i].height;
			int num = BraveMathCollege.GreatestCommonDivisor(width, height);
			int num2 = width / num;
			int num3 = height / num;
			labelOptions[i] = width.ToString() + " x " + height.ToString() + " (" + num2 + ":" + num3 + ")";
			if (width == Screen.width && height == Screen.height)
			{
				m_selectedIndex = i;
			}
		}
	}

	private void HandleFillbarClick(dfControl control, dfMouseEventArgs mouseEvent)
	{
		if (!mouseEvent.Buttons.IsSet(dfMouseButtons.Left))
		{
			return;
		}
		Collider component = control.GetComponent<Collider>();
		RaycastHit hitInfo;
		bool flag = component.Raycast(mouseEvent.Ray, out hitInfo, 1000f);
		Vector2 vector = Vector2.zero;
		if (flag)
		{
			vector = hitInfo.point;
		}
		else
		{
			Plane plane = new Plane(Vector3.back, component.bounds.center.WithZ(0f));
			float enter;
			if (plane.Raycast(mouseEvent.Ray, out enter))
			{
				vector = BraveMathCollege.ClosestPointOnRectangle(mouseEvent.Ray.GetPoint(enter), component.bounds.min, component.bounds.extents * 2f);
			}
		}
		float num = control.Width * control.transform.localScale.x * control.PixelsToUnits();
		float num2 = (vector.x - (control.transform.position.x - num / 2f)) / num;
		m_actualFillbarValue = Mathf.Clamp(num2 + FillbarDelta / 2f, 0f, 1f).Quantize(FillbarDelta);
		fillbarControl.Value = m_actualFillbarValue * 0.98f + 0.01f;
		HandleFillbarValueChanged();
		mouseEvent.Use();
	}

	private void HandleFillbarDown(dfControl control, dfMouseEventArgs mouseEvent)
	{
		HandleFillbarClick(control, mouseEvent);
	}

	private void HandleFillbarMove(dfControl control, dfMouseEventArgs mouseEvent)
	{
		HandleFillbarClick(control, mouseEvent);
	}

	private void HandleFillbarHover(dfControl control, dfMouseEventArgs mouseEvent)
	{
		HandleFillbarClick(control, mouseEvent);
	}

	private void DelControl()
	{
		BraveOptionsMenuItem braveOptionsMenuItem = ((!(up != null)) ? null : up.GetComponent<BraveOptionsMenuItem>());
		BraveOptionsMenuItem braveOptionsMenuItem2 = ((!(down != null)) ? null : down.GetComponent<BraveOptionsMenuItem>());
		if (braveOptionsMenuItem != null)
		{
			braveOptionsMenuItem.down = down;
		}
		else
		{
			UIKeyControls uIKeyControls = ((!(up != null)) ? null : up.GetComponent<UIKeyControls>());
			if (uIKeyControls != null)
			{
				uIKeyControls.down = down;
			}
		}
		if (braveOptionsMenuItem2 != null)
		{
			braveOptionsMenuItem2.up = up;
		}
		else
		{
			UIKeyControls uIKeyControls2 = ((!(down != null)) ? null : down.GetComponent<UIKeyControls>());
			if (uIKeyControls2 != null)
			{
				uIKeyControls2.up = up;
			}
		}
		m_self.Parent.RemoveControl(m_self);
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public IEnumerator Start()
	{
		yield return null;
		if (DisablePlatformSpecificOptions())
		{
			yield break;
		}
		if (buttonControl != null)
		{
			buttonControl.AutoSize = true;
		}
		if (itemType == BraveOptionsMenuItemType.Checkbox)
		{
			dfScrollPanel component = m_self.Parent.GetComponent<dfScrollPanel>();
			component.ScrollPositionChanged += delegate
			{
				checkboxChecked.Invalidate();
				checkboxUnchecked.Invalidate();
			};
		}
		SetUnselectedColors();
		InitializeFromOptions();
		GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().OptionsMenu.RegisterItem(this);
		if (itemType == BraveOptionsMenuItemType.LeftRightArrow || itemType == BraveOptionsMenuItemType.LeftRightArrowInfo)
		{
			left.HotZoneScale = Vector2.one * 3f;
			right.HotZoneScale = left.HotZoneScale;
			left.MouseEnter += ArrowHoverGrow;
			left.MouseLeave += ArrowReturnScale;
			right.MouseEnter += ArrowHoverGrow;
			right.MouseLeave += ArrowReturnScale;
		}
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER && (optionType == BraveOptionsOptionType.PLAYER_ONE_CONTROL_PORT || optionType == BraveOptionsOptionType.PLAYER_TWO_CONTROL_PORT))
		{
			Foyer instance = Foyer.Instance;
			instance.OnCoopModeChanged = (Action)Delegate.Combine(instance.OnCoopModeChanged, new Action(InitializeFromOptions));
		}
		m_panelStartHeight = GetComponent<dfControl>().Height;
		m_additionalStartHeight = ((!infoControl) ? (-1f) : infoControl.Height);
		UpdateInfoControlHeight();
	}

	private void UpdateInfoControlHeight()
	{
		if (!infoControl)
		{
			return;
		}
		if (m_panelStartHeight < 0f)
		{
			m_panelStartHeight = GetComponent<dfControl>().Height;
		}
		if (m_additionalStartHeight < 0f)
		{
			m_additionalStartHeight = infoControl.Height;
		}
		if ((Application.platform == RuntimePlatform.PS4 && Application.platform == RuntimePlatform.XboxOne) || (optionType != BraveOptionsOptionType.RESOLUTION && optionType != BraveOptionsOptionType.SCALING_MODE))
		{
			return;
		}
		if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.KOREAN || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.CHINESE || GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.RUSSIAN)
		{
			if (!m_infoControlHeightModified)
			{
				GetComponent<dfControl>().Height = m_panelStartHeight + 30f;
				infoControl.Height = m_additionalStartHeight + 30f;
				infoControl.RelativePosition += new Vector3(0f, 30f, 0f);
				m_infoControlHeightModified = true;
			}
		}
		else if (m_infoControlHeightModified)
		{
			GetComponent<dfControl>().Height = m_panelStartHeight;
			infoControl.Height = m_additionalStartHeight;
			infoControl.RelativePosition -= new Vector3(0f, 30f, 0f);
			m_infoControlHeightModified = false;
		}
		infoControl.PerformLayout();
	}

	private void ConvertPivots()
	{
		if ((bool)labelControl)
		{
			labelControl.Pivot = dfPivotPoint.BottomLeft;
		}
		if ((bool)selectedLabelControl)
		{
			selectedLabelControl.Pivot = dfPivotPoint.BottomLeft;
		}
		if ((bool)infoControl)
		{
			infoControl.Pivot = dfPivotPoint.BottomLeft;
		}
		if ((bool)buttonControl)
		{
			buttonControl.Pivot = dfPivotPoint.BottomLeft;
		}
	}

	private int GetIndexFromFullscreenMode(GameOptions.PreferredFullscreenMode fMode)
	{
		int result;
		switch (fMode)
		{
		case GameOptions.PreferredFullscreenMode.FULLSCREEN:
			result = 0;
			break;
		case GameOptions.PreferredFullscreenMode.BORDERLESS:
			result = 1;
			break;
		default:
			result = 2;
			break;
		}
		return result;
	}

	public void ForceRefreshDisplayLabel()
	{
		if ((bool)selectedLabelControl)
		{
			UpdateSelectedLabelText();
			selectedLabelControl.PerformLayout();
		}
	}

	public void InitializeFromOptions()
	{
		switch (optionType)
		{
		case BraveOptionsOptionType.MUSIC_VOLUME:
			m_actualFillbarValue = GameManager.Options.MusicVolume / 100f;
			fillbarControl.Value = m_actualFillbarValue * 0.98f + 0.01f;
			break;
		case BraveOptionsOptionType.SOUND_VOLUME:
			m_actualFillbarValue = GameManager.Options.SoundVolume / 100f;
			fillbarControl.Value = m_actualFillbarValue * 0.98f + 0.01f;
			break;
		case BraveOptionsOptionType.UI_VOLUME:
			m_actualFillbarValue = GameManager.Options.UIVolume / 100f;
			fillbarControl.Value = m_actualFillbarValue * 0.98f + 0.01f;
			break;
		case BraveOptionsOptionType.SPEAKER_TYPE:
			m_selectedIndex = (int)GameManager.Options.AudioHardware;
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.VISUAL_PRESET:
			InitializeVisualPreset();
			break;
		case BraveOptionsOptionType.FULLSCREEN:
			m_selectedIndex = GetIndexFromFullscreenMode(GameManager.Options.CurrentPreferredFullscreenMode);
			break;
		case BraveOptionsOptionType.GAMMA:
			m_actualFillbarValue = GameManager.Options.Gamma - 0.5f;
			fillbarControl.Value = m_actualFillbarValue * 0.98f + 0.01f;
			break;
		case BraveOptionsOptionType.DISPLAY_SAFE_AREA:
			m_actualFillbarValue = (GameManager.Options.DisplaySafeArea - 0.9f) * 10f;
			fillbarControl.Value = m_actualFillbarValue * 0.98f + 0.01f;
			break;
		case BraveOptionsOptionType.MONITOR_SELECT:
			m_selectedIndex = GameManager.Options.CurrentMonitorIndex;
			DetermineAvailableOptions();
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.VSYNC:
			if (QualitySettings.vSyncCount > 0 && !GameManager.Options.DoVsync)
			{
				GameManager.Options.DoVsync = false;
			}
			m_selectedIndex = (GameManager.Options.DoVsync ? 1 : 0);
			HandleCheckboxValueChanged();
			break;
		case BraveOptionsOptionType.LIGHTING_QUALITY:
			m_selectedIndex = ((GameManager.Options.LightingQuality != GameOptions.GenericHighMedLowOption.HIGH) ? 1 : 0);
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.QUICKSTART_CHARACTER:
			if (m_quickStartCharacters != null)
			{
				m_selectedIndex = GetQuickStartCharIndex(GameManager.Options.PreferredQuickstartCharacter);
				if (m_selectedIndex < 0 || m_selectedIndex >= m_quickStartCharacters.Count)
				{
					m_selectedIndex = 0;
				}
				UpdateSelectedLabelText();
			}
			break;
		case BraveOptionsOptionType.ADDITIONAL_BLANK_CONTROL:
			m_selectedIndex = (int)GameManager.Options.additionalBlankControl;
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.ADDITIONAL_BLANK_CONTROL_TWO:
			m_selectedIndex = (int)GameManager.Options.additionalBlankControlTwo;
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.ADDITIONAL_BLANK_CONTROL_PS4:
			UpdateLabelOptions(FullOptionsMenuController.CurrentBindingPlayerTargetIndex);
			if (FullOptionsMenuController.CurrentBindingPlayerTargetIndex == 0)
			{
				m_selectedIndex = (int)GameManager.Options.additionalBlankControl;
			}
			else
			{
				m_selectedIndex = (int)GameManager.Options.additionalBlankControlTwo;
			}
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.CURSOR_VARIATION:
			m_selectedIndex = GameManager.Options.CurrentCursorIndex;
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.SHADER_QUALITY:
			switch (GameManager.Options.ShaderQuality)
			{
			case GameOptions.GenericHighMedLowOption.HIGH:
				m_selectedIndex = 0;
				break;
			case GameOptions.GenericHighMedLowOption.MEDIUM:
				m_selectedIndex = 3;
				break;
			case GameOptions.GenericHighMedLowOption.LOW:
				m_selectedIndex = 2;
				break;
			case GameOptions.GenericHighMedLowOption.VERY_LOW:
				m_selectedIndex = 1;
				break;
			default:
				m_selectedIndex = 0;
				break;
			}
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.DEBRIS_QUANTITY:
			switch (GameManager.Options.DebrisQuantity)
			{
			case GameOptions.GenericHighMedLowOption.HIGH:
				m_selectedIndex = 0;
				break;
			case GameOptions.GenericHighMedLowOption.MEDIUM:
				m_selectedIndex = 3;
				break;
			case GameOptions.GenericHighMedLowOption.LOW:
				m_selectedIndex = 2;
				break;
			case GameOptions.GenericHighMedLowOption.VERY_LOW:
				m_selectedIndex = 1;
				break;
			default:
				m_selectedIndex = 0;
				break;
			}
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.LOOT_PROFILE:
			switch (GameManager.Options.CurrentGameLootProfile)
			{
			case GameOptions.GameLootProfile.CURRENT:
				m_selectedIndex = 0;
				break;
			case GameOptions.GameLootProfile.ORIGINAL:
				m_selectedIndex = 1;
				break;
			default:
				m_selectedIndex = 0;
				break;
			}
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.AUTOAIM:
			switch (GameManager.Options.controllerAutoAim)
			{
			case GameOptions.ControllerAutoAim.AUTO_DETECT:
				m_selectedIndex = 0;
				break;
			case GameOptions.ControllerAutoAim.ALWAYS:
				m_selectedIndex = 1;
				break;
			case GameOptions.ControllerAutoAim.NEVER:
				m_selectedIndex = 2;
				break;
			case GameOptions.ControllerAutoAim.COOP_ONLY:
				m_selectedIndex = 3;
				break;
			}
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.SCREEN_SHAKE_AMOUNT:
			m_actualFillbarValue = GameManager.Options.ScreenShakeMultiplier * 0.5f;
			fillbarControl.Value = m_actualFillbarValue * 0.98f + 0.01f;
			break;
		case BraveOptionsOptionType.COOP_SCREEN_SHAKE_AMOUNT:
			m_selectedIndex = (GameManager.Options.CoopScreenShakeReduction ? 1 : 0);
			HandleCheckboxValueChanged();
			break;
		case BraveOptionsOptionType.STICKY_FRICTION_AMOUNT:
			m_actualFillbarValue = GameManager.Options.StickyFrictionMultiplier * 0.8f;
			fillbarControl.Value = m_actualFillbarValue * 0.98f + 0.01f;
			break;
		case BraveOptionsOptionType.SAVE_SLOT:
			m_selectedIndex = (int)SaveManager.CurrentSaveSlot;
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.TEXT_SPEED:
			m_selectedIndex = ((GameManager.Options.TextSpeed != GameOptions.GenericHighMedLowOption.MEDIUM) ? ((GameManager.Options.TextSpeed != 0) ? 1 : 2) : 0);
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.LANGUAGE:
			m_selectedIndex = LanguageToInt(GameManager.Options.CurrentLanguage);
			if (m_selectedIndex >= labelOptions.Length)
			{
				DetermineAvailableOptions();
			}
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.CONTROLLER_AIM_ASSIST_AMOUNT:
			m_actualFillbarValue = GameManager.Options.controllerAimAssistMultiplier * 0.8f;
			fillbarControl.Value = m_actualFillbarValue * 0.98f + 0.01f;
			break;
		case BraveOptionsOptionType.CONTROLLER_BEAM_AIM_ASSIST:
			m_selectedIndex = (GameManager.Options.controllerBeamAimAssist ? 1 : 0);
			HandleCheckboxValueChanged();
			break;
		case BraveOptionsOptionType.BEASTMODE:
			m_selectedIndex = (GameManager.Options.m_beastmode ? 1 : 0);
			HandleCheckboxValueChanged();
			break;
		case BraveOptionsOptionType.SPEEDRUN:
			m_selectedIndex = (GameManager.Options.SpeedrunMode ? 1 : 0);
			HandleCheckboxValueChanged();
			break;
		case BraveOptionsOptionType.OUT_OF_COMBAT_SPEED_INCREASE:
			m_selectedIndex = (GameManager.Options.IncreaseSpeedOutOfCombat ? 1 : 0);
			HandleCheckboxValueChanged();
			break;
		case BraveOptionsOptionType.RUMBLE:
			m_selectedIndex = (GameManager.Options.RumbleEnabled ? 1 : 0);
			HandleCheckboxValueChanged();
			break;
		case BraveOptionsOptionType.SMALL_UI:
			m_selectedIndex = (GameManager.Options.SmallUIEnabled ? 1 : 0);
			HandleCheckboxValueChanged();
			break;
		case BraveOptionsOptionType.REALTIME_REFLECTIONS:
			m_selectedIndex = (GameManager.Options.RealtimeReflections ? 1 : 0);
			HandleCheckboxValueChanged();
			break;
		case BraveOptionsOptionType.HIDE_EMPTY_GUNS:
			m_selectedIndex = (GameManager.Options.HideEmptyGuns ? 1 : 0);
			HandleCheckboxValueChanged();
			break;
		case BraveOptionsOptionType.QUICKSELECT:
			m_selectedIndex = (GameManager.Options.QuickSelectEnabled ? 1 : 0);
			HandleCheckboxValueChanged();
			break;
		case BraveOptionsOptionType.CONTROLLER_AIM_LOOK:
			m_actualFillbarValue = GameManager.Options.controllerAimLookMultiplier * 0.8f;
			fillbarControl.Value = m_actualFillbarValue * 0.98f + 0.01f;
			break;
		case BraveOptionsOptionType.PLAYER_ONE_CONTROL_PORT:
			if (GameManager.Instance.PrimaryPlayer == null)
			{
				m_selectedIndex = 0;
			}
			else if (GameManager.Options.PlayerIDtoDeviceIndexMap.ContainsKey(GameManager.Instance.PrimaryPlayer.PlayerIDX))
			{
				m_selectedIndex = GameManager.Options.PlayerIDtoDeviceIndexMap[GameManager.Instance.PrimaryPlayer.PlayerIDX];
			}
			else
			{
				m_selectedIndex = 0;
			}
			break;
		case BraveOptionsOptionType.PLAYER_TWO_CONTROL_PORT:
			if (GameManager.Instance.AllPlayers.Length > 1)
			{
				m_selectedIndex = GameManager.Options.PlayerIDtoDeviceIndexMap[GameManager.Instance.SecondaryPlayer.PlayerIDX];
			}
			break;
		case BraveOptionsOptionType.PLAYER_ONE_CONTROLLER_SYMBOLOGY:
			m_selectedIndex = (int)GameManager.Options.PlayerOnePreferredSymbology;
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.PLAYER_TWO_CONTROLLER_SYMBOLOGY:
			m_selectedIndex = (int)GameManager.Options.PlayerTwoPreferredSymbology;
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.MINIMAP_STYLE:
			m_selectedIndex = ((GameManager.Options.MinimapDisplayMode != Minimap.MinimapDisplayMode.FADE_ON_ROOM_SEAL) ? ((GameManager.Options.MinimapDisplayMode != Minimap.MinimapDisplayMode.ALWAYS) ? 1 : 2) : 0);
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.CURRENT_BINDINGS_PRESET:
			UpdateLabelOptions(0);
			m_selectedIndex = Mathf.Clamp((int)GameManager.Options.CurrentControlPreset, 0, labelOptions.Length - 1);
			selectedLabelControl.PerformLayout();
			break;
		case BraveOptionsOptionType.CURRENT_BINDINGS_PRESET_P2:
			UpdateLabelOptions(1);
			m_selectedIndex = Mathf.Clamp((int)GameManager.Options.CurrentControlPresetP2, 0, labelOptions.Length - 1);
			selectedLabelControl.PerformLayout();
			break;
		case BraveOptionsOptionType.PLAYER_ONE_CONTROLLER_CURSOR:
			m_selectedIndex = (GameManager.Options.PlayerOneControllerCursor ? 1 : 0);
			HandleCheckboxValueChanged();
			break;
		case BraveOptionsOptionType.PLAYER_TWO_CONTROLLER_CURSOR:
			m_selectedIndex = (GameManager.Options.PlayerTwoControllerCursor ? 1 : 0);
			HandleCheckboxValueChanged();
			break;
		case BraveOptionsOptionType.BOTH_CONTROLLER_CURSOR:
			m_selectedIndex = (GameManager.Options.PlayerOneControllerCursor ? 1 : 0);
			HandleCheckboxValueChanged();
			break;
		case BraveOptionsOptionType.ALLOWED_CONTROLLER_TYPES:
			if (GameManager.Options.allowXinputControllers && GameManager.Options.allowNonXinputControllers)
			{
				m_selectedIndex = 0;
			}
			else if (!GameManager.Options.allowNonXinputControllers)
			{
				m_selectedIndex = 1;
			}
			else
			{
				m_selectedIndex = 2;
			}
			UpdateSelectedLabelText();
			break;
		case BraveOptionsOptionType.ALLOW_UNKNOWN_CONTROLLERS:
			m_selectedIndex = (GameManager.Options.allowUnknownControllers ? 1 : 0);
			HandleCheckboxValueChanged();
			break;
		}
		DetermineAvailableOptions();
		if (itemType == BraveOptionsMenuItemType.Checkbox)
		{
			labelControl.TextChanged += delegate
			{
				RepositionCheckboxControl();
			};
			dfLabel obj = labelControl;
			obj.LanguageChanged = (Action<dfControl>)Delegate.Combine(obj.LanguageChanged, (Action<dfControl>)delegate
			{
				RepositionCheckboxControl();
			});
			RepositionCheckboxControl();
		}
	}

	private void RepositionCheckboxControl()
	{
		labelControl.AutoSize = true;
		dfPanel component = labelControl.Parent.GetComponent<dfPanel>();
		float num2 = (component.Width = checkboxChecked.Width + 21f + labelControl.Width);
		checkboxChecked.Parent.RelativePosition = checkboxChecked.Parent.RelativePosition.WithX(0f).WithY(6f);
		checkboxChecked.RelativePosition = new Vector3(0f, 0f, 0f);
		checkboxUnchecked.RelativePosition = new Vector3(0f, 0f, 0f);
		labelControl.RelativePosition = labelControl.RelativePosition.WithX(checkboxChecked.Width + 21f);
	}

	private void DoFocus(dfControl control, dfFocusEventArgs args)
	{
		if (labelControl != null)
		{
			labelControl.Color = new Color(1f, 1f, 1f, 1f);
		}
		if (buttonControl != null)
		{
			buttonControl.TextColor = new Color(1f, 1f, 1f, 1f);
		}
		if (optionType == BraveOptionsOptionType.PLAYER_ONE_CONTROL_PORT || optionType == BraveOptionsOptionType.PLAYER_TWO_CONTROL_PORT || optionType == BraveOptionsOptionType.ALLOWED_CONTROLLER_TYPES)
		{
			InControlInputAdapter.CurrentlyUsingAllDevices = true;
			InControlInputAdapter.SkipInputForRestOfFrame = true;
		}
		if (itemType == BraveOptionsMenuItemType.LeftRightArrow || itemType == BraveOptionsMenuItemType.LeftRightArrowInfo)
		{
			left.Color = new Color(1f, 1f, 1f, 1f);
			right.Color = new Color(1f, 1f, 1f, 1f);
			selectedLabelControl.Color = new Color(1f, 1f, 1f, 1f);
		}
		else if (itemType == BraveOptionsMenuItemType.Checkbox)
		{
			checkboxUnchecked.Color = new Color(1f, 1f, 1f, 1f);
			checkboxChecked.Color = new Color(1f, 1f, 1f, 1f);
		}
		if (control.Parent is dfScrollPanel)
		{
			dfScrollPanel dfScrollPanel2 = control.Parent as dfScrollPanel;
			BraveInput bestInputInstance = GetBestInputInstance(GameManager.Instance.LastPausingPlayerID);
			if (bestInputInstance == null || bestInputInstance.ActiveActions == null || Input.anyKeyDown || bestInputInstance.ActiveActions.AnyActionPressed())
			{
				dfScrollPanel2.ScrollIntoView(control);
			}
		}
	}

	private void DoArrowBounce(dfControl targetControl)
	{
		StartCoroutine(HandleArrowBounce(targetControl));
	}

	private IEnumerator HandleArrowBounce(dfControl targetControl)
	{
		float elapsed = 0f;
		float duration = 0.15f;
		while (elapsed < duration)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t = Mathf.PingPong(elapsed / (duration / 2f), 1f);
			targetControl.transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(1.25f, 1.25f, 1.25f), t);
			yield return null;
		}
	}

	private void ArrowReturnScale(dfControl control, dfMouseEventArgs mouseEvent)
	{
		control.transform.localScale = Vector3.one;
	}

	private void ArrowHoverGrow(dfControl control, dfMouseEventArgs mouseEvent)
	{
		control.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
	}

	private void SetUnselectedColors()
	{
		if (labelControl != null)
		{
			labelControl.Color = m_unselectedColor;
		}
		if (buttonControl != null)
		{
			buttonControl.TextColor = m_unselectedColor;
		}
		if (itemType == BraveOptionsMenuItemType.LeftRightArrow || itemType == BraveOptionsMenuItemType.LeftRightArrowInfo)
		{
			left.Color = m_unselectedColor;
			right.Color = m_unselectedColor;
			selectedLabelControl.Color = m_unselectedColor;
		}
		else if (itemType == BraveOptionsMenuItemType.Checkbox)
		{
			checkboxUnchecked.Color = m_unselectedColor;
			checkboxChecked.Color = m_unselectedColor;
		}
	}

	public void LostFocus(dfControl control, dfFocusEventArgs args)
	{
		SetUnselectedColors();
		InControlInputAdapter.CurrentlyUsingAllDevices = false;
		if (optionType == BraveOptionsOptionType.RESOLUTION)
		{
			DoChangeResolution();
		}
		if (optionType != BraveOptionsOptionType.SAVE_SLOT)
		{
			return;
		}
		SaveManager.SaveSlot? targetSaveSlot = SaveManager.TargetSaveSlot;
		if (targetSaveSlot.HasValue)
		{
			if (SaveManager.TargetSaveSlot.Value != SaveManager.CurrentSaveSlot)
			{
				AskToChangeSaveSlot();
			}
			else
			{
				SaveManager.TargetSaveSlot = null;
			}
		}
	}

	private void AskToChangeSaveSlot()
	{
		FullOptionsMenuController OptionsMenu = GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().OptionsMenu;
		OptionsMenu.MainPanel.IsVisible = false;
		GameUIRoot.Instance.DoAreYouSure("#AYS_CHANGESAVESLOT");
		StartCoroutine(WaitForAreYouSure(ChangeSaveSlot, delegate
		{
			m_selectedIndex = (int)SaveManager.CurrentSaveSlot;
			HandleValueChanged();
			SaveManager.TargetSaveSlot = null;
			OptionsMenu.MainPanel.IsVisible = true;
			if ((bool)up)
			{
				up.GetComponent<BraveOptionsMenuItem>().LostFocus(null, null);
			}
			if ((bool)down)
			{
				BraveOptionsMenuItem component = down.GetComponent<BraveOptionsMenuItem>();
				if ((bool)component)
				{
					component.LostFocus(null, null);
				}
				else
				{
					down.Focus();
					down.Unfocus();
				}
			}
			m_self.Focus();
		}));
	}

	private void ChangeSaveSlot()
	{
		GameManager.Instance.LoadMainMenu();
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
			GameOptions.Save();
		}
		else if (OnNo != null)
		{
			OnNo();
		}
	}

	private void DoSelectedAction()
	{
		if (itemType == BraveOptionsMenuItemType.Checkbox)
		{
			ToggleCheckbox(null, null);
		}
		else if (itemType == BraveOptionsMenuItemType.Button)
		{
			FullOptionsMenuController OptionsMenu = GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().OptionsMenu;
			if (optionType == BraveOptionsOptionType.EDIT_KEYBOARD_BINDINGS)
			{
				FullOptionsMenuController.CurrentBindingPlayerTargetIndex = 0;
				OptionsMenu.ToggleToKeyboardBindingsPanel(false);
			}
			else if (optionType == BraveOptionsOptionType.PLAYER_ONE_CONTROLLER_BINDINGS)
			{
				FullOptionsMenuController.CurrentBindingPlayerTargetIndex = 0;
				OptionsMenu.ToggleToKeyboardBindingsPanel(true);
			}
			else if (optionType == BraveOptionsOptionType.PLAYER_TWO_CONTROLLER_BINDINGS)
			{
				FullOptionsMenuController.CurrentBindingPlayerTargetIndex = ((GameManager.Instance.AllPlayers.Length > 1) ? GameManager.Instance.SecondaryPlayer.PlayerIDX : 0);
				OptionsMenu.ToggleToKeyboardBindingsPanel(true);
			}
			else if (optionType == BraveOptionsOptionType.VIEW_CREDITS)
			{
				OptionsMenu.ToggleToCredits();
			}
			else if (optionType == BraveOptionsOptionType.VIEW_PRIVACY)
			{
				DataPrivacy.FetchPrivacyUrl(delegate(string url)
				{
					Application.OpenURL(url);
				});
			}
			else if (optionType == BraveOptionsOptionType.HOW_TO_PLAY)
			{
				OptionsMenu.ToggleToHowToPlay();
			}
			else
			{
				if (optionType != BraveOptionsOptionType.RESET_SAVE_SLOT)
				{
					return;
				}
				OptionsMenu.MainPanel.IsVisible = false;
				GameUIRoot.Instance.DoAreYouSure("#AYS_RESETSAVESLOT", false, "#AYS_RESETSAVESLOT2");
				StartCoroutine(WaitForAreYouSure(delegate
				{
					GameUIRoot.Instance.DoAreYouSure("#AREYOUSURE");
					StartCoroutine(WaitForAreYouSure(delegate
					{
						SaveManager.ResetSaveSlot = true;
						GameManager.Instance.LoadMainMenu();
					}, delegate
					{
						OptionsMenu.MainPanel.IsVisible = true;
						m_self.Focus();
					}));
				}, delegate
				{
					OptionsMenu.MainPanel.IsVisible = true;
					m_self.Focus();
				}));
			}
		}
		else if (optionType == BraveOptionsOptionType.RESOLUTION)
		{
			DoChangeResolution();
		}
		else if (optionType == BraveOptionsOptionType.SAVE_SLOT)
		{
			SaveManager.SaveSlot? targetSaveSlot = SaveManager.TargetSaveSlot;
			if (targetSaveSlot.HasValue && SaveManager.TargetSaveSlot.Value != SaveManager.CurrentSaveSlot)
			{
				AskToChangeSaveSlot();
			}
		}
	}

	private void IncrementArrow(dfControl control, dfMouseEventArgs mouseEvent)
	{
		AkSoundEngine.PostEvent("Play_UI_menu_select_01", base.gameObject);
		m_selectedIndex = (m_selectedIndex + 1) % labelOptions.Length;
		HandleValueChanged();
	}

	private void DecrementArrow(dfControl control, dfMouseEventArgs mouseEvent)
	{
		AkSoundEngine.PostEvent("Play_UI_menu_select_01", base.gameObject);
		m_selectedIndex = (m_selectedIndex - 1 + labelOptions.Length) % labelOptions.Length;
		HandleValueChanged();
	}

	private void HandleValueChanged()
	{
		switch (itemType)
		{
		case BraveOptionsMenuItemType.Checkbox:
			HandleCheckboxValueChanged();
			break;
		case BraveOptionsMenuItemType.Fillbar:
			HandleFillbarValueChanged();
			break;
		case BraveOptionsMenuItemType.LeftRightArrow:
			HandleLeftRightArrowValueChanged();
			break;
		case BraveOptionsMenuItemType.LeftRightArrowInfo:
			HandleLeftRightArrowValueChanged();
			break;
		}
		if ((bool)selectedLabelControl)
		{
			selectedLabelControl.PerformLayout();
		}
		if ((bool)infoControl)
		{
			infoControl.PerformLayout();
		}
	}

	private void HandleCheckboxValueChanged()
	{
		checkboxChecked.IsVisible = m_selectedIndex == 1;
		checkboxUnchecked.IsVisible = m_selectedIndex != 1;
		switch (optionType)
		{
		case BraveOptionsOptionType.VSYNC:
			GameManager.Options.DoVsync = m_selectedIndex == 1;
			break;
		case BraveOptionsOptionType.BEASTMODE:
			GameManager.Options.m_beastmode = m_selectedIndex == 1;
			break;
		case BraveOptionsOptionType.SPEEDRUN:
			GameManager.Options.SpeedrunMode = m_selectedIndex == 1;
			break;
		case BraveOptionsOptionType.RUMBLE:
			GameManager.Options.RumbleEnabled = m_selectedIndex == 1;
			break;
		case BraveOptionsOptionType.SMALL_UI:
			GameManager.Options.SmallUIEnabled = m_selectedIndex == 1;
			break;
		case BraveOptionsOptionType.REALTIME_REFLECTIONS:
			GameManager.Options.RealtimeReflections = m_selectedIndex == 1;
			break;
		case BraveOptionsOptionType.HIDE_EMPTY_GUNS:
			GameManager.Options.HideEmptyGuns = m_selectedIndex == 1;
			break;
		case BraveOptionsOptionType.QUICKSELECT:
			GameManager.Options.QuickSelectEnabled = m_selectedIndex == 1;
			break;
		case BraveOptionsOptionType.COOP_SCREEN_SHAKE_AMOUNT:
			GameManager.Options.CoopScreenShakeReduction = m_selectedIndex == 1;
			break;
		case BraveOptionsOptionType.PLAYER_ONE_CONTROLLER_CURSOR:
			GameManager.Options.PlayerOneControllerCursor = m_selectedIndex == 1;
			break;
		case BraveOptionsOptionType.PLAYER_TWO_CONTROLLER_CURSOR:
			GameManager.Options.PlayerTwoControllerCursor = m_selectedIndex == 1;
			break;
		case BraveOptionsOptionType.BOTH_CONTROLLER_CURSOR:
			GameManager.Options.PlayerOneControllerCursor = m_selectedIndex == 1;
			GameManager.Options.PlayerTwoControllerCursor = m_selectedIndex == 1;
			break;
		case BraveOptionsOptionType.ALLOW_UNKNOWN_CONTROLLERS:
			GameManager.Options.allowUnknownControllers = m_selectedIndex == 1;
			break;
		case BraveOptionsOptionType.OUT_OF_COMBAT_SPEED_INCREASE:
			GameManager.Options.IncreaseSpeedOutOfCombat = m_selectedIndex == 1;
			break;
		case BraveOptionsOptionType.CONTROLLER_BEAM_AIM_ASSIST:
			GameManager.Options.controllerBeamAimAssist = m_selectedIndex == 1;
			break;
		}
	}

	private List<Resolution> GetAvailableResolutions()
	{
		if (GameManager.Options.CurrentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.BORDERLESS)
		{
			return new List<Resolution>(new Resolution[1] { Screen.currentResolution });
		}
		List<Resolution> list = new List<Resolution>();
		Resolution[] resolutions = Screen.resolutions;
		int refreshRate = Screen.currentResolution.refreshRate;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution resolution = resolutions[i];
			AddResolutionInOrder(list, new Resolution
			{
				width = resolution.width,
				height = resolution.height,
				refreshRate = refreshRate
			});
		}
		if (GameManager.Options.CurrentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.WINDOWED || Application.platform == RuntimePlatform.OSXPlayer)
		{
			AddResolutionInOrder(list, new Resolution
			{
				width = Screen.width,
				height = Screen.height,
				refreshRate = Screen.currentResolution.refreshRate
			});
		}
		if (GameManager.Options.CurrentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.WINDOWED)
		{
			int num = 0;
			int num2 = 0;
			if (list.Count > 0)
			{
				num = list[list.Count - 1].width;
				num2 = list[list.Count - 1].height;
			}
			int num3 = 480;
			int num4 = 270;
			while (num3 <= num && num4 <= num2)
			{
				AddResolutionInOrder(list, new Resolution
				{
					width = num3,
					height = num4,
					refreshRate = refreshRate
				});
				num3 += 480;
				num4 += 270;
			}
		}
		return list;
	}

	private void AddResolutionInOrder(List<Resolution> resolutions, Resolution newResolution)
	{
		for (int i = 0; i < resolutions.Count; i++)
		{
			if (resolutions[i].width == newResolution.width && resolutions[i].height == newResolution.height)
			{
				return;
			}
			if (resolutions[i].width > newResolution.width || (resolutions[i].width == newResolution.width && resolutions[i].height > newResolution.height))
			{
				resolutions.Insert(i, newResolution);
				return;
			}
		}
		resolutions.Add(newResolution);
	}

	private void DoChangeResolution()
	{
		List<Resolution> availableResolutions = GetAvailableResolutions();
		m_selectedIndex = Mathf.Clamp(m_selectedIndex, 0, availableResolutions.Count - 1);
		if (availableResolutions[m_selectedIndex].width != Screen.width || availableResolutions[m_selectedIndex].height != Screen.height)
		{
			GameManager.Options.CurrentVisualPreset = GameOptions.VisualPresetMode.CUSTOM;
			HandleScreenDataChanged(availableResolutions[m_selectedIndex].width, availableResolutions[m_selectedIndex].height);
		}
	}

	private BraveInput GetBestInputInstance(int targetPlayerIndex)
	{
		BraveInput braveInput = null;
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER && Foyer.DoMainMenu)
		{
			return BraveInput.PlayerlessInstance;
		}
		if (targetPlayerIndex == -1)
		{
			return BraveInput.PrimaryPlayerInstance;
		}
		return BraveInput.GetInstanceForPlayer(targetPlayerIndex);
	}

	private void HandleLeftRightArrowValueChanged()
	{
		UpdateSelectedLabelText();
		switch (optionType)
		{
		case BraveOptionsOptionType.CURRENT_BINDINGS_PRESET:
		{
			FullOptionsMenuController optionsMenu2 = GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().OptionsMenu;
			GameManager.Options.CurrentControlPreset = (GameOptions.ControlPreset)m_selectedIndex;
			optionsMenu2.ReinitializeKeyboardBindings();
			selectedLabelControl.PerformLayout();
			break;
		}
		case BraveOptionsOptionType.CURRENT_BINDINGS_PRESET_P2:
		{
			FullOptionsMenuController optionsMenu = GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().OptionsMenu;
			GameManager.Options.CurrentControlPresetP2 = (GameOptions.ControlPreset)m_selectedIndex;
			optionsMenu.ReinitializeKeyboardBindings();
			selectedLabelControl.PerformLayout();
			break;
		}
		case BraveOptionsOptionType.SPEAKER_TYPE:
			GameManager.Options.AudioHardware = (GameOptions.AudioHardwareMode)m_selectedIndex;
			break;
		case BraveOptionsOptionType.VISUAL_PRESET:
			GameManager.Options.CurrentVisualPreset = (GameOptions.VisualPresetMode)m_selectedIndex;
			break;
		case BraveOptionsOptionType.SCALING_MODE:
			GameManager.Options.CurrentVisualPreset = GameOptions.VisualPresetMode.CUSTOM;
			if (m_scalingModes[m_selectedIndex] == GameOptions.PreferredScalingMode.PIXEL_PERFECT)
			{
				GameManager.Options.CurrentPreferredScalingMode = GameOptions.PreferredScalingMode.FORCE_PIXEL_PERFECT;
			}
			else if (m_scalingModes[m_selectedIndex] == GameOptions.PreferredScalingMode.UNIFORM_SCALING)
			{
				GameManager.Options.CurrentPreferredScalingMode = GameOptions.PreferredScalingMode.UNIFORM_SCALING;
			}
			else if (m_scalingModes[m_selectedIndex] == GameOptions.PreferredScalingMode.UNIFORM_SCALING_FAST)
			{
				GameManager.Options.CurrentPreferredScalingMode = GameOptions.PreferredScalingMode.UNIFORM_SCALING_FAST;
			}
			else if (m_scalingModes[m_selectedIndex] == GameOptions.PreferredScalingMode.FORCE_PIXEL_PERFECT)
			{
				GameManager.Options.CurrentPreferredScalingMode = GameOptions.PreferredScalingMode.FORCE_PIXEL_PERFECT;
			}
			HandleScreenDataChanged(Screen.width, Screen.height);
			break;
		case BraveOptionsOptionType.FULLSCREEN:
			GameManager.Options.CurrentVisualPreset = GameOptions.VisualPresetMode.CUSTOM;
			GameManager.Options.CurrentPreferredFullscreenMode = ((m_selectedIndex != 0) ? ((m_selectedIndex == 1) ? GameOptions.PreferredFullscreenMode.BORDERLESS : GameOptions.PreferredFullscreenMode.WINDOWED) : GameOptions.PreferredFullscreenMode.FULLSCREEN);
			HandleScreenDataChanged(Screen.width, Screen.height);
			break;
		case BraveOptionsOptionType.MONITOR_SELECT:
		{
			GameManager.Options.CurrentMonitorIndex = m_selectedIndex;
			PlayerPrefs.SetInt("UnitySelectMonitor", m_selectedIndex);
			DoChangeResolution();
			Resolution recommendedResolution = GameManager.Options.GetRecommendedResolution();
			if (Screen.width != recommendedResolution.width || Screen.height != recommendedResolution.height)
			{
				HandleScreenDataChanged(recommendedResolution.width, recommendedResolution.height);
			}
			break;
		}
		case BraveOptionsOptionType.LIGHTING_QUALITY:
			GameManager.Options.LightingQuality = ((m_selectedIndex == 0) ? GameOptions.GenericHighMedLowOption.HIGH : GameOptions.GenericHighMedLowOption.LOW);
			break;
		case BraveOptionsOptionType.QUICKSTART_CHARACTER:
			if (m_selectedIndex >= 0 && m_selectedIndex < m_quickStartCharacters.Count)
			{
				GameManager.Options.PreferredQuickstartCharacter = m_quickStartCharacters[m_selectedIndex];
			}
			else
			{
				GameManager.Options.PreferredQuickstartCharacter = GameOptions.QuickstartCharacter.LAST_USED;
			}
			break;
		case BraveOptionsOptionType.ADDITIONAL_BLANK_CONTROL:
			GameManager.Options.additionalBlankControl = (GameOptions.ControllerBlankControl)m_selectedIndex;
			break;
		case BraveOptionsOptionType.ADDITIONAL_BLANK_CONTROL_TWO:
			GameManager.Options.additionalBlankControlTwo = (GameOptions.ControllerBlankControl)m_selectedIndex;
			break;
		case BraveOptionsOptionType.ADDITIONAL_BLANK_CONTROL_PS4:
			if (FullOptionsMenuController.CurrentBindingPlayerTargetIndex == 0)
			{
				GameManager.Options.additionalBlankControl = (GameOptions.ControllerBlankControl)m_selectedIndex;
			}
			else
			{
				GameManager.Options.additionalBlankControlTwo = (GameOptions.ControllerBlankControl)m_selectedIndex;
			}
			break;
		case BraveOptionsOptionType.CURSOR_VARIATION:
			GameManager.Options.CurrentCursorIndex = m_selectedIndex;
			break;
		case BraveOptionsOptionType.SHADER_QUALITY:
			if (m_selectedIndex == 0)
			{
				GameManager.Options.ShaderQuality = GameOptions.GenericHighMedLowOption.HIGH;
			}
			if (m_selectedIndex == 1)
			{
				GameManager.Options.ShaderQuality = GameOptions.GenericHighMedLowOption.VERY_LOW;
			}
			if (m_selectedIndex == 2)
			{
				GameManager.Options.ShaderQuality = GameOptions.GenericHighMedLowOption.LOW;
			}
			if (m_selectedIndex == 3)
			{
				GameManager.Options.ShaderQuality = GameOptions.GenericHighMedLowOption.MEDIUM;
			}
			if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.LOW || GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.VERY_LOW)
			{
				GameManager.Options.RealtimeReflections = false;
			}
			break;
		case BraveOptionsOptionType.DEBRIS_QUANTITY:
			if (m_selectedIndex == 0)
			{
				GameManager.Options.DebrisQuantity = GameOptions.GenericHighMedLowOption.HIGH;
			}
			if (m_selectedIndex == 1)
			{
				GameManager.Options.DebrisQuantity = GameOptions.GenericHighMedLowOption.VERY_LOW;
			}
			if (m_selectedIndex == 2)
			{
				GameManager.Options.DebrisQuantity = GameOptions.GenericHighMedLowOption.LOW;
			}
			if (m_selectedIndex == 3)
			{
				GameManager.Options.DebrisQuantity = GameOptions.GenericHighMedLowOption.MEDIUM;
			}
			break;
		case BraveOptionsOptionType.LANGUAGE:
			GameManager.Options.CurrentLanguage = IntToLanguage(m_selectedIndex);
			break;
		case BraveOptionsOptionType.SAVE_SLOT:
			if (m_selectedIndex == 0)
			{
				SaveManager.TargetSaveSlot = SaveManager.SaveSlot.A;
			}
			if (m_selectedIndex == 1)
			{
				SaveManager.TargetSaveSlot = SaveManager.SaveSlot.B;
			}
			if (m_selectedIndex == 2)
			{
				SaveManager.TargetSaveSlot = SaveManager.SaveSlot.C;
			}
			if (m_selectedIndex == 3)
			{
				SaveManager.TargetSaveSlot = SaveManager.SaveSlot.D;
			}
			break;
		case BraveOptionsOptionType.TEXT_SPEED:
			if (m_selectedIndex == 0)
			{
				GameManager.Options.TextSpeed = GameOptions.GenericHighMedLowOption.MEDIUM;
			}
			if (m_selectedIndex == 1)
			{
				GameManager.Options.TextSpeed = GameOptions.GenericHighMedLowOption.HIGH;
			}
			if (m_selectedIndex == 2)
			{
				GameManager.Options.TextSpeed = GameOptions.GenericHighMedLowOption.LOW;
			}
			break;
		case BraveOptionsOptionType.PLAYER_ONE_CONTROL_PORT:
			BraveInput.ReassignPlayerPort(0, m_selectedIndex);
			m_ignoreLeftRightUntilReleased = true;
			break;
		case BraveOptionsOptionType.PLAYER_TWO_CONTROL_PORT:
			if (GameManager.Instance.AllPlayers.Length > 1)
			{
				BraveInput.ReassignPlayerPort(GameManager.Instance.SecondaryPlayer.PlayerIDX, m_selectedIndex);
			}
			break;
		case BraveOptionsOptionType.PLAYER_ONE_CONTROLLER_SYMBOLOGY:
			if (m_selectedIndex == 0)
			{
				GameManager.Options.PlayerOnePreferredSymbology = GameOptions.ControllerSymbology.PS4;
			}
			if (m_selectedIndex == 1)
			{
				GameManager.Options.PlayerOnePreferredSymbology = GameOptions.ControllerSymbology.Xbox;
			}
			if (m_selectedIndex == 2)
			{
				GameManager.Options.PlayerOnePreferredSymbology = GameOptions.ControllerSymbology.AutoDetect;
			}
			if (m_selectedIndex == 3)
			{
				GameManager.Options.PlayerOnePreferredSymbology = GameOptions.ControllerSymbology.Switch;
			}
			break;
		case BraveOptionsOptionType.PLAYER_TWO_CONTROLLER_SYMBOLOGY:
			if (m_selectedIndex == 0)
			{
				GameManager.Options.PlayerTwoPreferredSymbology = GameOptions.ControllerSymbology.PS4;
			}
			if (m_selectedIndex == 1)
			{
				GameManager.Options.PlayerTwoPreferredSymbology = GameOptions.ControllerSymbology.Xbox;
			}
			if (m_selectedIndex == 2)
			{
				GameManager.Options.PlayerTwoPreferredSymbology = GameOptions.ControllerSymbology.AutoDetect;
			}
			if (m_selectedIndex == 3)
			{
				GameManager.Options.PlayerOnePreferredSymbology = GameOptions.ControllerSymbology.Switch;
			}
			break;
		case BraveOptionsOptionType.MINIMAP_STYLE:
			if (m_selectedIndex == 0)
			{
				GameManager.Options.MinimapDisplayMode = Minimap.MinimapDisplayMode.FADE_ON_ROOM_SEAL;
			}
			if (m_selectedIndex == 1)
			{
				GameManager.Options.MinimapDisplayMode = Minimap.MinimapDisplayMode.NEVER;
			}
			if (m_selectedIndex == 2)
			{
				GameManager.Options.MinimapDisplayMode = Minimap.MinimapDisplayMode.ALWAYS;
			}
			break;
		case BraveOptionsOptionType.AUTOAIM:
			if (m_selectedIndex == 0)
			{
				GameManager.Options.controllerAutoAim = GameOptions.ControllerAutoAim.AUTO_DETECT;
			}
			if (m_selectedIndex == 1)
			{
				GameManager.Options.controllerAutoAim = GameOptions.ControllerAutoAim.ALWAYS;
			}
			if (m_selectedIndex == 2)
			{
				GameManager.Options.controllerAutoAim = GameOptions.ControllerAutoAim.NEVER;
			}
			if (m_selectedIndex == 3)
			{
				GameManager.Options.controllerAutoAim = GameOptions.ControllerAutoAim.COOP_ONLY;
			}
			break;
		case BraveOptionsOptionType.LOOT_PROFILE:
			if (m_selectedIndex == 0)
			{
				GameManager.Options.CurrentGameLootProfile = GameOptions.GameLootProfile.CURRENT;
			}
			if (m_selectedIndex == 1)
			{
				GameManager.Options.CurrentGameLootProfile = GameOptions.GameLootProfile.ORIGINAL;
			}
			break;
		case BraveOptionsOptionType.ALLOWED_CONTROLLER_TYPES:
			if (m_selectedIndex == 0)
			{
				GameManager.Options.allowXinputControllers = true;
				GameManager.Options.allowNonXinputControllers = true;
			}
			if (m_selectedIndex == 1)
			{
				GameManager.Options.allowXinputControllers = true;
				GameManager.Options.allowNonXinputControllers = false;
			}
			if (m_selectedIndex == 2)
			{
				GameManager.Options.allowXinputControllers = false;
				GameManager.Options.allowNonXinputControllers = true;
			}
			InControlInputAdapter.SkipInputForRestOfFrame = true;
			break;
		}
		if (itemType == BraveOptionsMenuItemType.LeftRightArrowInfo)
		{
			UpdateInfoControl();
		}
	}

	private static void HandleScreenDataChanged(int screenWidth, int screenHeight)
	{
		if (!GameManager.Instance.IsLoadingLevel && !GameManager.IsReturningToBreach)
		{
			GameOptions.PreferredFullscreenMode currentPreferredFullscreenMode = GameManager.Options.CurrentPreferredFullscreenMode;
			Resolution resolution = default(Resolution);
			if (currentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.BORDERLESS)
			{
				screenWidth = Screen.currentResolution.width;
				screenHeight = Screen.currentResolution.height;
			}
			GameManager.Options.preferredResolutionX = screenWidth;
			GameManager.Options.preferredResolutionY = screenHeight;
			resolution.width = screenWidth;
			resolution.height = screenHeight;
			resolution.refreshRate = Screen.currentResolution.refreshRate;
			WindowsResolutionManager.DisplayModes targetDisplayMode = WindowsResolutionManager.DisplayModes.Fullscreen;
			if (currentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.BORDERLESS)
			{
				targetDisplayMode = WindowsResolutionManager.DisplayModes.Borderless;
			}
			if (currentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.WINDOWED)
			{
				targetDisplayMode = WindowsResolutionManager.DisplayModes.Windowed;
			}
			if (Screen.fullScreen != (currentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.FULLSCREEN))
			{
				BraveOptionsMenuItem componentInChildren = GameUIRoot.Instance.PauseMenuPanel.GetComponent<PauseMenuController>().OptionsMenu.GetComponentInChildren<BraveOptionsMenuItem>();
				componentInChildren.StartCoroutine(componentInChildren.FrameDelayedWindowsShift(targetDisplayMode, resolution));
			}
			else
			{
				ResolutionManagerWin.TrySetDisplay(targetDisplayMode, resolution, false, null);
			}
			if (screenWidth != Screen.width || screenHeight != Screen.height || currentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.FULLSCREEN != Screen.fullScreen)
			{
				Debug.Log("BOMI setting resolution to: " + screenWidth + "|" + screenHeight + "||" + currentPreferredFullscreenMode.ToString());
				GameManager.Instance.DoSetResolution(screenWidth, screenHeight, currentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.FULLSCREEN);
			}
		}
	}

	public IEnumerator FrameDelayedWindowsShift(WindowsResolutionManager.DisplayModes targetDisplayMode, Resolution targetRes)
	{
		yield return null;
		ResolutionManagerWin.TrySetDisplay(targetDisplayMode, targetRes, false, null);
		yield return null;
	}

	private void HandleFillbarValueChanged()
	{
		switch (optionType)
		{
		case BraveOptionsOptionType.MUSIC_VOLUME:
			GameManager.Options.MusicVolume = Mathf.Clamp(m_actualFillbarValue * 100f, 0f, 100f);
			break;
		case BraveOptionsOptionType.SOUND_VOLUME:
			GameManager.Options.SoundVolume = Mathf.Clamp(m_actualFillbarValue * 100f, 0f, 100f);
			break;
		case BraveOptionsOptionType.UI_VOLUME:
			GameManager.Options.UIVolume = Mathf.Clamp(m_actualFillbarValue * 100f, 0f, 100f);
			break;
		case BraveOptionsOptionType.GAMMA:
			GameManager.Options.Gamma = Mathf.Clamp(m_actualFillbarValue + 0.5f, 0.5f, 1.5f);
			break;
		case BraveOptionsOptionType.DISPLAY_SAFE_AREA:
			GameManager.Options.DisplaySafeArea = Mathf.Clamp(BraveMathCollege.QuantizeFloat(m_actualFillbarValue, 0.2f) * 0.1f + 0.9f, 0.9f, 1f);
			break;
		case BraveOptionsOptionType.SCREEN_SHAKE_AMOUNT:
			GameManager.Options.ScreenShakeMultiplier = m_actualFillbarValue / 0.5f;
			break;
		case BraveOptionsOptionType.STICKY_FRICTION_AMOUNT:
			GameManager.Options.StickyFrictionMultiplier = m_actualFillbarValue / 0.8f;
			break;
		case BraveOptionsOptionType.CONTROLLER_AIM_ASSIST_AMOUNT:
			GameManager.Options.controllerAimAssistMultiplier = Mathf.Clamp(m_actualFillbarValue / 0.8f, 0f, 1.25f);
			break;
		case BraveOptionsOptionType.CONTROLLER_AIM_LOOK:
			GameManager.Options.controllerAimLookMultiplier = Mathf.Clamp(m_actualFillbarValue / 0.8f, 0f, 1.25f);
			break;
		}
	}

	public void OnKeyUp(dfControl sender, dfKeyEventArgs args)
	{
		if (!args.Used)
		{
			if (args.KeyCode == KeyCode.LeftArrow)
			{
				m_ignoreLeftRightUntilReleased = false;
			}
			else if (args.KeyCode == KeyCode.RightArrow)
			{
				m_ignoreLeftRightUntilReleased = false;
			}
		}
	}

	public void OnKeyDown(dfControl sender, dfKeyEventArgs args)
	{
		if (args.Used)
		{
			return;
		}
		if (args.KeyCode == KeyCode.UpArrow && (bool)up)
		{
			if (optionType == BraveOptionsOptionType.RESOLUTION)
			{
				DoChangeResolution();
			}
			if (OnNewControlSelected != null)
			{
				OnNewControlSelected(up);
			}
			up.Focus();
		}
		else if (args.KeyCode == KeyCode.DownArrow && (bool)down)
		{
			if (optionType == BraveOptionsOptionType.RESOLUTION)
			{
				DoChangeResolution();
			}
			if (OnNewControlSelected != null)
			{
				OnNewControlSelected(down);
			}
			down.Focus();
		}
		else if (args.KeyCode == KeyCode.LeftArrow)
		{
			if (itemType == BraveOptionsMenuItemType.Fillbar)
			{
				m_actualFillbarValue = Mathf.Clamp01(m_actualFillbarValue - FillbarDelta);
				fillbarControl.Value = m_actualFillbarValue * 0.98f + 0.01f;
				HandleValueChanged();
			}
			else if (itemType == BraveOptionsMenuItemType.LeftRightArrow)
			{
				if (!m_ignoreLeftRightUntilReleased)
				{
					DecrementArrow(null, null);
					DoArrowBounce(left);
				}
			}
			else if (itemType == BraveOptionsMenuItemType.LeftRightArrowInfo)
			{
				if (!m_ignoreLeftRightUntilReleased)
				{
					DecrementArrow(null, null);
					DoArrowBounce(left);
				}
			}
			else if ((bool)left)
			{
				if (OnNewControlSelected != null)
				{
					OnNewControlSelected(left);
				}
				left.Focus();
			}
		}
		else if (args.KeyCode == KeyCode.RightArrow)
		{
			if (itemType == BraveOptionsMenuItemType.Fillbar)
			{
				m_actualFillbarValue = Mathf.Clamp01(m_actualFillbarValue + FillbarDelta);
				fillbarControl.Value = m_actualFillbarValue * 0.98f + 0.01f;
				HandleValueChanged();
			}
			else if (itemType == BraveOptionsMenuItemType.LeftRightArrow)
			{
				if (!m_ignoreLeftRightUntilReleased)
				{
					IncrementArrow(null, null);
					DoArrowBounce(right);
				}
			}
			else if (itemType == BraveOptionsMenuItemType.LeftRightArrowInfo)
			{
				if (!m_ignoreLeftRightUntilReleased)
				{
					IncrementArrow(null, null);
					DoArrowBounce(right);
				}
			}
			else if ((bool)right)
			{
				if (OnNewControlSelected != null)
				{
					OnNewControlSelected(right);
				}
				right.Focus();
			}
		}
		if (selectOnAction && args.KeyCode == KeyCode.Return)
		{
			DoSelectedAction();
			args.Use();
		}
	}

	public void UpdateLabelOptions(int playerIndex)
	{
		switch (optionType)
		{
		case BraveOptionsOptionType.CURRENT_BINDINGS_PRESET:
		case BraveOptionsOptionType.CURRENT_BINDINGS_PRESET_P2:
			labelOptions = new string[3]
			{
				selectedLabelControl.ForceGetLocalizedValue("#OPTIONS_RECOMMENDED") + " 1",
				selectedLabelControl.ForceGetLocalizedValue("#OPTIONS_RECOMMENDED") + " 2",
				selectedLabelControl.ForceGetLocalizedValue("#OPTIONS_CUSTOM")
			};
			if (GameManager.Options.CurrentLanguage == StringTableManager.GungeonSupportedLanguages.ENGLISH)
			{
				labelOptions[0] = "Recommended";
				labelOptions[1] = "Flipped Triggers";
			}
			break;
		case BraveOptionsOptionType.ADDITIONAL_BLANK_CONTROL_PS4:
			labelOptions = new string[2]
			{
				selectedLabelControl.ForceGetLocalizedValue("#OPTIONS_NONE"),
				"%CONTROL_L_STICK_DOWN %CONTROL_R_STICK_DOWN"
			};
			break;
		}
	}
}
