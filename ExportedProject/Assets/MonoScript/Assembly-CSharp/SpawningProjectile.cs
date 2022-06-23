using Dungeonator;
using UnityEngine;

public class SpawningProjectile : Projectile
{
	public float startingHeight = 1f;

	public float gravity = -10f;

	[EnemyIdentifier]
	public string enemyGuid;

	private Vector3 m_current3DVelocity;

	private float m_kinematicTimer;

	public override void Start()
	{
		base.Start();
		m_current3DVelocity = (m_currentDirection * m_currentSpeed).ToVector3ZUp();
	}

	protected override void Move()
	{
		m_kinematicTimer += BraveTime.DeltaTime;
		m_current3DVelocity.x = m_currentDirection.x;
		m_current3DVelocity.y = m_currentDirection.y;
		m_current3DVelocity.z = gravity * m_kinematicTimer;
		float num = startingHeight + 0.5f * gravity * m_kinematicTimer * m_kinematicTimer;
		if (num < 0f)
		{
			Vector2 unitCenter = base.specRigidbody.UnitCenter;
			IntVector2 intVector = unitCenter.ToIntVector2(VectorConversions.Floor);
			RoomHandler roomFromPosition = GameManager.Instance.Dungeon.GetRoomFromPosition(intVector);
			AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(enemyGuid);
			AIActor aIActor = AIActor.Spawn(orLoadByGuid, intVector, roomFromPosition, true);
			if ((bool)aIActor && (bool)aIActor.aiAnimator)
			{
				aIActor.aiAnimator.PlayDefaultSpawnState();
				hitEffects.HandleEnemyImpact(unitCenter, 0f, null, Vector2.zero, Vector2.zero, true);
			}
			if (IsBlackBullet && (bool)aIActor)
			{
				aIActor.ForceBlackPhantom = true;
			}
			Object.Destroy(base.gameObject);
		}
		else
		{
			m_currentDirection = m_current3DVelocity.XY();
			Vector2 vector = m_current3DVelocity.XY().normalized * m_currentSpeed;
			base.specRigidbody.Velocity = new Vector2(vector.x, vector.y + m_current3DVelocity.z);
			base.LastVelocity = m_current3DVelocity.XY();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
}
