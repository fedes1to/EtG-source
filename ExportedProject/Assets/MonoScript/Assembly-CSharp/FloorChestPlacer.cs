using Dungeonator;
using UnityEngine;

public class FloorChestPlacer : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	public bool OverrideItemQuality;

	[ShowInInspectorIf("OverrideItemQuality", false)]
	public PickupObject.ItemQuality ItemQuality;

	public float OverrideMimicChance = -1f;

	[DwarfConfigurable]
	public int xPixelOffset;

	[DwarfConfigurable]
	public int yPixelOffset;

	public bool CenterChestInRegion;

	public bool OverrideLockChance;

	public bool ForceUnlockedIfWooden;

	public float LockChance = 0.5f;

	public bool UseOverrideChest;

	public DungeonPrerequisite OverrideChestPrereq;

	public Chest OverrideChestPrefab;

	public void ConfigureOnPlacement(RoomHandler room)
	{
		Chest chest = null;
		IntVector2 positionInRoom = base.transform.position.IntXY() - room.area.basePosition;
		chest = ((!UseOverrideChest || !OverrideChestPrereq.CheckConditionsFulfilled()) ? GameManager.Instance.RewardManager.GenerationSpawnRewardChestAt(positionInRoom, room, (!OverrideItemQuality) ? null : new PickupObject.ItemQuality?(ItemQuality), OverrideMimicChance) : Chest.Spawn(OverrideChestPrefab, base.transform.position.IntXY()));
		if (CenterChestInRegion && (bool)chest)
		{
			SpeculativeRigidbody component = chest.GetComponent<SpeculativeRigidbody>();
			if ((bool)component)
			{
				Vector2 vector = component.UnitCenter - chest.transform.position.XY();
				Vector2 vector2 = base.transform.position.XY() + new Vector2((float)xPixelOffset / 16f, (float)yPixelOffset / 16f) + new Vector2((float)placeableWidth / 2f, (float)placeableHeight / 2f);
				Vector2 vector3 = vector2 - vector;
				chest.transform.position = vector3.ToVector3ZisY().Quantize(0.0625f);
				component.Reinitialize();
			}
		}
		if (OverrideLockChance && (bool)chest)
		{
			if (Random.value < LockChance || (ForceUnlockedIfWooden && chest.lootTable.D_Chance == 1f))
			{
				chest.ForceUnlock();
			}
			else
			{
				chest.IsLocked = true;
			}
		}
		Object.Destroy(base.gameObject);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
