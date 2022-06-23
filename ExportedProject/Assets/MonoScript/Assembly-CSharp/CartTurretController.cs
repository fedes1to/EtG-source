using System;
using Dungeonator;
using UnityEngine;

public class CartTurretController : BraveBehaviour
{
	public float AwakeTimer = 3f;

	public float TimeBetweenShots = 0.2f;

	public float ReacquisitonTimer = 1f;

	public float TrackingPercentage;

	public Transform BarrelTransform;

	private bool m_active;

	private RoomHandler m_parentRoom;

	private PlayerController m_targetPlayer;

	private float m_awakeTimer;

	private float m_acquisitionTimer;

	private float m_fireTimer;

	public bool Inactive
	{
		get
		{
			return !m_active;
		}
	}

	private void Start()
	{
		m_parentRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY());
		m_parentRoom.Entered += Activate;
	}

	private void Activate(PlayerController p)
	{
		if (!m_active && m_parentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear))
		{
			m_active = true;
			m_awakeTimer = AwakeTimer;
			RoomHandler parentRoom = m_parentRoom;
			parentRoom.OnEnemiesCleared = (Action)Delegate.Combine(parentRoom.OnEnemiesCleared, new Action(HandleEnemiesCleared));
		}
	}

	private void HandleEnemiesCleared()
	{
		m_active = false;
		RoomHandler parentRoom = m_parentRoom;
		parentRoom.OnEnemiesCleared = (Action)Delegate.Remove(parentRoom.OnEnemiesCleared, new Action(HandleEnemiesCleared));
	}

	private void Update()
	{
		if (!m_active)
		{
			return;
		}
		m_awakeTimer -= BraveTime.DeltaTime;
		if (!(m_awakeTimer > 0f))
		{
			m_acquisitionTimer -= BraveTime.DeltaTime;
			if (m_targetPlayer != null && m_targetPlayer.healthHaver.IsDead)
			{
				m_targetPlayer = null;
			}
			if (m_targetPlayer == null || m_acquisitionTimer <= 0f)
			{
				AcquireTarget();
			}
			if (m_targetPlayer != null)
			{
				FaceTarget();
				Fire();
			}
		}
	}

	private void Fire()
	{
		m_fireTimer -= BraveTime.DeltaTime;
		if (m_fireTimer <= 0f)
		{
			float num = (m_targetPlayer.CenterPosition - base.transform.position.XY()).ToAngle();
			if (!float.IsNaN(num) && !float.IsInfinity(num))
			{
				FireBullet(BarrelTransform, num, "default");
				m_fireTimer = TimeBetweenShots;
			}
		}
	}

	private void FaceTarget()
	{
		Vector2 v = m_targetPlayer.CenterPosition - base.transform.position.XY();
		float num = BraveMathCollege.Atan2Degrees(v);
		if (!float.IsNaN(num) && !float.IsInfinity(num))
		{
			base.transform.rotation = Quaternion.Euler(0f, 0f, num);
		}
	}

	private void AcquireTarget()
	{
		m_targetPlayer = GameManager.Instance.GetActivePlayerClosestToPoint(base.transform.position.XY());
		m_acquisitionTimer = ReacquisitonTimer;
	}

	private void FireBullet(Transform shootPoint, float dir, string bulletType)
	{
		GameObject gameObject = base.bulletBank.CreateProjectileFromBank(shootPoint.position, dir, bulletType);
		Projectile component = gameObject.GetComponent<Projectile>();
		component.Shooter = base.specRigidbody;
		component.specRigidbody.RegisterSpecificCollisionException(base.specRigidbody);
		component.OwnerName = StringTableManager.GetEnemiesString("#TRAP");
	}

	private Vector2 FindPredictedTargetPosition(string bulletName)
	{
		AIBulletBank.Entry bulletEntry = GetBulletEntry(bulletName);
		float num = float.MaxValue;
		num = ((!bulletEntry.OverrideProjectile) ? bulletEntry.BulletObject.GetComponent<Projectile>().baseData.speed : bulletEntry.ProjectileData.speed);
		if (num < 0f)
		{
			num = float.MaxValue;
		}
		Vector2 unitCenter = m_targetPlayer.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		Vector2 averageVelocity = m_targetPlayer.AverageVelocity;
		Vector2 unitCenter2 = m_targetPlayer.specRigidbody.UnitCenter;
		return BraveMathCollege.GetPredictedPosition(unitCenter, averageVelocity, unitCenter2, num);
	}

	public AIBulletBank.Entry GetBulletEntry(string bulletName)
	{
		if (string.IsNullOrEmpty(bulletName))
		{
			return null;
		}
		AIBulletBank.Entry entry = base.bulletBank.Bullets.Find((AIBulletBank.Entry b) => b.Name == bulletName);
		if (entry == null)
		{
			Debug.LogError(string.Format("Unknown bullet type {0} on {1}", base.transform.name, bulletName), base.gameObject);
			return null;
		}
		return entry;
	}
}
