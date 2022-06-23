using System;
using System.Collections.Generic;
using System.Reflection;
using Brave;
using FullSerializer;
using UnityEngine;

public class GameOptions
{
	public enum ControllerBlankControl
	{
		NONE,
		BOTH_STICKS_DOWN,
		[Obsolete("Players should only see NONE and BOTH_STICKS_DOWN; this is kept for legacy conversions only.")]
		CIRCLE,
		[Obsolete("Players should only see NONE and BOTH_STICKS_DOWN; this is kept for legacy conversions only.")]
		L1
	}

	public enum ControllerAutoAim
	{
		AUTO_DETECT,
		ALWAYS,
		NEVER,
		COOP_ONLY
	}

	public enum QuickstartCharacter
	{
		LAST_USED,
		PILOT,
		CONVICT,
		SOLDIER,
		GUIDE,
		BULLET,
		ROBOT
	}

	public enum AudioHardwareMode
	{
		SPEAKERS,
		HEADPHONES
	}

	public enum PixelatorMotionEnhancementMode
	{
		ENHANCED_EXPENSIVE,
		UNENHANCED_CHEAP
	}

	public enum GameLootProfile
	{
		CURRENT = 0,
		ORIGINAL = 5
	}

	public enum FullscreenStyle
	{
		FULLSCREEN,
		BORDERLESS,
		WINDOWED
	}

	public enum VisualPresetMode
	{
		RECOMMENDED,
		CUSTOM
	}

	public enum PreferredScalingMode
	{
		PIXEL_PERFECT,
		UNIFORM_SCALING,
		FORCE_PIXEL_PERFECT,
		UNIFORM_SCALING_FAST
	}

	public enum PreferredFullscreenMode
	{
		FULLSCREEN,
		BORDERLESS,
		WINDOWED
	}

	public enum ControlPreset
	{
		RECOMMENDED,
		FLIPPED_TRIGGERS,
		CUSTOM
	}

	public enum GenericHighMedLowOption
	{
		LOW,
		MEDIUM,
		HIGH,
		VERY_LOW
	}

	public enum ControllerSymbology
	{
		PS4,
		Xbox,
		AutoDetect,
		Switch
	}

	private static bool? m_cachedSupportsStencil;

	[fsIgnore]
	private GenericHighMedLowOption? m_DefaultRecommendedQuality;

	[fsProperty]
	public bool SLOW_TIME_ON_CHALLENGE_MODE_REVEAL = true;

	[fsProperty]
	private float m_gamma = 1f;

	[fsProperty]
	public float DisplaySafeArea = 1f;

	[fsProperty]
	public ControllerBlankControl additionalBlankControl = ControllerBlankControl.BOTH_STICKS_DOWN;

	[fsProperty]
	public ControllerBlankControl additionalBlankControlTwo = ControllerBlankControl.BOTH_STICKS_DOWN;

	[fsIgnore]
	public bool OverrideMotionEnhancementModeForPause;

	[fsIgnore]
	public Dictionary<int, int> PlayerIDtoDeviceIndexMap = new Dictionary<int, int>();

	[fsIgnore]
	public static bool RequiresLanguageReinitialization;

	[fsProperty]
	public QuickstartCharacter PreferredQuickstartCharacter;

	[fsProperty]
	public PlayableCharacters LastPlayedCharacter;

	[fsProperty]
	public GameLootProfile CurrentGameLootProfile;

	[fsProperty]
	public bool IncreaseSpeedOutOfCombat;

	[fsProperty]
	private FullscreenStyle m_fullscreenStyle;

	[fsIgnore]
	public int CurrentMonitorIndex;

	[fsProperty]
	private int m_currentCursorIndex;

	[fsProperty]
	private VisualPresetMode m_visualPresetMode;

	[fsProperty]
	private ControlPreset m_currentControlPreset;

	[fsProperty]
	private ControlPreset m_currentControlPresetP2;

	[fsProperty]
	private StringTableManager.GungeonSupportedLanguages m_currentLanguage;

	[fsProperty]
	private bool m_doVsync = true;

	[fsProperty]
	private GenericHighMedLowOption m_lightingQuality = GenericHighMedLowOption.HIGH;

	[fsProperty]
	public bool QuickSelectEnabled = true;

	[fsProperty]
	public bool HideEmptyGuns = true;

	[fsProperty]
	private GenericHighMedLowOption m_shaderQuality = GenericHighMedLowOption.HIGH;

	[fsProperty]
	private bool m_realtimeReflections = true;

	[fsProperty]
	private GenericHighMedLowOption m_debrisQuantity = GenericHighMedLowOption.HIGH;

	[fsProperty]
	private GenericHighMedLowOption m_textSpeed = GenericHighMedLowOption.MEDIUM;

	[fsProperty]
	private float m_screenShakeMultiplier = 1f;

	[fsProperty]
	private bool m_coopScreenShakeReduction = true;

	[fsProperty]
	private float m_stickyFrictionMultiplier = 1f;

	[fsProperty]
	public bool HasEverSeenAmmonomicon;

	[fsProperty]
	public bool SpeedrunMode;

	[fsProperty]
	public bool RumbleEnabled = true;

	[fsProperty]
	public bool SmallUIEnabled;

	[fsProperty]
	public bool m_beastmode;

	[fsProperty]
	public bool mouseAimLook = true;

	[fsProperty]
	public bool SuperSmoothCamera;

	[fsProperty]
	public bool DisplaySpeedrunCentiseconds;

	[fsProperty]
	public bool DisableQuickGunKeys;

	[fsProperty]
	public bool AllowMoveKeysToChangeGuns;

	[fsProperty]
	public bool autofaceMovementDirection = true;

	[fsProperty]
	public float controllerAimLookMultiplier = 1f;

	[fsProperty]
	public ControllerAutoAim controllerAutoAim;

	[fsProperty]
	public float controllerAimAssistMultiplier = 1f;

	[fsProperty]
	public bool controllerBeamAimAssist;

	[fsProperty]
	public bool allowXinputControllers = true;

	[fsProperty]
	public bool allowNonXinputControllers = true;

	[fsProperty]
	public bool allowUnknownControllers;

	[fsProperty(DeserializeOnly = true)]
	public bool wipeAllAchievements;

	[fsProperty(DeserializeOnly = true)]
	public bool scanAchievementsForUnlocks;

	[fsProperty]
	public int preferredResolutionX = -1;

	[fsProperty]
	public int preferredResolutionY = -1;

	[fsProperty]
	private ControllerSymbology m_playerOnePreferredSymbology = ControllerSymbology.AutoDetect;

	[fsProperty]
	private ControllerSymbology m_playerTwoPreferredSymbology = ControllerSymbology.AutoDetect;

	[fsProperty]
	private bool m_playerOneControllerCursor;

	[fsProperty]
	private bool m_playerTwoControllerCursor;

	[fsProperty]
	private PreferredScalingMode m_preferredScalingMode = PreferredScalingMode.FORCE_PIXEL_PERFECT;

	[fsProperty]
	private PreferredFullscreenMode m_preferredFullscreenMode;

	[fsProperty]
	public float PreferredMapZoom;

	[fsProperty]
	public float PreferredMinimapZoom = 2f;

	[fsProperty]
	private Minimap.MinimapDisplayMode m_minimapDisplayMode = Minimap.MinimapDisplayMode.FADE_ON_ROOM_SEAL;

	[fsProperty]
	private AudioHardwareMode m_audioHardware;

	[fsProperty]
	private float m_musicVolume = 80f;

	[fsProperty]
	private float m_soundVolume = 80f;

	[fsProperty]
	private float m_uiVolume = 80f;

	[fsProperty]
	public string lastUsedShortcutTarget;

	[fsProperty]
	public string playerOneBindingData;

	[fsProperty]
	public string playerTwoBindingData;

	[fsProperty]
	public string playerOneBindingDataV2;

	[fsProperty]
	public string playerTwoBindingDataV2;

	public static bool SupportsStencil
	{
		get
		{
			bool? cachedSupportsStencil = m_cachedSupportsStencil;
			if (cachedSupportsStencil.HasValue && m_cachedSupportsStencil.HasValue)
			{
				return m_cachedSupportsStencil.Value;
			}
			bool flag = SystemInfo.supportsStencil > 0;
			if (flag)
			{
				string graphicsDeviceName = SystemInfo.graphicsDeviceName;
				if (!string.IsNullOrEmpty(graphicsDeviceName) && (graphicsDeviceName.Contains("HD Graphics 4000") || graphicsDeviceName.Contains("620M") || graphicsDeviceName.Contains("630M")))
				{
					flag = false;
				}
			}
			Debug.Log("BRV::StencilMode: " + flag);
			m_cachedSupportsStencil = flag;
			return flag;
		}
	}

	[fsIgnore]
	public float MusicVolume
	{
		get
		{
			return m_musicVolume;
		}
		set
		{
			m_musicVolume = value;
			AkSoundEngine.SetRTPCValue("VOL_MUS", m_musicVolume);
		}
	}

	[fsIgnore]
	public float SoundVolume
	{
		get
		{
			return m_soundVolume;
		}
		set
		{
			m_soundVolume = value;
			AkSoundEngine.SetRTPCValue("VOL_SFX", m_soundVolume);
		}
	}

	[fsIgnore]
	public float UIVolume
	{
		get
		{
			return m_uiVolume;
		}
		set
		{
			m_uiVolume = value;
			AkSoundEngine.SetRTPCValue("VOL_UI", m_uiVolume);
		}
	}

	[fsIgnore]
	public float Gamma
	{
		get
		{
			return m_gamma;
		}
		set
		{
			m_gamma = value;
		}
	}

	[fsIgnore]
	public PixelatorMotionEnhancementMode MotionEnhancementMode
	{
		get
		{
			if (OverrideMotionEnhancementModeForPause)
			{
				return PixelatorMotionEnhancementMode.UNENHANCED_CHEAP;
			}
			if (ShaderQuality == GenericHighMedLowOption.HIGH)
			{
				return PixelatorMotionEnhancementMode.ENHANCED_EXPENSIVE;
			}
			return PixelatorMotionEnhancementMode.UNENHANCED_CHEAP;
		}
	}

	[fsIgnore]
	public AudioHardwareMode AudioHardware
	{
		get
		{
			return m_audioHardware;
		}
		set
		{
			m_audioHardware = value;
			switch (m_audioHardware)
			{
			case AudioHardwareMode.SPEAKERS:
				AkSoundEngine.SetPanningRule(AkPanningRule.AkPanningRule_Speakers);
				break;
			case AudioHardwareMode.HEADPHONES:
				AkSoundEngine.SetPanningRule(AkPanningRule.AkPanningRule_Headphones);
				break;
			}
		}
	}

	[fsIgnore]
	public Minimap.MinimapDisplayMode MinimapDisplayMode
	{
		get
		{
			return m_minimapDisplayMode;
		}
		set
		{
			m_minimapDisplayMode = value;
		}
	}

	[fsIgnore]
	public FullscreenStyle CurrentFullscreenStyle
	{
		get
		{
			return m_fullscreenStyle;
		}
		set
		{
			m_fullscreenStyle = value;
			if (m_fullscreenStyle != FullscreenStyle.BORDERLESS)
			{
				GameCursorController component = GameUIRoot.Instance.GetComponent<GameCursorController>();
				if (component != null)
				{
					component.ToggleClip(false);
				}
			}
		}
	}

	public int CurrentCursorIndex
	{
		get
		{
			return m_currentCursorIndex;
		}
		set
		{
			m_currentCursorIndex = value;
		}
	}

	[fsIgnore]
	public VisualPresetMode CurrentVisualPreset
	{
		get
		{
			return m_visualPresetMode;
		}
		set
		{
			if (m_visualPresetMode != value)
			{
				m_visualPresetMode = value;
				if (m_visualPresetMode == VisualPresetMode.RECOMMENDED)
				{
					Resolution recommendedResolution = GetRecommendedResolution();
					CurrentPreferredScalingMode = GetRecommendedScalingMode();
					CurrentPreferredFullscreenMode = PreferredFullscreenMode.FULLSCREEN;
					Debug.Log("Setting screen resolution RECOMMENDED: " + recommendedResolution.width + "|" + recommendedResolution.height);
					GameManager.Instance.DoSetResolution(recommendedResolution.width, recommendedResolution.height, true);
				}
			}
		}
	}

	[fsIgnore]
	public StringTableManager.GungeonSupportedLanguages CurrentLanguage
	{
		get
		{
			return m_currentLanguage;
		}
		set
		{
			if (m_currentLanguage != value)
			{
				m_currentLanguage = value;
				StringTableManager.CurrentLanguage = value;
				BraveInput.OnLanguageChanged();
			}
		}
	}

	public ControlPreset CurrentControlPreset
	{
		get
		{
			return m_currentControlPreset;
		}
		set
		{
			m_currentControlPreset = value;
			if (m_currentControlPreset == ControlPreset.RECOMMENDED)
			{
				GetBestInputInstance(0).ActiveActions.ReinitializeDefaults();
			}
			else if (m_currentControlPreset == ControlPreset.FLIPPED_TRIGGERS)
			{
				GetBestInputInstance(0).ActiveActions.InitializeSwappedTriggersPreset();
			}
		}
	}

	public ControlPreset CurrentControlPresetP2
	{
		get
		{
			return m_currentControlPresetP2;
		}
		set
		{
			m_currentControlPresetP2 = value;
			if (m_currentControlPresetP2 == ControlPreset.RECOMMENDED)
			{
				GetBestInputInstance(1).ActiveActions.ReinitializeDefaults();
			}
			else if (m_currentControlPresetP2 == ControlPreset.FLIPPED_TRIGGERS)
			{
				GetBestInputInstance(1).ActiveActions.InitializeSwappedTriggersPreset();
			}
		}
	}

	[fsIgnore]
	public PreferredScalingMode CurrentPreferredScalingMode
	{
		get
		{
			return m_preferredScalingMode;
		}
		set
		{
			m_preferredScalingMode = value;
		}
	}

	[fsIgnore]
	public PreferredFullscreenMode CurrentPreferredFullscreenMode
	{
		get
		{
			return m_preferredFullscreenMode;
		}
		set
		{
			m_preferredFullscreenMode = value;
		}
	}

	[fsIgnore]
	public bool DoVsync
	{
		get
		{
			return m_doVsync;
		}
		set
		{
			m_doVsync = value;
			QualitySettings.vSyncCount = (m_doVsync ? 1 : 0);
		}
	}

	[fsIgnore]
	public GenericHighMedLowOption LightingQuality
	{
		get
		{
			return m_lightingQuality;
		}
		set
		{
			if (m_lightingQuality != value)
			{
				m_lightingQuality = value;
				if (m_lightingQuality == GenericHighMedLowOption.VERY_LOW)
				{
					m_lightingQuality = GenericHighMedLowOption.LOW;
				}
				if (m_lightingQuality == GenericHighMedLowOption.MEDIUM)
				{
					m_lightingQuality = GenericHighMedLowOption.HIGH;
				}
				ShadowSystem.ForceAllLightsUpdate();
				if (Pixelator.Instance != null)
				{
					Pixelator.Instance.OnChangedLightingQuality(m_lightingQuality);
				}
			}
		}
	}

	[fsIgnore]
	public GenericHighMedLowOption ShaderQuality
	{
		get
		{
			return m_shaderQuality;
		}
		set
		{
			m_shaderQuality = value;
			if (m_shaderQuality == GenericHighMedLowOption.HIGH || m_shaderQuality == GenericHighMedLowOption.MEDIUM)
			{
				Shader.SetGlobalFloat("_LowQualityMode", 0f);
			}
			else
			{
				Shader.SetGlobalFloat("_LowQualityMode", 1f);
			}
			if (GameManager.HasInstance && (bool)GameManager.Instance.Dungeon)
			{
				RenderSettings.ambientIntensity = GameManager.Instance.Dungeon.TargetAmbientIntensity;
			}
		}
	}

	[fsIgnore]
	public bool RealtimeReflections
	{
		get
		{
			return m_realtimeReflections;
		}
		set
		{
			Shader.SetGlobalFloat("_GlobalReflectionsEnabled", value ? 1 : 0);
			m_realtimeReflections = value;
		}
	}

	[fsIgnore]
	public GenericHighMedLowOption DebrisQuantity
	{
		get
		{
			return m_debrisQuantity;
		}
		set
		{
			m_debrisQuantity = value;
			if (SpawnManager.Instance != null)
			{
				SpawnManager.Instance.OnDebrisQuantityChanged();
			}
		}
	}

	[fsIgnore]
	public GenericHighMedLowOption TextSpeed
	{
		get
		{
			return m_textSpeed;
		}
		set
		{
			m_textSpeed = value;
		}
	}

	[fsIgnore]
	public float ScreenShakeMultiplier
	{
		get
		{
			return m_screenShakeMultiplier;
		}
		set
		{
			m_screenShakeMultiplier = value;
		}
	}

	[fsIgnore]
	public bool CoopScreenShakeReduction
	{
		get
		{
			return m_coopScreenShakeReduction;
		}
		set
		{
			m_coopScreenShakeReduction = value;
		}
	}

	[fsIgnore]
	public float StickyFrictionMultiplier
	{
		get
		{
			return m_stickyFrictionMultiplier;
		}
		set
		{
			m_stickyFrictionMultiplier = value;
		}
	}

	[fsIgnore]
	public ControllerSymbology PlayerOnePreferredSymbology
	{
		get
		{
			return m_playerOnePreferredSymbology;
		}
		set
		{
			m_playerOnePreferredSymbology = value;
		}
	}

	[fsIgnore]
	public ControllerSymbology PlayerTwoPreferredSymbology
	{
		get
		{
			return m_playerTwoPreferredSymbology;
		}
		set
		{
			m_playerTwoPreferredSymbology = value;
		}
	}

	[fsIgnore]
	public bool PlayerOneControllerCursor
	{
		get
		{
			return m_playerOneControllerCursor;
		}
		set
		{
			m_playerOneControllerCursor = value;
		}
	}

	[fsIgnore]
	public bool PlayerTwoControllerCursor
	{
		get
		{
			return m_playerTwoControllerCursor;
		}
		set
		{
			m_playerTwoControllerCursor = value;
		}
	}

	public static void SetStartupQualitySettings()
	{
		string graphicsDeviceName = SystemInfo.graphicsDeviceName;
		string graphicsDeviceVendor = SystemInfo.graphicsDeviceVendor;
		bool flag = false;
		flag |= graphicsDeviceVendor.Contains("NVIDIA");
		flag |= graphicsDeviceVendor.Contains("AMD");
		flag |= graphicsDeviceName.Contains("NVIDIA");
		flag |= graphicsDeviceName.Contains("AMD");
		Debug.Log("> = > = > BRAVE QUALITY: " + flag);
	}

	public static GameOptions CloneOptions(GameOptions source)
	{
		GameOptions gameOptions = new GameOptions();
		FieldInfo[] fields = typeof(GameOptions).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		foreach (FieldInfo fieldInfo in fields)
		{
			bool flag = false;
			if (fieldInfo.GetCustomAttributes(typeof(fsPropertyAttribute), false).Length > 0)
			{
				flag = true;
			}
			if (flag)
			{
				fieldInfo.SetValue(gameOptions, fieldInfo.GetValue(source));
			}
		}
		gameOptions.UpdateCmdArgs();
		return gameOptions;
	}

	public GenericHighMedLowOption GetDefaultRecommendedGraphicalQuality()
	{
		if (m_DefaultRecommendedQuality.HasValue)
		{
			return m_DefaultRecommendedQuality.Value;
		}
		if (SystemInfo.graphicsMemorySize <= 512 || SystemInfo.systemMemorySize <= 1536)
		{
			return GenericHighMedLowOption.LOW;
		}
		string graphicsDeviceName = SystemInfo.graphicsDeviceName;
		if (!string.IsNullOrEmpty(graphicsDeviceName) && graphicsDeviceName.ToLowerInvariant().Contains("intel"))
		{
			m_DefaultRecommendedQuality = GenericHighMedLowOption.MEDIUM;
			return m_DefaultRecommendedQuality.Value;
		}
		string graphicsDeviceVendor = SystemInfo.graphicsDeviceVendor;
		if (!string.IsNullOrEmpty(graphicsDeviceVendor) && graphicsDeviceVendor.ToLowerInvariant().Contains("intel"))
		{
			m_DefaultRecommendedQuality = GenericHighMedLowOption.MEDIUM;
			return m_DefaultRecommendedQuality.Value;
		}
		m_DefaultRecommendedQuality = GenericHighMedLowOption.HIGH;
		return m_DefaultRecommendedQuality.Value;
	}

	public void RevertToDefaults()
	{
		GameOptions obj = new GameOptions();
		FieldInfo[] fields = typeof(GameOptions).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		foreach (FieldInfo fieldInfo in fields)
		{
			bool flag = false;
			if (fieldInfo.GetCustomAttributes(typeof(fsPropertyAttribute), false).Length > 0)
			{
				flag = true;
			}
			if (flag)
			{
				fieldInfo.SetValue(this, fieldInfo.GetValue(obj));
			}
		}
		GenericHighMedLowOption defaultRecommendedGraphicalQuality = GetDefaultRecommendedGraphicalQuality();
		DoVsync = true;
		LightingQuality = ((defaultRecommendedGraphicalQuality != 0) ? GenericHighMedLowOption.HIGH : GenericHighMedLowOption.LOW);
		ShaderQuality = defaultRecommendedGraphicalQuality;
		DebrisQuantity = defaultRecommendedGraphicalQuality;
		RealtimeReflections = defaultRecommendedGraphicalQuality != GenericHighMedLowOption.LOW;
		CurrentLanguage = GameManager.Instance.platformInterface.GetPreferredLanguage();
		StringTableManager.SetNewLanguage(GameManager.Options.CurrentLanguage, true);
		GameManager.Options.MusicVolume = GameManager.Options.MusicVolume;
		GameManager.Options.SoundVolume = GameManager.Options.SoundVolume;
		GameManager.Options.UIVolume = GameManager.Options.UIVolume;
		GameManager.Options.AudioHardware = GameManager.Options.AudioHardware;
		UpdateCmdArgs();
	}

	private void UpdateCmdArgs()
	{
		string commandLine = Environment.CommandLine;
		if (commandLine.Contains("-xinputOnly", true))
		{
			allowNonXinputControllers = false;
		}
		if (commandLine.Contains("-noXinput", true))
		{
			allowXinputControllers = false;
		}
		if (commandLine.Contains("-allowUnknownControllers", true))
		{
			allowUnknownControllers = true;
		}
	}

	public static bool CompareSettings(GameOptions clone, GameOptions source)
	{
		if (clone == null || source == null)
		{
			Debug.LogError(string.Concat(clone, "|", source, " OPTIONS ARE NULL!"));
			return false;
		}
		bool flag = true;
		FieldInfo[] fields = typeof(GameOptions).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (fieldInfo == null)
			{
				continue;
			}
			bool flag2 = false;
			if (fieldInfo.GetCustomAttributes(typeof(fsPropertyAttribute), false).Length > 0)
			{
				flag2 = true;
			}
			if (flag2)
			{
				object value = fieldInfo.GetValue(clone);
				object value2 = fieldInfo.GetValue(source);
				if (value != null && value2 != null)
				{
					bool flag3 = value.Equals(value2);
					flag = flag && flag3;
				}
			}
		}
		return flag;
	}

	public void ApplySettings(GameOptions clone)
	{
		FieldInfo[] fields = typeof(GameOptions).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		foreach (FieldInfo fieldInfo in fields)
		{
			bool flag = false;
			if (fieldInfo.GetCustomAttributes(typeof(fsPropertyAttribute), false).Length > 0 && fieldInfo.GetValue(this) != fieldInfo.GetValue(clone))
			{
				fieldInfo.SetValue(this, fieldInfo.GetValue(clone));
			}
		}
		playerOneBindingDataV2 = clone.playerOneBindingDataV2;
		playerTwoBindingDataV2 = clone.playerTwoBindingDataV2;
		if (this == GameManager.Options)
		{
			BraveInput.ForceLoadBindingInfoFromOptions();
		}
	}

	public static void Load()
	{
		SaveManager.Init();
		GameOptions obj = null;
		bool flag = SaveManager.Load<GameOptions>(SaveManager.OptionsSave, out obj, true);
		if (!flag)
		{
			for (int i = 0; i < 3; i++)
			{
				if (flag)
				{
					break;
				}
				if (i != (int)SaveManager.CurrentSaveSlot)
				{
					obj = null;
					SaveManager.SaveType optionsSave = SaveManager.OptionsSave;
					bool allowDecrypted = true;
					SaveManager.SaveSlot? overrideSaveSlot = (SaveManager.SaveSlot)i;
					flag = SaveManager.Load<GameOptions>(optionsSave, out obj, allowDecrypted, 0u, null, overrideSaveSlot);
					flag = flag && obj != null;
				}
			}
		}
		if (!flag || obj == null)
		{
			GameManager.Options = new GameOptions();
			RequiresLanguageReinitialization = true;
		}
		else
		{
			GameManager.Options = obj;
			GameManager.Options.MusicVolume = GameManager.Options.MusicVolume;
			GameManager.Options.SoundVolume = GameManager.Options.SoundVolume;
			GameManager.Options.UIVolume = GameManager.Options.UIVolume;
			GameManager.Options.AudioHardware = GameManager.Options.AudioHardware;
		}
		GameManager.Options.UpdateCmdArgs();
		GameManager.Options.controllerAimAssistMultiplier = Mathf.Clamp(GameManager.Options.controllerAimAssistMultiplier, 0f, 1.25f);
		GameManager.Options.DisplaySafeArea = Mathf.Clamp(GameManager.Options.DisplaySafeArea, 0.9f, 1f);
		if (GameManager.Options.ShaderQuality == GenericHighMedLowOption.HIGH || GameManager.Options.ShaderQuality == GenericHighMedLowOption.MEDIUM)
		{
			Shader.SetGlobalFloat("_LowQualityMode", 0f);
		}
		else
		{
			Shader.SetGlobalFloat("_LowQualityMode", 1f);
		}
		if (Brave.PlayerPrefs.HasKey("UnitySelectMonitor"))
		{
			GameManager.Options.CurrentMonitorIndex = Brave.PlayerPrefs.GetInt("UnitySelectMonitor");
		}
	}

	public static bool Save()
	{
		return SaveManager.Save(GameManager.Options, SaveManager.OptionsSave, 0);
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

	public PreferredScalingMode GetRecommendedScalingMode()
	{
		if (Screen.width % Pixelator.Instance.CurrentMacroResolutionX == 0 && Screen.height % Pixelator.Instance.CurrentMacroResolutionY == 0)
		{
			return PreferredScalingMode.PIXEL_PERFECT;
		}
		return PreferredScalingMode.FORCE_PIXEL_PERFECT;
	}

	public Resolution GetRecommendedResolution()
	{
		Resolution[] resolutions = Screen.resolutions;
		Resolution result = resolutions[0];
		float num = 1.77777779f;
		bool flag = result.width % Pixelator.Instance.CurrentMacroResolutionX == 0 && result.height % Pixelator.Instance.CurrentMacroResolutionY == 0;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution resolution = resolutions[i];
			if (resolution.height < result.height)
			{
				continue;
			}
			float num2 = (float)result.width / ((float)result.height * 1f);
			float num3 = (float)resolution.width / ((float)resolution.height * 1f);
			if (num2 == num && num3 != num)
			{
				continue;
			}
			if (num2 == num && num3 == num)
			{
				bool flag2 = resolution.width % Pixelator.Instance.CurrentMacroResolutionX == 0 && resolution.height % Pixelator.Instance.CurrentMacroResolutionY == 0;
				if (flag)
				{
					if (flag2 && (resolution.height > result.height || resolution.refreshRate > result.refreshRate))
					{
						result = resolution;
						flag = true;
					}
				}
				else
				{
					result = resolution;
					flag = flag2;
				}
			}
			else
			{
				bool flag3 = resolution.width % Pixelator.Instance.CurrentMacroResolutionX == 0 && resolution.height % Pixelator.Instance.CurrentMacroResolutionY == 0;
				result = resolution;
				flag = flag3;
			}
		}
		return result;
	}
}
