using UnityEngine;

public abstract class dfTouchInputSourceComponent : MonoBehaviour
{
	public int Priority;

	public abstract IDFTouchInputSource Source { get; }
}
