using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Touch/Drag Source Events")]
public class TouchDragSourceEvents : MonoBehaviour
{
	private dfLabel _label;

	private bool isDragging;

	public void Start()
	{
		_label = GetComponent<dfLabel>();
	}

	public void OnGUI()
	{
		if (isDragging)
		{
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.y = (float)Screen.height - mousePosition.y;
			Rect position = new Rect(mousePosition.x - 100f, mousePosition.y - 50f, 200f, 100f);
			GUI.Box(position, _label.name);
		}
	}

	public void OnDragEnd(dfControl control, dfDragEventArgs dragEvent)
	{
		if (dragEvent.State == dfDragDropState.Dropped)
		{
			_label.Text = "Dropped on " + dragEvent.Target.name;
		}
		else
		{
			_label.Text = "Drag Ended: " + dragEvent.State;
		}
		isDragging = false;
	}

	public void OnDragStart(dfControl control, dfDragEventArgs dragEvent)
	{
		_label.Text = "Dragging...";
		dragEvent.Data = base.name;
		dragEvent.State = dfDragDropState.Dragging;
		dragEvent.Use();
		isDragging = true;
	}
}
