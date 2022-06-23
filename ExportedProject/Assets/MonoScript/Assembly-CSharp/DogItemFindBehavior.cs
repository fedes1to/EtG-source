using System.Collections;
using UnityEngine;

public class DogItemFindBehavior : BehaviorBase
{
	public GenericLootTable ItemFindLootTable;

	public float ChanceToFindItemOnRoomClear = 0.05f;

	public string ItemFindAnimName;

	private float m_findTimer;

	public override void Start()
	{
		base.Start();
		if (m_aiActor.CompanionOwner != null)
		{
			m_aiActor.CompanionOwner.OnRoomClearEvent += HandleRoomCleared;
		}
	}

	public override void Destroy()
	{
		if (m_aiActor.CompanionOwner != null)
		{
			m_aiActor.CompanionOwner.OnRoomClearEvent -= HandleRoomCleared;
		}
		base.Destroy();
	}

	private IEnumerator DelayedSpawnItem(Vector2 spawnPoint)
	{
		yield return new WaitForSeconds(0.5f);
		LootEngine.SpawnItem(ItemFindLootTable.SelectByWeight(), spawnPoint, Vector2.up, 1f);
	}

	private void HandleRoomCleared(PlayerController obj)
	{
		if (Random.value < ChanceToFindItemOnRoomClear)
		{
			m_findTimer = 4.5f;
			if (!string.IsNullOrEmpty(ItemFindAnimName))
			{
				m_aiAnimator.PlayUntilFinished(ItemFindAnimName);
			}
			GameManager.Instance.Dungeon.StartCoroutine(DelayedSpawnItem(m_aiActor.CenterPosition));
		}
	}

	public override void Upkeep()
	{
		base.Upkeep();
	}

	public override BehaviorResult Update()
	{
		if (m_findTimer > 0f)
		{
			DecrementTimer(ref m_findTimer);
			m_aiActor.ClearPath();
		}
		return base.Update();
	}
}
