using UnityEngine;

public class PathBlocker : BraveBehaviour
{
	public bool BlocksGoopsToo;

	public static void BlockRigidbody(SpeculativeRigidbody rigidbody, bool blockGoopsToo)
	{
		foreach (PixelCollider pixelCollider in rigidbody.PixelColliders)
		{
			if (pixelCollider.IsTrigger || (pixelCollider.CollisionLayer != CollisionLayer.LowObstacle && pixelCollider.CollisionLayer != CollisionLayer.HighObstacle && pixelCollider.CollisionLayer != CollisionLayer.EnemyBlocker))
			{
				continue;
			}
			if (pixelCollider.ColliderGenerationMode == PixelCollider.PixelColliderGeneration.Line)
			{
				Vector2 vector = rigidbody.transform.position.XY() + PhysicsEngine.PixelToUnit(new IntVector2(pixelCollider.ManualLeftX, pixelCollider.ManualLeftY));
				Vector2 vector2 = rigidbody.transform.position.XY() + PhysicsEngine.PixelToUnit(new IntVector2(pixelCollider.ManualRightX, pixelCollider.ManualRightY));
				float num = Vector2.Distance(vector, vector2);
				Vector2 normalized = (vector2 - vector).normalized;
				for (float num2 = 0f; num2 <= num; num2 += 0.1f)
				{
					IntVector2 key = (vector + normalized * num2).ToIntVector2(VectorConversions.Floor);
					GameManager.Instance.Dungeon.data[key].isOccupied = true;
					if (blockGoopsToo)
					{
						GameManager.Instance.Dungeon.data[key].forceDisallowGoop = true;
					}
				}
				GameManager.Instance.Dungeon.data[vector2.ToIntVector2(VectorConversions.Floor)].isOccupied = true;
				if (blockGoopsToo)
				{
					GameManager.Instance.Dungeon.data[vector2.ToIntVector2(VectorConversions.Floor)].forceDisallowGoop = true;
				}
				continue;
			}
			IntVector2 intVector = pixelCollider.UnitBottomLeft.ToIntVector2(VectorConversions.Floor);
			IntVector2 intVector2 = pixelCollider.UnitTopRight.ToIntVector2(VectorConversions.Ceil);
			for (int i = intVector.x; i < intVector2.x; i++)
			{
				for (int j = intVector.y; j < intVector2.y; j++)
				{
					if (GameManager.Instance.Dungeon.data.CheckInBoundsAndValid(new IntVector2(i, j)))
					{
						GameManager.Instance.Dungeon.data[i, j].isOccupied = true;
						if (blockGoopsToo)
						{
							GameManager.Instance.Dungeon.data[i, j].forceDisallowGoop = true;
						}
					}
				}
			}
		}
	}

	public void Start()
	{
		if ((bool)base.specRigidbody)
		{
			base.specRigidbody.Initialize();
			BlockRigidbody(base.specRigidbody, BlocksGoopsToo);
		}
	}
}
