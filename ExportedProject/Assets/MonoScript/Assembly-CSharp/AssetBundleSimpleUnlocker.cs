using UnityEngine;

public class AssetBundleSimpleUnlocker : MonoBehaviour
{
	public GungeonFlags[] FlagsToSetUponLoad;

	public void OnGameStartup()
	{
		for (int i = 0; i < FlagsToSetUponLoad.Length; i++)
		{
			GameStatsManager.Instance.SetFlag(FlagsToSetUponLoad[i], true);
		}
	}
}
