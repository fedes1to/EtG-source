using System;

[Serializable]
public class ChainRoom
{
	public PrototypeDungeonRoom prototypeRoom;

	public float weight = 0.1f;

	public bool limitedCopies;

	public int maxCopies = 1;
}
