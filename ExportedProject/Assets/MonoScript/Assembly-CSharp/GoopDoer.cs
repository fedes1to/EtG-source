using System;
using UnityEngine;

public class GoopDoer : BraveBehaviour
{
	public enum PositionSource
	{
		SpriteCenter,
		GroundCenter,
		HitBoxCenter,
		SpecifyGameObject
	}

	public enum UpdateTiming
	{
		Always,
		IfMoving,
		TriggerOnly
	}

	public GoopDefinition goopDefinition;

	public PositionSource positionSource;

	[ShowInInspectorIf("positionSource", 3, false)]
	public GameObject goopCenter;

	public UpdateTiming updateTiming;

	public float updateFrequency = 0.05f;

	public bool isTimed;

	[ShowInInspectorIf("isTimed", true)]
	public float goopTime = 1f;

	[Header("Triggers")]
	public bool updateOnPreDeath;

	public bool updateOnDeath;

	public bool updateOnAnimFrames;

	public bool updateOnCollision;

	public bool updateOnGrounded;

	public bool updateOnDestroy;

	[Header("Global Settings")]
	public float defaultGoopRadius = 1f;

	public bool suppressSplashes;

	public bool goopSizeVaries;

	[ShowInInspectorIf("goopSizeVaries", false)]
	public float varyCycleTime = 1f;

	[ShowInInspectorIf("goopSizeVaries", false)]
	public float radiusMin = 0.5f;

	[ShowInInspectorIf("goopSizeVaries", false)]
	public float radiusMax = 1f;

	[ShowInInspectorIf("goopSizeVaries", false)]
	public bool goopSizeRandom;

	[Header("Particles")]
	public bool UsesDispersalParticles;

	[ShowInInspectorIf("UsesDispersalParticles", false)]
	public float DispersalDensity = 3f;

	[ShowInInspectorIf("UsesDispersalParticles", false)]
	public float DispersalMinCoherency = 0.2f;

	[ShowInInspectorIf("UsesDispersalParticles", false)]
	public float DispersalMaxCoherency = 1f;

	[ShowInInspectorIf("UsesDispersalParticles", false)]
	public GameObject DispersalParticleSystemPrefab;

	private float m_updateTimer;

	private DeadlyDeadlyGoopManager m_gooper;

	private ParticleSystem m_dispersalParticles;

	private Vector2 m_lastGoopPosition = Vector2.zero;

	private float m_timeSinceLastGoop = 10f;

	public void Start()
	{
		m_gooper = DeadlyDeadlyGoopManager.GetGoopManagerForGoopType(goopDefinition);
		if (updateOnAnimFrames && (bool)base.spriteAnimator)
		{
			tk2dSpriteAnimator obj = base.spriteAnimator;
			obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Combine(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleAnimationEvent));
		}
		if (updateOnPreDeath && (bool)base.healthHaver)
		{
			base.healthHaver.OnPreDeath += OnPreDeath;
		}
		if (updateOnDeath && (bool)base.healthHaver)
		{
			base.healthHaver.OnDeath += OnDeath;
		}
		if (updateOnCollision)
		{
			SpeculativeRigidbody speculativeRigidbody = base.specRigidbody;
			speculativeRigidbody.OnCollision = (Action<CollisionData>)Delegate.Combine(speculativeRigidbody.OnCollision, new Action<CollisionData>(OnCollision));
		}
		if (updateOnGrounded)
		{
			if ((bool)base.debris)
			{
				DebrisObject debrisObject = base.debris;
				debrisObject.OnGrounded = (Action<DebrisObject>)Delegate.Combine(debrisObject.OnGrounded, new Action<DebrisObject>(OnDebrisGrounded));
			}
			if (base.projectile is ArcProjectile)
			{
				(base.projectile as ArcProjectile).OnGrounded += OnProjectileGrounded;
			}
		}
		if (UsesDispersalParticles && m_dispersalParticles == null)
		{
			m_dispersalParticles = GlobalDispersalParticleManager.GetSystemForPrefab(DispersalParticleSystemPrefab);
		}
	}

	public void Update()
	{
		m_timeSinceLastGoop += BraveTime.DeltaTime;
		if (ShouldUpdate() && (!(base.aiActor != null) || base.aiActor.HasBeenEngaged) && (!(base.aiActor != null) || base.aiActor.HasBeenAwoken))
		{
			m_updateTimer -= BraveTime.DeltaTime;
			if (m_updateTimer <= 0f)
			{
				GoopItUp();
				m_updateTimer = updateFrequency;
			}
		}
	}

	protected override void OnDestroy()
	{
		if (updateOnDestroy && m_gooper != null)
		{
			GoopItUp();
		}
		if (updateOnAnimFrames && (bool)base.spriteAnimator)
		{
			tk2dSpriteAnimator obj = base.spriteAnimator;
			obj.AnimationEventTriggered = (Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>)Delegate.Remove(obj.AnimationEventTriggered, new Action<tk2dSpriteAnimator, tk2dSpriteAnimationClip, int>(HandleAnimationEvent));
		}
		if (updateOnPreDeath && (bool)base.healthHaver)
		{
			base.healthHaver.OnPreDeath -= OnPreDeath;
		}
		if (updateOnDeath && (bool)base.healthHaver)
		{
			base.healthHaver.OnDeath -= OnDeath;
		}
		if (updateOnGrounded)
		{
			if ((bool)base.debris)
			{
				DebrisObject debrisObject = base.debris;
				debrisObject.OnGrounded = (Action<DebrisObject>)Delegate.Remove(debrisObject.OnGrounded, new Action<DebrisObject>(OnDebrisGrounded));
			}
			if (base.projectile is ArcProjectile)
			{
				(base.projectile as ArcProjectile).OnGrounded -= OnProjectileGrounded;
			}
		}
		base.OnDestroy();
	}

	private void HandleAnimationEvent(tk2dSpriteAnimator animator, tk2dSpriteAnimationClip clip, int frameNum)
	{
		if (clip.GetFrame(frameNum).eventInfo == "goop")
		{
			GoopItUp();
		}
	}

	private void OnPreDeath(Vector2 finalDamageDirection)
	{
		GoopItUp();
	}

	private void OnDeath(Vector2 finalDamageDirection)
	{
		GoopItUp();
	}

	private void OnCollision(CollisionData collisionData)
	{
		GoopItUp();
	}

	private void OnDebrisGrounded(DebrisObject debrisObject)
	{
		GoopItUp();
	}

	private void OnProjectileGrounded()
	{
		GoopItUp();
	}

	private bool ShouldUpdate()
	{
		if (updateTiming == UpdateTiming.Always)
		{
			return true;
		}
		if (updateTiming == UpdateTiming.IfMoving)
		{
			return Mathf.Abs(base.specRigidbody.Velocity.x) > 0.0001f || Mathf.Abs(base.specRigidbody.Velocity.y) > 0.0001f;
		}
		return false;
	}

	private void GoopItUp()
	{
		float num = defaultGoopRadius;
		Vector2 vector = base.transform.position;
		if (positionSource == PositionSource.SpriteCenter)
		{
			vector = base.sprite.WorldCenter;
		}
		else if (positionSource == PositionSource.GroundCenter)
		{
			vector = base.specRigidbody.GetUnitCenter(ColliderType.Ground);
		}
		else if (positionSource == PositionSource.HitBoxCenter)
		{
			if (base.specRigidbody.HitboxPixelCollider == null || !base.specRigidbody.HitboxPixelCollider.Enabled)
			{
				return;
			}
			vector = base.specRigidbody.GetUnitCenter(ColliderType.HitBox);
		}
		else if (positionSource == PositionSource.SpecifyGameObject)
		{
			vector = goopCenter.transform.position.XY();
		}
		if (!isTimed || !(m_lastGoopPosition != Vector2.zero) || !(Vector2.Distance(vector, m_lastGoopPosition) < 0.2f) || !(m_timeSinceLastGoop < goopTime))
		{
			if (goopSizeVaries)
			{
				num = ((!goopSizeRandom) ? BraveMathCollege.SmoothLerp(radiusMin, radiusMax, Mathf.PingPong(Time.time, varyCycleTime) / varyCycleTime) : UnityEngine.Random.Range(radiusMin, radiusMax));
			}
			if (isTimed)
			{
				m_gooper.TimedAddGoopCircle(vector, num, goopTime, suppressSplashes);
			}
			else
			{
				DeadlyDeadlyGoopManager gooper = m_gooper;
				Vector2 center = vector;
				float radius = num;
				bool flag = suppressSplashes;
				gooper.AddGoopCircle(center, radius, -1, flag);
			}
			if (UsesDispersalParticles)
			{
				DoDispersalParticles(vector, num);
			}
			m_lastGoopPosition = vector;
			m_timeSinceLastGoop = 0f;
		}
	}

	private void DoDispersalParticles(Vector2 posStart, float radius)
	{
		int num = Mathf.RoundToInt(radius * radius * DispersalDensity);
		for (int i = 0; i < num; i++)
		{
			Vector3 position = posStart + UnityEngine.Random.insideUnitCircle * UnityEngine.Random.Range(0f, radius);
			float num2 = Mathf.PerlinNoise(position.x / 3f, position.y / 3f);
			Vector3 a = Quaternion.Euler(0f, 0f, num2 * 360f) * Vector3.right;
			Vector3 vector = Vector3.Lerp(a, UnityEngine.Random.insideUnitSphere, UnityEngine.Random.Range(DispersalMinCoherency, DispersalMaxCoherency));
			ParticleSystem.EmitParams emitParams = default(ParticleSystem.EmitParams);
			emitParams.position = position;
			emitParams.velocity = vector * m_dispersalParticles.startSpeed;
			emitParams.startSize = m_dispersalParticles.startSize;
			emitParams.startLifetime = m_dispersalParticles.startLifetime;
			emitParams.startColor = m_dispersalParticles.startColor;
			ParticleSystem.EmitParams emitParams2 = emitParams;
			m_dispersalParticles.Emit(emitParams2, 1);
		}
	}
}
