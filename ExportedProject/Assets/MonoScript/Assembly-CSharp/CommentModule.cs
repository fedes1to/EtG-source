using System;

[Serializable]
public struct CommentModule
{
	public enum CommentTarget
	{
		PRIMARY,
		SECONDARY,
		DOG
	}

	public string stringKey;

	public float duration;

	public CommentTarget target;

	public float delay;
}
