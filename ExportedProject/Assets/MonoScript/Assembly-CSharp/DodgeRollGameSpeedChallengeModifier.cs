using UnityEngine;

public class DodgeRollGameSpeedChallengeModifier : ChallengeModifier
{
	public float SpeedGain = 2.5f;

	public float SpeedMax = 1.5f;

	[Header("Boss Parameters")]
	public float BossSpeedGain = 1f;

	public float BossSpeedMax = 1.3f;

	private float CurrentSpeedModifier = 1f;

	private void Start()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].OnPreDodgeRoll += OnDodgeRoll;
		}
	}

	private void OnDodgeRoll(PlayerController obj)
	{
		float num = SpeedGain;
		float max = SpeedMax;
		if (GameManager.Instance.PrimaryPlayer.CurrentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS)
		{
			num = BossSpeedGain;
			max = BossSpeedMax;
		}
		CurrentSpeedModifier = Mathf.Clamp(CurrentSpeedModifier + num * 0.01f, 1f, max);
		BraveTime.ClearMultiplier(base.gameObject);
		BraveTime.RegisterTimeScaleMultiplier(CurrentSpeedModifier, base.gameObject);
	}

	private void OnDestroy()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].OnPreDodgeRoll -= OnDodgeRoll;
		}
		BraveTime.ClearMultiplier(base.gameObject);
	}
}
