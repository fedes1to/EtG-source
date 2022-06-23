using UnityEngine;

public static class PastCameraUtility
{
	public static void UnlockConversation()
	{
		GameManager.Instance.PrimaryPlayer.ClearInputOverride("past");
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			GameManager.Instance.SecondaryPlayer.ClearInputOverride("past");
		}
		Pixelator.Instance.LerpToLetterbox(0.5f, 0.25f);
		Pixelator.Instance.DoFinalNonFadedLayer = false;
		GameUIRoot.Instance.ToggleLowerPanels(true, false, string.Empty);
		GameUIRoot.Instance.ShowCoreUI(string.Empty);
		GameManager.Instance.MainCameraController.SetManualControl(false);
	}

	public static void LockConversation(Vector2 lockPos)
	{
		GameManager.Instance.PrimaryPlayer.SetInputOverride("past");
		if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
		{
			GameManager.Instance.SecondaryPlayer.SetInputOverride("past");
		}
		Pixelator.Instance.LerpToLetterbox(0.35f, 0.25f);
		Pixelator.Instance.DoFinalNonFadedLayer = true;
		GameUIRoot.Instance.ToggleLowerPanels(false, false, string.Empty);
		GameUIRoot.Instance.HideCoreUI(string.Empty);
		CameraController mainCameraController = GameManager.Instance.MainCameraController;
		mainCameraController.SetManualControl(true);
		mainCameraController.OverridePosition = lockPos.ToVector3ZUp();
	}
}
