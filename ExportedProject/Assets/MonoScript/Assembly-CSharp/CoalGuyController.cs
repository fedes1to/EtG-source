using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CoalGuyController : BraveBehaviour
{
	[FormerlySerializedAs("fireEffect2")]
	public GameActorFireEffect fireEffect;

	public tk2dSpriteAnimator eyes;

	public float overrideMoveSpeed = -1f;

	public float overridePauseTime = -1f;

	[CheckDirectionalAnimation(null)]
	public string overrideAnimation;

	public List<DamageTypeModifier> onFireDamageTypeModifiers;

	public void Start()
	{
		base.healthHaver.OnDamaged += OnDamaged;
		base.healthHaver.OnPreDeath += OnPreDeath;
	}

	protected override void OnDestroy()
	{
		if ((bool)base.healthHaver)
		{
			base.healthHaver.OnDamaged -= OnDamaged;
			base.healthHaver.OnPreDeath -= OnPreDeath;
		}
		base.OnDestroy();
	}

	private void OnDamaged(float resultValue, float maxValue, CoreDamageTypes damageTypes, DamageCategory damageCategory, Vector2 damageDirection)
	{
		if ((damageTypes & CoreDamageTypes.Water) != CoreDamageTypes.Water && (damageTypes & CoreDamageTypes.Ice) != CoreDamageTypes.Ice)
		{
			FlameOn();
			if ((bool)base.healthHaver)
			{
				base.healthHaver.OnDamaged -= OnDamaged;
			}
		}
	}

	private void OnPreDeath(Vector2 obj)
	{
		if ((bool)eyes)
		{
			eyes.gameObject.SetActive(false);
		}
	}

	private void FlameOn()
	{
		base.aiActor.ApplyEffect(fireEffect);
		base.healthHaver.ApplyDamageModifiers(onFireDamageTypeModifiers);
		if (overrideMoveSpeed >= 0f)
		{
			base.aiActor.MovementSpeed = TurboModeController.MaybeModifyEnemyMovementSpeed(overrideMoveSpeed);
		}
		if (overridePauseTime >= 0f)
		{
			for (int i = 0; i < base.behaviorSpeculator.MovementBehaviors.Count; i++)
			{
				if (base.behaviorSpeculator.MovementBehaviors[i] is MoveErraticallyBehavior)
				{
					MoveErraticallyBehavior moveErraticallyBehavior = base.behaviorSpeculator.MovementBehaviors[i] as MoveErraticallyBehavior;
					moveErraticallyBehavior.PointReachedPauseTime = overridePauseTime;
					moveErraticallyBehavior.ResetPauseTimer();
					base.aiActor.ClearPath();
				}
			}
		}
		if (!string.IsNullOrEmpty(overrideAnimation))
		{
			base.aiAnimator.SetBaseAnim(overrideAnimation);
			base.aiAnimator.EndAnimation();
		}
		if ((bool)eyes)
		{
			eyes.gameObject.SetActive(true);
			eyes.Play(eyes.DefaultClip, 0f, eyes.DefaultClip.fps);
		}
		for (int j = 0; j < base.behaviorSpeculator.AttackBehaviors.Count; j++)
		{
			if (base.behaviorSpeculator.AttackBehaviors[j] is AttackBehaviorGroup)
			{
				ProcessAttackGroup(base.behaviorSpeculator.AttackBehaviors[j] as AttackBehaviorGroup);
			}
		}
		base.aiShooter.ToggleGunAndHandRenderers(false, "CoalGuyController");
		base.aiShooter.enabled = false;
		base.behaviorSpeculator.AttackCooldown = 0.66f;
	}

	private void ProcessAttackGroup(AttackBehaviorGroup attackGroup)
	{
		for (int i = 0; i < attackGroup.AttackBehaviors.Count; i++)
		{
			AttackBehaviorGroup.AttackGroupItem attackGroupItem = attackGroup.AttackBehaviors[i];
			if (attackGroupItem.Behavior is AttackBehaviorGroup)
			{
				ProcessAttackGroup(attackGroupItem.Behavior as AttackBehaviorGroup);
			}
			else if (attackGroupItem.Behavior is ShootGunBehavior)
			{
				attackGroupItem.Probability = 0f;
			}
			else if (attackGroupItem.Behavior is ShootBehavior)
			{
				attackGroupItem.Probability = 1f;
			}
		}
	}
}
