using UnityEngine;

public class CollisionData : CastResult
{
	public enum CollisionType
	{
		Rigidbody,
		TileMap,
		PathEnd,
		MovementRestriction,
		Pushable
	}

	public float TimeUsed;

	public bool CollidedX;

	public bool CollidedY;

	public IntVector2 NewPixelsToMove;

	public bool Overlap;

	public CollisionType collisionType;

	public SpeculativeRigidbody MyRigidbody;

	public SpeculativeRigidbody OtherRigidbody;

	public string TileLayerName;

	public IntVector2 TilePosition;

	public bool IsPushCollision;

	public bool IsInverse;

	public static ObjectPool<CollisionData> Pool = new ObjectPool<CollisionData>(() => new CollisionData(), 10, Cleanup);

	public Vector2 PostCollisionUnitCenter
	{
		get
		{
			return MyRigidbody.UnitCenter + PhysicsEngine.PixelToUnit(NewPixelsToMove);
		}
	}

	public bool IsTriggerCollision
	{
		get
		{
			return (MyPixelCollider != null && MyPixelCollider.IsTrigger) || (OtherPixelCollider != null && OtherPixelCollider.IsTrigger);
		}
	}

	private CollisionData()
	{
	}

	public void SetAll(LinearCastResult res)
	{
		Contact = res.Contact;
		Normal = res.Normal;
		MyPixelCollider = res.MyPixelCollider;
		OtherPixelCollider = res.OtherPixelCollider;
		TimeUsed = res.TimeUsed;
		NewPixelsToMove = res.NewPixelsToMove;
		CollidedX = res.CollidedX;
		CollidedY = res.CollidedY;
		Overlap = res.Overlap;
	}

	public void SetAll(CollisionData data)
	{
		Contact = data.Contact;
		Normal = data.Normal;
		MyPixelCollider = data.MyPixelCollider;
		OtherPixelCollider = data.OtherPixelCollider;
		TimeUsed = data.TimeUsed;
		NewPixelsToMove = data.NewPixelsToMove;
		CollidedX = data.CollidedX;
		CollidedY = data.CollidedY;
		Overlap = data.Overlap;
		collisionType = data.collisionType;
		MyRigidbody = data.MyRigidbody;
		OtherRigidbody = data.OtherRigidbody;
		TileLayerName = data.TileLayerName;
		TilePosition = data.TilePosition;
		IsPushCollision = data.IsPushCollision;
		IsInverse = data.IsInverse;
	}

	public CollisionData GetInverse()
	{
		CollisionData collisionData = Pool.Allocate();
		collisionData.Contact = Contact;
		collisionData.Normal = -Normal;
		collisionData.MyPixelCollider = OtherPixelCollider;
		collisionData.OtherPixelCollider = MyPixelCollider;
		collisionData.TimeUsed = TimeUsed;
		collisionData.CollidedX = CollidedX;
		collisionData.CollidedY = CollidedY;
		collisionData.NewPixelsToMove = new IntVector2(-NewPixelsToMove.x, -NewPixelsToMove.y);
		collisionData.Overlap = Overlap;
		collisionData.collisionType = collisionType;
		collisionData.MyRigidbody = OtherRigidbody;
		collisionData.OtherRigidbody = MyRigidbody;
		collisionData.TileLayerName = TileLayerName;
		collisionData.TilePosition = TilePosition;
		collisionData.IsPushCollision = IsPushCollision;
		collisionData.IsInverse = true;
		return collisionData;
	}

	public static void Cleanup(CollisionData collisionData)
	{
		collisionData.Contact.x = 0f;
		collisionData.Contact.y = 0f;
		collisionData.Normal.x = 0f;
		collisionData.Normal.y = 0f;
		collisionData.MyPixelCollider = null;
		collisionData.OtherPixelCollider = null;
		collisionData.TimeUsed = 0f;
		collisionData.CollidedX = false;
		collisionData.CollidedY = false;
		collisionData.NewPixelsToMove.x = 0;
		collisionData.NewPixelsToMove.y = 0;
		collisionData.Overlap = false;
		collisionData.collisionType = CollisionType.Rigidbody;
		collisionData.MyRigidbody = null;
		collisionData.OtherRigidbody = null;
		collisionData.TileLayerName = null;
		collisionData.TilePosition.x = 0;
		collisionData.TilePosition.y = 0;
		collisionData.IsPushCollision = false;
		collisionData.IsInverse = false;
	}
}
