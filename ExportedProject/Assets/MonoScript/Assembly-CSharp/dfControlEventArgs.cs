public class dfControlEventArgs
{
	public dfControl Source { get; internal set; }

	public bool Used { get; private set; }

	internal dfControlEventArgs(dfControl Target)
	{
		Source = Target;
	}

	public void Use()
	{
		Used = true;
	}
}
