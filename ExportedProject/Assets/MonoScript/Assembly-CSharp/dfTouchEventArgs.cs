using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class dfTouchEventArgs : dfMouseEventArgs
{
	public dfTouchInfo Touch { get; private set; }

	public List<dfTouchInfo> Touches { get; private set; }

	public bool IsMultiTouch
	{
		get
		{
			return Touches.Count > 1;
		}
	}

	public dfTouchEventArgs(dfControl Source, dfTouchInfo touch, Ray ray)
		: base(Source, dfMouseButtons.Left, touch.tapCount, ray, touch.position, 0f)
	{
		Touch = touch;
		Touches = new List<dfTouchInfo> { touch };
		float deltaTime = BraveTime.DeltaTime;
		if (touch.deltaTime > float.Epsilon && deltaTime > float.Epsilon)
		{
			base.MoveDelta = touch.deltaPosition * (deltaTime / touch.deltaTime);
		}
		else
		{
			base.MoveDelta = touch.deltaPosition;
		}
	}

	public dfTouchEventArgs(dfControl source, List<dfTouchInfo> touches, Ray ray)
		: this(source, touches.First(), ray)
	{
		Touches = touches;
	}

	public dfTouchEventArgs(dfControl Source)
		: base(Source)
	{
		base.Position = Vector2.zero;
	}
}
