public class LinearCastResult : CastResult
{
	public float TimeUsed;

	public bool CollidedX;

	public bool CollidedY;

	public IntVector2 NewPixelsToMove;

	public bool Overlap;

	public static ObjectPool<LinearCastResult> Pool = new ObjectPool<LinearCastResult>(() => new LinearCastResult(), 10, Cleanup);

	private LinearCastResult()
	{
	}

	public static void Cleanup(LinearCastResult linearCastResults)
	{
		linearCastResults.Contact.x = 0f;
		linearCastResults.Contact.y = 0f;
		linearCastResults.Normal.x = 0f;
		linearCastResults.Normal.y = 0f;
		linearCastResults.MyPixelCollider = null;
		linearCastResults.OtherPixelCollider = null;
		linearCastResults.TimeUsed = 0f;
		linearCastResults.CollidedX = false;
		linearCastResults.CollidedY = false;
		linearCastResults.NewPixelsToMove.x = 0;
		linearCastResults.NewPixelsToMove.y = 0;
		linearCastResults.Overlap = false;
	}
}
