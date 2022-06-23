using System;
using Dungeonator;

public class GunQueueChallengeModifier : ChallengeModifier
{
	public float AutoSwitchTime = 15f;

	private float m_elapsed;

	private float m_gunQueueTimer;

	private void Start()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			GameManager.Instance.AllPlayers[i].inventory.GunLocked.SetOverride("challenge", true);
			PlayerController obj = GameManager.Instance.AllPlayers[i];
			obj.OnReloadPressed = (Action<PlayerController, Gun>)Delegate.Combine(obj.OnReloadPressed, new Action<PlayerController, Gun>(HandleGunReloadPress));
			PlayerController obj2 = GameManager.Instance.AllPlayers[i];
			obj2.OnReloadedGun = (Action<PlayerController, Gun>)Delegate.Combine(obj2.OnReloadedGun, new Action<PlayerController, Gun>(HandleGunReloaded));
		}
		m_gunQueueTimer = AutoSwitchTime;
	}

	private void Update()
	{
		m_elapsed += BraveTime.DeltaTime;
		m_gunQueueTimer -= BraveTime.DeltaTime;
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			Gun currentGun = GameManager.Instance.AllPlayers[i].CurrentGun;
			if ((bool)currentGun && (currentGun.ammo == 0 || (currentGun.UsesRechargeLikeActiveItem && currentGun.RemainingActiveCooldownAmount > 0f)))
			{
				HandleGunReloaded(GameManager.Instance.AllPlayers[i], null);
			}
		}
		if (m_gunQueueTimer <= 0f)
		{
			for (int j = 0; j < GameManager.Instance.AllPlayers.Length; j++)
			{
				HandleGunReloaded(GameManager.Instance.AllPlayers[j], null);
			}
		}
	}

	public override bool IsValid(RoomHandler room)
	{
		if (room.IsGunslingKingChallengeRoom)
		{
			return false;
		}
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			if ((bool)GameManager.Instance.AllPlayers[i] && GameManager.Instance.AllPlayers[i].inventory != null && GameManager.Instance.AllPlayers[i].inventory.AllGuns.Count > 1)
			{
				return true;
			}
		}
		return false;
	}

	private void HandleGunReloadPress(PlayerController player, Gun playerGun)
	{
		if (m_elapsed > 1f)
		{
			QueueLogic(player, playerGun);
		}
	}

	private void HandleGunReloaded(PlayerController player, Gun playerGun)
	{
		QueueLogic(player, playerGun);
	}

	private void QueueLogic(PlayerController player, Gun playerGun)
	{
		if (!this)
		{
			return;
		}
		player.inventory.GunLocked.RemoveOverride("challenge");
		Gun currentGun = player.CurrentGun;
		if ((bool)currentGun && player.inventory.GunLocked.Value)
		{
			MimicGunController component = currentGun.GetComponent<MimicGunController>();
			if ((bool)component)
			{
				component.ForceClearMimic();
			}
		}
		if ((bool)ChallengeManager.Instance && (bool)currentGun && currentGun.ClipShotsRemaining == 0)
		{
			for (int i = 0; i < ChallengeManager.Instance.ActiveChallenges.Count; i++)
			{
				if (ChallengeManager.Instance.ActiveChallenges[i] is GunOverheatChallengeModifier)
				{
					GunOverheatChallengeModifier gunOverheatChallengeModifier = ChallengeManager.Instance.ActiveChallenges[i] as GunOverheatChallengeModifier;
					gunOverheatChallengeModifier.ForceGoop(player);
				}
			}
		}
		if ((bool)currentGun)
		{
			currentGun.ForceImmediateReload(true);
		}
		player.inventory.GunChangeForgiveness = true;
		player.ChangeGun(1);
		player.inventory.GunChangeForgiveness = false;
		player.inventory.GunLocked.SetOverride("challenge", true);
		m_gunQueueTimer = AutoSwitchTime;
		m_elapsed = 0f;
	}

	private void OnDestroy()
	{
		for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
		{
			PlayerController obj = GameManager.Instance.AllPlayers[i];
			obj.OnReloadedGun = (Action<PlayerController, Gun>)Delegate.Remove(obj.OnReloadedGun, new Action<PlayerController, Gun>(HandleGunReloaded));
			PlayerController obj2 = GameManager.Instance.AllPlayers[i];
			obj2.OnReloadPressed = (Action<PlayerController, Gun>)Delegate.Remove(obj2.OnReloadPressed, new Action<PlayerController, Gun>(HandleGunReloadPress));
			GameManager.Instance.AllPlayers[i].inventory.GunLocked.RemoveOverride("challenge");
		}
	}
}
