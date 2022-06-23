using DaikonForge.Tween;
using UnityEngine;

public static class TweenMaterialExtensions
{
	public static Tween<Color> TweenColor(this Material material)
	{
		return Tween<Color>.Obtain().SetStartValue(material.color).SetEndValue(material.color)
			.SetDuration(1f)
			.OnExecute(delegate(Color currentValue)
			{
				material.color = currentValue;
			});
	}

	public static Tween<float> TweenAlpha(this Material material)
	{
		return Tween<float>.Obtain().SetStartValue(material.color.a).SetEndValue(material.color.a)
			.SetDuration(1f)
			.OnExecute(delegate(float currentValue)
			{
				Color color = material.color;
				color.a = currentValue;
				material.color = color;
			});
	}
}
