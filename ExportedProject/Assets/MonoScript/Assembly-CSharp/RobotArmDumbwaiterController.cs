using System.Collections.Generic;
using UnityEngine;

public class RobotArmDumbwaiterController : BraveBehaviour, IPlayerInteractable
{
	public GameObject RobotArmObject;

	public static void HandlePuzzleSetup(GameObject RobotArmPrefab)
	{
		if (GameStatsManager.Instance.GetFlag(GungeonFlags.META_SHOP_DELIVERED_ROBOT_ARM))
		{
			return;
		}
		if (GameStatsManager.Instance.CurrentRobotArmFloor <= 0 || GameStatsManager.Instance.CurrentRobotArmFloor > 5)
		{
			GameStatsManager.Instance.CurrentRobotArmFloor = 5;
		}
		if (GameManager.Instance.Dungeon.tileIndices.tilesetId == GameStatsManager.Instance.GetCurrentRobotArmTileset() && GameManager.Instance.Dungeon.tileIndices.tilesetId != GlobalDungeonData.ValidTilesets.FORGEGEON)
		{
			List<PickupObject> list = new List<PickupObject>();
			list.Add(RobotArmPrefab.GetComponent<PickupObject>());
			if (list.Count > 0)
			{
				GameManager.Instance.Dungeon.data.DistributeComplexSecretPuzzleItems(list, null, true);
			}
		}
	}

	private void Start()
	{
		HandlePuzzleSetup(RobotArmObject);
	}

	public void OnEnteredRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white, 0.1f);
			base.sprite.UpdateZDepth();
		}
	}

	public void OnExitRange(PlayerController interactor)
	{
		if ((bool)this)
		{
			SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite);
			base.sprite.UpdateZDepth();
		}
	}

	public float GetDistanceToPoint(Vector2 point)
	{
		Bounds bounds = base.sprite.GetBounds();
		bounds.SetMinMax(bounds.min + base.transform.position, bounds.max + base.transform.position);
		float num = Mathf.Max(Mathf.Min(point.x, bounds.max.x), bounds.min.x);
		float num2 = Mathf.Max(Mathf.Min(point.y, bounds.max.y), bounds.min.y);
		return Mathf.Sqrt((point.x - num) * (point.x - num) + (point.y - num2) * (point.y - num2));
	}

	public float GetOverrideMaxDistance()
	{
		return -1f;
	}

	public void Interact(PlayerController player)
	{
		bool flag = false;
		for (int i = 0; i < player.additionalItems.Count; i++)
		{
			if (player.additionalItems[i] is RobotArmItem)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			return;
		}
		OnExitRange(player);
		for (int j = 0; j < player.additionalItems.Count; j++)
		{
			if (player.additionalItems[j] is RobotArmItem)
			{
				player.UsePuzzleItem(player.additionalItems[j]);
				break;
			}
		}
		GameStatsManager.Instance.CurrentRobotArmFloor = GameStatsManager.Instance.CurrentRobotArmFloor - 1;
		if (GameStatsManager.Instance.CurrentRobotArmFloor == 0)
		{
			GameStatsManager.Instance.SetFlag(GungeonFlags.META_SHOP_DELIVERED_ROBOT_ARM, true);
		}
	}

	public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
	{
		shouldBeFlipped = false;
		return string.Empty;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
