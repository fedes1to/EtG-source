namespace FullInspector.LayoutToolkit
{
	public static class fiLayoutUtility
	{
		public static fiLayout Margin(float margin, fiLayout layout)
		{
			fiHorizontalLayout fiHorizontalLayout2 = new fiHorizontalLayout();
			fiHorizontalLayout2.Add(margin);
			fiHorizontalLayout2.Add(new fiVerticalLayout { margin, layout, margin });
			fiHorizontalLayout2.Add(margin);
			return fiHorizontalLayout2;
		}
	}
}
