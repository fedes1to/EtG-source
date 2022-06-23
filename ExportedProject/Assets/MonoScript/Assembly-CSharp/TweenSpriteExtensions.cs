using DaikonForge.Tween;
using UnityEngine;

public static class TweenSpriteExtensions
{
	public static Tween<float> TweenAlpha(this SpriteRenderer sprite)
	{
		return Tween<float>.Obtain().SetStartValue(sprite.color.a).SetEndValue(sprite.color.a)
			.SetDuration(1f)
			.OnExecute(delegate(float currentValue)
			{
				Color color = sprite.color;
				color.a = currentValue;
				sprite.color = color;
			});
	}

	public static Tween<Color> TweenColor(this SpriteRenderer sprite)
	{
		return Tween<Color>.Obtain().SetStartValue(sprite.color).SetEndValue(sprite.color)
			.SetDuration(1f)
			.OnExecute(delegate(Color currentValue)
			{
				sprite.color = currentValue;
			});
	}
}
