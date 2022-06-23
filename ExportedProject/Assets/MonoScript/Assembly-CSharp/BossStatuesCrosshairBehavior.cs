using System;
using FullInspector;
using UnityEngine;

[InspectorDropdownName("Bosses/BossStatues/CrosshairBehavior")]
public class BossStatuesCrosshairBehavior : BossStatuesPatternBehavior
{
	public float CircleRadius;

	public float InitialJumpDelay;

	public float SequentialJumpDelays;

	public string AttackVfx;

	public float AttackVfxPreTimer;

	public BulletScriptSelector BulletScript;

	private BulletScriptSource m_bulletSource;

	private float[] m_statueAngles;

	private float m_cachedStatueAngle;

	private float m_jumpTimer;

	private bool m_hasStarted;

	private bool m_isGrounded;

	private bool m_hasPlayedAttackVfx;

	public override void Start()
	{
		base.Start();
		m_cachedStatueAngle = 0.5f * (360f / (float)m_statuesController.allStatues.Count);
		if (TurboModeController.IsActive)
		{
			InitialJumpDelay /= TurboModeController.sEnemyBulletSpeedMultiplier;
			SequentialJumpDelays /= TurboModeController.sEnemyBulletSpeedMultiplier;
		}
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		if (m_state == PatternState.InProgress)
		{
			if (!m_hasStarted)
			{
				if (!m_hasPlayedAttackVfx)
				{
					float num = m_statuesController.attackHopTime - m_timeElapsed;
					if (num < AttackVfxPreTimer)
					{
						m_statuesController.GetComponent<VfxController>().PlayVfx(AttackVfx);
						m_hasPlayedAttackVfx = true;
					}
				}
				if (m_timeElapsed > 0.1f)
				{
					SetActiveState(BossStatueController.StatueState.StandStill);
					if (AreAllGrounded())
					{
						m_hasStarted = true;
						ShootBulletScript();
						m_hasPlayedAttackVfx = false;
						m_isGrounded = true;
						m_jumpTimer = InitialJumpDelay;
					}
				}
			}
			else
			{
				m_jumpTimer -= m_deltaTime;
				if (!m_hasPlayedAttackVfx)
				{
					float jumpTimer = m_jumpTimer;
					if (jumpTimer < AttackVfxPreTimer)
					{
						m_statuesController.GetComponent<VfxController>().PlayVfx(AttackVfx);
						m_hasPlayedAttackVfx = true;
					}
				}
				if (m_isGrounded)
				{
					if (m_jumpTimer <= m_statuesController.attackHopTime)
					{
						float num2 = -1f;
						for (int i = 0; i < m_activeStatueCount; i++)
						{
							if ((bool)m_activeStatues[i] && m_activeStatues[i].healthHaver.IsAlive)
							{
								m_activeStatues[i].QueuedBulletScript.Add(null);
								m_activeStatues[i].State = BossStatueController.StatueState.HopToTarget;
								num2 = Math.Max(m_activeStatues[i].DistancetoTarget, num2);
							}
						}
						if (num2 > 0f)
						{
							m_statuesController.OverrideMoveSpeed = Mathf.Max(m_statuesController.moveSpeed, 1.5f * num2 / m_statuesController.attackHopTime);
						}
						m_jumpTimer += SequentialJumpDelays;
						m_isGrounded = false;
					}
				}
				else
				{
					m_isGrounded = true;
					for (int j = 0; j < m_activeStatueCount; j++)
					{
						if ((bool)m_activeStatues[j] && m_activeStatues[j].healthHaver.IsAlive)
						{
							m_activeStatues[j].State = BossStatueController.StatueState.StandStill;
							m_isGrounded &= m_activeStatues[j].IsGrounded;
						}
					}
					if (m_isGrounded)
					{
						m_hasPlayedAttackVfx = false;
						m_statuesController.OverrideMoveSpeed = null;
					}
				}
			}
		}
		return base.ContinuousUpdate();
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		m_cachedStatueAngle = BraveMathCollege.ClampAngle360(m_statueAngles[0]);
	}

	protected override void InitPositions()
	{
		m_statueAngles = new float[m_activeStatueCount];
		for (int i = 0; i < m_activeStatueCount; i++)
		{
			m_statueAngles[i] = m_cachedStatueAngle + (float)i * (360f / (float)m_activeStatueCount);
		}
		Vector2[] array = new Vector2[m_activeStatueCount];
		for (int j = 0; j < m_activeStatueCount; j++)
		{
			array[j] = GetTargetPoint(m_statueAngles[j]);
		}
		ReorderStatues(array);
		for (int k = 0; k < array.Length; k++)
		{
			m_activeStatues[k].Target = GetTargetPoint(m_statueAngles[k]);
		}
		m_hasStarted = false;
	}

	protected override void UpdatePositions()
	{
	}

	protected override bool IsFinished()
	{
		return m_hasStarted && m_bulletSource.IsEnded;
	}

	protected override void OnStatueDeath()
	{
		if ((bool)m_bulletSource)
		{
			m_statuesController.ClearBullets(m_bulletSource.transform.position);
			UnityEngine.Object.Destroy(m_bulletSource);
			m_bulletSource = null;
			AkSoundEngine.PostEvent("Stop_ENM_statue_ring_01", m_statuesController.bulletBank.gameObject);
		}
	}

	protected override void BeginState(PatternState state)
	{
		base.BeginState(state);
		if (state != PatternState.InProgress)
		{
			return;
		}
		m_hasStarted = false;
		m_hasPlayedAttackVfx = false;
		for (int i = 0; i < m_activeStatueCount; i++)
		{
			BossStatueController bossStatueController = m_activeStatues[i];
			if ((bool)bossStatueController && bossStatueController.healthHaver.IsAlive)
			{
				bossStatueController.knockbackDoer.SetImmobile(true, "CrosshairBehavior");
				bossStatueController.healthHaver.AllDamageMultiplier *= 0.5f;
				bossStatueController.QueuedBulletScript.Add(null);
				bossStatueController.State = BossStatueController.StatueState.HopToTarget;
				bossStatueController.SuppressShootVfx = true;
			}
		}
	}

	protected override void EndState(PatternState state)
	{
		switch (state)
		{
		case PatternState.MovingToStartingPosition:
			m_statuesController.IsTransitioning = false;
			break;
		case PatternState.InProgress:
		{
			if (OverrideMoveSpeed > 0f)
			{
				m_statuesController.OverrideMoveSpeed = null;
			}
			for (int i = 0; i < m_activeStatueCount; i++)
			{
				BossStatueController bossStatueController = m_activeStatues[i];
				if ((bool)bossStatueController && bossStatueController.healthHaver.IsAlive)
				{
					bossStatueController.knockbackDoer.SetImmobile(false, "CrosshairBehavior");
					bossStatueController.healthHaver.AllDamageMultiplier *= 2f;
					bossStatueController.SuppressShootVfx = true;
				}
			}
			break;
		}
		}
	}

	private Vector2 GetTargetPoint(float angle)
	{
		return m_statuesController.PatternCenter + BraveMathCollege.DegreesToVector(angle, CircleRadius);
	}

	private void ShootBulletScript()
	{
		if (!m_bulletSource)
		{
			Transform transform = new GameObject("crazy shoot point").transform;
			transform.position = m_statuesController.PatternCenter;
			m_bulletSource = transform.gameObject.GetOrAddComponent<BulletScriptSource>();
		}
		m_bulletSource.BulletManager = m_statuesController.bulletBank;
		m_bulletSource.BulletScript = BulletScript;
		m_bulletSource.Initialize();
	}
}
