using System.Collections.Generic;

public class TrackedMaximumsComparer : IEqualityComparer<TrackedMaximums>
{
	public bool Equals(TrackedMaximums x, TrackedMaximums y)
	{
		return x == y;
	}

	public int GetHashCode(TrackedMaximums obj)
	{
		return (int)obj;
	}
}
