using System.Collections.Generic;
using UnityEngine;

public class dfMobileTouchInputSource : IDFTouchInputSource
{
	private static dfMobileTouchInputSource instance;

	private List<dfTouchInfo> activeTouches = new List<dfTouchInfo>();

	public static dfMobileTouchInputSource Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new dfMobileTouchInputSource();
			}
			return instance;
		}
	}

	public int TouchCount
	{
		get
		{
			return Input.touchCount;
		}
	}

	public IList<dfTouchInfo> Touches
	{
		get
		{
			return activeTouches;
		}
	}

	public dfTouchInfo GetTouch(int index)
	{
		return Input.GetTouch(index);
	}

	public void Update()
	{
		activeTouches.Clear();
		for (int i = 0; i < TouchCount; i++)
		{
			activeTouches.Add(GetTouch(i));
		}
	}
}
