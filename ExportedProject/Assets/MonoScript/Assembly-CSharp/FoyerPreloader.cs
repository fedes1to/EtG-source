using System.Collections;
using UnityEngine;

public class FoyerPreloader : MonoBehaviour
{
	public static bool IsFirstLoadScreen = true;

	public static bool IsRatLoad;

	public dfLabel LoadingLabel;

	public dfLanguageManager LanguageManager;

	public dfSprite Throbber;

	public dfSprite RatThrobber;

	private bool m_wasFirstLoadScreen;

	private bool m_isLoading;

	public void Awake()
	{
		if (IsFirstLoadScreen)
		{
			LoadingLabel.gameObject.SetActive(false);
			LanguageManager.enabled = false;
			m_wasFirstLoadScreen = true;
			IsFirstLoadScreen = false;
		}
		else if (IsRatLoad)
		{
			Throbber.IsVisible = false;
			RatThrobber.IsVisible = true;
			IsRatLoad = false;
		}
	}

	public void Update()
	{
		if (m_wasFirstLoadScreen && Time.frameCount >= 5 && !m_isLoading)
		{
			StartCoroutine(AsyncLoadFoyer());
			m_isLoading = true;
		}
	}

	private IEnumerator AsyncLoadFoyer()
	{
		DebugTime.Log("FoyerLoader.AsyncLoadFoyer()");
		GameManager.AttemptSoundEngineInitializationAsync();
		yield return StartCoroutine(ResourceManager.InitAsync());
		DebugTime.RecordStartTime();
		GameManager targetManager = (BraveResources.Load("_GameManager") as GameObject).GetComponent<GameManager>();
		DebugTime.Log("Preloaded GameManager");
		yield return null;
		EnemyDatabase enemyDatabasePreload = EnemyDatabase.Instance;
		yield return null;
		EncounterDatabase encounterDatabasePreload = EncounterDatabase.Instance;
		yield return null;
		while (!GameManager.AUDIO_ENABLED)
		{
			yield return null;
		}
		if (m_wasFirstLoadScreen)
		{
			Object.DontDestroyOnLoad(base.gameObject);
		}
		AssetBundle assetBundle = ResourceManager.LoadAssetBundle("foyer_001");
		DebugTime.RecordStartTime();
		ResourceManager.LoadLevelFromBundle(assetBundle);
		DebugTime.Log("Application.LoadLevel(foyer)");
		if (m_wasFirstLoadScreen)
		{
			DebugTime.Log("Starting to destroy the load screen");
			int skipFrames = 3;
			for (int i = 0; i < skipFrames; i++)
			{
				yield return null;
			}
			DebugTime.Log("Finished destroying the load screen");
			Object.Destroy(base.gameObject);
		}
	}
}
