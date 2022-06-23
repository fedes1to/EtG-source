using System.Collections.Generic;

public class MidGamePassiveItemData
{
	public int PickupID = -1;

	public List<object> SerializedData;

	public MidGamePassiveItemData(PassiveItem p)
	{
		PickupID = p.PickupObjectId;
		SerializedData = new List<object>();
		p.MidGameSerialize(SerializedData);
	}
}
