using UnityEngine;

internal class dfTouchTrackingInfo
{
	private TouchPhase phase;

	private Vector2 position = Vector2.one * float.MinValue;

	private Vector2 deltaPosition = Vector2.zero;

	private float deltaTime;

	private float lastUpdateTime = Time.realtimeSinceStartup;

	public bool IsActive;

	public int FingerID { get; set; }

	public TouchPhase Phase
	{
		get
		{
			return phase;
		}
		set
		{
			IsActive = true;
			phase = value;
			if (value == TouchPhase.Stationary)
			{
				deltaTime = float.Epsilon;
				deltaPosition = Vector2.zero;
				lastUpdateTime = Time.realtimeSinceStartup;
			}
		}
	}

	public Vector2 Position
	{
		get
		{
			return position;
		}
		set
		{
			IsActive = true;
			if (Phase == TouchPhase.Began)
			{
				deltaPosition = Vector2.zero;
			}
			else
			{
				deltaPosition = value - position;
			}
			position = value;
			float realtimeSinceStartup = Time.realtimeSinceStartup;
			deltaTime = realtimeSinceStartup - lastUpdateTime;
			lastUpdateTime = realtimeSinceStartup;
		}
	}

	public static implicit operator dfTouchInfo(dfTouchTrackingInfo info)
	{
		dfTouchInfo result = new dfTouchInfo(info.FingerID, info.phase, (info.phase == TouchPhase.Began) ? 1 : 0, info.position, info.deltaPosition, info.deltaTime);
		return result;
	}
}
