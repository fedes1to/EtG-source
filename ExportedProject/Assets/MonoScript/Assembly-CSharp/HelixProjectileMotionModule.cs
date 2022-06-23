using System;
using UnityEngine;

public class HelixProjectileMotionModule : ProjectileAndBeamMotionModule
{
	public float helixWavelength = 3f;

	public float helixAmplitude = 1f;

	public float helixBeamOffsetPerSecond = 6f;

	public bool ForceInvert;

	private bool m_helixInitialized;

	private Vector2 m_initialRightVector;

	private Vector2 m_initialUpVector;

	private Vector2 m_privateLastPosition;

	private float m_displacement;

	private float m_yDisplacement;

	public override void UpdateDataOnBounce(float angleDiff)
	{
		if (!float.IsNaN(angleDiff))
		{
			m_initialUpVector = Quaternion.Euler(0f, 0f, angleDiff) * m_initialUpVector;
			m_initialRightVector = Quaternion.Euler(0f, 0f, angleDiff) * m_initialRightVector;
		}
	}

	public override void AdjustRightVector(float angleDiff)
	{
		if (!float.IsNaN(angleDiff))
		{
			m_initialUpVector = Quaternion.Euler(0f, 0f, angleDiff) * m_initialUpVector;
			m_initialRightVector = Quaternion.Euler(0f, 0f, angleDiff) * m_initialRightVector;
		}
	}

	public override void Move(Projectile source, Transform projectileTransform, tk2dBaseSprite projectileSprite, SpeculativeRigidbody specRigidbody, ref float m_timeElapsed, ref Vector2 m_currentDirection, bool Inverted, bool shouldRotate)
	{
		ProjectileData baseData = source.baseData;
		Vector2 vector = ((!projectileSprite) ? projectileTransform.position.XY() : projectileSprite.WorldCenter);
		if (!m_helixInitialized)
		{
			m_helixInitialized = true;
			m_initialRightVector = ((!shouldRotate) ? m_currentDirection : projectileTransform.right.XY());
			m_initialUpVector = ((!shouldRotate) ? (Quaternion.Euler(0f, 0f, 90f) * m_currentDirection) : projectileTransform.up);
			m_privateLastPosition = vector;
			m_displacement = 0f;
			m_yDisplacement = 0f;
		}
		m_timeElapsed += BraveTime.DeltaTime;
		int num = ((!(Inverted ^ ForceInvert)) ? 1 : (-1));
		float num2 = m_timeElapsed * baseData.speed;
		float num3 = (float)num * helixAmplitude * Mathf.Sin(m_timeElapsed * (float)Math.PI * baseData.speed / helixWavelength);
		float num4 = num2 - m_displacement;
		float num5 = num3 - m_yDisplacement;
		Vector2 vector2 = (m_privateLastPosition = m_privateLastPosition + m_initialRightVector * num4 + m_initialUpVector * num5);
		if (shouldRotate)
		{
			float num6 = (m_timeElapsed + 0.01f) * baseData.speed;
			float num7 = (float)num * helixAmplitude * Mathf.Sin((m_timeElapsed + 0.01f) * (float)Math.PI * baseData.speed / helixWavelength);
			float num8 = BraveMathCollege.Atan2Degrees(num7 - num3, num6 - num2);
			projectileTransform.localRotation = Quaternion.Euler(0f, 0f, num8 + m_initialRightVector.ToAngle());
		}
		Vector2 vector3 = (vector2 - vector) / BraveTime.DeltaTime;
		float f = BraveMathCollege.Atan2Degrees(vector3);
		if (!float.IsNaN(f))
		{
			m_currentDirection = vector3.normalized;
		}
		m_displacement = num2;
		m_yDisplacement = num3;
		specRigidbody.Velocity = vector3;
	}

	public override void SentInDirection(ProjectileData baseData, Transform projectileTransform, tk2dBaseSprite projectileSprite, SpeculativeRigidbody specRigidbody, ref float m_timeElapsed, ref Vector2 m_currentDirection, bool shouldRotate, Vector2 dirVec, bool resetDistance, bool updateRotation)
	{
		Vector2 privateLastPosition = ((!projectileSprite) ? projectileTransform.position.XY() : projectileSprite.WorldCenter);
		m_helixInitialized = true;
		m_initialRightVector = ((!shouldRotate) ? m_currentDirection : projectileTransform.right.XY());
		m_initialUpVector = ((!shouldRotate) ? (Quaternion.Euler(0f, 0f, 90f) * m_currentDirection) : projectileTransform.up);
		m_privateLastPosition = privateLastPosition;
		m_displacement = 0f;
		m_yDisplacement = 0f;
		m_timeElapsed = 0f;
	}

	public override Vector2 GetBoneOffset(BasicBeamController.BeamBone bone, BeamController sourceBeam, bool inverted)
	{
		int num = ((!(inverted ^ ForceInvert)) ? 1 : (-1));
		float num2 = bone.PosX - helixBeamOffsetPerSecond * (Time.timeSinceLevelLoad % 600000f);
		float to = (float)num * helixAmplitude * Mathf.Sin(num2 * (float)Math.PI / helixWavelength);
		return BraveMathCollege.DegreesToVector(bone.RotationAngle + 90f, Mathf.SmoothStep(0f, to, bone.PosX));
	}
}
