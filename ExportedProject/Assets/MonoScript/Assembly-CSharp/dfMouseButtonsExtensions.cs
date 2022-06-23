public static class dfMouseButtonsExtensions
{
	public static bool IsSet(this dfMouseButtons value, dfMouseButtons flag)
	{
		return flag == (value & flag);
	}
}
