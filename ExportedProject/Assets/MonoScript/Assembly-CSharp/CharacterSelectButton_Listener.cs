using UnityEngine;

public class CharacterSelectButton_Listener : MonoBehaviour
{
	public GameObject playerToSelect;

	private void OnClick(dfControl control, dfMouseEventArgs mouseEvent)
	{
		GameManager.PlayerPrefabForNewGame = playerToSelect;
		GameManager.Instance.LoadNextLevel();
	}
}
