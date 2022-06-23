using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Daikon Forge/Input/Gestures/Resize")]
public class dfResizeGesture : dfGestureBase
{
	private float lastDistance;

	public float SizeDelta { get; protected set; }

	public event dfGestureEventHandler<dfResizeGesture> ResizeGestureStart;

	public event dfGestureEventHandler<dfResizeGesture> ResizeGestureUpdate;

	public event dfGestureEventHandler<dfResizeGesture> ResizeGestureEnd;

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
		}
		else if (base.State == dfGestureState.Possible)
		{
			if (isResizeMovement(args.Touches))
			{
				base.State = dfGestureState.Began;
				Vector2 vector2 = (base.StartPosition = (base.CurrentPosition = getCenter(touches)));
				lastDistance = Vector2.Distance(touches[0].position, touches[1].position);
				SizeDelta = 0f;
				if (this.ResizeGestureStart != null)
				{
					this.ResizeGestureStart(this);
				}
				base.gameObject.Signal("OnResizeGestureStart", this);
			}
		}
		else if ((base.State == dfGestureState.Began || base.State == dfGestureState.Changed) && isResizeMovement(touches))
		{
			base.State = dfGestureState.Changed;
			base.CurrentPosition = getCenter(touches);
			float num = Vector2.Distance(touches[0].position, touches[1].position);
			SizeDelta = num - lastDistance;
			lastDistance = num;
			if (this.ResizeGestureUpdate != null)
			{
				this.ResizeGestureUpdate(this);
			}
			base.gameObject.Signal("OnResizeGestureUpdate", this);
		}
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

	private bool isResizeMovement(List<dfTouchInfo> list)
	{
		if (list.Count < 2)
		{
			return false;
		}
		dfTouchInfo dfTouchInfo2 = list[0];
		Vector2 normalized = (dfTouchInfo2.deltaPosition * (BraveTime.DeltaTime / dfTouchInfo2.deltaTime)).normalized;
		dfTouchInfo dfTouchInfo3 = list[1];
		Vector2 normalized2 = (dfTouchInfo3.deltaPosition * (BraveTime.DeltaTime / dfTouchInfo3.deltaTime)).normalized;
		float f = Vector2.Dot(normalized, (dfTouchInfo2.position - dfTouchInfo3.position).normalized);
		float f2 = Vector2.Dot(normalized2, (dfTouchInfo3.position - dfTouchInfo2.position).normalized);
		return Mathf.Abs(f) >= 0.214601845f || Mathf.Abs(f2) >= 0.214601845f;
	}

	private void endGesture()
	{
		if (base.State == dfGestureState.Began || base.State == dfGestureState.Changed)
		{
			if (base.State == dfGestureState.Began)
			{
				base.State = dfGestureState.Cancelled;
			}
			else
			{
				base.State = dfGestureState.Ended;
			}
			float num2 = (lastDistance = (SizeDelta = 0f));
			if (this.ResizeGestureEnd != null)
			{
				this.ResizeGestureEnd(this);
			}
			base.gameObject.Signal("OnResizeGestureEnd", this);
		}
		else
		{
			base.State = dfGestureState.None;
		}
	}
}
