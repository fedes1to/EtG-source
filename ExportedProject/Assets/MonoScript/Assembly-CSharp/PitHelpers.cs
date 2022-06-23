using System;

[Serializable]
public class PitHelpers
{
	[HelpBox("Extend pit colliders by x/y pixels")]
	public IntVector2 PreJump;

	public IntVector2 Landing;
}
