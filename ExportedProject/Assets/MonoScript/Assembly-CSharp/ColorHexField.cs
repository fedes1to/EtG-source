using System.Globalization;
using UnityEngine;

[AddComponentMenu("Daikon Forge/Examples/Color Picker/Color Hex Field")]
public class ColorHexField : MonoBehaviour
{
	public ColorFieldSelector colorField;

	private dfTextbox control;

	public void Start()
	{
		control = GetComponent<dfTextbox>();
	}

	public void Update()
	{
		if (!control.HasFocus)
		{
			Color32 color = colorField.SelectedColor;
			control.Text = string.Format("{0:X2}{1:X2}{2:X2}", color.r, color.g, color.b);
		}
	}

	public void OnTextSubmitted(dfControl control, string value)
	{
		uint result = 0u;
		if (uint.TryParse(value, NumberStyles.HexNumber, null, out result))
		{
			Color color = UIntToColor(result);
			colorField.Hue = HSBColor.GetHue(color);
			colorField.SelectedColor = color;
		}
	}

	private Color UIntToColor(uint color)
	{
		byte a = (byte)(color >> 24);
		byte r = (byte)(color >> 16);
		byte g = (byte)(color >> 8);
		byte b = (byte)(color >> 0);
		return new Color32(r, g, b, a);
	}
}
