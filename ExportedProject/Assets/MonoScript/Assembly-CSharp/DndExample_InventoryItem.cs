using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Drag and Drop/Inventory Item")]
public class DndExample_InventoryItem : MonoBehaviour
{
	public string ItemName;

	public int Count;

	public string Icon;

	private static dfPanel _container;

	private static dfSprite _sprite;

	private static dfLabel _label;

	public void OnEnable()
	{
		Refresh();
	}

	public void OnDoubleClick(dfControl source, dfMouseEventArgs args)
	{
		OnClick(source, args);
	}

	public void OnClick(dfControl source, dfMouseEventArgs args)
	{
		if (!string.IsNullOrEmpty(ItemName))
		{
			if (args.Buttons == dfMouseButtons.Left)
			{
				Count++;
			}
			else if (args.Buttons == dfMouseButtons.Right)
			{
				Count = Mathf.Max(Count - 1, 1);
			}
			Refresh();
		}
	}

	public void OnDragStart(dfControl source, dfDragEventArgs args)
	{
		if (Count > 0)
		{
			args.Data = this;
			args.State = dfDragDropState.Dragging;
			args.Use();
			DnDExample_DragCursor.Show(this, args.Position);
		}
	}

	public void OnDragEnd(dfControl source, dfDragEventArgs args)
	{
		DnDExample_DragCursor.Hide();
		if (args.State == dfDragDropState.Dropped)
		{
			Count = 0;
			ItemName = string.Empty;
			Icon = string.Empty;
			Refresh();
		}
	}

	public void OnDragDrop(dfControl source, dfDragEventArgs args)
	{
		if (Count == 0 && args.Data is DndExample_InventoryItem)
		{
			DndExample_InventoryItem dndExample_InventoryItem = (DndExample_InventoryItem)args.Data;
			ItemName = dndExample_InventoryItem.ItemName;
			Icon = dndExample_InventoryItem.Icon;
			Count = dndExample_InventoryItem.Count;
			args.State = dfDragDropState.Dropped;
			args.Use();
		}
		Refresh();
	}

	private void Refresh()
	{
		_container = GetComponent<dfPanel>();
		_sprite = _container.Find("Icon") as dfSprite;
		_label = _container.Find("Count") as dfLabel;
		_sprite.SpriteName = Icon;
		_label.Text = ((Count <= 1) ? string.Empty : Count.ToString());
	}
}
