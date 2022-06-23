using UnityEngine;

public class UndieListener : MonoBehaviour
{
	private void OnClick(dfControl control, dfMouseEventArgs mouseEvent)
	{
		dfGUIManager.PopModal();
		Pixelator.Instance.LerpToLetterbox(0.5f, 0f);
		GameManager.Instance.PrimaryPlayer.healthHaver.FullHeal();
		base.transform.parent.gameObject.SetActive(false);
		GameManager.Instance.Unpause();
		GameManager.Instance.PrimaryPlayer.ClearDeadFlags();
	}
}
