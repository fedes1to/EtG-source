using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
[AddComponentMenu("Daikon Forge/Examples/General/Load Level On Click")]
public class LoadLevelByName : MonoBehaviour
{
	public string LevelName;

	private void OnClick()
	{
		if (!string.IsNullOrEmpty(LevelName))
		{
			SceneManager.LoadScene(LevelName);
		}
	}
}
