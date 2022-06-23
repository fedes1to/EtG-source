using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dungeonator;
using UnityEngine;
using UnityEngine.Rendering;

public class Pixelator : MonoBehaviour
{
	internal class OcclusionCellData
	{
		public CellData cell;

		public float distance;

		public float changePercentModifier = 1f;

		public OcclusionCellData(CellData c, float dist)
		{
			cell = c;
			distance = dist;
		}
	}

	private enum CoreRenderMode
	{
		NORMAL,
		LOW_QUALITY,
		FAST_SCALING
	}

	public Texture2D localOcclusionTexture;

	private static Pixelator m_instance;

	public float occlusionRevealSpeed = 35f;

	public float occlusionTransitionFadeMultiplier = 4f;

	[NonSerialized]
	public float pointLightMultiplier = 1f;

	public Color occludedColor = new Color(0f, 0f, 0f, 0f);

	public AnimationCurve occlusionPerimeterCurve;

	public int perimeterTileWidth = 5;

	[Header("Vignette Settings")]
	public float vignettePower;

	public float damagedVignettePower = 0.5f;

	public Color vignetteColor = Color.black;

	public Color damagedVignetteColor = Color.red;

	public Shader vignetteShader;

	public Shader fadeShader;

	public Shader utilityShader;

	public bool UseTexturedOcclusion;

	public Texture2D ouchTexture;

	public Camera minimapCameraRef;

	public Texture2D sourceOcclusionTexture;

	public float saturation = 1f;

	public float fade = 1f;

	public bool DoMinimap = true;

	public bool DoRenderGBuffer = true;

	public bool DoOcclusionLayer = true;

	[NonSerialized]
	public bool ManualDoBloom = true;

	public bool PRECLUDE_DEPTH_RENDERING;

	[NonSerialized]
	public bool DoFinalNonFadedLayer;

	[NonSerialized]
	public bool CompositePixelatedUnfadedLayer;

	private List<bool> AdditionalRenderPassesInitialized = new List<bool>();

	private List<Material> AdditionalRenderPasses = new List<Material>();

	private bool m_hasInitializedAdditionalRenderTarget;

	public Material AdditionalCoreStackRenderPass;

	public int overrideTileScale = 1;

	public List<Camera> slavedCameras;

	private Camera m_camera;

	private Camera m_backgroundCamera;

	[SerializeField]
	private Material m_vignetteMaterial;

	[SerializeField]
	private Material m_combinedVignetteFadeMaterial;

	[SerializeField]
	private Material m_fadeMaterial;

	[NonSerialized]
	private Material m_backupFadeMaterial;

	[NonSerialized]
	private Material m_compositor;

	[NonSerialized]
	private Material m_pointLightMaterial;

	[NonSerialized]
	private Material m_pointLightMaterialFast;

	[NonSerialized]
	private Material m_coronalLightMaterial;

	[NonSerialized]
	private Material m_gbufferMaskMaterial;

	[SerializeField]
	private Material m_gbufferLightMaskCombinerMaterial;

	[SerializeField]
	private Material m_partialCopyMaterial;

	private static Texture2D m_smallBlackTexture;

	private Texture2D m_smallWhiteTexture;

	private RenderTexture m_texturedOcclusionTarget;

	private RenderTexture m_reflectionTargetTexture;

	private SENaturalBloomAndDirtyLens m_bloomer;

	public Camera AdditionalPreBGCamera;

	public Camera AdditionalBGCamera;

	public int NewTileScale = 3;

	[NonSerialized]
	public float CurrentTileScale = 3f;

	[NonSerialized]
	public float ScaleTileScale;

	private bool m_occlusionDirty;

	private OcclusionLayer occluder;

	private Transform m_gameQuadTransform;

	private int m_currentMacroResolutionX = 480;

	private int m_currentMacroResolutionY = 270;

	private int cm_occlusionPartition;

	private int cm_core1;

	private int cm_core2;

	private int cm_core3;

	private int cm_core4;

	private int cm_refl;

	private int cm_gbuffer;

	private int cm_gbufferSimple;

	private int cm_fg;

	private int cm_fg_important;

	private int cm_unoccluded;

	private int cm_unpixelated;

	private int cm_unfaded;

	private int PLATFORM_DEPTH;

	private RenderTextureFormat PLATFORM_RENDER_FORMAT;

	private Shader m_simpleSpriteMaskShader;

	private Shader m_simpleSpriteMaskUnpixelatedShader;

	public static bool DebugGraphicsInfo;

	private int m_gBufferID;

	private int m_saturationID;

	private int m_fadeID;

	private int m_fadeColorID;

	private int m_occlusionMapID;

	private int m_occlusionUVID;

	private int m_reflMapID;

	private int m_reflFlipID;

	private int m_gammaID;

	private int m_vignettePowerID;

	private int m_vignetteColorID;

	private int m_damagedTexID;

	private int m_cameraWSID;

	private int m_cameraOrthoSizeID;

	private int m_cameraOrthoSizeXID;

	private int m_lightPosID;

	private int m_lightColorID;

	private int m_lightRadiusID;

	private int m_lightIntensityID;

	private int m_lightCookieID;

	private int m_lightCookieAngleID;

	private int m_lightMaskTexID;

	private int m_preBackgroundTexID;

	private GenericFullscreenEffect m_gammaEffect;

	private float m_gammaAdjustment;

	public static bool AllowPS4MotionEnhancement;

	protected Dictionary<RoomHandler, IEnumerator> RoomOcclusionCoroutineMap = new Dictionary<RoomHandler, IEnumerator>();

	protected List<RoomHandler> ActiveOcclusionCoroutines = new List<RoomHandler>();

	private bool m_occlusionGridDirty;

	private List<IntVector2> m_modifiedRangeMins = new List<IntVector2>();

	private List<IntVector2> m_modifiedRangeMaxs = new List<IntVector2>();

	public int NUM_MACRO_PIXELS_HORIZONTAL = 480;

	public int NUM_MACRO_PIXELS_VERTICAL = 270;

	private bool generatedNewTexture;

	private IntVector2 oldBaseTile;

	[NonSerialized]
	public static bool IsRenderingOcclusionTexture;

	[NonSerialized]
	public static bool IsRenderingReflectionMap;

	private int m_uvRangeID = -1;

	public FilterMode DownsamplingFilterMode = FilterMode.Bilinear;

	private RenderTexture m_cachedFrame_VeryLowSettings;

	[NonSerialized]
	private bool m_timetubedInstance;

	private int extraPixels = 2;

	[NonSerialized]
	private RenderTexture m_UnblurredProjectileMaskTex;

	[NonSerialized]
	private RenderTexture m_BlurredProjectileMaskTex;

	public float ProjectileMaskBlurSize = 0.05f;

	private Material m_blurMaterial;

	public List<AdditionalBraveLight> AdditionalBraveLights = new List<AdditionalBraveLight>();

	private bool m_gammaLocked;

	private bool m_fadeLocked;

	[NonSerialized]
	public bool KillAllFades;

	private GenericFullscreenEffect m_gammaPass;

	public Vector3 CachedPlayerViewportPoint;

	public Vector3 CachedEnemyViewportPoint;

	public const int OCCLUSION_BUFFER = 2;

	private Dictionary<Shader, Material> _shaderMap = new Dictionary<Shader, Material>();

	public static Pixelator Instance
	{
		get
		{
			if (m_instance == null || !m_instance)
			{
				m_instance = UnityEngine.Object.FindObjectOfType<Pixelator>();
			}
			return m_instance;
		}
		set
		{
			m_instance = value;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return m_instance;
		}
	}

	public Vector3 CameraOrigin
	{
		get
		{
			return m_camera.ViewportToWorldPoint(Vector3.zero);
		}
	}

	public Color FadeColor
	{
		get
		{
			return m_fadeMaterial.GetColor("_FadeColor");
		}
		set
		{
			if (m_fadeMaterial != null)
			{
				m_fadeMaterial.SetColor("_FadeColor", value);
			}
		}
	}

	public bool DoBloom
	{
		get
		{
			return ManualDoBloom && GameManager.Options.ShaderQuality != 0 && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW;
		}
	}

	public Material FadeMaterial
	{
		get
		{
			return m_fadeMaterial;
		}
	}

	public static Texture2D SmallBlackTexture
	{
		get
		{
			if (m_smallBlackTexture == null)
			{
				m_smallBlackTexture = new Texture2D(1, 1);
				m_smallBlackTexture.SetPixel(0, 0, Color.black);
				m_smallBlackTexture.Apply();
			}
			return m_smallBlackTexture;
		}
	}

	private float m_deltaTime
	{
		get
		{
			return GameManager.INVARIANT_DELTA_TIME;
		}
	}

	public int CurrentMacroResolutionX
	{
		get
		{
			return m_currentMacroResolutionX;
		}
		set
		{
			m_currentMacroResolutionX = value;
		}
	}

	public int CurrentMacroResolutionY
	{
		get
		{
			return m_currentMacroResolutionY;
		}
		set
		{
			m_currentMacroResolutionY = value;
		}
	}

	public Rect CurrentCameraRect
	{
		get
		{
			return m_camera.rect;
		}
	}

	private bool IsInIntro
	{
		get
		{
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER && Foyer.DoIntroSequence)
			{
				return true;
			}
			return false;
		}
	}

	private bool IsInTitle
	{
		get
		{
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER && Foyer.DoMainMenu)
			{
				return true;
			}
			return false;
		}
	}

	private bool IsInPunchout
	{
		get
		{
			return PunchoutController.IsActive;
		}
	}

	private int GBUFFER_DESCALE
	{
		get
		{
			switch (GameManager.Options.ShaderQuality)
			{
			case GameOptions.GenericHighMedLowOption.LOW:
				return 8;
			case GameOptions.GenericHighMedLowOption.MEDIUM:
				return 4;
			case GameOptions.GenericHighMedLowOption.HIGH:
				return 2;
			case GameOptions.GenericHighMedLowOption.VERY_LOW:
				return 8;
			default:
				return 8;
			}
		}
	}

	protected float LightCullFactor
	{
		get
		{
			if (InfiniteMinecartZone.InInfiniteMinecartZone)
			{
				return 2f;
			}
			if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.HIGH)
			{
				return 1.5f;
			}
			return 1.25f;
		}
	}

	private bool ActuallyRenderGBuffer
	{
		get
		{
			return DoRenderGBuffer;
		}
	}

	public bool CacheCurrentFrameToBuffer
	{
		get
		{
			if (m_gammaPass == null)
			{
				m_gammaPass = GetComponent<GenericFullscreenEffect>();
			}
			return m_gammaPass.CacheCurrentFrameToBuffer;
		}
		set
		{
			if (m_gammaPass == null)
			{
				m_gammaPass = GetComponent<GenericFullscreenEffect>();
			}
			m_gammaPass.CacheCurrentFrameToBuffer = value;
		}
	}

	public void RegisterAdditionalRenderPass(Material pass)
	{
		if (!AdditionalRenderPasses.Contains(pass))
		{
			AdditionalRenderPasses.Add(pass);
			AdditionalRenderPassesInitialized.Add(false);
		}
	}

	public void DeregisterAdditionalRenderPass(Material pass)
	{
		if (AdditionalRenderPasses.Contains(pass))
		{
			int num = AdditionalRenderPasses.IndexOf(pass);
			if (num >= 0)
			{
				AdditionalRenderPassesInitialized.RemoveAt(num);
				AdditionalRenderPasses.RemoveAt(num);
			}
		}
	}

	public void SetOcclusionDirty()
	{
		m_occlusionGridDirty = true;
		m_occlusionDirty = true;
	}

	private void InitializePerPlatform()
	{
		PLATFORM_DEPTH = 24;
		bool flag = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.DefaultHDR);
		PLATFORM_RENDER_FORMAT = ((!flag) ? RenderTextureFormat.Default : RenderTextureFormat.DefaultHDR);
		if (!flag)
		{
			m_camera.hdr = false;
			GetComponent<SENaturalBloomAndDirtyLens>().enabled = false;
		}
	}

	private void Awake()
	{
		AkAudioListener[] components = GetComponents<AkAudioListener>();
		for (int i = 0; i < components.Length; i++)
		{
			if (components[i] != null)
			{
				UnityEngine.Object.Destroy(components[i]);
			}
		}
		m_gammaEffect = GetComponent<GenericFullscreenEffect>();
		m_reflMapID = Shader.PropertyToID("_ReflMapFromPixelator");
		m_reflFlipID = Shader.PropertyToID("_ReflectionYFactor");
		m_gammaID = Shader.PropertyToID("_GammaGamma");
		m_saturationID = Shader.PropertyToID("_Saturation");
		m_fadeID = Shader.PropertyToID("_Fade");
		m_fadeColorID = Shader.PropertyToID("_FadeColor");
		m_occlusionMapID = Shader.PropertyToID("_OcclusionMap");
		m_gBufferID = Shader.PropertyToID("_GBuffer");
		m_occlusionUVID = Shader.PropertyToID("_OcclusionUV");
		m_vignettePowerID = Shader.PropertyToID("_VignettePower");
		m_vignetteColorID = Shader.PropertyToID("_VignetteColor");
		m_damagedTexID = Shader.PropertyToID("_DamagedTex");
		m_cameraWSID = Shader.PropertyToID("_CameraWS");
		m_cameraOrthoSizeID = Shader.PropertyToID("_CameraOrthoSize");
		m_cameraOrthoSizeXID = Shader.PropertyToID("_CameraOrthoSizeX");
		m_lightPosID = Shader.PropertyToID("_LightPos");
		m_lightColorID = Shader.PropertyToID("_LightColor");
		m_lightRadiusID = Shader.PropertyToID("_LightRadius");
		m_lightIntensityID = Shader.PropertyToID("_LightIntensity");
		m_lightCookieID = Shader.PropertyToID("_LightCookie");
		m_lightCookieAngleID = Shader.PropertyToID("_LightCookieAngle");
		m_lightMaskTexID = Shader.PropertyToID("_LightMaskTex");
		m_preBackgroundTexID = Shader.PropertyToID("_PreBackgroundTex");
		m_camera = GetComponent<Camera>();
		m_simpleSpriteMaskShader = ShaderCache.Acquire("Brave/Internal/SimpleSpriteMask");
		m_simpleSpriteMaskUnpixelatedShader = ShaderCache.Acquire("Brave/Internal/SimpleSpriteMaskUnpixelated");
		InitializePerPlatform();
		BraveCameraUtility.MaintainCameraAspect(m_camera);
		if (m_smallBlackTexture == null)
		{
			m_smallBlackTexture = new Texture2D(1, 1);
			m_smallBlackTexture.SetPixel(0, 0, Color.black);
			m_smallBlackTexture.Apply();
		}
		m_smallWhiteTexture = new Texture2D(1, 1);
		m_smallWhiteTexture.SetPixel(0, 0, Color.white);
		m_smallWhiteTexture.Apply();
		m_bloomer = GetComponent<SENaturalBloomAndDirtyLens>();
		cm_occlusionPartition = 1 << LayerMask.NameToLayer("OcclusionRenderPartition");
		cm_core1 = 1 << LayerMask.NameToLayer("BG_Nonsense");
		cm_core2 = 1 << LayerMask.NameToLayer("BG_Critical");
		cm_core3 = (1 << LayerMask.NameToLayer("FG_Nonsense")) | (1 << LayerMask.NameToLayer("ShadowCaster")) | (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("Water"));
		cm_refl = 1 << LayerMask.NameToLayer("FG_Reflection");
		cm_gbuffer = (1 << LayerMask.NameToLayer("FG_Nonsense")) | (1 << LayerMask.NameToLayer("ShadowCaster")) | (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("BG_Nonsense")) | (1 << LayerMask.NameToLayer("BG_Critical")) | (1 << LayerMask.NameToLayer("FG_Critical")) | (1 << LayerMask.NameToLayer("FG_Reflection")) | (1 << LayerMask.NameToLayer("Unpixelated")) | (1 << LayerMask.NameToLayer("Unfaded"));
		cm_gbufferSimple = (1 << LayerMask.NameToLayer("FG_Nonsense")) | (1 << LayerMask.NameToLayer("ShadowCaster")) | (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("BG_Nonsense")) | (1 << LayerMask.NameToLayer("BG_Critical")) | (1 << LayerMask.NameToLayer("FG_Critical")) | (1 << LayerMask.NameToLayer("FG_Reflection")) | (1 << LayerMask.NameToLayer("Unfaded"));
		cm_fg = (1 << LayerMask.NameToLayer("FG_Nonsense")) | (1 << LayerMask.NameToLayer("ShadowCaster")) | (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("FG_Reflection")) | (1 << LayerMask.NameToLayer("FG_Critical"));
		cm_fg_important = (1 << LayerMask.NameToLayer("ShadowCaster")) | (1 << LayerMask.NameToLayer("Default")) | (1 << LayerMask.NameToLayer("FG_Reflection")) | (1 << LayerMask.NameToLayer("FG_Critical"));
		cm_unoccluded = 1 << LayerMask.NameToLayer("Unoccluded");
		cm_unfaded = 1 << LayerMask.NameToLayer("Unfaded");
		if (GameManager.Options == null)
		{
			GameOptions.Load();
		}
		OnChangedMotionEnhancementMode(GameManager.Options.MotionEnhancementMode);
		OnChangedLightingQuality(GameManager.Options.LightingQuality);
	}

	public void OnChangedLightingQuality(GameOptions.GenericHighMedLowOption lightingQuality)
	{
		switch (lightingQuality)
		{
		case GameOptions.GenericHighMedLowOption.LOW:
		case GameOptions.GenericHighMedLowOption.VERY_LOW:
			m_gammaAdjustment = -0.1f;
			QualitySettings.pixelLightCount = 0;
			break;
		case GameOptions.GenericHighMedLowOption.MEDIUM:
			m_gammaAdjustment = 0f;
			QualitySettings.pixelLightCount = 4;
			break;
		case GameOptions.GenericHighMedLowOption.HIGH:
			m_gammaAdjustment = 0f;
			QualitySettings.pixelLightCount = 16;
			break;
		}
	}

	public void OnChangedMotionEnhancementMode(GameOptions.PixelatorMotionEnhancementMode newMode)
	{
		switch (newMode)
		{
		case GameOptions.PixelatorMotionEnhancementMode.ENHANCED_EXPENSIVE:
			cm_core4 = (1 << LayerMask.NameToLayer("FG_Critical")) | (1 << LayerMask.NameToLayer("FG_Reflection"));
			cm_unpixelated = 1 << LayerMask.NameToLayer("Unpixelated");
			break;
		case GameOptions.PixelatorMotionEnhancementMode.UNENHANCED_CHEAP:
			cm_core4 = (1 << LayerMask.NameToLayer("FG_Critical")) | (1 << LayerMask.NameToLayer("FG_Reflection")) | (1 << LayerMask.NameToLayer("Unpixelated"));
			cm_unpixelated = 0;
			break;
		default:
			Debug.LogError("Unsupported MotionEnhancementMode in Pixelator. This should never, ever happen.");
			break;
		}
	}

	public static void DEBUG_LogSystemRenderingData()
	{
		Debug.Log("BRV::DeviceType = " + SystemInfo.deviceType);
		Debug.Log("BRV::GraphicsDeviceType = " + SystemInfo.graphicsDeviceName.ToString());
		Debug.Log("BRV::GraphicsDeviceType = " + SystemInfo.graphicsDeviceType);
		Debug.Log("BRV::GraphicsDeviceVendor = " + SystemInfo.graphicsDeviceVendor.ToString());
		Debug.Log("BRV::GraphicsDeviceVersion = " + SystemInfo.graphicsDeviceVersion.ToString());
		Debug.Log("BRV::GraphicsShaderLevel = " + SystemInfo.graphicsShaderLevel);
		Debug.Log("BRV::GraphicsMemorySize = " + SystemInfo.graphicsMemorySize);
		Debug.Log("BRV::MaxTextureSize = " + SystemInfo.maxTextureSize);
		Debug.Log("BRV::NPOTSupport = " + SystemInfo.npotSupport);
		Debug.Log("BRV::SupportedRenderTargetCount = " + SystemInfo.supportedRenderTargetCount);
		Debug.Log("BRV::SupportsImageEffects = " + SystemInfo.supportsImageEffects);
		Debug.Log("BRV::SupportsRenderTextures = " + SystemInfo.supportsRenderTextures);
		Debug.Log("BRV::SupportsStencil = " + SystemInfo.supportsStencil);
		Debug.Log("BRV::SupportsDefaultHDR = " + SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.DefaultHDR));
		Debug.Log("BRV::SupportsDepthFormat = " + SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth));
		Debug.Log("BRV::Iteration = 1");
	}

	public void SetVignettePower(float tp)
	{
		if (m_fadeMaterial != null)
		{
			m_fadeMaterial.SetFloat(m_vignettePowerID, tp);
		}
		if (m_combinedVignetteFadeMaterial != null)
		{
			m_combinedVignetteFadeMaterial.SetFloat(m_vignettePowerID, tp);
		}
	}

	private void Start()
	{
		if (!IsInIntro)
		{
			minimapCameraRef = Minimap.Instance.cameraRef;
		}
		if (GameManager.Instance.Dungeon != null && GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.WESTGEON)
		{
			UseTexturedOcclusion = true;
		}
		if (vignetteShader != null)
		{
			m_vignetteMaterial = new Material(vignetteShader);
			m_vignetteMaterial.SetColor("_OcclusionFallbackColor", occludedColor);
		}
		if (m_combinedVignetteFadeMaterial == null)
		{
			m_combinedVignetteFadeMaterial = new Material(ShaderCache.Acquire("Brave/CameraEffects/Pixelator_VignetteFade"));
			m_combinedVignetteFadeMaterial.SetColor("_OcclusionFallbackColor", occludedColor);
			m_combinedVignetteFadeMaterial.SetFloat(m_vignettePowerID, vignettePower);
			m_combinedVignetteFadeMaterial.SetColor(m_vignetteColorID, vignetteColor);
			m_combinedVignetteFadeMaterial.SetTexture(m_damagedTexID, ouchTexture);
			m_combinedVignetteFadeMaterial.SetVector("_LowlightColor", GameManager.Instance.BestGenerationDungeonPrefab.decoSettings.lowQualityCheapLightVector);
		}
		if (fadeShader != null)
		{
			m_fadeMaterial = new Material(fadeShader);
			m_fadeMaterial.SetFloat(m_vignettePowerID, vignettePower);
			m_fadeMaterial.SetColor(m_vignetteColorID, vignetteColor);
			m_fadeMaterial.SetTexture(m_damagedTexID, ouchTexture);
		}
		m_pointLightMaterial = new Material(ShaderCache.Acquire("Brave/Internal/GBuffer_LightRenderer"));
		m_pointLightMaterialFast = new Material(ShaderCache.Acquire("Brave/Internal/GBuffer_LightRenderer_Fast"));
		m_gbufferMaskMaterial = new Material(ShaderCache.Acquire("Brave/Internal/GBuffer_LightMask"));
		m_gbufferLightMaskCombinerMaterial = new Material(ShaderCache.Acquire("Brave/Internal/GBuffer_LightMaskCombiner"));
		m_partialCopyMaterial = new Material(ShaderCache.Acquire("Brave/Internal/PartialCopy"));
		occluder = new OcclusionLayer();
		occluder.SourceOcclusionTexture = sourceOcclusionTexture;
		occluder.occludedColor = occludedColor;
		overrideTileScale = 1;
		CheckSize();
		StartCoroutine(BackgroundCoroutineProcessor());
		if ((bool)GameManager.Instance.BestGenerationDungeonPrefab && GameManager.Instance.BestGenerationDungeonPrefab.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.OFFICEGEON)
		{
			SetLumaGain(0.1f);
		}
		else
		{
			SetLumaGain(0f);
		}
	}

	private void OnDestroy()
	{
		if (m_reflectionTargetTexture != null)
		{
			UnityEngine.Object.Destroy(m_reflectionTargetTexture);
		}
	}

	public void MarkOcclusionDirty()
	{
		m_occlusionDirty = true;
	}

	private bool IsExitDetailCell(CellData neighbor, CellData current)
	{
		return neighbor.isExitNonOccluder;
	}

	private IEnumerator BackgroundCoroutineProcessor()
	{
		while (true)
		{
			bool isFoyer = GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER;
			float privateDeltaTime = m_deltaTime;
			if (m_occlusionGridDirty)
			{
				DungeonData data = GameManager.Instance.Dungeon.data;
				bool flag = false;
				for (int i = 0; i < m_modifiedRangeMins.Count; i++)
				{
					IntVector2 intVector = m_modifiedRangeMins[i];
					IntVector2 intVector2 = m_modifiedRangeMaxs[i];
					bool flag2 = false;
					for (int j = intVector.x; j <= intVector2.x; j++)
					{
						for (int k = intVector.y; k <= intVector2.y; k++)
						{
							if (!data.CheckInBounds(j, k))
							{
								continue;
							}
							CellData cellData = data[j, k];
							if (cellData == null)
							{
								continue;
							}
							CellData cellData2 = cellData;
							if (cellData.IsAnyFaceWall() && data.CheckInBoundsAndValid(j, k - 2))
							{
								cellData2 = data[j, k - 2];
							}
							if (!cellData.occlusionData.cellOcclusionDirty)
							{
								continue;
							}
							if (cellData.occlusionData.remainingDelay > 0f)
							{
								cellData.occlusionData.remainingDelay = Mathf.Max(0f, cellData.occlusionData.remainingDelay - privateDeltaTime);
								flag2 = true;
								continue;
							}
							float num = 0.7f;
							if (cellData2.parentRoom != null && cellData2.occlusionData.occlusionParentDefintion != null)
							{
								num = ((cellData2.parentRoom.visibility != RoomHandler.VisibilityStatus.CURRENT && !isFoyer) ? cellData.occlusionData.cellVisitedTargetOcclusion : cellData.occlusionData.cellVisibleTargetOcclusion);
							}
							else if (cellData2.occlusionData.occlusionParentDefintion != null)
							{
								num = (((cellData2.occlusionData.occlusionParentDefintion.downstreamRoom == null || !cellData2.occlusionData.occlusionParentDefintion.downstreamRoom.IsSecretRoom) && (cellData2.occlusionData.occlusionParentDefintion.upstreamRoom == null || !cellData2.occlusionData.occlusionParentDefintion.upstreamRoom.IsSecretRoom)) ? ((cellData2.occlusionData.occlusionParentDefintion.Visibility != RoomHandler.VisibilityStatus.CURRENT && !isFoyer) ? cellData.occlusionData.cellVisitedTargetOcclusion : cellData.occlusionData.cellVisibleTargetOcclusion) : cellData.occlusionData.cellVisibleTargetOcclusion);
							}
							else if (cellData2.parentRoom != null || cellData2.cellVisualData.IsFeatureCell)
							{
								RoomHandler roomHandler = ((!cellData2.cellVisualData.IsFeatureCell) ? cellData2.parentRoom : cellData2.nearestRoom);
								num = ((roomHandler.visibility != RoomHandler.VisibilityStatus.CURRENT && !isFoyer) ? cellData.occlusionData.cellVisitedTargetOcclusion : cellData.occlusionData.cellVisibleTargetOcclusion);
							}
							else if (cellData.occlusionData.cellRoomVisiblityCount > 0)
							{
								num = cellData.occlusionData.cellVisibleTargetOcclusion;
							}
							else if (cellData.occlusionData.cellRoomVisitedCount > 0)
							{
								num = cellData.occlusionData.cellVisitedTargetOcclusion;
							}
							if (cellData.occlusionData.overrideOcclusion)
							{
								num = 0f;
							}
							float f = num - cellData.occlusionData.cellOcclusion;
							float num2 = Mathf.Sign(f) * Mathf.Min(Mathf.Abs(f), privateDeltaTime * occlusionTransitionFadeMultiplier);
							cellData.occlusionData.cellOcclusion += num2;
							cellData.occlusionData.minCellOccluionHistory = Mathf.Min(cellData.occlusionData.minCellOccluionHistory, cellData.occlusionData.cellOcclusion);
							if (cellData.occlusionData.cellOcclusion == num)
							{
								cellData.occlusionData.cellOcclusionDirty = false;
							}
							else
							{
								flag2 = true;
							}
							if (cellData.occlusionData.overrideOcclusion)
							{
								cellData.occlusionData.cellOcclusion = 0f;
							}
						}
					}
					if (!flag2)
					{
						m_modifiedRangeMins.RemoveAt(i);
						m_modifiedRangeMaxs.RemoveAt(i);
						i--;
					}
					else
					{
						flag = true;
						MarkOcclusionDirty();
					}
				}
				if (!flag)
				{
					m_occlusionGridDirty = false;
				}
			}
			yield return null;
		}
	}

	public float ProcessOcclusionChange(IntVector2 startingPosition, float targetVisibility, RoomHandler source, bool useFloodFill = true)
	{
		return HandleRoomOcclusionChange(startingPosition, source, useFloodFill);
	}

	public void ProcessRoomAdditionalExits(IntVector2 startingPosition, RoomHandler source, bool useFloodFill = true)
	{
		HandleRoomExitsCheck(startingPosition, source, useFloodFill);
	}

	protected List<CellData> GetExitCellsToProcess(IntVector2 startingPosition, RoomHandler targetRoom, RoomHandler currentVisibleRoom, DungeonData data)
	{
		List<CellData> list = new List<CellData>();
		if (!targetRoom.area.IsProceduralRoom)
		{
			for (int i = 0; i < targetRoom.area.instanceUsedExits.Count; i++)
			{
				RuntimeRoomExitData key = targetRoom.area.exitToLocalDataMap[targetRoom.area.instanceUsedExits[i]];
				RuntimeExitDefinition runtimeExitDefinition = targetRoom.exitDefinitionsByExit[key];
				if ((runtimeExitDefinition.downstreamRoom.IsSecretRoom && targetRoom == runtimeExitDefinition.upstreamRoom) || (runtimeExitDefinition.upstreamRoom.IsSecretRoom && targetRoom == runtimeExitDefinition.downstreamRoom))
				{
					continue;
				}
				foreach (IntVector2 item5 in runtimeExitDefinition.GetCellsForRoom(targetRoom))
				{
					CellData cellData = data[item5];
					if (cellData != null)
					{
						list.Add(cellData);
					}
					CellData cellData2 = data[cellData.position + IntVector2.Up];
					if (cellData2 != null)
					{
						list.Add(cellData2);
					}
					CellData cellData3 = data[cellData.position + IntVector2.Up * 2];
					if (cellData3 != null)
					{
						list.Add(cellData3);
					}
				}
				if (runtimeExitDefinition.upstreamExit != null && runtimeExitDefinition.upstreamExit.isWarpWingStart && runtimeExitDefinition.upstreamExit.warpWingPortal != null && runtimeExitDefinition.upstreamExit.warpWingPortal.failPortal != null && runtimeExitDefinition.upstreamExit.warpWingPortal.parentRoom == targetRoom)
				{
					RuntimeExitDefinition runtimeExitDefinition2 = runtimeExitDefinition.upstreamExit.warpWingPortal.failPortal.parentRoom.exitDefinitionsByExit[runtimeExitDefinition.upstreamExit.warpWingPortal.failPortal.parentExit];
					foreach (IntVector2 item6 in runtimeExitDefinition2.GetCellsForRoom(runtimeExitDefinition.upstreamExit.warpWingPortal.failPortal.parentRoom))
					{
						BraveUtility.DrawDebugSquare(item6.ToVector2(), Color.yellow, 1000f);
						CellData cellData4 = data[item6];
						if (cellData4 != null)
						{
							list.Add(cellData4);
						}
						CellData cellData5 = data[cellData4.position + IntVector2.Up];
						if (cellData5 != null)
						{
							list.Add(cellData5);
						}
						CellData cellData6 = data[cellData4.position + IntVector2.Up * 2];
						if (cellData6 != null)
						{
							list.Add(cellData6);
						}
					}
				}
				if (runtimeExitDefinition.downstreamExit != null && runtimeExitDefinition.downstreamExit.isWarpWingStart && runtimeExitDefinition.downstreamExit.warpWingPortal != null && runtimeExitDefinition.downstreamExit.warpWingPortal.failPortal != null && runtimeExitDefinition.downstreamExit.warpWingPortal.parentRoom == targetRoom)
				{
					RuntimeExitDefinition runtimeExitDefinition3 = runtimeExitDefinition.downstreamExit.warpWingPortal.failPortal.parentRoom.exitDefinitionsByExit[runtimeExitDefinition.downstreamExit.warpWingPortal.failPortal.parentExit];
					foreach (IntVector2 item7 in runtimeExitDefinition3.GetCellsForRoom(runtimeExitDefinition.downstreamExit.warpWingPortal.failPortal.parentRoom))
					{
						BraveUtility.DrawDebugSquare(item7.ToVector2(), Color.yellow, 1000f);
						CellData cellData7 = data[item7];
						list.Add(cellData7);
						CellData item = data[cellData7.position + IntVector2.Up];
						list.Add(item);
						CellData item2 = data[cellData7.position + IntVector2.Up * 2];
						list.Add(item2);
					}
				}
				if (runtimeExitDefinition.IntermediaryCells == null)
				{
					continue;
				}
				foreach (IntVector2 intermediaryCell in runtimeExitDefinition.IntermediaryCells)
				{
					CellData cellData8 = data[intermediaryCell];
					list.Add(cellData8);
					CellData item3 = data[cellData8.position + IntVector2.Up];
					list.Add(item3);
					CellData item4 = data[cellData8.position + IntVector2.Up * 2];
					list.Add(item4);
				}
			}
		}
		else
		{
			for (int j = 0; j < targetRoom.connectedRooms.Count; j++)
			{
				RoomHandler roomHandler = targetRoom.connectedRooms[j];
				PrototypeRoomExit exitConnectedToRoom = roomHandler.GetExitConnectedToRoom(targetRoom);
				if (exitConnectedToRoom == null)
				{
					continue;
				}
				RuntimeExitDefinition runtimeExitDefinition4 = roomHandler.exitDefinitionsByExit[roomHandler.area.exitToLocalDataMap[exitConnectedToRoom]];
				HashSet<IntVector2> cellsForRoom = runtimeExitDefinition4.GetCellsForRoom(targetRoom);
				foreach (IntVector2 item8 in cellsForRoom)
				{
					CellData cellData9 = data[item8];
					if (cellData9 != null)
					{
						list.Add(cellData9);
					}
					CellData cellData10 = data[cellData9.position + IntVector2.Up];
					if (cellData10 != null)
					{
						list.Add(cellData10);
					}
					CellData cellData11 = data[cellData9.position + IntVector2.Up * 2];
					if (cellData11 != null)
					{
						list.Add(cellData11);
					}
				}
				if (runtimeExitDefinition4.IntermediaryCells == null)
				{
					continue;
				}
				foreach (IntVector2 intermediaryCell2 in runtimeExitDefinition4.IntermediaryCells)
				{
					CellData cellData12 = data[intermediaryCell2];
					if (cellData12 != null)
					{
						list.Add(cellData12);
					}
					CellData cellData13 = data[cellData12.position + IntVector2.Up];
					if (cellData13 != null)
					{
						list.Add(cellData13);
					}
					CellData cellData14 = data[cellData12.position + IntVector2.Up * 2];
					if (cellData14 != null)
					{
						list.Add(cellData14);
					}
				}
			}
		}
		return list;
	}

	protected void HandleRoomExitsCheck(IntVector2 startingPosition, RoomHandler targetRoom, bool useFloodFill = true)
	{
		int num = ((targetRoom.visibility == RoomHandler.VisibilityStatus.CURRENT) ? 1 : (-1));
		int num2 = ((targetRoom.visibility == RoomHandler.VisibilityStatus.VISITED) ? 1 : 0);
		RoomHandler currentVisibleRoom = ((!(GameManager.Instance.PrimaryPlayer == null)) ? GameManager.Instance.PrimaryPlayer.CurrentRoom : GameManager.Instance.Dungeon.data.Entrance);
		DungeonData data = GameManager.Instance.Dungeon.data;
		List<CellData> exitCellsToProcess = GetExitCellsToProcess(startingPosition, targetRoom, currentVisibleRoom, data);
		m_occlusionGridDirty = true;
		IntVector2 intVector = IntVector2.MaxValue;
		IntVector2 intVector2 = IntVector2.MinValue;
		for (int i = 0; i < exitCellsToProcess.Count; i++)
		{
			CellData cellData = exitCellsToProcess[i];
			if (cellData != null)
			{
				float num3 = IntVector2.Distance(cellData.position, startingPosition);
				cellData.occlusionData.remainingDelay = ((!useFloodFill) ? 0f : (num3 / 35f));
				if (targetRoom.visibility == RoomHandler.VisibilityStatus.REOBSCURED)
				{
					cellData.occlusionData.cellRoomVisiblityCount = 0;
					cellData.occlusionData.cellRoomVisitedCount = 0;
					cellData.occlusionData.cellVisitedTargetOcclusion = 1f;
					cellData.occlusionData.minCellOccluionHistory = 1f;
				}
				else
				{
					cellData.occlusionData.cellRoomVisiblityCount = Mathf.RoundToInt(Mathf.Clamp01(num));
					cellData.occlusionData.cellRoomVisitedCount = Mathf.RoundToInt(Mathf.Clamp01(num2));
					cellData.occlusionData.cellVisibleTargetOcclusion = 0f;
					cellData.occlusionData.cellVisitedTargetOcclusion = 0.7f;
				}
				cellData.occlusionData.cellOcclusionDirty = true;
				intVector = IntVector2.Min(intVector, cellData.position);
				intVector2 = IntVector2.Max(intVector2, cellData.position);
			}
		}
		ProcessModifiedRanges(intVector + new IntVector2(-3, -3), intVector2 + new IntVector2(3, 3));
	}

	public void ProcessModifiedRanges(IntVector2 newMin, IntVector2 newMax)
	{
		bool flag = false;
		for (int i = 0; i < m_modifiedRangeMins.Count; i++)
		{
			if (IntVector2.AABBOverlap(newMin, newMax - newMin, m_modifiedRangeMins[i], m_modifiedRangeMaxs[i] - m_modifiedRangeMins[i]))
			{
				m_modifiedRangeMins[i] = IntVector2.Min(m_modifiedRangeMins[i], newMin);
				m_modifiedRangeMaxs[i] = IntVector2.Max(m_modifiedRangeMaxs[i], newMax);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			m_modifiedRangeMins.Add(newMin);
			m_modifiedRangeMaxs.Add(newMax);
		}
	}

	protected float HandleRoomOcclusionChange(IntVector2 startingPosition, RoomHandler targetRoom, bool useFloodFill = true)
	{
		if (targetRoom.PreventRevealEver)
		{
			return 0f;
		}
		int num = ((targetRoom.visibility == RoomHandler.VisibilityStatus.CURRENT) ? 1 : (-1));
		int num2 = ((targetRoom.visibility == RoomHandler.VisibilityStatus.VISITED) ? 1 : 0);
		RoomHandler currentVisibleRoom = ((!(GameManager.Instance.PrimaryPlayer == null)) ? GameManager.Instance.PrimaryPlayer.CurrentRoom : GameManager.Instance.Dungeon.data.Entrance);
		DungeonData data = GameManager.Instance.Dungeon.data;
		HashSet<CellData> hashSet = new HashSet<CellData>();
		for (int i = 0; i < targetRoom.CellsWithoutExits.Count; i++)
		{
			CellData cellData = data[targetRoom.CellsWithoutExits[i]];
			if (cellData == null || (cellData.isSecretRoomCell && !targetRoom.IsSecretRoom))
			{
				continue;
			}
			hashSet.Add(cellData);
			if (cellData.position.y + 1 < data.Height)
			{
				CellData cellData2 = data[cellData.position + IntVector2.Up];
				if (cellData2 != null)
				{
					hashSet.Add(cellData2);
				}
			}
			if (cellData.position.y + 2 < data.Height)
			{
				CellData cellData3 = data[cellData.position + IntVector2.Up * 2];
				if (cellData3 != null)
				{
					hashSet.Add(cellData3);
				}
			}
			if (!UseTexturedOcclusion)
			{
				continue;
			}
			for (int j = 0; j < IntVector2.Cardinals.Length; j++)
			{
				CellData cellData4 = data[cellData.position + IntVector2.Cardinals[j]];
				if (cellData4 != null)
				{
					hashSet.Add(cellData4);
				}
			}
		}
		List<CellData> exitCellsToProcess = GetExitCellsToProcess(startingPosition, targetRoom, currentVisibleRoom, data);
		for (int k = 0; k < exitCellsToProcess.Count; k++)
		{
			if (exitCellsToProcess[k] != null)
			{
				hashSet.Add(exitCellsToProcess[k]);
			}
		}
		for (int l = 0; l < targetRoom.FeatureCells.Count; l++)
		{
			CellData cellData5 = data[targetRoom.FeatureCells[l]];
			if (cellData5 != null)
			{
				hashSet.Add(cellData5);
			}
		}
		m_occlusionGridDirty = true;
		IntVector2 intVector = IntVector2.MaxValue;
		IntVector2 intVector2 = IntVector2.MinValue;
		float num3 = 0f;
		if (occlusionRevealSpeed <= 0f)
		{
			useFloodFill = false;
		}
		if (useFloodFill)
		{
			foreach (CellData item in hashSet)
			{
				if (item != null)
				{
					float num4 = IntVector2.Distance(item.position, startingPosition);
					item.occlusionData.remainingDelay = num4 / occlusionRevealSpeed;
					num3 = Mathf.Max(num3, item.occlusionData.remainingDelay);
					if (targetRoom.visibility == RoomHandler.VisibilityStatus.REOBSCURED)
					{
						item.occlusionData.cellRoomVisiblityCount = 0;
						item.occlusionData.cellRoomVisitedCount = 0;
						item.occlusionData.cellVisibleTargetOcclusion = 1f;
						item.occlusionData.cellVisitedTargetOcclusion = 1f;
						item.occlusionData.minCellOccluionHistory = 1f;
					}
					else
					{
						item.occlusionData.cellRoomVisiblityCount = Mathf.RoundToInt(Mathf.Clamp01(num));
						item.occlusionData.cellRoomVisitedCount = Mathf.RoundToInt(Mathf.Clamp01(num2));
						item.occlusionData.cellVisibleTargetOcclusion = 0f;
						item.occlusionData.cellVisitedTargetOcclusion = 0.7f;
					}
					item.occlusionData.cellOcclusionDirty = true;
					intVector = IntVector2.Min(intVector, item.position);
					intVector2 = IntVector2.Max(intVector2, item.position);
				}
			}
		}
		else
		{
			foreach (CellData item2 in hashSet)
			{
				if (item2 != null)
				{
					item2.occlusionData.remainingDelay = 0f;
					if (targetRoom.visibility == RoomHandler.VisibilityStatus.REOBSCURED)
					{
						item2.occlusionData.cellRoomVisiblityCount = 0;
						item2.occlusionData.cellRoomVisitedCount = 0;
						item2.occlusionData.cellVisibleTargetOcclusion = 1f;
						item2.occlusionData.cellVisitedTargetOcclusion = 1f;
						item2.occlusionData.minCellOccluionHistory = 1f;
					}
					else
					{
						item2.occlusionData.cellRoomVisiblityCount = Mathf.RoundToInt(Mathf.Clamp01(num));
						item2.occlusionData.cellRoomVisitedCount = Mathf.RoundToInt(Mathf.Clamp01(num2));
						item2.occlusionData.cellVisibleTargetOcclusion = 0f;
						item2.occlusionData.cellVisitedTargetOcclusion = 0.7f;
					}
					item2.occlusionData.cellOcclusionDirty = true;
					intVector = IntVector2.Min(intVector, item2.position);
					intVector2 = IntVector2.Max(intVector2, item2.position);
				}
			}
		}
		ProcessModifiedRanges(intVector + new IntVector2(-3, -3), intVector2 + new IntVector2(3, 3));
		return num3;
	}

	private void CheckSize()
	{
		if ((float)GameManager.Instance.Dungeon.Height > m_camera.farClipPlane)
		{
			m_camera.farClipPlane = GameManager.Instance.Dungeon.Height + 50;
		}
		CurrentMacroResolutionX = NUM_MACRO_PIXELS_HORIZONTAL;
		CurrentMacroResolutionY = NUM_MACRO_PIXELS_VERTICAL;
		CurrentTileScale = 3f;
		ScaleTileScale = Mathf.Max(1f, Mathf.Min(20f, (float)Screen.height * m_camera.rect.height / 270f));
		BraveCameraUtility.MaintainCameraAspect(m_camera);
		for (int i = 0; i < slavedCameras.Count; i++)
		{
			BraveCameraUtility.MaintainCameraAspect(slavedCameras[i]);
		}
		m_camera.orthographicSize = (float)NUM_MACRO_PIXELS_VERTICAL / 32f;
		if (!m_backgroundCamera)
		{
			m_backgroundCamera = BraveCameraUtility.GenerateBackgroundCamera(m_camera);
		}
		if (!IsInIntro)
		{
			GameUIRoot.Instance.UpdateScale();
		}
		if (GameManager.Options.CurrentPreferredFullscreenMode != GameOptions.PreferredFullscreenMode.BORDERLESS)
		{
			if (Screen.fullScreen && GameManager.Options.CurrentPreferredFullscreenMode != 0)
			{
				GameManager.Options.CurrentPreferredFullscreenMode = GameOptions.PreferredFullscreenMode.FULLSCREEN;
			}
			else if (!Screen.fullScreen && GameManager.Options.CurrentPreferredFullscreenMode == GameOptions.PreferredFullscreenMode.FULLSCREEN)
			{
				GameManager.Options.CurrentPreferredFullscreenMode = GameOptions.PreferredFullscreenMode.WINDOWED;
			}
		}
	}

	private void RenderOptionalMaps()
	{
		if (UseTexturedOcclusion)
		{
			RenderOcclusionTexture(m_vignetteMaterial);
		}
		else if (m_texturedOcclusionTarget != null)
		{
			m_texturedOcclusionTarget.Release();
			m_texturedOcclusionTarget = null;
			m_vignetteMaterial.SetTexture("_TextureOcclusionTex", null);
		}
		if (GameManager.Options != null && GameManager.Options.RealtimeReflections)
		{
			RenderReflectionMap();
		}
		else if (m_reflectionTargetTexture != null)
		{
			m_reflectionTargetTexture.Release();
			m_reflectionTargetTexture = null;
		}
	}

	private void RenderOcclusionTexture(Material targetVignetteMaterial)
	{
		IsRenderingOcclusionTexture = true;
		if (m_texturedOcclusionTarget == null)
		{
			m_texturedOcclusionTarget = new RenderTexture(NUM_MACRO_PIXELS_HORIZONTAL, NUM_MACRO_PIXELS_VERTICAL, 0, RenderTextureFormat.Default);
			m_texturedOcclusionTarget.hideFlags = HideFlags.DontSave;
			m_texturedOcclusionTarget.filterMode = FilterMode.Point;
			targetVignetteMaterial.SetTexture("_TextureOcclusionTex", m_texturedOcclusionTarget);
		}
		Camera camera = slavedCameras[0];
		camera.CopyFrom(m_camera);
		camera.rect = new Rect(0f, 0f, 1f, 1f);
		camera.hdr = false;
		camera.targetTexture = m_texturedOcclusionTarget;
		camera.clearFlags = CameraClearFlags.Color;
		camera.backgroundColor = Color.clear;
		camera.cullingMask = cm_occlusionPartition;
		camera.Render();
		IsRenderingOcclusionTexture = false;
	}

	public Vector2 GetCurrentSmoothCameraOffset()
	{
		Vector3 vector = ((!IsInIntro) ? CameraController.PLATFORM_CAMERA_OFFSET : Vector3.zero);
		Vector3 vector2 = m_camera.transform.position - vector;
		Vector3 vector3 = new Vector3(Mathf.Round(vector2.x * 16f), Mathf.Round(vector2.y * 16f), Mathf.Round(vector2.z * 16f)) / 16f;
		return (vector2 - vector3).XY();
	}

	public IntVector2 GetCurrentMicropixelOffset()
	{
		Vector2 currentSmoothCameraOffset = GetCurrentSmoothCameraOffset();
		int x = Mathf.RoundToInt(currentSmoothCameraOffset.x / (0.0625f / ScaleTileScale));
		int y = Mathf.RoundToInt(currentSmoothCameraOffset.y / (0.0625f / ScaleTileScale));
		return new IntVector2(x, y);
	}

	private void RenderReflectionMap()
	{
		IsRenderingReflectionMap = true;
		Vector3 vector = ((!IsInIntro) ? CameraController.PLATFORM_CAMERA_OFFSET : Vector3.zero);
		Vector3 vector2 = m_camera.transform.position - vector;
		Vector3 position = new Vector3(Mathf.Round(vector2.x * 16f) - 1f, Mathf.Round(vector2.y * 16f) - 1f, Mathf.Round(vector2.z * 16f)) / 16f;
		position += vector;
		m_camera.transform.position = position;
		if (m_reflectionTargetTexture == null || m_reflectionTargetTexture.width != NUM_MACRO_PIXELS_HORIZONTAL || m_reflectionTargetTexture.height != NUM_MACRO_PIXELS_VERTICAL)
		{
			if (m_reflectionTargetTexture != null)
			{
				m_reflectionTargetTexture.Release();
			}
			m_reflectionTargetTexture = new RenderTexture(NUM_MACRO_PIXELS_HORIZONTAL, NUM_MACRO_PIXELS_VERTICAL, 0, RenderTextureFormat.Default);
			m_reflectionTargetTexture.hideFlags = HideFlags.DontSave;
			m_reflectionTargetTexture.filterMode = FilterMode.Bilinear;
			Shader.SetGlobalTexture(m_reflMapID, m_reflectionTargetTexture);
		}
		Shader.SetGlobalFloat(m_reflFlipID, 2f);
		Camera camera = slavedCameras[0];
		camera.CopyFrom(m_camera);
		camera.rect = new Rect(0f, 0f, 1f, 1f);
		camera.hdr = true;
		camera.targetTexture = m_reflectionTargetTexture;
		CameraClearFlags cameraClearFlags2 = (camera.clearFlags = CameraClearFlags.Color);
		camera.backgroundColor = Color.clear;
		camera.cullingMask = cm_refl;
		camera.Render();
		Shader.SetGlobalFloat(m_reflFlipID, 0f);
		m_camera.transform.position = vector2 + vector;
		IsRenderingReflectionMap = false;
	}

	public void SetLumaGain(float gain)
	{
		m_gammaEffect.ActiveMaterial.SetFloat("_Gain", gain);
	}

	private void CalculateDepixelatedOffset(Vector3 cachedPosition, Vector3 quantizedPosition, int corePixelatedWidth, int corePixelatedHeight, RenderTexture referenceBufferA)
	{
		Vector2 vector = cachedPosition.XY() - quantizedPosition.XY();
		vector *= 16f;
		vector.x /= referenceBufferA.width;
		vector.y /= referenceBufferA.height;
		Vector4 value = new Vector4(vector.x, vector.y, vector.x + (float)corePixelatedWidth / (float)referenceBufferA.width, vector.y + (float)corePixelatedHeight / (float)referenceBufferA.height);
		if (m_uvRangeID == -1)
		{
			m_uvRangeID = Shader.PropertyToID("_UVRange");
		}
		m_partialCopyMaterial.SetVector(m_uvRangeID, value);
		if (m_gbufferLightMaskCombinerMaterial != null)
		{
			m_gbufferLightMaskCombinerMaterial.SetVector(m_uvRangeID, value);
		}
	}

	private void HandlePreDeathFramingLogic()
	{
		if (!CacheCurrentFrameToBuffer || GameManager.Instance.AllPlayers == null)
		{
			return;
		}
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if ((bool)GameManager.Instance.AllPlayers[i])
			{
				GameManager.Instance.AllPlayers[i].ToggleHandRenderers(false, "death");
			}
		}
	}

	private void DoOcclusionUpdate(Material targetVignetteMaterial)
	{
		if (DoOcclusionLayer && !IsInIntro && !GameManager.Instance.IsSelectingCharacter)
		{
			Vector3 vector = m_camera.transform.position + new Vector3(BraveCameraUtility.ASPECT * (0f - m_camera.orthographicSize), 0f - m_camera.orthographicSize, 0f);
			Vector3 vector2 = vector + new Vector3(CurrentMacroResolutionX / 16, CurrentMacroResolutionY / 16, 0f);
			IntVector2 intVector = vector.IntXY();
			if (generatedNewTexture && targetVignetteMaterial != null && occluder.SourceOcclusionTexture != null)
			{
				targetVignetteMaterial.SetTexture(m_occlusionMapID, occluder.SourceOcclusionTexture);
			}
			generatedNewTexture = false;
			if (localOcclusionTexture == null || occluder.cachedX != intVector.x - 2 || occluder.cachedY != intVector.y - 2 || m_occlusionDirty)
			{
				m_occlusionDirty = false;
				generatedNewTexture = true;
				occluder.GenerateOcclusionTexture(intVector.x - 2, intVector.y - 2, GameManager.Instance.Dungeon.data);
				localOcclusionTexture = occluder.SourceOcclusionTexture;
				if (targetVignetteMaterial != null && occluder.SourceOcclusionTexture != null)
				{
					targetVignetteMaterial.SetTexture(m_occlusionMapID, occluder.SourceOcclusionTexture);
				}
			}
			if (targetVignetteMaterial != null)
			{
				Vector2 vector3 = vector.XY() - intVector.ToVector2();
				Vector2 vector4 = vector2.XY() - (intVector + new IntVector2(CurrentMacroResolutionX / 16, CurrentMacroResolutionY / 16)).ToVector2();
				int num = CurrentMacroResolutionX / 16 + 4;
				int num2 = CurrentMacroResolutionY / 16 + 4;
				float num3 = 2f;
				float x = (num3 + vector3.x) / (float)num;
				float y = (num3 + vector3.y) / (float)num2;
				float z = 1f - (num3 - vector4.x) / (float)num;
				float w = 1f - (num3 - vector4.y) / (float)num2;
				Vector4 value = new Vector4(x, y, z, w);
				if (targetVignetteMaterial != null)
				{
					targetVignetteMaterial.SetVector(m_occlusionUVID, value);
				}
			}
		}
		else
		{
			if (localOcclusionTexture == null || localOcclusionTexture.width > 1)
			{
				localOcclusionTexture = new Texture2D(1, 1);
				localOcclusionTexture.SetPixel(0, 0, new Color(0f, 1f, 1f, 1f));
				localOcclusionTexture.Apply();
			}
			if (targetVignetteMaterial != null && localOcclusionTexture != null)
			{
				targetVignetteMaterial.SetTexture(m_occlusionMapID, localOcclusionTexture);
			}
		}
	}

	private bool ShouldOverrideMultiplexing()
	{
		return false;
	}

	private void OnRenderImage(RenderTexture source, RenderTexture target)
	{
		Dungeon dungeon = GameManager.Instance.Dungeon;
		if (dungeon == null || dungeon.data == null || dungeon.data.cellData == null)
		{
			bool flag = dungeon == null;
			bool flag2 = dungeon == null || dungeon.data == null;
			bool flag3 = dungeon == null || dungeon.data == null || dungeon.data.cellData == null;
			Debug.LogWarningFormat("No dungeon data found! {0} {1} {2}", flag, flag2, flag3);
			return;
		}
		if (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.VERY_LOW && !IsInIntro && !IsInTitle && !IsInPunchout)
		{
			if (m_backupFadeMaterial == null && m_fadeMaterial != null)
			{
				m_backupFadeMaterial = m_fadeMaterial;
				m_fadeMaterial = m_combinedVignetteFadeMaterial;
			}
			if ((bool)m_combinedVignetteFadeMaterial)
			{
				if (GameManager.Options.LightingQuality == GameOptions.GenericHighMedLowOption.LOW && !m_combinedVignetteFadeMaterial.IsKeywordEnabled("LOWLIGHT_ON"))
				{
					m_combinedVignetteFadeMaterial.DisableKeyword("LOWLIGHT_OFF");
					m_combinedVignetteFadeMaterial.EnableKeyword("LOWLIGHT_ON");
				}
				else if (GameManager.Options.LightingQuality == GameOptions.GenericHighMedLowOption.HIGH && !m_combinedVignetteFadeMaterial.IsKeywordEnabled("LOWLIGHT_OFF"))
				{
					m_combinedVignetteFadeMaterial.DisableKeyword("LOWLIGHT_ON");
					m_combinedVignetteFadeMaterial.EnableKeyword("LOWLIGHT_OFF");
				}
			}
			if ((bool)m_gammaEffect && (bool)m_gammaEffect.ActiveMaterial)
			{
				if (m_gammaEffect.enabled)
				{
					m_combinedVignetteFadeMaterial.SetTexture(m_occlusionMapID, occluder.SourceOcclusionTexture);
					m_gammaEffect.enabled = false;
				}
				m_combinedVignetteFadeMaterial.SetFloat(m_gammaID, m_gammaEffect.ActiveMaterial.GetFloat(m_gammaID));
			}
			RenderGame_Combined(source, target, CoreRenderMode.LOW_QUALITY);
			return;
		}
		if ((bool)m_gammaEffect && !m_gammaEffect.enabled)
		{
			m_vignetteMaterial.SetTexture(m_occlusionMapID, occluder.SourceOcclusionTexture);
			m_gammaEffect.enabled = true;
		}
		GameOptions.PreferredScalingMode preferredScalingMode = GameManager.Options.CurrentPreferredScalingMode;
		if (IsInIntro && preferredScalingMode == GameOptions.PreferredScalingMode.UNIFORM_SCALING_FAST)
		{
			preferredScalingMode = GameOptions.PreferredScalingMode.UNIFORM_SCALING;
		}
		if ((preferredScalingMode == GameOptions.PreferredScalingMode.UNIFORM_SCALING_FAST || ShouldOverrideMultiplexing()) && !IsInPunchout)
		{
			if (m_backupFadeMaterial == null && m_fadeMaterial != null)
			{
				m_backupFadeMaterial = m_fadeMaterial;
				m_fadeMaterial = m_combinedVignetteFadeMaterial;
			}
			if (m_combinedVignetteFadeMaterial.IsKeywordEnabled("LOWLIGHT_ON"))
			{
				m_combinedVignetteFadeMaterial.DisableKeyword("LOWLIGHT_ON");
				m_combinedVignetteFadeMaterial.EnableKeyword("LOWLIGHT_OFF");
			}
			if ((bool)m_gammaEffect && (bool)m_gammaEffect.ActiveMaterial)
			{
				m_combinedVignetteFadeMaterial.SetTexture(m_occlusionMapID, occluder.SourceOcclusionTexture);
				if (!m_gammaEffect.enabled)
				{
					m_gammaEffect.enabled = true;
				}
				m_combinedVignetteFadeMaterial.SetFloat(m_gammaID, 1f);
			}
			RenderGame_Combined(source, target, CoreRenderMode.FAST_SCALING);
		}
		else
		{
			if (m_backupFadeMaterial != null)
			{
				m_fadeMaterial = m_backupFadeMaterial;
				m_backupFadeMaterial = null;
			}
			RenderGame_Pretty(source, target);
		}
	}

	public RenderTexture GetCachedFrame_VeryLowSettings()
	{
		return m_cachedFrame_VeryLowSettings;
	}

	public void ClearCachedFrame_VeryLowSettings()
	{
		if (m_cachedFrame_VeryLowSettings != null)
		{
			RenderTexture.ReleaseTemporary(m_cachedFrame_VeryLowSettings);
		}
		m_cachedFrame_VeryLowSettings = null;
	}

	private void RenderGame_Combined(RenderTexture source, RenderTexture target, CoreRenderMode renderMode)
	{
		HandlePreDeathFramingLogic();
		if (renderMode == CoreRenderMode.LOW_QUALITY)
		{
			if ((bool)m_bloomer && m_bloomer.enabled)
			{
				m_bloomer.enabled = false;
			}
		}
		else
		{
			RenderOptionalMaps();
			if (((bool)m_bloomer && DoBloom && !m_bloomer.enabled) || (!DoBloom && m_bloomer.enabled))
			{
				m_bloomer.enabled = DoBloom;
			}
		}
		CheckSize();
		BraveCameraUtility.MaintainCameraAspect(m_camera);
		if (renderMode == CoreRenderMode.NORMAL)
		{
			DoOcclusionUpdate(m_vignetteMaterial);
		}
		else
		{
			DoOcclusionUpdate(m_combinedVignetteFadeMaterial);
		}
		int num = CurrentMacroResolutionX / overrideTileScale;
		int num2 = CurrentMacroResolutionY / overrideTileScale;
		Camera camera = slavedCameras[0];
		camera.CopyFrom(m_camera);
		camera.orthographicSize = m_camera.orthographicSize;
		camera.rect = new Rect(0f, 0f, 1f, 1f);
		camera.hdr = renderMode != CoreRenderMode.LOW_QUALITY && (bool)m_bloomer && m_bloomer.enabled;
		RenderTexture renderTexture = null;
		if (AdditionalPreBGCamera != null)
		{
			renderTexture = RenderTexture.GetTemporary(source.width, source.height, PLATFORM_DEPTH, PLATFORM_RENDER_FORMAT);
			renderTexture.filterMode = FilterMode.Point;
			AdditionalPreBGCamera.enabled = false;
			AdditionalPreBGCamera.clearFlags = CameraClearFlags.Color;
			AdditionalPreBGCamera.backgroundColor = Color.black;
			AdditionalPreBGCamera.targetTexture = renderTexture;
			AdditionalPreBGCamera.Render();
			Shader.SetGlobalTexture(m_preBackgroundTexID, renderTexture);
		}
		RenderTexture renderTexture2 = RenderTexture.GetTemporary(num, num2, PLATFORM_DEPTH, PLATFORM_RENDER_FORMAT);
		renderTexture2.filterMode = FilterMode.Point;
		camera.targetTexture = renderTexture2;
		CameraClearFlags clearFlags = CameraClearFlags.Color;
		if (AdditionalBGCamera != null)
		{
			AdditionalBGCamera.enabled = false;
			AdditionalBGCamera.clearFlags = CameraClearFlags.Color;
			clearFlags = CameraClearFlags.Nothing;
			AdditionalBGCamera.backgroundColor = Color.black;
			AdditionalBGCamera.targetTexture = renderTexture2;
			AdditionalBGCamera.Render();
		}
		camera.clearFlags = clearFlags;
		camera.backgroundColor = Color.black;
		camera.cullingMask = cm_core1 | cm_core2;
		camera.Render();
		camera.clearFlags = CameraClearFlags.Depth;
		camera.backgroundColor = Color.clear;
		if (renderMode == CoreRenderMode.FAST_SCALING)
		{
			camera.cullingMask = cm_core3 | cm_core4 | (1 << LayerMask.NameToLayer("Unpixelated"));
		}
		else
		{
			camera.cullingMask = cm_core3 | cm_core4;
		}
		camera.Render();
		if (AdditionalCoreStackRenderPass != null)
		{
			if (TimeTubeCreditsController.IsTimeTubing || m_timetubedInstance)
			{
				m_timetubedInstance = true;
				Graphics.Blit(renderTexture2, renderTexture2, AdditionalCoreStackRenderPass);
			}
			else
			{
				RenderTexture temporary = RenderTexture.GetTemporary(renderTexture2.width, renderTexture2.height, renderTexture2.depth, renderTexture2.format);
				temporary.filterMode = FilterMode.Point;
				Graphics.Blit(renderTexture2, temporary, AdditionalCoreStackRenderPass);
				RenderTexture.ReleaseTemporary(renderTexture2);
				renderTexture2 = temporary;
			}
		}
		if (AdditionalPreBGCamera != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture);
		}
		RenderTexture temporary2 = RenderTexture.GetTemporary(source.width, source.height, 0, PLATFORM_RENDER_FORMAT);
		temporary2.filterMode = FilterMode.Point;
		Graphics.Blit(m_smallBlackTexture, temporary2);
		switch (renderMode)
		{
		case CoreRenderMode.LOW_QUALITY:
			RenderGBufferCheap(source, camera, renderTexture2.depthBuffer, temporary2, num, num2);
			break;
		case CoreRenderMode.FAST_SCALING:
			RenderGBufferScaling(source, camera, renderTexture2.depthBuffer, temporary2);
			break;
		}
		if (AdditionalRenderPasses.Count > 0)
		{
			for (int i = 0; i < AdditionalRenderPasses.Count; i++)
			{
				if (AdditionalRenderPasses[i] == null)
				{
					AdditionalRenderPasses.RemoveAt(i);
					i--;
					continue;
				}
				RenderTexture temporary3 = RenderTexture.GetTemporary(renderTexture2.width, renderTexture2.height, renderTexture2.depth, renderTexture2.format);
				Graphics.Blit(renderTexture2, temporary3, AdditionalRenderPasses[i], 0);
				if (AdditionalRenderPassesInitialized[i])
				{
					RenderTexture.ReleaseTemporary(renderTexture2);
					renderTexture2 = temporary3;
				}
				else
				{
					AdditionalRenderPassesInitialized[i] = true;
					RenderTexture.ReleaseTemporary(temporary3);
				}
			}
		}
		else if (!m_hasInitializedAdditionalRenderTarget)
		{
			RenderTexture temporary4 = RenderTexture.GetTemporary(renderTexture2.width, renderTexture2.height, renderTexture2.depth, renderTexture2.format);
			Graphics.Blit(renderTexture2, temporary4);
			RenderTexture.ReleaseTemporary(renderTexture2);
			renderTexture2 = temporary4;
			m_hasInitializedAdditionalRenderTarget = true;
		}
		if (!m_gammaLocked)
		{
			m_gammaEffect.ActiveMaterial.SetFloat(m_gammaID, 2f - GameManager.Options.Gamma + m_gammaAdjustment);
		}
		if (m_combinedVignetteFadeMaterial != null)
		{
			m_combinedVignetteFadeMaterial.SetTexture(m_gBufferID, temporary2);
			m_combinedVignetteFadeMaterial.SetFloat(m_saturationID, saturation);
			m_combinedVignetteFadeMaterial.SetFloat(m_fadeID, fade);
		}
		if (DoFinalNonFadedLayer && m_combinedVignetteFadeMaterial != null)
		{
			Graphics.Blit(renderTexture2, target, m_combinedVignetteFadeMaterial);
			BraveCameraUtility.MaintainCameraAspect(camera);
			if (CompositePixelatedUnfadedLayer)
			{
				RenderTexture temporary5 = RenderTexture.GetTemporary(BraveCameraUtility.H_PIXELS, BraveCameraUtility.V_PIXELS);
				temporary5.filterMode = FilterMode.Point;
				camera.targetTexture = temporary5;
				camera.clearFlags = CameraClearFlags.Color;
				camera.backgroundColor = new Color(1f, 0f, 0f);
				camera.cullingMask = cm_unfaded;
				camera.Render();
				if (m_compositor == null)
				{
					m_compositor = new Material(ShaderCache.Acquire("Hidden/SimpleCompositor"));
				}
				m_compositor.SetTexture("_BaseTex", target);
				m_compositor.SetTexture("_LayerTex", temporary5);
				Graphics.Blit(temporary5, target, m_compositor);
				RenderTexture.ReleaseTemporary(temporary5);
			}
			else
			{
				camera.targetTexture = target;
				camera.clearFlags = CameraClearFlags.Depth;
				camera.cullingMask = cm_unfaded;
				camera.Render();
			}
		}
		else if (m_combinedVignetteFadeMaterial != null)
		{
			Graphics.Blit(renderTexture2, target, m_combinedVignetteFadeMaterial);
		}
		else
		{
			Graphics.Blit(renderTexture2, target);
		}
		BraveCameraUtility.MaintainCameraAspect(camera);
		camera.rect = new Rect(0f, 0f, 1f, 1f);
		camera.targetTexture = target;
		camera.clearFlags = CameraClearFlags.Depth;
		camera.cullingMask = cm_unoccluded;
		camera.Render();
		RenderTexture.ReleaseTemporary(temporary2);
		RenderTexture.ReleaseTemporary(renderTexture2);
		m_camera.cullingMask = 0;
		camera.cullingMask = 0;
		if (CacheCurrentFrameToBuffer)
		{
			ClearCachedFrame_VeryLowSettings();
			m_cachedFrame_VeryLowSettings = RenderTexture.GetTemporary(renderTexture2.width, renderTexture2.height, 0, renderTexture2.format);
			m_cachedFrame_VeryLowSettings.filterMode = FilterMode.Point;
			Graphics.Blit(renderTexture2, m_cachedFrame_VeryLowSettings);
			CacheCurrentFrameToBuffer = false;
		}
	}

	private void RenderGame_Pretty(RenderTexture source, RenderTexture target)
	{
		bool isCurrentlyZoomIntermediate = GameManager.Instance.MainCameraController.IsCurrentlyZoomIntermediate;
		HandlePreDeathFramingLogic();
		if (DebugGraphicsInfo)
		{
			DEBUG_LogSystemRenderingData();
		}
		RenderOptionalMaps();
		if (((bool)m_bloomer && DoBloom && !m_bloomer.enabled) || (!DoBloom && m_bloomer.enabled))
		{
			m_bloomer.enabled = DoBloom;
		}
		CheckSize();
		BraveCameraUtility.MaintainCameraAspect(m_camera);
		DoOcclusionUpdate(m_vignetteMaterial);
		int num = CurrentMacroResolutionX / overrideTileScale;
		int num2 = CurrentMacroResolutionY / overrideTileScale;
		int width = num + extraPixels;
		int num3 = num2 + extraPixels;
		Vector3 vector = ((!IsInIntro) ? CameraController.PLATFORM_CAMERA_OFFSET : Vector3.zero);
		Vector3 vector2 = m_camera.transform.position - vector;
		Vector3 vector3 = new Vector3(Mathf.Round(vector2.x * 16f) - 1f, Mathf.Round(vector2.y * 16f) - 1f, Mathf.Round(vector2.z * 16f)) / 16f;
		vector3 += vector;
		m_camera.transform.position = vector3;
		Camera camera = slavedCameras[0];
		camera.CopyFrom(m_camera);
		camera.orthographicSize = m_camera.orthographicSize * ((float)num3 / ((float)CurrentMacroResolutionY / (float)overrideTileScale));
		camera.rect = new Rect(0f, 0f, 1f, 1f);
		camera.hdr = true;
		RenderTexture renderTexture = null;
		if (AdditionalPreBGCamera != null)
		{
			renderTexture = RenderTexture.GetTemporary(source.width, source.height, PLATFORM_DEPTH, PLATFORM_RENDER_FORMAT);
			renderTexture.filterMode = FilterMode.Point;
			AdditionalPreBGCamera.enabled = false;
			AdditionalPreBGCamera.clearFlags = CameraClearFlags.Color;
			AdditionalPreBGCamera.backgroundColor = Color.black;
			AdditionalPreBGCamera.targetTexture = renderTexture;
			AdditionalPreBGCamera.Render();
			Shader.SetGlobalTexture(m_preBackgroundTexID, renderTexture);
		}
		RenderTexture renderTexture2 = RenderTexture.GetTemporary(width, num3, PLATFORM_DEPTH, PLATFORM_RENDER_FORMAT);
		renderTexture2.filterMode = FilterMode.Point;
		camera.targetTexture = renderTexture2;
		CameraClearFlags clearFlags = CameraClearFlags.Color;
		if (AdditionalBGCamera != null)
		{
			AdditionalBGCamera.enabled = false;
			AdditionalBGCamera.clearFlags = CameraClearFlags.Color;
			clearFlags = CameraClearFlags.Nothing;
			AdditionalBGCamera.backgroundColor = Color.black;
			AdditionalBGCamera.targetTexture = renderTexture2;
			AdditionalBGCamera.Render();
		}
		camera.clearFlags = clearFlags;
		camera.backgroundColor = Color.black;
		camera.cullingMask = cm_core1 | cm_core2;
		camera.Render();
		camera.clearFlags = CameraClearFlags.Depth;
		camera.backgroundColor = Color.clear;
		camera.cullingMask = cm_core3 | cm_core4;
		camera.Render();
		camera.orthographicSize = m_camera.orthographicSize;
		m_camera.transform.position = vector2 + vector;
		RenderTexture renderTexture3 = null;
		if (AdditionalCoreStackRenderPass != null)
		{
			if (TimeTubeCreditsController.IsTimeTubing || m_timetubedInstance)
			{
				m_timetubedInstance = true;
				Graphics.Blit(renderTexture2, renderTexture2, AdditionalCoreStackRenderPass);
			}
			else
			{
				RenderTexture temporary = RenderTexture.GetTemporary(renderTexture2.width, renderTexture2.height, renderTexture2.depth, renderTexture2.format);
				temporary.filterMode = FilterMode.Point;
				Graphics.Blit(renderTexture2, temporary, AdditionalCoreStackRenderPass);
				renderTexture3 = renderTexture2;
				renderTexture2 = temporary;
			}
		}
		if (AdditionalPreBGCamera != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture);
		}
		RenderTexture renderTexture4 = null;
		if (GameManager.Options.MotionEnhancementMode == GameOptions.PixelatorMotionEnhancementMode.ENHANCED_EXPENSIVE)
		{
			m_camera.transform.position = new Vector3(Mathf.Floor(vector2.x * (16f * ScaleTileScale)), Mathf.Floor(vector2.y * (16f * ScaleTileScale)), Mathf.Floor(vector2.z * (16f * ScaleTileScale))) / (16f * ScaleTileScale);
			m_camera.transform.position -= new Vector3(0.0625f, 0.0625f, 0f);
			camera.orthographicSize = m_camera.orthographicSize;
			renderTexture4 = RenderTexture.GetTemporary(source.width, source.height, PLATFORM_DEPTH, RenderTextureFormat.Depth);
			Graphics.Blit(m_smallBlackTexture, renderTexture4);
			camera.targetTexture = renderTexture4;
			camera.clearFlags = CameraClearFlags.Depth;
			camera.cullingMask = cm_fg_important;
			if (!PRECLUDE_DEPTH_RENDERING)
			{
				camera.Render();
			}
			camera.orthographicSize = m_camera.orthographicSize * ((float)num3 / ((float)CurrentMacroResolutionY / (float)overrideTileScale));
			m_camera.transform.position += new Vector3(0.0625f, 0.0625f, 0f);
			m_camera.transform.position = vector2 + vector;
		}
		CalculateDepixelatedOffset(vector2, vector3, num, num2, renderTexture2);
		RenderTexture temporary2 = RenderTexture.GetTemporary(source.width, source.height, 0, PLATFORM_RENDER_FORMAT);
		temporary2.filterMode = FilterMode.Point;
		Graphics.Blit(m_smallBlackTexture, temporary2);
		m_camera.transform.position = vector3;
		camera.orthographicSize = m_camera.orthographicSize * ((float)num3 / ((float)CurrentMacroResolutionY / (float)overrideTileScale));
		RenderGBuffer(source, camera, (!renderTexture3) ? renderTexture2.depthBuffer : renderTexture3.depthBuffer, temporary2, vector2, vector3);
		camera.orthographicSize = m_camera.orthographicSize;
		m_camera.transform.position = vector2 + vector;
		if (renderTexture3 != null)
		{
			RenderTexture.ReleaseTemporary(renderTexture3);
			renderTexture3 = null;
		}
		RenderTexture temporary3 = RenderTexture.GetTemporary(source.width, source.height, PLATFORM_DEPTH, PLATFORM_RENDER_FORMAT);
		int num4 = Mathf.Max(Mathf.CeilToInt((float)source.width / (float)CurrentMacroResolutionX), Mathf.CeilToInt((float)source.height / (float)CurrentMacroResolutionY));
		if (CurrentMacroResolutionX * num4 == source.width && CurrentMacroResolutionY * num4 == source.height)
		{
			Graphics.Blit(renderTexture2, temporary3, m_partialCopyMaterial);
		}
		else
		{
			RenderTexture temporary4 = RenderTexture.GetTemporary(CurrentMacroResolutionX * num4, CurrentMacroResolutionY * num4, PLATFORM_DEPTH, PLATFORM_RENDER_FORMAT);
			Graphics.Blit(renderTexture2, temporary4);
			temporary4.filterMode = DownsamplingFilterMode;
			Graphics.Blit(temporary4, temporary3, m_partialCopyMaterial);
			RenderTexture.ReleaseTemporary(temporary4);
		}
		if (GameManager.Options.MotionEnhancementMode == GameOptions.PixelatorMotionEnhancementMode.ENHANCED_EXPENSIVE)
		{
			m_camera.transform.position = new Vector3(Mathf.Floor(vector2.x * (16f * ScaleTileScale)), Mathf.Floor(vector2.y * (16f * ScaleTileScale)), Mathf.Floor(vector2.z * (16f * ScaleTileScale))) / (16f * ScaleTileScale);
			m_camera.transform.position -= new Vector3(0.0625f, 0.0625f, 0f);
			camera.orthographicSize = m_camera.orthographicSize;
			camera.targetTexture = temporary3;
			camera.SetTargetBuffers(temporary3.colorBuffer, renderTexture4.depthBuffer);
			camera.clearFlags = CameraClearFlags.Nothing;
			camera.cullingMask = cm_unpixelated;
			camera.Render();
			camera.orthographicSize = m_camera.orthographicSize * ((float)num3 / ((float)CurrentMacroResolutionY / (float)overrideTileScale));
			m_camera.transform.position += new Vector3(0.0625f, 0.0625f, 0f);
			m_camera.transform.position = vector2 + vector;
		}
		RenderTexture renderTexture5 = RenderTexture.GetTemporary(source.width, source.height, PLATFORM_DEPTH, PLATFORM_RENDER_FORMAT);
		if (!m_gammaLocked)
		{
			m_gammaEffect.ActiveMaterial.SetFloat(m_gammaID, 2f - GameManager.Options.Gamma + m_gammaAdjustment);
		}
		if (m_vignetteMaterial != null)
		{
			m_vignetteMaterial.SetTexture(m_gBufferID, temporary2);
			Graphics.Blit(temporary3, renderTexture5, m_vignetteMaterial);
		}
		else
		{
			Graphics.Blit(temporary3, renderTexture5);
		}
		m_camera.transform.position = new Vector3(Mathf.Floor(vector2.x * (16f * ScaleTileScale)), Mathf.Floor(vector2.y * (16f * ScaleTileScale)), Mathf.Floor(vector2.z * (16f * ScaleTileScale))) / (16f * ScaleTileScale);
		m_camera.transform.position -= new Vector3(0.0625f, 0.0625f, 0f);
		camera.orthographicSize = m_camera.orthographicSize;
		camera.targetTexture = renderTexture5;
		camera.clearFlags = CameraClearFlags.Depth;
		camera.cullingMask = cm_unoccluded;
		camera.Render();
		camera.orthographicSize = m_camera.orthographicSize * ((float)num3 / ((float)CurrentMacroResolutionY / (float)overrideTileScale));
		m_camera.transform.position += new Vector3(0.0625f, 0.0625f, 0f);
		m_camera.transform.position = vector2 + vector;
		if (AdditionalRenderPasses.Count > 0)
		{
			for (int i = 0; i < AdditionalRenderPasses.Count; i++)
			{
				if (AdditionalRenderPasses[i] == null)
				{
					AdditionalRenderPasses.RemoveAt(i);
					i--;
					continue;
				}
				RenderTexture temporary5 = RenderTexture.GetTemporary(renderTexture5.width, renderTexture5.height, renderTexture5.depth, renderTexture5.format);
				Graphics.Blit(renderTexture5, temporary5, AdditionalRenderPasses[i], 0);
				if (AdditionalRenderPassesInitialized[i])
				{
					RenderTexture.ReleaseTemporary(renderTexture5);
					renderTexture5 = temporary5;
				}
				else
				{
					AdditionalRenderPassesInitialized[i] = true;
					RenderTexture.ReleaseTemporary(temporary5);
				}
			}
		}
		else if (!m_hasInitializedAdditionalRenderTarget)
		{
			RenderTexture temporary6 = RenderTexture.GetTemporary(renderTexture5.width, renderTexture5.height, renderTexture5.depth, renderTexture5.format);
			Graphics.Blit(renderTexture5, temporary6);
			RenderTexture.ReleaseTemporary(renderTexture5);
			renderTexture5 = temporary6;
			m_hasInitializedAdditionalRenderTarget = true;
		}
		if (m_fadeMaterial != null)
		{
			m_fadeMaterial.SetFloat(m_saturationID, saturation);
			m_fadeMaterial.SetFloat(m_fadeID, fade);
		}
		if (DoFinalNonFadedLayer && m_fadeMaterial != null)
		{
			if (CompositePixelatedUnfadedLayer && SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11)
			{
				RenderTexture temporary7 = RenderTexture.GetTemporary(source.width, source.height);
				Graphics.Blit(renderTexture5, temporary7, m_fadeMaterial);
				Graphics.Blit(temporary7, target);
				m_camera.transform.position -= new Vector3(0.0625f, 0.0625f, 0f);
				camera.orthographicSize = m_camera.orthographicSize;
				RenderTexture temporary8 = RenderTexture.GetTemporary(BraveCameraUtility.H_PIXELS, BraveCameraUtility.V_PIXELS);
				temporary8.filterMode = FilterMode.Point;
				camera.targetTexture = temporary8;
				camera.clearFlags = CameraClearFlags.Color;
				camera.backgroundColor = new Color(1f, 0f, 0f);
				camera.cullingMask = cm_unfaded;
				camera.Render();
				if (m_compositor == null)
				{
					m_compositor = new Material(ShaderCache.Acquire("Hidden/SimpleCompositor"));
				}
				m_compositor.SetTexture("_BaseTex", temporary7);
				m_compositor.SetTexture("_LayerTex", temporary8);
				Graphics.Blit(temporary8, target, m_compositor);
				RenderTexture.ReleaseTemporary(temporary8);
				RenderTexture.ReleaseTemporary(temporary7);
				camera.orthographicSize = m_camera.orthographicSize * ((float)num3 / ((float)CurrentMacroResolutionY / (float)overrideTileScale));
				m_camera.transform.position += new Vector3(0.0625f, 0.0625f, 0f);
			}
			else
			{
				RenderTexture temporary9 = RenderTexture.GetTemporary(source.width, source.height);
				Graphics.Blit(renderTexture5, temporary9, m_fadeMaterial);
				Graphics.Blit(temporary9, target);
				if (CompositePixelatedUnfadedLayer)
				{
					m_camera.transform.position -= new Vector3(0.0625f, 0.0625f, 0f);
					camera.orthographicSize = m_camera.orthographicSize;
					RenderTexture temporary10 = RenderTexture.GetTemporary(BraveCameraUtility.H_PIXELS, BraveCameraUtility.V_PIXELS);
					temporary10.filterMode = FilterMode.Point;
					camera.targetTexture = temporary10;
					camera.clearFlags = CameraClearFlags.Color;
					camera.backgroundColor = new Color(1f, 0f, 0f);
					camera.cullingMask = cm_unfaded;
					camera.Render();
					if (m_compositor == null)
					{
						m_compositor = new Material(ShaderCache.Acquire("Hidden/SimpleCompositor"));
					}
					m_compositor.SetTexture("_BaseTex", temporary9);
					m_compositor.SetTexture("_LayerTex", temporary10);
					Graphics.Blit(temporary10, target, m_compositor);
					RenderTexture.ReleaseTemporary(temporary10);
					camera.orthographicSize = m_camera.orthographicSize * ((float)num3 / ((float)CurrentMacroResolutionY / (float)overrideTileScale));
					m_camera.transform.position += new Vector3(0.0625f, 0.0625f, 0f);
				}
				else
				{
					m_camera.transform.position = new Vector3(Mathf.Floor(vector2.x * (16f * ScaleTileScale)), Mathf.Floor(vector2.y * (16f * ScaleTileScale)), Mathf.Floor(vector2.z * (16f * ScaleTileScale))) / (16f * ScaleTileScale);
					m_camera.transform.position -= new Vector3(0.0625f, 0.0625f, 0f);
					camera.orthographicSize = m_camera.orthographicSize;
					camera.targetTexture = target;
					camera.clearFlags = CameraClearFlags.Depth;
					camera.cullingMask = cm_unfaded;
					camera.Render();
					camera.orthographicSize = m_camera.orthographicSize * ((float)num3 / ((float)CurrentMacroResolutionY / (float)overrideTileScale));
					m_camera.transform.position += new Vector3(0.0625f, 0.0625f, 0f);
					m_camera.transform.position = vector2 + vector;
				}
				RenderTexture.ReleaseTemporary(temporary9);
			}
		}
		else if (m_fadeMaterial != null)
		{
			Graphics.Blit(renderTexture5, target, m_fadeMaterial);
		}
		else
		{
			Graphics.Blit(renderTexture5, target);
		}
		RenderTexture.ReleaseTemporary(temporary2);
		if (GameManager.Options.MotionEnhancementMode == GameOptions.PixelatorMotionEnhancementMode.ENHANCED_EXPENSIVE)
		{
			RenderTexture.ReleaseTemporary(renderTexture4);
		}
		if (isCurrentlyZoomIntermediate)
		{
			renderTexture2.Release();
			renderTexture2 = null;
		}
		else
		{
			RenderTexture.ReleaseTemporary(renderTexture2);
		}
		RenderTexture.ReleaseTemporary(temporary3);
		RenderTexture.ReleaseTemporary(renderTexture5);
		m_camera.cullingMask = 0;
		camera.cullingMask = 0;
	}

	private void RenderEnemyProjectileMasks(Camera stackCamera, RenderTexture source)
	{
		int width = ((GameManager.Options.MotionEnhancementMode != 0) ? (CurrentMacroResolutionX / overrideTileScale) : source.width);
		int height = ((GameManager.Options.MotionEnhancementMode != 0) ? (CurrentMacroResolutionY / overrideTileScale) : source.height);
		m_UnblurredProjectileMaskTex = RenderTexture.GetTemporary(width, height, PLATFORM_DEPTH, RenderTextureFormat.Default);
		m_UnblurredProjectileMaskTex.filterMode = FilterMode.Point;
		m_BlurredProjectileMaskTex = RenderTexture.GetTemporary(m_UnblurredProjectileMaskTex.width, m_UnblurredProjectileMaskTex.height, PLATFORM_DEPTH, RenderTextureFormat.Default);
		m_BlurredProjectileMaskTex.filterMode = FilterMode.Point;
		if (m_blurMaterial == null)
		{
			m_blurMaterial = new Material(GetComponent<SENaturalBloomAndDirtyLens>().shader);
			m_blurMaterial.hideFlags = HideFlags.HideAndDontSave;
		}
		int num = LayerMask.NameToLayer("PlayerAndProjectiles");
		ReadOnlyCollection<Projectile> allProjectiles = StaticReferenceManager.AllProjectiles;
		for (int i = 0; i < allProjectiles.Count; i++)
		{
			if (!allProjectiles[i].neverMaskThis)
			{
				allProjectiles[i].CacheLayer(num);
			}
		}
		stackCamera.clearFlags = CameraClearFlags.Color;
		stackCamera.cullingMask = 1 << num;
		stackCamera.SetReplacementShader(m_simpleSpriteMaskShader, string.Empty);
		stackCamera.targetTexture = m_UnblurredProjectileMaskTex;
		stackCamera.Render();
		stackCamera.ResetReplacementShader();
		stackCamera.clearFlags = CameraClearFlags.Nothing;
		stackCamera.cullingMask = cm_fg & ~(1 << num);
		stackCamera.SetReplacementShader(ShaderCache.Acquire("Brave/Internal/Black"), string.Empty);
		stackCamera.Render();
		stackCamera.ResetReplacementShader();
		for (int j = 0; j < 3; j++)
		{
			m_blurMaterial.SetFloat("_BlurSize", ProjectileMaskBlurSize * 0.5f + (float)j);
			RenderTexture temporary = RenderTexture.GetTemporary(m_UnblurredProjectileMaskTex.width, m_UnblurredProjectileMaskTex.height, 0, RenderTextureFormat.Default);
			temporary.filterMode = FilterMode.Point;
			Graphics.Blit((j != 0) ? m_BlurredProjectileMaskTex : m_UnblurredProjectileMaskTex, temporary, m_blurMaterial, 2);
			RenderTexture.ReleaseTemporary(m_BlurredProjectileMaskTex);
			m_BlurredProjectileMaskTex = temporary;
			temporary = RenderTexture.GetTemporary(m_UnblurredProjectileMaskTex.width, m_UnblurredProjectileMaskTex.height, 0, RenderTextureFormat.Default);
			temporary.filterMode = FilterMode.Point;
			Graphics.Blit(m_BlurredProjectileMaskTex, temporary, m_blurMaterial, 3);
			RenderTexture.ReleaseTemporary(m_BlurredProjectileMaskTex);
			m_BlurredProjectileMaskTex = temporary;
		}
		for (int k = 0; k < allProjectiles.Count; k++)
		{
			if (!allProjectiles[k].neverMaskThis)
			{
				allProjectiles[k].DecacheLayer();
			}
		}
	}

	private void RenderGBufferCheap(RenderTexture source, Camera stackCamera, RenderBuffer depthTarget, RenderTexture TempBuffer_Lighting, int coreBufferWidth, int coreBufferHeight)
	{
		RenderTexture temporary = RenderTexture.GetTemporary(CurrentMacroResolutionX / (GBUFFER_DESCALE * overrideTileScale), CurrentMacroResolutionY / (GBUFFER_DESCALE * overrideTileScale), 0, PLATFORM_RENDER_FORMAT);
		Graphics.Blit(m_smallBlackTexture, temporary);
		Graphics.Blit(temporary, temporary, m_pointLightMaterial, 1);
		RenderTexture renderTexture = null;
		if (IsInIntro || GameManager.Options.LightingQuality != 0)
		{
			renderTexture = RenderTexture.GetTemporary(coreBufferWidth, coreBufferHeight, PLATFORM_DEPTH, RenderTextureFormat.Default);
			renderTexture.filterMode = FilterMode.Point;
			Graphics.Blit(m_smallBlackTexture, renderTexture);
			int cullingMask = ((GameManager.Options.MotionEnhancementMode != 0) ? cm_gbuffer : cm_gbufferSimple);
			stackCamera.clearFlags = CameraClearFlags.Nothing;
			stackCamera.cullingMask = cullingMask;
			stackCamera.targetTexture = renderTexture;
			stackCamera.SetTargetBuffers(renderTexture.colorBuffer, depthTarget);
			stackCamera.SetReplacementShader(m_simpleSpriteMaskShader, "UnlitTilted");
			stackCamera.Render();
			stackCamera.ResetReplacementShader();
		}
		for (int i = 0; i < AdditionalBraveLights.Count; i++)
		{
			if (!AdditionalBraveLights[i] || !AdditionalBraveLights[i].gameObject.activeSelf || !AdditionalBraveLights[i].UsesCustomMaterial)
			{
				continue;
			}
			AdditionalBraveLight additionalBraveLight = AdditionalBraveLights[i];
			float lightRadius = additionalBraveLight.LightRadius;
			float lightIntensity = additionalBraveLight.LightIntensity;
			if (lightIntensity != 0f)
			{
				Vector2 vector = ((!additionalBraveLight.sprite) ? ((Vector2)additionalBraveLight.transform.position) : additionalBraveLight.sprite.WorldCenter);
				Vector2 vector2 = stackCamera.transform.position.XY();
				if (!LightCulled(vector, vector2, lightRadius, stackCamera.orthographicSize, BraveCameraUtility.ASPECT))
				{
					Material customLightMaterial = additionalBraveLight.CustomLightMaterial;
					customLightMaterial.SetVector(m_cameraWSID, vector2.ToVector4());
					customLightMaterial.SetFloat(m_cameraOrthoSizeID, stackCamera.orthographicSize);
					customLightMaterial.SetFloat(m_cameraOrthoSizeXID, stackCamera.orthographicSize * stackCamera.aspect);
					customLightMaterial.SetVector(m_lightPosID, vector.ToVector4());
					customLightMaterial.SetColor(m_lightColorID, additionalBraveLight.LightColor);
					customLightMaterial.SetFloat(m_lightRadiusID, lightRadius);
					customLightMaterial.SetFloat(m_lightIntensityID, lightIntensity);
					Graphics.Blit(temporary, temporary, customLightMaterial, 0);
				}
			}
		}
		if (false && GameManager.Instance.Dungeon.PlayerIsLight && !GameManager.Instance.IsLoadingLevel && (bool)GameManager.Instance.PrimaryPlayer)
		{
			Vector2 vector3 = stackCamera.transform.position.XY();
			m_pointLightMaterialFast.SetVector(m_cameraWSID, vector3.ToVector4());
			m_pointLightMaterialFast.SetFloat(m_cameraOrthoSizeID, stackCamera.orthographicSize);
			m_pointLightMaterialFast.SetFloat(m_cameraOrthoSizeXID, stackCamera.orthographicSize * stackCamera.aspect);
			float playerLightRadius = GameManager.Instance.Dungeon.PlayerLightRadius;
			float value = GameManager.Instance.Dungeon.PlayerLightIntensity / 5f;
			Vector2 centerPosition = GameManager.Instance.PrimaryPlayer.CenterPosition;
			if (!LightCulled(centerPosition, vector3, playerLightRadius, stackCamera.orthographicSize, BraveCameraUtility.ASPECT))
			{
				m_pointLightMaterialFast.SetVector(m_lightPosID, centerPosition.ToVector4());
				m_pointLightMaterialFast.SetColor(m_lightColorID, GameManager.Instance.Dungeon.PlayerLightColor);
				m_pointLightMaterialFast.SetFloat(m_lightRadiusID, playerLightRadius);
				m_pointLightMaterialFast.SetFloat(m_lightIntensityID, value);
				Graphics.Blit(temporary, temporary, m_pointLightMaterialFast, 0);
			}
		}
		if (renderTexture == null)
		{
			Graphics.Blit(temporary, TempBuffer_Lighting);
		}
		else
		{
			RenderTexture temporary2 = RenderTexture.GetTemporary(source.width, source.height, PLATFORM_DEPTH, PLATFORM_RENDER_FORMAT);
			Graphics.Blit(renderTexture, temporary2);
			m_gbufferMaskMaterial.SetTexture(m_lightMaskTexID, temporary2);
			Graphics.Blit(temporary, TempBuffer_Lighting, m_gbufferMaskMaterial);
			RenderTexture.ReleaseTemporary(temporary2);
			RenderTexture.ReleaseTemporary(renderTexture);
		}
		RenderTexture.ReleaseTemporary(temporary);
	}

	private void RenderGBufferScaling(RenderTexture source, Camera stackCamera, RenderBuffer depthTarget, RenderTexture TempBuffer_Lighting)
	{
		int width = CurrentMacroResolutionX / overrideTileScale;
		int height = CurrentMacroResolutionY / overrideTileScale;
		if (ActuallyRenderGBuffer)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(CurrentMacroResolutionX / (GBUFFER_DESCALE * overrideTileScale), CurrentMacroResolutionY / (GBUFFER_DESCALE * overrideTileScale), 0, PLATFORM_RENDER_FORMAT);
			Graphics.Blit(m_smallBlackTexture, temporary);
			Graphics.Blit(temporary, temporary, m_pointLightMaterial, 1);
			RenderTexture renderTexture = null;
			RenderTexture renderTexture2 = null;
			if (IsInIntro || GameManager.Options.LightingQuality != 0)
			{
				renderTexture = RenderTexture.GetTemporary(width, height, PLATFORM_DEPTH, RenderTextureFormat.Default);
				renderTexture.filterMode = FilterMode.Point;
				Graphics.Blit(m_smallBlackTexture, renderTexture);
				int cullingMask = ((GameManager.Options.MotionEnhancementMode != 0) ? cm_gbuffer : cm_gbufferSimple);
				stackCamera.clearFlags = CameraClearFlags.Nothing;
				stackCamera.cullingMask = cullingMask;
				stackCamera.targetTexture = renderTexture;
				stackCamera.SetTargetBuffers(renderTexture.colorBuffer, depthTarget);
				stackCamera.SetReplacementShader(m_simpleSpriteMaskShader, "UnlitTilted");
				stackCamera.Render();
				stackCamera.ResetReplacementShader();
				Vector2 vector = stackCamera.transform.position.XY();
				m_pointLightMaterial.SetVector(m_cameraWSID, vector.ToVector4());
				m_pointLightMaterial.SetFloat(m_cameraOrthoSizeID, stackCamera.orthographicSize);
				m_pointLightMaterial.SetFloat(m_cameraOrthoSizeXID, stackCamera.orthographicSize * stackCamera.aspect);
				m_pointLightMaterialFast.SetVector(m_cameraWSID, vector.ToVector4());
				m_pointLightMaterialFast.SetFloat(m_cameraOrthoSizeID, stackCamera.orthographicSize);
				m_pointLightMaterialFast.SetFloat(m_cameraOrthoSizeXID, stackCamera.orthographicSize * stackCamera.aspect);
				for (int i = 0; i < ShadowSystem.AllLights.Count; i++)
				{
					ShadowSystem shadowSystem = ShadowSystem.AllLights[i];
					if (!shadowSystem || !shadowSystem.gameObject.activeSelf)
					{
						continue;
					}
					bool flag = shadowSystem.uLightCookie == null;
					Material material = ((!flag) ? m_pointLightMaterial : m_pointLightMaterialFast);
					Vector2 vector2 = shadowSystem.transform.position.XY();
					if (!LightCulled(vector2, vector, shadowSystem.uLightRange, stackCamera.orthographicSize, BraveCameraUtility.ASPECT))
					{
						material.SetVector(m_lightPosID, vector2.ToVector4());
						material.SetColor(m_lightColorID, shadowSystem.uLightColor);
						material.SetFloat(m_lightRadiusID, shadowSystem.uLightRange);
						material.SetFloat(m_lightIntensityID, shadowSystem.uLightIntensity * pointLightMultiplier);
						if (!flag)
						{
							material.SetTexture(m_lightCookieID, shadowSystem.uLightCookie);
							material.SetFloat(m_lightCookieAngleID, shadowSystem.uLightCookieAngle);
						}
						Graphics.Blit(temporary, temporary, material, 0);
					}
				}
				for (int j = 0; j < AdditionalBraveLights.Count; j++)
				{
					if (!AdditionalBraveLights[j] || !AdditionalBraveLights[j].gameObject.activeSelf)
					{
						continue;
					}
					AdditionalBraveLight additionalBraveLight = AdditionalBraveLights[j];
					float lightRadius = additionalBraveLight.LightRadius;
					float lightIntensity = additionalBraveLight.LightIntensity;
					if (lightIntensity == 0f)
					{
						continue;
					}
					Vector2 vector3 = ((!additionalBraveLight.sprite) ? ((Vector2)additionalBraveLight.transform.position) : additionalBraveLight.sprite.WorldCenter);
					if (!LightCulled(vector3, vector, lightRadius, stackCamera.orthographicSize, BraveCameraUtility.ASPECT))
					{
						Material material2 = m_pointLightMaterialFast;
						if (additionalBraveLight.UsesCustomMaterial)
						{
							material2 = additionalBraveLight.CustomLightMaterial;
							material2.SetVector(m_cameraWSID, vector.ToVector4());
							material2.SetFloat(m_cameraOrthoSizeID, stackCamera.orthographicSize);
							material2.SetFloat(m_cameraOrthoSizeXID, stackCamera.orthographicSize * stackCamera.aspect);
						}
						else if (additionalBraveLight.UsesCone)
						{
							material2 = m_pointLightMaterial;
							material2.SetFloat("_LightAngle", additionalBraveLight.LightAngle);
							material2.SetFloat("_LightOrient", additionalBraveLight.LightOrient);
						}
						material2.SetVector(m_lightPosID, vector3.ToVector4());
						material2.SetColor(m_lightColorID, additionalBraveLight.LightColor);
						material2.SetFloat(m_lightRadiusID, lightRadius);
						material2.SetFloat(m_lightIntensityID, lightIntensity);
						Graphics.Blit(temporary, temporary, material2, 0);
					}
				}
				if (GameManager.Instance.Dungeon.PlayerIsLight && !GameManager.Instance.IsLoadingLevel && (bool)GameManager.Instance.PrimaryPlayer)
				{
					float playerLightRadius = GameManager.Instance.Dungeon.PlayerLightRadius;
					float playerLightIntensity = GameManager.Instance.Dungeon.PlayerLightIntensity;
					Vector2 centerPosition = GameManager.Instance.PrimaryPlayer.CenterPosition;
					if (!LightCulled(centerPosition, vector, playerLightRadius, stackCamera.orthographicSize, BraveCameraUtility.ASPECT))
					{
						m_pointLightMaterialFast.SetVector(m_lightPosID, centerPosition.ToVector4());
						m_pointLightMaterialFast.SetColor(m_lightColorID, GameManager.Instance.Dungeon.PlayerLightColor);
						m_pointLightMaterialFast.SetFloat(m_lightRadiusID, playerLightRadius);
						m_pointLightMaterialFast.SetFloat(m_lightIntensityID, playerLightIntensity);
						Graphics.Blit(temporary, temporary, m_pointLightMaterialFast, 0);
					}
				}
			}
			if (renderTexture == null)
			{
				Graphics.Blit(temporary, TempBuffer_Lighting);
			}
			else
			{
				RenderTexture temporary2 = RenderTexture.GetTemporary(source.width, source.height, PLATFORM_DEPTH, PLATFORM_RENDER_FORMAT);
				Graphics.Blit(renderTexture, temporary2);
				m_gbufferMaskMaterial.SetTexture(m_lightMaskTexID, renderTexture);
				Graphics.Blit(temporary, TempBuffer_Lighting, m_gbufferMaskMaterial);
				RenderTexture.ReleaseTemporary(temporary2);
				RenderTexture.ReleaseTemporary(renderTexture);
				if (renderTexture2 != null)
				{
					RenderTexture.ReleaseTemporary(renderTexture2);
				}
			}
			RenderTexture.ReleaseTemporary(temporary);
		}
		else
		{
			Graphics.Blit(m_smallBlackTexture, TempBuffer_Lighting);
			RenderSettings.ambientMode = AmbientMode.Flat;
			RenderSettings.ambientLight = Color.white;
			Graphics.Blit(TempBuffer_Lighting, TempBuffer_Lighting, m_pointLightMaterial, 1);
		}
	}

	private bool LightCulled(Vector2 lightPosition, Vector2 cameraPosition, float lightRange, float orthoSize, float aspect)
	{
		return Vector2.Distance(lightPosition, cameraPosition) > lightRange + orthoSize * LightCullFactor * aspect;
	}

	private void RenderGBuffer(RenderTexture source, Camera stackCamera, RenderBuffer depthTarget, RenderTexture TempBuffer_Lighting, Vector3 cachedPosition, Vector3 quantizedPosition)
	{
		int width = CurrentMacroResolutionX / overrideTileScale + extraPixels;
		int num = CurrentMacroResolutionY / overrideTileScale + extraPixels;
		Vector3 vector = ((!IsInIntro) ? CameraController.PLATFORM_CAMERA_OFFSET : Vector3.zero);
		if (ActuallyRenderGBuffer)
		{
			RenderTexture temporary = RenderTexture.GetTemporary(CurrentMacroResolutionX / (GBUFFER_DESCALE * overrideTileScale), CurrentMacroResolutionY / (GBUFFER_DESCALE * overrideTileScale), 0, PLATFORM_RENDER_FORMAT);
			Graphics.Blit(m_smallBlackTexture, temporary);
			Graphics.Blit(temporary, temporary, m_pointLightMaterial, 1);
			RenderTexture renderTexture = null;
			RenderTexture renderTexture2 = null;
			if (IsInIntro || GameManager.Options.LightingQuality != 0)
			{
				renderTexture = RenderTexture.GetTemporary(width, num, PLATFORM_DEPTH, RenderTextureFormat.Default);
				renderTexture.filterMode = FilterMode.Point;
				Graphics.Blit(m_smallBlackTexture, renderTexture);
				int cullingMask = ((GameManager.Options.MotionEnhancementMode != 0) ? cm_gbuffer : cm_gbufferSimple);
				stackCamera.clearFlags = CameraClearFlags.Nothing;
				stackCamera.cullingMask = cullingMask;
				stackCamera.targetTexture = renderTexture;
				stackCamera.SetTargetBuffers(renderTexture.colorBuffer, depthTarget);
				stackCamera.SetReplacementShader(m_simpleSpriteMaskShader, "UnlitTilted");
				stackCamera.Render();
				if (GameManager.Options.MotionEnhancementMode == GameOptions.PixelatorMotionEnhancementMode.ENHANCED_EXPENSIVE)
				{
					int cullingMask2 = cm_unpixelated;
					renderTexture2 = RenderTexture.GetTemporary(source.width, source.height, PLATFORM_DEPTH, RenderTextureFormat.Default);
					Graphics.Blit(m_smallBlackTexture, renderTexture2);
					m_camera.transform.position = new Vector3(Mathf.Floor(cachedPosition.x * (16f * ScaleTileScale)), Mathf.Floor(cachedPosition.y * (16f * ScaleTileScale)), Mathf.Floor(cachedPosition.z * (16f * ScaleTileScale))) / (16f * ScaleTileScale);
					m_camera.transform.position -= new Vector3(0.0625f, 0.0625f, 0f);
					stackCamera.orthographicSize = m_camera.orthographicSize;
					stackCamera.cullingMask = cullingMask2;
					stackCamera.targetTexture = renderTexture2;
					stackCamera.SetReplacementShader(m_simpleSpriteMaskUnpixelatedShader, "UnlitTilted");
					stackCamera.Render();
					stackCamera.orthographicSize = m_camera.orthographicSize * ((float)num / ((float)CurrentMacroResolutionY / (float)overrideTileScale));
					m_camera.transform.position = cachedPosition + vector;
				}
				stackCamera.ResetReplacementShader();
				Vector2 vector2 = stackCamera.transform.position.XY();
				m_pointLightMaterial.SetVector(m_cameraWSID, vector2.ToVector4());
				m_pointLightMaterial.SetFloat(m_cameraOrthoSizeID, stackCamera.orthographicSize);
				m_pointLightMaterial.SetFloat(m_cameraOrthoSizeXID, stackCamera.orthographicSize * stackCamera.aspect);
				m_pointLightMaterialFast.SetVector(m_cameraWSID, vector2.ToVector4());
				m_pointLightMaterialFast.SetFloat(m_cameraOrthoSizeID, stackCamera.orthographicSize);
				m_pointLightMaterialFast.SetFloat(m_cameraOrthoSizeXID, stackCamera.orthographicSize * stackCamera.aspect);
				for (int i = 0; i < ShadowSystem.AllLights.Count; i++)
				{
					ShadowSystem shadowSystem = ShadowSystem.AllLights[i];
					if (!shadowSystem || !shadowSystem.gameObject.activeSelf)
					{
						continue;
					}
					bool flag = shadowSystem.uLightCookie == null;
					Material material = ((!flag) ? m_pointLightMaterial : m_pointLightMaterialFast);
					Vector2 vector3 = shadowSystem.transform.position.XY();
					if (!LightCulled(vector3, vector2, shadowSystem.uLightRange, stackCamera.orthographicSize, BraveCameraUtility.ASPECT))
					{
						material.SetVector(m_lightPosID, vector3.ToVector4());
						material.SetColor(m_lightColorID, shadowSystem.uLightColor);
						material.SetFloat(m_lightRadiusID, shadowSystem.uLightRange);
						material.SetFloat(m_lightIntensityID, shadowSystem.uLightIntensity * pointLightMultiplier);
						if (!flag)
						{
							material.SetTexture(m_lightCookieID, shadowSystem.uLightCookie);
							material.SetFloat(m_lightCookieAngleID, shadowSystem.uLightCookieAngle);
						}
						Graphics.Blit(temporary, temporary, material, 0);
					}
				}
				for (int j = 0; j < AdditionalBraveLights.Count; j++)
				{
					if (!AdditionalBraveLights[j] || !AdditionalBraveLights[j].gameObject.activeSelf)
					{
						continue;
					}
					AdditionalBraveLight additionalBraveLight = AdditionalBraveLights[j];
					float lightRadius = additionalBraveLight.LightRadius;
					float lightIntensity = additionalBraveLight.LightIntensity;
					if (lightIntensity == 0f)
					{
						continue;
					}
					Vector2 vector4 = ((!additionalBraveLight.sprite) ? ((Vector2)additionalBraveLight.transform.position) : additionalBraveLight.sprite.WorldCenter);
					if (!LightCulled(vector4, vector2, lightRadius, stackCamera.orthographicSize, BraveCameraUtility.ASPECT))
					{
						Material material2 = m_pointLightMaterialFast;
						if (additionalBraveLight.UsesCustomMaterial)
						{
							material2 = additionalBraveLight.CustomLightMaterial;
							material2.SetVector(m_cameraWSID, vector2.ToVector4());
							material2.SetFloat(m_cameraOrthoSizeID, stackCamera.orthographicSize);
							material2.SetFloat(m_cameraOrthoSizeXID, stackCamera.orthographicSize * stackCamera.aspect);
						}
						else if (additionalBraveLight.UsesCone)
						{
							material2 = m_pointLightMaterial;
							material2.SetFloat("_LightAngle", additionalBraveLight.LightAngle);
							material2.SetFloat("_LightOrient", additionalBraveLight.LightOrient);
						}
						material2.SetVector(m_lightPosID, vector4.ToVector4());
						material2.SetColor(m_lightColorID, additionalBraveLight.LightColor);
						material2.SetFloat(m_lightRadiusID, lightRadius);
						material2.SetFloat(m_lightIntensityID, lightIntensity);
						Graphics.Blit(temporary, temporary, material2, 0);
					}
				}
				if (GameManager.Instance.Dungeon.PlayerIsLight && !GameManager.Instance.IsLoadingLevel && (bool)GameManager.Instance.PrimaryPlayer)
				{
					float playerLightRadius = GameManager.Instance.Dungeon.PlayerLightRadius;
					float playerLightIntensity = GameManager.Instance.Dungeon.PlayerLightIntensity;
					Vector2 centerPosition = GameManager.Instance.PrimaryPlayer.CenterPosition;
					if (!LightCulled(centerPosition, vector2, playerLightRadius, stackCamera.orthographicSize, BraveCameraUtility.ASPECT))
					{
						m_pointLightMaterialFast.SetVector(m_lightPosID, centerPosition.ToVector4());
						m_pointLightMaterialFast.SetColor(m_lightColorID, GameManager.Instance.Dungeon.PlayerLightColor);
						m_pointLightMaterialFast.SetFloat(m_lightRadiusID, playerLightRadius);
						m_pointLightMaterialFast.SetFloat(m_lightIntensityID, playerLightIntensity);
						Graphics.Blit(temporary, temporary, m_pointLightMaterialFast, 0);
					}
				}
			}
			else
			{
				for (int k = 0; k < AdditionalBraveLights.Count; k++)
				{
					if (!AdditionalBraveLights[k] || !AdditionalBraveLights[k].gameObject.activeSelf || !AdditionalBraveLights[k].UsesCustomMaterial)
					{
						continue;
					}
					AdditionalBraveLight additionalBraveLight2 = AdditionalBraveLights[k];
					Vector2 vector5 = stackCamera.transform.position.XY();
					float lightRadius2 = additionalBraveLight2.LightRadius;
					float lightIntensity2 = additionalBraveLight2.LightIntensity;
					if (lightIntensity2 == 0f)
					{
						continue;
					}
					Vector2 vector6 = ((!additionalBraveLight2.sprite) ? ((Vector2)additionalBraveLight2.transform.position) : additionalBraveLight2.sprite.WorldCenter);
					if (!LightCulled(vector6, vector5, lightRadius2, stackCamera.orthographicSize, BraveCameraUtility.ASPECT))
					{
						Material material3 = m_pointLightMaterialFast;
						if (additionalBraveLight2.UsesCustomMaterial)
						{
							material3 = additionalBraveLight2.CustomLightMaterial;
							material3.SetVector(m_cameraWSID, vector5.ToVector4());
							material3.SetFloat(m_cameraOrthoSizeID, stackCamera.orthographicSize);
							material3.SetFloat(m_cameraOrthoSizeXID, stackCamera.orthographicSize * stackCamera.aspect);
						}
						material3.SetVector(m_lightPosID, vector6.ToVector4());
						material3.SetColor(m_lightColorID, additionalBraveLight2.LightColor);
						material3.SetFloat(m_lightRadiusID, lightRadius2);
						material3.SetFloat(m_lightIntensityID, lightIntensity2);
						Graphics.Blit(temporary, temporary, material3, 0);
					}
				}
			}
			if (renderTexture == null)
			{
				Graphics.Blit(temporary, TempBuffer_Lighting);
			}
			else
			{
				RenderTexture temporary2 = RenderTexture.GetTemporary(source.width, source.height, PLATFORM_DEPTH, PLATFORM_RENDER_FORMAT);
				int num2 = Mathf.Max(Mathf.CeilToInt((float)source.width / (float)CurrentMacroResolutionX), Mathf.CeilToInt((float)source.height / (float)CurrentMacroResolutionY));
				if ((CurrentMacroResolutionX * num2 != source.width || CurrentMacroResolutionY * num2 != source.height) && (GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.MEDIUM || GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.HIGH || GameManager.Options.LightingQuality != 0))
				{
					RenderTexture temporary3 = RenderTexture.GetTemporary(CurrentMacroResolutionX * num2, CurrentMacroResolutionY * num2, PLATFORM_DEPTH, PLATFORM_RENDER_FORMAT);
					Graphics.Blit(renderTexture, temporary3);
					temporary3.filterMode = DownsamplingFilterMode;
					if (renderTexture2 != null)
					{
						m_gbufferLightMaskCombinerMaterial.SetTexture("_MainTex", temporary3);
						m_gbufferLightMaskCombinerMaterial.SetTexture("_LightTex", renderTexture2);
						Graphics.Blit(temporary3, temporary2, m_gbufferLightMaskCombinerMaterial);
					}
					else
					{
						Graphics.Blit(temporary3, temporary2, m_partialCopyMaterial);
					}
					RenderTexture.ReleaseTemporary(temporary3);
				}
				else if (renderTexture2 != null)
				{
					m_gbufferLightMaskCombinerMaterial.SetTexture("_MainTex", renderTexture);
					m_gbufferLightMaskCombinerMaterial.SetTexture("_LightTex", renderTexture2);
					Graphics.Blit(renderTexture, temporary2, m_gbufferLightMaskCombinerMaterial);
				}
				else
				{
					Graphics.Blit(renderTexture, temporary2, m_partialCopyMaterial);
				}
				m_gbufferMaskMaterial.SetTexture(m_lightMaskTexID, temporary2);
				Graphics.Blit(temporary, TempBuffer_Lighting, m_gbufferMaskMaterial);
				RenderTexture.ReleaseTemporary(temporary2);
				RenderTexture.ReleaseTemporary(renderTexture);
				if (renderTexture2 != null)
				{
					RenderTexture.ReleaseTemporary(renderTexture2);
				}
			}
			RenderTexture.ReleaseTemporary(temporary);
		}
		else
		{
			Graphics.Blit(m_smallBlackTexture, TempBuffer_Lighting);
			RenderSettings.ambientMode = AmbientMode.Flat;
			RenderSettings.ambientLight = Color.white;
			Graphics.Blit(TempBuffer_Lighting, TempBuffer_Lighting, m_pointLightMaterial, 1);
		}
	}

	public void CustomFade(float duration, float holdTime, Color startColor, Color endColor, float startScreenBrightness, float endScreenBrightness)
	{
		StartCoroutine(CustomFade_CR(duration, holdTime, startColor, endColor, startScreenBrightness, endScreenBrightness));
	}

	private IEnumerator CustomFade_CR(float duration, float holdTime, Color startColor, Color endColor, float startScreenBrightness, float endScreenBrightness)
	{
		if (holdTime > 0f)
		{
			m_fadeMaterial.SetColor(m_fadeColorID, startColor);
			fade = startScreenBrightness;
			while (holdTime > 0f)
			{
				holdTime -= m_deltaTime;
				yield return null;
			}
		}
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += m_deltaTime;
			float t = elapsed / duration;
			m_fadeMaterial.SetColor(m_fadeColorID, Color.Lerp(startColor, endColor, t));
			fade = Mathf.Lerp(startScreenBrightness, endScreenBrightness, t);
			yield return null;
		}
	}

	public void TriggerPastFadeIn()
	{
		StartCoroutine(HandlePastFadeIn());
	}

	private IEnumerator HandlePastFadeIn()
	{
		m_gammaLocked = true;
		float elapsed = -3f;
		float duration = 3f;
		float startGamma = Mathf.Max(0.05f, 2f - GameManager.Options.Gamma - 0.5f);
		while (elapsed < duration)
		{
			elapsed += BraveTime.DeltaTime;
			float t2 = elapsed / duration;
			t2 = (saturation = Mathf.Clamp01(t2));
			m_gammaEffect.ActiveMaterial.SetFloat(m_gammaID, Mathf.Lerp(startGamma, 2f - GameManager.Options.Gamma, t2));
			yield return null;
		}
		m_gammaLocked = false;
		saturation = 1f;
	}

	public void SetFadeColor(Color c)
	{
		if (m_fadeMaterial != null)
		{
			m_fadeMaterial.SetColor(m_fadeColorID, c);
		}
		fade = 1f - c.a;
	}

	public void FreezeFrame()
	{
		StartCoroutine(HandleFreezeFrame());
	}

	private IEnumerator HandleFreezeFrame()
	{
		float ela = 0f;
		float dura = 0.6f;
		while (ela < dura)
		{
			ela += GameManager.INVARIANT_DELTA_TIME;
			float t = ela / dura;
			SetFreezeFramePower(t);
			yield return null;
		}
	}

	private IEnumerator HandleTimedFreezeFrame(float duration, float holdDuration)
	{
		float ela2 = 0f;
		SetFreezeFramePower(1f, true);
		while (ela2 < holdDuration)
		{
			ela2 += GameManager.INVARIANT_DELTA_TIME;
			yield return null;
		}
		ela2 = 0f;
		while (ela2 < duration)
		{
			ela2 += GameManager.INVARIANT_DELTA_TIME;
			float t = ela2 / duration;
			SetFreezeFramePower(1f - t, true);
			yield return null;
		}
		ClearFreezeFrame();
	}

	public void TimedFreezeFrame(float duration, float holdDuration)
	{
		StartCoroutine(HandleTimedFreezeFrame(duration, holdDuration));
	}

	public void SetSaturationColorPower(Color satColor, float t)
	{
		m_gammaAdjustment = Mathf.Lerp((GameManager.Options.LightingQuality != 0) ? 0f : (-0.1f), -0.35f, t);
		m_fadeMaterial.SetColor("_SaturationColor", Color.Lerp(new Color(1f, 1f, 1f), satColor, t));
		saturation = Mathf.Lerp(1f, 0f, t);
		m_fadeMaterial.SetFloat(m_saturationID, saturation);
	}

	public void SetFreezeFramePower(float t, bool isCameraEffect = false)
	{
		m_gammaAdjustment = Mathf.Lerp((GameManager.Options.LightingQuality != 0) ? 0f : (-0.1f), -0.35f, t);
		m_fadeMaterial.SetColor("_SaturationColor", Color.Lerp(new Color(1f, 1f, 1f), new Color(0.825f, 0.7f, 0.3f), t));
		saturation = Mathf.Lerp(1f, 0f, t);
		if (isCameraEffect)
		{
			m_gammaAdjustment = Mathf.Lerp((GameManager.Options.LightingQuality != 0) ? 0f : (-0.1f), -0.6f, t);
		}
		m_fadeMaterial.SetFloat(m_saturationID, saturation);
	}

	public void ClearFreezeFrame()
	{
		OnChangedLightingQuality(GameManager.Options.LightingQuality);
		m_fadeMaterial.SetColor("_SaturationColor", new Color(1f, 1f, 1f));
		saturation = 1f;
		m_fadeMaterial.SetFloat(m_saturationID, 1f);
	}

	public void FadeToColor(float duration, Color c, bool reverse = false, float holdTime = 0f)
	{
		if (!m_fadeLocked)
		{
			StartCoroutine(FadeToColor_CR(duration, c, reverse, holdTime));
		}
	}

	public void FadeToBlack(float duration, bool reverse = false, float holdTime = 0f)
	{
		if (reverse || fade != 0f)
		{
			m_fadeLocked = true;
			StartCoroutine(FadeToColor_CR(duration, Color.black, reverse, holdTime));
		}
	}

	private IEnumerator FadeToColor_CR(float duration, Color targetColor, bool reverse = false, float hold = 0f)
	{
		float elapsed = 0f;
		float minFade = 1f - targetColor.a;
		if (hold > 0f)
		{
			m_fadeMaterial.SetColor(m_fadeColorID, targetColor);
			fade = ((!reverse) ? 1f : minFade);
			while (hold > 0f)
			{
				hold -= m_deltaTime;
				if (KillAllFades)
				{
					m_fadeLocked = false;
					yield break;
				}
				yield return null;
			}
		}
		while (elapsed < duration)
		{
			elapsed += m_deltaTime;
			float t = elapsed / duration;
			m_fadeMaterial.SetColor(m_fadeColorID, targetColor);
			float fadeFrac = ((!reverse) ? (1f - t) : t);
			fade = Mathf.Lerp(minFade, 1f, fadeFrac);
			if (KillAllFades)
			{
				m_fadeLocked = false;
				yield break;
			}
			yield return null;
		}
		fade = (reverse ? 1 : 0);
		m_fadeLocked = false;
	}

	public void HandleDamagedVignette(Vector2 damageDirection)
	{
		StartCoroutine(HandleDamagedVignette_CR());
	}

	private IEnumerator HandleDamagedVignette_CR()
	{
		float elapsed = 0f;
		float inDuration = 0.04f;
		float outDuration = 0.5f;
		while (elapsed < inDuration + outDuration)
		{
			elapsed += GameManager.INVARIANT_DELTA_TIME;
			float t2 = 0f;
			t2 = ((!(elapsed < inDuration)) ? (1f - Mathf.SmoothStep(0f, 1f, (elapsed - inDuration) / outDuration)) : Mathf.SmoothStep(0f, 1f, elapsed / inDuration));
			m_fadeMaterial.SetFloat("_DamagedPower", t2);
			yield return null;
		}
	}

	public void SetWindowbox(float targetFraction)
	{
		m_fadeMaterial.SetFloat("_WindowboxFrac", targetFraction);
	}

	public void LerpToLetterbox(float targetFraction, float duration)
	{
		if (duration <= 0f)
		{
			m_fadeMaterial.SetFloat("_LetterboxFrac", targetFraction);
		}
		else
		{
			StartCoroutine(LerpToLetterbox_CR(targetFraction, duration));
		}
	}

	private IEnumerator LerpToLetterbox_CR(float targetFraction, float duration)
	{
		float elapsed = 0f;
		float startFraction = m_fadeMaterial.GetFloat("_LetterboxFrac");
		while (elapsed < duration)
		{
			elapsed += m_deltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			m_fadeMaterial.SetFloat("_LetterboxFrac", Mathf.Lerp(startFraction, targetFraction, t));
			yield return null;
		}
	}

	public void CacheScreenSpacePositionsForDeathFrame(Vector2 playerPosition, Vector2 enemyPosition)
	{
		CachedPlayerViewportPoint = m_camera.WorldToViewportPoint(playerPosition.ToVector3ZUp());
		if (enemyPosition != Vector2.zero)
		{
			CachedEnemyViewportPoint = m_camera.WorldToViewportPoint(enemyPosition.ToVector3ZUp());
		}
		else
		{
			CachedEnemyViewportPoint = new Vector3(-1f, -1f, 0f);
		}
	}

	public RenderTexture GetCachedFrame()
	{
		if ((GameManager.Options.ShaderQuality == GameOptions.GenericHighMedLowOption.VERY_LOW || GameManager.Options.CurrentPreferredScalingMode == GameOptions.PreferredScalingMode.UNIFORM_SCALING_FAST) && !IsInIntro)
		{
			return GetCachedFrame_VeryLowSettings();
		}
		return GetComponent<GenericFullscreenEffect>().GetCachedFrame();
	}

	public void ClearCachedFrame()
	{
		ClearCachedFrame_VeryLowSettings();
		GetComponent<GenericFullscreenEffect>().ClearCachedFrame();
	}

	private Material GetMaterial(Shader shader)
	{
		Material value;
		if (_shaderMap.TryGetValue(shader, out value))
		{
			return value;
		}
		value = new Material(shader);
		_shaderMap.Add(shader, value);
		return value;
	}

	public static bool IsValidReflectionObject(tk2dBaseSprite source)
	{
		if (source.gameActor != null)
		{
			return true;
		}
		return false;
	}
}
