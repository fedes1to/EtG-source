using System;
using Dungeonator;

public class ExitController : DungeonPlaceableBehaviour, IPlaceConfigurable
{
	public int wallClearanceXStart = 1;

	public int wallClearanceYStart = 1;

	public int wallClearanceWidth = 4;

	public int wallClearanceHeight = 4;

	private void Start()
	{
		SpeculativeRigidbody componentInChildren = GetComponentInChildren<SpeculativeRigidbody>();
		if ((bool)componentInChildren)
		{
			componentInChildren.OnRigidbodyCollision = (SpeculativeRigidbody.OnRigidbodyCollisionDelegate)Delegate.Combine(componentInChildren.OnRigidbodyCollision, new SpeculativeRigidbody.OnRigidbodyCollisionDelegate(OnRigidbodyCollision));
		}
	}

	public void ConfigureOnPlacement(RoomHandler room)
	{
		IntVector2 intVector = base.transform.position.IntXY(VectorConversions.Floor);
		for (int i = wallClearanceXStart; i < wallClearanceWidth + wallClearanceXStart; i++)
		{
			for (int j = wallClearanceYStart; j < wallClearanceHeight + wallClearanceYStart; j++)
			{
				IntVector2 key = intVector + new IntVector2(i, j);
				CellData cellData = GameManager.Instance.Dungeon.data[key];
				cellData.cellVisualData.containsObjectSpaceStamp = true;
				cellData.cellVisualData.containsWallSpaceStamp = true;
				cellData.cellVisualData.shouldIgnoreWallDrawing = true;
			}
		}
	}

	protected virtual void OnRigidbodyCollision(CollisionData rigidbodyCollision)
	{
		PlayerController component = rigidbodyCollision.OtherRigidbody.GetComponent<PlayerController>();
		if (component != null)
		{
			Pixelator.Instance.FadeToBlack(0.5f);
			GameManager.Instance.DelayedLoadNextLevel(0.5f);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
