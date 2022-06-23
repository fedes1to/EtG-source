public class NoBlankChecker : BraveBehaviour
{
	public void Update()
	{
		if (GameManager.Instance.BestActivePlayer != null && GameManager.Instance.BestActivePlayer.Blanks == 0)
		{
			GameManager.BroadcastRoomTalkDoerFsmEvent("hasNoBlanks");
		}
	}
}
