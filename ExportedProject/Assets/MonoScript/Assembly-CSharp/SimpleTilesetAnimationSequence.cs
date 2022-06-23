using System;
using System.Collections.Generic;

[Serializable]
public class SimpleTilesetAnimationSequence
{
	public enum TilesetSequencePlayStyle
	{
		SIMPLE_LOOP,
		DELAYED_LOOP,
		RANDOM_FRAMES,
		TRIGGERED_ONCE,
		LOOPCEPTION
	}

	public TilesetSequencePlayStyle playstyle;

	public float loopDelayMin = 5f;

	public float loopDelayMax = 10f;

	public int loopceptionTarget = -1;

	public int loopceptionMin = 1;

	public int loopceptionMax = 3;

	public int coreceptionMin = 1;

	public int coreceptionMax = 1;

	public bool randomStartFrame;

	public List<SimpleTilesetAnimationSequenceEntry> entries = new List<SimpleTilesetAnimationSequenceEntry>();
}
