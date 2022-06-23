using UnityEngine;

public class FuseController : MonoBehaviour
{
	public tk2dTiledSprite[] fuseSegments;

	public GameObject sparkVFXPrefab;

	public float duration = 5f;

	private float TotalPixelLength;

	private Transform m_sparkInstance;

	private float UsedPixelLength;

	private bool m_triggered;

	private void Start()
	{
		for (int i = 0; i < fuseSegments.Length; i++)
		{
			TotalPixelLength += fuseSegments[i].dimensions.x;
		}
	}

	public void Trigger()
	{
		m_triggered = true;
	}

	private void Update()
	{
		if (!m_triggered)
		{
			return;
		}
		float usedPixelLength = UsedPixelLength;
		UsedPixelLength = usedPixelLength + TotalPixelLength / duration * BraveTime.DeltaTime;
		float num = UsedPixelLength - usedPixelLength;
		for (int i = 0; i < fuseSegments.Length; i++)
		{
			if (!(fuseSegments[i].dimensions.x <= 0f))
			{
				if (!(fuseSegments[i].dimensions.x < num))
				{
					fuseSegments[i].dimensions = fuseSegments[i].dimensions - new Vector2(num, 0f);
					num = 0f;
					break;
				}
				num -= fuseSegments[i].dimensions.x;
				fuseSegments[i].dimensions = new Vector2(0f, fuseSegments[i].dimensions.y);
			}
		}
	}
}
