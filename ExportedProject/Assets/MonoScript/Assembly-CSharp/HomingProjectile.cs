using Dungeonator;
using UnityEngine;

public class HomingProjectile : Projectile
{
	public float detectionRange = 5f;

	public float trackingSpeed = 5f;

	public bool stopTrackingIfLeaveRadius;

	private AIActor nearestEnemy;

	protected override void Move()
	{
		if (stopTrackingIfLeaveRadius)
		{
			nearestEnemy = null;
		}
		if (nearestEnemy == null)
		{
			RoomHandler absoluteRoomFromPosition = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY());
			nearestEnemy = BraveUtility.GetClosestToPosition(absoluteRoomFromPosition.GetActiveEnemies(RoomHandler.ActiveEnemyType.All), base.transform.position.XY());
		}
		if (nearestEnemy != null)
		{
			Vector3 vector = nearestEnemy.transform.position - base.transform.position;
			float f = (Mathf.Atan2(vector.y, vector.x) - Mathf.Atan2(base.specRigidbody.Velocity.y, base.specRigidbody.Velocity.x)) * 57.29578f;
			float zAngle = Mathf.Min(Mathf.Abs(f), trackingSpeed * BraveTime.DeltaTime) * Mathf.Sign(f);
			base.transform.Rotate(0f, 0f, zAngle);
		}
		base.specRigidbody.Velocity = base.transform.right * baseData.speed;
		base.LastVelocity = base.specRigidbody.Velocity;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
