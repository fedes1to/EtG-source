using System;

public class GunOverheatChallengeModifier : ChallengeModifier
{
	public GoopDefinition Goop;

	public float Radius = 3f;

	private void Start()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController obj = GameManager.Instance.AllPlayers[i];
			obj.OnReloadedGun = (Action<PlayerController, Gun>)Delegate.Combine(obj.OnReloadedGun, new Action<PlayerController, Gun>(HandleGunReloaded));
		}
	}

	private void HandleGunReloaded(PlayerController player, Gun playerGun)
	{
		if (playerGun.ClipShotsRemaining == 0)
		{
			DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(Goop).TimedAddGoopCircle(player.CenterPosition, Radius);
		}
	}

	public void ForceGoop(PlayerController player)
	{
		DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(Goop).TimedAddGoopCircle(player.CenterPosition, Radius);
	}

	private void OnDestroy()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController obj = GameManager.Instance.AllPlayers[i];
			obj.OnReloadedGun = (Action<PlayerController, Gun>)Delegate.Remove(obj.OnReloadedGun, new Action<PlayerController, Gun>(HandleGunReloaded));
		}
	}
}
