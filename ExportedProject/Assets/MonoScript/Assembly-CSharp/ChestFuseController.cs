using UnityEngine;

public class ChestFuseController : MonoBehaviour
{
	public tk2dTiledSprite[] fuseSegmentsInOrderOfAppearance;

	public GameObject sparksVFXPrefab;

	private Transform sparksInstance;

	private float totalLength = -1f;

	private float m_accumParticles;

	private void CalcLength()
	{
		totalLength = 0f;
		for (int i = 0; i < fuseSegmentsInOrderOfAppearance.Length; i++)
		{
			totalLength += fuseSegmentsInOrderOfAppearance[i].dimensions.x;
		}
	}

	public Vector2? SetFuseCompletion(float t)
	{
		if (totalLength < 0f)
		{
			CalcLength();
		}
		float num = Mathf.Clamp01(1f - t) * totalLength;
		Vector2? result = null;
		for (int i = 0; i < fuseSegmentsInOrderOfAppearance.Length; i++)
		{
			if (num < 0f)
			{
				break;
			}
			if (num > fuseSegmentsInOrderOfAppearance[i].dimensions.x)
			{
				num -= fuseSegmentsInOrderOfAppearance[i].dimensions.x;
				continue;
			}
			fuseSegmentsInOrderOfAppearance[i].dimensions = fuseSegmentsInOrderOfAppearance[i].dimensions.WithX(num);
			m_accumParticles += 30f * BraveTime.DeltaTime;
			int num2 = Mathf.FloorToInt(m_accumParticles);
			m_accumParticles -= num2;
			Vector3 vector = fuseSegmentsInOrderOfAppearance[i].transform.position + (Quaternion.Euler(0f, 0f, fuseSegmentsInOrderOfAppearance[i].transform.eulerAngles.z) * fuseSegmentsInOrderOfAppearance[i].dimensions.ToVector3ZUp() * 0.0625f).XY().ToVector3ZisY();
			result = vector.XY();
			float num3 = ((fuseSegmentsInOrderOfAppearance[i].transform.eulerAngles.z != 0f) ? 0f : (-0.0625f));
			GlobalSparksDoer.DoRandomParticleBurst(num2, vector + new Vector3(-0.125f, -0.125f + num3, 0f), vector + new Vector3(0f, num3, 0f), Vector3.up, 180f, 0.25f, null, null, Color.yellow);
		}
		return result;
	}
}
