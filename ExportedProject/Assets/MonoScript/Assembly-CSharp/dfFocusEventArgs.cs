public class dfFocusEventArgs : dfControlEventArgs
{
	public bool AllowScrolling;

	public dfControl GotFocus
	{
		get
		{
			return base.Source;
		}
	}

	public dfControl LostFocus { get; private set; }

	internal dfFocusEventArgs(dfControl GotFocus, dfControl LostFocus, bool AllowScrolling)
		: base(GotFocus)
	{
		this.LostFocus = LostFocus;
		this.AllowScrolling = AllowScrolling;
	}
}
