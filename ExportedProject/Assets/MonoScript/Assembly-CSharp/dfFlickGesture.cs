using UnityEngine;

[AddComponentMenu("Daikon Forge/Input/Gestures/Flick")]
public class dfFlickGesture : dfGestureBase
{
	[SerializeField]
	private float timeout = 0.25f;

	[SerializeField]
	private float minDistance = 25f;

	private float hoverTime;

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

	public float MinimumDistance
	{
		get
		{
			return minDistance;
		}
		set
		{
			minDistance = value;
		}
	}

	public float DeltaTime { get; protected set; }

	public event dfGestureEventHandler<dfFlickGesture> FlickGesture;

	protected void Start()
	{
	}

	public void OnMouseDown(dfControl source, dfMouseEventArgs args)
	{
		Vector2 vector2 = (base.StartPosition = (base.CurrentPosition = args.Position));
		base.State = dfGestureState.Possible;
		base.StartTime = Time.realtimeSinceStartup;
		hoverTime = Time.realtimeSinceStartup;
	}

	public void OnMouseHover(dfControl source, dfMouseEventArgs args)
	{
		if (base.State == dfGestureState.Possible && Time.realtimeSinceStartup - hoverTime >= timeout)
		{
			Vector2 vector2 = (base.StartPosition = (base.CurrentPosition = args.Position));
			base.StartTime = Time.realtimeSinceStartup;
		}
	}

	public void OnMouseMove(dfControl source, dfMouseEventArgs args)
	{
		hoverTime = Time.realtimeSinceStartup;
		if (base.State == dfGestureState.Possible || base.State == dfGestureState.Began)
		{
			base.State = dfGestureState.Began;
			base.CurrentPosition = args.Position;
		}
	}

	public void OnMouseUp(dfControl source, dfMouseEventArgs args)
	{
		if (base.State != dfGestureState.Began)
		{
			return;
		}
		base.CurrentPosition = args.Position;
		if (Time.realtimeSinceStartup - base.StartTime <= timeout)
		{
			float num = Vector2.Distance(base.CurrentPosition, base.StartPosition);
			if (num >= minDistance)
			{
				base.State = dfGestureState.Ended;
				DeltaTime = Time.realtimeSinceStartup - base.StartTime;
				if (this.FlickGesture != null)
				{
					this.FlickGesture(this);
				}
				base.gameObject.Signal("OnFlickGesture", this);
			}
			else
			{
				base.State = dfGestureState.Failed;
			}
		}
		else
		{
			base.State = dfGestureState.Failed;
		}
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
		base.State = dfGestureState.None;
	}
}
