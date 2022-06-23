using System;

[Serializable]
public class BagelColliderData
{
	public BagelCollider[] bagelColliders;

	public BagelColliderData(BagelCollider[] bcs)
	{
		bagelColliders = bcs;
	}
}
