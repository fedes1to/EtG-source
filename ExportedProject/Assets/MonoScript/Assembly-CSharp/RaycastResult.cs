public class RaycastResult : CastResult
{
	public IntVector2 HitPixel;

	public IntVector2 LastRayPixel;

	public float Distance;

	public SpeculativeRigidbody SpeculativeRigidbody;

	public static ObjectPool<RaycastResult> Pool = new ObjectPool<RaycastResult>(() => new RaycastResult(), 10, Cleanup);

	private RaycastResult()
	{
	}

	public static void Cleanup(RaycastResult raycastResult)
	{
		raycastResult.Contact.x = 0f;
		raycastResult.Contact.y = 0f;
		raycastResult.Normal.x = 0f;
		raycastResult.Normal.y = 0f;
		raycastResult.MyPixelCollider = null;
		raycastResult.OtherPixelCollider = null;
		raycastResult.HitPixel.x = 0;
		raycastResult.HitPixel.y = 0;
		raycastResult.LastRayPixel.x = 0;
		raycastResult.LastRayPixel.y = 0;
		raycastResult.Distance = 0f;
		raycastResult.SpeculativeRigidbody = null;
	}
}
