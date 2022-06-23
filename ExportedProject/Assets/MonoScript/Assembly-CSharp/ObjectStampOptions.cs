using UnityEngine;

public class ObjectStampOptions : MonoBehaviour
{
	public Vector2 xPositionRange;

	public Vector2 yPositionRange;

	public Vector3 GetPositionOffset()
	{
		return new Vector3(Random.Range(xPositionRange.x, xPositionRange.y), Random.Range(yPositionRange.x, yPositionRange.y));
	}
}
