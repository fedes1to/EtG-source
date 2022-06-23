using System.Collections.Generic;

public class TrackedStatsComparer : IEqualityComparer<TrackedStats>
{
	public bool Equals(TrackedStats x, TrackedStats y)
	{
		return x == y;
	}

	public int GetHashCode(TrackedStats obj)
	{
		return (int)obj;
	}
}
