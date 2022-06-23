using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Daikon Forge/Input/Gestures/Pan")]
public class dfPanGesture : dfGestureBase
{
	[SerializeField]
	protected float minDistance = 25f;

	private bool multiTouchMode;

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

	public Vector2 Delta { get; protected set; }

	public event dfGestureEventHandler<dfPanGesture> PanGestureStart;

	public event dfGestureEventHandler<dfPanGesture> PanGestureMove;

	public event dfGestureEventHandler<dfPanGesture> PanGestureEnd;

	protected void Start()
	{
	}

	public void OnMouseDown(dfControl source, dfMouseEventArgs args)
	{
		Vector2 vector2 = (base.StartPosition = (base.CurrentPosition = args.Position));
		base.State = dfGestureState.Possible;
		base.StartTime = Time.realtimeSinceStartup;
		Delta = Vector2.zero;
	}

	public void OnMouseMove(dfControl source, dfMouseEventArgs args)
	{
		if (base.State == dfGestureState.Possible)
		{
			if (Vector2.Distance(args.Position, base.StartPosition) >= minDistance)
			{
				base.State = dfGestureState.Began;
				base.CurrentPosition = args.Position;
				Delta = args.Position - base.StartPosition;
				if (this.PanGestureStart != null)
				{
					this.PanGestureStart(this);
				}
				base.gameObject.Signal("OnPanGestureStart", this);
			}
		}
		else if (base.State == dfGestureState.Began || base.State == dfGestureState.Changed)
		{
			base.State = dfGestureState.Changed;
			Delta = args.Position - base.CurrentPosition;
			base.CurrentPosition = args.Position;
			if (this.PanGestureMove != null)
			{
				this.PanGestureMove(this);
			}
			base.gameObject.Signal("OnPanGestureMove", this);
		}
	}

	public void OnMouseUp(dfControl source, dfMouseEventArgs args)
	{
		endPanGesture();
	}

	public void OnMultiTouchEnd()
	{
		endPanGesture();
		multiTouchMode = false;
	}

	public void OnMultiTouch(dfControl source, dfTouchEventArgs args)
	{
		Vector2 center = getCenter(args.Touches);
		if (!multiTouchMode)
		{
			endPanGesture();
			multiTouchMode = true;
			base.State = dfGestureState.Possible;
			base.StartPosition = center;
		}
		else if (base.State == dfGestureState.Possible)
		{
			if (Vector2.Distance(center, base.StartPosition) >= minDistance)
			{
				base.State = dfGestureState.Began;
				base.CurrentPosition = center;
				Delta = base.CurrentPosition - base.StartPosition;
				if (this.PanGestureStart != null)
				{
					this.PanGestureStart(this);
				}
				base.gameObject.Signal("OnPanGestureStart", this);
			}
		}
		else if (base.State == dfGestureState.Began || base.State == dfGestureState.Changed)
		{
			base.State = dfGestureState.Changed;
			Delta = center - base.CurrentPosition;
			base.CurrentPosition = center;
			if (this.PanGestureMove != null)
			{
				this.PanGestureMove(this);
			}
			base.gameObject.Signal("OnPanGestureMove", this);
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

	private void endPanGesture()
	{
		Delta = Vector2.zero;
		base.StartPosition = Vector2.one * float.MinValue;
		if (base.State == dfGestureState.Began || base.State == dfGestureState.Changed)
		{
			base.State = dfGestureState.Ended;
			if (this.PanGestureEnd != null)
			{
				this.PanGestureEnd(this);
			}
			base.gameObject.Signal("OnPanGestureEnd", this);
		}
		else if (base.State == dfGestureState.Possible)
		{
			base.State = dfGestureState.Cancelled;
		}
	}
}
