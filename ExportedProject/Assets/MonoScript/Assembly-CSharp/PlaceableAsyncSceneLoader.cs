using System.Collections;
using Dungeonator;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlaceableAsyncSceneLoader : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	public string asyncSceneName;

	public string asyncChunkIdentifier;

	public bool DoNoncombatSetup;

	public void ConfigureOnPlacement(RoomHandler room)
	{
		if (DoNoncombatSetup)
		{
			NoncombatSetup();
		}
		DebugTime.Log("PlaceableAsyncSceneLoader.LoadScene({0})", asyncSceneName);
		if (asyncSceneName == "Foyer")
		{
			LoadBundledScene("Foyer", "foyer_002");
		}
		else if (asyncSceneName == "Foyer_Coop")
		{
			LoadBundledScene("Foyer_Coop", "foyer_003");
		}
		else
		{
			SceneManager.LoadScene(asyncSceneName, LoadSceneMode.Additive);
		}
	}

	private IEnumerator WaitForChunkLoaded(AsyncOperation loader)
	{
		while (!loader.isDone)
		{
			yield return null;
		}
		GameObject rootObject = GameObject.Find(asyncChunkIdentifier);
		rootObject.transform.position = base.transform.position;
	}

	private void NoncombatSetup()
	{
		GameUIRoot.Instance.ForceHideGunPanel = true;
		GameUIRoot.Instance.ForceHideItemPanel = true;
	}

	private void LoadBundledScene(string sceneName, string bundleName)
	{
		AssetBundle assetBundle = ResourceManager.LoadAssetBundle(bundleName);
		DebugTime.RecordStartTime();
		ResourceManager.LoadSceneFromBundle(assetBundle, LoadSceneMode.Additive);
		DebugTime.Log("Application.LoadLevel(foyer)");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
