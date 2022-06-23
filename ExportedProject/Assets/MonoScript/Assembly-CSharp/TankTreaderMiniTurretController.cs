using UnityEngine;

public class TankTreaderMiniTurretController : BodyPartController
{
	public enum AimMode
	{
		AtPlayer,
		Away
	}

	private enum State
	{
		Idle,
		Firing,
		Cooldown
	}

	public GameObject ShootPoint;

	public string BulletName;

	public float FireTime;

	public float ShotCooldown;

	public float MinCooldown;

	public float MaxCooldown;

	public float StartAngle;

	public float SweepAngle;

	private float m_fireTimeRemaining;

	private float m_timeUntilNextShot;

	private float m_cooldown;

	private static int m_arcCount;

	private static int m_lastFrame;

	private State m_state;

	public AimMode aimMode { get; set; }

	public float? OverrideAngle { get; set; }

	public override void Start()
	{
		base.Start();
		aimMode = AimMode.Away;
	}

	public void OnEnable()
	{
		m_state = State.Cooldown;
		m_cooldown = Random.Range(MinCooldown, MaxCooldown);
	}

	public override void Update()
	{
		base.Update();
		bool flag = false;
		if (aimMode == AimMode.AtPlayer)
		{
			if ((bool)m_body.TargetRigidbody)
			{
				Vector2 unitCenter = m_body.TargetRigidbody.GetUnitCenter(ColliderType.HitBox);
				float testAngle = (unitCenter - (Vector2)base.transform.position).ToAngle();
				flag = BraveMathCollege.IsAngleWithinSweepArea(testAngle, StartAngle + m_body.aiAnimator.FacingDirection + 90f, SweepAngle);
			}
		}
		else if (aimMode == AimMode.Away)
		{
			flag = true;
		}
		if (m_state == State.Idle)
		{
			if (flag)
			{
				m_state = State.Firing;
				m_fireTimeRemaining = FireTime;
				m_timeUntilNextShot = 0f;
			}
		}
		else if (m_state == State.Firing)
		{
			m_fireTimeRemaining -= BraveTime.DeltaTime;
			m_timeUntilNextShot -= BraveTime.DeltaTime;
			if (!flag || m_fireTimeRemaining <= 0f)
			{
				m_state = State.Cooldown;
				m_cooldown = Random.Range(MinCooldown, MaxCooldown);
			}
			else if (m_timeUntilNextShot <= 0f)
			{
				Fire();
				m_timeUntilNextShot = ShotCooldown;
			}
		}
		else if (m_state == State.Cooldown)
		{
			m_cooldown = Mathf.Max(0f, m_cooldown - BraveTime.DeltaTime);
			if (m_cooldown <= 0f)
			{
				m_state = State.Idle;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void Fire()
	{
		GameObject gameObject = m_body.bulletBank.CreateProjectileFromBank(ShootPoint.transform.position, base.transform.eulerAngles.z, BulletName);
		Projectile component = gameObject.GetComponent<Projectile>();
		Vector2 vector = BraveMathCollege.DegreesToVector(base.transform.eulerAngles.z, component.baseData.speed);
		Vector2 velocity = specifyActor.specRigidbody.Velocity;
		component.Direction = (vector + velocity).normalized;
		component.Speed = (vector + velocity).magnitude;
	}

	protected override bool TryGetAimAngle(out float angle)
	{
		if (OverrideAngle.HasValue)
		{
			angle = OverrideAngle.Value;
			return true;
		}
		if (aimMode == AimMode.Away)
		{
			angle = StartAngle + 0.5f * SweepAngle;
			angle += m_body.aiAnimator.FacingDirection + 90f;
			return true;
		}
		return base.TryGetAimAngle(out angle);
	}
}
