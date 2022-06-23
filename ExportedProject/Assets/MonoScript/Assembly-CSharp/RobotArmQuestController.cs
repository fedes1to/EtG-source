using System.Collections.Generic;
using DaikonForge.Tween;
using Dungeonator;
using UnityEngine;

public static class RobotArmQuestController
{
	public static void HandlePuzzleSetup()
	{
		if (GameStatsManager.Instance.GetPlayerStatValue(TrackedStats.TIMES_CLEARED_FORGE) == 0f)
		{
			return;
		}
		PickupObject byId = PickupObjectDatabase.GetById(GlobalItemIds.RobotArm);
		PickupObject byId2 = PickupObjectDatabase.GetById(GlobalItemIds.RobotBalloons);
		List<PickupObject> list = new List<PickupObject>();
		if (!GameStatsManager.Instance.GetFlag(GungeonFlags.META_SHOP_DELIVERED_ROBOT_ARM))
		{
			if (GameStatsManager.Instance.CurrentRobotArmFloor < 0 || GameStatsManager.Instance.CurrentRobotArmFloor > 5)
			{
				GameStatsManager.Instance.CurrentRobotArmFloor = 5;
			}
			if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER && GameStatsManager.Instance.CurrentRobotArmFloor == 0)
			{
				RoomHandler entrance = GameManager.Instance.Dungeon.data.Entrance;
				if (entrance != null)
				{
					DungeonPlaceableUtility.InstantiateDungeonPlaceable(location: new IntVector2(29, 62) - entrance.area.basePosition, objectToInstantiate: BraveResources.Load("Global Prefabs/Global Items/RobotArmPlaceable") as GameObject, targetRoom: entrance, deferConfiguration: false);
				}
			}
			else if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GameStatsManager.Instance.GetCurrentRobotArmTileset())
			{
				if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GlobalDungeonData.ValidTilesets.FORGEGEON)
				{
					BaseShopController[] array = Object.FindObjectsOfType<BaseShopController>();
					RoomHandler roomHandler = null;
					Transform transform = null;
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i].name.Contains("Blacksmith"))
						{
							roomHandler = array[i].GetAbsoluteParentRoom();
							transform = array[i].transform.Find("ArmPoint");
							break;
						}
					}
					if (roomHandler != null)
					{
						bool success = false;
						IntVector2 zero = IntVector2.Zero;
						if (transform != null)
						{
							success = true;
							zero = transform.position.IntXY();
						}
						else
						{
							zero = roomHandler.GetCenteredVisibleClearSpot(2, 2, out success, true);
						}
						if (success)
						{
							DungeonPlaceableUtility.InstantiateDungeonPlaceable(BraveResources.Load("Global Prefabs/Global Items/RobotArmPlaceable") as GameObject, roomHandler, zero - roomHandler.area.basePosition, false);
							if (GameStatsManager.Instance.GetFlag(GungeonFlags.META_SHOP_EVER_SEEN_ROBOT_ARM))
							{
								list.Add(byId2.GetComponent<PickupObject>());
							}
						}
					}
				}
				else
				{
					list.Add(byId);
					list.Add(byId2);
				}
			}
		}
		if (list.Count > 0)
		{
			GameManager.Instance.Dungeon.data.DistributeComplexSecretPuzzleItems(list, null, true);
		}
	}

	public static void CombineBalloonsWithArm(PickupObject balloonsObject, PickupObject armObject, PlayerController relevantPlayer)
	{
		relevantPlayer.UsePuzzleItem(balloonsObject);
		relevantPlayer.UsePuzzleItem(armObject);
		if ((bool)balloonsObject)
		{
			Object.Destroy(balloonsObject.gameObject);
		}
		if ((bool)armObject)
		{
			Object.Destroy(armObject.gameObject);
		}
		BalloonAttachmentDoer balloonAttachmentDoer = Object.FindObjectOfType<BalloonAttachmentDoer>();
		if ((bool)balloonAttachmentDoer)
		{
			Object.Destroy(balloonAttachmentDoer.gameObject);
		}
		GameObject gameObject = (GameObject)Object.Instantiate(BraveResources.Load("Global VFX/VFX_BalloonArmLift"));
		gameObject.transform.position = relevantPlayer.SpriteBottomCenter;
		Tween<Vector3> tween = gameObject.transform.TweenMoveTo(gameObject.transform.position + new Vector3(0f, 20f, 0f));
		AnimationCurve sourceCurve = gameObject.GetComponent<SimpleAnimationCurveHolder>().curve;
		tween.Easing = (float a) => sourceCurve.Evaluate(a);
		tween.Duration = 4.5f;
		tween.Play();
		GameStatsManager.Instance.CurrentRobotArmFloor = GameStatsManager.Instance.CurrentRobotArmFloor - 1;
		GameUIRoot.Instance.notificationController.DoCustomNotification(StringTableManager.GetString("#METASHOP_ARM_UP_ONE_LEVEL_HEADER"), StringTableManager.GetString("#METASHOP_ARM_UP_ONE_LEVEL_BODY"), gameObject.GetComponent<tk2dBaseSprite>().Collection, gameObject.GetComponent<tk2dBaseSprite>().spriteId, UINotificationController.NotificationColor.GOLD);
	}
}
