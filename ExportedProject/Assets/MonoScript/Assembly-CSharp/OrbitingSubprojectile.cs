using System;
using UnityEngine;

public class OrbitingSubprojectile : Projectile
{
	public float RotationPeriod = 1f;

	public float RotationRadius = 2f;

	[NonSerialized]
	public Projectile TargetMainProjectile;

	private float m_elapsed;

	public void AssignProjectile(Projectile p)
	{
		TargetMainProjectile = p;
	}

	protected override void Move()
	{
		if (!TargetMainProjectile)
		{
			base.Move();
			return;
		}
		m_elapsed += BraveTime.DeltaTime;
		float z = Mathf.Lerp(0f, 360f, m_elapsed % RotationPeriod / RotationPeriod);
		Vector2 vector = (Quaternion.Euler(0f, 0f, z) * Vector2.right).normalized * RotationRadius;
		Vector2 vector2 = TargetMainProjectile.specRigidbody.UnitCenter + vector - (base.specRigidbody.UnitCenter - base.transform.position.XY());
		base.specRigidbody.Velocity = (vector2 - base.transform.position.XY()) / BraveTime.DeltaTime;
		base.LastVelocity = base.specRigidbody.Velocity;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
