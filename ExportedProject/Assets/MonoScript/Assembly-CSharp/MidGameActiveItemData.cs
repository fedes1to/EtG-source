using System.Collections.Generic;

public class MidGameActiveItemData
{
	public int PickupID = -1;

	public bool IsOnCooldown;

	public float DamageCooldown;

	public int RoomCooldown;

	public float TimeCooldown;

	public int NumberOfUses;

	public List<object> SerializedData;

	public MidGameActiveItemData(PlayerItem p)
	{
		PickupID = p.PickupObjectId;
		IsOnCooldown = p.IsOnCooldown;
		DamageCooldown = p.CurrentDamageCooldown;
		RoomCooldown = p.CurrentRoomCooldown;
		TimeCooldown = p.CurrentTimeCooldown;
		NumberOfUses = p.numberOfUses;
		SerializedData = new List<object>();
		p.MidGameSerialize(SerializedData);
	}
}
