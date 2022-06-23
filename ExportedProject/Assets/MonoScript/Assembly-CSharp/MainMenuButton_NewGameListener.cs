using UnityEngine;

public class MainMenuButton_NewGameListener : MonoBehaviour
{
	private void OnClick(dfControl control, dfMouseEventArgs mouseEvent)
	{
		GameManager.Instance.LoadCharacterSelect();
	}
}
