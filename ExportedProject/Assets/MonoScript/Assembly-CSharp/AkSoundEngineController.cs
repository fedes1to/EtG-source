using System;
using System.IO;
using System.Threading;
using UnityEngine;

public class AkSoundEngineController
{
	public static readonly string s_DefaultBasePath = Path.Combine("Audio", "GeneratedSoundBanks");

	public static string s_Language = "English(US)";

	public static int s_DefaultPoolSize = 16384;

	public static int s_LowerPoolSize = 16384;

	public static int s_StreamingPoolSize = 2048;

	public static int s_PreparePoolSize = 0;

	public static float s_MemoryCutoffThreshold = 0.95f;

	public static int s_MonitorPoolSize = 128;

	public static int s_MonitorQueuePoolSize = 64;

	public static int s_CallbackManagerBufferSize = 4;

	public static bool s_EngineLogging = true;

	public static int s_SpatialAudioPoolSize = 8194;

	public string basePath = s_DefaultBasePath;

	public string language = s_Language;

	public bool engineLogging = s_EngineLogging;

	private static AkSoundEngineController ms_Instance;

	public static AkSoundEngineController Instance
	{
		get
		{
			if (ms_Instance == null)
			{
				ms_Instance = new AkSoundEngineController();
			}
			return ms_Instance;
		}
	}

	~AkSoundEngineController()
	{
		if (ms_Instance == this)
		{
			ms_Instance = null;
		}
	}

	public static string GetDecodedBankFolder()
	{
		return "DecodedBanks";
	}

	public static string GetDecodedBankFullPath()
	{
		return Path.Combine(AkBasePathGetter.GetPlatformBasePath(), GetDecodedBankFolder());
	}

	public void LateUpdate()
	{
		AkCallbackManager.PostCallbacks();
		AkBankManager.DoUnloadBanks();
		AkSoundEngine.RenderAudio();
	}

	public void Init(AkInitializer akInitializer)
	{
		bool flag = AkSoundEngine.IsInitialized();
		engineLogging = akInitializer.engineLogging;
		AkLogger.Instance.Init();
		if (flag)
		{
			return;
		}
		Debug.Log("WwiseUnity: Initialize sound engine ...");
		basePath = akInitializer.basePath;
		language = akInitializer.language;
		AkMemSettings akMemSettings = new AkMemSettings();
		akMemSettings.uMaxNumPools = 20u;
		AkDeviceSettings akDeviceSettings = new AkDeviceSettings();
		AkSoundEngine.GetDefaultDeviceSettings(akDeviceSettings);
		AkStreamMgrSettings akStreamMgrSettings = new AkStreamMgrSettings();
		akStreamMgrSettings.uMemorySize = (uint)(akInitializer.streamingPoolSize * 1024);
		AkInitSettings akInitSettings = new AkInitSettings();
		AkSoundEngine.GetDefaultInitSettings(akInitSettings);
		akInitSettings.uDefaultPoolSize = (uint)(akInitializer.defaultPoolSize * 1024);
		akInitSettings.uMonitorPoolSize = (uint)(akInitializer.monitorPoolSize * 1024);
		akInitSettings.uMonitorQueuePoolSize = (uint)(akInitializer.monitorQueuePoolSize * 1024);
		akInitSettings.szPluginDLLPath = Path.Combine(Application.dataPath, "Plugins" + Path.DirectorySeparatorChar);
		AkPlatformInitSettings akPlatformInitSettings = new AkPlatformInitSettings();
		AkSoundEngine.GetDefaultPlatformInitSettings(akPlatformInitSettings);
		akPlatformInitSettings.uLEngineDefaultPoolSize = (uint)(akInitializer.lowerPoolSize * 1024);
		akPlatformInitSettings.fLEngineDefaultPoolRatioThreshold = akInitializer.memoryCutoffThreshold;
		AkMusicSettings akMusicSettings = new AkMusicSettings();
		AkSoundEngine.GetDefaultMusicSettings(akMusicSettings);
		AkSpatialAudioInitSettings akSpatialAudioInitSettings = new AkSpatialAudioInitSettings();
		akSpatialAudioInitSettings.uPoolSize = (uint)(akInitializer.spatialAudioPoolSize * 1024);
		akSpatialAudioInitSettings.uMaxSoundPropagationDepth = akInitializer.maxSoundPropagationDepth;
		akSpatialAudioInitSettings.uDiffractionFlags = (uint)akInitializer.diffractionFlags;
		AkSoundEngine.SetGameName(Application.productName);
		AKRESULT aKRESULT = AkSoundEngine.Init(akMemSettings, akStreamMgrSettings, akDeviceSettings, akInitSettings, akPlatformInitSettings, akMusicSettings, akSpatialAudioInitSettings, (uint)(akInitializer.preparePoolSize * 1024));
		if (aKRESULT != AKRESULT.AK_Success)
		{
			Debug.LogError("WwiseUnity: Failed to initialize the sound engine. Abort. :" + aKRESULT);
			AkSoundEngine.Term();
			return;
		}
		string soundbankBasePath = AkBasePathGetter.GetSoundbankBasePath();
		if (string.IsNullOrEmpty(soundbankBasePath))
		{
			Debug.LogError("WwiseUnity: Couldn't find soundbanks base path. Terminate sound engine.");
			AkSoundEngine.Term();
			return;
		}
		aKRESULT = AkSoundEngine.SetBasePath(soundbankBasePath);
		if (aKRESULT != AKRESULT.AK_Success)
		{
			Debug.LogError("WwiseUnity: Failed to set soundbanks base path. Terminate sound engine.");
			AkSoundEngine.Term();
			return;
		}
		string decodedBankFullPath = GetDecodedBankFullPath();
		AkSoundEngine.SetDecodedBankPath(decodedBankFullPath);
		AkSoundEngine.SetCurrentLanguage(language);
		AkSoundEngine.AddBasePath(Application.persistentDataPath + Path.DirectorySeparatorChar);
		AkSoundEngine.AddBasePath(decodedBankFullPath);
		aKRESULT = AkCallbackManager.Init(akInitializer.callbackManagerBufferSize * 1024);
		if (aKRESULT != AKRESULT.AK_Success)
		{
			Debug.LogError("WwiseUnity: Failed to initialize Callback Manager. Terminate sound engine.");
			AkSoundEngine.Term();
			return;
		}
		AkBankManager.Reset();
		Debug.Log("WwiseUnity: Sound engine initialized.");
		uint out_bankID;
		aKRESULT = AkSoundEngine.LoadBank("Init.bnk", -1, out out_bankID);
		if (aKRESULT != AKRESULT.AK_Success)
		{
			Debug.LogError("WwiseUnity: Failed load Init.bnk with result: " + aKRESULT);
		}
	}

	public void OnDisable()
	{
	}

	public void Terminate()
	{
		if (!AkSoundEngine.IsInitialized())
		{
			return;
		}
		AkSoundEngine.StopAll();
		AkSoundEngine.ClearBanks();
		AkSoundEngine.RenderAudio();
		int num = 5;
		do
		{
			int num2 = 0;
			do
			{
				num2 = AkCallbackManager.PostCallbacks();
				using (EventWaitHandle eventWaitHandle = new ManualResetEvent(false))
				{
					eventWaitHandle.WaitOne(TimeSpan.FromMilliseconds(1.0));
				}
			}
			while (num2 > 0);
			using (EventWaitHandle eventWaitHandle2 = new ManualResetEvent(false))
			{
				eventWaitHandle2.WaitOne(TimeSpan.FromMilliseconds(10.0));
			}
			num--;
		}
		while (num > 0);
		AkSoundEngine.Term();
		AkCallbackManager.PostCallbacks();
		AkCallbackManager.Term();
		AkBankManager.Reset();
	}

	public void OnApplicationPause(bool pauseStatus)
	{
		ActivateAudio(!pauseStatus);
	}

	public void OnApplicationFocus(bool focus)
	{
		ActivateAudio(focus);
	}

	private static void ActivateAudio(bool activate)
	{
		if (AkSoundEngine.IsInitialized())
		{
			if (activate)
			{
				AkSoundEngine.WakeupFromSuspend();
			}
			else
			{
				AkSoundEngine.Suspend();
			}
			AkSoundEngine.RenderAudio();
		}
	}
}
