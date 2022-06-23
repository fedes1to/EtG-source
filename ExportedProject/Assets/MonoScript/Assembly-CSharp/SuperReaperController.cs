using System;
using Dungeonator;
using UnityEngine;

public class SuperReaperController : BraveBehaviour
{
	private static SuperReaperController m_instance;

	public static bool PreventShooting;

	public BulletScriptSelector BulletScript;

	public Transform ShootPoint;

	public float ShootTimer = 3f;

	public float MinSpeed = 3f;

	public float MaxSpeed = 10f;

	public float MinSpeedDistance = 10f;

	public float MaxSpeedDistance = 50f;

	[NonSerialized]
	public Vector2 knockbackComponent;

	private PlayerController m_currentTargetPlayer;

	private BulletScriptSource m_bulletSource;

	private float m_shootTimer;

	private int c_particlesPerSecond = 50;

	public static SuperReaperController Instance
	{
		get
		{
			return m_instance;
		}
	}

	private void Start()
	{
		m_instance = this;
		m_shootTimer = ShootTimer;
		SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
		speculativeRigidbody.OnEnterTrigger = (SpeculativeRigidbody.OnTriggerDelegate)Delegate.Combine(speculativeRigidbody.OnEnterTrigger, new SpeculativeRigidbody.OnTriggerDelegate(HandleTriggerEntered));
		base.aiAnimator.PlayUntilCancelled("idle");
		base.aiAnimator.PlayUntilFinished("intro");
		tk2dSpriteAnimator obj = base.spriteAnimator;
		obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleAnimationEvent));
		for (int i = 0; i < EncounterDatabase.Instance.Entries.Count; i++)
		{
			if (EncounterDatabase.Instance.Entries[i].journalData.PrimaryDisplayName == "#SREAPER_ENCNAME")
			{
				GameStatsManager.Instance.HandleEncounteredObjectRaw(EncounterDatabase.Instance.Entries[i].myGuid);
			}
		}
		m_currentTargetPlayer = GameManager.Instance.GetRandomActivePlayer();
		if ((bool)base.encounterTrackable)
		{
			GameStatsManager.Instance.HandleEncounteredObject(base.encounterTrackable);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		m_instance = null;
	}

	private void HandleTriggerEntered(SpeculativeRigidbody targetRigidbody, SpeculativeRigidbody sourceSpecRigidbody, CollisionData collisionData)
	{
		Projectile projectile = targetRigidbody.projectile;
		if ((bool)projectile)
		{
			projectile.HandleKnockback(base.specRigidbody, targetRigidbody.GetComponent<PlayerController>());
		}
	}

	private void HandleAnimationEvent(tk2dSpriteAnimator arg1, tk2dSpriteAnimationClip arg2, int arg3)
	{
		tk2dSpriteAnimationFrame frame = arg2.GetFrame(arg3);
		if (frame.eventInfo == "fire")
		{
			SpawnProjectiles();
		}
	}

	private void Update()
	{
		if (GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.END_TIMES || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.CHARACTER_PAST || GameManager.Instance.CurrentLevelOverrideState == GameManager.LevelOverrideState.FOYER || BossKillCam.BossDeathCamRunning || GameManager.Instance.PreventPausing)
		{
			return;
		}
		if (TimeTubeCreditsController.IsTimeTubing)
		{
			base.gameObject.SetActive(false);
			return;
		}
		HandleMotion();
		if (!PreventShooting)
		{
			HandleAttacks();
		}
		UpdateBlackPhantomParticles();
	}

	private void HandleAttacks()
	{
		if (base.aiAnimator.IsPlaying("intro"))
		{
			return;
		}
		CellData cellData = GameManager.Instance.Dungeon.data[ShootPoint.position.IntXY(VectorConversions.Floor)];
		if (cellData != null && cellData.type != CellType.WALL)
		{
			m_shootTimer -= BraveTime.DeltaTime;
			if (m_shootTimer <= 0f)
			{
				base.aiAnimator.PlayUntilFinished("attack");
				m_shootTimer = ShootTimer;
			}
		}
	}

	private void SpawnProjectiles()
	{
		if (GameManager.Instance.PreventPausing || BossKillCam.BossDeathCamRunning || PreventShooting)
		{
			return;
		}
		CellData cellData = GameManager.Instance.Dungeon.data[ShootPoint.position.IntXY(VectorConversions.Floor)];
		if (cellData != null && cellData.type != CellType.WALL)
		{
			if (!m_bulletSource)
			{
				m_bulletSource = ShootPoint.gameObject.GetOrAddComponent<BulletScriptSource>();
			}
			m_bulletSource.BulletManager = base.bulletBank;
			m_bulletSource.BulletScript = BulletScript;
			m_bulletSource.Initialize();
		}
	}

	private void UpdateBlackPhantomParticles()
	{
		if (GameManager.Options.ShaderQuality != 0 && GameManager.Options.ShaderQuality != GameOptions.GenericHighMedLowOption.VERY_LOW && !base.aiAnimator.IsPlaying("intro"))
		{
			Vector3 vector = base.specRigidbody.UnitBottomLeft.ToVector3ZisY();
			Vector3 vector2 = base.specRigidbody.UnitTopRight.ToVector3ZisY();
			float num = (vector2.y - vector.y) * (vector2.x - vector.x);
			float num2 = (float)c_particlesPerSecond * num;
			int num3 = Mathf.CeilToInt(Mathf.Max(1f, num2 * BraveTime.DeltaTime));
			int num4 = num3;
			Vector3 minPosition = vector;
			Vector3 maxPosition = vector2;
			Vector3 up = Vector3.up;
			float angleVariance = 120f;
			float magnitudeVariance = 0.5f;
			float? startLifetime = UnityEngine.Random.Range(1f, 1.65f);
			GlobalSparksDoer.DoRandomParticleBurst(num4, minPosition, maxPosition, up, angleVariance, magnitudeVariance, null, startLifetime, null, GlobalSparksDoer.SparksType.BLACK_PHANTOM_SMOKE);
		}
	}

	private void HandleMotion()
	{
		base.specRigidbody.Velocity = Vector2.zero;
		if (!base.aiAnimator.IsPlaying("intro") && !(m_currentTargetPlayer == null))
		{
			if (m_currentTargetPlayer.healthHaver.IsDead || m_currentTargetPlayer.IsGhost)
			{
				m_currentTargetPlayer = GameManager.Instance.GetRandomActivePlayer();
			}
			Vector2 centerPosition = m_currentTargetPlayer.CenterPosition;
			Vector2 vector = centerPosition - base.specRigidbody.UnitCenter;
			float magnitude = vector.magnitude;
			float num = Mathf.Lerp(MinSpeed, MaxSpeed, (magnitude - MinSpeedDistance) / (MaxSpeedDistance - MinSpeedDistance));
			base.specRigidbody.Velocity = vector.normalized * num;
			base.specRigidbody.Velocity += knockbackComponent;
		}
	}
}
