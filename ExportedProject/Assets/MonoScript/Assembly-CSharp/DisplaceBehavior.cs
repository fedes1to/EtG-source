using FullInspector;
using UnityEngine;

public class DisplaceBehavior : BasicAttackBehavior
{
	private enum State
	{
		Idle,
		Summoning
	}

	public float ImageHealthMultiplier = 1f;

	public float InitialImageAttackDelay = 0.5f;

	[InspectorCategory("Visuals")]
	public string Anim;

	private State m_state;

	private Shader m_cachedShader;

	private float m_timer;

	private bool m_hasInstantSpawned;

	private BulletLimbController[] m_limbControllers;

	private AIActor m_image;

	public override void Start()
	{
		base.Start();
		m_aiAnimator.ChildAnimator.renderer.enabled = false;
		m_limbControllers = m_aiActor.GetComponentsInChildren<BulletLimbController>();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		m_aiAnimator.ChildAnimator.renderer.enabled = false;
		DecrementTimer(ref m_timer);
		if ((bool)m_image && m_image.healthHaver.IsDead)
		{
			m_image = null;
			UpdateCooldowns();
		}
	}

	public override BehaviorResult Update()
	{
		if (!m_aiActor.GetComponent<DisplacedImageController>() && !m_hasInstantSpawned)
		{
			SpawnImage();
			m_hasInstantSpawned = true;
		}
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		m_aiAnimator.PlayUntilFinished(Anim, true);
		m_aiActor.ClearPath();
		if ((bool)m_aiActor && (bool)m_aiActor.knockbackDoer)
		{
			m_aiActor.knockbackDoer.SetImmobile(true, "DisplaceBehavior");
		}
		for (int i = 0; i < m_limbControllers.Length; i++)
		{
			m_limbControllers[i].enabled = true;
			m_limbControllers[i].HideBullets = false;
		}
		m_state = State.Summoning;
		m_updateEveryFrame = true;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (m_state == State.Summoning && !m_aiAnimator.IsPlaying(Anim))
		{
			SpawnImage();
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (!string.IsNullOrEmpty(Anim))
		{
			m_aiAnimator.EndAnimationIf(Anim);
		}
		if ((bool)m_aiActor && (bool)m_aiActor.knockbackDoer)
		{
			m_aiActor.knockbackDoer.SetImmobile(false, "DisplaceBehavior");
		}
		for (int i = 0; i < m_limbControllers.Length; i++)
		{
			m_limbControllers[i].enabled = false;
			m_limbControllers[i].HideBullets = true;
		}
		m_state = State.Idle;
		m_updateEveryFrame = false;
		UpdateCooldowns();
	}

	public override bool IsReady()
	{
		if ((bool)m_image && m_image.healthHaver.IsAlive)
		{
			return false;
		}
		return base.IsReady();
	}

	public override bool IsOverridable()
	{
		return false;
	}

	private void SpawnImage()
	{
		if ((bool)m_behaviorSpeculator && m_behaviorSpeculator.MovementBehaviors.Count == 0)
		{
			return;
		}
		AIActor orLoadByGuid = EnemyDatabase.GetOrLoadByGuid(m_aiActor.EnemyGuid);
		m_image = AIActor.Spawn(orLoadByGuid, m_aiActor.specRigidbody.UnitBottomLeft, m_aiActor.ParentRoom, false, AIActor.AwakenAnimationType.Spawn);
		m_image.transform.position = m_aiActor.transform.position;
		m_image.specRigidbody.Reinitialize();
		m_image.aiAnimator.healthHaver.SetHealthMaximum(ImageHealthMultiplier * m_aiActor.healthHaver.GetMaxHealth());
		DisplacedImageController displacedImageController = m_image.gameObject.AddComponent<DisplacedImageController>();
		displacedImageController.Init();
		displacedImageController.SetHost(m_aiActor);
		if ((bool)m_behaviorSpeculator && m_behaviorSpeculator.MovementBehaviors != null && m_behaviorSpeculator.MovementBehaviors.Count > 0)
		{
			FleeTargetBehavior fleeTargetBehavior = m_behaviorSpeculator.MovementBehaviors[0] as FleeTargetBehavior;
			if (fleeTargetBehavior != null)
			{
				fleeTargetBehavior.ForceRun = true;
			}
		}
		if (!m_hasInstantSpawned)
		{
			m_image.behaviorSpeculator.GlobalCooldown = InitialImageAttackDelay;
		}
	}
}
