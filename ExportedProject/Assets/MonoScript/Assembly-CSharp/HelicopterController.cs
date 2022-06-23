public class HelicopterController : BraveBehaviour
{
	public void Start()
	{
		base.specRigidbody.AddCollisionLayerIgnoreOverride(CollisionMask.LayerToMask(CollisionLayer.PlayerCollider, CollisionLayer.PlayerHitBox));
	}
}
