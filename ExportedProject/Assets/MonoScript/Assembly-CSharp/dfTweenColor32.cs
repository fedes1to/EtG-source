using UnityEngine;

[AddComponentMenu("Daikon Forge/Tweens/Color32")]
public class dfTweenColor32 : dfTweenComponent<Color32>
{
	public override Color32 offset(Color32 lhs, Color32 rhs)
	{
		return (Color)lhs + (Color)rhs;
	}

	public override Color32 evaluate(Color32 startValue, Color32 endValue, float time)
	{
		Vector4 vector = (Color)startValue;
		Vector4 vector2 = (Color)endValue;
		Vector4 vector3 = new Vector4(dfTweenComponent<Color32>.Lerp(vector.x, vector2.x, time), dfTweenComponent<Color32>.Lerp(vector.y, vector2.y, time), dfTweenComponent<Color32>.Lerp(vector.z, vector2.z, time), dfTweenComponent<Color32>.Lerp(vector.w, vector2.w, time));
		return (Color)vector3;
	}
}
