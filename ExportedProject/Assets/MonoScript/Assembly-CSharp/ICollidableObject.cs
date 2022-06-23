using System.Collections.Generic;

public interface ICollidableObject
{
	PixelCollider PrimaryPixelCollider { get; }

	bool CanCollideWith(SpeculativeRigidbody rigidbody);

	List<PixelCollider> GetPixelColliders();
}
