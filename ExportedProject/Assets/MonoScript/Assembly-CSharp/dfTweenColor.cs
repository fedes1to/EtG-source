using UnityEngine;

[AddComponentMenu("Daikon Forge/Tweens/Color")]
public class dfTweenColor : dfTweenComponent<Color>
{
	public override Color offset(Color lhs, Color rhs)
	{
		return lhs + rhs;
	}

	public override Color evaluate(Color startValue, Color endValue, float time)
	{
		Vector4 vector = startValue;
		Vector4 vector2 = endValue;
		Vector4 vector3 = new Vector4(dfTweenComponent<Color>.Lerp(vector.x, vector2.x, time), dfTweenComponent<Color>.Lerp(vector.y, vector2.y, time), dfTweenComponent<Color>.Lerp(vector.z, vector2.z, time), dfTweenComponent<Color>.Lerp(vector.w, vector2.w, time));
		return vector3;
	}
}
