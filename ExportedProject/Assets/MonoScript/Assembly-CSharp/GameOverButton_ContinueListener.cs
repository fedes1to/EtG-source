using UnityEngine;

public class GameOverButton_ContinueListener : MonoBehaviour
{
	private void OnClick(dfControl control, dfMouseEventArgs mouseEvent)
	{
		Object.Destroy(GameManager.Instance);
		GameManager.Instance.LoadMainMenu();
	}
}
