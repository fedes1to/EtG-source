using System.Collections.Generic;

public interface IDFTouchInputSource
{
	int TouchCount { get; }

	IList<dfTouchInfo> Touches { get; }

	void Update();

	dfTouchInfo GetTouch(int index);
}
