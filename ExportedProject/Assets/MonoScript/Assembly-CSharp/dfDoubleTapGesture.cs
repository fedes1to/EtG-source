using UnityEngine;

[AddComponentMenu("Daikon Forge/Input/Gestures/Double Tap")]
public class dfDoubleTapGesture : dfGestureBase
{
	[SerializeField]
	private float timeout = 0.5f;

	[SerializeField]
	private float maxDistance = 35f;

	public float Timeout
	{
		get
		{
			return timeout;
		}
		set
		{
			timeout = value;
		}
	}

	public float MaximumDistance
	{
		get
		{
			return maxDistance;
		}
		set
		{
			maxDistance = value;
		}
	}

	public event dfGestureEventHandler<dfDoubleTapGesture> DoubleTapGesture;

	protected void Start()
	{
	}

	public void OnMouseDown(dfControl source, dfMouseEventArgs args)
	{
		Vector2 vector2;
		if (base.State == dfGestureState.Possible)
		{
			float num = Time.realtimeSinceStartup - base.StartTime;
			if (num <= timeout && Vector2.Distance(args.Position, base.StartPosition) <= maxDistance)
			{
				vector2 = (base.StartPosition = (base.CurrentPosition = args.Position));
				base.State = dfGestureState.Began;
				if (this.DoubleTapGesture != null)
				{
					this.DoubleTapGesture(this);
				}
				base.gameObject.Signal("OnDoubleTapGesture", this);
				endGesture();
				return;
			}
		}
		vector2 = (base.StartPosition = (base.CurrentPosition = args.Position));
		base.State = dfGestureState.Possible;
		base.StartTime = Time.realtimeSinceStartup;
	}

	public void OnMouseLeave()
	{
		endGesture();
	}

	public void OnMultiTouchEnd()
	{
		endGesture();
	}

	public void OnMultiTouch()
	{
		endGesture();
	}

	private void endGesture()
	{
		if (base.State == dfGestureState.Began || base.State == dfGestureState.Changed)
		{
			base.State = dfGestureState.Ended;
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
