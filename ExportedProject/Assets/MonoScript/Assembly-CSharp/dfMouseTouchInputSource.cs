using System.Collections.Generic;
using UnityEngine;

public class dfMouseTouchInputSource : IDFTouchInputSource
{
	private List<dfTouchInfo> activeTouches = new List<dfTouchInfo>();

	private dfTouchTrackingInfo touch;

	private dfTouchTrackingInfo altTouch;

	public bool MirrorAlt { get; set; }

	public bool ParallelAlt { get; set; }

	public int TouchCount
	{
		get
		{
			int num = 0;
			if (touch != null)
			{
				num++;
			}
			if (altTouch != null)
			{
				num++;
			}
			return num;
		}
	}

	public IList<dfTouchInfo> Touches
	{
		get
		{
			activeTouches.Clear();
			if (touch != null)
			{
				activeTouches.Add(touch);
			}
			if (altTouch != null)
			{
				activeTouches.Add(altTouch);
			}
			return activeTouches;
		}
	}

	public void Update()
	{
		if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButtonDown(1))
		{
			if (altTouch != null)
			{
				altTouch.Phase = TouchPhase.Ended;
				return;
			}
			altTouch = new dfTouchTrackingInfo
			{
				Phase = TouchPhase.Began,
				FingerID = 1,
				Position = Input.mousePosition
			};
			return;
		}
		if (Input.GetKeyUp(KeyCode.LeftAlt))
		{
			if (altTouch != null)
			{
				altTouch.Phase = TouchPhase.Ended;
				return;
			}
		}
		else if (altTouch != null)
		{
			if (altTouch.Phase == TouchPhase.Ended)
			{
				altTouch = null;
			}
			else if (altTouch.Phase == TouchPhase.Began || altTouch.Phase == TouchPhase.Moved)
			{
				altTouch.Phase = TouchPhase.Stationary;
			}
		}
		if (touch != null)
		{
			touch.IsActive = false;
		}
		if (touch != null && Input.GetKeyDown(KeyCode.Escape))
		{
			touch.Phase = TouchPhase.Canceled;
		}
		else if (touch == null || touch.Phase != TouchPhase.Canceled)
		{
			if (Input.GetMouseButtonUp(0))
			{
				if (touch != null)
				{
					touch.Phase = TouchPhase.Ended;
				}
			}
			else if (Input.GetMouseButtonDown(0))
			{
				touch = new dfTouchTrackingInfo
				{
					FingerID = 0,
					Phase = TouchPhase.Began,
					Position = Input.mousePosition
				};
			}
			else if (touch != null && touch.Phase != TouchPhase.Ended)
			{
				Vector2 vector = (Vector2)Input.mousePosition - touch.Position;
				bool flag = Vector2.Distance(Input.mousePosition, touch.Position) > float.Epsilon;
				touch.Position = Input.mousePosition;
				touch.Phase = (flag ? TouchPhase.Moved : TouchPhase.Stationary);
				if (flag && altTouch != null && (MirrorAlt || ParallelAlt))
				{
					if (MirrorAlt)
					{
						altTouch.Position -= vector;
					}
					else
					{
						altTouch.Position += vector;
					}
					altTouch.Phase = TouchPhase.Moved;
				}
			}
		}
		if (touch != null && !touch.IsActive)
		{
			touch = null;
		}
	}

	public dfTouchInfo GetTouch(int index)
	{
		return Touches[index];
	}
}
