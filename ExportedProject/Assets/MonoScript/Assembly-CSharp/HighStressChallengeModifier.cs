public class HighStressChallengeModifier : ChallengeModifier
{
	public float StressDuration = 5f;

	private void Start()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].OnReceivedDamage += OnPlayerReceivedDamage;
		}
	}

	private void OnPlayerReceivedDamage(PlayerController p)
	{
		if ((bool)p && (bool)p.healthHaver)
		{
			p.TriggerHighStress(StressDuration);
		}
	}

	private void OnDestroy()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].OnReceivedDamage -= OnPlayerReceivedDamage;
		}
	}
}
