using System.Collections.Generic;
using Dungeonator;

public class CrazedController : BraveBehaviour
{
	private enum State
	{
		Idle,
		Transforming,
		Crazed
	}

	[CheckDirectionalAnimation(null)]
	public string TellAnimation;

	public bool SpecifyTellDuration = true;

	[ShowInInspectorIf("SpecifyTellDuration", true)]
	public float TellDuration;

	public bool DoCharge = true;

	[ShowInInspectorIf("DoCharge", true)]
	public string CrazedAnimaton;

	[ShowInInspectorIf("DoCharge", true)]
	public float CrazedRunSpeed = -1f;

	public bool EnableBehavior;

	[ShowInInspectorIf("EnableBehavior", true)]
	public string BehaviorName;

	public bool TriggerWhenLastEnemy = true;

	public bool DisableHitAnims;

	private State m_state;

	private static List<AIActor> s_activeEnemies = new List<AIActor>();

	public void Update()
	{
		if (!base.aiActor || !base.aiActor.enabled || !base.healthHaver || base.healthHaver.IsDead)
		{
			return;
		}
		if (m_state == State.Idle)
		{
			if (!TriggerWhenLastEnemy || !base.behaviorSpeculator.enabled)
			{
				return;
			}
			base.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear, ref s_activeEnemies);
			bool flag = false;
			for (int i = 0; i < s_activeEnemies.Count; i++)
			{
				if (s_activeEnemies[i].EnemyGuid != base.aiActor.EnemyGuid)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				GoCrazed();
			}
		}
		else if (m_state == State.Transforming)
		{
			base.behaviorSpeculator.GlobalCooldown = 1f;
			if (!base.behaviorSpeculator.enabled)
			{
				m_state = State.Idle;
			}
			else if (!base.aiAnimator.IsPlaying(TellAnimation))
			{
				DoCrazedBehavior();
			}
		}
		else if (m_state != State.Crazed)
		{
		}
	}

	public void GoCrazed()
	{
		base.aiActor.ClearPath();
		base.behaviorSpeculator.GlobalCooldown = 1f;
		if (DisableHitAnims)
		{
			base.aiActor.aiAnimator.HitAnimation.Type = DirectionalAnimation.DirectionType.None;
		}
		if (!string.IsNullOrEmpty(TellAnimation))
		{
			if (!SpecifyTellDuration)
			{
				base.aiAnimator.PlayUntilFinished(TellAnimation, true);
			}
			else
			{
				base.aiAnimator.PlayForDurationOrUntilFinished(TellAnimation, TellDuration, true);
			}
			m_state = State.Transforming;
		}
		else
		{
			DoCrazedBehavior();
		}
	}

	private void DoCrazedBehavior()
	{
		base.behaviorSpeculator.GlobalCooldown = 0f;
		if (DoCharge)
		{
			base.aiAnimator.SetBaseAnim(CrazedAnimaton);
			base.behaviorSpeculator.MovementBehaviors.Clear();
			base.behaviorSpeculator.AttackBehaviors.Clear();
			SeekTargetBehavior seekTargetBehavior = new SeekTargetBehavior();
			seekTargetBehavior.StopWhenInRange = false;
			seekTargetBehavior.CustomRange = -1f;
			seekTargetBehavior.LineOfSight = false;
			seekTargetBehavior.ReturnToSpawn = false;
			seekTargetBehavior.SpawnTetherDistance = 0f;
			seekTargetBehavior.PathInterval = 0.25f;
			SeekTargetBehavior item = seekTargetBehavior;
			base.behaviorSpeculator.MovementBehaviors.Add(item);
			base.behaviorSpeculator.RefreshBehaviors();
			base.behaviorSpeculator.enabled = true;
			if (CrazedRunSpeed > 0f)
			{
				base.aiActor.MovementSpeed = CrazedRunSpeed;
			}
			base.aiActor.CollisionDamage = 0.5f;
		}
		if (EnableBehavior)
		{
			for (int i = 0; i < base.behaviorSpeculator.AttackBehaviors.Count; i++)
			{
				if (base.behaviorSpeculator.AttackBehaviors[i] is AttackBehaviorGroup)
				{
					ProcessAttackGroup(base.behaviorSpeculator.AttackBehaviors[i] as AttackBehaviorGroup);
				}
			}
			base.behaviorSpeculator.enabled = true;
		}
		m_state = State.Crazed;
	}

	private void ProcessAttackGroup(AttackBehaviorGroup attackGroup)
	{
		for (int i = 0; i < attackGroup.AttackBehaviors.Count; i++)
		{
			AttackBehaviorGroup.AttackGroupItem attackGroupItem = attackGroup.AttackBehaviors[i];
			if (attackGroupItem.NickName == BehaviorName)
			{
				attackGroupItem.Probability = 1f;
			}
			if (attackGroupItem.Behavior is AttackBehaviorGroup)
			{
				ProcessAttackGroup(attackGroupItem.Behavior as AttackBehaviorGroup);
			}
		}
	}
}
