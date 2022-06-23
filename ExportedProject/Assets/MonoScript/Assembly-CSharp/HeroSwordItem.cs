using System.Collections;
using UnityEngine;

public class HeroSwordItem : PlayerItem
{
	public float Damage = 20f;

	public float MaxHealthDamage = 30f;

	public float DamageLength = 1.25f;

	public float MaxHealthDamageLength = 2.5f;

	private float SwingDuration = 0.5f;

	public VFXPool NormalSwordVFX;

	public VFXPool MaxHealthSwordVFX;

	protected override void DoEffect(PlayerController user)
	{
		Vector2 vector = user.unadjustedAimPoint.XY() - user.CenterPosition;
		float zRotation = BraveMathCollege.Atan2Degrees(vector);
		float rayDamage = Damage;
		float rayLength = DamageLength;
		if (user.healthHaver.GetCurrentHealthPercentage() >= 1f)
		{
			rayDamage = MaxHealthDamage;
			rayLength = MaxHealthDamageLength;
			MaxHealthSwordVFX.SpawnAtPosition(user.CenterPosition, zRotation, user.transform, null, null, 1f, false, null, user.sprite);
		}
		else
		{
			NormalSwordVFX.SpawnAtPosition(user.CenterPosition, zRotation, user.transform, null, null, 1f, false, null, user.sprite);
		}
		user.StartCoroutine(HandleSwing(user, vector, rayDamage, rayLength));
	}

	private IEnumerator HandleSwing(PlayerController user, Vector2 aimVec, float rayDamage, float rayLength)
	{
		float elapsed = 0f;
		while (elapsed < SwingDuration)
		{
			elapsed += BraveTime.DeltaTime;
			SpeculativeRigidbody hitRigidbody = IterativeRaycast(user.CenterPosition, aimVec, rayLength, int.MaxValue, user.specRigidbody);
			if ((bool)hitRigidbody && (bool)hitRigidbody.aiActor && hitRigidbody.aiActor.IsNormalEnemy)
			{
				hitRigidbody.aiActor.healthHaver.ApplyDamage(rayDamage, aimVec, "Hero's Sword");
			}
			yield return null;
		}
	}

	protected SpeculativeRigidbody IterativeRaycast(Vector2 rayOrigin, Vector2 rayDirection, float rayDistance, int collisionMask, SpeculativeRigidbody ignoreRigidbody)
	{
		int num = 0;
		RaycastResult result;
		while (PhysicsEngine.Instance.Raycast(rayOrigin, rayDirection, rayDistance, out result, true, true, collisionMask, CollisionLayer.Projectile, false, null, ignoreRigidbody))
		{
			num++;
			SpeculativeRigidbody speculativeRigidbody = result.SpeculativeRigidbody;
			if (num < 3 && speculativeRigidbody != null)
			{
				MinorBreakable component = speculativeRigidbody.GetComponent<MinorBreakable>();
				if (component != null)
				{
					component.Break(rayDirection.normalized * 3f);
					RaycastResult.Pool.Free(ref result);
					continue;
				}
			}
			RaycastResult.Pool.Free(ref result);
			return speculativeRigidbody;
		}
		return null;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
