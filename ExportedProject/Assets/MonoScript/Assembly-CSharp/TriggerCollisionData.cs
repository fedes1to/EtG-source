public class TriggerCollisionData
{
	public PixelCollider PixelCollider;

	public SpeculativeRigidbody SpecRigidbody;

	public bool FirstFrame = true;

	public bool ContinuedCollision;

	public bool Notified;

	public TriggerCollisionData(SpeculativeRigidbody specRigidbody, PixelCollider pixelCollider)
	{
		SpecRigidbody = specRigidbody;
		PixelCollider = pixelCollider;
	}

	public void Reset()
	{
		FirstFrame = false;
		ContinuedCollision = false;
		Notified = false;
	}
}
