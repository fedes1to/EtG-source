using UnityEngine;

[ExecuteInEditMode]
[DisallowMultipleComponent]
[RequireComponent(typeof(AkTerminator))]
[AddComponentMenu("Wwise/AkInitializer")]
public class AkInitializer : MonoBehaviour
{
	public string basePath = AkSoundEngineController.s_DefaultBasePath;

	public string language = AkSoundEngineController.s_Language;

	public int defaultPoolSize = AkSoundEngineController.s_DefaultPoolSize;

	public int lowerPoolSize = AkSoundEngineController.s_LowerPoolSize;

	public int streamingPoolSize = AkSoundEngineController.s_StreamingPoolSize;

	public int preparePoolSize = AkSoundEngineController.s_PreparePoolSize;

	public float memoryCutoffThreshold = AkSoundEngineController.s_MemoryCutoffThreshold;

	public int monitorPoolSize = AkSoundEngineController.s_MonitorPoolSize;

	public int monitorQueuePoolSize = AkSoundEngineController.s_MonitorQueuePoolSize;

	public int callbackManagerBufferSize = AkSoundEngineController.s_CallbackManagerBufferSize;

	public int spatialAudioPoolSize = AkSoundEngineController.s_SpatialAudioPoolSize;

	[Range(0f, 8f)]
	public uint maxSoundPropagationDepth = 8u;

	[Tooltip("Default Diffraction Flags combine all the diffraction flags")]
	public AkDiffractionFlags diffractionFlags = AkDiffractionFlags.DefaultDiffractionFlags;

	public bool engineLogging = AkSoundEngineController.s_EngineLogging;

	private static AkInitializer ms_Instance;

	public static string GetBasePath()
	{
		return AkSoundEngineController.Instance.basePath;
	}

	public static string GetCurrentLanguage()
	{
		return AkSoundEngineController.Instance.language;
	}

	private void Awake()
	{
		if ((bool)ms_Instance)
		{
			Object.DestroyImmediate(this);
			return;
		}
		ms_Instance = this;
		Object.DontDestroyOnLoad(this);
	}

	private void OnEnable()
	{
		if (ms_Instance == this)
		{
			AkSoundEngineController.Instance.Init(this);
		}
	}

	private void OnDisable()
	{
		if (ms_Instance == this)
		{
			AkSoundEngineController.Instance.OnDisable();
		}
	}

	private void OnDestroy()
	{
		if (ms_Instance == this)
		{
			ms_Instance = null;
		}
	}

	private void OnApplicationPause(bool pauseStatus)
	{
		if (ms_Instance == this)
		{
			AkSoundEngineController.Instance.OnApplicationPause(pauseStatus);
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		if (ms_Instance == this)
		{
			AkSoundEngineController.Instance.OnApplicationFocus(focus);
		}
	}

	private void OnApplicationQuit()
	{
		if (ms_Instance == this)
		{
			AkSoundEngineController.Instance.Terminate();
		}
	}

	private void LateUpdate()
	{
		if (ms_Instance == this)
		{
			AkSoundEngineController.Instance.LateUpdate();
		}
	}
}
