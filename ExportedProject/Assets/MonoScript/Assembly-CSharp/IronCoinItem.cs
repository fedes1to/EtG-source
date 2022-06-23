using System.Collections;
using System.Collections.Generic;
using Dungeonator;
using UnityEngine;

public class IronCoinItem : PlayerItem
{
	public float ChanceToTargetBoss = 0.01f;

	public GameObject OnUsedVFX;

	public GameObject NotePrefab;

	public GoopDefinition BloodDefinition;

	protected override void DoEffect(PlayerController user)
	{
		if ((bool)OnUsedVFX)
		{
			user.PlayEffectOnActor(OnUsedVFX, new Vector3(0f, 0.25f, 0f), false);
		}
		List<RoomHandler> list = new List<RoomHandler>();
		bool flag = Random.value < ChanceToTargetBoss;
		flag = false;
		for (int i = 0; i < GameManager.Instance.Dungeon.data.rooms.Count; i++)
		{
			if (GameManager.Instance.Dungeon.data.rooms[i].visibility != 0 || !GameManager.Instance.Dungeon.data.rooms[i].HasActiveEnemies(RoomHandler.ActiveEnemyType.All))
			{
				continue;
			}
			if (flag)
			{
				if (GameManager.Instance.Dungeon.data.rooms[i].area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS)
				{
					list.Add(GameManager.Instance.Dungeon.data.rooms[i]);
				}
			}
			else if (GameManager.Instance.Dungeon.data.rooms[i].area.PrototypeRoomCategory != PrototypeDungeonRoom.RoomCategory.BOSS)
			{
				list.Add(GameManager.Instance.Dungeon.data.rooms[i]);
			}
		}
		if (list.Count > 0)
		{
			RoomHandler targetRoom = list[Random.Range(0, list.Count)];
			user.StartCoroutine(SlaughterRoom(targetRoom));
		}
	}

	private IEnumerator SlaughterRoom(RoomHandler targetRoom)
	{
		if (targetRoom == null)
		{
			yield break;
		}
		targetRoom.ClearReinforcementLayers();
		List<AIActor> enemies = targetRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (enemies != null)
		{
			List<AIActor> enemiesToKill = new List<AIActor>(enemies);
			for (int i = 0; i < enemiesToKill.Count; i++)
			{
				AIActor aIActor = enemiesToKill[i];
				if ((bool)aIActor)
				{
					aIActor.enabled = true;
				}
			}
			yield return null;
			for (int j = 0; j < enemiesToKill.Count; j++)
			{
				AIActor aIActor2 = enemiesToKill[j];
				if ((bool)aIActor2)
				{
					Object.Destroy(aIActor2.gameObject);
				}
			}
		}
		if (!(NotePrefab != null))
		{
			yield break;
		}
		bool success = false;
		IntVector2 centeredVisibleClearSpot = targetRoom.GetCenteredVisibleClearSpot(3, 3, out success, true);
		centeredVisibleClearSpot = centeredVisibleClearSpot - targetRoom.area.basePosition + IntVector2.One;
		if (!success)
		{
			yield break;
		}
		GameObject gameObject = DungeonPlaceableUtility.InstantiateDungeonPlaceable(NotePrefab.gameObject, targetRoom, centeredVisibleClearSpot, false);
		if ((bool)gameObject)
		{
			IPlayerInteractable[] interfacesInChildren = gameObject.GetInterfacesInChildren<IPlayerInteractable>();
			for (int k = 0; k < interfacesInChildren.Length; k++)
			{
				targetRoom.RegisterInteractable(interfacesInChildren[k]);
			}
		}
		if (BloodDefinition != null)
		{
			DeadlyDeadlyGoopManager goopManagerForGoopType = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(BloodDefinition);
			goopManagerForGoopType.AddGoopCircle(centeredVisibleClearSpot.ToVector2(0.75f, 0.5f) + targetRoom.area.basePosition.ToVector2(), 2f, -1, true);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
