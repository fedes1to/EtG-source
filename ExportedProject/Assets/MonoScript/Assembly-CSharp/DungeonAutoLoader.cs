using UnityEngine;

public class DungeonAutoLoader : MonoBehaviour
{
	public void Awake()
	{
		if ((bool)GameManager.Instance.DungeonToAutoLoad)
		{
			Object.Instantiate(GameManager.Instance.DungeonToAutoLoad);
			GameManager.Instance.DungeonToAutoLoad = null;
		}
		Object.Destroy(base.gameObject);
	}
}
