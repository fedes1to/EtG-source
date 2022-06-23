using System;
using System.Collections;
using UnityEngine;

public class AIBeamShooter : BraveBehaviour
{
	public string shootAnim;

	public AIAnimator specifyAnimator;

	[Header("Beam Data")]
	public Transform beamTransform;

	public VFXPool chargeVfx;

	public Projectile beamProjectile;

	[HideInInspectorIf("beamProjectile", false)]
	public ProjectileModule beamModule;

	public bool TurnDuringDissipation = true;

	public bool PreventBeamContinuation;

	[Header("Depth")]
	public float heightOffset = 1.9f;

	public float northAngleTolerance = 90f;

	public float northRampHeight;

	public float otherRampHeight = 5f;

	[Header("Beam Firing Point")]
	public Vector2 firingEllipseCenter;

	public float firingEllipseA;

	public float firingEllipseB;

	public float eyeballFudgeAngle;

	private bool m_firingLaser;

	private float m_laserAngle;

	private BasicBeamController m_laserBeam;

	private BodyPartController m_bodyPart;

	public float LaserAngle
	{
		get
		{
			return m_laserAngle;
		}
		set
		{
			m_laserAngle = value;
			if (m_firingLaser)
			{
				CurrentAiAnimator.FacingDirection = value;
			}
		}
	}

	public bool IsFiringLaser
	{
		get
		{
			return m_firingLaser;
		}
	}

	public Vector2 LaserFiringCenter
	{
		get
		{
			return beamTransform.position.XY() + firingEllipseCenter;
		}
	}

	public AIAnimator CurrentAiAnimator
	{
		get
		{
			return (!specifyAnimator) ? base.aiAnimator : specifyAnimator;
		}
	}

	public float MaxBeamLength { get; set; }

	public BeamController LaserBeam
	{
		get
		{
			return m_laserBeam;
		}
	}

	public bool IgnoreAiActorPlayerChecks { get; set; }

	public void Start()
	{
		base.healthHaver.OnDamaged += OnDamaged;
		if ((bool)specifyAnimator)
		{
			m_bodyPart = specifyAnimator.GetComponent<BodyPartController>();
		}
	}

	public void Update()
	{
	}

	public void LateUpdate()
	{
		if ((bool)m_laserBeam && MaxBeamLength > 0f)
		{
			m_laserBeam.projectile.baseData.range = MaxBeamLength;
			m_laserBeam.ShowImpactOnMaxDistanceEnd = true;
		}
		if (m_firingLaser && (bool)m_laserBeam)
		{
			m_laserBeam.LateUpdatePosition(GetTrueLaserOrigin());
		}
		else if ((bool)m_laserBeam && m_laserBeam.State == BasicBeamController.BeamState.Dissipating)
		{
			m_laserBeam.LateUpdatePosition(GetTrueLaserOrigin());
		}
		else if (m_firingLaser && !m_laserBeam)
		{
			StopFiringLaser();
		}
	}

	protected override void OnDestroy()
	{
		if ((bool)m_laserBeam)
		{
			m_laserBeam.CeaseAttack();
		}
		base.OnDestroy();
	}

	public void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		if (resultValue <= 0f)
		{
			if (m_firingLaser)
			{
				chargeVfx.DestroyAll();
				StopFiringLaser();
			}
			if ((bool)m_laserBeam)
			{
				m_laserBeam.DestroyBeam();
				m_laserBeam = null;
			}
		}
	}

	public void StartFiringLaser(float laserAngle)
	{
		m_firingLaser = true;
		LaserAngle = laserAngle;
		if ((bool)m_bodyPart)
		{
			m_bodyPart.OverrideFacingDirection = true;
		}
		if (!string.IsNullOrEmpty(shootAnim))
		{
			CurrentAiAnimator.LockFacingDirection = true;
			CurrentAiAnimator.PlayUntilCancelled(shootAnim, true);
		}
		chargeVfx.DestroyAll();
		StartCoroutine(FireBeam((!beamProjectile) ? beamModule.GetCurrentProjectile() : beamProjectile));
	}

	public void StopFiringLaser()
	{
		m_firingLaser = false;
		if ((bool)m_bodyPart)
		{
			m_bodyPart.OverrideFacingDirection = false;
		}
		if (!string.IsNullOrEmpty(shootAnim))
		{
			CurrentAiAnimator.LockFacingDirection = false;
			CurrentAiAnimator.EndAnimationIf(shootAnim);
		}
	}

	protected IEnumerator FireBeam(Projectile projectile)
	{
		GameObject beamObject = UnityEngine.Object.Instantiate(projectile.gameObject);
		m_laserBeam = beamObject.GetComponent<BasicBeamController>();
		m_laserBeam.Owner = base.aiActor;
		m_laserBeam.HitsPlayers = projectile.collidesWithPlayer || (!IgnoreAiActorPlayerChecks && (bool)base.aiActor && base.aiActor.CanTargetPlayers);
		m_laserBeam.HitsEnemies = projectile.collidesWithEnemies || ((bool)base.aiActor && base.aiActor.CanTargetEnemies);
		bool facingNorth2 = BraveMathCollege.AbsAngleBetween(LaserAngle, 90f) < northAngleTolerance;
		m_laserBeam.HeightOffset = heightOffset;
		m_laserBeam.RampHeightOffset = ((!facingNorth2) ? otherRampHeight : northRampHeight);
		m_laserBeam.ContinueBeamArtToWall = !PreventBeamContinuation;
		bool firstFrame = true;
		while (m_laserBeam != null && m_firingLaser)
		{
			float clampedAngle = BraveMathCollege.ClampAngle360(LaserAngle);
			Vector2 dirVec = new Vector3(Mathf.Cos(clampedAngle * ((float)Math.PI / 180f)), Mathf.Sin(clampedAngle * ((float)Math.PI / 180f))) * 10f;
			m_laserBeam.Origin = GetTrueLaserOrigin();
			m_laserBeam.Direction = dirVec;
			if (firstFrame)
			{
				yield return null;
				firstFrame = false;
				continue;
			}
			facingNorth2 = BraveMathCollege.AbsAngleBetween(LaserAngle, 90f) < northAngleTolerance;
			m_laserBeam.RampHeightOffset = ((!facingNorth2) ? otherRampHeight : northRampHeight);
			yield return null;
			while (Time.timeScale == 0f)
			{
				yield return null;
			}
		}
		if (!m_firingLaser && m_laserBeam != null)
		{
			m_laserBeam.CeaseAttack();
		}
		if (TurnDuringDissipation && (bool)m_laserBeam)
		{
			m_laserBeam.SelfUpdate = false;
			while ((bool)m_laserBeam)
			{
				m_laserBeam.Origin = GetTrueLaserOrigin();
				yield return null;
			}
		}
		m_laserBeam = null;
	}

	private Vector3 GetTrueLaserOrigin()
	{
		Vector2 vector = LaserFiringCenter;
		if (firingEllipseA != 0f && firingEllipseB != 0f)
		{
			float num = Mathf.Lerp(eyeballFudgeAngle, 0f, BraveMathCollege.AbsAngleBetween(90f, Mathf.Abs(BraveMathCollege.ClampAngle180(LaserAngle))) / 90f);
			vector = BraveMathCollege.GetEllipsePoint(vector, firingEllipseA, firingEllipseB, LaserAngle + num);
		}
		return vector;
	}
}
