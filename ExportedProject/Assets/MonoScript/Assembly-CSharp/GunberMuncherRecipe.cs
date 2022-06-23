using System;
using System.Collections.Generic;

[Serializable]
public class GunberMuncherRecipe
{
	public string Annotation;

	[PickupIdentifier]
	public List<int> gunIDs_A;

	[PickupIdentifier]
	public List<int> gunIDs_B;

	[PickupIdentifier]
	public int resultID;

	[LongEnum]
	public GungeonFlags flagToSet;
}
