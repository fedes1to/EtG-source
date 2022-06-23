using UnityEngine;

public class LockpicksItem : PlayerItem
{
	public float ChanceToUnlock = 0.2f;

	public float MasterOfUnlocking_ChanceToUnlock = 0.8f;

	private bool m_isTransformed;

	public override bool CanBeUsed(PlayerController user)
	{
		if (!user || user.CurrentRoom == null)
		{
			return false;
		}
		if (!m_isTransformed && user.HasActiveBonusSynergy(CustomSynergyType.MASTER_OF_UNLOCKING))
		{
			m_isTransformed = true;
			base.sprite.SetSprite("lockpicks_upgrade_001");
		}
		else if (m_isTransformed && !user.HasActiveBonusSynergy(CustomSynergyType.MASTER_OF_UNLOCKING))
		{
			m_isTransformed = false;
			base.sprite.SetSprite("lockpicks_001");
		}
		IPlayerInteractable nearestInteractable = user.CurrentRoom.GetNearestInteractable(user.CenterPosition, 1f, user);
		if (nearestInteractable is InteractableLock || nearestInteractable is Chest || nearestInteractable is DungeonDoorController)
		{
			if (nearestInteractable is InteractableLock)
			{
				InteractableLock interactableLock = nearestInteractable as InteractableLock;
				if ((bool)interactableLock && !interactableLock.IsBusted && interactableLock.transform.position.GetAbsoluteRoom() == user.CurrentRoom && interactableLock.IsLocked && !interactableLock.HasBeenPicked && interactableLock.lockMode == InteractableLock.InteractableLockMode.NORMAL)
				{
					return base.CanBeUsed(user);
				}
			}
			else if (nearestInteractable is DungeonDoorController)
			{
				DungeonDoorController dungeonDoorController = nearestInteractable as DungeonDoorController;
				if (dungeonDoorController != null && dungeonDoorController.Mode == DungeonDoorController.DungeonDoorMode.COMPLEX && dungeonDoorController.isLocked && !dungeonDoorController.lockIsBusted)
				{
					return base.CanBeUsed(user);
				}
			}
			else if (nearestInteractable is Chest)
			{
				Chest chest = nearestInteractable as Chest;
				if (!chest)
				{
					return false;
				}
				if (chest.GetAbsoluteParentRoom() != user.CurrentRoom)
				{
					return false;
				}
				if (!chest.IsLocked)
				{
					return false;
				}
				if (chest.IsLockBroken)
				{
					return false;
				}
				return base.CanBeUsed(user);
			}
		}
		return false;
	}

	protected override void DoEffect(PlayerController user)
	{
		base.DoEffect(user);
		float num = ChanceToUnlock;
		if ((bool)user && user.HasActiveBonusSynergy(CustomSynergyType.MASTER_OF_UNLOCKING))
		{
			num = MasterOfUnlocking_ChanceToUnlock;
		}
		IPlayerInteractable nearestInteractable = user.CurrentRoom.GetNearestInteractable(user.CenterPosition, 1f, user);
		if (!(nearestInteractable is InteractableLock) && !(nearestInteractable is Chest) && !(nearestInteractable is DungeonDoorController))
		{
			return;
		}
		if (nearestInteractable is InteractableLock)
		{
			InteractableLock interactableLock = nearestInteractable as InteractableLock;
			if (interactableLock.lockMode == InteractableLock.InteractableLockMode.NORMAL)
			{
				interactableLock.HasBeenPicked = true;
				if (Random.value < num)
				{
					AkSoundEngine.PostEvent("Play_OBJ_lock_pick_01", base.gameObject);
					interactableLock.ForceUnlock();
				}
				else
				{
					AkSoundEngine.PostEvent("Play_OBJ_purchase_unable_01", base.gameObject);
					interactableLock.BreakLock();
				}
			}
		}
		else if (nearestInteractable is DungeonDoorController)
		{
			DungeonDoorController dungeonDoorController = nearestInteractable as DungeonDoorController;
			if (dungeonDoorController != null && dungeonDoorController.Mode == DungeonDoorController.DungeonDoorMode.COMPLEX && dungeonDoorController.isLocked)
			{
				if (Random.value < num)
				{
					AkSoundEngine.PostEvent("Play_OBJ_lock_pick_01", base.gameObject);
					dungeonDoorController.Unlock();
				}
				else
				{
					AkSoundEngine.PostEvent("Play_OBJ_purchase_unable_01", base.gameObject);
					dungeonDoorController.BreakLock();
				}
			}
		}
		else
		{
			if (!(nearestInteractable is Chest))
			{
				return;
			}
			Chest chest = nearestInteractable as Chest;
			if (chest.IsLocked && !chest.IsLockBroken)
			{
				if (Random.value < num)
				{
					AkSoundEngine.PostEvent("Play_OBJ_lock_pick_01", base.gameObject);
					chest.ForceUnlock();
				}
				else
				{
					AkSoundEngine.PostEvent("Play_WPN_gun_empty_01", base.gameObject);
					chest.BreakLock();
				}
			}
		}
	}
}
