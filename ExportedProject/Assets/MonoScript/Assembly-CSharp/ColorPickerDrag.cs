using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Color Picker/Drag and Drop")]
public class ColorPickerDrag : MonoBehaviour
{
	private Texture2D dragTexture;

	private dfSlicedSprite control;

	private bool isDragging;

	public void Start()
	{
		control = GetComponent<dfSlicedSprite>();
	}

	private void OnGUI()
	{
		if (Application.isPlaying && isDragging)
		{
			if (dragTexture == null)
			{
				dragTexture = new Texture2D(2, 2);
				dragTexture.SetPixel(0, 0, Color.white);
				dragTexture.SetPixel(0, 1, Color.white);
				dragTexture.SetPixel(1, 0, Color.white);
				dragTexture.SetPixel(1, 1, Color.white);
				dragTexture.Apply();
			}
			Vector3 mousePosition = Input.mousePosition;
			Rect position = new Rect(mousePosition.x - 15f, (float)Screen.height - mousePosition.y - 5f, 30f, 15f);
			Color color = GUI.color;
			GUI.color = control.Color;
			GUI.DrawTexture(position, dragTexture);
			GUI.color = color;
		}
	}

	public void OnDragStart(dfControl control, dfDragEventArgs dragEvent)
	{
		isDragging = true;
		dragEvent.Data = control.Color;
		dragEvent.State = dfDragDropState.Dragging;
		dragEvent.Use();
	}

	public void OnDragEnd(dfControl source, dfDragEventArgs args)
	{
		isDragging = false;
	}
}
