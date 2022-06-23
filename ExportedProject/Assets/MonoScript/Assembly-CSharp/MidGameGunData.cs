using System.Collections.Generic;

public class MidGameGunData
{
	public int PickupID = -1;

	public int CurrentAmmo = -1;

	public List<object> SerializedData;

	public List<int> DuctTapedGunIDs;

	public MidGameGunData(Gun g)
	{
		PickupID = g.PickupObjectId;
		CurrentAmmo = g.CurrentAmmo;
		SerializedData = new List<object>();
		g.MidGameSerialize(SerializedData);
		DuctTapedGunIDs = new List<int>();
		if (g.DuctTapeMergedGunIDs != null)
		{
			DuctTapedGunIDs.AddRange(g.DuctTapeMergedGunIDs);
		}
	}
}
