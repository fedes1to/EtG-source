using UnityEngine;

public class dfMouseEventArgs : dfControlEventArgs
{
	public dfMouseButtons Buttons { get; private set; }

	public int Clicks { get; private set; }

	public float WheelDelta { get; private set; }

	public Vector2 MoveDelta { get; set; }

	public Vector2 Position { get; set; }

	public Ray Ray { get; set; }

	public dfMouseEventArgs(dfControl Source, dfMouseButtons button, int clicks, Ray ray, Vector2 location, float wheel)
		: base(Source)
	{
		Buttons = button;
		Clicks = clicks;
		Position = location;
		WheelDelta = wheel;
		Ray = ray;
	}

	public dfMouseEventArgs(dfControl Source)
		: base(Source)
	{
		Buttons = dfMouseButtons.None;
		Clicks = 0;
		Position = Vector2.zero;
		WheelDelta = 0f;
	}
}
