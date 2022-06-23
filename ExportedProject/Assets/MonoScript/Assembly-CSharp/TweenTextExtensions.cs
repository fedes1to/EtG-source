using DaikonForge.Tween;
using UnityEngine;

public static class TweenTextExtensions
{
	public static Tween<float> TweenAlpha(this TextMesh text)
	{
		return Tween<float>.Obtain().SetStartValue(text.color.a).SetEndValue(text.color.a)
			.SetDuration(1f)
			.OnExecute(delegate(float currentValue)
			{
				Color color = text.color;
				color.a = currentValue;
				text.color = color;
			});
	}

	public static Tween<Color> TweenColor(this TextMesh text)
	{
		return Tween<Color>.Obtain().SetStartValue(text.color).SetEndValue(text.color)
			.SetDuration(1f)
			.OnExecute(delegate(Color currentValue)
			{
				text.color = currentValue;
			});
	}
}
