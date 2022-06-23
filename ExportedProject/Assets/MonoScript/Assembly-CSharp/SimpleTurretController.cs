using Dungeonator;
using UnityEngine;

public class SimpleTurretController : DungeonPlaceableBehaviour
{
	public bool ControlledByPlaymaker = true;

	public float AwakeTimer = 3f;

	public float TimeBetweenShots = 0.2f;

	public Vector2 ShootDirection;

	public Transform BarrelTransform;

	private bool m_active;

	private RoomHandler m_parentRoom;

	private AIBulletBank m_bank;

	private float m_awakeTimer;

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
		if (!base.specRigidbody)
		{
			base.specRigidbody = GetComponentInChildren<SpeculativeRigidbody>();
		}
		m_bank = base.bulletBank;
		if (!base.bulletBank)
		{
			m_bank = GetComponentInChildren<AIBulletBank>();
		}
		m_parentRoom = GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY());
		if (!ControlledByPlaymaker)
		{
			m_parentRoom.Entered += Activate;
		}
	}

	public void DeactivateManual()
	{
		m_active = false;
	}

	public void ActivateManual()
	{
		if (!m_active)
		{
			m_active = true;
			m_awakeTimer = AwakeTimer;
		}
	}

	private void Activate(PlayerController p)
	{
		ActivateManual();
	}

	private void Update()
	{
		if (!m_active)
		{
			return;
		}
		if (!ControlledByPlaymaker && !GameManager.Instance.IsAnyPlayerInRoom(m_parentRoom))
		{
			m_active = false;
			return;
		}
		m_awakeTimer -= BraveTime.DeltaTime;
		if (!(m_awakeTimer > 0f))
		{
			Fire();
		}
	}

	private void Fire()
	{
		m_fireTimer -= BraveTime.DeltaTime;
		if (m_fireTimer <= 0f)
		{
			FireBullet(BarrelTransform, ShootDirection, "default");
			m_fireTimer = TimeBetweenShots;
		}
	}

	private void FireBullet(Transform shootPoint, Vector2 dirVec, string bulletType)
	{
		GameObject gameObject = m_bank.CreateProjectileFromBank(shootPoint.position, BraveMathCollege.Atan2Degrees(dirVec.normalized), bulletType);
		Projectile component = gameObject.GetComponent<Projectile>();
		component.Shooter = base.specRigidbody;
		component.specRigidbody.RegisterSpecificCollisionException(base.specRigidbody);
	}

	public AIBulletBank.Entry GetBulletEntry(string bulletName)
	{
		if (string.IsNullOrEmpty(bulletName))
		{
			return null;
		}
		AIBulletBank.Entry entry = m_bank.Bullets.Find((AIBulletBank.Entry b) => b.Name == bulletName);
		if (entry == null)
		{
			Debug.LogError(string.Format("Unknown bullet type {0} on {1}", base.transform.name, bulletName), base.gameObject);
			return null;
		}
		return entry;
	}
}
