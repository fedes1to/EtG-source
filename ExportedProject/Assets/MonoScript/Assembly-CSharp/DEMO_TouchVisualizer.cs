using UnityEngine;

[AddComponentMenu("Daikon Forge/Input/Debugging/Touch Visualizer")]
public class DEMO_TouchVisualizer : MonoBehaviour
{
	public bool editorOnly;

	public bool showMouse;

	public bool showPlatformInfo;

	public int iconSize = 32;

	public Texture2D touchIcon;

	private IDFTouchInputSource input;

	private void Awake()
	{
		base.useGUILayout = false;
	}

	public void OnGUI()
	{
		if (editorOnly && !Application.isEditor)
		{
			return;
		}
		if (input == null)
		{
			dfInputManager component = GetComponent<dfInputManager>();
			if (component == null)
			{
				Debug.LogError("No dfInputManager instance found", this);
				base.enabled = false;
				return;
			}
			if (!component.UseTouch)
			{
				if (Application.isPlaying)
				{
					base.enabled = false;
				}
				return;
			}
			input = component.TouchInputSource;
			if (input == null)
			{
				Debug.LogError("No dfTouchInputSource component found", this);
				base.enabled = false;
				return;
			}
		}
		if (showPlatformInfo)
		{
			Rect position = new Rect(5f, 0f, 800f, 25f);
			GUI.Label(position, string.Concat("Touch Source: ", input, ", Platform: ", Application.platform));
		}
		if (showMouse && !Application.isEditor)
		{
			drawTouchIcon(Input.mousePosition);
		}
		int touchCount = input.TouchCount;
		for (int i = 0; i < touchCount; i++)
		{
			drawTouchIcon(input.GetTouch(i).position);
		}
	}

	private void drawTouchIcon(Vector3 pos)
	{
		int height = Screen.height;
		pos.y = (float)height - pos.y;
		Rect position = new Rect(pos.x - (float)(iconSize / 2), pos.y - (float)(iconSize / 2), iconSize, iconSize);
		GUI.DrawTexture(position, touchIcon);
	}
}
