public static class AnchorStyleExtensions
{
	public static bool IsFlagSet(this dfAnchorStyle value, dfAnchorStyle flag)
	{
		return flag == (value & flag);
	}

	public static bool IsAnyFlagSet(this dfAnchorStyle value, dfAnchorStyle flag)
	{
		return 0 != (value & flag);
	}

	public static dfAnchorStyle SetFlag(this dfAnchorStyle value, dfAnchorStyle flag)
	{
		return value | flag;
	}

	public static dfAnchorStyle SetFlag(this dfAnchorStyle value, dfAnchorStyle flag, bool on)
	{
		if (on)
		{
			return value | flag;
		}
		return value & ~flag;
	}
}
