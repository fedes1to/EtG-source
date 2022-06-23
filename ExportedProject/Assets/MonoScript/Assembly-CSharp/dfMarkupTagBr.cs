[dfMarkupTagInfo("br")]
public class dfMarkupTagBr : dfMarkupTag
{
	public dfMarkupTagBr()
		: base("br")
	{
		IsClosedTag = true;
	}

	public dfMarkupTagBr(dfMarkupTag original)
		: base(original)
	{
		IsClosedTag = true;
	}

	protected override void _PerformLayoutImpl(dfMarkupBox container, dfMarkupStyle style)
	{
		container.AddLineBreak();
	}
}
