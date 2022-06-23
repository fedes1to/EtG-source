using System;
using Dungeonator;
using UnityEngine;
using UnityEngine.Serialization;

public class KeyBulletManController : BraveBehaviour
{
	[PickupIdentifier]
	[FormerlySerializedAs("keyId")]
	public int lookPickupId = -1;

	public GenericLootTable lootTable;

	public Vector2 offset;

	public bool doubleForBlackPhantom = true;

	public bool RemoveShaderOnDeath;

	private bool m_cachedIsBlackPhantom;

	public void Start()
	{
		base.healthHaver.OnPreDeath += OnPreDeath;
		AIActor aIActor = base.aiActor;
		aIActor.OnHandleRewards = (Action)Delegate.Combine(aIActor.OnHandleRewards, new Action(OnHandleRewards));
		base.aiActor.SuppressBlackPhantomCorpseBurn = true;
	}

	protected override void OnDestroy()
	{
		if ((bool)base.healthHaver)
		{
			base.healthHaver.OnPreDeath -= OnPreDeath;
		}
		if ((bool)base.aiActor)
		{
			AIActor aIActor = base.aiActor;
			aIActor.OnHandleRewards = (Action)Delegate.Remove(aIActor.OnHandleRewards, new Action(OnHandleRewards));
		}
		base.OnDestroy();
	}

	private void OnPreDeath(Vector2 dir)
	{
		m_cachedIsBlackPhantom = base.aiActor.IsBlackPhantom;
		if (lookPickupId == GlobalItemIds.Key && base.aiActor.IsBlackPhantom)
		{
			base.aiActor.UnbecomeBlackPhantom();
		}
		if (RemoveShaderOnDeath)
		{
			base.renderer.sharedMaterials = new Material[1] { base.renderer.sharedMaterials[0] };
		}
	}

	public void ForceHandleRewards()
	{
		OnHandleRewards();
	}

	private void OnHandleRewards()
	{
		bool flag = false;
		if (lookPickupId == GlobalItemIds.Key)
		{
			GameStatsManager.Instance.RegisterStatChange(TrackedStats.KEYBULLETMEN_KILLED, 1f);
			flag = true;
		}
		Vector3 vector = base.transform.position + (Vector3)offset;
		if (!flag && GameManager.Instance.Dungeon.data.isAnyFaceWall(Mathf.FloorToInt(vector.x), Mathf.FloorToInt(vector.y + 0.5f)))
		{
			vector += new Vector3(0f, -1f, 0f);
		}
		CellData cell = (vector + new Vector3(0f, 0.5f, 0f)).GetCell();
		if (cell == null || cell.type == CellType.WALL || cell.IsAnyFaceWall())
		{
			cell = (vector += Vector3.down).GetCell();
			if (cell != null && cell.type != CellType.WALL)
			{
				vector += Vector3.down;
			}
		}
		if (doubleForBlackPhantom && m_cachedIsBlackPhantom)
		{
			LootEngine.SpawnItem(GetReward(), vector, Vector2.left, 2f, false, false, true);
			LootEngine.SpawnItem(GetReward(), vector, Vector2.right, 2f, false, false, true);
		}
		else if (flag)
		{
			LootEngine.SpawnItem(GetReward(), vector, Vector2.zero, 0f, true, false, true);
		}
		else
		{
			LootEngine.SpawnItem(GetReward(), vector, Vector2.up, 0.1f, true, false, true);
		}
	}

	private GameObject GetReward()
	{
		if ((bool)lootTable)
		{
			return lootTable.SelectByWeight();
		}
		return PickupObjectDatabase.GetById(lookPickupId).gameObject;
	}
}
