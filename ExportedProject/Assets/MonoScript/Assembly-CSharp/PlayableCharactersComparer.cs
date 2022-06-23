using System.Collections.Generic;

public class PlayableCharactersComparer : IEqualityComparer<PlayableCharacters>
{
	public bool Equals(PlayableCharacters x, PlayableCharacters y)
	{
		return x == y;
	}

	public int GetHashCode(PlayableCharacters obj)
	{
		return (int)obj;
	}
}
