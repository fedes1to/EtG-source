using UnityEngine;

[AddComponentMenu("Daikon Forge/Input/Gestures/Hold")]
public class dfHoldGesture : dfGestureBase
{
	[SerializeField]
	private float minTime = 0.75f;

	[SerializeField]
	private float maxDistance = 25f;

	public float MinimumTime
	{
		get
		{
			return minTime;
		}
		set
		{
			minTime = value;
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

	public float HoldLength
	{
		get
		{
			if (base.State == dfGestureState.Began)
			{
				return Time.realtimeSinceStartup - base.StartTime;
			}
			return 0f;
		}
	}

	public event dfGestureEventHandler<dfHoldGesture> HoldGestureStart;

	public event dfGestureEventHandler<dfHoldGesture> HoldGestureEnd;

	protected void Start()
	{
	}

	protected void Update()
	{
		if (base.State == dfGestureState.Possible && Time.realtimeSinceStartup - base.StartTime >= minTime)
		{
			base.State = dfGestureState.Began;
			if (this.HoldGestureStart != null)
			{
				this.HoldGestureStart(this);
			}
			base.gameObject.Signal("OnHoldGestureStart", this);
		}
	}

	public void OnMouseDown(dfControl source, dfMouseEventArgs args)
	{
		base.State = dfGestureState.Possible;
		Vector2 vector2 = (base.StartPosition = (base.CurrentPosition = args.Position));
		base.StartTime = Time.realtimeSinceStartup;
	}

	public void OnMouseMove(dfControl source, dfMouseEventArgs args)
	{
		if (base.State != dfGestureState.Possible && base.State != dfGestureState.Began)
		{
			return;
		}
		base.CurrentPosition = args.Position;
		if (!(Vector2.Distance(args.Position, base.StartPosition) > maxDistance))
		{
			return;
		}
		if (base.State == dfGestureState.Possible)
		{
			base.State = dfGestureState.Failed;
		}
		else if (base.State == dfGestureState.Began)
		{
			base.State = dfGestureState.Cancelled;
			if (this.HoldGestureEnd != null)
			{
				this.HoldGestureEnd(this);
			}
			base.gameObject.Signal("OnHoldGestureEnd", this);
		}
	}

	public void OnMouseUp(dfControl source, dfMouseEventArgs args)
	{
		if (base.State == dfGestureState.Began)
		{
			base.CurrentPosition = args.Position;
			base.State = dfGestureState.Ended;
			if (this.HoldGestureEnd != null)
			{
				this.HoldGestureEnd(this);
			}
			base.gameObject.Signal("OnHoldGestureEnd", this);
		}
		base.State = dfGestureState.None;
	}

	public void OnMultiTouch(dfControl source, dfTouchEventArgs args)
	{
		if (base.State == dfGestureState.Began)
		{
			base.State = dfGestureState.Cancelled;
			if (this.HoldGestureEnd != null)
			{
				this.HoldGestureEnd(this);
			}
			base.gameObject.Signal("OnHoldGestureEnd", this);
		}
		else
		{
			base.State = dfGestureState.Failed;
		}
	}
}
