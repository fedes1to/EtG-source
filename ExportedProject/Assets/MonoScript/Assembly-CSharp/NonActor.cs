using UnityEngine;

public class NonActor : GameActor
{
	public override Gun CurrentGun
	{
		get
		{
			return null;
		}
	}

	public override Transform GunPivot
	{
		get
		{
			return null;
		}
	}

	public override Vector3 SpriteDimensions
	{
		get
		{
			return Vector3.zero;
		}
	}

	public override bool SpriteFlipped
	{
		get
		{
			return false;
		}
	}

	public override void Awake()
	{
	}

	public override void Update()
	{
	}
}
