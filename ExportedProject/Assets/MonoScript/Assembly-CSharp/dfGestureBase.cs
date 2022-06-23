using UnityEngine;

public abstract class dfGestureBase : MonoBehaviour
{
	private dfControl control;

	public dfGestureState State { get; protected set; }

	public Vector2 StartPosition { get; protected set; }

	public Vector2 CurrentPosition { get; protected set; }

	public float StartTime { get; protected set; }

	public dfControl Control
	{
		get
		{
			if (control == null)
			{
				control = GetComponent<dfControl>();
			}
			return control;
		}
	}
}
