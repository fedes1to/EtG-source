public interface IDFVirtualScrollingTile
{
	int VirtualScrollItemIndex { get; set; }

	void OnScrollPanelItemVirtualize(object backingListItem);

	dfPanel GetDfPanel();
}
