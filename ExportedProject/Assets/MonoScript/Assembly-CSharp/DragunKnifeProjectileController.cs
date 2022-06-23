using System;
using UnityEngine;

public class DragunKnifeProjectileController : BraveBehaviour
{
	[EnemyIdentifier]
	public string knifeGuid;

	public void Start()
	{
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnTileCollision = (SpeculativeRigidbody.OnTileCollisionDelegate)Delegate.Combine(speculativeRigidbody.OnTileCollision, new SpeculativeRigidbody.OnTileCollisionDelegate(OnTileCollision));
	}

	private void OnTileCollision(CollisionData tileCollision)
	{
		if ((bool)base.projectile.Owner && base.projectile.Owner is AIActor)
		{
			AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(knifeGuid);
			Vector2 contact = tileCollision.Contact;
			if (tileCollision.Normal.x < 0f)
			{
				contact.x -= PhysicsEngine.PixelToUnit(orLoadByGuid.specRigidbody.PrimaryPixelCollider.ManualWidth);
			}
			AIActor aIActor = AIActor.Spawn(orLoadByGuid, contact.ToIntVector2() + new IntVector2(0, -1), (base.projectile.Owner as AIActor).ParentRoom);
			aIActor.aiAnimator.LockFacingDirection = true;
			aIActor.aiAnimator.FacingDirection = ((tileCollision.Normal.x < 0f) ? 180 : 0);
			aIActor.aiAnimator.Update();
			if (tileCollision.Normal.x < 0f)
			{
				PixelCollider primaryPixelCollider = aIActor.specRigidbody.PrimaryPixelCollider;
				int num = primaryPixelCollider.ManualWidth / 2;
				primaryPixelCollider.ManualOffsetX += num;
				primaryPixelCollider.ManualWidth -= num;
				aIActor.specRigidbody.ForceRegenerate();
			}
			else
			{
				PixelCollider primaryPixelCollider2 = aIActor.specRigidbody.PrimaryPixelCollider;
				int num2 = primaryPixelCollider2.ManualWidth / 2;
				primaryPixelCollider2.ManualWidth -= num2;
				aIActor.specRigidbody.ForceRegenerate();
			}
		}
	}
}
