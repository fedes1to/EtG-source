using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Color Picker/Hue Slider")]
public class HueSliderSelector : MonoBehaviour
{
	private dfSlider slider;

	private Color hue;

	public Color Hue
	{
		get
		{
			return hue;
		}
		set
		{
			if (!object.Equals(value, hue))
			{
				hue = value;
				if (slider != null)
				{
					slider.Value = HSBColor.FromColor(value).h;
				}
			}
		}
	}

	public void Start()
	{
		slider = GetComponent<dfSlider>();
	}

	public void OnValueChanged(dfControl control, float value)
	{
		hue = new HSBColor(value, 1f, 1f, 1f).ToColor();
	}
}
