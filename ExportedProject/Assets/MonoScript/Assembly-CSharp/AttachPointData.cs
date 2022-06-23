using System;

[Serializable]
public class AttachPointData
{
	public tk2dSpriteDefinition.AttachPoint[] attachPoints;

	public AttachPointData(tk2dSpriteDefinition.AttachPoint[] bcs)
	{
		attachPoints = bcs;
	}
}
