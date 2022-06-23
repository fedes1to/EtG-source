public class dfMarkupTokenAttribute : IPoolable
{
	public dfMarkupToken Key;

	public dfMarkupToken Value;

	private static dfList<dfMarkupTokenAttribute> pool = new dfList<dfMarkupTokenAttribute>();

	private dfMarkupTokenAttribute()
	{
	}

	public static dfMarkupTokenAttribute Obtain(dfMarkupToken key, dfMarkupToken value)
	{
		dfMarkupTokenAttribute dfMarkupTokenAttribute2 = ((pool.Count <= 0) ? new dfMarkupTokenAttribute() : pool.Pop());
		dfMarkupTokenAttribute2.Key = key;
		dfMarkupTokenAttribute2.Value = value;
		return dfMarkupTokenAttribute2;
	}

	public void Release()
	{
		if (Key != null)
		{
			Key.Release();
			Key = null;
		}
		if (Value != null)
		{
			Value.Release();
			Value = null;
		}
		if (!pool.Contains(this))
		{
			pool.Add(this);
		}
	}
}
