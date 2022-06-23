using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Dungeonator;
using FullInspector;
using UnityEngine;

public class BehaviorSpeculator : BaseBehavior<FullSerializerSerializer>
{
	public bool InstantFirstTick;

	public float TickInterval = 0.1f;

	public float PostAwakenDelay;

	public bool RemoveDelayOnReinforce;

	public bool OverrideStartingFacingDirection;

	[ShowInInspectorIf("OverrideStartingFacingDirection", false)]
	public float StartingFacingDirection = -90f;

	public bool SkipTimingDifferentiator;

	[InspectorCollectionRotorzFlags(HideRemoveButtons = true)]
	[InspectorHeader("Behaviors")]
	public List<OverrideBehaviorBase> OverrideBehaviors;

	[InspectorCollectionRotorzFlags(HideRemoveButtons = true)]
	public List<TargetBehaviorBase> TargetBehaviors;

	[InspectorCollectionRotorzFlags(HideRemoveButtons = true)]
	public List<MovementBehaviorBase> MovementBehaviors;

	[InspectorCollectionRotorzFlags(HideRemoveButtons = true)]
	public List<AttackBehaviorBase> AttackBehaviors;

	[InspectorCollectionRotorzFlags(HideRemoveButtons = true)]
	public List<BehaviorBase> OtherBehaviors;

	private float m_localTimeScale = 1f;

	private GameActor m_playerTarget;

	private bool m_isFirstUpdate;

	private float m_postAwakenDelay;

	private float m_tickTimer;

	private float m_attackCooldownTimer;

	private float m_globalCooldownTimer;

	private float m_stunTimer;

	private tk2dSpriteAnimator m_extantStunVFX;

	private BraveDictionary<string, float> m_groupCooldownTimers;

	private float m_cooldownScale = 1f;

	private List<BehaviorBase> m_behaviors = new List<BehaviorBase>();

	private BehaviorBase m_activeContinuousBehavior;

	private Dictionary<IList, BehaviorBase> m_classSpecificContinuousBehavior = new Dictionary<IList, BehaviorBase>();

	private AIActor m_aiActor;

	public float LocalTimeScale
	{
		get
		{
			if ((bool)m_aiActor)
			{
				return m_aiActor.LocalTimeScale;
			}
			return m_localTimeScale;
		}
		set
		{
			if ((bool)m_aiActor)
			{
				m_aiActor.LocalTimeScale = value;
			}
			else
			{
				m_localTimeScale = value;
			}
		}
	}

	public float LocalDeltaTime
	{
		get
		{
			if (m_aiActor != null)
			{
				return m_aiActor.LocalDeltaTime;
			}
			return BraveTime.DeltaTime * LocalTimeScale;
		}
	}

	public float AttackCooldown
	{
		get
		{
			return m_attackCooldownTimer;
		}
		set
		{
			m_attackCooldownTimer = value;
		}
	}

	public float GlobalCooldown
	{
		get
		{
			return m_globalCooldownTimer;
		}
		set
		{
			m_globalCooldownTimer = value;
		}
	}

	public float CooldownScale
	{
		get
		{
			return m_cooldownScale;
		}
		set
		{
			m_cooldownScale = value;
		}
	}

	public BehaviorBase ActiveContinuousAttackBehavior
	{
		get
		{
			if (m_activeContinuousBehavior is AttackBehaviorBase)
			{
				return m_activeContinuousBehavior;
			}
			if (m_classSpecificContinuousBehavior.ContainsKey(AttackBehaviors))
			{
				return m_classSpecificContinuousBehavior[AttackBehaviors];
			}
			return null;
		}
	}

	public bool IsInterruptable
	{
		get
		{
			bool flag = true;
			if (m_activeContinuousBehavior != null)
			{
				flag &= m_activeContinuousBehavior.IsOverridable();
			}
			if (m_classSpecificContinuousBehavior.Count > 0)
			{
				if (m_classSpecificContinuousBehavior.ContainsKey(OverrideBehaviors))
				{
					flag &= m_classSpecificContinuousBehavior[OverrideBehaviors].IsOverridable();
				}
				if (m_classSpecificContinuousBehavior.ContainsKey(TargetBehaviors))
				{
					flag &= m_classSpecificContinuousBehavior[TargetBehaviors].IsOverridable();
				}
				if (m_classSpecificContinuousBehavior.ContainsKey(MovementBehaviors))
				{
					flag &= m_classSpecificContinuousBehavior[MovementBehaviors].IsOverridable();
				}
				if (m_classSpecificContinuousBehavior.ContainsKey(AttackBehaviors))
				{
					flag &= m_classSpecificContinuousBehavior[AttackBehaviors].IsOverridable();
				}
				if (m_classSpecificContinuousBehavior.ContainsKey(OtherBehaviors))
				{
					flag &= m_classSpecificContinuousBehavior[OtherBehaviors].IsOverridable();
				}
				return flag;
			}
			return flag;
		}
	}

	public GameActor PlayerTarget
	{
		get
		{
			if ((bool)m_aiActor)
			{
				return m_aiActor.PlayerTarget;
			}
			return m_playerTarget;
		}
		set
		{
			if ((bool)m_aiActor)
			{
				m_aiActor.PlayerTarget = value;
			}
			else
			{
				m_playerTarget = value;
			}
		}
	}

	public SpeculativeRigidbody TargetRigidbody
	{
		get
		{
			if ((bool)m_aiActor)
			{
				return m_aiActor.TargetRigidbody;
			}
			if ((bool)m_playerTarget)
			{
				return m_playerTarget.specRigidbody;
			}
			return null;
		}
	}

	public Vector2 TargetVelocity
	{
		get
		{
			if ((bool)m_aiActor)
			{
				return m_aiActor.TargetVelocity;
			}
			if ((bool)m_playerTarget)
			{
				return m_playerTarget.specRigidbody.Velocity;
			}
			return Vector2.zero;
		}
	}

	public bool PreventMovement { get; set; }

	public FleePlayerData FleePlayerData { get; set; }

	public AttackBehaviorGroup AttackBehaviorGroup
	{
		get
		{
			if (AttackBehaviors == null)
			{
				return null;
			}
			for (int i = 0; i < AttackBehaviors.Count; i++)
			{
				if (AttackBehaviors[i] is AttackBehaviorGroup)
				{
					return AttackBehaviors[i] as AttackBehaviorGroup;
				}
			}
			return null;
		}
	}

	public bool IsStunned
	{
		get
		{
			return m_stunTimer > 0f;
		}
	}

	public bool ImmuneToStun { get; set; }

	public event Action<string> AnimationEventTriggered;

	private void Start()
	{
		m_aiActor = GetComponent<AIActor>();
		if ((bool)m_aiActor)
		{
			m_aiActor.healthHaver.OnPreDeath += OnPreDeath;
		}
		if ((bool)base.healthHaver)
		{
			base.healthHaver.OnDamaged += OnDamaged;
		}
		if (OverrideStartingFacingDirection && (bool)base.aiAnimator)
		{
			base.aiAnimator.FacingDirection = StartingFacingDirection;
		}
		if ((bool)m_aiActor)
		{
			m_aiActor.specRigidbody.Initialize();
		}
		RegisterBehaviors(OverrideBehaviors);
		RegisterBehaviors(TargetBehaviors);
		RegisterBehaviors(MovementBehaviors);
		RegisterBehaviors(AttackBehaviors);
		RegisterBehaviors(OtherBehaviors);
		StartBehaviors();
		if (InstantFirstTick)
		{
			m_tickTimer = TickInterval;
		}
		m_postAwakenDelay = PostAwakenDelay;
	}

	private void Update()
	{
		if ((bool)m_aiActor)
		{
			if (!m_aiActor.enabled || m_aiActor.healthHaver.IsDead || !m_aiActor.HasBeenAwoken)
			{
				return;
			}
			if (m_postAwakenDelay > 0f && (!RemoveDelayOnReinforce || !base.aiActor.IsInReinforcementLayer))
			{
				m_postAwakenDelay = Mathf.Max(0f, m_postAwakenDelay - LocalDeltaTime);
				return;
			}
			if (m_aiActor.SpeculatorDelayTime > 0f)
			{
				m_aiActor.SpeculatorDelayTime = Mathf.Max(0f, m_aiActor.SpeculatorDelayTime - LocalDeltaTime);
				return;
			}
		}
		if (!m_isFirstUpdate)
		{
			FirstUpdate();
			m_isFirstUpdate = true;
		}
		m_tickTimer += LocalDeltaTime;
		m_globalCooldownTimer = Mathf.Max(0f, m_globalCooldownTimer - LocalDeltaTime);
		m_attackCooldownTimer = Mathf.Max(0f, m_attackCooldownTimer - LocalDeltaTime);
		m_stunTimer = Mathf.Max(0f, m_stunTimer - LocalDeltaTime);
		UpdateStunVFX();
		if (m_groupCooldownTimers != null)
		{
			for (int i = 0; i < m_groupCooldownTimers.Count; i++)
			{
				m_groupCooldownTimers.Values[i] = Mathf.Max(0f, m_groupCooldownTimers.Values[i] - LocalDeltaTime);
			}
		}
		bool flag = m_tickTimer > TickInterval;
		bool onGlobalCooldown = m_globalCooldownTimer > 0f;
		UpkeepBehaviors(flag);
		if (!IsStunned)
		{
			UpdateBehaviors(flag, onGlobalCooldown);
		}
		if (flag)
		{
			m_tickTimer = 0f;
		}
	}

	protected virtual void UpdateStunVFX()
	{
		if (m_stunTimer <= 0f && m_extantStunVFX != null)
		{
			SpawnManager.Despawn(m_extantStunVFX.gameObject);
			m_extantStunVFX = null;
		}
		else if (m_stunTimer > 0f && m_extantStunVFX != null)
		{
			m_extantStunVFX.transform.position = base.aiActor.sprite.WorldTopCenter.ToVector3ZUp(m_extantStunVFX.transform.position.z);
		}
	}

	private void OnPreDeath(Vector2 dir)
	{
		if (IsStunned)
		{
			m_stunTimer = 0f;
			UpdateStunVFX();
		}
		for (int i = 0; i < m_behaviors.Count; i++)
		{
			m_behaviors[i].OnActorPreDeath();
		}
		Interrupt();
	}

	private void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		m_postAwakenDelay = 0f;
		if ((bool)base.healthHaver)
		{
			base.healthHaver.OnDamaged -= OnDamaged;
		}
	}

	protected override void OnDestroy()
	{
		for (int i = 0; i < m_behaviors.Count; i++)
		{
			m_behaviors[i].Destroy();
		}
		if ((bool)m_aiActor)
		{
			m_aiActor.healthHaver.OnPreDeath -= OnPreDeath;
		}
		if ((bool)base.healthHaver)
		{
			base.healthHaver.OnDamaged -= OnDamaged;
		}
		base.OnDestroy();
	}

	public void Stun(float duration, bool createVFX = true)
	{
		if (((bool)base.aiActor && (bool)base.aiActor.healthHaver && base.aiActor.healthHaver.IsBoss) || ((bool)base.healthHaver && !base.healthHaver.IsVulnerable) || ImmuneToStun)
		{
			return;
		}
		m_stunTimer = Mathf.Max(m_stunTimer, duration);
		if (m_stunTimer > 0f)
		{
			if (IsInterruptable)
			{
				Interrupt();
			}
			if (m_extantStunVFX == null && createVFX)
			{
				m_extantStunVFX = base.aiActor.PlayEffectOnActor(ResourceCache.Acquire("Global VFX/VFX_Stun") as GameObject, (base.aiActor.sprite.WorldTopCenter - base.aiActor.CenterPosition).WithX(0f), true, true).GetComponent<tk2dSpriteAnimator>();
			}
			base.aiActor.ClearPath();
		}
	}

	public void UpdateStun(float maxStunTime)
	{
		if (IsStunned && (!base.aiActor || !base.aiActor.healthHaver || !base.aiActor.healthHaver.IsBoss) && (!base.healthHaver || base.healthHaver.IsVulnerable) && !ImmuneToStun)
		{
			m_stunTimer = maxStunTime;
		}
	}

	public void EndStun()
	{
		m_stunTimer = 0f;
		UpdateStunVFX();
	}

	public float GetDesiredCombatDistance()
	{
		float num = -1f;
		for (int i = 0; i < MovementBehaviors.Count; i++)
		{
			float desiredCombatDistance = MovementBehaviors[i].DesiredCombatDistance;
			if (desiredCombatDistance > -1f)
			{
				num = ((!(num < 0f)) ? Mathf.Min(num, desiredCombatDistance) : MovementBehaviors[i].DesiredCombatDistance);
			}
		}
		float num2 = num;
		float num3 = float.MinValue;
		for (int j = 0; j < AttackBehaviors.Count; j++)
		{
			float minReadyRange = AttackBehaviors[j].GetMinReadyRange();
			if (minReadyRange >= 0f)
			{
				num2 = Mathf.Min(num2, minReadyRange);
			}
			num3 = Mathf.Max(num3, AttackBehaviors[j].GetMaxRange());
		}
		if (num2 < 2.14748365E+09f)
		{
			return num2;
		}
		if (num3 > -2.14748365E+09f)
		{
			return num3;
		}
		return -1f;
	}

	public void Interrupt()
	{
		if (m_activeContinuousBehavior != null)
		{
			EndContinuousBehavior();
		}
		if (m_classSpecificContinuousBehavior.Count > 0)
		{
			if (m_classSpecificContinuousBehavior.ContainsKey(OverrideBehaviors))
			{
				EndClassSpecificContinuousBehavior(OverrideBehaviors);
			}
			if (m_classSpecificContinuousBehavior.ContainsKey(TargetBehaviors))
			{
				EndClassSpecificContinuousBehavior(TargetBehaviors);
			}
			if (m_classSpecificContinuousBehavior.ContainsKey(MovementBehaviors))
			{
				EndClassSpecificContinuousBehavior(MovementBehaviors);
			}
			if (m_classSpecificContinuousBehavior.ContainsKey(AttackBehaviors))
			{
				EndClassSpecificContinuousBehavior(AttackBehaviors);
			}
			if (m_classSpecificContinuousBehavior.ContainsKey(OtherBehaviors))
			{
				EndClassSpecificContinuousBehavior(OtherBehaviors);
			}
		}
	}

	public void InterruptAndDisable()
	{
		Interrupt();
		base.enabled = false;
	}

	public void RefreshBehaviors()
	{
		List<BehaviorBase> behaviors = m_behaviors;
		m_behaviors = new List<BehaviorBase>();
		RefreshBehaviors(OverrideBehaviors, behaviors);
		RefreshBehaviors(TargetBehaviors, behaviors);
		RefreshBehaviors(MovementBehaviors, behaviors);
		RefreshBehaviors(AttackBehaviors, behaviors);
		RefreshBehaviors(OtherBehaviors, behaviors);
	}

	public void TriggerAnimationEvent(string eventInfo)
	{
		if (this.AnimationEventTriggered != null)
		{
			this.AnimationEventTriggered(eventInfo);
		}
	}

	public void SetGroupCooldown(string groupName, float newCooldown)
	{
		if (m_groupCooldownTimers == null)
		{
			m_groupCooldownTimers = new BraveDictionary<string, float>();
		}
		float value;
		if (m_groupCooldownTimers.TryGetValue(groupName, out value))
		{
			if (value < newCooldown)
			{
				m_groupCooldownTimers[groupName] = newCooldown;
			}
		}
		else
		{
			m_groupCooldownTimers[groupName] = newCooldown;
		}
	}

	public float GetGroupCooldownTimer(string groupName)
	{
		if (m_groupCooldownTimers == null)
		{
			return 0f;
		}
		float value;
		if (m_groupCooldownTimers.TryGetValue(groupName, out value))
		{
			return value;
		}
		return 0f;
	}

	private void RegisterBehaviors(IList behaviors)
	{
		if (behaviors == null)
		{
			behaviors = new BehaviorBase[0];
		}
		for (int i = 0; i < behaviors.Count; i++)
		{
			m_behaviors.Add(behaviors[i] as BehaviorBase);
		}
	}

	private void StartBehaviors()
	{
		for (int i = 0; i < m_behaviors.Count; i++)
		{
			m_behaviors[i].Init(base.gameObject, m_aiActor, base.aiShooter);
			m_behaviors[i].Start();
		}
	}

	private void UpkeepBehaviors(bool isTick)
	{
		for (int i = 0; i < m_behaviors.Count; i++)
		{
			if (isTick || m_behaviors[i].UpdateEveryFrame())
			{
				m_behaviors[i].SetDeltaTime((!m_behaviors[i].UpdateEveryFrame()) ? m_tickTimer : LocalDeltaTime);
				m_behaviors[i].Upkeep();
			}
		}
		if (m_activeContinuousBehavior != null && m_activeContinuousBehavior.IsOverridable())
		{
			for (int j = 0; j < m_behaviors.Count; j++)
			{
				if ((isTick || m_behaviors[j].UpdateEveryFrame()) && m_behaviors[j] != m_activeContinuousBehavior && m_behaviors[j].OverrideOtherBehaviors())
				{
					EndContinuousBehavior();
					break;
				}
			}
		}
		else
		{
			if (m_classSpecificContinuousBehavior.Count <= 0)
			{
				return;
			}
			KeyValuePair<IList, BehaviorBase> keyValuePair = m_classSpecificContinuousBehavior.First();
			IList key = keyValuePair.Key;
			BehaviorBase value = keyValuePair.Value;
			if (!value.IsOverridable())
			{
				return;
			}
			for (int k = 0; k < key.Count; k++)
			{
				BehaviorBase behaviorBase = key[k] as BehaviorBase;
				if ((isTick || behaviorBase.UpdateEveryFrame()) && key[k] != value && behaviorBase.OverrideOtherBehaviors())
				{
					EndClassSpecificContinuousBehavior(key);
					break;
				}
			}
		}
	}

	private void UpdateBehaviors(bool isTick, bool onGlobalCooldown)
	{
		if (m_activeContinuousBehavior != null && m_classSpecificContinuousBehavior.Count > 1)
		{
			BraveUtility.Log("Trying to activate a class specific continuous behavior at the same time as a global continuous behavior; this isn't supported.", Color.red, BraveUtility.LogVerbosity.IMPORTANT);
		}
		if (m_activeContinuousBehavior != null)
		{
			if ((isTick || m_activeContinuousBehavior.UpdateEveryFrame()) && (!onGlobalCooldown || m_activeContinuousBehavior.IgnoreGlobalCooldown()))
			{
				ContinuousBehaviorResult continuousBehaviorResult = m_activeContinuousBehavior.ContinuousUpdate();
				if (continuousBehaviorResult == ContinuousBehaviorResult.Finished)
				{
					EndContinuousBehavior();
				}
			}
		}
		else if (UpdateBehaviorClass(OverrideBehaviors, isTick, onGlobalCooldown) != BehaviorResult.SkipAllRemainingBehaviors && UpdateBehaviorClass(TargetBehaviors, isTick, onGlobalCooldown) != BehaviorResult.SkipAllRemainingBehaviors && (PreventMovement || UpdateBehaviorClass(MovementBehaviors, isTick, onGlobalCooldown) != BehaviorResult.SkipAllRemainingBehaviors) && (!(m_attackCooldownTimer <= 0f) || UpdateBehaviorClass(AttackBehaviors, isTick, onGlobalCooldown) != BehaviorResult.SkipAllRemainingBehaviors) && UpdateBehaviorClass(OtherBehaviors, isTick, onGlobalCooldown) != BehaviorResult.SkipAllRemainingBehaviors)
		{
		}
	}

	private BehaviorResult UpdateBehaviorClass(IList behaviors, bool isTick, bool onGlobalCooldown)
	{
		if (behaviors == null)
		{
			return BehaviorResult.Continue;
		}
		if (m_classSpecificContinuousBehavior.ContainsKey(behaviors))
		{
			BehaviorBase behaviorBase = m_classSpecificContinuousBehavior[behaviors];
			if ((isTick || behaviorBase.UpdateEveryFrame()) && (!onGlobalCooldown || behaviorBase.IgnoreGlobalCooldown()))
			{
				ContinuousBehaviorResult continuousBehaviorResult = behaviorBase.ContinuousUpdate();
				if (continuousBehaviorResult == ContinuousBehaviorResult.Finished)
				{
					EndClassSpecificContinuousBehavior(behaviors);
				}
			}
			return BehaviorResult.SkipRemainingClassBehaviors;
		}
		for (int i = 0; i < behaviors.Count; i++)
		{
			BehaviorBase behaviorBase2 = behaviors[i] as BehaviorBase;
			if ((!isTick && !behaviorBase2.UpdateEveryFrame()) || (onGlobalCooldown && !behaviorBase2.IgnoreGlobalCooldown()))
			{
				continue;
			}
			switch (behaviorBase2.Update())
			{
			case BehaviorResult.RunContinuous:
				if (m_activeContinuousBehavior != null)
				{
					BraveUtility.Log("Trying to overwrite the current continuous behaviors; this shouldn't happen.", Color.red, BraveUtility.LogVerbosity.IMPORTANT);
				}
				m_activeContinuousBehavior = behaviorBase2;
				return BehaviorResult.SkipAllRemainingBehaviors;
			case BehaviorResult.RunContinuousInClass:
				if (m_classSpecificContinuousBehavior.ContainsKey(behaviors))
				{
					BraveUtility.Log("Trying to overwrite the current class continuous behaviors; this shouldn't happen.", Color.red, BraveUtility.LogVerbosity.IMPORTANT);
				}
				m_classSpecificContinuousBehavior[behaviors] = behaviorBase2;
				return BehaviorResult.SkipRemainingClassBehaviors;
			case BehaviorResult.SkipAllRemainingBehaviors:
				return BehaviorResult.SkipAllRemainingBehaviors;
			case BehaviorResult.SkipRemainingClassBehaviors:
				return BehaviorResult.SkipRemainingClassBehaviors;
			}
		}
		return BehaviorResult.Continue;
	}

	private void EndContinuousBehavior()
	{
		if (m_activeContinuousBehavior != null)
		{
			BehaviorBase activeContinuousBehavior = m_activeContinuousBehavior;
			m_activeContinuousBehavior = null;
			activeContinuousBehavior.EndContinuousUpdate();
		}
	}

	private void EndClassSpecificContinuousBehavior(IList key)
	{
		BehaviorBase value;
		if (m_classSpecificContinuousBehavior.TryGetValue(key, out value))
		{
			m_classSpecificContinuousBehavior.Remove(key);
			value.EndContinuousUpdate();
		}
	}

	private void RefreshBehaviors(IList behaviors, List<BehaviorBase> oldBehaviors)
	{
		for (int i = 0; i < behaviors.Count; i++)
		{
			if (!oldBehaviors.Contains(behaviors[i] as BehaviorBase))
			{
				BehaviorBase behaviorBase = behaviors[i] as BehaviorBase;
				behaviorBase.Init(base.gameObject, m_aiActor, base.aiShooter);
				behaviorBase.Start();
			}
			m_behaviors.Add(behaviors[i] as BehaviorBase);
		}
	}

	private void FirstUpdate()
	{
		if (SkipTimingDifferentiator || !base.aiActor || ((bool)base.healthHaver && base.healthHaver.IsBoss) || base.aiActor.ParentRoom == null)
		{
			return;
		}
		int num = 0;
		List<AIActor> activeEnemies = base.aiActor.ParentRoom.GetActiveEnemies(RoomHandler.ActiveEnemyType.All);
		if (activeEnemies == null)
		{
			return;
		}
		for (int i = 0; i < activeEnemies.Count; i++)
		{
			if ((bool)activeEnemies[i] && activeEnemies[i].EnemyGuid == base.aiActor.EnemyGuid)
			{
				num++;
				if (num == 1 && activeEnemies[i] == base.aiActor)
				{
					return;
				}
			}
		}
		if (num <= 1)
		{
			return;
		}
		float quickestCooldown = float.MaxValue;
		ProcessAttacks(delegate(AttackBehaviorBase attackBase)
		{
			BasicAttackBehavior basicAttackBehavior = attackBase as BasicAttackBehavior;
			if (attackBase is SequentialAttackBehaviorGroup)
			{
				SequentialAttackBehaviorGroup sequentialAttackBehaviorGroup = attackBase as SequentialAttackBehaviorGroup;
				basicAttackBehavior = sequentialAttackBehaviorGroup.AttackBehaviors[sequentialAttackBehaviorGroup.AttackBehaviors.Count - 1] as BasicAttackBehavior;
			}
			if (basicAttackBehavior != null)
			{
				if (basicAttackBehavior.CooldownVariance < 0.2f)
				{
					basicAttackBehavior.CooldownVariance = 0.2f;
				}
				float b = Mathf.Max(basicAttackBehavior.Cooldown, basicAttackBehavior.GlobalCooldown, basicAttackBehavior.GroupCooldown, basicAttackBehavior.InitialCooldown);
				quickestCooldown = Mathf.Min(quickestCooldown, b);
			}
		}, true);
		if (quickestCooldown < float.MaxValue && !InstantFirstTick)
		{
			AttackCooldown = UnityEngine.Random.Range(0f, Mathf.Max(quickestCooldown, 4f));
		}
	}

	private void ProcessAttacks(Action<AttackBehaviorBase> func, bool skipSimultaneous = false)
	{
		for (int i = 0; i < AttackBehaviors.Count; i++)
		{
			ProcessAttacksRecursive(AttackBehaviors[i], func, skipSimultaneous);
		}
	}

	private void ProcessAttacksRecursive(AttackBehaviorBase attack, Action<AttackBehaviorBase> func, bool skipSimultaneous)
	{
		if (attack is IAttackBehaviorGroup)
		{
			IAttackBehaviorGroup attackBehaviorGroup = attack as IAttackBehaviorGroup;
			if (!skipSimultaneous || !(attack is SimultaneousAttackBehaviorGroup))
			{
				for (int i = 0; i < attackBehaviorGroup.Count; i++)
				{
					ProcessAttacksRecursive(attackBehaviorGroup.GetAttackBehavior(i), func, skipSimultaneous);
				}
			}
		}
		else
		{
			func(attack);
		}
	}
}
