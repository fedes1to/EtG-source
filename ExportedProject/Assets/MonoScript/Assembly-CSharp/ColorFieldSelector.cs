using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Color Picker/Color Field Selector")]
public class ColorFieldSelector : MonoBehaviour
{
	public dfControl indicator;

	public dfControl sliders;

	public dfControl selectedColorDisplay;

	private dfTextureSprite control;

	private Color hue;

	private Color color;

	public Color SelectedColor
	{
		get
		{
			return color;
		}
		set
		{
			color = value;
			updateHotspot();
		}
	}

	public Color Hue
	{
		get
		{
			return hue;
		}
		set
		{
			hue = value;
			updateSelectedColor();
		}
	}

	public void Start()
	{
		control = GetComponent<dfTextureSprite>();
		hue = HSBColor.GetHue(control.Color);
		color = control.Color;
		updateHotspot();
	}

	public void Update()
	{
		Material renderMaterial = control.RenderMaterial;
		if (renderMaterial != null)
		{
			control.RenderMaterial.color = hue;
		}
		if (selectedColorDisplay != null)
		{
			selectedColorDisplay.Color = color;
		}
	}

	public void OnMouseDown(dfControl control, dfMouseEventArgs mouseEvent)
	{
		updateHotspot(mouseEvent);
	}

	public void OnMouseMove(dfControl control, dfMouseEventArgs mouseEvent)
	{
		if (mouseEvent.Buttons == dfMouseButtons.Left)
		{
			updateHotspot(mouseEvent);
		}
	}

	private void updateHotspot()
	{
		if (!(control == null))
		{
			HSBColor hSBColor = HSBColor.FromColor(SelectedColor);
			Vector2 vector = new Vector2(hSBColor.s * control.Width, (1f - hSBColor.b) * control.Height);
			indicator.RelativePosition = vector - indicator.Size * 0.5f;
		}
	}

	private void updateHotspot(dfMouseEventArgs mouseEvent)
	{
		if (!(indicator == null))
		{
			Vector2 hitPosition = control.GetHitPosition(mouseEvent);
			indicator.RelativePosition = hitPosition - indicator.Size * 0.5f;
			updateSelectedColor();
		}
	}

	private void updateSelectedColor()
	{
		if (control == null)
		{
			control = GetComponent<dfTextureSprite>();
		}
		Vector3 vector = indicator.RelativePosition + (Vector3)indicator.Size * 0.5f;
		color = getColor(vector.x, vector.y, control.Width, control.Height, Hue);
	}

	private Color getColor(float x, float y, float width, float height, Color hue)
	{
		float value = x / width;
		float value2 = y / height;
		value = Mathf.Clamp01(value);
		value2 = Mathf.Clamp01(value2);
		return Vector4.Lerp(Color.white, hue, value) * (1f - value2);
	}
}
