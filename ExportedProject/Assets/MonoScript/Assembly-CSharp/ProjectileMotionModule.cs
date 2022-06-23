using UnityEngine;

public abstract class ProjectileMotionModule
{
	public abstract void UpdateDataOnBounce(float angleDiff);

	public abstract void Move(Projectile source, Transform projectileTransform, tk2dBaseSprite projectileSprite, SpeculativeRigidbody specRigidbody, ref float m_timeElapsed, ref Vector2 m_currentDirection, bool Inverted, bool shouldRotate);

	public virtual void AdjustRightVector(float angleDiff)
	{
	}

	public virtual void SentInDirection(ProjectileData baseData, Transform projectileTransform, tk2dBaseSprite projectileSprite, SpeculativeRigidbody specRigidbody, ref float m_timeElapsed, ref Vector2 m_currentDirection, bool shouldRotate, Vector2 dirVec, bool resetDistance, bool updateRotation)
	{
	}
}
