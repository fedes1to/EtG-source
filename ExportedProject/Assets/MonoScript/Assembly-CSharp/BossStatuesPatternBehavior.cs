using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossStatuesPatternBehavior : BasicAttackBehavior
{
	protected enum PatternState
	{
		Idle,
		MovingToStartingPosition,
		InProgress,
		Ending
	}

	[Serializable]
	public abstract class StatueAttack
	{
		public virtual void Start(List<BossStatueController> statues)
		{
		}

		public abstract void Update(float prevTimeElapsed, float timeElapsed, List<BossStatueController> statues);
	}

	[Serializable]
	public class TimedAttacks : StatueAttack
	{
		[Serializable]
		public class TimedAttack
		{
			public int index;

			public float delay;

			public BulletScriptSelector bulletScript;
		}

		public List<TimedAttack> attacks;

		public override void Update(float prevTimeElapsed, float timeElapsed, List<BossStatueController> statues)
		{
			for (int i = 0; i < attacks.Count; i++)
			{
				TimedAttack timedAttack = attacks[i];
				if (prevTimeElapsed < timedAttack.delay && timeElapsed >= timedAttack.delay && timedAttack.index < statues.Count)
				{
					statues[timedAttack.index].QueuedBulletScript.Add(timedAttack.bulletScript);
				}
			}
		}
	}

	[Serializable]
	public class ConstantAttacks : StatueAttack
	{
		[Serializable]
		public class ConstantAttackGroup
		{
			public int index;

			public List<BulletScriptSelector> bulletScript;
		}

		public List<ConstantAttackGroup> attacks;

		[NonSerialized]
		private int[] m_bulletScriptIndices;

		public override void Start(List<BossStatueController> statues)
		{
			m_bulletScriptIndices = new int[statues.Count];
		}

		public override void Update(float prevTimeElapsed, float timeElapsed, List<BossStatueController> statues)
		{
			for (int i = 0; i < attacks.Count; i++)
			{
				ConstantAttackGroup constantAttackGroup = attacks[i];
				int index = constantAttackGroup.index;
				if (index < statues.Count && statues[index].QueuedBulletScript.Count == 0)
				{
					int num = m_bulletScriptIndices[index];
					num = (num + 1) % constantAttackGroup.bulletScript.Count;
					m_bulletScriptIndices[index] = num;
					BulletScriptSelector item = constantAttackGroup.bulletScript[num];
					statues[index].QueuedBulletScript.Add(item);
				}
			}
		}
	}

	public string numStatues = "2-4";

	public float OverrideMoveSpeed = -1f;

	public bool waitForStartingPositions = true;

	public StatueAttack attackType;

	protected PatternState m_state;

	protected int[] numStatuesArray;

	protected BossStatuesController m_statuesController;

	protected List<BossStatueController> m_activeStatues;

	protected int m_activeStatueCount;

	protected float m_stateTimer;

	protected float m_timeElapsed;

	protected PatternState State
	{
		get
		{
			return m_state;
		}
		set
		{
			if (m_state != value)
			{
				EndState(m_state);
				m_state = value;
				BeginState(m_state);
			}
		}
	}

	public override void Start()
	{
		base.Start();
		m_statuesController = m_gameObject.GetComponent<BossStatuesController>();
		m_activeStatues = new List<BossStatueController>(m_statuesController.allStatues);
		UpdateNumStatuesArray();
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_stateTimer);
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
		m_updateEveryFrame = true;
		RefreshActiveStatues();
		InitPositions();
		if (attackType != null)
		{
			attackType.Start(m_activeStatues);
		}
		State = (waitForStartingPositions ? PatternState.MovingToStartingPosition : PatternState.InProgress);
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		base.ContinuousUpdate();
		if (State != PatternState.Ending && AnyStatuesHaveDied())
		{
			for (int i = 0; i < m_activeStatueCount; i++)
			{
				BossStatueController bossStatueController = m_activeStatues[i];
				if ((bool)bossStatueController)
				{
					bossStatueController.ForceStopBulletScript();
				}
			}
			OnStatueDeath();
			State = PatternState.Ending;
			return ContinuousBehaviorResult.Continue;
		}
		if (State == PatternState.MovingToStartingPosition)
		{
			if (m_stateTimer <= 0f)
			{
				SetActiveState(BossStatueController.StatueState.StandStill);
				if (AreAllGroundedAndReadyToJump())
				{
					State = PatternState.InProgress;
				}
			}
		}
		else if (State == PatternState.InProgress)
		{
			float timeElapsed = m_timeElapsed;
			m_timeElapsed += m_deltaTime;
			UpdatePositions();
			if (attackType != null)
			{
				attackType.Update(timeElapsed, m_timeElapsed, m_activeStatues);
			}
			if (IsFinished())
			{
				State = PatternState.Ending;
			}
		}
		if (State == PatternState.Ending && AreAllGroundedAndReadyToJump())
		{
			return ContinuousBehaviorResult.Finished;
		}
		return ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		base.EndContinuousUpdate();
		for (int i = 0; i < m_activeStatueCount; i++)
		{
			m_activeStatues[i].ClearQueuedAttacks();
			m_activeStatues[i].State = BossStatueController.StatueState.WaitForAttack;
		}
		m_updateEveryFrame = false;
		UpdateCooldowns();
		State = PatternState.Idle;
	}

	public override bool IsOverridable()
	{
		return false;
	}

	public override bool IsReady()
	{
		if (!base.IsReady())
		{
			return false;
		}
		if (Array.IndexOf(numStatuesArray, m_statuesController.NumLivingStatues) < 0)
		{
			return false;
		}
		for (int i = 0; i < m_activeStatues.Count; i++)
		{
			BossStatueController bossStatueController = m_activeStatues[i];
			if ((bool)bossStatueController && bossStatueController.healthHaver.IsAlive && bossStatueController.IsTransforming)
			{
				return false;
			}
		}
		return true;
	}

	protected virtual void BeginState(PatternState state)
	{
		switch (state)
		{
		case PatternState.MovingToStartingPosition:
		{
			m_statuesController.IsTransitioning = true;
			m_stateTimer = 0f;
			SetActiveState(BossStatueController.StatueState.HopToTarget);
			float effectiveMoveSpeed = m_statuesController.GetEffectiveMoveSpeed(m_statuesController.transitionMoveSpeed);
			for (int i = 0; i < m_activeStatueCount; i++)
			{
				float b = m_activeStatues[i].DistancetoTarget / effectiveMoveSpeed;
				m_stateTimer = Mathf.Max(m_stateTimer, b);
			}
			break;
		}
		case PatternState.InProgress:
			m_timeElapsed = 0f;
			SetActiveState(BossStatueController.StatueState.HopToTarget);
			if (OverrideMoveSpeed > 0f)
			{
				m_statuesController.OverrideMoveSpeed = OverrideMoveSpeed;
			}
			if (attackType != null)
			{
				attackType.Update(-0.02f, 0f, m_activeStatues);
			}
			break;
		case PatternState.Ending:
			SetActiveState(BossStatueController.StatueState.StandStill);
			break;
		}
	}

	protected virtual void EndState(PatternState state)
	{
		switch (state)
		{
		case PatternState.MovingToStartingPosition:
			m_statuesController.IsTransitioning = false;
			break;
		case PatternState.InProgress:
			if (OverrideMoveSpeed > 0f)
			{
				m_statuesController.OverrideMoveSpeed = null;
			}
			break;
		}
	}

	protected void SetActiveState(BossStatueController.StatueState newState)
	{
		for (int i = 0; i < m_activeStatueCount; i++)
		{
			BossStatueController bossStatueController = m_activeStatues[i];
			if ((bool)bossStatueController && bossStatueController.healthHaver.IsAlive)
			{
				bossStatueController.State = newState;
			}
		}
	}

	protected bool AreAllGrounded()
	{
		for (int i = 0; i < m_activeStatueCount; i++)
		{
			BossStatueController bossStatueController = m_activeStatues[i];
			if ((bool)bossStatueController && bossStatueController.healthHaver.IsAlive && !bossStatueController.IsGrounded)
			{
				return false;
			}
		}
		return true;
	}

	protected bool AreAllGroundedAndReadyToJump()
	{
		for (int i = 0; i < m_activeStatueCount; i++)
		{
			BossStatueController bossStatueController = m_activeStatues[i];
			if ((bool)bossStatueController && bossStatueController.healthHaver.IsAlive && (!bossStatueController.IsGrounded || !bossStatueController.ReadyToJump))
			{
				return false;
			}
		}
		return true;
	}

	private bool AnyStatuesHaveDied()
	{
		for (int i = 0; i < m_activeStatueCount; i++)
		{
			BossStatueController bossStatueController = m_activeStatues[i];
			if (!bossStatueController || bossStatueController.healthHaver.IsDead)
			{
				return true;
			}
		}
		return false;
	}

	private void RefreshActiveStatues()
	{
		for (int num = m_activeStatues.Count - 1; num >= 0; num--)
		{
			if (!m_activeStatues[num] || m_activeStatues[num].healthHaver.IsDead)
			{
				m_activeStatues.RemoveAt(num);
			}
		}
		m_activeStatueCount = m_activeStatues.Count;
	}

	private void UpdateNumStatuesArray()
	{
		numStatuesArray = BraveUtility.ParsePageNums(numStatues);
	}

	protected abstract void InitPositions();

	protected abstract void UpdatePositions();

	protected abstract bool IsFinished();

	protected virtual void OnStatueDeath()
	{
	}

	protected void ReorderStatues(Vector2[] positions)
	{
		int[] numList = new int[m_activeStatueCount];
		for (int i = 0; i < m_activeStatueCount; i++)
		{
			numList[i] = i;
		}
		float num = float.MaxValue;
		int[] array = new int[m_activeStatueCount];
		do
		{
			int num2 = 0;
			float num3 = 0f;
			for (int j = 0; j < m_activeStatueCount; j++)
			{
				if ((bool)m_activeStatues[num2])
				{
					num3 += Vector2.Distance(m_activeStatues[num2].GroundPosition, positions[numList[j]]);
				}
				num2++;
			}
			if (num3 < num)
			{
				num = num3;
				Array.Copy(numList, array, m_activeStatueCount);
			}
		}
		while (BraveMathCollege.NextPermutation(ref numList));
		List<BossStatueController> list = new List<BossStatueController>(m_activeStatues);
		for (int k = 0; k < m_activeStatueCount; k++)
		{
			m_activeStatues[k] = list[array[k]];
		}
	}
}
