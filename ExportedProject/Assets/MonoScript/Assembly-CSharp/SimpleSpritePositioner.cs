using UnityEngine;

public class SimpleSpritePositioner : DungeonPlaceableBehaviour
{
	public float Rotation;

	public void Start()
	{
		base.transform.localRotation = Quaternion.Euler(0f, 0f, Rotation);
		if ((bool)base.sprite)
		{
			base.sprite.UpdateZDepth();
			base.sprite.ForceRotationRebuild();
		}
	}
}
