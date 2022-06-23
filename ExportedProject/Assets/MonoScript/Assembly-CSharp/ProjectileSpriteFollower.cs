using UnityEngine;

public class ProjectileSpriteFollower : MonoBehaviour
{
	public tk2dBaseSprite TargetSprite;

	public float SmoothTime = 0.25f;

	private Vector3 m_currentVelocity;

	private void Awake()
	{
		base.transform.parent = null;
		base.transform.localRotation = Quaternion.identity;
	}

	private void LateUpdate()
	{
		if (!TargetSprite)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			base.transform.position = Vector3.SmoothDamp(base.transform.position, TargetSprite.transform.position, ref m_currentVelocity, SmoothTime);
		}
	}
}
