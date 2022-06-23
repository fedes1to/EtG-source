using System;
using UnityEngine;

[Serializable]
public class ProjectileData
{
	public static float FixedFallbackDamageToEnemies = 10f;

	public static float FixedEnemyDamageToBreakables = 8f;

	public float damage = 2.5f;

	public float speed = 10f;

	public float range = 10f;

	public float force = 10f;

	public float damping;

	public bool UsesCustomAccelerationCurve;

	[ShowInInspectorIf("UsesCustomAccelerationCurve", true)]
	public AnimationCurve AccelerationCurve;

	[ShowInInspectorIf("UsesCustomAccelerationCurve", true)]
	public float CustomAccelerationCurveDuration = 1f;

	[NonSerialized]
	public float IgnoreAccelCurveTime;

	public BulletScriptSelector onDestroyBulletScript;

	public ProjectileData()
	{
	}

	public ProjectileData(ProjectileData other)
	{
		SetAll(other);
	}

	public void SetAll(ProjectileData other)
	{
		damage = other.damage;
		speed = other.speed;
		range = other.range;
		force = other.force;
		damping = other.damping;
		UsesCustomAccelerationCurve = other.UsesCustomAccelerationCurve;
		AccelerationCurve = other.AccelerationCurve;
		CustomAccelerationCurveDuration = other.CustomAccelerationCurveDuration;
		onDestroyBulletScript = other.onDestroyBulletScript.Clone();
	}
}
