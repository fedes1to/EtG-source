using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Daikon Forge/Input/Gestures/Rotate")]
public class dfRotateGesture : dfGestureBase
{
	[SerializeField]
	protected float thresholdAngle = 10f;

	private float accumulatedDelta;

	public float AngleDelta { get; protected set; }

	public event dfGestureEventHandler<dfRotateGesture> RotateGestureStart;

	public event dfGestureEventHandler<dfRotateGesture> RotateGestureUpdate;

	public event dfGestureEventHandler<dfRotateGesture> RotateGestureEnd;

	protected void Start()
	{
	}

	public void OnMultiTouchEnd()
	{
		endGesture();
	}

	public void OnMultiTouch(dfControl sender, dfTouchEventArgs args)
	{
		List<dfTouchInfo> touches = args.Touches;
		if (base.State == dfGestureState.None || base.State == dfGestureState.Cancelled || base.State == dfGestureState.Ended)
		{
			base.State = dfGestureState.Possible;
			accumulatedDelta = 0f;
		}
		else if (base.State == dfGestureState.Possible)
		{
			if (!isRotateMovement(args.Touches))
			{
				return;
			}
			float num = getAngleDelta(touches) + accumulatedDelta;
			if (Mathf.Abs(num) < thresholdAngle)
			{
				accumulatedDelta = num;
				return;
			}
			base.State = dfGestureState.Began;
			Vector2 vector2 = (base.StartPosition = (base.CurrentPosition = getCenter(touches)));
			AngleDelta = num;
			if (this.RotateGestureStart != null)
			{
				this.RotateGestureStart(this);
			}
			base.gameObject.Signal("OnRotateGestureStart", this);
		}
		else
		{
			if (base.State != dfGestureState.Began && base.State != dfGestureState.Changed)
			{
				return;
			}
			float angleDelta = getAngleDelta(touches);
			if (!(Mathf.Abs(angleDelta) <= float.Epsilon) && !(Mathf.Abs(angleDelta) > 22.5f))
			{
				base.State = dfGestureState.Changed;
				AngleDelta = angleDelta;
				base.CurrentPosition = getCenter(touches);
				if (this.RotateGestureUpdate != null)
				{
					this.RotateGestureUpdate(this);
				}
				base.gameObject.Signal("OnRotateGestureUpdate", this);
			}
		}
	}

	private float getAngleDelta(List<dfTouchInfo> touches)
	{
		if (touches.Count < 2)
		{
			return 0f;
		}
		dfTouchInfo dfTouchInfo2 = touches[0];
		dfTouchInfo dfTouchInfo3 = touches[1];
		if (Vector2.Distance(dfTouchInfo2.deltaPosition, dfTouchInfo3.deltaPosition) <= float.Epsilon)
		{
			return 0f;
		}
		Vector2 vector = dfTouchInfo2.deltaPosition * (BraveTime.DeltaTime / dfTouchInfo2.deltaTime);
		Vector2 vector2 = dfTouchInfo3.deltaPosition * (BraveTime.DeltaTime / dfTouchInfo3.deltaTime);
		Vector2 vector3 = dfTouchInfo2.position - vector - (dfTouchInfo3.position - vector2);
		float num = deltaAngle(end: (dfTouchInfo2.position - dfTouchInfo3.position).normalized, start: vector3.normalized);
		if (float.IsNaN(num))
		{
			return 0f;
		}
		if (dfTouchInfo2.phase == TouchPhase.Stationary || dfTouchInfo3.phase == TouchPhase.Stationary)
		{
			num *= 0.5f;
		}
		return num;
	}

	private float deltaAngle(Vector2 start, Vector2 end)
	{
		float y = start.x * end.y - start.y * end.x;
		return 57.29578f * Mathf.Atan2(y, Vector2.Dot(start, end));
	}

	private Vector2 getCenter(List<dfTouchInfo> list)
	{
		Vector2 zero = Vector2.zero;
		for (int i = 0; i < list.Count; i++)
		{
			zero += list[i].position;
		}
		return zero / list.Count;
	}

	private bool isRotateMovement(List<dfTouchInfo> list)
	{
		return Mathf.Abs(getAngleDelta(list)) >= 0.1f;
	}

	private void endGesture()
	{
		AngleDelta = 0f;
		accumulatedDelta = 0f;
		if (base.State == dfGestureState.Began || base.State == dfGestureState.Changed)
		{
			base.State = dfGestureState.Ended;
			if (this.RotateGestureEnd != null)
			{
				this.RotateGestureEnd(this);
			}
			base.gameObject.Signal("OnRotateGestureEnd", this);
		}
		else if (base.State == dfGestureState.Possible)
		{
			base.State = dfGestureState.Cancelled;
		}
		else
		{
			base.State = dfGestureState.None;
		}
	}
}
