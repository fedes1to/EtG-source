using UnityEngine;

[AddComponentMenu("Daikon Forge/Input/Gestures/Tap")]
public class dfTapGesture : dfGestureBase
{
	[SerializeField]
	private float timeout = 0.25f;

	[SerializeField]
	private float maxDistance = 25f;

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

	public event dfGestureEventHandler<dfTapGesture> TapGesture;

	protected void Start()
	{
	}

	public void OnMouseDown(dfControl source, dfMouseEventArgs args)
	{
		Vector2 vector2 = (base.StartPosition = (base.CurrentPosition = args.Position));
		base.State = dfGestureState.Possible;
		base.StartTime = Time.realtimeSinceStartup;
	}

	public void OnMouseMove(dfControl source, dfMouseEventArgs args)
	{
		if (base.State == dfGestureState.Possible || base.State == dfGestureState.Began)
		{
			base.CurrentPosition = args.Position;
			if (Vector2.Distance(args.Position, base.StartPosition) > maxDistance)
			{
				base.State = dfGestureState.Failed;
			}
		}
	}

	public void OnMouseUp(dfControl source, dfMouseEventArgs args)
	{
		if (base.State == dfGestureState.Possible)
		{
			if (Time.realtimeSinceStartup - base.StartTime <= timeout)
			{
				base.CurrentPosition = args.Position;
				base.State = dfGestureState.Ended;
				if (this.TapGesture != null)
				{
					this.TapGesture(this);
				}
				base.gameObject.Signal("OnTapGesture", this);
			}
			else
			{
				base.State = dfGestureState.Failed;
			}
		}
		else
		{
			base.State = dfGestureState.None;
		}
	}

	public void OnMultiTouch(dfControl source, dfTouchEventArgs args)
	{
		base.State = dfGestureState.Failed;
	}
}
