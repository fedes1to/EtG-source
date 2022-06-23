using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/GatlingGull/WalkAndShoot")]
public class GatlingGullWalkAndShoot : BasicAttackBehavior
{
	public float Duration = 5f;

	public float AngleVariance = 20f;

	public bool ContinuesOnPathComplete;

	public string OverrideBulletName;

	private float m_durationTimer;

	public override void Start()
	{
		base.Start();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_durationTimer);
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		m_durationTimer = Duration;
		m_aiActor.SuppressTargetSwitch = true;
		AkSoundEngine.PostEvent("Play_ANM_Gull_Loop_01", m_gameObject);
		AkSoundEngine.PostEvent("Play_ANM_Gull_Gatling_01", m_gameObject);
		return BehaviorResult.RunContinuousInClass;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (ContinuesOnPathComplete)
		{
			m_aiAnimator.OverrideIdleAnimation = "idle_shoot";
		}
		if (m_durationTimer <= 0f || !m_aiActor.TargetRigidbody || (m_aiActor.PathComplete && !ContinuesOnPathComplete))
		{
			return ContinuousBehaviorResult.Finished;
		}
		Vector2 inVec = m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - m_aiActor.CenterPosition;
		int num = BraveMathCollege.VectorToOctant(inVec);
		m_aiShooter.ManualGunAngle = true;
		m_aiShooter.GunAngle = Mathf.Atan2(inVec.y, inVec.x) * 57.29578f;
		Vector2 direction = Quaternion.Euler(0f, 0f, num * -45) * Vector2.up;
		m_aiShooter.volley.projectiles[0].angleVariance = AngleVariance;
		m_aiShooter.ShootInDirection(direction, OverrideBulletName);
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		if (ContinuesOnPathComplete)
		{
			m_aiAnimator.OverrideIdleAnimation = string.Empty;
		}
		m_aiShooter.ManualGunAngle = false;
		UpdateCooldowns();
		m_aiActor.SuppressTargetSwitch = false;
		AkSoundEngine.PostEvent("Stop_ANM_Gull_Loop_01", m_gameObject);
	}

	public override void Destroy()
	{
		base.Destroy();
		if ((bool)m_aiActor.GetComponent<AkGameObj>())
		{
			AkSoundEngine.PostEvent("Stop_ANM_Gull_Loop_01", m_gameObject);
		}
	}
}
