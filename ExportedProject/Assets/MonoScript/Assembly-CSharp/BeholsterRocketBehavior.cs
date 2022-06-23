using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/Beholster/RocketBehavior2")]
public class BeholsterRocketBehavior : BasicAttackBehavior
{
	public bool LineOfSight = true;

	public float WindUpTime = 1f;

	public GameObject TargetVFX;

	public float[] FiringAngles;

	public float FireCooldown;

	public BeholsterTentacleController[] Tentacles;

	private BeholsterController m_beholster;

	private float m_windupTimer;

	private GameObject m_spawnedTargetVfx;

	private float m_fireTimer;

	private int m_fireIndex;

	public override void Start()
	{
		base.Start();
		m_beholster = m_aiActor.GetComponent<BeholsterController>();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_windupTimer);
		DecrementTimer(ref m_fireTimer);
	}

	public override BehaviorResult Update()
	{
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		bool flag = LineOfSight && !m_aiActor.HasLineOfSightToTarget;
		if (m_aiActor.TargetRigidbody == null || flag)
		{
			m_beholster.StopFiringTentacles(Tentacles);
			return BehaviorResult.Continue;
		}
		if (WindUpTime > 0f)
		{
			m_windupTimer = WindUpTime;
			m_aiActor.ClearPath();
			if ((bool)TargetVFX)
			{
				m_spawnedTargetVfx = Object.Instantiate(TargetVFX, m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox), Quaternion.identity);
				m_spawnedTargetVfx.transform.parent = m_aiActor.TargetRigidbody.transform;
				tk2dBaseSprite component = m_spawnedTargetVfx.GetComponent<tk2dBaseSprite>();
				tk2dBaseSprite sprite = m_aiActor.TargetRigidbody.sprite;
				if ((bool)component && (bool)sprite)
				{
					sprite.AttachRenderer(component);
					component.HeightOffGround = 5f;
					component.UpdateZDepth();
				}
			}
		}
		m_fireIndex = 0;
		m_fireTimer = 0f;
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (m_windupTimer > 0f)
		{
			return ContinuousBehaviorResult.Continue;
		}
		if (m_fireTimer <= 0f)
		{
			m_beholster.SingleFireTentacle(Tentacles, FiringAngles[m_fireIndex % FiringAngles.Length]);
			m_fireTimer = FireCooldown;
			m_fireIndex++;
			if ((bool)m_spawnedTargetVfx)
			{
				Object.Destroy(m_spawnedTargetVfx);
				m_spawnedTargetVfx = null;
			}
			int num = ((!m_aiActor.IsBlackPhantom) ? 1 : 2);
			if (m_fireIndex >= FiringAngles.Length * num)
			{
				UpdateCooldowns();
				return ContinuousBehaviorResult.Finished;
			}
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if ((bool)m_spawnedTargetVfx)
		{
			Object.Destroy(m_spawnedTargetVfx);
			m_spawnedTargetVfx = null;
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		if ((bool)m_spawnedTargetVfx)
		{
			Object.Destroy(m_spawnedTargetVfx);
			m_spawnedTargetVfx = null;
		}
	}

	public override bool IsReady()
	{
		if (!base.IsReady())
		{
			return false;
		}
		for (int i = 0; i < Tentacles.Length; i++)
		{
			if (Tentacles[i].IsReady)
			{
				return true;
			}
		}
		return false;
	}
}
