using System;
using System.Collections;
using FullInspector;
using UnityEngine;

public class BossFinalRogueLaserGun : BossFinalRogueGunController
{
	[InspectorHeader("Beam Stuff")]
	public Transform beamTransform;

	public Projectile beamProjectile;

	public float fireTime = 6f;

	public bool doScreenShake;

	[InspectorShowIf("doScreenShake")]
	public ScreenShakeSettings screenShake;

	[InspectorHeader("Gun Motion")]
	public float sweepAngle;

	public float sweepAwayTime;

	public float sweepBackTime;

	public AdditionalBraveLight LightToTrigger;

	private bool m_firingLaser;

	private float m_laserAngle;

	private float m_fireTimer;

	private BasicBeamController m_laserBeam;

	public float LaserAngle
	{
		get
		{
			return m_laserAngle;
		}
		set
		{
			m_laserAngle = value;
			base.transform.rotation = Quaternion.Euler(0f, 0f, m_laserAngle + 90f);
		}
	}

	public override bool IsFinished
	{
		get
		{
			return !m_firingLaser && !m_laserBeam;
		}
	}

	public override void Start()
	{
		base.Start();
		ship.healthHaver.OnPreDeath += OnPreDeath;
	}

	public override void Update()
	{
		base.Update();
		if (m_fireTimer > 0f)
		{
			m_fireTimer -= BraveTime.DeltaTime;
			if (m_fireTimer <= 0f)
			{
				m_firingLaser = false;
			}
		}
	}

	public void LateUpdate()
	{
		if (m_firingLaser && (bool)m_laserBeam)
		{
			m_laserBeam.LateUpdatePosition(beamTransform.position);
		}
		else if ((bool)m_laserBeam && m_laserBeam.State == BasicBeamController.BeamState.Dissipating)
		{
			m_laserBeam.LateUpdatePosition(beamTransform.position);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	public override void Fire()
	{
		m_firingLaser = true;
		m_fireTimer = fireTime;
		LaserAngle = -90f;
		if ((bool)LightToTrigger)
		{
			LightToTrigger.ManuallyDoBulletSpawnedFade();
		}
		StartCoroutine(FireBeam(beamProjectile));
		StartCoroutine(DoGunMotionCR());
		if (doScreenShake)
		{
			GameManager.Instance.MainCameraController.DoContinuousScreenShake(screenShake, this);
		}
	}

	public override void CeaseFire()
	{
		if ((bool)LightToTrigger)
		{
			LightToTrigger.EndEarly();
		}
		m_firingLaser = false;
		if (doScreenShake)
		{
			GameManager.Instance.MainCameraController.StopContinuousScreenShake(this);
		}
	}

	public void OnPreDeath(Vector2 deathDir)
	{
		m_firingLaser = false;
		if ((bool)m_laserBeam)
		{
			m_laserBeam.DestroyBeam();
			m_laserBeam = null;
		}
	}

	private IEnumerator DoGunMotionCR()
	{
		float elapsed2 = 0f;
		float duration2 = sweepAwayTime;
		while (elapsed2 < duration2)
		{
			yield return null;
			elapsed2 += BraveTime.DeltaTime;
			LaserAngle = -90f + Mathf.SmoothStep(0f, sweepAngle, elapsed2 / duration2);
			base.sprite.UpdateZDepth();
		}
		elapsed2 = 0f;
		duration2 = sweepBackTime;
		while (elapsed2 < duration2)
		{
			yield return null;
			elapsed2 += BraveTime.DeltaTime;
			LaserAngle = -90f + Mathf.SmoothStep(sweepAngle, 0f, elapsed2 / duration2);
			base.sprite.UpdateZDepth();
		}
	}

	protected IEnumerator FireBeam(Projectile projectile)
	{
		GameObject beamObject = UnityEngine.Object.Instantiate(projectile.gameObject);
		m_laserBeam = beamObject.GetComponent<BasicBeamController>();
		m_laserBeam.Owner = ship.aiActor;
		m_laserBeam.HitsPlayers = projectile.collidesWithPlayer;
		m_laserBeam.HitsEnemies = projectile.collidesWithEnemies;
		m_laserBeam.ContinueBeamArtToWall = true;
		bool firstFrame = true;
		while (m_laserBeam != null && m_firingLaser)
		{
			float clampedAngle = BraveMathCollege.ClampAngle360(LaserAngle);
			Vector2 dirVec = new Vector3(Mathf.Cos(clampedAngle * ((float)Math.PI / 180f)), Mathf.Sin(clampedAngle * ((float)Math.PI / 180f))) * 10f;
			m_laserBeam.Origin = beamTransform.position;
			m_laserBeam.Direction = dirVec;
			if (firstFrame)
			{
				yield return null;
				firstFrame = false;
				continue;
			}
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
		if ((bool)m_laserBeam)
		{
			m_laserBeam.SelfUpdate = false;
			while ((bool)m_laserBeam)
			{
				m_laserBeam.Origin = beamTransform.position;
				yield return null;
			}
		}
		m_laserBeam = null;
	}
}
