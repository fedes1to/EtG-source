using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/GatlingGull/FanSprayBehavior")]
public class GatlingGullFanSpray : BasicAttackBehavior
{
	public float SprayAngle = 120f;

	public float SpraySpeed = 60f;

	public int SprayIterations = 4;

	public string OverrideBulletName;

	private float m_remainingDuration;

	private float m_totalDuration;

	public override void Start()
	{
		base.Start();
	}

	public override void Upkeep()
	{
		base.Upkeep();
	}

	public override BehaviorResult Update()
	{
		base.Update();
		if (!m_aiActor.TargetRigidbody)
		{
			return BehaviorResult.Continue;
		}
		m_aiShooter.volley.projectiles[0].angleVariance = 0f;
		AkSoundEngine.PostEvent("Play_ANM_Gull_Shoot_01", m_gameObject);
		m_totalDuration = SprayAngle / SpraySpeed * (float)SprayIterations;
		m_remainingDuration = m_totalDuration;
		m_aiActor.ClearPath();
		AkSoundEngine.PostEvent("Play_ANM_Gull_Loop_01", m_gameObject);
		AkSoundEngine.PostEvent("Play_ANM_Gull_Gatling_01", m_gameObject);
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		DecrementTimer(ref m_remainingDuration);
		if (m_remainingDuration <= 0f || !m_aiActor.TargetRigidbody)
		{
			return ContinuousBehaviorResult.Finished;
		}
		float num = 1f - m_remainingDuration / m_totalDuration;
		float num2 = num * (float)SprayIterations % 2f;
		float input = (m_aiActor.TargetRigidbody.GetUnitCenter(ColliderType.HitBox) - m_aiShooter.volleyShootPosition.position.XY()).ToAngle();
		input = BraveMathCollege.QuantizeFloat(input, 45f);
		input += (0f - SprayAngle) / 2f + Mathf.PingPong(num2 * SprayAngle, SprayAngle);
		m_aiShooter.ShootInDirection(Quaternion.Euler(0f, 0f, input) * Vector2.right, OverrideBulletName);
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		AkSoundEngine.PostEvent("Stop_ANM_Gull_Loop_01", m_gameObject);
		UpdateCooldowns();
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
