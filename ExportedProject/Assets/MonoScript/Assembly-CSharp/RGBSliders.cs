using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Color Picker/RGB Sliders Container")]
public class RGBSliders : MonoBehaviour
{
	public ColorFieldSelector colorField;

	public dfSlider redSlider;

	public dfSlider greenSlider;

	public dfSlider blueSlider;

	private dfPanel container;

	private Color color;

	private Color hue;

	public Color SelectedColor
	{
		get
		{
			return color;
		}
		set
		{
			color = value;
			updateSliders();
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
		}
	}

	public void Start()
	{
		container = GetComponent<dfPanel>();
	}

	public void Update()
	{
		if (!container.ContainsFocus)
		{
			SelectedColor = colorField.SelectedColor;
		}
	}

	public void OnValueChanged(dfControl source, float value)
	{
		if (container.ContainsFocus)
		{
			color = new Color(redSlider.Value, greenSlider.Value, blueSlider.Value);
			colorField.Hue = (hue = HSBColor.GetHue(color));
			colorField.SelectedColor = color;
		}
	}

	private void updateSliders()
	{
		redSlider.Value = color.r;
		greenSlider.Value = color.g;
		blueSlider.Value = color.b;
	}
}
