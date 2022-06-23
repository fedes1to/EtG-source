using System;
using System.Collections;
using UnityEngine;

public class HitEffectHandler : BraveBehaviour
{
	[Serializable]
	public class AdditionalHitEffect
	{
		public VFXPool hitEffect;

		public float chance = 1f;

		public Transform transform;

		public bool flipNormals;

		public float radius;

		public float delay;

		public float angleVariance;

		public bool doForce;

		[ShowInInspectorIf("doForce", true)]
		public float minForce;

		[ShowInInspectorIf("doForce", true)]
		public float maxForce;

		[ShowInInspectorIf("doForce", true)]
		public float additionalVerticalForce;

		public bool spawnOnGround;

		[ShowInInspectorIf("spawnOnGround", true)]
		public float minDistance;

		[ShowInInspectorIf("spawnOnGround", true)]
		public float maxDistance;

		public bool specificPixelCollider;

		[ShowInInspectorIf("specificPixelCollider", false)]
		public int pixelColliderIndex;

		[NonSerialized]
		public float delayTimer;
	}

	public DungeonMaterial overrideMaterialDefinition;

	public VFXComplex overrideHitEffect;

	public VFXPool overrideHitEffectPool;

	public AdditionalHitEffect[] additionalHitEffects;

	public bool SuppressAllHitEffects;

	private bool m_isTrackingDelays;

	public bool SuppressAdditionalHitEffects { get; set; }

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public void HandleAdditionalHitEffects(Vector2 projVelocity, PixelCollider hitPixelCollider)
	{
		if (SuppressAdditionalHitEffects)
		{
			return;
		}
		for (int i = 0; i < additionalHitEffects.Length; i++)
		{
			AdditionalHitEffect additionalHitEffect = additionalHitEffects[i];
			if (additionalHitEffect.delayTimer > 0f || (additionalHitEffect.chance < 1f && UnityEngine.Random.value > additionalHitEffect.chance))
			{
				continue;
			}
			if (additionalHitEffect.specificPixelCollider)
			{
				int num = base.specRigidbody.PixelColliders.IndexOf(hitPixelCollider);
				if (additionalHitEffect.pixelColliderIndex != num)
				{
					continue;
				}
			}
			float num2 = ((!additionalHitEffect.flipNormals) ? projVelocity : (-projVelocity)).ToAngle();
			if (additionalHitEffect.spawnOnGround)
			{
				Vector2 vector = base.specRigidbody.UnitCenter + BraveMathCollege.DegreesToVector(num2 + UnityEngine.Random.Range(0f - additionalHitEffect.angleVariance, additionalHitEffect.angleVariance), UnityEngine.Random.Range(additionalHitEffect.minDistance, additionalHitEffect.maxDistance));
				additionalHitEffect.hitEffect.SpawnAtPosition(vector, num2);
			}
			else
			{
				Vector2 vector2 = ((!additionalHitEffect.transform) ? base.specRigidbody.GetUnitCenter(ColliderType.HitBox) : additionalHitEffect.transform.position.XY());
				vector2 = vector2 - base.transform.position.XY() + BraveMathCollege.DegreesToVector(num2, additionalHitEffect.radius);
				if (additionalHitEffect.doForce)
				{
					Vector2 vector3 = BraveMathCollege.DegreesToVector(num2 + UnityEngine.Random.Range(0f - additionalHitEffect.angleVariance, additionalHitEffect.angleVariance), UnityEngine.Random.Range(additionalHitEffect.minForce, additionalHitEffect.maxForce));
					vector3 += new Vector2(0f, additionalHitEffect.additionalVerticalForce);
					Vector2 value = (Quaternion.Euler(0f, 0f, 90f) * vector3).normalized;
					additionalHitEffect.hitEffect.SpawnAtLocalPosition(vector2, num2, base.transform, value, vector3);
				}
				else
				{
					additionalHitEffect.hitEffect.SpawnAtLocalPosition(vector2, num2, base.transform);
				}
			}
			if (additionalHitEffect.delay > 0f)
			{
				additionalHitEffect.delayTimer = additionalHitEffect.delay;
				if (!m_isTrackingDelays)
				{
					StartCoroutine(TrackDelaysCR());
				}
			}
		}
	}

	private IEnumerator TrackDelaysCR()
	{
		m_isTrackingDelays = true;
		while (true)
		{
			for (int i = 0; i < additionalHitEffects.Length; i++)
			{
				additionalHitEffects[i].delayTimer = Mathf.Max(0f, additionalHitEffects[i].delayTimer - BraveTime.DeltaTime);
			}
			yield return null;
		}
	}
}
