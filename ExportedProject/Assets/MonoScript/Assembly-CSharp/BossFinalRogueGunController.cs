using FullInspector;
using UnityEngine;

public abstract class BossFinalRogueGunController : BaseBehavior<FullSerializerSerializer>
{
	public enum FireType
	{
		Triggered = 10,
		Timed = 20
	}

	public BossFinalRogueController ship;

	public FireType fireType = FireType.Triggered;

	[InspectorShowIf("IsTimed")]
	public float initialDelay;

	[InspectorShowIf("IsTimed")]
	public float delay;

	private float m_shotTimer;

	public abstract bool IsFinished { get; }

	public bool IsTimed()
	{
		return fireType == FireType.Timed;
	}

	public virtual void Start()
	{
		if (fireType == FireType.Timed)
		{
			m_shotTimer = initialDelay;
		}
		ship.healthHaver.OnPreDeath += OnPreDeath;
	}

	public virtual void Update()
	{
		if (ship.aiActor.enabled && ship.behaviorSpeculator.enabled && fireType == FireType.Timed && IsFinished)
		{
			m_shotTimer -= BraveTime.DeltaTime;
			if (m_shotTimer <= 0f)
			{
				Fire();
				m_shotTimer = delay;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	private void OnPreDeath(Vector2 deathDir)
	{
		CeaseFire();
	}

	public abstract void Fire();

	public abstract void CeaseFire();
}
