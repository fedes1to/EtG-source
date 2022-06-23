using System;

[Serializable]
public class GameUIAmmoType
{
	public enum AmmoType
	{
		SMALL_BULLET,
		MEDIUM_BULLET,
		BEAM,
		GRENADE,
		SHOTGUN,
		SMALL_BLASTER,
		MEDIUM_BLASTER,
		NAIL,
		MUSKETBALL,
		ARROW,
		MAGIC,
		BLUE_SHOTGUN,
		SKULL,
		FISH,
		CUSTOM
	}

	public AmmoType ammoType;

	public string customAmmoType = string.Empty;

	public dfTiledSprite ammoBarFG;

	public dfTiledSprite ammoBarBG;
}
