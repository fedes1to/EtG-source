using UnityEngine;

public class SimpleSpriteRotator : MonoBehaviour
{
	public float angularVelocity;

	public float acceleration;

	public bool UseWorldCenter = true;

	public bool ForceUpdateZDepth;

	public bool RotateParent;

	public bool RotateDuringBossIntros;

	public bool RandomStartingAngle;

	private Transform m_transform;

	private tk2dSprite m_sprite;

	private void Start()
	{
		m_transform = ((!RotateParent) ? base.transform : base.transform.parent);
		m_sprite = GetComponent<tk2dSprite>();
		if (RandomStartingAngle)
		{
			DoRotation(Random.Range(0, 360));
		}
	}

	private void Update()
	{
		float num = BraveTime.DeltaTime;
		if (RotateDuringBossIntros && GameManager.IsBossIntro)
		{
			num = GameManager.INVARIANT_DELTA_TIME;
		}
		angularVelocity += acceleration * num;
		DoRotation(angularVelocity * num);
	}

	private void DoRotation(float degrees)
	{
		if (UseWorldCenter)
		{
			m_transform.RotateAround(m_sprite.WorldCenter, Vector3.forward, degrees);
		}
		else
		{
			m_transform.Rotate(Vector3.forward, degrees);
		}
		if (ForceUpdateZDepth)
		{
			m_sprite.ForceRotationRebuild();
			m_sprite.UpdateZDepth();
		}
	}
}
