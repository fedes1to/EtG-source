using System.Collections.Generic;
using Dungeonator;
using FullInspector;
using UnityEngine;

public class BuffEnemiesBehavior : BasicAttackBehavior
{
	public float SearchInterval = 1f;

	public float EnemiesToBuff = 1f;

	public bool UsesBuffEffect = true;

	public AIActorBuffEffect buffEffect;

	public bool JamEnemies;

	public GameObject SmallJamEffect;

	public GameObject LargeJamEffect;

	[InspectorCategory("Visuals")]
	public string BuffAnimation;

	[InspectorCategory("Visuals")]
	public string BuffVfx;

	private float m_searchTimer;

	private List<AIActor> m_buffedEnemies = new List<AIActor>();

	private static List<AIActor> s_activeEnemies = new List<AIActor>();

	public override void Start()
	{
		base.Start();
		m_aiActor.IsBuffEnemy = true;
	}

	public override void Upkeep()
	{
		base.Upkeep();
		DecrementTimer(ref m_searchTimer);
	}

	public override BehaviorResult Update()
	{
		base.Update();
		BehaviorResult behaviorResult = base.Update();
		if (behaviorResult != 0)
		{
			return behaviorResult;
		}
		if (!IsReady())
		{
			return BehaviorResult.Continue;
		}
		if (m_searchTimer > 0f)
		{
			return BehaviorResult.Continue;
		}
		m_aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All, ref s_activeEnemies);
		s_activeEnemies.Remove(m_aiActor);
		for (int num = s_activeEnemies.Count - 1; num >= 0; num--)
		{
			if (!IsGoodBuffTarget(s_activeEnemies[num]))
			{
				s_activeEnemies.RemoveAt(num);
			}
		}
		if (s_activeEnemies.Count == 0)
		{
			return BehaviorResult.Continue;
		}
		while ((float)m_buffedEnemies.Count < EnemiesToBuff && s_activeEnemies.Count > 0)
		{
			int index = Random.Range(0, s_activeEnemies.Count);
			m_buffedEnemies.Add(s_activeEnemies[index]);
			s_activeEnemies.RemoveAt(index);
		}
		for (int i = 0; i < m_buffedEnemies.Count; i++)
		{
			BuffEnemy(m_buffedEnemies[i]);
		}
		m_searchTimer = SearchInterval;
		if (!string.IsNullOrEmpty(BuffAnimation))
		{
			m_aiAnimator.PlayUntilCancelled(BuffAnimation, true);
		}
		if (!string.IsNullOrEmpty(BuffVfx))
		{
			m_aiAnimator.PlayVfx(BuffVfx);
		}
		if ((bool)m_aiActor && (bool)m_aiActor.knockbackDoer)
		{
			m_aiActor.knockbackDoer.SetImmobile(true, "BuffEnemiesBehavior");
		}
		return BehaviorResult.RunContinuous;
	}

	public override ContinuousBehaviorResult ContinuousUpdate()
	{
		for (int i = 0; i < m_buffedEnemies.Count; i++)
		{
			AIActor aIActor = m_buffedEnemies[i];
			if (!aIActor || aIActor.healthHaver.IsDead)
			{
				m_buffedEnemies.RemoveAt(i);
				i--;
			}
		}
		if (m_searchTimer <= 0f)
		{
			m_aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All, ref s_activeEnemies);
			s_activeEnemies.Remove(m_aiActor);
			for (int j = 0; j < m_buffedEnemies.Count; j++)
			{
				s_activeEnemies.Remove(m_buffedEnemies[j]);
			}
			for (int num = s_activeEnemies.Count - 1; num >= 0; num--)
			{
				if (!IsGoodBuffTarget(s_activeEnemies[num]))
				{
					s_activeEnemies.RemoveAt(num);
				}
			}
			if (s_activeEnemies.Count > 0)
			{
				while ((float)m_buffedEnemies.Count < EnemiesToBuff && s_activeEnemies.Count > 0)
				{
					int index = Random.Range(0, s_activeEnemies.Count);
					AIActor aIActor2 = s_activeEnemies[index];
					s_activeEnemies.RemoveAt(index);
					m_buffedEnemies.Add(aIActor2);
					BuffEnemy(aIActor2);
				}
			}
			m_searchTimer = SearchInterval;
		}
		return (m_buffedEnemies.Count <= 0) ? ContinuousBehaviorResult.Finished : ContinuousBehaviorResult.Continue;
	}

	public override void EndContinuousUpdate()
	{
		for (int i = 0; i < m_buffedEnemies.Count; i++)
		{
			UnbuffEnemy(m_buffedEnemies[i]);
		}
		m_buffedEnemies.Clear();
		if (!string.IsNullOrEmpty(BuffAnimation))
		{
			m_aiAnimator.EndAnimationIf(BuffAnimation);
		}
		if (!string.IsNullOrEmpty(BuffVfx))
		{
			m_aiAnimator.StopVfx(BuffVfx);
		}
		if ((bool)m_aiActor && (bool)m_aiActor.knockbackDoer)
		{
			m_aiActor.knockbackDoer.SetImmobile(false, "BuffEnemiesBehavior");
		}
		UpdateCooldowns();
	}

	public override void OnActorPreDeath()
	{
		if (m_buffedEnemies.Count > 0)
		{
			for (int i = 0; i < m_buffedEnemies.Count; i++)
			{
				UnbuffEnemy(m_buffedEnemies[i]);
			}
		}
	}

	protected virtual void BuffEnemy(AIActor enemy)
	{
		if (!enemy)
		{
			return;
		}
		if (JamEnemies)
		{
			if ((bool)enemy.specRigidbody)
			{
				if (enemy.IsSignatureEnemy)
				{
					enemy.PlaySmallExplosionsStyleEffect(LargeJamEffect, 8, 0.025f);
				}
				else
				{
					enemy.PlayEffectOnActor(SmallJamEffect, Vector3.zero, true, false, true);
				}
			}
			enemy.BecomeBlackPhantom();
		}
		if (UsesBuffEffect)
		{
			enemy.ApplyEffect(buffEffect);
		}
	}

	protected virtual void UnbuffEnemy(AIActor enemy)
	{
		if ((bool)enemy)
		{
			if (JamEnemies)
			{
				enemy.UnbecomeBlackPhantom();
			}
			if (UsesBuffEffect)
			{
				enemy.RemoveEffect(buffEffect);
			}
		}
	}

	private bool IsGoodBuffTarget(AIActor enemy)
	{
		if (!enemy)
		{
			return false;
		}
		if (enemy.IsBuffEnemy || enemy.IsHarmlessEnemy)
		{
			return false;
		}
		if ((bool)enemy.healthHaver && (!enemy.healthHaver.IsVulnerable || enemy.healthHaver.PreventAllDamage))
		{
			return false;
		}
		if (JamEnemies && enemy.IsBlackPhantom)
		{
			return false;
		}
		return true;
	}
}
