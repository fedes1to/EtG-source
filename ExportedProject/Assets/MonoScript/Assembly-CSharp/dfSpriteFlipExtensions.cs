public static class dfSpriteFlipExtensions
{
	public static bool IsSet(this dfSpriteFlip value, dfSpriteFlip flag)
	{
		return flag == (value & flag);
	}

	public static dfSpriteFlip SetFlag(this dfSpriteFlip value, dfSpriteFlip flag, bool on)
	{
		if (on)
		{
			return value | flag;
		}
		return value & ~flag;
	}
}
