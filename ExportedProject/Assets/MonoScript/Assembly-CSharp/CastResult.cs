using UnityEngine;

public abstract class CastResult
{
	public Vector2 Contact;

	public Vector2 Normal;

	public PixelCollider MyPixelCollider;

	public PixelCollider OtherPixelCollider;
}
